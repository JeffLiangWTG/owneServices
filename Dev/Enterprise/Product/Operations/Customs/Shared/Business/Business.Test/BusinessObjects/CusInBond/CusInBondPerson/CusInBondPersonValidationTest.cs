using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondPersonValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCP_DateOfBirthIsValidZDateTimeRange()
		{
			var person = Factory.New<CusInBondPersonForTesting>();
			person.CP_DateOfBirth = ZDateTime.Today.AddDays(1);
			AssertHasError(person.CP_DateOfBirthInfo, "Date Of Birth cannot be a future date.");
			person.CP_DateOfBirth = ZDateTime.Today.AddYears(-50);
			AssertNoNotifications(person.CP_DateOfBirthInfo);
		}

		public void TestCheckCP_GS_NKStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			var person = Factory.New<CusInBondPersonForTesting>();
			ValidationTestHelper.AssertErrorIfInvalidCode(person.CP_GS_NKStaffInfo, "??", staff.GS_Code);
		}
	}
}
