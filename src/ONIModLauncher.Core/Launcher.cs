using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NativeMessageBox;
using ONIModLauncher.Core.Configs;

namespace ONIModLauncher.Core
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
		
		public bool DebugEnabled
		{
			get => DebugPrefs?.EnableDebug ?? false;
			set
			{
				if (DebugPrefs == null) throw new InvalidOperationException("DebugPrefs not loaded");
				DebugPrefs.EnableDebug = value;
				InvokePropertyChanged(nameof(DebugEnabled));
			}
		}

		public bool CanToggleDLC1 => GamePaths.HasSpacedOut && !IsRunning;
		
		public bool? DLC1Enabled
		{
			get => PlayerPrefs?.SpacedOutEnabled;
			set
			{
				if (PlayerPrefs == null) throw new InvalidOperationException("PlayerPrefs not loaded");
				PlayerPrefs.SpacedOutEnabled = value.GetValueOrDefault();
				InvokePropertyChanged(nameof(DLC1Enabled));
			}
		}

		public bool HasFrostyPlanetPack => GamePaths.HasFrostyPlanetPack;

		public bool HasBionicBoosterPack => GamePaths.HasBionicBoosterPack;

		public bool CanEditLastSave => !IsRunning && (DebugPrefs?.AutoResumeLastSave ?? false);

		public bool IsRunning
		{ get; private set; } = false;

		public bool IsNotRunning => !IsRunning;

		public KPlayerPrefsYaml? PlayerPrefs
		{ get; private set; }

		public DebugSettingsYaml? DebugPrefs
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
			LoadPlayerPrefs();
			LoadDebugPrefs();
		}

		private void LoadPlayerPrefs()
		{
			if (PlayerPrefs != null)
			{
				PlayerPrefs.PropertyChanged -= PlayerPrefs_PropertyChanged;
			}

			PlayerPrefs = KPlayerPrefsYaml.Load(GamePaths.PlayerPrefsFile);

			if (PlayerPrefs != null)
			{
				PlayerPrefs.PropertyChanged += PlayerPrefs_PropertyChanged;
			}
		}

		private void LoadDebugPrefs()
		{
			if (DebugPrefs != null)
			{
				DebugPrefs.PropertyChanged += DebugPrefs_PropertyChanged;
			}

			DebugPrefs = DebugSettingsYaml.Load(GamePaths.DebugSettingsFile);
			if (DebugPrefs == null)
			{
				DebugPrefs = new DebugSettingsYaml();
			}

			if (DebugPrefs != null)
			{
				DebugPrefs.PropertyChanged += DebugPrefs_PropertyChanged;
			}
		}

		private void PlayerPrefs_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			KPlayerPrefsYaml.Save(GamePaths.PlayerPrefsFile, PlayerPrefs);
			
			InvokePropertyChanged(nameof(PlayerPrefs));
		}

		private void DebugPrefs_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DebugSettingsYaml.Save(GamePaths.DebugSettingsFile, DebugPrefs);
			
			InvokePropertyChanged(nameof(DebugPrefs));
		}

		public void StartGameMonitor()
		{
			LoadLaunchConfigs(); // hacky but this is called at the right time
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

			// Ensure prefs are committed so the game sees the intended settings
			KPlayerPrefsYaml.Save(GamePaths.PlayerPrefsFile, PlayerPrefs);
			DebugSettingsYaml.Save(GamePaths.DebugSettingsFile, DebugPrefs);
			
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
						ShellHelper.ShowMessageBox($"{GamePaths.ONI_EXE_NAME} did not start. (Unknown Reason)", "Error", icon: ShellHelper.MessageBoxIcons.Error);
					}
					else
					{
						OnLaunched();
					}
				}
			}
			catch (Exception ex)
			{
				ShellHelper.ShowMessageBox($"An error occurred while trying to launch OxygenNotIncluded.exe:\n\n{ex}", "Error", icon: ShellHelper.MessageBoxIcons.Error);
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
