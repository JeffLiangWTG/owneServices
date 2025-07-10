using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportLicensingMessageGoodsShipmentConsignment))]
	sealed class NX401ExportLicensingMessageGoodsShipmentConsignmentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBorderTransportMeansType()
		{
			NUnit.Framework.Assert.That(consignment.BorderTransportMeans, NUnit.Framework.Is.TypeOf<NX401ExportGoodsShipmentConsignmentBorderTransportMeans>());
		}

		[ExpectNoExceptions]
		public void TestTransportContractDocuments()
		{
			declaration.JE_TransportMode = "AIR";
			declaration.JE_MasterBill = "M01";
			declaration.JE_HouseBill = "H01";
			NUnit.Framework.Assert.That(consignment.TransportContractDocuments, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ITransportContractDocument>)), "EXP: NULL - should be [null]");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			consignment = new NX401ExportLicensingMessageGoodsShipmentConsignment(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX401ExportLicensingMessageGoodsShipmentConsignment consignment;
	}
}
