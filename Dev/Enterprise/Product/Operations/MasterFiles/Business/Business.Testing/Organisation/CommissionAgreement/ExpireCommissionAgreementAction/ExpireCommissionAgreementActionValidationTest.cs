using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExpireCommissionAgreementActionValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2002, 2, 2)]
		public void TestCheckDate()
		{
			var action = new ExpireCommissionAgreementAction(Enumerable.Empty<OrgCommissionAgreement>());
			action.Date = ZDateTime.Empty;

			AssertMandatoryValidationError(action.DateInfo, true);
			AssertMandatoryValidationError(action.DateLocalInfo, true);

			action.Date = new ZDateTime(2001, 1, 1);

			AssertMandatoryValidationError(action.DateInfo, false);
			AssertMandatoryValidationError(action.DateLocalInfo, false);

			action.Date = new ZDateTime(1901, 1, 1);
			AssertNoErrors("Should be no error having a very old date", action.DateInfo);
			AssertNoErrors("Should be no error having a very old date", action.DateLocalInfo);
		}
	}
}
