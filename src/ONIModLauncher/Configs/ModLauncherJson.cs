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
	public class ModLauncherJson : ConfigBase
	{
		/// <summary>
		/// The author(s) that created the mod.
		/// </summary>
		[JsonProperty]
		public string Author
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to some website associated with the author. (i.e. Patreon, portfolio, Steam profile)
		/// </summary>
		[JsonProperty]
		public string AuthorURL
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to this mod's source code repository.
		/// </summary>
		[JsonProperty]
		public string RepoURL
		{ get; set; } = null;

		/// <summary>
		/// A URL that leads to a website where bugs can be reported.
		/// </summary>
		[JsonProperty]
		public string BugReportURL
		{ get; set; } = null;

		/// <summary>
		/// A list of config files for this mod.
		/// The paths can be relative to the mod .DLL or absolute.
		/// </summary>
		[JsonProperty]
		public Dictionary<string, string> ConfigFiles
		{ get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// Determines whether Mod Launcher should search for config files and show them when the ConfigFiles list is empty.
		/// </summary>
		[JsonProperty]
		public bool AutoDetectConfigs
		{ get; set; } = true;

		/// <summary>
		/// An object that defines rules for the mod to be loaded in the correct position on the mod list.
		/// </summary>
		[JsonProperty]
		public LoadOrderRules Rules
		{ get; set; } = new LoadOrderRules();

		/// <summary>
		/// A list of any arbitrary strings that can be associated with this mod.
		/// Currently unused by Mod Launcher.
		/// </summary>
		[JsonProperty]
		public string[] Tags
		{ get; set; } = new string[0];

		public ModLauncherJson()
		{ }

		public static ModLauncherJson Load(string file)
		{
			if (!File.Exists(file)) return new ModLauncherJson();

			string json = File.ReadAllText(file);

			var s = JsonConvert.DeserializeObject<ModLauncherJson>(json);

			return s;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class LoadOrderRules
	{
		/// <summary>
		/// A list of mods that must be enabled for this mod to work correctly.
		/// </summary>
		[JsonProperty]
		public string[] Dependencies
		{ get; set; } = new string[0];

		/// <summary>
		/// A list of mods that must not be enabled for this mod to work correctly.
		/// </summary>
		[JsonProperty]
		public string[] AntiDependencies
		{ get; set; } = new string[0];

		/// <summary>
		/// A list of mods that must be before this mod in the load order.
		/// </summary>
		[JsonProperty]
		public string[] LoadBefore
		{ get; set; } = new string[0];

		/// <summary>
		/// A list of mods that must be after this mod in the load order.
		/// </summary>
		[JsonProperty]
		public string[] LoadAfter
		{ get; set; } = new string[0];

		public LoadOrderRules()
		{ }
	}
}
