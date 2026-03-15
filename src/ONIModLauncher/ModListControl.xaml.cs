using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
		private bool warningAcknowledged = false;

		private readonly DelayedEvent searchTypingDelay = new DelayedEvent(TimeSpan.FromMilliseconds(200));

		public ModListControl()
		{
			InitializeComponent();
		}

		internal void SetupFilters()
		{
			modsList.ItemsSource = ModManager.Instance.Mods;
			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(modsList.ItemsSource);
			view.Filter = FilterMethod;
			view.Refresh();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			searchTypingDelay.DelayFinished += SearchTypingDelay_DelayFinished;

			Launcher.Instance.PropertyChanged += Launcher_PropertyChanged;

			modListLockScreen.Visibility = Visibility.Collapsed;
		}

		private void Launcher_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(() => Launcher_PropertyChanged(sender, e));
				return;
			}

			GongSolutions.Wpf.DragDrop.DragDrop.SetIsDragSource(modsList, Launcher.Instance.IsNotRunning);
			modListLockScreen.Visibility = (warningAcknowledged || Launcher.Instance.IsNotRunning) ? Visibility.Collapsed : Visibility.Visible;
			saveModListButton.IsEnabled = Launcher.Instance.IsNotRunning;
			loadModListButton.IsEnabled = Launcher.Instance.IsNotRunning;
			selectAllModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			unselectAllModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			sortModsButton.IsEnabled = Launcher.Instance.IsNotRunning;
			bisectTopButton.IsEnabled = Launcher.Instance.IsNotRunning;
			bisectBottomButton.IsEnabled = Launcher.Instance.IsNotRunning;
		}

		private void refreshModsButton_Click(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.LoadModList();

			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(modsList.ItemsSource);
			view.Filter = FilterMethod;
			view.Refresh();
		}

		private void DetectModsButton_OnClick(object sender, RoutedEventArgs e)
		{
			
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

		private void InstallModUpdaterMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var owner = Window.GetWindow(this);
			if (MessageBox.Show(owner, "This option will download/update Mod Updater to the latest version from Peter Han's GitHub repo and install it as a Local mod.\nDo you want to continue?", "Download Mod Updater", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

			Task.Run(async () => {
				try
				{
					string modFolder = System.IO.Path.Combine(GamePaths.LocalModsFolder, "ModUpdateDate");
					await ModManager.Instance.InstallModFromURL(ModUpdateUrls.PeterHan_ModUpdater, modFolder, null, "ModUpdateDate");
					Dispatcher.Invoke(() =>
					{
						MessageBox.Show(owner, "Mod Updater successfully installed.", "Update Success", MessageBoxButton.OK);
					});
				}
				catch (Exception ex)
				{
					Dispatcher.Invoke(() =>
					{
						MessageBox.Show(owner, $"Failed to update mod.\n{ex.Message}", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
					});
				}
			});
		}

		private void RebuildModsListMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var owner = Window.GetWindow(this);
			if (MessageBox.Show(owner, "Are you sure you want to rebuild the mods list and overwrite the mods.json?", "Rebuild Mods List?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

			try
			{
				ModManager.Instance.RebuildModList();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to rebuild mods list:");
				Debug.WriteLine(ex.ToString());
				MessageBox.Show(owner, "Failed to rebuild mods list.\nYou may need to run the game to generate mods.json again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(modsList.ItemsSource);
			view.Filter = FilterMethod;
			view.Refresh();
		}

		private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			searchTypingDelay.Start();
		}

		private void ModTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			searchTypingDelay.Start();
		}

		private void SearchTypingDelay_DelayFinished(object sender, EventArgs e)
		{
			FilterModsList();
		}

		private void FilterModsList()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(FilterModsList);
				return;
			}

			if (modsList != null)
			{
				CollectionViewSource.GetDefaultView(modsList.ItemsSource)?.Refresh();
			}
		}

		private bool FilterMethod(object item)
		{
			ONIMod mod = (ONIMod)item;

			int folderTypeIndex = modTypeComboBox.SelectedIndex;
			if (folderTypeIndex != 0)
			{
				if (folderTypeIndex == 1 && !mod.IsDev) return false;
				if (folderTypeIndex == 2 && !mod.IsLocal) return false;
				if (folderTypeIndex == 3 && !mod.IsSteam) return false;
			}

			string searchText = searchBox.Text;
			if (string.IsNullOrEmpty(searchText)) return true;

			if (mod.Title.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) return true;
			if (mod.FolderName.ToLowerInvariant().Contains(searchText.ToLowerInvariant())) return true;

			return false;
		}

		private void ToggleKeepEnabledMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				mod.KeepEnabled = !mod.KeepEnabled;
			}
		}

		private void ToggleBrokenMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				mod.IsBroken = !mod.IsBroken;
			}
		}

		private void MoveToTopMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				ModManager.Instance.Mods.Remove(mod);
				ModManager.Instance.Mods.Insert(0, mod);
			}
		}

		private void MoveToBottomMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				ModManager.Instance.Mods.Remove(mod);
				ModManager.Instance.Mods.Add(mod);
			}
		}

		private void BisectTopButton_OnClick(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.BisectTop();
		}

		private void BisectBottomButton_OnClick(object sender, RoutedEventArgs e)
		{
			ModManager.Instance.BisectBottom();
		}

		private void HideGameRunningWarningBtn_Click(object sender, RoutedEventArgs e)
		{
			warningAcknowledged = true;
			modListLockScreen.Visibility = Visibility.Collapsed;
		}

		private void ConvertToLocalMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is ONIMod mod)
			{
				ModManager.Instance.ConvertToLocal(mod);
			}
		}

		private async void UpdateModMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is not ONIMod mod) return;
			
			string updateUrl = "";

			try
			{
				await ModManager.Instance.InstallModFromURL(updateUrl, mod.Folder, mod.StaticID);
			}
			catch (Exception ex)
			{
				Window owner = Window.GetWindow(this);
				MessageBox.Show(owner, $"Failed to update mod.\n{ex.Message}", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void GenerateModYamlMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuItem mi = sender as MenuItem;
			if (mi.DataContext is not ONIMod mod) return;
			
			mod.WriteModYaml();
		}
	}
}
