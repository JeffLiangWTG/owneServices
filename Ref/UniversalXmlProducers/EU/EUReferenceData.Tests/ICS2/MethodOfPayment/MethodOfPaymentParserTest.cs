using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Tests
{
	[TestFixture]
	class MethodOfPaymentDataParserTest : CommonXmlDataParserTest<MethodOfPaymentDataParser>
	{
		protected override Stream GetXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.MethodOfPayment.TestFiles.Input.RD_ICS2_TransportChargesMethodOfPayment.xml");
		}

		protected override Stream GetInvalidXMLFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.MethodOfPayment.TestFiles.Input.RD_ICS2_TransportChargesMethodOfPayment-Err2.xml");
		}

		protected override Stream GetRDEntityNotFoundErrorFile()
		{
			return TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.ICS2.MethodOfPayment.TestFiles.Input.RD_ICS2_TransportChargesMethodOfPayment-Err3.xml");
		}

		protected override string RDEntityAttributeValue => Constants.MethodOfPayment.RDEntityAttributeValue;
	}
}
