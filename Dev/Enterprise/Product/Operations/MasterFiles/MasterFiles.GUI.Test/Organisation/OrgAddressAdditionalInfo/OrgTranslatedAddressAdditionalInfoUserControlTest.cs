using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class OrgTranslatedAddressAdditionalInfoUserControlTest : TransactionedTestCase
	{
		public void TestGridAndColumnsBinding()
		{
			using (var control = new OrgTranslatedAdressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("translatedAdditionalInfoGrid", true).FirstOrDefault() as ZGrid;

				CombineAssertions(() =>
				{
					AssertNotNull(grid);
					AssertNotNull("Should have the column TranslatedAdditionalInfo", grid.GetColumnStyle("TranslatedAdditionalInfo"));
					AssertNotNull("Should have the column IsPrimary", grid.GetColumnStyle("IsPrimary"));
					AssertNotNull("Should have the column AdditionalInfo", grid.GetColumnStyle("AdditionalInfo"));
					AssertEquals("ActiveOrAllAddresses.CurrentTranslatedAddress.AdditionalInfoWrapperCollection", control.BindingSource.GetBindingMember(grid));
				});
			}
		}

		public void TestGridColumnStylesCharacterCasing()
		{
			Env.Registry.SetOrgAllowMixedCase(false);

			using (var control = new OrgTranslatedAdressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("TranslatedAdditionalInfoGrid", true).FirstOrDefault() as ZGrid;
				AssertEquals("AdditionalInfo.CharacterCasing", CharacterCasing.Upper, GetCharacterCasing(grid.GetColumnStyle("AdditionalInfo")));
				AssertEquals("TranslatedAdditionalInfo.CharacterCasing", CharacterCasing.Upper, GetCharacterCasing(grid.GetColumnStyle("TranslatedAdditionalInfo")));
			}

			Env.Registry.SetOrgAllowMixedCase(true);

			using (var control = new OrgTranslatedAdressAdditionalInfoUserControl())
			{
				var grid = control.Controls.Find("TranslatedAdditionalInfoGrid", true).FirstOrDefault() as ZGrid;
				AssertEquals("AdditionalInfo.CharacterCasing", CharacterCasing.Normal, GetCharacterCasing(grid.GetColumnStyle("AdditionalInfo")));
				AssertEquals("TranslatedAdditionalInfo.CharacterCasing", CharacterCasing.Normal, GetCharacterCasing(grid.GetColumnStyle("TranslatedAdditionalInfo")));
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
