using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;
using ONIModLauncher.Core;

namespace ONIModLauncher.App.Common.Controls
{
	/// <summary>
	/// Interaction logic for LauncherSidebarControl.xaml
	/// </summary>
	public partial class LauncherSidebarControl : UserControl
	{
		public LauncherSidebarControl()
		{
			InitializeComponent();
		}

		private void LauncherSidebarControl_OnLoaded(object sender, RoutedEventArgs e)
		{
#if ENABLE_WORKSHOP
			modBrowserButton.Visibility = SteamIntegration.Instance.UseSteam ? Visibility.Visible : Visibility.Collapsed;
#endif
		}

		private void launchButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.SaveModList();

			Launcher.Instance.Launch();
		}

		private void pickSaveButton_Click(object sender, RoutedEventArgs e)
		{
			var filePath = ShellHelper.OpenFile("Oxygen Not Included Save", "*.sav", GamePaths.SavesFolder);
			if (filePath != null)
			{
				Launcher.Instance.PlayerPrefs.SaveFile = filePath;
			}
		}


		private void modFolderButton_Click(object sender, RoutedEventArgs e)
		{
			if (Directory.Exists(GamePaths.ModsFolder))
			{
				ShellHelper.OpenFolder(GamePaths.ModsFolder);
			}
		}

		private void gameLogButton_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(GamePaths.GameLogFile))
			{
				ShellHelper.OpenTextFile(GamePaths.GameLogFile);
			}
		}
	}
}
