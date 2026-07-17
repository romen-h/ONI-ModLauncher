using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using ONIModManager.CLI.Commands;
using ONIModManager.Core;
using ONIModManager.Core.Models;

namespace ONIModManager.CLI;

static class Program
{
	private static readonly Option<string> _dlcID = new("--dlc")
	{
		Description = "The DLC ID of the mod to modify."
	};

	static async Task<int> Main(string[] args)
	{
		RootCommand rootCommand = new("Oxygen Not Included CLI Mod Manager.");
		
		rootCommand.AddCommandInfo<SetPathCommandInfo>();
		rootCommand.AddCommandInfo<LaunchCommandInfo>();
		rootCommand.AddCommandInfo<ListModsCommandInfo>();
		rootCommand.AddCommandInfo<EnableModCommandInfo>();
		rootCommand.AddCommandInfo<DisableModComamndInfo>();
		
		try
		{
			Settings.Load();
			Debug.WriteLine("Loaded settings.");
		}
		catch (Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Failed to load settings.");
			Console.Error.WriteLine(ex.ToString());
			return 1;
		}
		
		Console.ForegroundColor = ConsoleColor.White;
		return await rootCommand.Parse(args).InvokeAsync();
	}
	
	private static void AddCommandInfo<TCommand>(this RootCommand root)
		where TCommand : ICommandInfo, new()
	{
		TCommand info = new TCommand();
		
		var command = new Command(info.Name, info.Description);
		var args = info.Arguments;
		if (args != null)
		{
			foreach (var arg in info.Arguments)
			{
				command.Add(arg);
			}
		}
		
		var options = info.Options;
		if (options != null)
		{
			foreach (var option in options)
			{
				command.Add(option);
			}
		}
		
		command.SetAction(info.Execute);
		
		root.Subcommands.Add(command);
	}
}
