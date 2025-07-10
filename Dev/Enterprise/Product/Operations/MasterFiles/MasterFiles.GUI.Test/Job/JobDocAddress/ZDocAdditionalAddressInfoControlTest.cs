using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.GUI.Test
{
	class ZDocAdditionalAddressInfoControlTest : TestCaseWithFactory
	{
		public void TestAddressAdditionalInformationControls()
		{
			Env.Registry.SetOrgAllowMixedCase(false);
			using (var control = new ZDocAdditionalAddressInfoControl())
			{
				AssertNotNull(control.AdditionalAddressInfoDropEdit);
				AssertNotNull(control.AdditionalInfoLabel);
				AssertEquals(CharacterCasing.Upper, control.AdditionalAddressInfoDropEdit.CharacterCasing);
			}

			Env.Registry.SetOrgAllowMixedCase(true);
			using (var control = new ZDocAdditionalAddressInfoControl())
			{
				AssertNotNull(control.AdditionalAddressInfoDropEdit);
				AssertEquals(CharacterCasing.Normal, control.AdditionalAddressInfoDropEdit.CharacterCasing);
			}
		}
	}
}
