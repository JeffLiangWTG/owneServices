using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	sealed class LoggerTest
	{
		[Test]
		public void TestAllLogs()
		{
			Assert.AreEqual(5, logger.AllLogs.Count);
		}

		[Test]
		public void TestInfoLogs()
		{
			Assert.AreEqual(1, logger.InfoLogs.Count);
			Assert.That(logger.InfoLogs.Single().Contains("Information"), Is.True);
		}

		[Test]
		public void TestDebugLogs()
		{
			Assert.AreEqual(1, logger.DebugLogs.Count);
			Assert.That(logger.DebugLogs.Single().Contains("Debug"), Is.True);
		}


		[Test]
		public void TestWarningLogs()
		{
			Assert.AreEqual(1, logger.WarningLogs.Count);
			Assert.That(logger.WarningLogs.Single().Contains("Warning"), Is.True);
		}


		[Test]
		public void TestErrorLogs()
		{
			Assert.AreEqual(2, logger.ErrorLogs.Count);
			Assert.That(logger.ErrorLogs.Single(r => r.Contains("Error1")).Contains("Error1"), Is.True);
			Assert.That(logger.ErrorLogs.Single(r => r.Contains("Error2:Error Test")).Contains("Error2:Error Test"), Is.True);
		}

		[Test]
		public void TestOutputAllLogs()
		{
			Assert.That(stringWriter.ToString().Contains("Information"), Is.True);
			Assert.That(stringWriter.ToString().Contains("Debug"), Is.True);
			Assert.That(stringWriter.ToString().Contains("Warning"), Is.True);
			Assert.That(consoleError.ToString().Contains("Error"), Is.True);
			Assert.That(consoleError.ToString().Contains("Error2:Error Test"), Is.True);
		}

		[OneTimeSetUp]
		public void Setup()
		{
			originalOutput = Console.Out;
			originalError = Console.Error;

			stringWriter = new StringWriter();
			consoleError = new StringWriter();

			Console.SetOut(stringWriter);
			Console.SetError(consoleError);

			logger = new Logger();
			logger.Log(LogType.Information, "Information");
			logger.Log(LogType.Debug, "Debug");
			logger.Log(LogType.Warning, "Warning");
			logger.Log(LogType.Error, "Error1");
			logger.Log(LogType.Error, "Error2", new System.Exception("Error Test"));
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			Console.SetOut(originalOutput);
			Console.SetError(originalError);

			stringWriter.Dispose();
			consoleError.Dispose();
		}

		Logger logger;
		StringWriter stringWriter;
		TextWriter originalOutput;
		StringWriter consoleError;
		TextWriter originalError;
	}
}
