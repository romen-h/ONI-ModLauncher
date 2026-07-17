using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NativeMessageBox;
using ONIModLauncher.Core;

namespace ONIModLauncher.App.Common
{
	public sealed partial class App : Application
	{
		public App()
		{
			AppSettings.Init();
			SteamIntegration.Instance.Init();
			if (SteamIntegration.Instance.InitError != null)
			{
				ShellHelper.ShowMessageBox(
					SteamIntegration.Instance.InitError,
					"Steam Initialization Error",
					icon: ShellHelper.MessageBoxIcons.Error);
			}
			
			if (!GamePaths.Init())
			{
				ShellHelper.ShowMessageBox(
					"Failed to initialize game paths. Please make sure the game is installed and the game folder is set in the launcher settings.",
					"Paths Initialization Error",
					icon: ShellHelper.MessageBoxIcons.Error);
				throw new Exception("Failed to initialize game paths.");
			}
				
			Launcher.Instance.StartGameMonitor();
			
			this.InitializeComponent();

			var mainPage = new MainPage();
			this.RootVisual = mainPage;
		}
	}
}
