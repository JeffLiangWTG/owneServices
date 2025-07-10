using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class TariffGridFindBoxTest : TestCase
	{
		public void TestTariffTypeDefaultShouldBeHSN()
		{
			var info = new TariffColumnStyleInfo()
			{
				GetTariffType = () => Universal.Constants.TariffTypes.HarmonizedSystem
			};

			using (var tariffColumnStyle = new TariffColumnStyle(info))
			{
				var findBox = (TariffGridFindBox)tariffColumnStyle.EditControl;
				AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, findBox.GetTariffType());
			}
		}
	}
}
