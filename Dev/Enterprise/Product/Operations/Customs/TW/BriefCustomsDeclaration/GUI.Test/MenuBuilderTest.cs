using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (var form = new BriefDeclarationForm(header))
			{
				var menuBuilder = new MenuBuilder(header, form);
				AssertEquals("Brief Customs Declaration", menuBuilder.MenuCaption);
			}
		}

		public void TestSendToCustomsMenuItemsWhenImport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			CombineAssertions(() =>
			{
				using (var form = new BriefDeclarationForm(header))
				{
					var menuBuilder = new MenuBuilder(header, form);
					var menuItem = menuBuilder.BuildMenu().Single();
					AssertEquals("Send to Customs", menuItem.Text);
					AssertEquals("Should have 'Send N5135 Message' menu when IMP", true, menuItem.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Send N5135 Message"));
					AssertEquals("Should not have 'Send N5205 Message' menu when IMP", false, menuItem.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Send N5205 Message"));
				}
			});
		}

		public void TestSendToCustomsMenuItemsWhenExport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			CombineAssertions(() =>
			{
				using (var form = new BriefDeclarationForm(header))
				{
					var menuBuilder = new MenuBuilder(header, form);
					var menuItem = menuBuilder.BuildMenu().Single();
					AssertEquals("Send to Customs", menuItem.Text);
					AssertEquals("Should not have 'Send N5135 Message' menu when EXP", false, menuItem.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Send N5135 Message"));
					AssertEquals("Should have 'Send N5205 Message' menu when EXP", true, menuItem.MenuItems.Cast<MenuItem>().Any(x => x.Text == "Send N5205 Message"));
				}
			});
		}
	}
}
