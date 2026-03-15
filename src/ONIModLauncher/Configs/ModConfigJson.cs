using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ONIModLauncher.Configs
{
	public enum DistributionPlatform
	{
		Local,
		Steam,
		Epic,
		Rail,
		Dev
	}

	public enum ModStatus
	{
		NotInstalled,
		Installed,
		UninstallPending,
		ReinstallPending
	}

	[JsonObject(MemberSerialization.Fields)]
	public class ModConfigJson
	{
		public const int CURRENT_SCHEMA_VERSION = 1;
		
		/// <summary>
		/// The mods list schema version.
		/// </summary>
		[JsonProperty("version")]
		[JsonRequired]
		public required int version = CURRENT_SCHEMA_VERSION;

		/// <summary>
		/// The list of installed mods.
		/// </summary>
		[JsonProperty("mods")]
		[JsonRequired]
		public required ObservableCollection<ModConfigItem> mods = new ObservableCollection<ModConfigItem>();

		public static ModConfigJson Load(string file)
		{
			if (!File.Exists(file)) return null;

			string json = File.ReadAllText(file);
			var list = JsonConvert.DeserializeObject<ModConfigJson>(json);
			return list;
		}

		public static void Save(ModConfigJson config, string file)
		{
			string json = JsonConvert.SerializeObject(config, Formatting.Indented);
			File.WriteAllText(file, json);
		}
	}

	[JsonObject(MemberSerialization.Fields)]
	public class ModConfigItem
	{
		/// <summary>
		/// The static ID for the mod.
		/// </summary>
		[JsonProperty("staticID")]
		[JsonRequired]
		public required string staticID;

		/// <summary>
		/// Describes platform information about the mod.
		/// </summary>
		[JsonProperty("label")]
		[JsonRequired]
		public required ModConfigLabel label;

		/// <summary>
		/// The current installation status of the mod.
		/// </summary>
		[JsonProperty("status")]
		[JsonRequired]
		public required ModStatus status;

		/// <summary>
		/// Deprecated toggle for enabling the mod in vanilla ONI.
		/// </summary>
		[JsonProperty("enabled")]
		[JsonRequired]
		public required bool enabled = false;

		/// <summary>
		/// A list of DLC ids that the mod is enabled for.
		/// </summary>
		[JsonProperty("enabledForDlc", NullValueHandling = NullValueHandling.Include)]
		public required List<string> enabledForDlc = [];

		/// <summary>
		/// The number of times the mod has been blamed for a crash.
		/// </summary>
		[JsonProperty("crash_count")]
		[JsonRequired]
		public required int crash_count = 0;

		/// <summary>
		/// Unknown purpose.
		/// </summary>
		[JsonProperty("reinstall_path", NullValueHandling = NullValueHandling.Include)]
		public string? reinstall_path = null;
	}

	[JsonObject(MemberSerialization.Fields)]
	public class ModConfigLabel
	{
		/// <summary>
		/// Describes how the mod was installed, and which mods folder it belongs in.
		/// </summary>
		[JsonProperty("distribution_platform")]
		[JsonRequired]
		public required DistributionPlatform distribution_platform;

		/// <summary>
		/// The platform ID of the mod.
		/// This is usually the folder name for the mod.
		/// </summary>
		[JsonProperty("id")]
		[JsonRequired]
		public required string id;

		/// <summary>
		/// The display name of the mod.
		/// </summary>
		[JsonProperty("title")]
		[JsonRequired]
		public required string title;

		/// <summary>
		/// A unix timestamp in seconds representing when the mod was published.
		/// </summary>
		[JsonProperty("version")]
		[JsonRequired]
		public required long version;
	}
}
