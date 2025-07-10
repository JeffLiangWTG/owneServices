using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDG_UNNO()
		{
			var expectedErrorMessage = MandatoryValidation.MustBeEnteredMessage(substance.DG_UNNOInfo.HumanReadableName);
			CombineAssertions(() =>
			{
				substance.DG_UNNO = string.Empty;
				AssertHasError("No Unno", substance.DG_UNNOInfo, expectedErrorMessage);
				substance.DG_UNNO = "8000";
				AssertNoError("Has Unno", substance.DG_UNNOInfo, expectedErrorMessage);
			});
		}

		public void TestCheckDG_PSN()
		{
			var expectedErrorMessage = MandatoryValidation.MustBeEnteredMessage(substance.DG_PSNInfo.HumanReadableName);
			CombineAssertions(() =>
			{
				substance.DG_PSN = string.Empty;
				AssertHasError("No PSN", substance.DG_PSNInfo, expectedErrorMessage);
				substance.DG_PSN = "Some Subs";
				AssertNoError("Has PSN", substance.DG_PSNInfo, expectedErrorMessage);
			});
		}

		public void TestCheckDG_Class()
		{
			var expectedErrorMessage = MandatoryValidation.MustBeEnteredMessage(substance.DG_ClassInfo.HumanReadableName);
			CombineAssertions(() =>
			{
				substance.DG_Class = string.Empty;
				AssertHasError("No Class", substance.DG_ClassInfo, expectedErrorMessage);
				substance.DG_Class = "2.1";
				AssertNoError("Has Class", substance.DG_ClassInfo, expectedErrorMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			substance = Factory.New<UNDGSubstance>();
		}
		UNDGSubstance substance;
	}
}
