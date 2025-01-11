using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ONIModLauncher.Configs;

namespace ONIModLauncher
{
	public class ONIMod : INotifyPropertyChanged
	{
		private static readonly Dictionary<string, string> s_SortingBiases = new Dictionary<string, string>()
		{
			{ "2018291283", "0000" }, // Mod Updater
			{ "2854869130", "0001" }, // Mod Profile Manager
			{ "3281716506", "0002" }, // Mod Preset Manager
			{ "1967921388", "0003" }, // Stock Bug Fix
		};

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public string SortingIndex
		{
			get
			{
				if (s_SortingBiases.TryGetValue(ID, out string sortPrefix))
				{
					return $"0_{sortPrefix}.{ID}";
				}
				else if (ModManager.Instance.Settings.KeepEnabled.Contains(UniqueKey))
				{
					return $"1_{ID}";
				}
				else if (IsDev)
				{
					return $"2_{ID}";
				}
				else if (IsLocal)
				{
					return $"3_{ID}";
				}
				else if (IsBroken)
				{
					return $"5_{ID}";
				}
				else
				{
					return $"4_{ID}";
				}
			}
		}

		public string UniqueKey => $"{Type}.{ID}";

		public string ID
		{ get; set; }

		public string StaticID
		{ get; set; }

		public string Title
		{ get; set; }

		public Color TitleColor
		{ get; set; } = Colors.White;

		public DateTimeOffset Version
		{ get; set; }

		public string Author
		{ get; set; }

		public Uri RepoURL
		{ get; set; }

		public bool RepoIsGithub
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

		public bool EnabledForCurrentDLC
		{
			get => Launcher.Instance.PlayerPrefs.DLC1Enabled ? EnabledDLC1 : EnabledVanilla;
			set
			{
				if (Launcher.Instance.PlayerPrefs.DLC1Enabled)
				{
					EnabledDLC1 = value;
				}
				else
				{
					EnabledVanilla = value;
				}
			}
		}

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

		public bool KeepEnabled
		{
			get => ModManager.Instance.Settings.KeepEnabled.Contains(UniqueKey);
			set
			{
				if (value)
				{
					ModManager.Instance.Settings.AddKeepEnabled(UniqueKey);
				}
				else
				{
					ModManager.Instance.Settings.RemoveKeepEnabled(UniqueKey);
				}
				InvokePropertyChanged(nameof(KeepEnabled));
			}
		}

		public bool IsBroken
		{
			get => ModManager.Instance.Settings.BrokenMods.Contains(UniqueKey);
			set
			{
				if (value)
				{
					ModManager.Instance.Settings.AddBrokenMod(UniqueKey);
				}
				else
				{
					ModManager.Instance.Settings.RemoveBrokenMod(UniqueKey);
				}
				InvokePropertyChanged(nameof(IsBroken));
			}
		}

		public LauncherMetadataJson LauncherData
		{ get; set; }

		public ICommand OpenConfigCommand
		{ get; private set; }

		public ICommand OpenWorkshopCommand
		{ get; private set; }

		public ICommand OpenRepoCommand
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

			OpenRepoCommand = new BasicActionCommand(
				(param) => ShellHelper.OpenURL(RepoURL.ToString()),
				(param) => RepoURL != null
			);

			OpenFolderCommand = new BasicActionCommand(
				(param) => ShellHelper.OpenFolder(Folder),
				(param) => true
			);
		}
	}
}
