using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public static class WinProcessor
	{
		public static void RunProcessWithNoExceptionHandler(ProcessStartInfo procStartInfo)
		{
			Argument.NotNull(procStartInfo, nameof(procStartInfo));

			RunProcess(procStartInfo, false);
		}

		public static void RunProcess(ProcessStartInfo procStartInfo)
		{
			Argument.NotNull(procStartInfo, nameof(procStartInfo));

			RunProcess(procStartInfo, true);
		}

		static void RunProcess(ProcessStartInfo procStartInfo, bool handleException)
		{
			Argument.NotNull(procStartInfo, nameof(procStartInfo));

			try
			{
				using (var process = new Process
				{
					StartInfo = procStartInfo,
					EnableRaisingEvents = true
				})
				{
					process.OutputDataReceived += (s, d) =>
					{
						Console.WriteLine(d.Data);
					};

					process.ErrorDataReceived += (s, d) =>
					{
						Console.WriteLine(d.Data);
					};

					process.Start();
					process.BeginErrorReadLine();
					process.BeginOutputReadLine();

					process.WaitForExit();

					if (handleException && process.ExitCode != 0)
					{
						throw new TaskSchedulerException("The process has exited with an unexpected error. Please check the message above");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}
	}
}
