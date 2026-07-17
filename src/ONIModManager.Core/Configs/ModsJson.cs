using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ONIModManager.Core.Models;
using ONIModManager.Core.Types;

namespace ONIModManager.Core.Configs
{
	public class ModsJson
	{
		public const int CurrentVersion = 1;
		
		private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true
		};

		public static ModsJson Deserialize(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
			if (!File.Exists(filePath)) throw new FileNotFoundException("mods.json file not found.", filePath);
			
			string json = File.ReadAllText(filePath, Encoding.UTF8);
			var modsList = JsonSerializer.Deserialize<ModsJson>(json, s_serializerOptions);
			if (modsList == null) throw new Exception("Deserialized instance is null.");
			return modsList;
		}
		
		public static void Serialize(ModsJson modsList, string modsFile)
		{
			if (modsList == null) throw new ArgumentNullException(nameof(modsList));
			if (string.IsNullOrWhiteSpace(modsFile)) throw new ArgumentNullException(nameof(modsFile));
			
			string json = JsonSerializer.Serialize(modsList, s_serializerOptions);
			File.WriteAllText(modsFile, json, Encoding.UTF8);
		}
		
		public static void SaveMods(IEnumerable<Mod> mods, string modsFile)
		{
			ModsJson modsList = new ModsJson()
			{
				Version = CurrentVersion,
				Mods = new List<ONIModsJson_Mod>()
			};
			
			foreach (var mod in mods)
			{
				modsList.Mods.Add(ONIModsJson_Mod.FromMod(mod));
			}
			
			Serialize(modsList, modsFile);
		}
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("version")]
		public int Version
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("mods")]
		public List<ONIModsJson_Mod> Mods
		{ get; private set; }
	}
	
	public class ONIModsJson_Mod
	{
		public static ONIModsJson_Mod FromMod(Mod mod)
		{
			return new ONIModsJson_Mod()
			{
				Label = ONIModsJson_Mod_Label.FromMod(mod),
				Status = 1, // idk what 1 means but most mods are on it
				Enabled = mod.EnabledStates.TryGetValue("", out bool enabled) ? enabled : false,
				EnabledForDlc = mod.EnabledStates.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray(),
				CrashCount = mod.CrashCount,
				ReinstallPath = null,
				StaticID = mod.StaticID
			};
		}
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("label")]
		public ONIModsJson_Mod_Label Label
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("status")]
		public int Status
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("enabled")]
		public bool Enabled
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("enabledForDlc")]
		public string[] EnabledForDlc
		{ get; private set; }

		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("crash_count")]
		public int CrashCount
		{ get; private set; }

		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("reinstall_path")]
		public string ReinstallPath
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("staticID")]
		public string StaticID
		{ get; private set; }
	}
	
	public class ONIModsJson_Mod_Label
	{
		public static ONIModsJson_Mod_Label FromMod(Mod mod)
		{
			ONIModsJson_Mod_Label label = new ONIModsJson_Mod_Label();
			
			label.Title = mod.Title;
			
			switch (mod)
			{
				case SteamMod steamMod:
					label.DistributionPlatform = ModDistributionPlatform.Steam;
					label.ID = steamMod.WorkshopID.ToString();
					label.Version = steamMod.WorkshopTimestamp;
					break;
				
				case LocalMod _:
				case DevMod _:
					string folderName = Path.GetFileName(mod.InstallPath);
					label.DistributionPlatform = ModDistributionPlatform.Local;
					label.ID = folderName;
					label.Version = folderName.GetHashCode();
					break;
				
				default:
					throw new NotImplementedException("Unsupported mod type: " + mod.GetType().Name);
			}
			
			return label;
		}
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("distribution_platform")]
		public ModDistributionPlatform DistributionPlatform
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("id")]
		public string ID
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("title")]
		public string Title
		{ get; private set; }
		
		[JsonRequired]
		[JsonInclude]
		[JsonPropertyName("version")]
		public long Version
		{ get; private set; }
	}
}
