using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainerBillLinkCollection<AsycudaContainerBillOrPackageLink, AsycudaBill>))]
	sealed class AsycudaContainerBillLinkCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAdditionalFilterLoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();

			var asycudaContainerBillOrPackageLink1 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink1.APC_ABL_Bill = bill1.PK;
			asycudaContainerBillOrPackageLink1.APC_ACN_Container = container1.PK;

			var asycudaContainerBillOrPackageLink2 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink2.APC_ABL_Bill = bill1.PK;
			asycudaContainerBillOrPackageLink2.APC_ACN_Container = container2.PK;

			var asycudaContainerBillOrPackageLinkCollection1 = bill1.BillLinkContainerDivots;
			var asycudaContainerBillOrPackageLinkCollection2 = bill2.BillLinkContainerDivots;
			asycudaContainerBillOrPackageLinkCollection1.Load();
			asycudaContainerBillOrPackageLinkCollection2.Load();

			CombineAssertions(() =>
			{
				AssertEquals("asycudaContainerBillOrPackageLinkCollection1.Count", 2, asycudaContainerBillOrPackageLinkCollection1.Count);
				AssertEquals("asycudaContainerBillOrPackageLinkCollection2.Count", 0, asycudaContainerBillOrPackageLinkCollection2.Count);
				AssertContainsExactElementsInAnyOrder("asycudaContainerBillOrPackageLinkCollection1", new[] { asycudaContainerBillOrPackageLink1, asycudaContainerBillOrPackageLink2 }, asycudaContainerBillOrPackageLinkCollection1);
			});
		}

		protected override Type GetExpectedCollectionType() => typeof(AsycudaContainerBillLinkCollection<AsycudaContainerBillOrPackageLink, AsycudaBill>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew().BillLinkContainerDivots;
		}
	}
}
