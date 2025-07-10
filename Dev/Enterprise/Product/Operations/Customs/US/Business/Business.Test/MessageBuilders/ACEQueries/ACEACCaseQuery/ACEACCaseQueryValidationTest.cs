using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEACCaseQueryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_CompanyCaseStatus()
		{
			var query = new ACEACCaseQuery(Factory);
			query.US_CompanyCaseStatus = "";
			AssertHasMessageErrorContaining(query.US_CompanyCaseStatusInfo, MandatoryValidation.YouHaveNotEntered);

			query.US_CompanyCaseStatus = "~";
			AssertHasMessageError(query.US_CompanyCaseStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(query.US_CompanyCaseStatusInfo, MandatoryValidation.YouHaveNotEntered);

			query.US_CompanyCaseStatus = ACCaseStatusList.QueryMessageCodes.Both;
			AssertHasMessageError(query.US_CompanyCaseStatusInfo, ACEACCaseQueryValidation.EnterAnotherCriteria);
			AssertNoMessageError(query.US_CompanyCaseStatusInfo, ListValidation.InvalidCodeMessageError);

			query.US_CountryCode = "AU";
			AssertNoMessageError(query.US_CompanyCaseStatusInfo, ACEACCaseQueryValidation.EnterAnotherCriteria);

			query.CaseNumbers.AddNew();
			query.CaseNumbers[0].CaseNumber = "12456";

			query.Validation.ValidateUS_CompanyCaseStatus();
			AssertHasMessageError(query.US_CompanyCaseStatusInfo, ACEACCaseQueryValidation.CaseNumbersExist);

			query.US_CompanyCaseStatus = "";
			AssertNoMessageError(query.US_CompanyCaseStatusInfo, ACEACCaseQueryValidation.CaseNumbersExist);
			AssertNoMessageErrorContaining(query.US_CompanyCaseStatusInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_HTSNumber()
		{
			var query = new ACEACCaseQuery(Factory);
			query.US_HTSNumber = "000000";
			AssertHasMessageError(query.US_HTSNumberInfo, ACEACCaseQueryValidation.TariffNumberShouldBe8Or10);

			query.US_HTSNumber = "0000000000";
			AssertNoMessageError(query.US_HTSNumberInfo, ACEACCaseQueryValidation.TariffNumberShouldBe8Or10);
			AssertHasMessageError(query.US_HTSNumberInfo, ACEACCaseQueryValidation.NoTariffWithThisNumber);

			query.US_HTSNumber = USCTariff.AGOABenefitsApplicable;
			AssertNoMessageError(query.US_HTSNumberInfo, ACEACCaseQueryValidation.NoTariffWithThisNumber);
		}

		[NUnit.Framework.TestDate(2009, 4, 10)]
		public void TestCheckUS_DateSinceLastUpdate()
		{
			var query = new ACEACCaseQuery(Factory);
			query.US_DateSinceLastUpdate = new ZDate(2009, 4, 3);
			AssertHasMessageError(query.US_DateSinceLastUpdateInfo, ACEACCaseQueryValidation.MustBeWithin7Days);

			query.US_DateSinceLastUpdate = new ZDate(2009, 4, 4);
			AssertNoMessageError(query.US_DateSinceLastUpdateInfo, ACEACCaseQueryValidation.MustBeWithin7Days);

			query.US_DateSinceLastUpdate = new ZDate(2009, 4, 11);
			AssertHasMessageError(query.US_DateSinceLastUpdateInfo, ACEACCaseQueryValidation.MustBeWithin7Days);

			query.US_DateSinceLastUpdate = ZDate.Empty;
			AssertNoMessageError(query.US_DateSinceLastUpdateInfo, ACEACCaseQueryValidation.MustBeWithin7Days);
		}
	}
}
