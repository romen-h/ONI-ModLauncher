using System.CommandLine;
using ONIModManager.Core;

namespace ONIModManager.CLI.Commands
{
	public class ListModsCommandInfo : ICommandInfo
	{
		public string Name => "list";
		
		public string Description => "Lists all installed mods";
		
		public IReadOnlyList<Argument> Arguments => null;
		
		public IReadOnlyList<Option> Options => [
		];

		public async Task Execute(ParseResult args)
		{
			ModManager.Init(Settings.Instance.Paths.GameFolder, null);
			
			Console.WriteLine("[Enabled] StaticID");
			Console.WriteLine("----------------------------------------");
			foreach (var mod in ModManager.Instance.Mods.Values)
			{
				Console.Write("[");
				if (mod.EnabledForCurrentDlc)
				{
					Console.Write("X");
				}
				else
				{
					Console.Write(" ");
				}
				Console.Write("] ");
				Console.WriteLine(mod.StaticID);
			}
		}
	}
}
