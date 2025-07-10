using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CensusWarningQueryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDates()
		{
			var censusWarningQuery = new CensusWarningQuery(Factory);
			censusWarningQuery.Validation.ValidateDateFrom();
			censusWarningQuery.Validation.ValidateDateTo();
			AssertHasErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DataIsMandatory);
			AssertNoErrorContaining(censusWarningQuery.DateToInfo, MandatoryValidation.MustBeEntered);

			censusWarningQuery.DateFrom = ZDateTime.Today.AddDays(2);
			censusWarningQuery.Validation.ValidateDateTo();
			AssertHasErrorContaining(censusWarningQuery.DateToInfo, MandatoryValidation.MustBeEntered);

			censusWarningQuery.DateTo = ZDateTime.Today;
			AssertNoErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DataIsMandatory);
			AssertNoErrorContaining(censusWarningQuery.DateToInfo, MandatoryValidation.MustBeEntered);

			AssertHasErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DateFromCannotBeGreaterThanDateTo);
			censusWarningQuery.DateFrom = ZDateTime.Today.AddDays(-1);
			AssertNoErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DateFromCannotBeGreaterThanDateTo);

			censusWarningQuery.DateFrom = ZDateTime.Today.AddDays(1);
			AssertHasWarningContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DateCannotBeInFuture);

			censusWarningQuery.DateFrom = ZDateTime.Today.AddDays(-1);
			AssertNoWarningContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.DateCannotBeInFuture);

			censusWarningQuery.EntryNumber = "00000456";
			AssertHasErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.SomeDataIsRedundant);

			censusWarningQuery.DateFrom = ZDateTime.Empty;
			AssertNoErrorContaining(censusWarningQuery.DateFromInfo, CensusWarningQueryValidation.SomeDataIsRedundant);

			censusWarningQuery.DateTo = ZDateTime.Today;
			AssertHasWarning(censusWarningQuery.DateToInfo, CensusWarningQueryValidation.DateToWillBeIgnored);

			censusWarningQuery.DateTo = ZDateTime.Empty;
			AssertNoWarning(censusWarningQuery.DateToInfo, CensusWarningQueryValidation.DateToWillBeIgnored);

			censusWarningQuery.DateFrom = ZDateTime.Today.AddDays(-31);
			censusWarningQuery.DateTo = ZDateTime.Today;
			AssertHasMessageErrorContaining(censusWarningQuery.DateToInfo, CensusWarningQueryValidation.DateRangeExceeded);
		}

		public void TestFilerCode()
		{
			var censusWarningQuery = new CensusWarningQuery(Factory);
			censusWarningQuery.EntryFilerCode = "XJ5";
			AssertNoErrorContaining(censusWarningQuery.EntryFilerCodeInfo, MandatoryValidation.MustBeEntered);

			censusWarningQuery.EntryFilerCode = "";
			AssertHasErrorContaining(censusWarningQuery.EntryFilerCodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestEntryNumber()
		{
			var censusWarningQuery = new CensusWarningQuery(Factory);
			censusWarningQuery.Validation.ValidateEntryNumber();
			AssertHasErrorContaining(censusWarningQuery.EntryNumberInfo, CensusWarningQueryValidation.DataIsMandatory);

			censusWarningQuery.DateFrom = ZDateTime.Today;
			AssertNoErrorContaining(censusWarningQuery.EntryNumberInfo, CensusWarningQueryValidation.DataIsMandatory);
		}

		public void TestDistrictPort()
		{
			var censusWarningQuery = new CensusWarningQuery(Factory);
			censusWarningQuery.Validation.ValidateDistrictPortCode();
			AssertHasErrorContaining(censusWarningQuery.DistrictPortCodeInfo, CensusWarningQueryValidation.DataIsMandatory);

			censusWarningQuery.EntryNumber = "00000123";
			AssertNoErrorContaining(censusWarningQuery.DistrictPortCodeInfo, CensusWarningQueryValidation.DataIsMandatory);

			censusWarningQuery.DistrictPortCode = "3901";
			AssertHasErrorContaining(censusWarningQuery.DistrictPortCodeInfo, CensusWarningQueryValidation.SomeDataIsRedundant);

			censusWarningQuery.DistrictPortCode = "";
			AssertNoErrorContaining(censusWarningQuery.DistrictPortCodeInfo, CensusWarningQueryValidation.SomeDataIsRedundant);
		}
	}
}
