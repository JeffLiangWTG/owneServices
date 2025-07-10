using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class TransportModeCodeListTest : TestCaseWithFactory
	{
		public void TestTransportIsSeaOrAir()
		{
			AssertEquals(true, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_1_SEA));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_2_Rail));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_3_Road));
			AssertEquals(true, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_4_Air));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_5_Mail));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(TransportModeCodeList.Codes.TransportMode_7_Pipeline));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(ZString.Empty));
			AssertEquals(false, TransportModeCodeList.TransportIsSeaOrAir(null));
		}
	}
}
