using System;
using System.IO;

namespace ONIModManager.Core.Models
{
	public class ONIDocumentsFolder
	{
		public string FolderPath
		{ get; private set; }
		
		public string PlayerLogFile
		{ get; private set; }
		
		public string KPlayerPrefsFile
		{ get; private set; }
		
		public RootModsFolder ModsFolder
		{ get; private set; }
		
		public ONIDocumentsFolder(string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			if (!Directory.Exists(path)) throw new ArgumentException($"Directory '{path}' does not exist.", nameof(path));
			
			FolderPath = path;
			
			PlayerLogFile = Path.Combine(path, "Player.log");
			
			KPlayerPrefsFile = Path.Combine(path, "kplayerprefs.yaml");
			
			ModsFolder = new RootModsFolder(Path.Combine(path, "mods"));
		}
	}
}
