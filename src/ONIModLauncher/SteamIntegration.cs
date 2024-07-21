using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace ONIModLauncher
{
    class SteamIntegration
    {
	    public const ulong ONIAppID = 457140;

		public static readonly SteamIntegration Instance = new SteamIntegration();

		private FileSystemWatcher _workshopContentWatcher;

		private readonly ConcurrentDictionary<string, CancellationTokenSource> _waitForDownloadTaskTokens = new ConcurrentDictionary<string, CancellationTokenSource>();

		private readonly ConcurrentDictionary<ulong, string> _subscribedContent = new ConcurrentDictionary<ulong, string>();
		private readonly ConcurrentDictionary<ulong, long> _modUpdateTimestamps = new ConcurrentDictionary<ulong, long>();

		private readonly DelayedEvent _ingestModsAfterDownloadsDelay = new DelayedEvent(TimeSpan.FromSeconds(5));

		internal string InitError
		{ get; private set; }

		public bool UseSteam
		{ get; private set; } = false;

		public string SteamExecutablePath
		{ get; private set; } = null;

		public string SteamInstallFolder
		{ get; private set; } = null;

		public string ONILibraryFolder
		{ get; private set; } = null;

		public string ONIFolder
		{ get; private set; } = null;

		public string ONIExecutablePath
		{ get; private set; } = null;

		public string WorkshopManifestFile
		{ get; private set; }

		public string WorkshopContentFolder
		{ get; private set; } = null;

		private SteamIntegration()
        {
			_ingestModsAfterDownloadsDelay.DelayFinished += IngestModsAfterDownloadsDelay_DelayFinished;
		}

		public void Init()
		{
			InitError = "Unknown Error";

			// Find steam executable from registry
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam"))
				{
					if (key != null)
					{
						SteamInstallFolder = (string)key.GetValue("InstallPath");
						if (!Directory.Exists(SteamInstallFolder))
						{
							InitError = $"Steam registry key found but install folder doesn't exist: {SteamInstallFolder}";
							return;
						}
					}
					else
					{
						InitError = "Steam registry key is missing.";
						return;
					}
				}
			}
			catch (Exception ex)
			{
				InitError = "Failed to find steam install path.";
				return;
			}

			SteamExecutablePath = Path.Combine(SteamInstallFolder, "steam.exe");
			if (!File.Exists(SteamExecutablePath))
			{
				InitError = "steam.exe is missing from Steam install folder.";
				return;
			}

			// Ensure steam is actually running
			if (!IsSteamRunning())
			{
				InitError = "Steam is not running.";
				return;
			}

			// Find the steam libraries and ONI install path properly
			string primarySteamLibrary = Path.Combine(SteamInstallFolder, "steamapps");
			string libraryFoldersFile = Path.Combine(primarySteamLibrary, "libraryfolders.vdf");

			if (!File.Exists(libraryFoldersFile))
			{
				InitError = "Failed to find libraryfolders.vdf";
				return;
			}

			try
			{
				string oniAppIDStr = $"{ONIAppID}";
				dynamic libraryFoldersVdf = VdfConvert.Deserialize(File.ReadAllText(libraryFoldersFile));
				foreach (var library in libraryFoldersVdf.Value)
				{
					try
					{
						foreach (var app in library.Value.apps)
						{
							if (app.Key == oniAppIDStr)
							{
								ONILibraryFolder = library.Value.path.ToString();
								break;
							}
						}
					}
					catch
					{
						continue;
					}
					
					if (ONILibraryFolder != null) break;
				}
			}
			catch
			{
				InitError = "Failed to parse libraryfolders.vdf";
				return;
			}

			if (ONILibraryFolder == null)
			{
				InitError = "Could not find Oxygen Not Included app in any Steam libraries.";
				return;
			}

			if (!Directory.Exists(ONILibraryFolder))
			{
				InitError = $"Steam library folder for Oxygen Not Included doesn't exist: {ONILibraryFolder}";
				return;
			}

			string steamAppsFolder = Path.Combine(ONILibraryFolder, "steamapps");
			string steamAppsCommonFolder = Path.Combine(steamAppsFolder, "common");
			ONIFolder = Path.Combine(steamAppsCommonFolder, "OxygenNotIncluded");
			if (!Directory.Exists(ONIFolder))
			{
				InitError = $"Oxygen Not Included install folder doesn't exist: {ONIFolder}";
				return;
			}

			ONIExecutablePath = Path.Combine(ONIFolder, "OxygenNotIncluded.exe");
			if (!File.Exists(ONIExecutablePath))
			{
				InitError = $"OxygenNotIncluded executable doesn't exist: {ONIExecutablePath}";
				return;
			}

			UseSteam = true;

			string workshopFolder = Path.Combine(steamAppsFolder, "workshop");
			WorkshopManifestFile = Path.Combine(workshopFolder, $"appworkshop_{ONIAppID}.acf");
			string contentFolder = Path.Combine(workshopFolder, "content");
			WorkshopContentFolder = Path.Combine(contentFolder, $"{ONIAppID}");

			if (Directory.Exists(WorkshopContentFolder))
			{
				foreach (var folder in Directory.GetDirectories(WorkshopContentFolder))
				{
					string folderName = Path.GetFileName(folder);
					if (ulong.TryParse(folderName, out ulong id))
					{
						AppSettings.Instance.AddSteamContent(id);
					}
				}
			}

			ParseWorkshopManifest();

			// TODO: This whole system is currently dysfunctional and bad bad bad
			//InitContentWatcher();
		}

		public bool IsSteamRunning()
		{
			var processes = Process.GetProcessesByName("steam");
			return processes != null && processes.Length > 0;
		}

		public void LaunchGame()
        {
	        var startInfo = new ProcessStartInfo()
	        {
		        FileName = SteamExecutablePath,
		        Arguments = $"-applaunch {ONIAppID}",
		        WindowStyle = ProcessWindowStyle.Hidden,
		        UseShellExecute = false,
		        CreateNoWindow = true
	        };

	        Process.Start(startInfo);
		}

		public void AddManagedContentID(ulong id)
		{
			AppSettings.Instance.AddSteamContent(id);
			DownloadWorkshopContent(id);
		}

		public void UpdateMods(bool forceRedownload = false)
		{
			try
			{
				File.Delete(WorkshopManifestFile);
				Directory.Delete(WorkshopContentFolder, true);
				ValidateGameFiles();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to update mods.");
				Debug.WriteLine(ex.ToString());
			}
#if false
			foreach (ulong id in AppSettings.Instance.GetSteamContent())
			{
				try
				{
					if (forceRedownload)
					{
						DeleteWorkshopContent(id);
					}
					DownloadWorkshopContent(id);
					_ingestModsAfterDownloadsDelay.Start(); // Ensures that we won't bother until this loop is done
					Thread.Sleep(250);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.ToString());
				}
			}
#endif
		}

		private void DownloadWorkshopContent(ulong id)
		{
			var startInfo = new ProcessStartInfo()
			{
				FileName = SteamExecutablePath,
				Arguments = $"+workshop_download_item {ONIAppID} {id}",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process.Start(startInfo);
		}

		public void ValidateGameFiles()
		{
			var startInfo = new ProcessStartInfo()
			{
				FileName = SteamExecutablePath,
				Arguments = $"+app_start_validation {ONIAppID}",
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			Process.Start(startInfo);
		}

		private void DeleteWorkshopContent(ulong id)
		{
			string modFolder = Path.Combine(WorkshopContentFolder, id.ToString());
			if (Directory.Exists(modFolder))
			{
				Directory.Delete(modFolder, true);
			}
		}

		private void InitContentWatcher()
		{
			_workshopContentWatcher = new FileSystemWatcher(WorkshopContentFolder)
			{
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
				EnableRaisingEvents = true
			};

			_workshopContentWatcher.Created += WorkshopContentWatcher_Created;
			_workshopContentWatcher.Changed += WorkshopContentWatcher_Changed;
			_workshopContentWatcher.Deleted += WorkshopContentWatcher_Deleted;
		}

		private void WorkshopContentWatcher_Created(object sender, FileSystemEventArgs e)
		{
			
		}

		private void WorkshopContentWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			
		}

		private void WorkshopContentWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.FullPath.EndsWith("_legacy.bin"))
			{
				string dir = Path.GetDirectoryName(e.FullPath);
				string dirName = Path.GetFileName(dir);

				if (ulong.TryParse(dirName, out ulong id))
				{
					if (_waitForDownloadTaskTokens.TryGetValue(e.FullPath, out CancellationTokenSource tokenSrc))
					{
						Trace.WriteLine($"Cancelling existing handler for {e.FullPath}");
						tokenSrc.Cancel();
					}

					Trace.WriteLine($"Starting handler for {e.FullPath}");
					CancellationTokenSource newTokenSrc = new CancellationTokenSource();
					_waitForDownloadTaskTokens[e.FullPath] = newTokenSrc;
					Task.Delay(5000, newTokenSrc.Token).ContinueWith((state) =>
					{
						_waitForDownloadTaskTokens.TryRemove(e.FullPath, out _);
						IngestWorkshopContent(id, e.FullPath);
					}, TaskContinuationOptions.OnlyOnRanToCompletion);
				}
			}
		}

		

		private void IngestWorkshopContent(ulong id, string file)
		{
			Trace.WriteLine($"Time to handle {file} for mod {id}!");
			if (AppSettings.Instance.HasSteamContent(id))
			{
				Trace.WriteLine("Detected mod update.");
			}
			else
			{
				Trace.WriteLine("Detected new mod.");
				AppSettings.Instance.AddSteamContent(id);
			}

			//_ingestModsAfterDownloadsDelay.Start();
		}

		private readonly object _ingestModsLock = new object();

		private void IngestModsAfterDownloadsDelay_DelayFinished(object sender, EventArgs e)
		{
			if (Monitor.TryEnter(_ingestModsLock))
			{
				try
				{
					Debug.WriteLine("Refreshing mod version timestamps...");
					ParseWorkshopManifest();

					Debug.WriteLine("Extracting mod files...");
					foreach (ulong id in AppSettings.Instance.GetSteamContent())
					{
						try
						{
							InstallMod(id);
						}
						catch (Exception ex)
						{
							Debug.WriteLine(ex.ToString());
						}
					}

					Debug.WriteLine("Rebuilding mod list...");
					ModManager.Instance.RebuildModList();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Failed to ingest mods.");
					Debug.WriteLine(ex);
				}
				finally
				{
					Monitor.Exit(_ingestModsLock);
				}
			}
			else
			{
				Debug.WriteLine("Something else tried to start ingesting mods before the current pass was done.");
			}
		}

		public void ParseWorkshopManifest()
		{
			try
			{
				_subscribedContent.Clear();

				dynamic appWorkshop = VdfConvert.Deserialize(File.ReadAllText(WorkshopManifestFile));

				foreach (var content in appWorkshop.Value.WorkshopItemsInstalled)
				{
					try
					{
						string idStr = content.Key;
						ulong id = ulong.Parse(idStr);
						string timeStr = content.Value.timeupdated.ToString();
						long time = long.Parse(timeStr);

						_subscribedContent[id] = string.Empty;
						_modUpdateTimestamps[id] = time;
					}
					catch
					{
						continue;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		public long GetModUpdateTime(ulong id)
		{
			if (_modUpdateTimestamps.TryGetValue(id, out long time))
			{
				return time;
			}

			return 0;
		}

		private void InstallMod(ulong id)
		{
			string archiveFile = GetContentFile(id);
			if (File.Exists(archiveFile))
			{
				// TODO: back up config files to keep user settings

				string modFolder = Path.Combine(GamePaths.SteamModsFolder, id.ToString());
				ZipFile.ExtractToDirectory(archiveFile, modFolder, overwriteFiles: true);
			}
		}

		private string GetContentFile(ulong id)
		{
			string contentIDFolder = Path.Combine(WorkshopContentFolder, id.ToString());
			if (Directory.Exists(contentIDFolder))
			{
				string newestFile = null;
				DateTime newestFileDate = DateTime.MinValue;

				foreach (string file in Directory.GetFiles(contentIDFolder, "*_legacy.bin"))
				{
					FileInfo fileInfo = new FileInfo(file);
					if (fileInfo.CreationTimeUtc > newestFileDate)
					{
						newestFile = file;
						newestFileDate = fileInfo.CreationTimeUtc;
					}
				}
				
				return newestFile;
			}

			return null;
		}
	}
}
