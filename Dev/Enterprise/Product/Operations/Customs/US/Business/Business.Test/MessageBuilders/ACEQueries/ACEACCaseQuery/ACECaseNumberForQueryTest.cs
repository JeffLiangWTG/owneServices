using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACECaseNumberForQuery))]
	sealed class ACECaseNumberForQueryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCaseNumberValidation()
		{
			var caseNumber = new ACECaseNumberForQuery();
			caseNumber.CaseNumber = "123";
			AssertHasMessageError(caseNumber.CaseNumberInfo, ACECaseNumberForQuery.CaseNoLength);

			caseNumber.CaseNumber = "1234567";
			AssertNoMessageError(caseNumber.CaseNumberInfo, ACECaseNumberForQuery.CaseNoLength);

			caseNumber.CaseNumber = "12345678";
			AssertNoMessageError(caseNumber.CaseNumberInfo, ACECaseNumberForQuery.CaseNoLength);
		}

		protected override BusinessObject GetNewBusinessObject() => new ACECaseNumberForQuery();
	}
}
