using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillLinkAsycudaContainer))]
	sealed class AsycudaBillLinkAsycudaContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetLink()
		{
			var asycudaBillLinkAsycudaContainer = GetNewBusinessObject() as AsycudaBillLinkAsycudaContainer;
			AssertEquals(false, asycudaBillLinkAsycudaContainer.Link);

			var asycudaContainerBillOrPackageLink1 = Factory.NewWithValidTestData<AsycudaContainerBillOrPackageLink>();
			asycudaContainerBillOrPackageLink1.APC_ABL_Bill = bill.PK;
			asycudaContainerBillOrPackageLink1.APC_ClusterKey = bill.ABL_ClusterKey;
			asycudaContainerBillOrPackageLink1.APC_ACN_Container = container.PK;
			bill.BillLinkContainerDivots.Load();
			AssertEquals(true, asycudaBillLinkAsycudaContainer.Link);
		}

		public void TestSetLink()
		{
			var asycudaBillLinkAsycudaContainer = GetNewBusinessObject() as AsycudaBillLinkAsycudaContainer;
			var asycudaContainerBillOrPackageLinkArray = Factory.LoadTop1<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, bill.PK));
			AssertNull(asycudaContainerBillOrPackageLinkArray);

			asycudaBillLinkAsycudaContainer.Link = true;
			asycudaContainerBillOrPackageLinkArray = Factory.LoadTop1<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, bill.PK));
			AssertNotNull(asycudaContainerBillOrPackageLinkArray);

			asycudaBillLinkAsycudaContainer.Link = false;
			asycudaContainerBillOrPackageLinkArray = Factory.LoadTop1<AsycudaContainerBillOrPackageLink>(new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, bill.PK));
			AssertNull(asycudaContainerBillOrPackageLinkArray);
		}

		public void TestFieldMapping()
		{
			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "40GP";
			refcontainer.RC_ISOType = "40GP";
			refcontainer.RC_Length = 0;
			refcontainer.RC_Width = 0;
			refcontainer.RC_Height = 0;

			var asycudaBillLinkAsycudaContainer = GetNewBusinessObject() as AsycudaBillLinkAsycudaContainer;
			container.ACN_ContainerNumber = "123456";
			container.ACN_RC_ContainerType = refcontainer.PK;
			container.ACN_EmptyFullIndicator = "1";
			container.ACN_Seal1 = "Seal 1";
			container.ACN_Seal2 = "Seal 2";
			container.ACN_Seal3 = "Seal 3";
			CombineAssertions(() =>
			{
				AssertEquals(container.PK, asycudaBillLinkAsycudaContainer.PK);
				AssertEquals("123456", asycudaBillLinkAsycudaContainer.ContainerNumber);
				AssertEquals(refcontainer.PK, asycudaBillLinkAsycudaContainer.ContainerType);
				AssertEquals("1", asycudaBillLinkAsycudaContainer.ContainerMode);
				AssertEquals("Seal 1", asycudaBillLinkAsycudaContainer.Seal1);
				AssertEquals("Seal 2", asycudaBillLinkAsycudaContainer.Seal2);
				AssertEquals("Seal 3", asycudaBillLinkAsycudaContainer.Seal3);
				AssertEquals("LinkInfo.ReadOnly", false, asycudaBillLinkAsycudaContainer.LinkInfo.ReadOnly);
				AssertEquals("LinkInfo.ContainerNumberInfo", true, asycudaBillLinkAsycudaContainer.ContainerNumberInfo.ReadOnly);
				AssertEquals("LinkInfo.ContainerTypeInfo", true, asycudaBillLinkAsycudaContainer.ContainerTypeInfo.ReadOnly);
				AssertEquals("LinkInfo.ContainerModeInfo", true, asycudaBillLinkAsycudaContainer.ContainerModeInfo.ReadOnly);
				AssertEquals("LinkInfo.Seal1Info", true, asycudaBillLinkAsycudaContainer.Seal1Info.ReadOnly);
				AssertEquals("LinkInfo.Seal2Info", true, asycudaBillLinkAsycudaContainer.Seal2Info.ReadOnly);
				AssertEquals("LinkInfo.Seal3Info", true, asycudaBillLinkAsycudaContainer.Seal3Info.ReadOnly);
			});
		}

		public void TestSeals()
		{
			var bill = Factory.New<AsycudaBill>();
			var container = Factory.New<AsycudaContainer>();
			container.ACN_Seal1 = "Seal1";
			container.ACN_Seal2 = "Seal2";
			container.ACN_Seal3 = "Seal3";
			bill.LinkContainer(container.PK);
			var billContainer = new AsycudaBillLinkAsycudaContainer(bill, container);
			CombineAssertions(() =>
			{
				AssertEquals("Seal1", billContainer.Seal1);
				AssertEquals("Seal2", billContainer.Seal2);
				AssertEquals("Seal3", billContainer.Seal3);
			});
		}

		public void TestRefContainer()
		{
			var bill = Factory.New<AsycudaBill>();
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "RC_Code";
			refContainer.RC_Description = "RC_Description";
			var container = Factory.New<AsycudaContainer>();
			container.ACN_RC_ContainerType = refContainer.PK;
			bill.LinkContainer(container.PK);
			var billContainer = new AsycudaBillLinkAsycudaContainer(bill, container);
			var billRefContainer = billContainer.RefContainer;
			AssertEquals(refContainer, billRefContainer);
		}

		public void TestLookups()
		{
			var asycudaBillLinkAsycudaContainer = GetNewBusinessObject() as AsycudaBillLinkAsycudaContainer;
			AssertType<AsycudaBillLinkAsycudaContainerLookups>(asycudaBillLinkAsycudaContainer.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			container = header.Containers.AddNew();
			bill = header.Bills.AddNew();
			return bill.AsycudaBillLinkAsycudaContainers.Cast<AsycudaBillLinkAsycudaContainer>().Single();
		}

		AsycudaBill bill;
		AsycudaContainer container;
	}
}
