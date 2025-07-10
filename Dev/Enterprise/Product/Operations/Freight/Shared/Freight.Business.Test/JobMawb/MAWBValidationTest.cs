using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MAWBValidationTest : TestCaseWithDummy
	{
		public void TestCheckMawbCheckDigit()
		{
			string lengthError = "Value should be 8 digits in length";
			string numberError = "Please enter only numerals";

			Value = "5555555";
			AssertHasError(ValueInfo, lengthError);
			AssertNoError(ValueInfo, numberError);

			Value = "55555555";
			AssertNoError(ValueInfo, lengthError);
			AssertNoError(ValueInfo, numberError);

			Value = "555555555";
			AssertHasError(ValueInfo, lengthError);
			AssertNoError(ValueInfo, numberError);

			Value = "aaaaaaaa";
			AssertNoError(ValueInfo, lengthError);
			AssertHasError(ValueInfo, numberError);

			Value = "55555554";
			AssertNoError(ValueInfo, lengthError);
			AssertNoError(ValueInfo, numberError);
			AssertHasError(ValueInfo, "Incorrect Check Digit. Last digit should be '5'");
		}

		#region Implementation

		#region Value

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in TestCheckMawbCheckDigit")]
		ZString Value
		{
			get { return Dummy.Z0_Description; }
			set
			{
				Dummy.Z0_Description = value;
				Dummy.Z0_DescriptionInfo.ClearAllNotifications();
				MAWBValidation.CheckMawbCheckDigit(Dummy.Z0_DescriptionInfo);
			}
		}

		ZPropertyInfo ValueInfo
		{
			get { return Dummy.Z0_DescriptionInfo; }
		}

		#endregion

		#region RunTest

		protected override void RunTest()
		{
			using (Dummy.SuspendValidationTesting())
			{
				base.RunTest();
			}
		}

		#endregion

		#endregion
	}
}
