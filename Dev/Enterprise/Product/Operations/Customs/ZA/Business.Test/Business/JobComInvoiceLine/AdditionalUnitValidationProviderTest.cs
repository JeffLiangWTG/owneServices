using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AdditionalUnitValidationProviderTest : TestCaseWithDummy
	{
		public void TestValidateDuplicateAdditionalUnit()
		{
			Dummy.Z0_Code = "U1";
			Dummy.Z0_VarCharMax = "U2";
			AssertNoMessageErrors("Precondition - no errors", Dummy.Z0_CodeInfo);
			AssertNoMessageErrors("Precondition - no errors", Dummy.Z0_VarCharMaxInfo);
			new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(Dummy.Z0_CodeInfo, Dummy.Z0_VarCharMaxInfo);
			AssertNoMessageErrors("Different code - no errors", Dummy.Z0_CodeInfo);
			AssertNoMessageErrors("Different - no errors", Dummy.Z0_VarCharMaxInfo);
			Dummy.Z0_VarCharMax = "U1";
			using (IDisposable token = Dummy.SuspendValidationTesting())
			{
				new AdditionalUnitValidationProvider().ValidateDuplicateAdditionalUnit(Dummy.Z0_CodeInfo, Dummy.Z0_VarCharMaxInfo);
			}

			AssertHasMessageError(Dummy.Z0_CodeInfo, "You have entered a duplicate unit code, U1");
			AssertNoMessageErrors("Shouldn't get error", Dummy.Z0_VarCharMaxInfo);
		}
	}
}
