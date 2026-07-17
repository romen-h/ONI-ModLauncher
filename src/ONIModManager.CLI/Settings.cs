using System.Text.Json;
using System.Text.Json.Serialization;

namespace ONIModManager.CLI
{
	public class Settings
	{
		private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions()
		{
			WriteIndented = true
		};
		
		private static Settings s_instance = null;
		
		public static Settings Instance => s_instance;
		
		public static void Load()
		{
			string json = File.ReadAllText("settings.json", System.Text.Encoding.UTF8);
			s_instance = JsonSerializer.Deserialize<Settings>(json, s_serializerOptions);
		}
		
		public static void Save()
		{
			string json = JsonSerializer.Serialize(s_instance, s_serializerOptions);
			File.WriteAllText("settings.json", json, System.Text.Encoding.UTF8);
		}
		
		[JsonInclude]
		public PathSettings Paths
		{ get; set; } = new PathSettings();	
	}
	
	public class PathSettings
	{
		[JsonInclude]
		public string? GameFolder
		{ get; set; }
		
		[JsonInclude]
		public string? ONIDocumentsFolder
		{ get; set; }
	}
}
