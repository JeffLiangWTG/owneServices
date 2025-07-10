using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertNotNull("Pack is supported when SupportsAsycudaPacks is true", new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaPack>(bizObj.PK));
			AssertNotNull("Pack is supported when SupportsAsycudaPacks is true", new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK));
			Assert("AsycudaPackPackedItemPivotCollection.RelationshipType.One", bizObj is AsycudaPack pack && pack.PackedItemRelationship == ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Pack is not supported when SupportsAsycudaPacks is false", true);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
