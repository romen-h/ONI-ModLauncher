using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ONIModLauncher.Configs
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModUpdateIndexJson
	{
		[JsonProperty("mods", Required = Required.Always)]
		public ModUpdateIndexItem[] Mods
		{ get; set; } = [];

		public static bool TryParse(string json, out ModUpdateIndexJson updateIndex)
		{
			updateIndex = null;
			try
			{
				updateIndex = JsonConvert.DeserializeObject<ModUpdateIndexJson>(json);
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to parse update index:");
				Debug.WriteLine(ex.ToString());
				return false;
			}
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModUpdateIndexItem
	{
		[JsonProperty("staticID", Required = Required.Always)]
		public string StaticID
		{ get; set; }

		[JsonProperty("version", Required = Required.Always)]
		public Version Version
		{ get; set; } = null;

		[JsonProperty("downloadURL", Required = Required.Always)]
		public string DownloadUrl
		{ get; set; } = null;
	}
}
