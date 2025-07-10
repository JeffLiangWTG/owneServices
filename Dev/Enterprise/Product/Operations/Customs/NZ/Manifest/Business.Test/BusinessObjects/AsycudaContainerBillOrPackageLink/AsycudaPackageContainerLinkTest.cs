using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaPackageContainerLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPackageContainerLink()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.IAsycudaContainerBillOrPackageLink>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainerBillOrPackageLink>(bizObj.PK).GetType());
		}

		public void TestContainer()
		{
			var packageContainerLink = (AsycudaContainerBillOrPackageLink)GetNewBusinessObject();
			AssertType<AsycudaContainer>(packageContainerLink.Container);
		}

		public void TestPack()
		{
			var packageContainerLink = (AsycudaContainerBillOrPackageLink)GetNewBusinessObject();
			AssertType<AsycudaPack>(packageContainerLink.Pack);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var pivot = CreateBusinessObject();
			manifestHeader.SuspendCheckBusinessObjectType();
			return pivot;
		}

		AsycudaContainerBillOrPackageLink CreateBusinessObject()
		{
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack.Pivot;
		}

		AsycudaManifestHeader manifestHeader;
	}
}
