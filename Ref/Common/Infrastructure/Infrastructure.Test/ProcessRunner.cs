using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class ProcessRunner
	{
		public static string RunProcess(ProcessStartInfo processStartInfo, bool combineOutputError, bool checkExitCode = false)
		{
			var outputTextBuilder = new StringBuilder();
			var errorTextBuilder = new StringBuilder();
			using (var process = new Process())
			{
				process.StartInfo = processStartInfo;

				process.OutputDataReceived += (sender, e) => outputTextBuilder.AppendLine(e.Data);
				process.ErrorDataReceived += (sender, e) => errorTextBuilder.AppendLine(e.Data);

				Assert.That(process.Start(), Is.True, "Failed to start Process");
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				if (!process.WaitForExit(TimeoutInMilliseconds))
				{
					try
					{
						process.Kill();
					}
					catch (InvalidOperationException)
					{
						Assert.Fail(errorTextBuilder.ToString());
					}
					process.WaitForExit();
				}
				process.WaitForExit();
				if (combineOutputError)
				{
					return outputTextBuilder.AppendLine(errorTextBuilder.ToString()).ToString();
				}
				if (!checkExitCode)
				{
					return errorTextBuilder.ToString();
				}
				return process.ExitCode == 0 ? string.Empty : errorTextBuilder.ToString();
			}
		}

		const int TimeoutInMilliseconds = 600 * 1000;
	}
}
