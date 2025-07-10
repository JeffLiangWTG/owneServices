using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DataTransfer.Testing
{
	sealed class InvoiceXmlValueObjectSerializerTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdateFromValueObject()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			organisation.OH_FullName = "Supplier";
			organisation.OH_IsConsignee = true;
			organisation.OH_IsConsignor = true;
			organisation.OH_RL_NKClosestPort = "AUMEL";
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 10m;
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 20m;
			organisation.MiscServ.OM_LandedCostMarginPercent1 = 30m;

			var address = organisation.Addresses.MainAddress;
			address.OA_Address1 = "7J1B7RFZ4WSH3LJTGUG7P55D1ABG3TMYMKFCPHQ2VSK4UDXQF0";
			organisation.OH_Code = "L1FZUT4BYOD8";

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_InvoiceNumber = "INVOICENUMBER";
			invoice.JZ_OH_Supplier = organisation.PK;
			invoice.JZ_Weight = 200m;
			invoice.JZ_WeightUQ = "KG";

			//different supplier
			var invoice2 = Factory.New<BaseJobComInvoiceHeader>();
			invoice2.JZ_InvoiceNumber = "INVOICENUMBER2";
			invoice2.JZ_OH_Supplier = organisation.PK;
			invoice2.JZ_Weight = 200m;
			invoice2.JZ_WeightUQ = "KG";

			//for different company
			var company = Factory.New<GlbCompany>();
			company.FillWithValidTestData();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			var invoice3 = Factory.New<BaseJobComInvoiceHeader>();
			invoice3.JZ_InvoiceNumber = "INVOICENUMBER";
			invoice3.JZ_OH_Supplier = organisation.PK;
			invoice3.JZ_GB = branch.PK;
			invoice3.JZ_Weight = 200m;
			invoice3.JZ_WeightUQ = "KG";

			Factory.Save();

			var filename = TestFileHelper.GetPathForTesting("PopulatedStandAloneInvoiceIncludingRootName.xml");
			new InvoiceDataTransferImporter(StandAloneInvoiceValueObjectDataAdapter.New(), false).Import(filename, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			AssertEquals("weight should have been udpated for invoice1 as to xml file", 168m, invoice.JZ_Weight);
			AssertEquals("weight should have been udpated for invoice1", 200m, invoice2.JZ_Weight);
			AssertEquals("weight should have been udpated for invoice1", 200m, invoice3.JZ_Weight);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testFileHelper?.Dispose();
			testFileHelper = null;
		}

		TestFileHelper TestFileHelper => testFileHelper ??= new ();
		TestFileHelper testFileHelper;
	}
}
