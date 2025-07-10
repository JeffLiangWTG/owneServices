using Enterprise.Customs.Common;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("49019990", Formatter.Format("4901. 99 .90"));
			AssertEquals("490199", Formatter.Format("4901.99"));
			AssertEquals("490199909934", Formatter.Format("4901. 99 .90.99.34"));
		}

		public void TestSpringArchitecture()
		{
			AssertEquals(typeof(TariffFormatter), TariffFormatterDecider.GetByCountryCode(Core.Constants.CountryCodes.Singapore).GetType());
		}

		protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}
