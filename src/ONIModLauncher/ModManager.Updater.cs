using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Automation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ONIModLauncher.Configs;

using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace ONIModLauncher
{
	public partial class ModManager
	{
		HttpClient _httpClient = new HttpClient();

		private readonly ConcurrentDictionary<string, ModUpdateIndexJson> _updateIndexCache = new ConcurrentDictionary<string, ModUpdateIndexJson>();

		public async Task<(bool,string)> CheckForUpdate(ONIMod mod)
		{
			if (mod == null) throw new ArgumentNullException(nameof(mod));

			string? updateIndexUrl = mod.LauncherData?.Updates?.UpdateIndexUrl;
			if (string.IsNullOrEmpty(updateIndexUrl)) return (false,null);

			ModUpdateIndexJson updateIndex;
			try
			{
				string indexJson = await _httpClient.GetStringAsync(updateIndexUrl);
				updateIndex = JsonConvert.DeserializeObject<ModUpdateIndexJson>(indexJson);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to get update index for mod:");
				Debug.WriteLine(mod.UniqueKey);
				Debug.WriteLine(ex.ToString());
				return (false,null);
			}
			
			foreach (var indexMod in updateIndex.Mods)
			{
				if (indexMod.StaticID != mod.StaticID) continue;
				
				if (mod.Version >= indexMod.Version) return (false,null);
				
				if (string.IsNullOrWhiteSpace(indexMod.DownloadUrl))
				{
					Debug.WriteLine("Failed to get update download URL for mod:");
					Debug.WriteLine(mod.UniqueKey);
					Debug.WriteLine("Update index has newer version but URL is empty.");
					return (false,null);
				}
				
				return (true,indexMod.DownloadUrl);
			}
			
			Debug.WriteLine("Failed to check for update for mod:");
			Debug.WriteLine(mod.UniqueKey);
			Debug.WriteLine("Update index does not contain this mod's static ID.");
			return (false,null);
		}

		public async Task InstallModFromURL(string zipUrl, string modFolder, string modId = null, string subfolderName = null)
		{
			string modFolderName = Path.GetFileName(modFolder);
			
			// Prepare temp file paths
			string fileName = $"DownloadedMod_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
			string downloadPath = Path.Combine(GamePaths.DownloadsFolder, fileName);
			if (File.Exists(downloadPath))
			{
				File.Delete(downloadPath);
			}
			string tempUnzipFolder = Path.Combine(GamePaths.AppDataFolder, "Temp");
			if (Directory.Exists(tempUnzipFolder))
			{
				Directory.Delete(tempUnzipFolder, true);
			}
			Directory.CreateDirectory(tempUnzipFolder);

			// Download the mod
			await DownloadZip(zipUrl, downloadPath);
			
			// Unzip the mod to a temp folder
			try
			{
				Unzip(downloadPath, tempUnzipFolder);
			}
			catch
			{
				Cleanup(downloadPath, tempUnzipFolder);
				throw;
			}
			
			if (subfolderName != null)
			{
				tempUnzipFolder = Path.Combine(tempUnzipFolder, subfolderName);
			}
			
			// Parse the mod.yaml file
			string modFile = Path.Combine(tempUnzipFolder,  "mod.yaml");
			if (!File.Exists(modFile))
			{
				Cleanup(downloadPath, tempUnzipFolder);
				throw new Exception("Mod does not contain a mod.yaml file.");
			}
			ModYaml modYaml = ModYaml.Load(modFile);
			
			if (modId != null && modId != modYaml.staticID)
			{
				Cleanup(downloadPath, tempUnzipFolder);
				throw new Exception($"Downloaded mod's static ID does not match the expected static ID. (Expected: {modId}, Downloaded: {modYaml.staticID})");
			}
			modId = modYaml.staticID;
			
			// TODO: If mod has a metadata json then preserve the named files 
			
			// Clear the existing installed mod folder
			if (Directory.Exists(modFolder))
			{
				Directory.Delete(modFolder, true);
			}
			Directory.CreateDirectory(modFolder);
			
			// Copy the temp unzipped files to the installed mod folder
			ShellHelper.CopyDirectory(tempUnzipFolder, modFolder, true);
			
			// TODO: Restore preserved files
			
			Cleanup(downloadPath, tempUnzipFolder);
			
			// Re-add mod to list
			
			var mod = FindMod(modId);
			if (mod != null)
			{
				RemoveMod(mod);
			}

			ModConfigItem modConfig = new ModConfigItem()
			{
				label = new ModConfigLabel()
				{
					distribution_platform = DistributionPlatform.Local,
					id = modFolderName,
					title = modYaml.title,
					version = modFolderName.GetHashCode()
				},
				status = ModStatus.Installed,
				enabled = false,
				enabledForDlc = new List<string>(),
				crash_count = 0,
				reinstall_path = null,
				staticID = modId
			};

			ctx.Post((state) =>
			{
				AddMod(modConfig);
			}, null);
		}
		
		public async Task DownloadZip(string zipUrl, string destination)
		{
			var stream = await _httpClient.GetStreamAsync(zipUrl);
			await using FileStream fs = new FileStream(destination, FileMode.CreateNew, FileAccess.Write);
			await stream.CopyToAsync(fs);
		}
		
		public void Unzip(string zipPath, string destination)
		{
			if (!System.IO.File.Exists(zipPath)) throw new FileNotFoundException($"The specified zip file does not exist: {zipPath}");
			if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

			System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, destination, true);
		}
		
		private void Cleanup(string downloadedFile, string unzippedFolder)
		{
			if (File.Exists(downloadedFile))
			{
				File.Delete(downloadedFile);
			}
			
			if (Directory.Exists(unzippedFolder))
			{
				Directory.Delete(unzippedFolder, true);
			}
		}
	}
}
