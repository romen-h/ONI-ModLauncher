using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ONIModManager.Core.Configs;
using ONIModManager.Core.Types;

namespace ONIModManager.Core.Models
{
	public abstract class Mod
	{
		/// <summary>
		/// The path that the mod is installed to.
		/// </summary>
		public string InstallPath
		{ get; private set; }
		
		public string ModYamlFile
		{ get; private set; }
		
		public ModYaml? ModYaml
		{ get; private set; }
		
		public string ModInfoYamlFile
		{ get; private set; }
		
		/// <summary>
		/// The mods subfolder that the mod is installed in.
		/// </summary>
		public ModDistributionPlatform PlatformFolder
		{ get; private set; }
		
		/// <summary>
		/// The unique string identifying the mod on its respective platform.
		/// </summary>
		public abstract string PlatformID
		{ get; }
		
		/// <summary>
		/// The static ID of the mod from its mod.yaml.
		/// </summary>
		public string StaticID
		{ get; protected set; }
		
		/// <summary>
		/// The display title of the mod from its mod.yaml.
		/// </summary>
		public string? Title => ModYaml?.Title;
		
		/// <summary>
		/// The description of the mod from its mod.yaml.
		/// </summary>
		public string? Description => ModYaml?.Description;
		
		/// <summary>
		/// The version number of the mod.
		/// </summary>
		public Version Version
		{ get; private set; }
		
		/// <summary>
		/// Indicates whether the mod is enabled for a specific DLC.
		/// </summary>
		public Dictionary<string, bool> EnabledStates
		{ get; private set; } = new Dictionary<string, bool>();
		
		public bool EnabledForCurrentDlc => ModManager.Instance.SpacedOutEnabled ? EnabledStates[Constants.Dlc.SpacedOut] : EnabledStates[Constants.Dlc.Vanilla];
		
		/// <summary>
		/// Tracks the number of times a mod has crashed.
		/// </summary>
		public int CrashCount
		{ get; private set; }
		
		protected Mod(string folderPath, ModDistributionPlatform platform)
		{
			if (string.IsNullOrEmpty(folderPath)) throw new ArgumentNullException(nameof(folderPath));
			if (!Directory.Exists(folderPath)) throw new ArgumentException($"Directory '{folderPath}' does not exist.", nameof(folderPath));
			
			foreach (var dlc in Constants.Dlc.ToggleableDlcIds)
			{
				EnabledStates[dlc] = false;
			}
			
			InstallPath = folderPath;
			
			string modYamlFile = Path.Combine(folderPath, "mod.yaml");
			if (File.Exists(modYamlFile))
			{
				ModYamlFile = modYamlFile;
				ModYaml = ModYaml.Deserialize(ModYamlFile);
				Debug.Assert(ModYaml != null);
				StaticID = ModYaml.StaticID;
			}
			else
			{
				StaticID = Path.GetFileName(folderPath) + "." + platform.ToString();
			}
			
			PlatformFolder = platform;
		}
		
		internal virtual void ApplyJsonInfo(ONIModsJson_Mod info)
		{
			EnabledStates[""] = info.Enabled;
			if (info.EnabledForDlc != null)
			{
				foreach (var dlc in info.EnabledForDlc)
				{
					EnabledStates[dlc] = true;
				}
			}
			CrashCount = info.CrashCount;
		}
	}
	
	public class LocalMod : Mod
	{
		public override string PlatformID => Path.GetFileName(InstallPath);

		public LocalMod(string folderPath) : base(folderPath, ModDistributionPlatform.Local)
		{ }
	}
	
	public class SteamMod : Mod
	{
		public override string PlatformID => WorkshopID.ToString();
		
		/// <summary>
		/// The steam workshop ID for the mod.
		/// </summary>
		public ulong WorkshopID
		{ get; private set; }
		
		/// <summary>
		/// The unix timestamp of when the mod was last updated.
		/// </summary>
		public long WorkshopTimestamp
		{ get; private set; }
		
		public SteamMod(string folderPath)  : base(folderPath, ModDistributionPlatform.Steam)
		{
			WorkshopID = ulong.Parse(Path.GetFileName(folderPath));
		}
		
		override internal void ApplyJsonInfo(ONIModsJson_Mod info)
		{
			base.ApplyJsonInfo(info);
			WorkshopTimestamp = info.Label.Version;
		}
	}
	
	public class DevMod : Mod
	{
		public override string PlatformID => Path.GetFileName(InstallPath);
		
		public DevMod(string folderPath)  : base(folderPath, ModDistributionPlatform.Dev)
		{ }
	}
}
