using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(TWNCATKClientSettingRegistryItemUserControl))]
	public sealed class TWNCATKClientSettingRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new TWNCATKClientSetting();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((TWNCATKClientSettingRegistryItemUserControl)control).ReadOnly;
		}

		public void TestControlReadOnly()
		{
			using (var control = new TWNCATKClientSettingRegistryItemUserControlForTest())
			{
				AssertControlReadOnly(control, true);
				AssertControlReadOnly(control, false);
			}
		}

		void AssertControlReadOnly(TWNCATKClientSettingRegistryItemUserControlForTest control, bool readOnly)
		{
			control.SetControlReadOnly(readOnly);
			AssertEquals(readOnly, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "MachineNameTextBox").ReadOnly);
			AssertEquals(readOnly, control.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "RunningIntervalInSecondsCalcEdit").ReadOnly);
			AssertEquals(readOnly, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "SendFolderTextBox").ReadOnly);
			AssertEquals(false, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "EHubClientIDTextBox").ReadOnly);
			AssertEquals(false, control.FindSingleOrDefault<ZTextBox>(c => c.Name == "EHubClientStatusTextBox").ReadOnly);
		}

		public void TestMachineNameTextBoxCharacterCasingShouldBeNormal()
		{
			using (var control = new TWNCATKClientSettingRegistryItemUserControlForTest())
			{
				var machineNameTextBox = control.FindSingleOrDefault<ZTextBox>(c => c.Name == "MachineNameTextBox");
				AssertEquals(System.Windows.Forms.CharacterCasing.Normal, machineNameTextBox.CharacterCasing);
			}
		}

		class TWNCATKClientSettingRegistryItemUserControlForTest : TWNCATKClientSettingRegistryItemUserControl
		{
			public TWNCATKClientSettingRegistryItemUserControlForTest() : base()
			{
			}

			public void SetControlReadOnly(bool readOnly)
			{
				base.SetControlOrBusinessEntityReadOnly(readOnly);
			}
		}
	}
}
