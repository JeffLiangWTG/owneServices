using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ManifestToOpenPackCollection))]
	class ManifestToOpenPackCollectionTest : ActiveBusinessObjectCollectionTestCase<ManifestToOpenPackCollection>
	{
		protected override ManifestToOpenPackCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			var bill = manifest.Bills.AddNew();
			return bill.Packs;
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClusterKey = 10;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("ClusterKey", 10, pack.TPI_ClusterKey);
		}
	}
}
