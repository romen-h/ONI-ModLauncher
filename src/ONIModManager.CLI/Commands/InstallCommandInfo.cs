using System.CommandLine;

namespace ONIModManager.CLI.Commands
{
	public class InstallCommandInfo : ICommandInfo
	{
		public string Name => "install";
		
		public string Description => "Installs a mod";
		
		public IReadOnlyList<Argument> Arguments
		{ get; } = new List<Argument>() {
			new Argument<string>("zipPath")
			{
				Description = "The path to the zip file to install."
			}
		};
		
		public IReadOnlyList<Option> Options => null;

		public async Task Execute(ParseResult args)
		{
			
		}
	}
}
