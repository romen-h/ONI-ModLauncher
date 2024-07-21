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
		[JsonProperty("version")]
		public int version;

		[JsonProperty("mods")]
		public ObservableCollection<ModConfigItem> mods = new ObservableCollection<ModConfigItem>();

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
		public ModConfigLabel label;
		public ModStatus status;
		public bool enabled;
		public List<string> enabledForDlc;
		public int crash_count;
		public string reinstall_path;
		public string staticID;
	}

	[JsonObject(MemberSerialization.Fields)]
	public class ModConfigLabel
	{
		public DistributionPlatform distribution_platform;
		public string id;
		public string title;
		public long version;
	}
}
