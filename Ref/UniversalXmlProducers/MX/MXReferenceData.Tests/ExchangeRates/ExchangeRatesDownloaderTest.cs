using System.Collections.Generic;
using System.Reflection;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using FluentFTP;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRatesDownloaderTest
	{
		[Test]
		public void TestDownloadXmls()
		{
			using (var inputINPCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_INPC.xml"))
			using (var inputDEPAISXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_DEPAIS.xml"))
			using (var inputRECARGXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_RECARG.xml"))
			using (var inputTIPCAMXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Input.CTARC_TIPCAM.xml"))
			using (var outputINPCXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.CTARC_INPC.xml"))
			using (var outputDEPAISXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.CTARC_DEPAIS.xml"))
			using (var outputRECARGXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.CTARC_RECARG.xml"))
			using (var outputTIPCAMXML = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.ExchangeRates.TestFiles.Output.CTARC_TIPCAM.xml"))
			{
				var files = new[] { "CTARC_INPC.xml", "CTARC_DEPAIS.xml", "CTARC_RECARG.xml", "CTARC_TIPCAM.xml" };
				var filesStream = new[] { inputINPCXML, inputDEPAISXML, inputRECARGXML, inputTIPCAMXML };
				var fakeFtpClient = new Mock<IFtpClient>();
				for (var fileIndex = 0;fileIndex < files.Length; fileIndex++)
				{
					var outBytes = filesStream[fileIndex].Stream2ByteArray();
					fakeFtpClient.Setup(x => x.DownloadBytes(out outBytes, ConfigurationProvider.ExchangeRate.DownloadPath + files[fileIndex], 0, null, 0))
					.Returns(() => true);
				}
				fakeFtpClient.Setup(x => x.IsConnected)
					.Returns(true);

				var xmls = ExchangeRatesDownloader.DownloadXmls(fakeFtpClient.Object);
				var outputXmls = new Dictionary<string, string>
				{
					{ "CTARC_INPC.xml", outputINPCXML.Stream2String() },
					{ "CTARC_DEPAIS.xml", outputDEPAISXML.Stream2String() },
					{ "CTARC_RECARG.xml", outputRECARGXML.Stream2String() },
					{ "CTARC_TIPCAM.xml", outputTIPCAMXML.Stream2String() }
				};

				Assert.True(xmls.Count == 4);

				foreach (var xml in xmls)
				{
					Assert.That(outputXmls[xml.Key], Is.EqualTo(xml.Value), $"xml {xml.Key} should be equal");
				}
			}
		}
	}
}
