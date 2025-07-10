using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ReceiptNumberHolder))]
	sealed class ReceiptNumberHolderValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckReceiptNumber()
		{
			VAT404TestHelper.SetupPayInfo(Factory);
			var testerParent = new VAT404DocumentInstruction(Factory);
			var tester1 = testerParent.ReceiptNumbersForFilter.AddNew();
			tester1.ReceiptNumber = "123";
			var tester2 = testerParent.ReceiptNumbersForFilter.AddNew();
			tester2.ReceiptNumber = "";
			var tester3 = testerParent.ReceiptNumbersForFilter.AddNew();
			tester3.ReceiptNumber = "123";
			var tester4 = testerParent.ReceiptNumbersForFilter.AddNew();
			tester4.ReceiptNumber = "1";
			AssertNoErrors(tester1.ReceiptNumberInfo);
			AssertNoMessageErrors(tester1.ReceiptNumberInfo);
			AssertNoWarnings(tester1.ReceiptNumberInfo);
			AssertNoErrors(tester2.ReceiptNumberInfo);
			AssertNoMessageErrors(tester2.ReceiptNumberInfo);
			AssertHasWarningContaining(tester2.ReceiptNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrors(tester3.ReceiptNumberInfo);
			AssertNoMessageErrors(tester3.ReceiptNumberInfo);
			AssertHasWarningContaining(tester3.ReceiptNumberInfo, "This value has already been entered");
			AssertNoErrors(tester4.ReceiptNumberInfo);
			AssertNoMessageErrors(tester4.ReceiptNumberInfo);
			AssertNoWarnings(tester4.ReceiptNumberInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = new VAT404DocumentInstruction(Factory);
			return new ReceiptNumberHolder(instruction);
		}
	}
}
