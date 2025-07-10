namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			AssertEquals("1234567890", Formatter.Format("123. 45 .67.890"));
			AssertEquals(TariffViewAsCodeDescription.NotApplicableCode, Formatter.Format(TariffViewAsCodeDescription.NotApplicableCode));
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("1234.56.7890", Formatter.DisplayFormat("123. 45 .67.890"));
			AssertEquals(TariffViewAsCodeDescription.NotApplicableCode, Formatter.DisplayFormat(TariffViewAsCodeDescription.NotApplicableCode));
		}

		public void TestDottedFormatForUS()
		{
			AssertEquals("1234.56.7890", Formatter.DisplayFormat("123.45.67.8 90"));
			AssertEquals(TariffViewAsCodeDescription.NotApplicableCode, Formatter.DisplayFormat(TariffViewAsCodeDescription.NotApplicableCode));
		}

		public void TestGetFormattedTariff()
		{
			AssertEquals("1112.22.333", Formatter.DisplayFormat("111222333"));
			AssertEquals("333", Formatter.DisplayFormat("333"));
			AssertEquals("4444", Formatter.DisplayFormat("4444"));
			AssertEquals("5555.5", Formatter.DisplayFormat("55555"));
			AssertEquals("6666.66", Formatter.DisplayFormat("666666"));
			AssertEquals("", Formatter.DisplayFormat(""));
		}

		protected override Customs.Business.TariffFormatter GetNewTariffFormatter() => new TariffFormatter();
	}
}
