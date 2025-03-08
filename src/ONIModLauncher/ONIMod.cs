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

		private readonly Dictionary<string, Compatibility> _compatibilities = new Dictionary<string, Compatibility>();

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		internal void RefreshForUI()
		{
			InvokePropertyChanged(null);
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

		public bool ParsedLegacyCompatibility
		{ get; set; } = false;

		public bool CanEditEnabled
		{
			get
			{
				if (Launcher.Instance.IsRunning) return false;
				if (ParsedLegacyCompatibility) return true;
				return SupportsCurrentDLC;
			}
		}

		internal bool enabledForVanilla = false;
		internal bool enabledForSpacedOut = false;

		public bool Enabled
		{
			get
			{
				// If we're using strictly the new compatibility yaml then we can infer it may never be enabled
				if (!ParsedLegacyCompatibility && !SupportsCurrentDLC) return false;
				if (Launcher.Instance.PlayerPrefs.SpacedOutEnabled)
				{
					return enabledForSpacedOut;
				}
				else
				{
					return enabledForVanilla;
				}
			}
			set
			{
				// If we're using strictly the new compatibility yaml then we can deny this edit
				if (value && !ParsedLegacyCompatibility && !SupportsCurrentDLC) return;
				if (Launcher.Instance.PlayerPrefs.SpacedOutEnabled)
				{
					enabledForSpacedOut = value;
				}
				else
				{
					enabledForVanilla = value;
				}
			}
		}

		public bool SupportsCurrentDLC
		{
			get
			{
				if (Launcher.Instance.PlayerPrefs.SpacedOutEnabled == false && !SupportsVanilla) return false;
				if (GamePaths.HasSpacedOut && Launcher.Instance.PlayerPrefs.SpacedOutEnabled && !SupportsSpacedOut) return false;
				if (GamePaths.HasFrostyPlanetPack && !SupportsFrostyPlanetPack) return false;
				if (GamePaths.HasBionicBoosterPack && !SupportsBionicBoosterPack) return false;
				
				return true;
			}
		}

		public bool SupportsVanilla => (int)_compatibilities[DLC.Vanilla] >= (int)Compatibility.Compatible;

		public bool SupportsSpacedOut => (int)_compatibilities[DLC.SpacedOut] >= (int)Compatibility.Compatible;

		public bool SupportsFrostyPlanetPack => (int)_compatibilities[DLC.FrostyPlanetPack] >= (int)Compatibility.Compatible;

		public bool SupportsBionicBoosterPack => (int)_compatibilities[DLC.BionicBoosterPack] >= (int)Compatibility.Compatible;

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

		public bool IsEditable => !Launcher.Instance.IsRunning;

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

			_compatibilities[DLC.Vanilla] = Compatibility.Unknown;
			_compatibilities[DLC.SpacedOut] = Compatibility.Unknown;
			_compatibilities[DLC.FrostyPlanetPack] = Compatibility.Unknown;
			_compatibilities[DLC.BionicBoosterPack] = Compatibility.Unknown;
		}

		internal void SetCompatibility(string dlc, Compatibility compat)
		{
			_compatibilities[dlc] = compat;
		}
	}
}
