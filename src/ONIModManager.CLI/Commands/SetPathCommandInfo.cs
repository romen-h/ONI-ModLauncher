using System.CommandLine;

namespace ONIModManager.CLI.Commands
{
	public class SetPathCommandInfo : ICommandInfo
	{
		public string Name => "set-path";
		
		public string Description => "Sets a path in the tool settings.";
		
		public IReadOnlyList<Argument> Arguments => [
			new Argument<string>("path-key")
			{
				Description = "The path setting key to set."
			},
			new Argument<string>("path-value")
			{
				Description = "The path setting value to set."
			}
		];
		
		public IReadOnlyList<Option> Options => null;

		public async Task Execute(ParseResult args)
		{
			string key = args.GetRequiredValue<string>("path-key").ToLowerInvariant();
			string path = args.GetRequiredValue<string>("path-value");
		
			switch (key)
			{
				case "gamefolder":
					if (!Directory.Exists(path)) throw new Exception($"Path '{path}' does not exist.");
					string oniExecutable = Path.Combine(path, "OxygenNotIncluded");
					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					{
						oniExecutable += ".exe";
					}
					if (!File.Exists(oniExecutable)) throw new Exception($"Oxygen Not Included executable '{oniExecutable}' does not exist in the given folder.");
					Settings.Instance.Paths.GameFolder = path;
					Settings.Save();
					Console.WriteLine($"Set game folder to '{path}'.");
					return;
			
				default:
					throw new Exception($"Unknown path key '{key}'.");
			}
		}
	}
}
