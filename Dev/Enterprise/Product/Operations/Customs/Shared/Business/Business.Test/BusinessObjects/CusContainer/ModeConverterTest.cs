using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ModeConverterTest : TestCase
	{
		public void TestConvertFreightToCustoms()
		{
			ModeConverter converter = new ModeConverter();
			converter.AddConversion("FRT", "CUS");
			AssertEquals("CUS", converter.ConvertFreightToCustoms("FRT"));
			AssertEquals("FRX", converter.ConvertFreightToCustoms("FRX"));
		}

		public void TestConvertCustomsToFreight()
		{
			ModeConverter converter = new ModeConverter();
			converter.AddConversion("FRT", "CUS");
			AssertEquals("FRT", converter.ConvertCustomsToFreight("CUS"));
			AssertEquals("CUX", converter.ConvertCustomsToFreight("CUX"));
		}
	}
}
