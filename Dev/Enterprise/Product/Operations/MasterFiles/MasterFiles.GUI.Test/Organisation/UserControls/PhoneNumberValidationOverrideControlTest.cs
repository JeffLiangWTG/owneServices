using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class PhoneNumberValidationOverrideControlTest : TestCaseWithFactory
	{
		public void TestManualVerifyButton_Click()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var control = new PhoneNumberValidationOverrideControl(dummy.Z0_BoolInfo, "blah"))
			{
				Assert("Pre-conditon", !dummy.Z0_Bool);
				control.ManualVerifyButton.PerformClick();
				Assert(dummy.Z0_Bool);
			}
		}

		public void TestNotAcceptedReason()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var control = new PhoneNumberValidationOverrideControl(dummy.Z0_BoolInfo, "blah"))
			{
				var infoLabel = control.Controls.Find("InfoLabel", true);
				control.NotAcceptedReason = "";
				AssertEquals(infoLabel[0].Text, "blah" + System.Environment.NewLine + System.Environment.NewLine + "If you are very confident you can Accept as Entered.");

				control.NotAcceptedReason = "blahblah";
				AssertEquals(infoLabel[0].Text, "blah" + System.Environment.NewLine + System.Environment.NewLine + "blahblah");
			}
		}
	}
}
