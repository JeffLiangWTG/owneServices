using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDeclarationCustomsChargesTest : TestCaseWithFactory
	{
		public void TestReconDeclarationCustomsCharges()
		{
			ReconDeclaration reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			reconDeclaration.InvoiceLines[0].JI_LinePrice = 4000m; //more duty to pay
			reconDeclaration.CalculateDutyFeesForChangedEntries();
			CustomsCharge[] customsCharges = ((ICustomsCharges)new ReconDeclarationCustomsCharges(reconDeclaration)).GetCustomsCharges(null);
			AssertEquals("One charge expected", 1, customsCharges.Length);
			AssertEquals("Duty", customsCharges[0].Description);
			AssertEquals("Duty Amount should be ReconDuty - OriginalDuty", 15m, customsCharges[0].Amount);
			AssertEquals("However it is not paid by broker", false, customsCharges[0].IsPaidByBroker);
			reconDeclaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			customsCharges = ((ICustomsCharges)new ReconDeclarationCustomsCharges(reconDeclaration)).GetCustomsCharges(null);
			AssertEquals("One charge expected", 1, customsCharges.Length);
			AssertEquals("Duty", customsCharges[0].Description);
			AssertEquals("Duty Amount should be 'ReconDuty - OriginalDuty'", 15m, customsCharges[0].Amount);
			AssertEquals("It is now paid by broker", true, customsCharges[0].IsPaidByBroker);
		}
	}
}
