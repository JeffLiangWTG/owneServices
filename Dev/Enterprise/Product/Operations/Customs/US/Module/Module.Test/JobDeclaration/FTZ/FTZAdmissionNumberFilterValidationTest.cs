using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class FTZAdmissionNumberFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestProperties()
		{
			var filter = new FTZAdmissionNumberFilter("Hello");
			filter.ZoneID = ZString.Empty;
			AssertNoError(filter.ZoneIDInfo, FTZAdmissionNumberFilterValidation.ZoneIDFormat);
			filter.ZoneID = "123";
			AssertHasError(filter.ZoneIDInfo, FTZAdmissionNumberFilterValidation.ZoneIDFormat);
			filter.ZoneID = "1234567";
			AssertNoError(filter.ZoneIDInfo, FTZAdmissionNumberFilterValidation.ZoneIDFormat);
			filter.ZoneID = "123456789";
			AssertNoError(filter.ZoneIDInfo, FTZAdmissionNumberFilterValidation.ZoneIDFormat);
			filter.Year = ZString.Empty;
			AssertNoError(filter.YearInfo, FTZAdmissionNumberFilterValidation.YearFormat);
			filter.Year = "1";
			AssertHasError(filter.YearInfo, FTZAdmissionNumberFilterValidation.YearFormat);
			filter.Year = "12";
			AssertNoError(filter.YearInfo, FTZAdmissionNumberFilterValidation.YearFormat);
			filter.ControlNumber = ZString.Empty;
			AssertNoError(filter.ControlNumberInfo, FTZAdmissionNumberFilterValidation.ControlNumberFormat);
			filter.ControlNumber = "223";
			AssertNoError(filter.ControlNumberInfo, FTZAdmissionNumberFilterValidation.ControlNumberFormat);
			filter.ControlNumber = "12345678";
			AssertNoError(filter.ControlNumberInfo, FTZAdmissionNumberFilterValidation.ControlNumberFormat);
			filter.ControlNumber = "1";
			AssertHasError(filter.ControlNumberInfo, FTZAdmissionNumberFilterValidation.ControlNumberFormat);
		}
	}
}
