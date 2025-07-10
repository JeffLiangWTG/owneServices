using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ManifestToOpenBillCollection))]
	class ManifestToOpenBillCollectionTest : ActiveBusinessObjectCollectionTestCase<ManifestToOpenBillCollection>
	{
		protected override ManifestToOpenBillCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			return manifest.Bills;
		}

		public void TestSetDefaultsForNewElementCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ClusterKey = 9;
			var manifest = declaration.ManifestToOpenHeaders.AddNew();
			var bill = manifest.Bills.AddNew();
			AssertEquals("ClusterKey", 9, bill.TPD_ClusterKey);
		}
	}
}
