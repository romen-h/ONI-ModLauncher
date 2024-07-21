using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using ONIModLauncher.Configs;

namespace ONIModLauncher
{
	public class Launcher : INotifyPropertyChanged
	{
		private static Launcher s_instance;

		public static Launcher Instance
		{
			get
			{
				if (s_instance == null) s_instance = new Launcher();
				return s_instance;
			}
		}

		private Process process = null;

		public bool HasBaseGame => GamePaths.GameExecutablePath != null;

		public bool CanLaunch => HasBaseGame && !IsRunning;

		public bool CanToggleDLC1 => GamePaths.HasDLC1 && !IsRunning;

		public bool HasDLC2 => GamePaths.HasDLC2 && !IsRunning;

		public bool CanEditLastSave => !IsRunning && DebugPrefs.AutoResumeLastSave;

		public bool IsRunning
		{ get; private set; } = false;

		public bool IsNotRunning => !IsRunning;

		public KPlayerPrefsYaml PlayerPrefs
		{ get; private set; }

		public DebugSettingsYaml DebugPrefs
		{ get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event EventHandler GameStarted;

		public event EventHandler GameStopped;

		private BackgroundWorker gameMonitor;

		private Launcher()
		{
			gameMonitor = new BackgroundWorker();
			gameMonitor.DoWork += GameMonitor_DoWork;
			gameMonitor.RunWorkerCompleted += GameMonitor_RunWorkerCompleted;
			gameMonitor.WorkerSupportsCancellation = true;

			LoadLaunchConfigs();
		}

		public void LoadLaunchConfigs()
		{
			PlayerPrefs = KPlayerPrefsYaml.Load(GamePaths.PlayerPrefsFile);
			DebugPrefs = DebugSettingsYaml.Load(GamePaths.DebugSettingsFile);
		}

		public void StartGameMonitor()
		{
			gameMonitor.RunWorkerAsync();
		}

		public void StopGameMonitor()
		{
			gameMonitor.CancelAsync();
		}

		private void GameMonitor_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker bgw = (BackgroundWorker)sender;

			while (!bgw.CancellationPending)
			{
				if (process == null)
				{
					var processes = Process.GetProcessesByName("OxygenNotIncluded");

					if (processes.Length > 0)
					{
						foreach (var p in processes)
						{
							Debug.WriteLine("Process: " + p.ProcessName);
							Debug.WriteLine("---");
						}

						process = processes[0];
						IsRunning = true;
						OnLaunched();
						InvokePropertyChanged(null);
					}
				}
				else
				{
					process.Refresh();
					if (process.HasExited)
					{
						process.Dispose();
						process = null;
						IsRunning = false;
						OnExited();
						InvokePropertyChanged(null);
					}
				}
				Thread.Sleep(1000);
			}
		}

		private void GameMonitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{ }

		private void OnLaunched()
		{
			GameStarted?.Invoke(this, EventArgs.Empty);
		}

		private void OnExited()
		{
			ModManager.Instance.LoadModList();
			LoadLaunchConfigs();

			GameStopped?.Invoke(this, EventArgs.Empty);
		}

		public void Launch()
		{
			if (IsRunning) return;

			try
			{
				if (SteamIntegration.Instance.UseSteam)
				{
					SteamIntegration.Instance.LaunchGame();
				}
				else
				{
					process = LaunchDirect();
					if (process == null)
					{
						MessageBox.Show($"{GamePaths.ONI_EXE_NAME} did not start. (Unknown Reason)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
					else
					{
						OnLaunched();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"An error occurred while trying to launch OxygenNotIncluded.exe:\n\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private Process LaunchDirect()
		{
			var startInfo = new ProcessStartInfo()
			{
				FileName = GamePaths.GameExecutablePath,
				UseShellExecute = false
			};

			return Process.Start(startInfo);
		}
	}
}
