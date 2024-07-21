using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ONIModLauncher
{
	public class DiagnosticLogMessageEventArgs : EventArgs
	{
		public string Message
		{ get; private set; }

		public DiagnosticLogMessageEventArgs(string message)
		{
			Message = message;
		}
	}

	class CrashDiagnostics
    {
	    private readonly EventWaitHandle _waitForGameStarted = new AutoResetEvent(false);
		private readonly EventWaitHandle _waitForGameStopped = new AutoResetEvent(false);

		public event EventHandler<DiagnosticLogMessageEventArgs> DiagnosticLogMessage;

        public async void StartCrashDiagnostic()
        {
			Launcher.Instance.GameStarted += OnGameStarted;
			Launcher.Instance.GameStopped += OnGameStopped;
		}

		private void OnGameStarted(object sender, EventArgs e)
		{
			_waitForGameStarted.Set();
		}

		private void OnGameStopped(object sender, EventArgs e)
		{
			_waitForGameStopped.Set();
		}

		private async Task WaitForLaunch()
        {
	        if (!_waitForGameStarted.WaitOne(TimeSpan.FromSeconds(10))) throw new Exception("Game did not start within 10 seconds.");
        }

        private async void TryLaunchingGame()
        {
            Launcher.Instance.Launch();
			await WaitForLaunch();
        }
    }
}
