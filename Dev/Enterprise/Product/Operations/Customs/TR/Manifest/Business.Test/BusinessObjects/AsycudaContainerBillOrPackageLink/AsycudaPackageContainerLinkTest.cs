using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	public class AsycudaPackageContainerLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPackageContainerLink()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaContainerBillOrPackageLink>(bizObj.PK).GetType());
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

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBusinessObject(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var pivot = CreateBusinessObject(factory);
			manifestHeader.SuspendCheckBusinessObjectType();
			return pivot;
		}

		AsycudaContainerBillOrPackageLink CreateBusinessObject(BusinessObjectFactory factory)
		{
			manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			return pack.Pivot;
		}

		AsycudaManifestHeader manifestHeader;
		#endregion
	}
}
