using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class TariffColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			var tariffColumnStyleInfo = new TariffColumnStyleInfo();
			AssertEquals(typeof(TariffColumnStyle), tariffColumnStyleInfo.ColumnStyleType);
		}
	}
}
