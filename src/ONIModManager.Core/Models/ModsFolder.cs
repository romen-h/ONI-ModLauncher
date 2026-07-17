using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ONIModManager.Core.Types;

namespace ONIModManager.Core.Models
{
	public abstract class ModsFolder<TMod>
		where TMod : Mod
	{
		public string Path
		{ get; private set; }
		
		public ModDistributionPlatform DistributionPlatform
		{ get; private set ;}

		protected ModsFolder(ModDistributionPlatform platform, string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			
			DistributionPlatform = platform;
			
			Path = path;
		}
		
		public IEnumerable<TMod> LoadMods()
		{
			if (!Directory.Exists(Path)) yield break;
			
			foreach (var folder in Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly))
			{
				TMod mod = null;
				try
				{
					mod = LoadMod(folder);
				}
				catch (Exception ex)
				{
					
				}

				if (mod != null)
				{
					yield return mod;
				}
			}
		}
		
		protected abstract TMod LoadMod(string modFolder);
	}
	
	public class LocalModsFolder : ModsFolder<LocalMod>
	{
		public LocalModsFolder(string path) : base(ModDistributionPlatform.Local, path)
		{}

		protected override LocalMod LoadMod(string modFolder) => new LocalMod(modFolder);
	}
	
	public class SteamModsFolder : ModsFolder<SteamMod>
	{
		public SteamModsFolder(string path) : base(ModDistributionPlatform.Steam, path)
		{ }
		
		override protected SteamMod LoadMod(string modFolder) => new SteamMod(modFolder);
	}
	
	public class DevModsFolder : ModsFolder<DevMod>
	{
		public DevModsFolder(string path) : base(ModDistributionPlatform.Dev, path)
		{}

		protected override DevMod LoadMod(string modFolder) => new DevMod(modFolder);
	}
}
