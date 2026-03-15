using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ONIModLauncher.Common.Configs;

namespace ONIModLauncher.Configs
{
	public static class LauncherMetadataUtils
	{
		private static readonly JsonSerializerOptions s_serializerSettings = new JsonSerializerOptions()
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
		};
		
		public static LauncherMetadataJson Load(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Debug.WriteLine($"Failed to load json file:\n{filePath} does not exist.");
				return null;
			}

			try
			{
				string json = File.ReadAllText(filePath, Encoding.UTF8);
				LauncherMetadataJson instance = JsonSerializer.Deserialize<LauncherMetadataJson>(json, s_serializerSettings);
				Debug.WriteLine($"Loaded config: {filePath}");
				return instance;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load json file:");
				Debug.WriteLine(filePath);
				Debug.WriteLine(ex.ToString());
				return null;
			}
		}
		
		public static LauncherMetadataJson Parse(string json)
		{
			if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));
			
			try
			{
				LauncherMetadataJson instance = JsonSerializer.Deserialize<LauncherMetadataJson>(json, s_serializerSettings);
				return instance;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to parse metadata json:");
				Debug.WriteLine(ex.ToString());
				return null;
			}
		}
	}
}
