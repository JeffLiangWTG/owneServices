using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eServices.BuildTools.PackTool;

internal class Program
{
	private static async Task<int> Main(string[] args)
	{
		MSBuildLocator.RegisterInstance(MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(
		   instance => instance.Version).First());

		var serviceProvider = new ServiceCollection()
			.AddLogging(builder => builder.AddSimpleConsole(options =>
			{
				options.SingleLine = true;
				options.TimestampFormat = "hh:mm:ss.fff ";
			})
			.SetMinimumLevel(LogLevel.Trace))
			.AddSingleton(new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true }))
			.AddSingleton<PackCommandService>()
			.BuildServiceProvider();

		var project = new Option<FileInfo>(CommandStrings.Project_Name, CommandStrings.Project_Description) { IsRequired = true };
		var packageSource = new Option<Uri>(CommandStrings.PackageSource_Name, () => new Uri("https://proget.wtg.zone/nuget/WTG-Internal/v3/index.json"), CommandStrings.PackageSource_Description);

		var rootCommand = new RootCommand(CommandStrings.Root_Description) { project, packageSource };
		rootCommand.SetHandler((projectOption, packageSourceOption) =>
		{
			var buildHandler = ActivatorUtilities.CreateInstance<BuildHandler>(serviceProvider, projectOption);
			var handler = serviceProvider.GetRequiredService<PackCommandService>();
			handler.Invoke(buildHandler, packageSourceOption);
		}, project, packageSource);

		var parser = new CommandLineBuilder(rootCommand)
			.UseDefaults()
			.UseExceptionHandler((exception, context) =>
			{
				var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
				logger.LogError(exception, "Error executing command.");
				context.ExitCode = 1;
			}).Build();

		return await parser.InvokeAsync(args);
	}
}