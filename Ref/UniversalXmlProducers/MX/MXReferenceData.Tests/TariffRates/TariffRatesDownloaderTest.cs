using System.Collections.Generic;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	sealed class TariffRatesDownloaderTest
	{
		[Test]
		public void TestDownloadXmls()
		{
			using (var inputFRACCIXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTARC_FRACCI.xml"))
			using (var inputFRACCIDXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTARC_FRACCID.xml"))
			using (var inputCTRAC_FRACCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Input.CTRAC_FRACC.xml"))
			using (var outputFRACCIXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Output.CTARC_FRACCI.xml"))
			using (var outputFRACCIDXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Output.CTARC_FRACCID.xml"))
			using (var outputFRACCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.TariffRates.TestFiles.Output.CTRAC_FRACC.xml"))
			{
				var files = new[] { "CTARC_FRACCI.xml", "CTARC_FRACCID.xml", "CTRAC_FRACC.xml" };
				var filesStream = new[] { inputFRACCIXML, inputFRACCIDXML, inputCTRAC_FRACCXML };
				var fakeFtpClient = new Mock<IFtpClient>();
				for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
				{
					var outBytes = filesStream[fileIndex].Stream2ByteArray();
					fakeFtpClient.Setup(x => x.DownloadBytes(out outBytes, ConfigurationProvider.TariffRate.DownloadPath + files[fileIndex], 0, null, 0))
					.Returns(() => true);
				}
				fakeFtpClient.Setup(x => x.IsConnected)
					.Returns(true);

				var xmls = TariffRatesDownloader.DownloadXmls(fakeFtpClient.Object);
				var outputXmls = new Dictionary<string, string>
				{
					{ "CTARC_FRACCI.xml", outputFRACCIXML.Stream2String() },
					{ "CTARC_FRACCID.xml", outputFRACCIDXML.Stream2String() },
					{ "CTRAC_FRACC.xml", outputFRACCXML.Stream2String() }
				};

				Assert.True(xmls.Count == 3);

				foreach (var xml in xmls)
				{
					Assert.That(outputXmls[xml.Key], Is.EqualTo(xml.Value), $"xml {xml.Key} should be equal");
				}
			}
		}
	}
}
