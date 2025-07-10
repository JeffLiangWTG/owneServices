namespace Enterprise.Customs.TR.Business.Testing
{
	public class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("1234", Formatter.Format("1234"));
			AssertEquals("12345", Formatter.Format("1234.5"));
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("1234", Formatter.DisplayFormat("1234"));
			AssertEquals("1234.5", Formatter.DisplayFormat("12345"));
			AssertEquals("1234.56", Formatter.DisplayFormat("123456"));
			AssertEquals("1234.56.7", Formatter.DisplayFormat("1234567"));
			AssertEquals("1234.56.78", Formatter.DisplayFormat("12345678"));
			AssertEquals("1234.56.78.9", Formatter.DisplayFormat("123456789"));
			AssertEquals("1234.56.78.90", Formatter.DisplayFormat("123. 45 .67.890"));
			AssertEquals("1234.56.78.90.1", Formatter.DisplayFormat("12345678901"));
			AssertEquals("1234.56.78.90.12", Formatter.DisplayFormat("1234567890123"));
		}
		protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}
