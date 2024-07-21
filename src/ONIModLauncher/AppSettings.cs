using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ONIModLauncher
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AppSettings
    {
        #region Singleton Management

        private static AppSettings s_instance;

        public static AppSettings Instance => s_instance;

        private static string GetSettingsFile()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string romenFolder = Path.Combine(appDataFolder, "romen-h");
            string modLauncherFolder = Path.Combine(romenFolder, "ONIModLauncher");

            Directory.CreateDirectory(modLauncherFolder);

            return Path.Combine(modLauncherFolder, "settings.json");
        }

        public static void Init()
        {
            string settingsFile = GetSettingsFile();

			if (File.Exists(settingsFile))
			{
				try
				{
					string json = File.ReadAllText(settingsFile);
					s_instance = JsonConvert.DeserializeObject<AppSettings>(json);
				}
				catch (Exception ex)
				{ }
			}

			if (s_instance == null)
			{
				s_instance = new AppSettings();
				Save();
			}
		}

        public static void Save()
        {
            string settingsFile = GetSettingsFile();

            try
            {
                string json = JsonConvert.SerializeObject(s_instance, Formatting.Indented);
                File.WriteAllText(settingsFile, json);
            }
            catch (Exception ex)
            {
	            
            }
        }

        #endregion

        [JsonProperty]
        public string GameExecutablePath
        { get; set; } = null;

        [JsonProperty]
        public HashSet<ulong> SteamContent
        { get; set; } = new HashSet<ulong>();

        private readonly object steamContentLock = new object();

        public bool HasSteamContent(ulong modID)
        {
            lock (steamContentLock)
            {
                return SteamContent.Contains(modID);
            }
        }

        public void AddSteamContent(ulong id)
        {
            lock (steamContentLock)
            {
                SteamContent.Add(id);
            }
        }

        public void RemoveSteamContent(ulong id)
        {
            lock (steamContentLock)
            {
                SteamContent.Remove(id);
            }
        }

        public IEnumerable<ulong> GetSteamContent()
        {
            lock (steamContentLock)
            {
                return SteamContent.ToArray();
            }
        }

        private AppSettings()
        {
            
        }
    }
}
