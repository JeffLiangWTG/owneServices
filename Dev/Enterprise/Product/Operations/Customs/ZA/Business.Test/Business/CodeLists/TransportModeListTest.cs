using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class TransportModeListTest : TestCase
	{
		public void TestTransportModeListCanBeTranslatedToWCOCodeList()
		{
			var transportModeTranslator = new TransportModeTranslator();
			foreach (ZString code in new TransportModeList().GetAllCodes())
			{
				AssertNotNullOrEmpty(transportModeTranslator.TranslateToWCOCode(code));
			}
		}
	}
}
