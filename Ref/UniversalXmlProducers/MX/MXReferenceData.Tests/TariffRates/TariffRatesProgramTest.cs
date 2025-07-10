using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.CmdLine;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	sealed class TariffRatesProgramTest
	{
		[Test]
		public void TestDictionaryToText()
		{
			var downloadedContent = new Dictionary<string, string>
			{
				{ "Test", "TestContent" },
				{ "Test2", "TestContent2" }
			};
			Assert.That(TariffRatesProgramForTesting.DictionaryToTextExposed(downloadedContent), Is.EqualTo("TestContent,TestContent2"));
		}

		[Test]
		public void TestXmlGeneration()
		{
			using (var sw = new StringWriter())
			using (var program = new TariffRatesProgramForTesting())
			{
				Console.SetOut(sw);
				Assert.DoesNotThrow(() => program.Run());

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);

				Assert.True(generatedFiles.Length == 3);
				Assert.True(generatedFiles[0].EndsWith("CTARC_FRACCI.xml"), "file 0 should be CTARC_FRACCI.xml");
				Assert.True(generatedFiles[1].EndsWith("CTARC_FRACCID.xml"), "file 1 should be CTARC_FRACCID.xml");
				Assert.True(generatedFiles[2].EndsWith("CTRAC_FRACC.xml"), "file 2 should be CTARC_FRACC.xml");
				Assert.That(sw.ToString(), Is.EqualTo("Xml file: CTARC_FRACCI.xml, generated.\r\nXml file: CTARC_FRACCID.xml, generated.\r\nXml file: CTRAC_FRACC.xml, generated.\r\nFile Tariff Rates, exported with success!\r\n"));
			}
		}

		[Test]
		public void TestNoXmlGeneratedWhenNoChanges()
		{
			using (var program = new TariffRatesProgramForTesting())
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				Assert.DoesNotThrow(() => program.Run());

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);
				Assert.True(generatedFiles.Length == 3);

				Directory.Delete(ConfigurationProvider.OutputFolder, true);
				Assert.False(Directory.Exists(ConfigurationProvider.OutputFolder));

				Assert.DoesNotThrow(() => program.Run());

				generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);
				Assert.True(generatedFiles.Length == 0);
				Assert.That(sw.ToString(), Does.Contain("No update found on Tariff Rates!"));
			}
		}

		[SetUp]
		[TearDown]
		public void Setup()
		{
			if (Directory.Exists(ConfigurationProvider.OutputFolder))
			{
				Directory.Delete(ConfigurationProvider.OutputFolder, true);
			}
		}

		class TariffRatesProgramForTesting : TariffRatesProgram, IDisposable
		{
			public static string DictionaryToTextExposed(Dictionary<string, string> downloadedContent) => DictionaryToText(downloadedContent);

			protected override Dictionary<string, string> DowloadContent()
			{
				using (var client = GetConnection)
				{
					return TariffRatesDownloader.DownloadXmls(client);
				}
			}

			public void Dispose()
			{
				DeleteLogFile();
			}

			protected static new IFtpClient GetConnection
			{
				get
				{
					using (var inputFRACCIXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTARC_FRACCI.xml"))
					using (var inputFRACCIDXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTARC_FRACCID.xml"))
					using (var inputFRACCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTRAC_FRACC.xml"))
					{
						var files = new[] { "CTARC_FRACCI.xml", "CTARC_FRACCID.xml", "CTRAC_FRACC.xml" };
						var filesStream = new[] { inputFRACCIXML, inputFRACCIDXML, inputFRACCXML };
						var fakeFtpClient = new Mock<IFtpClient>();
						for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
						{
							var outBytes = filesStream[fileIndex].Stream2ByteArray();
							fakeFtpClient.Setup(x => x.DownloadBytes(out outBytes, ConfigurationProvider.TariffRate.DownloadPath + files[fileIndex], 0, null, 0))
							.Returns(() => true);
						}
						fakeFtpClient.Setup(x => x.IsConnected)
							.Returns(true);

						return fakeFtpClient.Object;
					}
				}
			}
		}
	}
}
