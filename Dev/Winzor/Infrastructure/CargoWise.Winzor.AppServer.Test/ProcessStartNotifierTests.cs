// #define CAPTURE_BLAZOR_OUTPUT
// uncomment the above and the server will capture stdout/stderr from the launched blazor app, and upload to DAT artifact repository
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Blazor.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WTG.DevTools.Common;

namespace CargoWise.Blazor.AppServer.Test
{
	public class ProcessStartNotifierTests
	{
		Process blazorProcess;
		readonly string tempFolder = Path.GetTempPath();
#if CAPTURE_BLAZOR_OUTPUT
		StringWriter stdOutWriter;
		StringWriter stdErrWriter;
#endif
		[SetUp]
		public void SetUp()
		{
			blazorProcess = null;
		}

		[TearDown]
		public void TearDown()
		{
			if (blazorProcess != null)
			{
				StopBlazorApp();
			}
		}

		[Test]
		public void FileIsWritten()
		{
			RunTest($" --BlazorAppProcInfoDirectory {tempFolder} --urls http://localhost:12345", (address, uniqueId) =>
			{
				Assert.That(address, Is.EqualTo("http://localhost:12345"));
				Assert.That(uniqueId, Is.Not.EqualTo(Guid.Empty));
			});
		}

		[Test]
		public void EventIsSet()
		{
			var eventName = Guid.NewGuid().ToString();
			using var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);
			var args = $" --BlazorAppProcInfoDirectory {tempFolder} --urls http://*:0 --SignalEventWhenStarted {eventName}";
			StartBlazorApp(args);
			Assert.That(Path.Combine(@"..", BuildFileSystem.AppServerBin.PublishExePath), Does.Exist, message: "CargoWise Blazor AppServer executable is not in the publish folder");
			Assert.That(blazorProcess.HasExited, Is.False, "The blazor process should still be running");
			Assert.That(blazorProcess.Id, Is.Not.Zero, "Blazor Process ID should not be zero");
			Assert.That(ewh.WaitOne(120000), message: $"Failed to wait on event with these args {args}");
		}

		[Test]
		public void BlazorProcessIsListeningOnAddressInFile()
		{
			RunTest($" --urls http://*:0 --BlazorAppProcInfoDirectory {tempFolder}", (address, _) =>
			{
				var port = new Uri(address).Port;
				var netstat = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "netstat.exe",
						Arguments = "-ano",
						UseShellExecute = false,
						RedirectStandardOutput = true,
					},
				};
				netstat.Start();
				var netstatOutput = netstat.StandardOutput.ReadToEnd();
				// sample target output line
				//   TCP    127.0.0.1:5000         0.0.0.0:0              LISTENING       2956
				// Things we want to check:
				// * TCP (not UDP)
				// * the listening port - the address is a bit tricky, could be IPv4 or v6, could be all addresses (0.0.0.0/[::]) or a specific address
				// * "LISTENING" status
				// owned by the BlazorApp process
				var regex = new Regex($@"^\s*TCP\s+.+:{port}.+LISTENING\s+{blazorProcess.Id}\s*$", RegexOptions.Multiline);
				Assert.That(netstat.ExitCode, Is.EqualTo(0));
				Assert.That(regex.IsMatch(netstatOutput), Is.True);
			});
		}

		void RunTest(string arguments, Action<string, Guid> afterStartProcessAction)
		{
			var eventName = Guid.NewGuid().ToString();
			using var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);
			StartBlazorApp(arguments + $" --SignalEventWhenStarted {eventName}");
			if (ewh.WaitOne(20000))
			{
				Exception exception = null;

				try
				{
					var fileName = Path.Combine(tempFolder, $"{blazorProcess.Id}.json");
					if (File.Exists(fileName))
					{
						using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete))
						{
							var root = JsonDocument.Parse(stream).RootElement;
							var address = root.GetProperty("Address").GetString();
							var uniqueId = root.GetProperty("UniqueId").GetGuid();
							afterStartProcessAction(address, uniqueId);
						}
						exception = null;
						return;
					}
				}
				catch (Exception e)
				{
					exception = e;
				}
				Assert.That(exception, Is.Null);
			}
			else
			{
				Assert.Fail("EventWaitHandle did not get signaled");
			}
		}

		/// todo: crossover with <see cref="Common.AppServerProcess"/>?
		void StartBlazorApp(string args)
		{
			if (blazorProcess != null)
			{
				throw new NotSupportedException("Starting multiple blazor processes");
			}

			var cwoptions = new CargoWiseOptions();
			var configuration = new TestConfiguration();
			ConfigurationBinder.Bind(configuration, "CargoWiseOptions", cwoptions);
			var binPath = Path.GetFullPath("..");
			var token = new DebugClientTokenGenerator(Options.Create(cwoptions)).GenerateClientToken();
			args += @$" --webroot {Path.Combine(binPath, BuildFileSystem.AppServerBin.PublishDirectoryPath, "wwwroot")} --cargowiseoptions:dbservername {cwoptions.DbServerName} --cargowiseoptions:databasename {cwoptions.DatabaseName} --clienttoken {token}";
			var blazorBinPath = Path.Combine(binPath, BuildFileSystem.AppServerBin.PublishExePath);
			blazorProcess = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = blazorBinPath,
					WorkingDirectory = Path.GetDirectoryName(blazorBinPath),
					Arguments = args,
#if CAPTURE_BLAZOR_OUTPUT
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
#else
					RedirectStandardOutput = false,
					RedirectStandardError = false,
#endif
				},
			};

#if CAPTURE_BLAZOR_OUTPUT
			stdOutWriter = new StringWriter();
			stdErrWriter = new StringWriter();
			blazorProcess.OutputDataReceived += (sender, args) => stdOutWriter.WriteLine(args.Data);
			blazorProcess.ErrorDataReceived += (sender, args) => stdErrWriter.WriteLine(args.Data);
#endif

			if (!blazorProcess.Start())
			{
				throw new InvalidOperationException("Failed to start blazorProcess");
			}

#if CAPTURE_BLAZOR_OUTPUT
			blazorProcess.BeginOutputReadLine();
			blazorProcess.BeginErrorReadLine();
#endif
		}

		public void StopBlazorApp()
		{
			if (blazorProcess == null)
			{
				throw new InvalidOperationException("No blazor process reference to stop");
			}

			//capture some properties here before attempting shutdown
			var procWasAlreadyStopped = blazorProcess.HasExited;
			var args = blazorProcess.StartInfo.Arguments;
			int? exitCode;

			Exception blazorKillException = null;
			try
			{
				if (!procWasAlreadyStopped)
				{
					blazorProcess.Kill(true);
				}
				exitCode = blazorProcess.ExitCode;
			}
			catch (Exception ex)
			{
				blazorKillException = ex;
				exitCode = null;
			}

#if CAPTURE_BLAZOR_OUTPUT
			if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
			{
				string outputUrl = null;
				var output = "Launched BlazorApp with arguments: " + args + "\r\n\r\n";
				output += stdOutWriter.ToString();
				output += "\r\n\r\nStandard error:\r\n\r\n";
				output += stdErrWriter.ToString();
				output += $"\r\n\r\nBlazor app was {(procWasAlreadyStopped ? "stopped" : "running")} after completion of the loop, exit code {exitCode}";
				using var stream = new MemoryStream(Encoding.UTF8.GetBytes(output));
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits (todo: this can be now be replaced with AddTestAttachment())
				outputUrl = new TestFailureDataClient().Upload(stream, "text/plain", ".txt").GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

				TestContext.Error.WriteLine("CW Blazor process info " + (!string.IsNullOrEmpty(outputUrl) ? ", output at " + outputUrl : ""));
			}
#endif

			if (blazorKillException != null)
			{
#pragma warning disable CA2201 // Do not raise reserved exception types
				throw new Exception("Failed to stop process", blazorKillException);
#pragma warning restore CA2201 // Do not raise reserved exception types
			}
		}
	}
}
