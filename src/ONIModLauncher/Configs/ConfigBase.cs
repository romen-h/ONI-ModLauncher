using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace ONIModLauncher.Configs
{
	public abstract class ConfigBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected void InvokePropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
	
	public abstract class YamlConfig<TConfig> : ConfigBase
		where TConfig : class
	{
		private static readonly UTF8Encoding s_encoding = new UTF8Encoding(false);

		public static bool Save(string filePath, TConfig instance)
		{
			try
			{
				var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
				string yaml = serializer.Serialize(instance);
				File.WriteAllText(filePath, yaml, s_encoding);
				Debug.WriteLine($"Saved config: {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save yaml file:");
				Debug.WriteLine(filePath);
				Debug.WriteLine(ex.ToString());
				return false;
			}
		}
		
		public static TConfig Load(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Debug.WriteLine($"Failed to load yaml file:\n{filePath} does not exist.");
				return null;
			}

			try
			{
				string yaml = File.ReadAllText(filePath, s_encoding);
				var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
				TConfig instance = deserializer.Deserialize<TConfig>(yaml);
				Debug.WriteLine($"Loaded config: {filePath}");
				return instance;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load yaml file:");
				Debug.WriteLine(filePath);
				Debug.WriteLine(ex.ToString());
				return null;
			}
		}
	}
	
	public abstract class JsonConfig<TConfig> : ConfigBase
		where TConfig : class
	{
		private static readonly JsonSerializerSettings s_serializerSettings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented
		};
		
		public static bool Save(string filePath, TConfig instance)
		{
			try
			{
				string json = JsonConvert.SerializeObject(instance, s_serializerSettings);
				File.WriteAllText(filePath, json, Encoding.UTF8);
				Debug.WriteLine($"Saved config: {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save json file:");
				Debug.WriteLine(filePath);
				Debug.WriteLine(ex.ToString());
				return false;
			}
		}

		public static TConfig Load(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Debug.WriteLine($"Failed to load json file:\n{filePath} does not exist.");
				return null;
			}

			try
			{
				string json = File.ReadAllText(filePath, Encoding.UTF8);
				TConfig instance = JsonConvert.DeserializeObject<TConfig>(json, s_serializerSettings);
				Debug.WriteLine($"Loaded config: {filePath}");
				return instance;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load json file:");
				Debug.WriteLine(filePath);
				Debug.WriteLine(ex.ToString());
				return null;
			}
		}
	}
}
