using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Newtonsoft.Json;

using ONIModLauncher.Configs;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ONIModLauncher
{
	public class ModManager
	{
		public static readonly ModManager Instance = new ModManager();

		private const string VANILLA_ID = "";

		private const string DLC1_ID = "EXPANSION1_ID";

		private FileSystemWatcher configWatcher;

		private ModConfigJson modConfig;

		public ObservableCollection<ONIMod> Mods
		{ get; private set; }

		private SynchronizationContext ctx;

		private ModManager()
		{
			//configWatcher = new FileSystemWatcher(GamePaths.ModsFolder);

			Mods = new ObservableCollection<ONIMod>();
			Mods.CollectionChanged += Mods_CollectionChanged;

			ctx = SynchronizationContext.Current;

			LoadModList(GamePaths.ModsConfigFile);
		}

		public void LoadModList(string path)
		{
			ctx.Post((state) =>
			{
				autoSaveDisabled = true;
				try
				{

					Mods.Clear();

					modConfig = ModConfigJson.Load(path);

					foreach (var mod in modConfig.mods)
					{
						try
						{
							AddMod(mod);
						}
						catch (Exception ex)
						{
							//Debug.Fail(ex.ToString());
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
				autoSaveDisabled = false;
			}, null);
		}

		private void AddMod(ModConfigItem modListItem)
		{
			ONIMod mod = new ONIMod();

			mod.StaticID = modListItem.staticID;

			if (modListItem.label != null)
			{
				ModConfigLabel label = modListItem.label;

				mod.ID = label.id;
				mod.Title = label.title;
				mod.Version = label.version;

				if (label.distribution_platform == 0) // Local
				{
					mod.Folder = Path.Combine(GamePaths.LocalModsFolder, label.id);
					mod.Type = ModType.Local;
				}
				else if (label.distribution_platform == 1) // Steam
				{
					mod.Folder = Path.Combine(GamePaths.SteamModsFolder, label.id);
					mod.Type = ModType.Steam;
				}
				else if (label.distribution_platform == 4) // Dev
				{
					mod.Folder = Path.Combine(GamePaths.DevModsFolder, label.id);
					mod.Type = ModType.Dev;
				}
			}

			string modInfoYamlFile = Path.Combine(mod.Folder, "mod_info.yaml");
			ModInfoYaml modInfo = ModInfoYaml.Load(modInfoYamlFile);

			if (modInfo != null)
			{
				string supported = modInfo.supportedContent.ToLower();

				if (supported.Contains("vanilla_id") || supported.Contains("all"))
				{
					mod.SupportsVanilla = true;
				}
				if (modListItem.enabledForDlc != null)
				{
					mod.EnabledVanilla = modListItem.enabledForDlc.Contains(VANILLA_ID);
				}

				if (supported.Contains("expansion1_id") || supported.Contains("all"))
				{
					mod.SupportsDLC1 = true;
				}
				if (modListItem.enabledForDlc != null)
				{
					mod.EnabledDLC1 = modListItem.enabledForDlc.Contains(DLC1_ID);
				}
			}
			else
			{
				mod.SupportsVanilla = false;
				mod.EnabledVanilla = false;
				mod.SupportsDLC1 = false;
				mod.EnabledDLC1 = false;
			}

			mod.CrashCount = modListItem.crash_count;

			string[] modJsonFiles = Directory.GetFiles(mod.Folder, "*.json");
			foreach (string jsonFile in modJsonFiles)
			{
				string fileNameNoExt = Path.GetFileNameWithoutExtension(jsonFile).ToLower();
				if (fileNameNoExt.Contains("config"))
				{
					mod.ConfigFile = jsonFile;
					break;
				}
				else if (fileNameNoExt.Contains("settings"))
				{
					mod.ConfigFile = jsonFile;
					break;
				}
			}

			mod.PropertyChanged += Mod_PropertyChanged;

			Mods.Add(mod);
		}

		private bool autoSaveDisabled = false;

		private void Mods_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (autoSaveDisabled) return;

			SaveModList(GamePaths.ModsConfigFile);
		}

		private void Mod_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (autoSaveDisabled) return;

			if (e.PropertyName == null || e.PropertyName == nameof(ONIMod.EnabledVanilla) || e.PropertyName == nameof(ONIMod.EnabledDLC1))
			{
				SaveModList(GamePaths.ModsConfigFile);
			}
		}

		public void EnableAllMods()
		{
			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				mod.EnabledVanilla = true;
				mod.EnabledDLC1 = true;
			}
			autoSaveDisabled = false;
			SaveModList(GamePaths.ModsConfigFile);
		}

		public void DisableAllMods()
		{
			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				mod.EnabledVanilla = false;
				mod.EnabledDLC1 = false;
			}
			autoSaveDisabled = false;
			SaveModList(GamePaths.ModsConfigFile);
		}

		public void SortMods()
		{
			autoSaveDisabled = true;
			var arr = Mods.ToArray();
			Mods.Clear();
			var sorted = arr.OrderBy(i => i.SortingIndex);
			foreach (var mod in sorted)
			{
				Mods.Add(mod);
			}
			autoSaveDisabled = false;
			SaveModList(GamePaths.ModsConfigFile);
		}

		public void SaveModList(string path)
		{
			try
			{
				ModConfigJson modConfig = new ModConfigJson();

				modConfig.version = this.modConfig.version;

				modConfig.mods = new ObservableCollection<ModConfigItem>();

				foreach (var mod in Mods)
				{
					try
					{
						ModConfigItem modListItem = new ModConfigItem();

						modListItem.label = new ModConfigLabel();
						modListItem.label.id = mod.ID;
						modListItem.label.title = mod.Title;
						modListItem.label.version = mod.Version;

						if (mod.Type == ModType.Local)
						{
							modListItem.label.distribution_platform = 0;
						}
						else if (mod.Type == ModType.Steam)
						{
							modListItem.label.distribution_platform = 1;
						}
						else if (mod.Type == ModType.Dev)
						{
							modListItem.label.distribution_platform = 4;
						}

						modListItem.status = 1;
						modListItem.enabled = false;
						modListItem.enabledForDlc = new List<string>();
						if (mod.EnabledVanilla)
							modListItem.enabledForDlc.Add(VANILLA_ID);
						if (mod.EnabledDLC1)
							modListItem.enabledForDlc.Add(DLC1_ID);
						modListItem.crash_count = mod.CrashCount;
						modListItem.staticID = mod.StaticID;

						modConfig.mods.Add(modListItem);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.ToString());
					}
				}

				ModConfigJson.Save(modConfig, path);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
			}
		}
	}

	public enum ModType
	{
		Steam,
		Local,
		Dev
	}
}
