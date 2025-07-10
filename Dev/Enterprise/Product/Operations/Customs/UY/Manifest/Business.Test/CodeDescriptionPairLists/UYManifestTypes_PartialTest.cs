using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class UYManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var uymanifestTypes = new UYManifestTypes().All;
			AssertEquals(1, uymanifestTypes.Count);

			var man = uymanifestTypes.FirstOrDefault(x => x.Code == UYManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.ShippingLine));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Rail));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Road));
		}
	}
}


