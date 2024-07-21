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
					return $"...{sortPrefix}.{ID}";
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
