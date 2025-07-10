using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.CmdLine;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRatesProgramTest
	{
		[Test]
		public void TestDictionaryToText()
		{
			var downloadedContent = new Dictionary<string, string>
			{
				{ "Test", "TestContent" },
				{ "Test2", "TestContent2" }
			};
			Assert.That(ExchangeRatesProgramForTesting.DictionaryToTextExposed(downloadedContent), Is.EqualTo("TestContent,TestContent2"));
		}

		[Test]
		public void TestXmlGeneration()
		{
			var date = DateTime.Now;
			using (var sw = new StringWriter())
			using (var program = new ExchangeRatesProgramForTesting())
			{
				Console.SetOut(sw);
				Assert.DoesNotThrow(() => program.Run());

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder).OrderBy(s => s).ToArray();

				Assert.True(generatedFiles.Length == 2);
				Assert.True(generatedFiles[0].EndsWith("RefExchangeRateZZ_MX_CUE.xml"), "file 1 shoulb RefExchangeRateZZ_MX_CUE.xml");
				Assert.True(generatedFiles[1].EndsWith("RefExchangeRateZZ_MX_CUS.xml"), "file 0 shoulb RefExchangeRateZZ_MX_CUS.xml");
				Assert.That(sw.ToString(), Is.EqualTo("Xml file :RefExchangeRateZZ_MX_CUS.xml, generated.\r\nXml file :RefExchangeRateZZ_MX_CUE.xml, generated.\r\nFile Exchange Rates, exported with success!\r\n"));
			}
		}

		[Test]
		public void TestNoXmlGeneratedWhenNoChanges()
		{
			using (var program = new ExchangeRatesProgramForTesting())
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				Assert.DoesNotThrow(() => program.Run());

				var generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);
				Assert.True(generatedFiles.Length == 2);

				Directory.Delete(ConfigurationProvider.OutputFolder, true);
				Assert.False(Directory.Exists(ConfigurationProvider.OutputFolder));

				Assert.DoesNotThrow(() => program.Run());

				generatedFiles = Directory.GetFiles(ConfigurationProvider.OutputFolder);
				Assert.True(generatedFiles.Length == 0);
				Assert.That(sw.ToString(), Does.Contain("No update found on Exchange Rates!"));
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

		class ExchangeRatesProgramForTesting : ExchangeRatesProgram, IDisposable
		{
			public static string DictionaryToTextExposed(Dictionary<string, string> downloadedContent) => DictionaryToText(downloadedContent);

			protected override Dictionary<string, string> DowloadContent()
			{
				using (var client = GetConnection)
				{
					return ExchangeRatesDownloader.DownloadXmls(client);
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
					using (var inputINPCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_INPC.xml"))
					using (var inputDEPAISXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_DEPAIS.xml"))
					using (var inputRECARGXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_RECARG.xml"))
					using (var inputTIPCAMXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_TIPCAM.xml"))
					{
						var files = new[] { "CTARC_INPC.xml", "CTARC_DEPAIS.xml", "CTARC_RECARG.xml", "CTARC_TIPCAM.xml" };
						var filesStream = new[] { inputINPCXML, inputDEPAISXML, inputRECARGXML, inputTIPCAMXML };
						var fakeFtpClient = new Mock<IFtpClient>();
						for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
						{
							var outBytes = filesStream[fileIndex].Stream2ByteArray();
							fakeFtpClient.Setup(x => x.DownloadBytes(out outBytes, ConfigurationProvider.ExchangeRate.DownloadPath + files[fileIndex], 0, null, 0))
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
