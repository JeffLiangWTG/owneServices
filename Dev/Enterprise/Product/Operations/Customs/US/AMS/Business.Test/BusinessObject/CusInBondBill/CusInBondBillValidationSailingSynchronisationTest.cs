namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondBillValidationSailingSynchronisationTest : LinkedSailingBillsImportedTest
	{
		public void TestBillIsNotLinkedToSailing()
		{
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "SCAC";
			bill.B0_MasterBillNumber = "AAB";
			AssertHasWarning(bill.B0_MasterBillNumberInfo, ValidationConstants.SailingSynchronisation.BillMightBeIncorectlyAdded.ToString());
			bill.B0_MasterBillNumber = "AAA";
			AssertNoWarning(bill.B0_MasterBillNumberInfo, ValidationConstants.SailingSynchronisation.BillMightBeIncorectlyAdded.ToString());
		}
	}
}
