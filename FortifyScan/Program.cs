// See https://aka.ms/new-console-template for more information

using CommandLine;

await CommandLine.Parser.Default.ParseArguments<RunArgs>(args)
    .WithParsedAsync(async arg => await RunCommand.Run(arg));