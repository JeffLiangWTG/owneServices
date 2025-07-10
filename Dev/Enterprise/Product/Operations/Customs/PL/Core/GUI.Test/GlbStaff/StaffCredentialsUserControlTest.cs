using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(StaffCredentialsUserControl))]
sealed class StaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
{
	public void TestControlsVisibility()
	{
		CombineAssertions(() =>
		{
			RunForControl(control =>
			{
				AssertEquals("PUESCLoginTextBox should be visible", true, control.PUESCLoginTextBox.Visible);
				AssertEquals("PUESCPasswordTextBox should be visible", true,
					control.PUESCPasswordTextBox.Visible);
				AssertEquals("SeapIdTextBox should be visible", true, control.SeapIdTextBox.Visible);
				AssertEquals("CommunicationChannelEmailTextBox should be visible", true, control.CommunicationChannelEmailTextBox.Visible);
			});

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSPhase4, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, true))
			{
				RunForControl(control =>
				{
					AssertEquals("SeapIdTextBox not should be visible", false, control.SeapIdTextBox.Visible);
				});
			}
		});
	}

	public void TestPUESCPasswordCharacterCasing()
	{
		RunForControl(control =>
		{
			AssertEquals("Password allows lower/upper case characters", System.Windows.Forms.CharacterCasing.Normal, control.PUESCPasswordTextBox.CharacterCasing);
		});
	}

	void RunForControl(Action<StaffCredentialsUserControl> methodToRun)
	{
		using (var form = this.GetFormToBash())
		using (var control = new StaffCredentialsUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			methodToRun.Invoke(control);
		}
	}
}
