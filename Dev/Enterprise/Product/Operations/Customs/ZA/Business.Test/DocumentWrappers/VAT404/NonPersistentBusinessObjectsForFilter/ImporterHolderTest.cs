using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ImporterHolder))]
	sealed class ImporterPKHolderValidationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckImporterPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var testerParent = new VAT404DocumentInstruction(Factory);
			var tester1 = testerParent.ImporterForFilter.AddNew();
			tester1.ImporterPK = org1.PK;
			var tester2 = testerParent.ImporterForFilter.AddNew();
			tester2.ImporterPK = ZGuid.Empty;
			var tester3 = testerParent.ImporterForFilter.AddNew();
			tester3.ImporterPK = org1.PK;
			var tester4 = testerParent.ImporterForFilter.AddNew();
			tester4.ImporterPK = org2.PK;
			AssertNoErrors(tester1.ImporterPKInfo);
			AssertNoMessageErrors(tester1.ImporterPKInfo);
			AssertNoWarnings(tester1.ImporterPKInfo);
			AssertNoErrors(tester2.ImporterPKInfo);
			AssertNoMessageErrors(tester2.ImporterPKInfo);
			AssertHasWarningContaining(tester2.ImporterPKInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrors(tester3.ImporterPKInfo);
			AssertNoMessageErrors(tester3.ImporterPKInfo);
			AssertHasWarningContaining(tester3.ImporterPKInfo, "This value has already been entered");
			AssertNoErrors(tester4.ImporterPKInfo);
			AssertNoMessageErrors(tester4.ImporterPKInfo);
			AssertNoWarnings(tester4.ImporterPKInfo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = new VAT404DocumentInstruction(Factory);
			return new ImporterHolder(instruction);
		}
	}
}
