using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using eServices.BuildTools.SqlAgentTool;
using Microsoft.Data.SqlClient;

[ExcludeFromCodeCoverage]
internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		var connStringOption = new Option<string>(
			name: CommandStrings.ConnectionString_Name,
			description: CommandStrings.ConnectionString_Description);
		var connStringOptionRqd = new Option<string>(
			name: CommandStrings.ConnectionString_Name,
			description: CommandStrings.ConnectionString_Description)
		{
			IsRequired = true
		};
		var sqlJobsXmlOptiion = new Option<FileInfo>(
			name: CommandStrings.SqlJobsXml_Name,
			description: CommandStrings.SqlJobsXml_Description);
		var filesOption = new Option<DirectoryInfo>(
			name: CommandStrings.Files_Name,
			description: CommandStrings.Files_Description);
		var outputOption = new Option<FileInfo>(
			name: CommandStrings.Output_Name,
			description: CommandStrings.Output_Description)
		{ IsRequired = true };
		var jobsOption = new Option<List<string>>(
			name: CommandStrings.Jobs_Name,
			description: CommandStrings.Jobs_Description)
		{
			AllowMultipleArgumentsPerToken = true
		};
		var targetOption = new Option<string>(
			name: CommandStrings.Target_Name,
			description: CommandStrings.Target_Description);
		var ignoreNewOption = new Option<bool?>(
			name: CommandStrings.IgnoreNew_Name,
			description: CommandStrings.IgnoreNew_Description);
		var ignoreExtraOption = new Option<bool?>(
			name: CommandStrings.IgnoreExtra_Name,
			description: CommandStrings.IgnoreExtra_Description);
		var environmentOption = new Option<string>(
			name: CommandStrings.Environment_Name,
			description: CommandStrings.Environment_Description,
			getDefaultValue: () => "Default");

		var jobsXmlCommand = new Command(
			name: CommandStrings.JobsXml_Name,
			description: CommandStrings.JobsXml_Description)
		{
			connStringOption,
			sqlJobsXmlOptiion
		};
		jobsXmlCommand.SetHandler(
			JobsXmlCommandHandler,
			connStringOption,
			sqlJobsXmlOptiion);

		var exportCommand = new Command(
			name: CommandStrings.Export_Name,
			description: CommandStrings.Export_Description)
		{
			connStringOption,
			sqlJobsXmlOptiion,
			filesOption,
			environmentOption,
			jobsOption,
			ignoreNewOption
		};
		exportCommand.AddValidator(result =>
		{
			if (result.Children.Count(s => s.Symbol == connStringOption || s.Symbol == sqlJobsXmlOptiion) != 1)
				result.ErrorMessage = $"You must use either {CommandStrings.ConnectionString_Name} or {CommandStrings.SqlJobsXml_Name}.";
		});
		exportCommand.SetHandler(
			ExportCommandHandler,
			connStringOption,
			sqlJobsXmlOptiion,
			filesOption,
			environmentOption,
			jobsOption,
			ignoreNewOption);

		var scriptCommand = new Command(
			name: CommandStrings.Script_Name,
			description: CommandStrings.Script_Description)
		{
			connStringOption,
			sqlJobsXmlOptiion,
			targetOption,
			filesOption,
			outputOption,
			environmentOption,
			jobsOption,
			ignoreExtraOption
		};
		scriptCommand.SetHandler(
			ScriptCommandHandler,
			connStringOption,
			sqlJobsXmlOptiion,
			targetOption,
			filesOption,
			outputOption,
			environmentOption,
			jobsOption,
			ignoreExtraOption);

		var publishCommand = new Command(CommandStrings.Publish_Name, CommandStrings.Publish_Description)
		{
			connStringOption,
			sqlJobsXmlOptiion,
			targetOption,
			filesOption,
			environmentOption,
			jobsOption,
			ignoreExtraOption
		};
		publishCommand.SetHandler(
			PublishCommandHandler,
			connStringOption,
			sqlJobsXmlOptiion,
			targetOption,
			filesOption,
			environmentOption,
			jobsOption,
			ignoreExtraOption);

		var rootCommand = new RootCommand(CommandStrings.Root_Description)
		{
			jobsXmlCommand,
			exportCommand,
			scriptCommand,
			publishCommand
		};

		var parser = new CommandLineBuilder(rootCommand)
			.UseDefaults()
			.UseExceptionHandler((exception, context) =>
			{
				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine(exception.ToString());
				Console.ForegroundColor = originalColor;
				context.ExitCode = 1;
			}).Build();

		return await parser.InvokeAsync(args);
	}

	private static async Task JobsXmlCommandHandler(string connStringValue, FileInfo sqlJobsXmlValue)
	{
		using var sqlConnection = await GetSqlConnection(connStringValue);

		var serverJobsService = new ServerJobsService(sqlConnection, sqlJobsXmlValue);
		await serverJobsService.SaveXml();
	}

	private static async Task ExportCommandHandler(string connStringValue, FileInfo sqlJobsXmlValue, DirectoryInfo filesValue, string environment, List<string> jobsValue, bool? ignoreNewValue)
	{
		using var sqlConnection = await GetSqlConnection(connStringValue);

		var serverJobsService = new ServerJobsService(sqlConnection, sqlJobsXmlValue, jobsValue);
		var sourceJobsService = new SourceJobsService(filesValue, jobsValue);
		var commandHandler = new CommandHandlerService(serverJobsService, sourceJobsService);

		await commandHandler.Export(environment, ignoreNewValue.GetValueOrDefault());
	}

	private static async Task ScriptCommandHandler(string connStringValue, FileInfo sqlJobsXmlValue, string? targetValue, DirectoryInfo filesValue, FileInfo outputValue, string environment, List<string> jobsValue, bool? ignoreExtraValue)
	{
		using var sqlConnection = await GetSqlConnection(connStringValue);

		var serverJobsService = new ServerJobsService(sqlConnection, sqlJobsXmlValue, jobsValue, targetValue);
		var sourceJobsService = new SourceJobsService(filesValue, jobsValue);
		var commandHandler = new CommandHandlerService(serverJobsService, sourceJobsService);

		await commandHandler.Script(environment, outputValue, ignoreExtraValue.GetValueOrDefault());
	}

	private static async Task PublishCommandHandler(string connStringValue, FileInfo sqlJobsXmlValue, string? targetValue, DirectoryInfo filesValue, string environment, List<string> jobsValue, bool? ignoreExtraValue)
	{
		using var sqlConnection = await GetSqlConnection(connStringValue);

		var serverJobsService = new ServerJobsService(sqlConnection, sqlJobsXmlValue, jobsValue, targetValue);
		var sourceJobsService = new SourceJobsService(filesValue, jobsValue);
		var commandHandler = new CommandHandlerService(serverJobsService, sourceJobsService);

		await commandHandler.Publish(environment, ignoreExtraValue.GetValueOrDefault());
	}

	private static async Task<SqlConnection?> GetSqlConnection(string connStringValue)
	{
		if (string.IsNullOrWhiteSpace(connStringValue)) return null;
		var sqlConnection = new SqlConnection(connStringValue);
		await sqlConnection.OpenAsync();
		return sqlConnection;
	}
}
