using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;

using ONIModLauncher.Configs;

using YamlDotNet.Serialization;

namespace ONIModLauncher
{
	public class ModManager : INotifyPropertyChanged
	{
		private static ModManager s_instance;

		public static ModManager Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new ModManager();
				}

				return s_instance;
			}
		}

		private const string VANILLA_ID = "";

		private const string DLC1_ID = "EXPANSION1_ID";

		public LauncherSettingsJson Settings
		{ get; private set; }

		private ModConfigJson modConfig;

		private readonly Dictionary<ulong, ONIMod> _steamMods = new Dictionary<ulong, ONIMod>();

		public ObservableCollection<ONIMod> Mods
		{ get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private SynchronizationContext ctx;

		private ModManager()
		{
			Mods = new ObservableCollection<ONIMod>();
			Mods.CollectionChanged += Mods_CollectionChanged;

			ctx = SynchronizationContext.Current;

			LoadSettings();

			LoadModList();
		}

		public void LoadSettings()
		{
			Settings = new LauncherSettingsJson();

			if (File.Exists(GamePaths.LauncherSettingsFile))
			{
				try
				{
					string json = File.ReadAllText(GamePaths.LauncherSettingsFile);
					Settings = JsonConvert.DeserializeObject<LauncherSettingsJson>(json);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}

			Settings.PropertyChanged += LauncherConfig_PropertyChanged;
		}

		public void SaveSettings()
		{
			try
			{
				string json = JsonConvert.SerializeObject(Settings);
				File.WriteAllText(GamePaths.LauncherSettingsFile, json);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		private void LauncherConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SaveSettings();
		}

		public void LoadModList() => LoadModList(GamePaths.ModsConfigFile);

		public void LoadModList(string path)
		{
			ctx.Post((state) =>
			{
				autoSaveDisabled = true;
				if (File.Exists(path))
				{
					try
					{
						Mods.Clear();
						_steamMods.Clear();

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
				}
				else if (SteamIntegration.Instance.UseSteam)
				{
					RebuildModList();
				}
				autoSaveDisabled = false;
			}, null);
		}

		public void RebuildModList()
		{
			ctx.Post((state) =>
			{
				if (modConfig == null)
				{
					modConfig = new ModConfigJson()
					{
						version = 1
					};
				}

				var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

				foreach (var modFolder in Directory.GetDirectories(GamePaths.DevModsFolder))
				{
					try
					{
						string folderName = Path.GetFileName(modFolder);

						string modYamlFile = Path.Combine(modFolder, "mod.yaml");
						string modYaml = File.ReadAllText(modYamlFile);
						ModYaml modInfo = deserializer.Deserialize<ModYaml>(modYaml);

						ONIMod mod = FindMod(modInfo.staticID);

						if (mod == null)
						{
							ModConfigItem modConfig = new ModConfigItem()
							{
								label = new ModConfigLabel()
								{
									distribution_platform = DistributionPlatform.Dev,
									id = folderName,
									title = modInfo.title,
									version = folderName.GetHashCode()
								},
								status = ModStatus.Installed,
								enabled = false,
								enabledForDlc = new List<string>(),
								crash_count = 0,
								reinstall_path = null,
								staticID = modInfo.staticID
							};

							AddMod(modConfig);
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.ToString());
					}
				}

				foreach (var modFolder in Directory.GetDirectories(GamePaths.LocalModsFolder))
				{
					try
					{
						string folderName = Path.GetFileName(modFolder);

						string modYamlFile = Path.Combine(modFolder, "mod.yaml");
						string modYaml = File.ReadAllText(modYamlFile);
						ModYaml modInfo = deserializer.Deserialize<ModYaml>(modYaml);

						ONIMod mod = FindMod(modInfo.staticID);

						if (mod == null)
						{
							ModConfigItem modConfig = new ModConfigItem()
							{
								label = new ModConfigLabel()
								{
									distribution_platform = DistributionPlatform.Local,
									id = folderName,
									title = modInfo.title,
									version = folderName.GetHashCode()
								},
								status = ModStatus.Installed,
								enabled = false,
								enabledForDlc = new List<string>(),
								crash_count = 0,
								reinstall_path = null,
								staticID = modInfo.staticID
							};

							AddMod(modConfig);
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.ToString());
					}
				}

				if (SteamIntegration.Instance.UseSteam)
				{
					foreach (var modFolder in Directory.GetDirectories(GamePaths.SteamModsFolder))
					{
						try
						{
							string folderName = Path.GetFileName(modFolder);
							ulong id = ulong.Parse(folderName);

							string modYamlFile = Path.Combine(modFolder, "mod.yaml");
							string modYaml = File.ReadAllText(modYamlFile);
							ModYaml modInfo = deserializer.Deserialize<ModYaml>(modYaml);

							ONIMod mod = FindMod(modInfo.staticID);

							if (mod == null)
							{
								ModConfigItem modConfig = new ModConfigItem()
								{
									label = new ModConfigLabel()
									{
										distribution_platform = DistributionPlatform.Steam,
										id = id.ToString(),
										title = modInfo.title,
										version = SteamIntegration.Instance.GetModUpdateTime(id)
									},
									status = ModStatus.Installed,
									enabled = false,
									enabledForDlc = new List<string>(),
									crash_count = 0,
									reinstall_path = null,
									staticID = modInfo.staticID
								};

								AddMod(modConfig);
							}
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex.ToString());
						}
					}
				}

				SaveModList();

			}, null);
		}

		private void AddMod(ModConfigItem modListItem)
		{
			ONIMod mod = new ONIMod();

			mod.StaticID = modListItem.staticID;

			ulong steamID = 0;

			if (modListItem.label != null)
			{
				ModConfigLabel label = modListItem.label;

				mod.ID = label.id;
				mod.Title = label.title;
				mod.Version = DateTimeOffset.FromUnixTimeSeconds(label.version);

				if (label.distribution_platform == DistributionPlatform.Local) // Local
				{
					mod.Folder = Path.Combine(GamePaths.LocalModsFolder, label.id);
					mod.Type = ModType.Local;
				}
				else if (label.distribution_platform == DistributionPlatform.Steam) // Steam
				{
					mod.Folder = Path.Combine(GamePaths.SteamModsFolder, label.id);
					mod.Type = ModType.Steam;
					steamID = ulong.Parse(label.id);
				}
				else if (label.distribution_platform == DistributionPlatform.Dev) // Dev
				{
					mod.Folder = Path.Combine(GamePaths.DevModsFolder, label.id);
					mod.Type = ModType.Dev;
				}
			}

			try
			{
				if (mod.Title != null)
				{
					Regex findColorTags = new Regex("<color=#(......)>(.*)</color>");
					var match = findColorTags.Match(mod.Title);
					if (match.Success)
					{
						if (match.Groups.Count == 3)
						{
							string colorHexStr = match.Groups[1].Value;
							byte r = byte.Parse(colorHexStr.Substring(0,2), NumberStyles.HexNumber);
							byte g = byte.Parse(colorHexStr.Substring(2,2), NumberStyles.HexNumber);
							byte b = byte.Parse(colorHexStr.Substring(4,2), NumberStyles.HexNumber);
							Color color = Color.FromRgb(r, g, b);

							mod.Title = match.Groups[2].Value;
							mod.TitleColor = color;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
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

			string modMetadataFile = Path.Combine(mod.Folder, "LauncherMetadata.json");
			LauncherMetadataJson metadata = LauncherMetadataJson.Load(modMetadataFile);

			if (metadata != null)
			{
				mod.Author = metadata.Author;
				if (metadata.RepoURL != null)
				{
					mod.RepoURL = new Uri(metadata.RepoURL);
					mod.RepoIsGithub = mod.RepoURL.Host == "github.com";
				}
			}

			mod.CrashCount = modListItem.crash_count;
			
			if (metadata.ConfigFiles != null)
			{
				foreach (var configPath in metadata.ConfigFiles)
				{
					string fullConfigPath = Path.Combine(mod.Folder, configPath.Value);
					if (File.Exists(fullConfigPath))
					{
						mod.ConfigFile = fullConfigPath;
						break;
					}
				}
			}

			mod.PropertyChanged += Mod_PropertyChanged;

			Mods.Add(mod);

			if (mod.IsSteam)
			{
				_steamMods.Add(steamID, mod);
			}
		}

		private ONIMod FindMod(string staticID)
		{
			foreach (var mod in Mods)
			{
				if (mod.StaticID == staticID)
				{
					return mod;
				}
			}

			return null;
		}

		private bool autoSaveDisabled = false;

		private void Mods_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (autoSaveDisabled) return;

			SaveModList();
		}

		private void Mod_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (autoSaveDisabled) return;

			if (e.PropertyName == null || e.PropertyName == nameof(ONIMod.EnabledVanilla) || e.PropertyName == nameof(ONIMod.EnabledDLC1))
			{
				SaveModList();
			}
		}

		public void EnableAllMods()
		{
			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				if (mod.IsBroken) continue;

				mod.EnabledVanilla = true;
				mod.EnabledDLC1 = true;
			}
			autoSaveDisabled = false;
			SaveModList();
		}

		public void DisableAllMods()
		{
			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				if (mod.KeepEnabled) continue;

				mod.EnabledVanilla = false;
				mod.EnabledDLC1 = false;
			}
			autoSaveDisabled = false;
			SaveModList();
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
			SaveModList();
		}

		public void BisectTop()
		{
			int enabledModCount = Mods.Count((m) => m.EnabledForCurrentDLC);
			int halfOfEnabledMods = (int)Math.Ceiling(enabledModCount / 2.0);

			foreach (var mod in Mods.Reverse())
			{
				if (mod.EnabledForCurrentDLC)
				{
					mod.EnabledForCurrentDLC = false;
					enabledModCount--;
				}

				if (enabledModCount <= halfOfEnabledMods) break;
			}
		}

		public void BisectBottom()
		{
			int enabledModCount = Mods.Count((m) => m.EnabledForCurrentDLC);
			int halfOfEnabledMods = (int)Math.Floor(enabledModCount / 2.0);

			foreach (var mod in Mods)
			{
				if (mod.EnabledForCurrentDLC)
				{
					mod.EnabledForCurrentDLC = false;
					enabledModCount--;
				}

				if (enabledModCount <= halfOfEnabledMods) break;
			}
		}

		public void SaveModList() => SaveModList(GamePaths.ModsConfigFile);

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
						modListItem.label.version = mod.Version.ToUnixTimeSeconds();

						if (mod.Type == ModType.Local)
						{
							modListItem.label.distribution_platform = DistributionPlatform.Local;
						}
						else if (mod.Type == ModType.Steam)
						{
							modListItem.label.distribution_platform = DistributionPlatform.Steam;
						}
						else if (mod.Type == ModType.Dev)
						{
							modListItem.label.distribution_platform = DistributionPlatform.Dev;
						}

						modListItem.status = ModStatus.Installed;
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
