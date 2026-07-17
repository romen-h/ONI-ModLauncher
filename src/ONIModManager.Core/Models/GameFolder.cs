using System;
using System.IO;

namespace ONIModManager.Core.Models
{
	public class GameFolder
	{
		public string FolderPath
		{ get; private set; }
		
		public string GameExecutableFile
		{ get; private set; }
		
		public GameFolder(string path)
		{
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
			if (!Directory.Exists(path)) throw new ArgumentException($"Directory '{path}' does not exist.", nameof(path));
			
			FolderPath = path;
			
			GameExecutableFile = Path.Combine(path, "OxygenNotIncluded");
		}
	}
}
