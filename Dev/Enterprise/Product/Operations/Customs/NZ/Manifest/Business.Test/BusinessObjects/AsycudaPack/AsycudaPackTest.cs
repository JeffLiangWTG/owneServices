using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestPivot()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainerBillOrPackageLink>(pack.Pivot);
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		public void TestContainer()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainer>(pack.Container);
		}

		public void TestGetPackageContainerLinkType()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainerBillOrPackageLink>(pack.Pivot);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack;
		}
	}
}
