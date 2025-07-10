namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("1234567890", Formatter.Format("123. 45 .67.890"));
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("1234.56.7890", Formatter.DisplayFormat("123. 45 .67.890"));
		}

		protected override Customs.Business.TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}

		public void TestDottedFormatForUS()
		{
			AssertEquals("1234.56.7890", Formatter.DisplayFormat("123.45.67.8 90"));
		}
	}
}
