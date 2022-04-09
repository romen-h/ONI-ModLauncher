using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ONIModLauncher
{
	public class Launcher : INotifyPropertyChanged
	{
		private static Launcher _instance;

		public static Launcher Instance
		{
			get
			{
				if (_instance == null) _instance = new Launcher();
				return _instance;
			}
		}

		private Process process = null;

		public bool CanLaunch => !IsRunning && GamePaths.GameExecutablePath != null;

		public bool IsRunning
		{ get; private set; } = false;

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private BackgroundWorker gameMonitor;

		private Launcher()
		{
			gameMonitor = new BackgroundWorker();
			gameMonitor.DoWork += GameMonitor_DoWork;
			gameMonitor.RunWorkerCompleted += GameMonitor_RunWorkerCompleted;
			gameMonitor.WorkerSupportsCancellation = true;
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
						InvokePropertyChanged(null);
					}
				}
				Thread.Sleep(1000);
			}
		}

		private void GameMonitor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{ }

		public void Launch()
		{
			if (IsRunning) return;

			try
			{
				if (GamePaths.UseSteam)
				{
					LaunchSteam();
				}
				else
				{
					process = LaunchDirect();
					if (process == null)
					{
						MessageBox.Show($"{GamePaths.ONI_EXE_NAME} did not start. (Unknown Reason)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

		private void LaunchSteam()
		{
			var startInfo = new ProcessStartInfo()
			{
				FileName = GamePaths.SteamExecutablePath,
				Arguments = $"-applaunch {GamePaths.STEAM_APP_ID}",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process.Start(startInfo);
		}
	}
}
