using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using ONIModLauncher.Configs;

namespace ONIModLauncher
{
	public class ONIMod : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public string SortingIndex
		{
			get
			{
				if (ID == "2018291283" || Title == "Mod Updater") // Mod Updater
				{
					return ".....ModUpdater";
				}
				else if (Title == "Fast Track") // Fast Track
				{
					return "....FastTrack";
				}
				else if (ID == "1967921388" || Title == "Stock Bug Fix") // Stock Bug Fix
				{
					return "...StockBugFix";
				}
				else if (ID == "2692663069") // Mod Translations
				{
					return "###ModTranslation";
				}
				else if (IsDev)
				{
					return $"..{ID}";
				}
				else if (IsLocal)
				{
					return $".{ID}";
				}
				else
				{
					return ID;
				}
			}
		}

		public string ID
		{ get; set; }

		public string StaticID
		{ get; set; }

		public string Title
		{ get; set; }

		public int Version
		{ get; set; }

		public ModType Type
		{ get; set; }

		public bool IsSteam => Type == ModType.Steam;

		public bool IsLocal => Type == ModType.Local;

		public bool IsDev => Type == ModType.Dev;

		public string Folder
		{ get; set; }

		public int CrashCount
		{ get; set; }

		public bool HasConfig => ConfigFile != null;

		public string ConfigFile
		{ get; set; }


		// Vanilla

		public bool SupportsVanilla
		{ get; set; }

		private bool enabledVanilla;
		public bool EnabledVanilla
		{
			get => SupportsVanilla && enabledVanilla;
			set
			{
				enabledVanilla = value;
				InvokePropertyChanged(nameof(EnabledVanilla));
			}
		}

		// DLC 1

		public bool SupportsDLC1
		{ get; set; }

		private bool enabledDLC1;
		public bool EnabledDLC1
		{
			get => SupportsDLC1 && enabledDLC1;
			set
			{
				enabledDLC1 = value;
				InvokePropertyChanged(nameof(EnabledDLC1));
			}
		}

		public ModLauncherJson LauncherData
		{ get; set; }

		public ICommand OpenConfigCommand
		{ get; private set; }

		public ICommand OpenWorkshopCommand
		{ get; private set; }

		public ICommand OpenFolderCommand
		{ get; private set; }

		public ONIMod()
		{
			OpenConfigCommand = new BasicActionCommand(
				(param) => ShellHelper.OpenTextFile(ConfigFile),
				(param) => HasConfig
			);

			OpenWorkshopCommand = new BasicActionCommand(
				(param) => ShellHelper.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={ID}"),
				(param) => IsSteam
			);

			OpenFolderCommand = new BasicActionCommand(
				(param) => ShellHelper.OpenFolder(Folder),
				(param) => true
			);
		}
	}
}
