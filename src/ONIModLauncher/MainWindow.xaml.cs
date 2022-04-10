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

using Newtonsoft.Json;

namespace ONIModLauncher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void window_Loaded(object sender, RoutedEventArgs e)
		{
			if (GamePaths.Init())
			{
				Launcher.Instance.StartGameMonitor();
				sideBar.DataContext = Launcher.Instance;
				modsList.DataContext = ModManager.Instance;
			}
			else
			{
				Close();
			}
		}

		private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Launcher.Instance.StopGameMonitor();
		}

		private void minimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void maximizeButton_Click(object sender, RoutedEventArgs e)
		{
			if (WindowState != WindowState.Maximized)
			{
				WindowState = WindowState.Maximized;
			}
			else
			{
				WindowState = WindowState.Normal;
			}
		}

		private void closeButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
