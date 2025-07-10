using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	[TestedType(typeof(VNManifestTypes))]
	sealed class VNManifestTypes_PartialTest : TestCaseWithFactory
	{
		/// <summary>
		/// Verify the mode of transport contain in Manifest
		/// </summary>
		public void TestTransportMode()
		{
			var manifestTypes = new VNManifestTypes().All;
			AssertEquals(1, manifestTypes.Count);
			var vsw = manifestTypes.First(x => x.Code == VNManifestTypes.Codes.VSW);

			AssertEquals(VNManifestTypes.Codes.VSW, vsw.Code);
			AssertEquals(VNManifestTypes.Descriptions.VSW, vsw.Description);
			AssertContainsExactElementsInAnyOrder(
				new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air },
				vsw.ApplicableTransportModes);
			AssertContainsExactElementsInAnyOrder(
				new[] { ApplicationCodeTypeList.Codes.Consolidator },
				vsw.ApplicableManifestStyles);
			AssertEquals(MessageLevel.Manifest, vsw.MessageLevel);
			Assert(vsw.ManifestNatures.ContainsOnly(ShipmentTypeList.Codes.Import23));
		}
	}
}
