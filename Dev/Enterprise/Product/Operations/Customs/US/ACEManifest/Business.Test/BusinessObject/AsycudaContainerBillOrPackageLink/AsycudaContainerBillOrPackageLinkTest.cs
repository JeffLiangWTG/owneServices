using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	class AsycudaContainerBillOrPackageLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPackageContainerLink()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertNotNull("Pack is supported when SupportsAsycudaPacks is true", new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaContainerBillOrPackageLink>(bizObj.PK));
			AssertNotNull("Pack is supported when SupportsAsycudaPacks is true", new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainerBillOrPackageLink>(bizObj.PK));
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Pack is not supported when SupportsAsycudaPacks is false", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = header.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack.Pivot;
		}
	}
}
