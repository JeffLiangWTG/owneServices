using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Tests
{
	[TestFixture]
	class MethodOfPaymentXMLProducerTest : CommonXmlProducerTest<MethodOfPaymentXMLProducer>
	{
		protected override string DownloadUrl => ApplicationConfig.Instance.TransportChargesMethodOfPaymentUrl;

		protected override Stream ZipFileForTest => TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.MethodOfPayment.TestFiles.Input.RD_ICS2_TransportChargesMethodOfPayment.zip");

		protected override string OutputFileName => "EUICS2_TransportMethodOfPayment.xml";

		protected override string ExpectedXML => TestHelper.ReadManifestResourceContentAsString($"CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.MethodOfPayment.TestFiles.Output.EUICS2_TransportMethodOfPayment.xml");
	}
}
