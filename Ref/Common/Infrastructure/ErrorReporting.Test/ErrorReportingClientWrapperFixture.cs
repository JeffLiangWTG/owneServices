using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Moq;
using NUnit.Framework;
using WTG.ErrorReporting;

namespace CargoWise.RefDbRepo.Common.ErrorReporting.Test
{
	[TestFixture]
	public class ErrorReportingClientWrapperFixture
	{
		Mock<IErrorReportingClientProvider> errorReportingClientProviderMock;
		Mock<IErrorReportingClient> errorReportingClientMock;
		Mock<IErrorReportBuilder> errorReportBuilderMock;
		ErrorReportingClientWrapper wrapper;

		[Test]
		public void PostCrashReportForceReport()
		{
			try
			{
				var a = 0;
				var b = 1 / a;
			}
			catch (Exception ex)
			{
				var errorReport = BuildErrorReportToTest(ex, true);
				errorReportBuilderMock.Setup(x => x.BuildErrorReport(It.IsAny<Exception>(), null, null, It.IsAny<bool>())).
					Returns(errorReport);

				wrapper.PostCrashReport(ex, null, null);
				errorReportingClientMock.Verify(x => x.PostCrashReportAsync(errorReport, It.IsAny<CancellationToken>()), Times.Once);
			}
		}

		[TestCase("JNBCO-WLML-1", false, Description = "Debug should not report")]
		[TestCase("JNBCO-WMD2-1", false, Description = "Debug should not report")]
		[TestCase("SYDWP-SAPP-7", false, Description = "Debug should not report")]
		[TestCase("Server1", false, Description = "Debug should not report")]
		[TestCase("MYHOMEPC", false, Description = "Debug should not report")]
		public void BuildErrorReport(string machineName, bool expectedResult)
		{
			if (expectedResult)
			{
				Assert.IsNotNull(new ErrorReportBuilder().BuildErrorReport(new Exception("Test Exception"), null, null, true, machineName));
			}
			else
			{
				Assert.IsNull(new ErrorReportBuilder().BuildErrorReport(new Exception("Test Exception"), null, null, true, machineName));
			}
		}

		[Test]
		public void DoesNotReportSqlConnectionExceptions()
		{
			foreach (var message in ErrorReportingKnownExceptionsMapping.SQLConnectionExceptionMessages)
			{
				var exception = new Exception(message);
				var errorReport = BuildErrorReportToTest(exception);
				Assert.IsNull(errorReport);

				wrapper.PostCrashReport(exception, null, null);
				errorReportingClientMock.Verify(x => x.PostCrashReportAsync(errorReport, It.IsAny<CancellationToken>()), Times.Never);
			}
		}

		[Test]
		public void DoesNotReportSqlConnectionExceptions_InnerExceptions()
		{
			foreach (var message in ErrorReportingKnownExceptionsMapping.SQLConnectionExceptionMessages)
			{
				var exception = new Exception("An Error has occurred", new Exception(message));
				var errorReport = BuildErrorReportToTest(exception);
				Assert.IsNull(errorReport);

				wrapper.PostCrashReport(exception, null, null);
				errorReportingClientMock.Verify(x => x.PostCrashReportAsync(errorReport, It.IsAny<CancellationToken>()), Times.Never);
			}
		}

		[Test]
		public void DoesNotReportTestMessages()
		{
			var exception = new Exception(ErrorReportingKnownExceptionsMapping.ErrorReportingTestMessage);
			var errorReport = BuildErrorReportToTest(exception);
			Assert.IsNull(errorReport);
			wrapper.PostCrashReport(exception, null, null);
			errorReportingClientMock.Verify(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()), Times.Never);
		}

		IOpaqueErrorReport BuildErrorReportToTest(Exception exception, bool forceReport = false)
		{
			return new ErrorReportBuilder().BuildErrorReport(exception, null, null, true, "SYDWP -SAPP-7", forceReport);
		}

		[Test]
		public void XmlProducersShouldNotUseErrorReporting()
		{
			var errorReportingAssemblyName = "CargoWise.RefDbRepo.Common.ErrorReporting";
			var binPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..");
			var xmlProducerFolder = Path.Combine(binPath, "UniversalXMLProducers\\net8.0");
			var dllFiles = Directory.GetFiles(xmlProducerFolder, "CargoWise.RefDbRepo.*.dll");
			var xmlProducerDllFiles = dllFiles.Where(x => !x.Contains("CargoWise.RefDbRepo.Staging") && !x.Contains("CargoWise.RefDbRepo.Common"));
			foreach (var file in xmlProducerDllFiles)
			{
				var assembly = Assembly.LoadFrom(file);
				var referencedAssembly = assembly.GetReferencedAssemblies();
				Assert.False(referencedAssembly.Any(x => x.Name == errorReportingAssemblyName), $"{assembly.GetName().Name} should not reference {errorReportingAssemblyName}");
			}
		}

		[SetUp]
		public void SetUp()
		{
			errorReportBuilderMock = new Mock<IErrorReportBuilder>();
			errorReportingClientMock = new Mock<IErrorReportingClient>();

			errorReportingClientProviderMock = new Mock<IErrorReportingClientProvider>();
			errorReportingClientProviderMock.Setup(x => x.CreateClient(It.IsAny<Uri>(), null)).Returns(errorReportingClientMock.Object);

			wrapper = new ErrorReportingClientWrapper(errorReportingClientProviderMock.Object, errorReportBuilderMock.Object, null);
		}

		[TearDown]
		public void TearDown()
		{
			wrapper.Dispose();
		}
	}
}
