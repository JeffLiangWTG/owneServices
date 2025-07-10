using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class TransportModeTranslatorTest : TestCase
	{
		public void TestTranslateToWCOCode_Air()
		{
			AssertEquals("5", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Air));
		}

		public void TestTranslateToWCOCode_InlandWaterways()
		{
			AssertEquals("3", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.InlandWaterwayTransport));
		}

		public void TestTranslateToWCOCode_Other()
		{
			AssertEquals("_", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Other));
		}

		public void TestTranslateToWCOCode_Rail()
		{
			AssertEquals("6", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Rail));
		}

		public void TestTranslateToWCOCode_Road()
		{
			AssertEquals("4", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Road));
		}

		public void TestTranslateToWCOCode_Sea()
		{
			AssertEquals("3", transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Sea));
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportModeTranslator = new TransportModeTranslatorForTest();
		}

		TransportModeTranslatorForTest transportModeTranslator;
		sealed class TransportModeTranslatorForTest : TransportModeTranslator
		{
			public string[] CargoWiseToWCOCodesForTest => CargoWiseToWCO.Keys.ToArray();
			public string[] WCOToCargoWiseCodesForTest => WCOToCargoWise.Keys.ToArray();
		}
	}
}
