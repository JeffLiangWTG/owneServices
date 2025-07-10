using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	public class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackCanBeDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_MessageStatus = "ACP";

			var pack = bill.Packs.AddNew();

			AssertEquals("This Bill is already registered with Customs.", pack.ReasonForNotAbleToDelete);
			AssertEquals(false, pack.CanDelete);

			bill.ABL_MessageStatus = "";
			AssertEquals(true, pack.CanDelete);
		}

		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.UYManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
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

