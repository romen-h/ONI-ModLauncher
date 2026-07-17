using System.CommandLine;

namespace ONIModManager.CLI.Commands
{
	public interface ICommandInfo
	{
		string Name { get; }
		string Description { get; }
		IReadOnlyList<Argument> Arguments { get; }
		IReadOnlyList<Option> Options { get; }
		Task Execute(ParseResult args);
	}
}
