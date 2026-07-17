using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ONIModManager.Core.Models;

namespace ONIModManager.Core
{
	public class Tasks
	{
		public static async Task InstallZipMod(string zipPath, string folderName)
		{
			var localMods = ModManager.Instance.DocumentsFolder.ModsFolder.LocalModsFolder;
			string destination = Path.Combine(localMods.Path, folderName);
			Directory.CreateDirectory(destination);
			
			using (var zip = ZipFile.OpenRead(zipPath))
			{
				foreach (var entry in zip.Entries)
				{
					string destinationPath = Path.Combine(destination, entry.FullName);
					entry.ExtractToFile(destinationPath, true);
				}
			}
		}
		
		public static async Task DeleteMod(string modFolder)
		{
			Directory.Delete(modFolder, true);
			ModManager.Instance.DocumentsFolder.ModsFolder.LoadMods();
		}
		
		public static async Task EnableMod(string staticID)
		{
			
		}
	}
}
