using System.CommandLine;
using ONIModManager.Core;
using ONIModManager.Core.Configs;

namespace ONIModManager.CLI.Commands
{
	public class EnableModCommandInfo : ICommandInfo
	{
		public string Name => "enable";
		
		public string Description => "Enables a mod";
		
		public IReadOnlyList<Argument> Arguments => [
			new Argument<string>("mod")
			{
				Description = "The static ID of the mod, a path to the mod folder, or a path to a mod.yaml file."
			}
		];
		
		public IReadOnlyList<Option> Options => null;

		public async Task Execute(ParseResult args)
		{
			string modString = args.GetRequiredValue<string>("mod");
			
			ModManager.Init(Settings.Instance.Paths.GameFolder, null);
			
			if (modString == "all")
			{
				ModManager.Instance.EnableAllMods();
				return;
			}
			
			if (ModManager.Instance.Mods.ContainsKey(modString))
			{
				ModManager.Instance.EnableMod(modString);
				return;
			}
			
			string modYamlFile;
			if (Directory.Exists(modString))
			{
				modYamlFile = Path.Combine(modString, "mod.yaml");
				if (!File.Exists(modYamlFile)) throw new Exception("Mod folder does not contain a mod.yaml file.");
			}
			else if (File.Exists(modString))
			{
				modYamlFile = modString;
			}
			else
			{
				throw new Exception("Mod argument must be a mod staticID, a path to a mod folder, or a mod.yaml file.");
			}
			
			ModYaml modYaml = ModYaml.Deserialize(modYamlFile);
			ModManager.Instance.EnableMod(modYaml.StaticID);
			ModManager.Instance.SaveModsJson();
		}
	}
	
	public class DisableModComamndInfo : ICommandInfo
	{
		public string Name => "disable";
		
		public string Description => "Disables a mod";
		
		public IReadOnlyList<Argument> Arguments => [
			new Argument<string>("mod")
			{
				Description = "The static ID of the mod, a path to the mod folder, or a path to a mod.yaml file."
			}
		];
		
		public IReadOnlyList<Option> Options => null;

		public async Task Execute(ParseResult args)
		{
			string modString = args.GetRequiredValue<string>("mod");
			
			ModManager.Init(Settings.Instance.Paths.GameFolder, null);
			
			if (modString == "all")
			{
				ModManager.Instance.DisableAllMods();
				return;
			}
			
			if (ModManager.Instance.Mods.ContainsKey(modString))
			{
				ModManager.Instance.EnableMod(modString);
				return;
			}
			
			string modYamlFile;
			if (Directory.Exists(modString))
			{
				modYamlFile = Path.Combine(modString, "mod.yaml");
				if (!File.Exists(modYamlFile)) throw new Exception("Mod folder does not contain a mod.yaml file.");
				
			}
			else if (File.Exists(modString))
			{
				modYamlFile = modString;
			}
			else
			{
				throw new Exception("Mod argument must be a mod staticID, a path to a mod folder, or a mod.yaml file.");
			}
			
			ModYaml modYaml = ModYaml.Deserialize(modYamlFile);
			ModManager.Instance.DisableMod(modYaml.StaticID);
			
			ModManager.Instance.SaveModsJson();
		}
	}
}
