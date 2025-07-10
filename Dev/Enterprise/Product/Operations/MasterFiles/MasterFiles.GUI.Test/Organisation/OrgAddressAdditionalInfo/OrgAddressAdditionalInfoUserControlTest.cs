using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class OrgAddressAdditionalInfoUserControlTest : TransactionedTestCase
	{
		public void TestGridAndColumnsBinding()
		{
			using (var control = new OrgAddressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("AdditionalInfoGrid", true).FirstOrDefault() as ZGrid;

				CombineAssertions(() =>
				{
					AssertNotNull(grid);
					AssertNotNull("Should have the column OAI_IsPrimary", grid.GetColumnStyle("OAI_IsPrimary"));
					AssertNotNull("Should have the column OAI_AdditionalInfo", grid.GetColumnStyle("OAI_AdditionalInfo"));
					AssertEquals("ActiveOrAllAddresses.AdditionalInfos", control.BindingSource.GetBindingMember(grid));
				});
			}
		}

		public void TestGridColumnStylesCharacterCasing()
		{
			Env.Registry.SetOrgAllowMixedCase(false);

			using (var control = new OrgAddressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("AdditionalInfoGrid", true).FirstOrDefault() as ZGrid;
				AssertEquals("OAI_AdditionalInfo.CharacterCasing", CharacterCasing.Upper, GetCharacterCasing(grid.GetColumnStyle("OAI_AdditionalInfo")));
			}

			Env.Registry.SetOrgAllowMixedCase(true);

			using (var control = new OrgAddressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("AdditionalInfoGrid", true).FirstOrDefault() as ZGrid;
				AssertEquals("OAI_AdditionalInfo.CharacterCasing", CharacterCasing.Normal, GetCharacterCasing(grid.GetColumnStyle("OAI_AdditionalInfo")));
			}

			CharacterCasing GetCharacterCasing(object styleInfo)
			{
				var chrCasing = CharacterCasing.Lower;
				if (styleInfo is ZTextBoxColumnStyleInfo textboxInfo)
				{
					chrCasing = textboxInfo.CharacterCasing;
				}
				return chrCasing;
			}
		}
	}
}

