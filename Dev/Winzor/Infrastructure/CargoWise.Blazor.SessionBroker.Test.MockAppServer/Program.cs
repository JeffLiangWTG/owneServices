using System.CommandLine;
using System.Diagnostics;

void ParrotStdIn(string procInfoDirectory)
{
	using var reader = new StreamReader(Console.OpenStandardInput());
	File.WriteAllText(Path.Combine(procInfoDirectory, Environment.ProcessId + ".txt"), reader.ReadToEnd());
}

var exitOption = new Option<int?>(
	name: "--exit",
	description: "Exit with the specified exit code.");
var waitOption = new Option<int?>(
	name: "--wait",
	description: "Wait until timeout expires. Pass -1 to wait indefinitely");
var parrotOption = new Option<bool>(
	name: "--parrot",
	description: "Parrot out stdin until end of stream or timeout");
var procInfoDirectoryOption = new Option<string?>(
	name: "--BlazorAppProcInfoDirectory",
	description: "The BlazorAppProcInfoDirectory from SessionBroker");
var signalEventOption = new Option<string?>(
	name: "--SignalEventWhenStarted",
	description: "The event to signal on startup");
var versionBrokerProcessCorrelationIdOption = new Option<string?>(
	name: "--CargoWiseOptions:VersionBrokerProcessCorrelationId",
	description: "The Version Broker process correlation ID");
var sessionBrokerProcessCorrelationIdOption = new Option<string?>(
	name: "--CargoWiseOptions:SessionBrokerProcessCorrelationId",
	description: "The Session Broker process correlation ID");
var hostnameOption = new Option<string?>(
	name: "--CargoWiseOptions:Hostname",
	description: "The authority URI domain name");
var rootCommand = new RootCommand("Mock appserver for unit testing")
{
	TreatUnmatchedTokensAsErrors = false
};

rootCommand.AddOption(exitOption);
rootCommand.AddOption(waitOption);
rootCommand.AddOption(parrotOption);
rootCommand.AddOption(procInfoDirectoryOption);
rootCommand.AddOption(signalEventOption);
rootCommand.AddOption(versionBrokerProcessCorrelationIdOption);
rootCommand.AddOption(sessionBrokerProcessCorrelationIdOption);
rootCommand.AddOption(hostnameOption);

rootCommand.SetHandler(async (exitCode, wait, parrot, procInfoDirectory, signalEventOption, versionBrokerProcessCorrelationId, sessionBrokerProcessCorrelationId, hostname) =>
	{
		if (string.IsNullOrEmpty(versionBrokerProcessCorrelationId))
		{
			versionBrokerProcessCorrelationId =
				Environment.GetEnvironmentVariable("CargoWiseOptions:VersionBrokerProcessCorrelationId");
		}

		if (string.IsNullOrEmpty(sessionBrokerProcessCorrelationId))
		{
			sessionBrokerProcessCorrelationId = Environment.GetEnvironmentVariable("CargoWiseOptions:SessionBrokerProcessCorrelationId");
		}

		if (procInfoDirectory is not null)
		{
			File.WriteAllText(Path.Combine(procInfoDirectory, Environment.ProcessId + ".json"), @"{
""Address"": ""Address"",
""UniqueId"": ""UniqueId""
}");
		}

		if (parrot)
		{
			Debug.Assert(procInfoDirectory is not null, null, nameof(procInfoDirectory) + " must be provided when using the parrot option");
			ParrotStdIn(procInfoDirectory);
		}

		if (!string.IsNullOrEmpty(signalEventOption))
		{
			using var ewh = EventWaitHandle.OpenExisting(signalEventOption);
			ewh.Set();
		}

		if (wait.HasValue)
		{
			await Task.Delay(wait.Value);
		}

		if (exitCode.HasValue)
		{
			Environment.Exit(exitCode.Value);
		}

		if (!string.IsNullOrEmpty(versionBrokerProcessCorrelationId))
		{
			Debug.Assert(procInfoDirectory is not null, null, nameof(procInfoDirectory) + " must be provided when providing a Version Broker Process Correlation Id");
			File.WriteAllText(Path.Combine(procInfoDirectory, "VersionBrokerProcessCorrelationId"), versionBrokerProcessCorrelationId);
		}

		if (!string.IsNullOrEmpty(sessionBrokerProcessCorrelationId))
		{
			Debug.Assert(procInfoDirectory is not null, null, nameof(procInfoDirectory) + " must be provided when providing a Session Broker Process Correlation Id");
			File.WriteAllText(Path.Combine(procInfoDirectory, "SessionBrokerProcessCorrelationId"), sessionBrokerProcessCorrelationId);
		}

		if (!string.IsNullOrEmpty(hostname))
		{
			Debug.Assert(procInfoDirectory is not null, null, nameof(procInfoDirectory) + " must be provided when providing a Hostname");
			File.WriteAllText(Path.Combine(procInfoDirectory, "Hostname"), hostname);
		}
	},
	exitOption, waitOption, parrotOption, procInfoDirectoryOption, signalEventOption, versionBrokerProcessCorrelationIdOption, sessionBrokerProcessCorrelationIdOption, hostnameOption);

await rootCommand.InvokeAsync(args);
