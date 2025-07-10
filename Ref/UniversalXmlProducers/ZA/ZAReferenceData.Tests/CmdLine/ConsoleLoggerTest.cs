using System;
using System.IO;
using CargoWise.RefDbRepo.ZAReferenceData.CmdLine;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.CmdLine
{
	[TestFixture]
	class ConsoleLoggerTest
	{
		[Test]
		public void LogError()
		{
			using (var memStream = new MemoryStream())
			using (var memWriter = new StreamWriter(memStream))
			{
				memWriter.AutoFlush = true;
				Console.SetError(memWriter);

				ILogger logger = new ConsoleLogger();
				logger.LogError("Logging an error");
				logger.LogInfo("Logging some information");

				memStream.Position = 0;
				using (var strRead = new StreamReader(memStream))
				{
					var consoleOutput = strRead.ReadToEnd();
					Assert.That(consoleOutput, Is.EqualTo("Logging an error\r\n"));
				}
			}
		}

		[Test]
		public void LogInfo()
		{
			using (var memStream = new MemoryStream())
			using (var memWriter = new StreamWriter(memStream))
			{
				memWriter.AutoFlush = true;
				Console.SetOut(memWriter);

				ILogger logger = new ConsoleLogger();
				logger.LogInfo("Logging some information");
				logger.LogError("Logging an error");

				memStream.Position = 0;
				using (var strRead = new StreamReader(memStream))
				{
					var consoleOutput = strRead.ReadToEnd();
					Assert.That(consoleOutput, Is.EqualTo("Logging some information\r\n"));
				}
			}
		}

		[SetUp]
		public void SetUp()
		{
			defOut = Console.Out;
			defErr = Console.Error;
		}

		[TearDown]
		public void TearDown()
		{
			Console.SetOut(defOut);
			Console.SetError(defErr);
		}

		TextWriter defOut;
		TextWriter defErr;
	}
}
