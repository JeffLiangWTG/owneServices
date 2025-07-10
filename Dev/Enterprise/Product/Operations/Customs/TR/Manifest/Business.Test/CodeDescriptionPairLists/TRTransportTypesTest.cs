using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class TRTransportTypesTest : BusinessObjectLookupsTestCase
	{
		public void TestGetTransportTypesByTransportMode()
		{
			var list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.Sea);
			AssertEquals(5, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT10));
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT12));
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT16));
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT17));
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT18));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.Rail);
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT20));
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT23));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.Road);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT30));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.Air);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT40));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.Mail);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT50));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.FixedTransportInstallations);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT70));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.InlandWaterwayTransport);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT80));
			list = TRTransportTypes.GetTransportTypesByTransportMode(Core.Constants.TransportModes.OwnPropulsion);
			AssertEquals(1, list.Count);
			Assert(list.ContainsCode(TRTransportTypes.Codes.TT90));
		}
	}
}
