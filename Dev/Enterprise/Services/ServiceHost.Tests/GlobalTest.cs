using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Services.ServiceHost.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Common;
using Enterprise.ZArchitecture.Web.GlobalBase;
using Enterprise.ZArchitecture.Web.Shared.Test;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class GlobalTest : TestCase
	{
		public void TestApplicationStart()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			{
				InstallWebFiles(tempDirectory);

				// Act
				// Assert
				var processOutput = StartWebTesterProcess(nameof(GlobalApplicationStartAndAssert), tempDirectory);

				// Assert
				AssertEquals(
					$@"{processOutput.Errors}
{processOutput.Output}",
					true,
					string.IsNullOrEmpty(processOutput.Errors));
			}

			void InstallWebFiles(string targetDirectory)
			{
				var binDirectory = Path.Combine(targetDirectory, "bin");
				Directory.CreateDirectory(binDirectory);

				var binFiles = new List<string>
				{
					"NUnitCore.dll",
					"Enterprise.Services.ServiceHost.dll",
					"Enterprise.ZArchitecture.Web.GlobalBase.dll",
					"Enterprise.ZArchitecture.Core.dll",
					"CargoWise.Common.dll",
					"Enterprise.Environment.dll",
					"CargoWise.Shared.40.dll",
					"System.Web.Http.dll",
					"System.Web.Http.WebHost.dll",
					"Enterprise.Initialisation.dll",
					"Enterprise.Integration.dll",
					"CargoWise.Data.dll",
					"CargoWiseOne.WebInfrastructure.dll",
					"Newtonsoft.Json.dll",
					"moq.dll",
					"Castle.Core.dll",
					"Enterprise.ZArchitecture.Web.Business.dll",
					"CargoWise.Types.dll",
					"Resources.dll",
					"CargoWise.BrandManager.dll",
					"CargoWise.ApplicationContext.dll",
					"CargoWise.Bi.Common.dll",
					"CargoWise.Bi.Deployment.ReportingServices.dll",
					"CargoWise.Bi.Registration.dll",
					"CargoWise.ComponentModel.dll",
					"CargoWise.Database.Shared.dll",
					"CargoWise.Definitions.XmlSerializers.dll",
					"CargoWise.eHub.Adapter.dll",
					"CargoWise.eHub.Common.dll",
					"CargoWise.EntityFramework.dll",
					"CargoWise.Definitions.dll",
					"CargoWise.Authentication.Primitives.dll",
					"CargoWise.Authentication.Glow.Ticketing.dll",
					"CargoWise.Glow.Model.Interfaces.dll",
					"CargoWise.Integration.dll",
					"CargoWise.IO.dll",
					"CargoWise.PAVE.Common.DTO.dll",
					"CargoWise.PAVE.Common.Interfaces.dll",
					"CargoWise.ResourceStrings.Cache.dll",
					"CargoWise.Workflow.dll",
					"CargoWise.ResourceStrings.dll",
					"CargoWise.Schema.dll",
					"Enterprise.Accounting.Business.dll",
					"Enterprise.Accounting.DataTransfer.dll",
					"Enterprise.Billing.Integration.dll",
					"Enterprise.BufferManagement.Service.dll",
					"Enterprise.BufferManagement.Service.Shared.dll",
					"Enterprise.Customs.Business.dll",
					"Enterprise.DataTransfer.Common.dll",
					"Enterprise.DocumentEngine.dll",
					"Enterprise.DocumentEngineCore.dll",
					"Enterprise.DocumentEngineIntegration.dll",
					"Enterprise.DocumentScanning.Business.dll",
					"Enterprise.DocumentScanning.Integration.dll",
					"Enterprise.DocumentScanning.Web.dll",
					"Enterprise.eHubMessaging.Business.dll",
					"Enterprise.eTail.Business.dll",
					"Enterprise.eTail.Integration.dll",
					"Enterprise.ExcelTemplates.Integration.dll",
					"Enterprise.Freight.dll",
					"Enterprise.Freight.Common.Business.dll",
					"Enterprise.Freight.Forwarding.Business.dll",
					"Enterprise.Freight.Integration.dll",
					"Enterprise.Licensing.Core.dll",
					"Enterprise.MasterData.Common.dll",
					"Enterprise.MasterFiles.Business.dll",
					"Enterprise.MasterFiles.DataTransfer.dll",
					"Enterprise.MasterFiles.Integration.dll",
					"Enterprise.MasterFiles.Tracking.dll",
					"Enterprise.Messaging.Business.dll",
					"Enterprise.Messaging.Integration.dll",
					"Enterprise.Rating.Business.dll",
					"Enterprise.Registry.Business.dll",
					"Enterprise.Security.dll",
					"Enterprise.Security.Core.dll",
					"Enterprise.ServiceManager.Module.dll",
					"Enterprise.ServiceManager.Shared.dll",
					"Enterprise.Tracking.Business.dll",
					"Enterprise.UniversalDataBuss.DataObjects.dll",
					"Enterprise.UniversalDataBuss.Integration.dll",
					"Enterprise.UniversalDataBuss.Management.dll",
					"Enterprise.Upgrades.dll",
					"Enterprise.Warehouse.Integration.dll",
					"Enterprise.ZArchitecture.Business.dll",
					"Enterprise.ZArchitecture.GUI.dll",
					"Enterprise.ZArchitecture.Modules.dll",
					"Enterprise.ZArchitecture.Schema.dll",
					"Enterprise.ZArchitecture.Web.Common.dll",
					"Enterprise.ZArchitecture.Web.Shared.dll",
					"Enterprise.ZArchitecture.Web.Utilities.dll",
					"ICSharpCode.SharpZipLib.dll",
					"System.Collections.Immutable.dll",
					"System.Net.Http.Formatting.dll",
					"WTG.Foundation.FrameworkExtensions.dll",
					"WTG.Foundation.Http.dll",
					"WTG.StaticAnalysis.Annotation.dll",
					"Enterprise.Semaphores.Common.dll",
					"CargoWise.Database.Abstractions.dll",
					"CargoWise.ApplicationContext.XmlSerializers.dll",
					"CargoWise.Windows.UI.dll",
					"Enterprise.ZArchitecture.Web.Shared.Tester.exe",
					"CargoWise.Async.dll",
					"CargoWise.DataProtection.dll",
					"CargoWise.DataProtection.SqlExtensions.dll",
					"CargoWise.DataProtection.Administration.dll",
					"CargoWise.DataProtection.Administration.SqlServer.dll",
					"Microsoft.Extensions.DependencyInjection.dll",
					"Microsoft.Extensions.DependencyInjection.Abstractions.dll",
					"Microsoft.Bcl.AsyncInterfaces.dll",
					"System.Threading.Tasks.Extensions.dll",
					"System.Text.Json.dll",
				};
				binFiles.Add(Path.GetFileName(typeof(TestHttpContextHelper).Assembly.Location));
				var globalTestPath = typeof(GlobalTest).Assembly.Location;
				binFiles.Add(Path.GetFileName(globalTestPath));
				var currentDir = Path.GetDirectoryName(globalTestPath);
				binFiles.ForEach(binFileName =>
				{
					File.Copy(Path.Combine(currentDir, binFileName), Path.Combine(binDirectory, binFileName));
				});

				// Remove all ApplicationConfigurationAttribute setting to stop system expecting to have all dlls which have embedded configuration to exists in binDirectory
				File.WriteAllText(Path.Combine(binDirectory, "AssemblyMetaData.xml"), File.ReadAllText(Path.Combine(currentDir, "AssemblyMetaData.xml")).Replace("<ApplicationConfigurationAttribute", "<_ApplicationConfigurationAttribute").Replace("</ApplicationConfigurationAttribute", "</_ApplicationConfigurationAttribute"));

				var webConfigFilePath = Path.Combine(targetDirectory, "Web.Config");
				using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Services.ServiceHost.Tests.Web.config"))
				using (var webConfigFile = new FileStream(webConfigFilePath, FileMode.Create, FileAccess.Write))
				{
					resource.CopyTo(webConfigFile);
				}

				var webConfigDoc = XDocument.Load(webConfigFilePath);
				var assembliesElement = webConfigDoc.XPathSelectElement(@"/configuration/system.web/compilation/assemblies");
				assembliesElement.RemoveAll();
				assembliesElement.Add(new XElement("remove", new XAttribute("assembly", "*")));
				webConfigDoc.Save(webConfigFilePath);
			}

			(string Errors, string Output) StartWebTesterProcess(string testMethodName, string applicationPath)
			{
				var arguments = new List<string>
				{
					QuoteQuote(Db.ServerName),
					QuoteQuote(Db.DatabaseName),
					QuoteQuote(Path.Combine(applicationPath, "bin", Path.GetFileName(Assembly.GetExecutingAssembly().Location))),
					QuoteQuote(GetType().FullName),
					QuoteQuote(testMethodName),
					QuoteQuote(applicationPath)
				};

				var processOutputLogs = new List<string>();
				var processErrorLogs = new List<string>();
				var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var processStartInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(root, WebTesterExe),
					WorkingDirectory = root,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					Arguments = string.Join(" ", arguments),
				};

				var processHasExited = false;
				var process = new Process();

				process.Exited += (object sender, EventArgs e) => { processHasExited = true; };
				process.EnableRaisingEvents = true;
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (e?.Data != null)
					{
						processOutputLogs.Add(e.Data);
					}
				};

				process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (e?.Data != null)
					{
						processErrorLogs.Add(e.Data);
					}
				};

				process.StartInfo = processStartInfo;

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				try
				{
					var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
					while (!process.HasExited && !processHasExited && !cancellationTokenSource.IsCancellationRequested)
					{
						Thread.Sleep(100);
						process.WaitForExit(100);
					}
				}
				finally
				{
					if (!processHasExited)
					{
						process.Kill();
						process.WaitForExit(1000);
					}

					// CargoWise.Async.dll not been released yet, give it a grace time
					Thread.Sleep(TimeSpan.FromSeconds(3));
				}

				var consoleErrors = string.Join(System.Environment.NewLine, processErrorLogs);
				var consoleOutput = string.Join(System.Environment.NewLine, processOutputLogs);

				return (consoleErrors, consoleOutput);

				string QuoteQuote(string argument)
				{
					return $@"""{argument}""";
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console I/O necessary for developer testing.")]
		public void GlobalApplicationStartAndAssert()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			// Arrange
			var serverName = (string)AppDomain.CurrentDomain.GetData(".serverName");
			var databaseName = (string)AppDomain.CurrentDomain.GetData(".databaseName");
			var enterpriseGlobalMock = new Mock<Global>() { CallBase = true };
			enterpriseGlobalMock.Protected()
				.Setup("InitializeServerAndDatabaseNames")
				.Callback(() =>
				{
					Db.InitializeDatabaseDetails(serverName, databaseName);
				});

			Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} is testing {nameof(GlobalApplicationStartAndAssert)}..."); // Console I/O necessary for developer testing.

			// Act
			var enterpriseGlobal = enterpriseGlobalMock.Object;
			using (enterpriseGlobal.StartApplicationDisposable())
			{
				// Assert
				CombineAssertions("Global Application Start initializations", () =>
				{
					AssertType<HttpConfiguration>("HttpConfiguration", GlobalConfiguration.Configuration);

					AssertEquals("MapHttpAttributeRoutes", true, GlobalConfiguration.Configuration.Routes.Any());
					AssertEquals("MessageHandlers", true, GlobalConfiguration.Configuration.MessageHandlers.Any(x => x is InstanceIdHandler));
					AssertType<WebApiServiceExceptionReporter>("IExceptionLogger", GlobalConfiguration.Configuration.Services.GetExceptionLoggers().FirstOrDefault());

					AssertType<BaseWebExceptionReporter>("ErrorReporter.Instance is BaseWebExceptionReporter", ErrorReporter.Instance);
					AssertType<ZArchitecture.Web.GlobalBase.WebServicesEnvironment>("Env.Instance is WebServicesEnvironment", Env.Instance);
					AssertType<BaseWebDbEnvironment>("DbEnv.Instance is BaseWebDbEnvironment", DbEnv.Instance);

					AssertType<WebAssemblyLoader>("AssemblyLoader.Instance is WebAssemblyLoader", AssemblyLoader.Instance);
					AssertEquals("ObjectFactory IsConfigured", true, ObjectFactory.IsConfigured);

					AssertEquals("Not IsUserInteractive", false, Globals.IsUserInteractive);
					AssertEquals("Not IsPostableProcess", false, ThreadSentry.IsPostableProcess);
					AssertEquals("Is Web Service", true, Globals.IsWebService);
				});
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		public void TestDontRemoveHeaderWhenIntegratedPipelineIsFalse()
		{
			var global = new Global();

			var context = new HttpContext(new HttpRequest(string.Empty, "http://test.com", string.Empty), new HttpResponse(new StringWriter()));
			context.Response.AddHeader("Server", "TEST");

			using var resetContext = HttpContextHelper.SetUp(context);

			AssertEquals("Precondition", false, HttpRuntime.UsingIntegratedPipeline);
			AssertNoExceptionThrown("Can only remove headers when HttpRuntime.UsingIntegratedPipeline", () => global.Application_PreSendRequestHeaders_Exposed(null, EventArgs.Empty));
		}

		const string WebTesterExe = "Enterprise.ZArchitecture.Web.Shared.Tester.exe";
	}
}
