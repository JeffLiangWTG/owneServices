using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(LocalReferenceNumberHolder))]
	sealed class LocalReferenceNumberHolderValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckLocalReferenceNumber()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			Factory.Save();
			VAT404TestHelper.SetupPayInfo(Factory);
			var testerParent = new VAT404DocumentInstruction(Factory);
			var tester1 = testerParent.LocalReferenceNumbersForFilter.AddNew();
			tester1.LocalReferenceNumber = "00505655JSA20160101000001";
			var tester2 = testerParent.LocalReferenceNumbersForFilter.AddNew();
			tester2.LocalReferenceNumber = "";
			var tester3 = testerParent.LocalReferenceNumbersForFilter.AddNew();
			tester3.LocalReferenceNumber = "00505655JSA20160101000001";
			var tester4 = testerParent.LocalReferenceNumbersForFilter.AddNew();
			tester4.LocalReferenceNumber = "1";
			AssertNoErrors(tester1.LocalReferenceNumberInfo);
			AssertNoMessageErrors(tester1.LocalReferenceNumberInfo);
			AssertNoWarnings(tester1.LocalReferenceNumberInfo);
			AssertNoErrors(tester2.LocalReferenceNumberInfo);
			AssertNoMessageErrors(tester2.LocalReferenceNumberInfo);
			AssertHasWarningContaining(tester2.LocalReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrors(tester3.LocalReferenceNumberInfo);
			AssertNoMessageErrors(tester3.LocalReferenceNumberInfo);
			AssertHasWarningContaining(tester3.LocalReferenceNumberInfo, "This value has already been entered");
			AssertNoErrors(tester4.LocalReferenceNumberInfo);
			AssertHasMessageErrorContaining(tester4.LocalReferenceNumberInfo, ValidationConstants.Shared.LRNFormatInvalidSize);
			AssertNoWarnings(tester4.LocalReferenceNumberInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = new VAT404DocumentInstruction(Factory);
			return new LocalReferenceNumberHolder(instruction);
		}
	}
}
