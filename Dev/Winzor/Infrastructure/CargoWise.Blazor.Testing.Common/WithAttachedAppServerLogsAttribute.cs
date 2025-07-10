using System;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace CargoWise.Blazor.Testing.Common
{
	// For tests that execute an CargoWise.Winzor.AppServer (either directly using the AppServerProcess class or via the session broker), this attribute will
	// ensure that the CargoWise.Winzor.AppServer logs are attached to the test if the test fails, either in Visual Studio or DAT.
	// The BeforeTest will allocate a separate temp log directory for each test method and store it on the test context.
	// There is a new TestAppServerProcess that inherits from AppServerProcess.  It adds environment variables to the process that gets launched
	// that will cause a new Serilog file sink to be added to the logging configuration, writing to a random file in the test method's log directory
	// After the test has executed (AfterTest below), if the test failed, then any files in that directory will be added to the test as attachments.
	// This attribute has been added as an assembly attribute to CargoWise.Winzor.AppServer.Test and CargoWise.Blazor.SessionBroker.Test
	// so you don't need to add it unless it is to a new test project.  You *do* need to remember to use TestAppServerProcess instead of AppServerProcess in tests.
	// Also, for integration tests that use WebApplicationFactory<T>, there is now CustomWebApplicationFactory.cs which takes care of replacing AppServerProcess
	// in the session broker's DI container.

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class WithAttachedAppServerLogsAttribute : Attribute, ITestAction
	{
		const string LogPathPropertyName = nameof(LogPath);
		public static string LogPath => TestContext.CurrentContext.Test.Properties.Get(LogPathPropertyName) as string;

		public ActionTargets Targets => ActionTargets.Suite | ActionTargets.Test;

		public void BeforeTest(ITest test)
		{
			if (!test.IsSuite)
			{
				var directory = Path.Combine(Path.GetTempPath(), "cw-blazor-test", Path.GetRandomFileName());
				Directory.CreateDirectory(directory);
				test.Properties.Set("LogPath", directory);
			}
		}

		public void AfterTest(ITest test)
		{
			if (!test.IsSuite)
			{
				var logPath = LogPath;
				if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
				{
					if (Directory.Exists(logPath))
					{
						foreach (var file in Directory.GetFiles(logPath))
						{
							TestContext.AddTestAttachment(file, $"CargoWise.Winzor.AppServer log file ({Path.GetFileName(file)})");
						}
					}
					else
					{
						throw new FileNotFoundException("CargoWise.Winzor.AppServer log file not found in expected location", logPath);
					}
				}
				else // test didn't fail, don't need to keep the log file, but don't really mind if it fails to delete
				{
					try
					{
						Directory.Delete(logPath, recursive: true);
					}
					catch
					{
					}
				}
			}
		}
	}
}
