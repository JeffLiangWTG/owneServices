using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestToOpenCollection))]
	public class ManifestToOpenCollectionTest : CusSupportingInfoCollectionTest<ManifestToOpen>
	{
		protected override CusSupportingInfoCollection<ManifestToOpen> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return new ManifestToOpenCollection(header);
		}
	}
}
