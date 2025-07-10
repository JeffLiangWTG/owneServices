using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class GeneralCountryClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestCC_TariffNumFindBox()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.Guadeloupe;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Liechtenstein))
			using (var form = new ZForm(classification))
			using (var control = new GeneralCountryClassificationUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tariffNumFindBox = control.Controls.Find("CC_TariffNumFindBox", true).FirstOrDefault() as Universal.GUI.TariffFindBox;
				AssertNotNull("CC_TariffNumFindBox should be a Universal.GUI.TariffFindBox", tariffNumFindBox);
				AssertEquals("CC_TariffNumFindBox.TariffType", "HSN", tariffNumFindBox.TariffType);
				AssertEquals("CC_TariffNumFindBox.GetCountryCode()", Core.Constants.CountryCodes.Guadeloupe, tariffNumFindBox.GetCountryCode());
				AssertEquals("CC_TariffNumFindBox.GetDataGrouping()", Core.Constants.CountryCodes.France, tariffNumFindBox.GetDataGrouping());

				classification.Delete();
				AssertEquals("CC_TariffNumFindBox.GetCountryCode() - Deleted", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());
				AssertEquals("CC_TariffNumFindBox.GetDataGrouping() - Deleted", Core.Constants.CountryCodes.Switzerland, tariffNumFindBox.GetDataGrouping());

				using (var control2 = new GeneralCountryClassificationUserControl())
				{
					control2.CreateControl();
					tariffNumFindBox = control2.Controls.Find("CC_TariffNumFindBox", true).FirstOrDefault() as Universal.GUI.TariffFindBox;
					AssertEquals("CC_TariffNumFindBox.GetCountryCode() - Null", Core.Constants.CountryCodes.Liechtenstein, tariffNumFindBox.GetCountryCode());
					AssertEquals("CC_TariffNumFindBox.GetDataGrouping() - Null", Core.Constants.CountryCodes.Switzerland, tariffNumFindBox.GetDataGrouping());
				}
			}
		}
	}
}
