using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Blazor.Common;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.PlaywrightTesting;

namespace CargoWise.Blazor.Testing.Common
{
	public class WithClientAppPlaywrightAttribute : WithPlaywrightPageAttribute
	{
		const string ZipFilePath = @"\\datfiles.wtg.zone\ThirdParty\Web\ClientAppInstallers\CargoWise.Blazor.Client.Bundle.Setup.20230712125223.zip";

		const string DefaultServerUrl = "https://localhost:5000/";

		const int DefaultCdpPort = 9222;

		const int LaunchAppTimeout = 30;

		readonly string TempDirectory = Path.Combine("..", "Temp", Guid.NewGuid().ToString());
		string ClientAppExePath => Path.Combine(TempDirectory, "Debug", "CargoWise.exe");
		string ServerAppExePath => Path.Combine("..", BuildFileSystem.AppServerBin.PublishExePath);

		Process ClientAppProcess;
		Process ServerProcess;

		public string ServerUrl { get; set; } = DefaultServerUrl;

		public WithClientAppPlaywrightAttribute() : base()
		{
			base.CdpPort = DefaultCdpPort;
		}

		public override void BeforeTest(ITest test)
		{
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			ClientAppPlaywrightBeforeTestAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
			base.BeforeTest(test);
		}

		public override void AfterTest(ITest test)
		{
			base.AfterTest(test);

			Shutdown(ClientAppProcess);
			Shutdown(ServerProcess);
			Uninstall();
		}

		async Task ClientAppPlaywrightBeforeTestAsync()
		{
			Install();
			Assert.That(Path.Exists(ClientAppExePath), Is.True, $"{ClientAppExePath} is not exists!");
			Assert.That(Path.Exists(ServerAppExePath), Is.True, $"{ServerAppExePath} is not exists!");

			EnsureCertificateIsTrusted();

			await StartServerAsync();

			await StartClientAppAsync();
		}

		async Task StartClientAppAsync()
		{
			ClientAppProcess = new Process
			{
				//We need to update this if we use msix installer in the future.
				StartInfo = new ProcessStartInfo(ClientAppExePath, $"cargowiseclient:{ServerUrl} diag -d -t -s -f")
				{
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					EnvironmentVariables = { ["WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"] = $"--remote-debugging-port={CdpPort}" }
				}
			};
			ClientAppProcess.Start();

			DateTime now = DateTime.Now;
			while (true)
			{
				Assert.That(ClientAppProcess!.HasExited, Is.False, "Client process exited unexpectedly");
				Assert.That(DateTime.Now.Subtract(now).TotalSeconds, Is.LessThan(LaunchAppTimeout), "Client app launch timeout.");

				var output = await ClientAppProcess!.StandardOutput.ReadLineAsync();
				if (output != null && output.Contains("HostWebViewOnNavigationCompleted", StringComparison.InvariantCulture))
				{
					Thread.Sleep(500);
					break;
				}
			}
		}

		void EnsureCertificateIsTrusted()
		{
			var process = Process.Start(new ProcessStartInfo("dotnet", " dev-certs https"));
			process.WaitForExit();
		}

		async Task StartServerAsync()
		{
			ServerProcess = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = ServerAppExePath,
					Arguments = $"--urls {ServerUrl} --environment=Development"
				}
			};

			ServerProcess.Start();

			using var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
			{
				return true;
			};
			DateTime now = DateTime.Now;
			using var client = new HttpClient(handler);

			while (true)
			{
				Assert.That(ServerProcess!.HasExited, Is.False, $"Server process exited unexpectedly, Server app exe path: {Path.GetFullPath(ServerAppExePath)}");
				Assert.That(DateTime.Now.Subtract(now).TotalSeconds, Is.LessThan(LaunchAppTimeout), $"Server app launch timeout, Server app exe path: {Path.GetFullPath(ServerAppExePath)}");
				try
				{
					var response = await client.GetAsync($"{ServerUrl}health");
					if (HttpStatusCode.OK == response.StatusCode)
					{
						break;
					}
				}
				catch (Exception)
				{
					Thread.Sleep(200);
				}
			}
		}

		void Shutdown(Process process)
		{
			if (process != null)
			{
				process.Kill();
				process.Dispose();
			}
		}

		void Install()
		{
			if (!Directory.Exists(TempDirectory))
			{
				Directory.CreateDirectory(TempDirectory);
			}
			var tempZipPath = Path.Combine(TempDirectory, "CargoWise_Debug.zip");

			File.Copy(ZipFilePath, tempZipPath);
			ZipFile.ExtractToDirectory(tempZipPath, TempDirectory);

			var process = Process.Start(new ProcessStartInfo(Path.Combine(TempDirectory, "CargoWiseSetupBundle.exe"), "-install -quiet")
			{
				UseShellExecute = false,
			});
			process.WaitForExit();
			Assert.That(process.ExitCode, Is.EqualTo(0), "CargoWiseSetupBundle install fail.");
		}

		void Uninstall()
		{
			var process = Process.Start(new ProcessStartInfo(Path.Combine(TempDirectory, "CargoWiseSetupBundle.exe"), "-uninstall -quiet")
			{
				UseShellExecute = false,
			});
			process.WaitForExit();

			Directory.Delete(TempDirectory, recursive: true);
			Assert.That(process.ExitCode, Is.EqualTo(0), "CargoWiseSetupBundle uninstall fail.");
		}
	}
}
