using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderDocManagerInfo))]
	sealed class AsycudaManifestHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<AsycudaManifestHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.NewWithValidTestData<AsycudaManifestHeader>();
	}
}
