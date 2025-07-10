using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	public class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var constainer = header.Containers.AddNew();
			constainer.ACN_ContainerNumber = "Test";
			return constainer;
		}
		public void TestContainerForHeaderAndBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.PreviousContainerNo = "Prev";
			header.NewContainerNo = "New";
			var bill = header.Bills.AddNew();
			bill.ContainerNumber = "XXX";
			CombineAssertions(() =>
			{
				AssertEquals("Previous Container", header.PreviousContainer.ContainerLevel, AsycudaManifestHeader.PreviousContainerCode);
				AssertEquals("New Container", header.NewContainer.ContainerLevel, AsycudaManifestHeader.NewContainerCode);
				AssertEquals("BillContainer", (bill.Container as AsycudaContainer).ContainerLevel, AsycudaBill.BillContainerCode);
			});
		}
	}
}
