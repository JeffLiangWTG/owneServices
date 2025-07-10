using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing.CusGoodsCatalog
{
	sealed class CusGoodsCatalogUserControlTest : TestCaseWithFactory
	{
		public void TestTariffNumFindBox()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Guadeloupe;
			var goodsCatalog = Factory.New<BaseCusGoodsCatalog>();
			goodsCatalog.CGC_GC_Company = company.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
			using (var form = new ZForm(goodsCatalog))
			using (var control = new CusGoodsCatalogUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tariffNumFindBox = control.TariffNumFindBox;
				AssertEquals("TariffNumFindBox.TariffType", "HSN", tariffNumFindBox.TariffType);
				AssertEquals("TariffNumFindBox.GetCountryCode()", Core.Constants.CountryCodes.Guadeloupe, tariffNumFindBox.GetCountryCode());
				AssertEquals("TariffNumFindBox.GetDataGrouping()", Core.Constants.CountryCodes.France, tariffNumFindBox.GetDataGrouping());

				goodsCatalog.Delete();
				AssertEquals("TariffNumFindBox.GetCountryCode() - Deleted", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());
				AssertEquals("TariffNumFindBox.GetDataGrouping() - Deleted", ZString.Empty, tariffNumFindBox.GetDataGrouping());

				using (var control2 = new CusGoodsCatalogUserControl())
				{
					control2.CreateControl();
					tariffNumFindBox = control.TariffNumFindBox;
					AssertEquals("TariffNumFindBox.GetCountryCode() - Deleted", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());
					AssertEquals("TariffNumFindBox.GetDataGrouping() - Deleted", ZString.Empty, tariffNumFindBox.GetDataGrouping());
				}
			}
			}

		public void TestConsigneeFindBox()
		{
			using (var control = new CusGoodsCatalogUserControl())
			{
				var consigneeFindBox = control.ConsigneeFindBox;
				AssertType<ZGuidFindBox>("ConsigneeFindBox should be a ZGuidFindBox", consigneeFindBox);
			}
		}
	}
}
