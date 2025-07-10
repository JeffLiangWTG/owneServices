using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemperatureFormatterTest : TestCase
	{
		public void TestEmptyInputGetsEmptyOutput()
		{
			AssertEquals("Check Value with empty input", "", TemperatureFormatter.FormatTemperatureString(""));
		}

		public void TestValue1()
		{
			AssertEquals("40.0", TemperatureFormatter.FormatTemperatureString("40 to 51 cc"));
		}

		public void TestValue2()
		{
			AssertEquals("32.0", TemperatureFormatter.FormatTemperatureString("approx. 32 cc"));
		}

		public void TestValue3()
		{
			AssertEquals("-18.0", TemperatureFormatter.FormatTemperatureString("below -18 cc"));
		}

		public void TestValue4()
		{
			AssertEquals("-05.0", TemperatureFormatter.FormatTemperatureString("below-5 cc"));
		}

		public void TestValue5()
		{
			AssertEquals("-03.5", TemperatureFormatter.FormatTemperatureString("-3.5"));
		}

		public void TestValue6()
		{
			AssertEquals("00.0", TemperatureFormatter.FormatTemperatureString("0.000"));
		}
	}
}
