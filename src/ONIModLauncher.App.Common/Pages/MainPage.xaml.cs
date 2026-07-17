using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NativeMessageBox;
using ONIModLauncher.Core;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace ONIModLauncher.App.Common
{
	public partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
		}
		
		private void window_Loaded(object sender, RoutedEventArgs e)
		{
			sideBar.DataContext = Launcher.Instance;
			modsList.DataContext = ModManager.Instance;
			modsList.SetupFilters();
		}

		private void window_Closing(object sender, RoutedEventArgs e)
		{
			Launcher.Instance.StopGameMonitor();
			AppSettings.Save();
		}
	}
}
