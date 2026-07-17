using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NativeFileDialogNET;
using NativeMessageBox;

namespace ONIModLauncher.Core
{
	public static class GamePaths
	{
		private static readonly Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

		private static string GetKnownFolderPath(Guid knownFolderId)
		{
			IntPtr pszPath = IntPtr.Zero;
			try
			{
				int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
				if (hr >= 0)
					return Marshal.PtrToStringAuto(pszPath);
				throw Marshal.GetExceptionForHR(hr);
			}
			finally
			{
				if (pszPath != IntPtr.Zero)
					Marshal.FreeCoTaskMem(pszPath);
			}
		}

		[DllImport("shell32.dll")]
		static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

		private const string LAUNCHER_SETTINGS_CONFIG_NAME = "settings.json";

		public const string ONI_EXE_NAME = "OxygenNotIncluded.exe";
		
		private const string ONI_DEBUG_PREFS_NAME = "settings.yml";
		private const string ONI_LAUNCH_PREFS_NAME = "kplayerprefs.yaml";
		private const string ONI_MODS_CONFIG_NAME = "mods.json";

		private const string SPACEDOUT_FLAG_FILE = "OxygenNotIncluded_Data/StreamingAssets/expansion1_bundle";

		private const string FROSTY_FLAG_FILE = "OxygenNotIncluded_Data/StreamingAssets/dlc2_bundle";

		private const string BIONIC_FLAG_FILE = "OxygenNotIncluded_Data/StreamingAssets/dlc3_bundle";

		public static string AppDataFolder
		{ get; private set; }

		public static string DownloadsFolder
		{ get; private set; }

		public static string GameExecutablePath
		{ get; private set; }

		public static string GameInstallFolder
		{ get; private set; }

		public static string DebugSettingsFile
		{ get; private set; }

		public static string GameLogFile
		{ get; private set; }

		public static bool HasSpacedOut
		{ get; private set; }

		public static bool HasFrostyPlanetPack
		{ get; private set; }

		public static bool HasBionicBoosterPack
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded
		/// </summary>
		public static string GameDocumentsFolder
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncludes/ModLauncher/settings.json
		/// </summary>
		public static string LauncherSettingsFile
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/save_files
		/// </summary>
		public static string SavesFolder
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/kplayerprefs.yaml
		/// </summary>
		public static string PlayerPrefsFile
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/mods/mods.json
		/// </summary>
		public static string ModsConfigFile
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/mods
		/// </summary>
		public static string ModsFolder
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/mods/Steam
		/// </summary>
		public static string SteamModsFolder
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/mods/local
		/// </summary>
		public static string LocalModsFolder
		{ get; private set; }

		/// <summary>
		/// Documents/Klei/OxygenNotIncluded/mods/dev
		/// </summary>
		public static string DevModsFolder
		{ get; private set; }
		
		public static string WorkshopDownloadsFolder
		{ get; private set; }

		public static bool Init()
		{
			if (SteamIntegration.Instance.UseSteam)
			{
				GameExecutablePath = SteamIntegration.Instance.ONIExecutablePath;
				AppSettings.Instance.GameExecutablePath = GameExecutablePath;
				AppSettings.Save();
			}
			else
			{
				if (AppSettings.Instance.GameExecutablePath == null || !File.Exists(AppSettings.Instance.GameExecutablePath))
				{
					ShellHelper.ShowMessageBox(
						$"Steam was not detected, so the game executable must be located manually.\nSteam Init Error:\n{SteamIntegration.Instance.InitError}",
						"Locate Oxygen Not Included Executable",
						icon: ShellHelper.MessageBoxIcons.Info);
					var filePath = ShellHelper.OpenFile("OxygenNotIncluded Executable", ONI_EXE_NAME);
					if (filePath == null) return false;
					
					if (filePath.ToLowerInvariant().Contains("steamapps"))
					{
						ShellHelper.ShowMessageBox(
							"The selected game executable exists inside a steam library.\nONI Mod Launcher should have detected this executable automatically.",
							"Warning",
							icon: ShellHelper.MessageBoxIcons.Warning);
					}
					
					AppSettings.Instance.GameExecutablePath = filePath;
					AppSettings.Save();
				}

				GameExecutablePath = AppSettings.Instance.GameExecutablePath;
			}
			
			string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			AppDataFolder = Path.Combine(appData, "ONIModLauncher");
			Directory.CreateDirectory(AppDataFolder);

			DownloadsFolder = Path.Combine(AppDataFolder, "Downloads");
			Directory.CreateDirectory(DownloadsFolder);
			
			GameInstallFolder = Path.GetDirectoryName(GameExecutablePath);
			DebugSettingsFile = Path.Combine(GameInstallFolder, "OxygenNotIncluded_Data", ONI_DEBUG_PREFS_NAME);
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				GameDocumentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Klei\\OxygenNotIncluded");
				GameLogFile = Path.Combine(GetKnownFolderPath(localLowId), "Klei/Oxygen Not Included/Player.log");
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				GameDocumentsFolder = Path.Combine(userFolder, ".config/unity3d/Klei/Oxygen Not Included");
				GameLogFile = Path.Combine(GameDocumentsFolder, "Player.log");
			}
			
			if (!Directory.Exists(GameDocumentsFolder))
			{
				ShellHelper.ShowMessageBox("Could not find Oxygen Not Included documents folder.\nMake sure that the game is installed and has been run at least once.", "Error", icon: ShellHelper.MessageBoxIcons.Error);
				return false;
			}

			string spacedOutFile = Path.Combine(GameInstallFolder, SPACEDOUT_FLAG_FILE);
			HasSpacedOut = File.Exists(spacedOutFile);

			string frostyFile = Path.Combine(GameInstallFolder, FROSTY_FLAG_FILE);
			HasFrostyPlanetPack = File.Exists(frostyFile);

			string bionicFile = Path.Combine(GameInstallFolder, BIONIC_FLAG_FILE);
			HasBionicBoosterPack = File.Exists(bionicFile);
			
			string modLauncherFolder = Path.Combine(GameDocumentsFolder, "Mod Launcher");
			Directory.CreateDirectory(modLauncherFolder);
			LauncherSettingsFile = Path.Combine(modLauncherFolder, LAUNCHER_SETTINGS_CONFIG_NAME);

			SavesFolder = Path.Combine(GameDocumentsFolder, "save_files");

			PlayerPrefsFile = Path.Combine(GameDocumentsFolder, ONI_LAUNCH_PREFS_NAME);
			ModsFolder = Path.Combine(GameDocumentsFolder, "mods");
			SteamModsFolder = Path.Combine(ModsFolder, "Steam");
			LocalModsFolder = Path.Combine(ModsFolder, "Local");
			DevModsFolder = Path.Combine(ModsFolder, "Dev");

			ModsConfigFile = Path.Combine(ModsFolder, ONI_MODS_CONFIG_NAME);
			
			string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string oniModLauncherAppData = Path.Combine(localAppData, "ONI Mod Launcher");
			Directory.CreateDirectory(oniModLauncherAppData);
			string workshopTempFolder = Path.Combine(oniModLauncherAppData, "Workshop Downloads");
			Directory.CreateDirectory(workshopTempFolder);
			
			WorkshopDownloadsFolder = workshopTempFolder;

			return true;
		}
	}
}
