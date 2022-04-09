using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ONIModLauncher
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

		private const string STEAM_EXE_NAME = "steam.exe";
		public const string STEAM_APP_ID = "457140";

		public const string ONI_EXE_NAME = "OxygenNotIncluded.exe";
		
		private const string ONI_DEBUG_PREFS_NAME = "settings.yml";
		private const string ONI_LAUNCH_PREFS_NAME = "kplayerprefs.yaml";
		private const string ONI_MODS_CONFIG_NAME = "mods.json";

		private const string DLC1_FLAG_FILE = "OxygenNotIncluded_Data\\StreamingAssets\\expansion1_bundle";

		public static string GameExecutablePath
		{ get; private set; }

		public static string GameInstallFolder
		{ get; private set; }

		public static string DebugSettingsFile
		{ get; private set; }

		public static string GameLogFile
		{ get; private set; }

		public static bool HasDLC1
		{ get; private set; }

		public static string GameDocumentsFolder
		{ get; private set; }

		public static string SavesFolder
		{ get; private set; }

		public static string PlayerPrefsFile
		{ get; private set; }

		public static string ModsConfigFile
		{ get; private set; }

		public static string ModsFolder
		{ get; private set; }

		public static string SteamModsFolder
		{ get; private set; }

		public static string LocalModsFolder
		{ get; private set; }

		public static string DevModsFolder
		{ get; private set; }

		public static bool UseSteam
		{ get; private set; }

		public static string SteamExecutablePath
		{ get; private set; }

		public static bool Init()
		{
			if (Settings.Default.GameExePath == null || !File.Exists(Settings.Default.GameExePath))
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.Title = $"Locate {ONI_EXE_NAME}";
				dlg.Filter = $"{ONI_EXE_NAME}|{ONI_EXE_NAME}";
				if (dlg.ShowDialog() == true)
				{
					Settings.Default.GameExePath = dlg.FileName;
					Settings.Default.Save();
				}
			}

			GameExecutablePath = Settings.Default.GameExePath;
			GameInstallFolder = Path.GetDirectoryName(GameExecutablePath);
			DebugSettingsFile = Path.Combine(GameInstallFolder, ONI_DEBUG_PREFS_NAME);

			GameLogFile = Path.Combine(GetKnownFolderPath(localLowId), "Klei/Oxygen Not Included/Player.log"); 

			string dlc1BundleFile = Path.Combine(GameInstallFolder, DLC1_FLAG_FILE);
			HasDLC1 = File.Exists(dlc1BundleFile);

			GameDocumentsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Klei\\OxygenNotIncluded");
			if (!Directory.Exists(GameDocumentsFolder))
			{
				MessageBox.Show("Could not find Oxygen Not Included documents folder.\nMake sure that the game is installed and has been run at least once.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			SavesFolder = Path.Combine(GameDocumentsFolder, "save_files");

			PlayerPrefsFile = Path.Combine(GameDocumentsFolder, ONI_LAUNCH_PREFS_NAME);
			ModsFolder = Path.Combine(GameDocumentsFolder, "mods");
			SteamModsFolder = Path.Combine(ModsFolder, "Steam");
			LocalModsFolder = Path.Combine(ModsFolder, "local");
			DevModsFolder = Path.Combine(ModsFolder, "dev");

			ModsConfigFile = Path.Combine(ModsFolder, ONI_MODS_CONFIG_NAME);

			if (GameExecutablePath.ToLower().Contains("steamapps"))
			{
				UseSteam = true;
				if (!FindSteamExecutable())
				{
					MessageBox.Show("Failed to find steam.exe", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return false;
				}
			}

			return true;
		}

		private static bool FindSteamExecutable()
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Valve\\Steam"))
				{
					if (key != null)
					{
						string steamPath = (string)key.GetValue("InstallPath");
						SteamExecutablePath = Path.Combine(steamPath, STEAM_EXE_NAME);
						return true;
					}
				}
			}
			catch
			{ }

			return false;
		}
	}
}
