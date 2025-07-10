using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZLineKeyGeneratorTest : TestCaseWithFactory
	{
		public void TestLineKeyGenerator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "M1";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = bill.PK;

			var org = Factory.New<OrgHeader>();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.US_SupTariff = "2";
			invoiceLine.US_SPI = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SecondarySPI = "X";
			invoiceLine.US_TextileCategoryNo = "383";
			invoiceLine.US_F_PNDisclaimer = "Y";
			invoiceLine.US_ZoneStatus = "Z";
			invoiceLine.JI_OA_ManufacturerAddress = org.MainAddress.PK;

			var keys = new MergeKey();
			new FTZLineKeyGenerator().AddKey(keys, invoiceLine);

			Assert(keys.Contains(bill.CU_BillUniqueCode));
			Assert(keys.Contains(invoiceLine.JI_Tariff));
			Assert(keys.Contains(invoiceLine.US_SPI));
			Assert(keys.Contains(invoiceLine.US_UC_NKCountryOfOrigin));
			Assert(keys.Contains(invoiceLine.US_SecondarySPI));
			Assert(keys.Contains(invoiceLine.US_TextileCategoryNo));
			Assert(keys.Contains(invoiceLine.US_F_PNDisclaimer));
			Assert(keys.Contains(invoiceLine.US_ZoneStatus));
			Assert(keys.Contains(invoiceLine.JI_OA_ManufacturerAddress));
		}
	}
}
