using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class CusClassificationUserControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBox()
		{
			using (var control = new CusClassificationUserControl())
			{
				var tariffFindBox = control.Controls.Find("CC_TariffNumFindBox", true)[0] as Universal.GUI.TariffFindBox;
				AssertNotNull("TariffFindBox should be a Universal TariffFindBox", tariffFindBox);
				AssertEquals("TariffFindBox.GetCountryCode", "ZA", tariffFindBox.GetCountryCode());
				AssertEquals("TariffFindBox.GetDataGrouping", "ZA", tariffFindBox.GetDataGrouping());
				AssertEquals("TariffFindBox.TariffType", "1P1", tariffFindBox.TariffType);
			}
		}
	}
}
