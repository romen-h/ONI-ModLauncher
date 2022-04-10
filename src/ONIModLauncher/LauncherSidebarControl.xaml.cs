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

namespace ONIModLauncher
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

		private void launchButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.SaveModList(GamePaths.ModsConfigFile);

			Launcher.Instance.Launch();
		}

		private void pickSaveButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Oxygen Not Included Save (*.sav)|*.sav";
			if (Directory.Exists(GamePaths.SavesFolder))
			{
				dlg.InitialDirectory = GamePaths.SavesFolder;
			}

			if (dlg.ShowDialog() == true)
			{
				Launcher.Instance.PlayerPrefs.SaveFile = dlg.FileName;
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
