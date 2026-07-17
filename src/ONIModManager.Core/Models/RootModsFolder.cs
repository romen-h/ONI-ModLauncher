using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ONIModManager.Core.Configs;

namespace ONIModManager.Core.Models
{
	public class RootModsFolder
	{
		public string FolderPath
		{ get; private set; }
		
		public string ModsJsonFile
		{ get; private set;}
		
		public string ModSettingsFolder
		{ get; private set; }
		
		public LocalModsFolder LocalModsFolder
		{ get; private set; }
		
		public SteamModsFolder SteamModsFolder
		{ get; private set; }
		
		public DevModsFolder DevModsFolder
		{ get; private set; }

		public RootModsFolder(string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			if (!Directory.Exists(path)) throw new ArgumentException($"Directory '{path}' does not exist.", nameof(path));
			
			FolderPath = path;
			
			ModsJsonFile = Path.Combine(path, "mods.json");
			
			ModSettingsFolder = Path.Combine(path, "config");
			
			string localFolder = Path.Combine(path, "Local");
			LocalModsFolder = new LocalModsFolder(localFolder);
			
			string steamFolder = Path.Combine(path, "Steam");
			SteamModsFolder = new SteamModsFolder(steamFolder);
			
			string devFolder = Path.Combine(path, "Dev");
			DevModsFolder = new DevModsFolder(devFolder);
		}
		
		public IEnumerable<Mod> LoadMods()
		{
			foreach (var mod in LocalModsFolder.LoadMods()) yield return mod;
			foreach (var mod in SteamModsFolder.LoadMods()) yield return mod;
			foreach (var mod in DevModsFolder.LoadMods()) yield return mod;
		}
	}
}
