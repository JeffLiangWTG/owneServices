using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Analyzers.Integration.Test
{
	static class ProcessRunner
	{
		public static string RunTestsOnProject(string testCode, string analyzerType, List<string> devAssemblies)
		{
			var analyzerRunnerExeLocation = Assembly.GetExecutingAssembly().Location;
			var analyzerRunnerDirectory = Path.GetDirectoryName(analyzerRunnerExeLocation) ?? throw new InvalidOperationException($"Could not get directory for analyzer runner executing assembly. Exe location is: {analyzerRunnerExeLocation}");
			var integrationRunnerExeLocation = Path.Combine(analyzerRunnerDirectory, "IntegrationTests", "IntegrationTestsRunner.exe");

			var arguments = $"\"{testCode}\" \"{analyzerType}\" \"{string.Join(",", devAssemblies)}\"";

			return RunProcessStart(arguments, integrationRunnerExeLocation);
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		static string RunProcessStart(string arguments, string fileName)
		{
			var process = Process.Start(new ProcessStartInfo()
			{
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			}) ?? throw new InvalidOperationException($"Process.Start did not return a valid process. FileName: {fileName} Arguments: {arguments}");

			var output = new StringBuilder();
			process.OutputDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					output.AppendLine(e.Data);
				}
			};
			process.BeginOutputReadLine();

			process.ErrorDataReceived += (sender, e) =>
			{
				if (e.Data != null)
				{
					output.AppendLine(e.Data);
				}
			};
			process.BeginErrorReadLine();

			process.WaitForExit();

			return output.ToString();
		}

		public static void AssertNoIssuesOnTest(string testCode, string analyzerType, List<string> devAssemblies)
		{
			var output = RunTestsOnProject(testCode, analyzerType, devAssemblies);

			if (output.Length > 0)
			{
				AssertionWithHtml.HtmlFail(output);
			}
			else
			{
				Assertion.Assert(true);
			}
		}
	}
}
