using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ONIModManager.Core.Configs;
using ONIModManager.Core.Models;

namespace ONIModManager.Core
{
	public class ModManager
	{
		private static ModManager s_instance;
		
		public static ModManager Instance
		{
			get
			{
				if (s_instance == null) s_instance = new ModManager();
				return s_instance;
			}
		}
		
		public static void Init(string? gameFolder = null, string? documentsFolder = null)
		{
			s_instance = new ModManager(gameFolder, documentsFolder);
		}
		
		private KPlayerPrefsYaml? _playerPrefsYaml;
		
		private readonly Dictionary<string, Mod> _mods = new Dictionary<string, Mod>();
		
		private ModsJson? _modsJson = null;
		
		public IReadOnlyDictionary<string, Mod> Mods => _mods;
		
		public bool SpacedOutEnabled => _playerPrefsYaml?.SpacedOutEnabled ?? false;

		public ONIDocumentsFolder DocumentsFolder
		{ get; private set; }
		
		public GameFolder GameFolder
		{ get; private set; }
		
		private ModManager(string? gameFolder = null, string? documentsFolder = null)
		{
			if (gameFolder == null) gameFolder = DetectGameFolder();
			if (gameFolder != null)
			{
				GameFolder = new GameFolder(gameFolder);
			}
			
			if (documentsFolder == null) documentsFolder = DetectDocumentsFolder();
			if (documentsFolder != null)
			{
				DocumentsFolder = new ONIDocumentsFolder(documentsFolder);
				
				// Load KPlayerPrefs
				if (File.Exists(DocumentsFolder.KPlayerPrefsFile))
				{
					_playerPrefsYaml = KPlayerPrefsYaml.Deserialize(DocumentsFolder.KPlayerPrefsFile);
				}
			
				// Load mod folders
				foreach (var mod in DocumentsFolder.ModsFolder.LoadMods())
				{
					_mods[mod.StaticID] = mod;
				}
			
				// Load mods.json
				if (File.Exists(DocumentsFolder.ModsFolder.ModsJsonFile))
				{
					_modsJson = ModsJson.Deserialize(DocumentsFolder.ModsFolder.ModsJsonFile);
					foreach (var modJsonInfo in _modsJson.Mods)
					{
						Debug.Assert(modJsonInfo != null);
						if (!_mods.TryGetValue(modJsonInfo.StaticID, out var mod)) continue;
					
						mod.ApplyJsonInfo(modJsonInfo);
					}
				}
			}
		}
		
		public static string DetectDocumentsFolder()
		{
			string oniFolder;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				string kleiFolder = Path.Combine(documentsFolder, "Klei");
				oniFolder = Path.Combine(kleiFolder, "OxygenNotIncluded");
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				string configFolder = Path.Combine(homeFolder, ".config");
				string unity3dFolder = Path.Combine(configFolder, "unity3d");
				string kleiFolder = Path.Combine(unity3dFolder, "Klei");
				oniFolder = Path.Combine(kleiFolder, "Oxygen Not Included");
			}
			else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				oniFolder = Path.Combine(appDataFolder, "unity.Klei.Oxygen Not Included");
			}
			else
			{
				throw new NotImplementedException("Platform not supported: " + Environment.OSVersion.Platform);
			}
			
			return oniFolder;
		}
		
		public static string DetectGameFolder()
		{
			return null;
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				
			}
			else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
			{
				
			}
			else
			{
				throw new NotImplementedException("Platform not supported: " + Environment.OSVersion.Platform);
			}
		}
		
		public void ToggleSpacedOut(bool enabled)
		{
			if (_playerPrefsYaml == null) throw new InvalidOperationException("KPlayerPrefsYaml is not loaded.");
			_playerPrefsYaml.SpacedOutEnabled = enabled;
			KPlayerPrefsYaml.Serialize(DocumentsFolder.KPlayerPrefsFile, _playerPrefsYaml);
		}
		
		public void ToggleAllowSafeMode(bool enabled)
		{
			if (_playerPrefsYaml == null) throw new InvalidOperationException("KPlayerPrefsYaml is not loaded.");
			_playerPrefsYaml.DisableSafeMode = !enabled;
			KPlayerPrefsYaml.Serialize(DocumentsFolder.KPlayerPrefsFile, _playerPrefsYaml);
		}

		public void EnableAllMods()
		{
			foreach (var mod in _mods.Values)
			{
				
			}
		}

		public void EnableMod(string staticID, string? dlc = null)
		{
			if (string.IsNullOrWhiteSpace(staticID)) throw new ArgumentNullException(nameof(staticID));
			if (dlc == null)
			{
				if (_playerPrefsYaml == null) throw new InvalidOperationException("KPlayerPrefsYaml is not loaded.");
				dlc = _playerPrefsYaml.SpacedOutEnabled ? Constants.Dlc.SpacedOut : Constants.Dlc.Vanilla;
			}
			
			if (_mods.Count == 0) throw new InvalidOperationException("Mods are not loaded.");
			if (!_mods.TryGetValue(staticID, out var mod)) throw new KeyNotFoundException();
			
			mod.EnabledStates[dlc] = true;
		}
		
		public void DisableMod(string staticID, string? dlc = null)
		{
			if (string.IsNullOrWhiteSpace(staticID)) throw new ArgumentNullException(nameof(staticID));
			if (dlc == null)
			{
				if (_playerPrefsYaml == null) throw new InvalidOperationException("KPlayerPrefsYaml is not loaded.");
				dlc = _playerPrefsYaml.SpacedOutEnabled ? Constants.Dlc.SpacedOut : Constants.Dlc.Vanilla;
			}
			
			if (_mods.Count == 0) throw new InvalidOperationException("Mods are not loaded.");
			if (!_mods.TryGetValue(staticID, out var mod)) throw new KeyNotFoundException();
			
			mod.EnabledStates[dlc] = false;
		}
		
		public void SaveModsJson(string? filePath = null)
		{
			if (_mods.Count == 0) throw new InvalidOperationException("Mods are not loaded.");
			if (filePath == null) filePath = DocumentsFolder.ModsFolder.ModsJsonFile;
			ModsJson.SaveMods(_mods.Values, filePath);
		}
	}
}
