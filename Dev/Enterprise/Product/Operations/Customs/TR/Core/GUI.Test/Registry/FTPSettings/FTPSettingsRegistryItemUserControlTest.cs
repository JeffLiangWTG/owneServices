using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(FTPSettingsRegistryItemUserControl))]
	sealed class FTPSettingsRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new FTPSettings(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var userControl = (FTPSettingsRegistryItemUserControl)control;
			return control.FindSingleOrDefault<ZDropEdit>(x => x.Name == "FTPAddressDropEdit").ReadOnly &&
				control.FindSingleOrDefault<ZCalcEdit>(x => x.Name == "FTPPortCalcEdit").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "FTPInTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "FTPOutTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "ExportUnionUserCodeTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "ExportUnionUserPasswordTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "ExportUnionPaymentPasswordTextBox").ReadOnly;
		}

		public void TestFTPPortCalcEdit_TextChanged_RemovesDots()
		{
			using (var userControl = new FTPSettingsRegistryItemUserControl())
			{
				userControl.FTPPortCalcEdit.Text = "2.435";
				userControl.ZCalc_EditValueChanged(userControl.FTPPortCalcEdit, EventArgs.Empty);

				AssertEquals("2435", userControl.FTPPortCalcEdit.Text);

				userControl.FTPPortCalcEdit.Text = "2,435";
				userControl.ZCalc_EditValueChanged(userControl.FTPPortCalcEdit, EventArgs.Empty);

				AssertEquals("2435", userControl.FTPPortCalcEdit.Text);
			}
		}

		public void TestFTPPortCalcEdit_TextChanged_HandlesLargeValue()
		{
			using (var userControl = new FTPSettingsRegistryItemUserControl())
			{
				userControl.FTPPortCalcEdit.Text = "70000";
				userControl.ZCalc_EditValueChanged(userControl.FTPPortCalcEdit, EventArgs.Empty);

				AssertEquals("The value comes back to the largest one possible", "65535", userControl.FTPPortCalcEdit.Text);
			}
		}

		public void TestFTPPortCalcEdit_KeyPress_PreventsNegativeValues()
		{
			using (var userControl = new FTPSettingsRegistryItemUserControl())
			{
				var keyPressEventArgs = new System.Windows.Forms.KeyPressEventArgs('-');
				userControl.FTPPortCalcEdit_KeyPress(userControl.FTPPortCalcEdit, keyPressEventArgs);

				AssertEquals("Event should be handled for negative values", true, keyPressEventArgs.Handled);
			}
		}

		public void TestFTPPortCalcEdit_KeyPress_AllowsPositiveNumbers()
		{
			using (var userControl = new FTPSettingsRegistryItemUserControl())
			{
				var keyPressEventArgs = new System.Windows.Forms.KeyPressEventArgs('5');
				userControl.FTPPortCalcEdit_KeyPress(userControl.FTPPortCalcEdit, keyPressEventArgs);

				AssertEquals("Event should not be handled for positive values", false, keyPressEventArgs.Handled);
			}
		}

		public void TestFTPSettings_CharacterCasing()
		{
			using (var userControl = new FTPSettingsRegistryItemUserControl())
			{
				CombineAssertions("Character casing test for Normal", () =>
				{
					AssertEquals("Casing should be normal for FTPInTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.FTPInTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for FTPOutTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.FTPOutTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for ExportUnionUserCodeTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.ExportUnionUserCodeTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for ExportUnionUserPasswordTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.ExportUnionUserPasswordTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for ExportUnionPaymentPasswordTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.ExportUnionPaymentPasswordTextBox.CharacterCasing);
				});
			}
		}
	}
}
