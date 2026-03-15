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
	public partial class ModManager : INotifyPropertyChanged
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

		public LauncherSettingsJson Settings
		{ get; private set; }

		private ModConfigJson modConfig;

		private readonly Dictionary<ulong, ONIMod> _steamMods = new Dictionary<ulong, ONIMod>();

		public ObservableCollection<ONIMod> Mods
		{ get; private set; }
		
		public int EnabledModCount => Mods.Count(mod => mod.Enabled);
		internal void RecountEnabledMods()
		{
			InvokePropertyChanged(nameof(EnabledModCount));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private SynchronizationContext ctx;

		private ModManager()
		{
			Launcher.Instance.PropertyChanged += Launcher_PropertyChanged;

			Mods = new ObservableCollection<ONIMod>();
			Mods.CollectionChanged += Mods_CollectionChanged;

			ctx = SynchronizationContext.Current;

			LoadSettings();

			LoadModList();
		}

		private void Launcher_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Launcher.IsRunning) || e.PropertyName == null)
			{
				RefreshAllModsForUI();
			}
			
			if (e.PropertyName == nameof(Launcher.PlayerPrefs) || e.PropertyName == null)
			{
				RefreshAllModsForUI();
			}
		}

		public void LoadSettings()
		{
			if (Settings != null)
			{
				Settings.PropertyChanged -= LauncherConfig_PropertyChanged;
				Settings = null;
			}
			
			Settings = LauncherSettingsJson.Load(GamePaths.LauncherSettingsFile);
			
			if (Settings == null)
			{
				Settings = new LauncherSettingsJson();
			}

			Settings.PropertyChanged += LauncherConfig_PropertyChanged;
		}

		private void LauncherConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			LauncherSettingsJson.Save(GamePaths.LauncherSettingsFile, Settings);
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
								Debug.WriteLine(ex.ToString());
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
				Mods.Clear();
				_steamMods.Clear();

				if (modConfig == null)
				{
					modConfig = new ModConfigJson()
					{
						version = ModConfigJson.CURRENT_SCHEMA_VERSION,
						mods = new ObservableCollection<ModConfigItem>()
					};
				}

				var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

				// Scan dev mods for ones missing from the mods.json
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

				// Scan local mods for ones missing from the mods.json
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

				// Scan steam mods for ones missing from the mods.json
				if (SteamIntegration.Instance.UseSteam)
				{
					foreach (var modFolder in Directory.GetDirectories(GamePaths.SteamModsFolder))
					{
						try
						{
							string folderName = Path.GetFileName(modFolder);
							ulong id = ulong.Parse(folderName);
							
							string staticID = folderName;
							string title = "(Untitled Mod)";

							string modYamlFile = Path.Combine(modFolder, "mod.yaml");
							if (File.Exists(modYamlFile))
							{
								string modYaml = File.ReadAllText(modYamlFile);
								ModYaml modInfo = deserializer.Deserialize<ModYaml>(modYaml);
								staticID = modInfo.staticID;
								title = modInfo.title;
							}
							else
							{
								staticID = $"{folderName}.Steam";
								
								var dlls = Directory.GetFiles(modFolder, "*.dll");
								if (dlls.Length > 0)
								{
									title = Path.GetFileNameWithoutExtension(dlls[0]);
								}
							}

							ONIMod mod = FindMod(staticID);

							if (mod == null)
							{
								ModConfigItem modConfig = new ModConfigItem()
								{
									label = new ModConfigLabel()
									{
										distribution_platform = DistributionPlatform.Steam,
										id = id.ToString(),
										title = title,
										version = SteamIntegration.Instance.GetModUpdateTime(id)
									},
									status = ModStatus.Installed,
									enabled = false,
									enabledForDlc = new List<string>(),
									crash_count = 0,
									reinstall_path = null,
									staticID = staticID
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
			if (modListItem == null) throw new ArgumentNullException(nameof(modListItem));
			
			ONIMod mod = new ONIMod();

			if (string.IsNullOrWhiteSpace(modListItem.staticID)) throw new Exception("StaticID is empty.");
			mod.StaticID = modListItem.staticID;

			if (modListItem.label != null)
			{
				ModConfigLabel label = modListItem.label;

				mod.DistributionId = label.id;
				mod.Title = label.title;

				if (label.distribution_platform == DistributionPlatform.Local) // Local
				{
					mod.Folder = Path.Combine(GamePaths.LocalModsFolder, label.id);
					mod.Type = ModType.Local;
				}
				else if (label.distribution_platform == DistributionPlatform.Steam) // Steam
				{
					mod.Folder = Path.Combine(GamePaths.SteamModsFolder, label.id);
					mod.Type = ModType.Steam;
					mod.SteamWorkshopId = ulong.Parse(label.id);
					mod.SteamUpdateTimestamp = label.version;
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

			string modYamlFile = Path.Combine(mod.Folder, "mod.yaml");
			mod.HasModYaml = File.Exists(modYamlFile);

			string modInfoYamlFile = Path.Combine(mod.Folder, "mod_info.yaml");
			ModInfoYaml modInfo = ModInfoYaml.Load(modInfoYamlFile);

			if (modInfo != null)
			{
				if (Version.TryParse(modInfo.version, out Version version))
				{
					mod.Version = version;
				}
				
				if (modInfo.supportedContent != null)
				{
					string supported = modInfo.supportedContent.ToUpperInvariant();

					if (supported.Contains("ALL"))
					{
						mod.SetCompatibility(DLC.Vanilla, Compatibility.Compatible);
						mod.SetCompatibility(DLC.SpacedOut, Compatibility.Compatible);
						mod.SetCompatibility(DLC.FrostyPlanetPack, Compatibility.Compatible);
						mod.SetCompatibility(DLC.BionicBoosterPack, Compatibility.Compatible);
					}
					else
					{
						// Definitely need to handle this carefully
						mod.SetCompatibility(DLC.Vanilla, supported.Contains(DLC.Vanilla) ? Compatibility.Compatible : Compatibility.Incompatible);
						mod.SetCompatibility(DLC.SpacedOut, supported.Contains(DLC.SpacedOut) ? Compatibility.Compatible : Compatibility.Incompatible);
						mod.SetCompatibility(DLC.FrostyPlanetPack, Compatibility.Unknown);
						mod.SetCompatibility(DLC.BionicBoosterPack, Compatibility.Unknown);
					}

					mod.ParsedLegacyCompatibility = true;
				}
				else
				{
					mod.SetCompatibility(DLC.Vanilla, Compatibility.Compatible);
					mod.SetCompatibility(DLC.SpacedOut, Compatibility.Compatible);
					mod.SetCompatibility(DLC.FrostyPlanetPack, Compatibility.Compatible);
					mod.SetCompatibility(DLC.BionicBoosterPack, Compatibility.Compatible);
				}

				if (modInfo.forbiddenDlcIds != null)
				{
					if (modInfo.forbiddenDlcIds.Contains(DLC.SpacedOut))
					{
						// Vanilla already compatible
						mod.SetCompatibility(DLC.SpacedOut, Compatibility.Incompatible);
					}
					if (modInfo.forbiddenDlcIds.Contains(DLC.FrostyPlanetPack))
					{
						mod.SetCompatibility(DLC.FrostyPlanetPack, Compatibility.Incompatible);
					}
					if (modInfo.forbiddenDlcIds.Contains(DLC.BionicBoosterPack))
					{
						mod.SetCompatibility(DLC.BionicBoosterPack, Compatibility.Incompatible);
					}
				}
				if (modInfo.requiredDlcIds != null)
				{
					if (modInfo.requiredDlcIds.Contains(DLC.SpacedOut))
					{
						mod.SetCompatibility(DLC.Vanilla, Compatibility.Incompatible);
						mod.SetCompatibility(DLC.SpacedOut, Compatibility.Required);
					}
					if (modInfo.requiredDlcIds.Contains(DLC.FrostyPlanetPack))
					{
						mod.SetCompatibility(DLC.FrostyPlanetPack, Compatibility.Required);
					}
					if (modInfo.requiredDlcIds.Contains(DLC.BionicBoosterPack))
					{
						mod.SetCompatibility(DLC.BionicBoosterPack, Compatibility.Required);
					}
				}
			}

			if (modListItem.enabledForDlc != null)
			{
				mod.enabledForVanilla = modListItem.enabledForDlc.Contains("");
			}

			if (modListItem.enabledForDlc != null)
			{
				mod.enabledForSpacedOut = modListItem.enabledForDlc.Contains(DLC.SpacedOut);
			}

			//string modMetadataFile = Path.Combine(mod.Folder, "LauncherMetadata.json");
			//mod.LauncherData = LauncherMetadataJson.Load(modMetadataFile);

			if (mod.LauncherData != null)
			{
#if false
				if (mod.LauncherData.ConfigFiles != null)
				{
					foreach (var configPath in mod.LauncherData.ConfigFiles)
					{
						string fullConfigPath = Path.Combine(mod.Folder, configPath.Value);
						if (File.Exists(fullConfigPath))
						{
							mod.ConfigFile = fullConfigPath;
							break;
						}
					}
				}
#endif
			}

			mod.CrashCount = modListItem.crash_count;
			
			mod.PropertyChanged += Mod_PropertyChanged;

			Mods.Add(mod);

			if (mod.IsSteam)
			{
				_steamMods[mod.SteamWorkshopId.Value] = mod;
			}
		}
		
		private void RemoveMod(ONIMod mod)
		{
			Mods.Remove(mod);
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

		private void RefreshAllModsForUI()
		{
			foreach (var mod in Mods)
			{
				mod.RefreshForUI();
			}
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

			if (e.PropertyName == null || e.PropertyName == nameof(ONIMod.Enabled))
			{
				SaveModList();
			}
		}

		public void EnableAllMods()
		{
			if (Launcher.Instance.IsRunning) return;

			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				if (mod.IsBroken) continue;

				mod.Enabled = true;
			}
			autoSaveDisabled = false;
			SaveModList();
			RefreshAllModsForUI();
		}

		public void DisableAllMods()
		{
			if (Launcher.Instance.IsRunning) return;

			autoSaveDisabled = true;
			foreach (var mod in Mods)
			{
				if (mod.KeepEnabled) continue;

				mod.Enabled = false;
			}
			autoSaveDisabled = false;
			SaveModList();
			RefreshAllModsForUI();
		}

		public void SortMods()
		{
			if (Launcher.Instance.IsRunning) return;

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
			if (Launcher.Instance.IsRunning) return;

			int enabledModCount = Mods.Count((m) => m.Enabled);
			int halfOfEnabledMods = (int)Math.Ceiling(enabledModCount / 2.0);

			foreach (var mod in Mods.Reverse())
			{
				if (mod.Enabled)
				{
					mod.Enabled = false;
					enabledModCount--;
				}

				if (enabledModCount <= halfOfEnabledMods) break;
			}
		}

		public void BisectBottom()
		{
			if (Launcher.Instance.IsRunning) return;

			int enabledModCount = Mods.Count((m) => m.Enabled);
			int halfOfEnabledMods = (int)Math.Floor(enabledModCount / 2.0);

			foreach (var mod in Mods)
			{
				if (mod.Enabled)
				{
					mod.Enabled = false;
					enabledModCount--;
				}

				if (enabledModCount <= halfOfEnabledMods) break;
			}
		}

		public void SaveModList() => SaveModList(GamePaths.ModsConfigFile);

		public void SaveModList(string path)
		{
			ctx.Post((state) =>
			{
				try
				{
					ModConfigJson modConfig = new ModConfigJson()
					{
						version = this.modConfig.version,
						mods = new ObservableCollection<ModConfigItem>()
					};

					foreach (var mod in Mods)
					{
						try
						{
							ModConfigItem modListItem = new ModConfigItem()
							{
								staticID = mod.StaticID,
								label = new ModConfigLabel()
								{
									id = mod.DistributionId,
									title = mod.Title,
									version = mod.SteamUpdateTimestamp,
									distribution_platform = mod.DistributionPlatform
								},
								status = ModStatus.Installed,
								enabled = false,
								enabledForDlc = new List<string>(),
								crash_count = mod.CrashCount
							};

							if (mod.Type == ModType.Steam)
							{
								modListItem.label.version = mod.SteamUpdateTimestamp;
							}
							if (mod.enabledForVanilla)
							{
								modListItem.enabledForDlc.Add("");
							}
							if (mod.enabledForSpacedOut)
							{
								modListItem.enabledForDlc.Add(DLC.SpacedOut);
							}

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
			}, null);
		}

		public void ConvertToLocal(ONIMod mod)
		{
			// Copy content to new folder
			string sourceModFolder = mod.Folder;
			string fileSafeStaticID = mod.StaticID.ToFileSafeString();
			string destModFolder = Path.Combine(GamePaths.LocalModsFolder, fileSafeStaticID);
			Directory.CreateDirectory(destModFolder);
			ShellHelper.CopyDirectory(sourceModFolder, destModFolder, true);
			
			ModConfigItem cloneInfo = new ModConfigItem()
			{
				staticID = mod.StaticID,
				label = new ModConfigLabel()
				{
					distribution_platform = DistributionPlatform.Local,
					id = fileSafeStaticID,
					title = mod.Title,
					version = 0
				},
				status = ModStatus.Installed,
				crash_count = 0,
				enabled = false,
				enabledForDlc = new List<string>()
			};
			
			AddMod(cloneInfo);
			SaveModList();
		}
	}

	public enum ModType
	{
		Steam,
		Local,
		Dev
	}
}
