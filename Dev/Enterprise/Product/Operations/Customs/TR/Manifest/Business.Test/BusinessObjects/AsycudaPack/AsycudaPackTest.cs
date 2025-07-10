using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestPropertiesForDocWrapper()
		{
			var contType = Factory.New<MasterFiles.Business.RefContainer>();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			CombineAssertions("Empty Data", () =>
			{
				AssertEquals("BillSequenceNo", (ZShort)1, pack.BillSequenceNo);
				AssertEquals("BillNumber", "", pack.BillNumber);
				AssertEquals("ContainerNumber", "", pack.ContainerNumber);
				AssertEquals("ContainerSealNumber", "", pack.ContainerSealNumber);
				AssertEquals("ContainerType", "", pack.ContainerType);
				AssertEquals("PackWeightInKG", (ZDecimal)0, pack.PackWeightInKG);
				AssertEquals("GrossWeightUQ", "KGM", pack.GrossWeightUQ);
				AssertEquals("PackUQ", "BI", pack.PackUQ);
			});
			var bill2 = header.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packedItem = pack2.PackedItems.AddNewPackedItem();
			var container = header.Containers.AddNew();
			bill2.ABL_SequenceNumber = 2;
			bill2.ABL_BillNumber = "12345";
			pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			container.ACN_ContainerNumber = "ABCD 123456-7";
			container.ACN_Seal1 = "SEAL1";
			container.Relation = "Foreign";
			pack2.APA_Weight = 10000m;
			pack2.APA_WeightUQ = "G";
			pack2.ContainerPK = container.PK;
			container.ACN_RC_ContainerType = contType.PK;
			packedItem.API_GoodsDescription = "SOME PACKS";
			packedItem.API_GrossWeightUQ = "KG";
			packedItem.API_Tariff = "12 34.56. 78";
			packedItem.API_GrossWeight = 12;
			packedItem.API_NetWeight = 8;
			CombineAssertions("With Data", () =>
			{
				AssertEquals("BillSequenceNo", (ZShort)2, pack2.BillSequenceNo);
				AssertEquals("BillNumber", "12345", pack2.BillNumber);
				AssertEquals("ContainerNumber", "ABCD 123456-7", pack2.ContainerNumber);
				AssertEquals("ContainerSealNumber", "SEAL1", pack2.ContainerSealNumber);
				AssertEquals("ContainerType", "YAB", pack2.ContainerType);
				AssertEquals("PackWeightInKG", (ZDecimal)10m, pack2.PackWeightInKG);
				AssertEquals("GrossWeightUQ", "KGM", pack2.GrossWeightUQ);
				AssertEquals("PackUQ", "BI", pack2.PackUQ);
				AssertEquals("API_GrossWeight", (ZDecimal)12m, packedItem.API_GrossWeight);
				AssertEquals("API_NetWeight", (ZDecimal)8m, packedItem.API_NetWeight);
				AssertEquals("ACN_Seal1", "SEAL1", container.ACN_Seal1);
			});
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

		public void TestContainerNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_MarksAndNumbers = "ADR";
			AssertEquals("No container", "ADR", pack.ContainerNumber);
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CN001";
			pack.ContainerPK = container.PK;
			AssertEquals("Has container", "CN001", pack.ContainerNumber);
		}

		public void TestContainerType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("No container", ZString.Empty, pack.ContainerType);
			var container = header.Containers.AddNew();
			container.Relation = RelationList.Codes.Foreign;
			pack.ContainerPK = container.PK;
			AssertEquals("Foreign", "YAB", pack.ContainerType);
			container.Relation = RelationList.Codes.Local;
			AssertEquals("Local", "YER", pack.ContainerType);
		}

		public void TestGetPackageContainerLinkType()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaContainerBillOrPackageLink>(pack.Pivot);
		}

		public void TestHasEmptyContainer()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("when no container", false, pack.HasEmptyContainer);
			var container = manifestHeader.Containers.AddNew();
			pack.ContainerPK = container.PK;
			container.ACN_EmptyFullIndicator = ASYCUDA.Business.EmptyFullIndicatorList.Codes.EmptyContainer;
			AssertEquals("when MT", true, pack.HasEmptyContainer);
			container.ACN_EmptyFullIndicator = ASYCUDA.Business.EmptyFullIndicatorList.Codes.FullContainerLoad;
			AssertEquals("when not MT", false, pack.HasEmptyContainer);
		}

		public void TestAPA_PackUQDefaultValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			CombineAssertions("Test case for default value of APA_PackUQ", () =>
			{
				AssertEquals("Default value for APA_PackUQ", "BI", pack.APA_PackUQ);
				var pack2 = bill.Packs.AddNew();
				AssertEquals("Default value for APA_PackUQ should be BI again", "BI", pack2.APA_PackUQ);
			});
		}

		public void TestCreateNewAsycudaPackedItemCollectionType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertType<AsycudaPackedItemCollection>("PackedItemsForBinding must be TR.Manifest.Business.AsycudaPackedItemCollection", pack.PackedItemsForBinding);
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
