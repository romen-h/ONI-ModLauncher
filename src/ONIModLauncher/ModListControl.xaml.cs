using System;
using System.Collections.Generic;
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
	/// Interaction logic for ModListControl.xaml
	/// </summary>
	public partial class ModListControl : UserControl
	{
		public ModListControl()
		{
			InitializeComponent();

			Launcher.Instance.PropertyChanged += Launcher_PropertyChanged;

			IsEnabled = Launcher.Instance.IsNotRunning;
			modListLockScreen.Visibility = Visibility.Collapsed;
		}

		private void Launcher_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Launcher_PropertyChanged(sender, e));
				return;
			}

			IsEnabled = Launcher.Instance.IsNotRunning;
			modListLockScreen.Visibility = Launcher.Instance.IsNotRunning ? Visibility.Collapsed : Visibility.Visible;
		}

		private void refreshModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.LoadModList(GamePaths.ModsConfigFile);
			modsList.ItemsSource = ModManager.Instance.Mods;
		}

		private void selectAllModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.EnableAllMods();
		}

		private void unselectAllModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.DisableAllMods();
		}

		private void sortModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.SortMods();
		}

		private void saveModListButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == true)
			{
				ModManager.Instance.SaveModList(dlg.FileName);
			}
		}

		private void loadModListButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (dlg.ShowDialog() == true)
			{
				ModManager.Instance.LoadModList(dlg.FileName);
			}
		}
	}
}
