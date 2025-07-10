using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaPackageContainerLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPackageContainerLink()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaContainerBillOrPackageLink>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainerBillOrPackageLink>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

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
