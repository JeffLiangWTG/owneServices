using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TaiwanTariffFormatterTest : Customs.Business.Testing.TariffFormatterTest
	{
		public override void TestFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("87149990905", Formatter.Format("87149990905"));
				AssertEquals("87149990905", Formatter.Format("A87149990905"));
				AssertEquals("87149990905", Formatter.Format("8714.99.90.90-5"));
			});
		}

		public override void TestDisplayFormat()
		{
			AssertEquals("8714.99.90.90-5", Formatter.DisplayFormat("87149990905"));
		}

		public void TestDottedFormatWithPrintLength()
		{
			var taiwanTariffFormatter = (TaiwanTariffFormatter)Formatter;
			CombineAssertions(() =>
			{
				AssertEquals("Not Printed tariff", ZString.Empty, taiwanTariffFormatter.DottedFormatWithPrintLength("8714.99.90.90-55", TariffPrintLengthList.Codes.NotPrinted));
				AssertEquals("6 digits tariff formatted", "8714.99", taiwanTariffFormatter.DottedFormatWithPrintLength("8714.99.90.90-55", TariffPrintLengthList.Codes.One));
				AssertEquals("8 digits tariff formatted", "8714.99.90", taiwanTariffFormatter.DottedFormatWithPrintLength("8714.99.90.90-55", TariffPrintLengthList.Codes.TWO));
				AssertEquals("11 digits tariff formatted", "8714.99.90.90-5", taiwanTariffFormatter.DottedFormatWithPrintLength("8714.99.90.90-55", TariffPrintLengthList.Codes.THREE));
			});
		}

		protected override TariffFormatter GetNewTariffFormatter() => new TaiwanTariffFormatter();
	}
}
