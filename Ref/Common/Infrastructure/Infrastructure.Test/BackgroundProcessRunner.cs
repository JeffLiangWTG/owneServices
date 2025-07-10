using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public class BackgroundProcessRunner
	{
		int TimeoutInMilliseconds { get; }
		ProcessStartInfo ProcessStartInfo { get; }
		public StringBuilder OutputLog { get; } = new StringBuilder();
		public Process Process { get; set; }

		public BackgroundProcessRunner(ProcessStartInfo processStartInfo, int timeoutInMilliseconds = 600 * 1000)
		{
			TimeoutInMilliseconds = timeoutInMilliseconds;
			ProcessStartInfo = processStartInfo;
		}

		public void Run()
		{
			var task = new ThreadStart(RunProcess);
			new Thread(task) { IsBackground = true }.Start();
			Thread.Sleep(TimeSpan.FromSeconds(5));
		}

		void RunProcess()
		{
			using (Process = new Process())
			{
				Process.StartInfo = ProcessStartInfo;
				Process.OutputDataReceived += (sender, e) => OutputLog.AppendLine(e.Data);
				Process.ErrorDataReceived += (sender, e) => OutputLog.AppendLine(e.Data);

				if (!Process.Start())
				{
					OutputLog.AppendLine("Failed to start Process");
					return;
				}

				Process.BeginErrorReadLine();
				Process.BeginOutputReadLine();

				if (Process.WaitForExit(TimeoutInMilliseconds))
				{
					return;
				}

				OutputLog.AppendLine("Killing process because it has not exited within the timeout period.");
				Kill();
			}
		}

		public void Kill()
		{
			try
			{
				Process.Kill();
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
