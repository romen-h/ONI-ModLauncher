using System.CommandLine;
using System.Diagnostics;
using ONIModManager.Core;

namespace ONIModManager.CLI.Commands
{
	public class LaunchCommandInfo : ICommandInfo
	{
		public string Name => "launch";

		public string Description => "Launches Oxygen Not Included with mods enabled";
		
		public IReadOnlyList<Argument> Arguments => null;
		
		public IReadOnlyList<Option> Options => null;
	
		public async Task Execute(ParseResult args)
		{
			ModManager.Init(Settings.Instance.Paths.GameFolder, null);
			
			Process.Start(new ProcessStartInfo()
			{
				Environment =
				{
					{"SteamAppId", "457140"}
				},
				FileName = ModManager.Instance.GameFolder.GameExecutableFile,
				WorkingDirectory = ModManager.Instance.GameFolder.FolderPath
			});
		}
	}
}
