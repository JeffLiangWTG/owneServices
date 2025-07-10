using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common.TestClasses;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.ExchangeRates
{
	[TestFixture]
	sealed class ExchangeRateProcessorTests
	{
		[Test]
		public void Run()
		{
			var msg1PK = new Guid("22300000000000000000000000000003");
			var msg2PK = new Guid("22300000000000000000000000000004");

			var folder = TempFolder;

			var inputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(inputFolder);

			File.WriteAllText(Path.Combine(inputFolder, "RubbishFile.txt"), "This is not a GESMES FILE");
			File.WriteAllText(Path.Combine(inputFolder, "ValidFile.txt"), TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Valid_01.txt"));

			var files = Directory.GetFiles(folder);
			Assert.That(files, Is.EquivalentTo(new string[0]));

			var logger = new TestLogger();
			using (var messageHandler = ObjFactoryForTest.GetMessageHandler(inputFolder, string.Empty, logger, SupportedMessageTypes.Gesmes))
			{
				var tariffProcessor = new ExchangeRateProcessor(messageHandler, logger);
				tariffProcessor.Run(folder);

				files = Directory.GetFiles(folder);
				Assert.That(files, Is.Not.Null.And.Not.Empty);
				Assert.That(files.Count, Is.EqualTo(2));
				Assert.That(logger.ErrorString, Contains.Substring($"Validation errors for: {msg2PK}"));
				Assert.That(logger.ErrorString, Does.Not.Contain($"Validation errors for: {msg1PK}"));

				var fileContent = File.ReadAllText(files[0]);
				Assert.That(fileContent, Contains.Substring("<ZZN_RX_NKExCurrency>AUD</ZZN_RX_NKExCurrency>"));
				Assert.That(fileContent, Contains.Substring("<PublicationTime>2022-03-03T13:14:16</PublicationTime>"));

				var msgHandler = messageHandler as MessageHandlerForTest;
				Assert.That(msgHandler, Is.Not.Null);
				Assert.That(msgHandler.StatusUpdates, Is.Not.Null);
				Assert.That(msgHandler.StatusUpdates.ContainsKey(msg1PK));
				Assert.That(msgHandler.StatusUpdates[msg1PK], Is.EqualTo("MER"));
				Assert.That(msgHandler.StatusUpdates.ContainsKey(msg2PK));
				Assert.That(msgHandler.StatusUpdates[msg2PK], Is.EqualTo("ERR"));

				Assert.That(logger.InfoString, Contains.Substring("Processing message: 22300000-0000-0000-0000-000000000004 "));
				Assert.That(logger.InfoString, Contains.Substring("Processing message: 22300000-0000-0000-0000-000000000003 "));
				Assert.That(logger.InfoString, Contains.Substring("Processing message: 00000000-0000-0000-0000-000000000000 ValidFile.txt"));
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			if (!Directory.Exists(TempFolder))
			{
				Directory.CreateDirectory(TempFolder);
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}
		string TempFolder;
	}
}
