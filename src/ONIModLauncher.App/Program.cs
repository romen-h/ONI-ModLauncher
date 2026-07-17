using ONIModLauncher.App.Common;
using ONIModLauncher.Core;
using OpenSilver.Photino;
using Photino.NET;

namespace App1.Photino
{
	//NOTE: To hide the console window, go to the project properties and change the Output Type to Windows Application.
	// Or edit the .csproj file and change the <OutputType> tag from "WinExe" to "Exe".
	internal static class Program
	{
		private static PhotinoWindow s_window;
		
		[STAThread]
		static void Main(string[] args)
		{
			// Creating a new PhotinoWindow instance with the fluent API
			s_window = new PhotinoWindow()
				.SetTitle("ONI Mod Launcher")
				.SetMinSize(1280, 800)
				.SetSize(1280, 800)
				.Center() // Center window in the middle of the screen
				.SetLogVerbosity(0)
				.ConfigureOpenSilver<App>() // Configure OpenSilver App
#if DEBUG
				.SetDevToolsEnabled(true)
#endif
				.SetContextMenuEnabled(false)
				.Load("wwwroot/index.html"); // Can be used with relative path strings or "new URI()" instance to load a website.
			
			var messageBoxes = new PhotinoMessageBoxes(s_window);
			ShellHelper.SetMessageBoxesHandler(messageBoxes);

			s_window.WaitForClose(); // Starts the application event loop
		}
	}
}
