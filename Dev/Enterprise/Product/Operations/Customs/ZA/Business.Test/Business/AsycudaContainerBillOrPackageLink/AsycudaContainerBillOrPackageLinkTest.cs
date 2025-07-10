using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillOrPackageLink))]
	sealed class AsycudaContainerBillOrPackageLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainerAndPack()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			AssertNull(Factory.LoadTop1<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, cont.PK)));
			pack.ContainerPK = cont.PK;
			var pivot = Factory.LoadTop1<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, cont.PK));
			AssertSame(cont, pivot.Container);
			AssertSame(pack, pivot.Pack);
		}

		public void TestValidation()
		{
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			AssertType<AsycudaPackageContainerLinkValidation>(link.Validation);
		}

		public void TestLookups()
		{
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			AssertType<AsycudaPackageContainerLinkLookups>(link.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var container = header.Containers.AddNew();
			var link = Factory.New<AsycudaContainerBillOrPackageLink>();
			link.APC_ACN_Container = container.PK;
			link.APC_APA_Pack = pack.PK;
			return link;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
