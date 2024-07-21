using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ONIModLauncher.Configs
{
	[JsonObject(MemberSerialization.OptIn)]
	public class LauncherMetadataJson : ConfigBase
	{
		/// <summary>
		/// The author(s) that created the mod.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string Author
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to some website associated with the author. (i.e. Patreon, portfolio, Steam profile)
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string AuthorURL
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to this mod's source code repository.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string RepoURL
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to a website where bugs can be reported.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public string BugReportURL
		{ get; set; } = null;

		/// <summary>
		/// A list of config files for this mod.
		/// The paths can be relative to the mod .DLL or absolute.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public Dictionary<string, string> ConfigFiles
		{ get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// A list of any arbitrary strings that can be associated with this mod.
		/// Currently unused by Mod Launcher.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public List<string> Tags
		{ get; set; } = new List<string>();

		public LauncherMetadataJson()
		{ }

		public static LauncherMetadataJson Load(string file)
		{
			if (!File.Exists(file)) return new LauncherMetadataJson();

			string json = File.ReadAllText(file);

			var s = JsonConvert.DeserializeObject<LauncherMetadataJson>(json);

			return s;
		}
	}
}
