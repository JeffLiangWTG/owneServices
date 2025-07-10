using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class PEManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var manifestTypes = new PEManifestTypes().All;
			AssertEquals(1, manifestTypes.Count);

			var man = manifestTypes.FirstOrDefault(x => x.Code == PEManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));
		}
	}
}
