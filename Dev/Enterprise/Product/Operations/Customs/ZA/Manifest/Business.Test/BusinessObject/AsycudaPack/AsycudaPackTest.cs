using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDeleteExistingPackedItems()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALH, ApplicationCodeTypeList.Codes.Consolidator);
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem1 = ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(pack1);
			var packedItem2 = ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(pack1);
			var pack2 = bill.Packs.AddNew();
			pack2.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var packedItem3 = ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(pack2);
			var packedItem4 = ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(pack2);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			if (ErrorReporter.LastMessageReported.StartsWith("Changing GlobalManifestApplicationKey from 'ALH' to 'ZAALH'"))
			{
				ErrorReporter.Clear();
			}

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var zaHeader = newFactory.Load<ASYCUDA.Business.AsycudaManifestHeader>(header.PK);
			AssertType<AsycudaManifestHeader>(zaHeader);
			AssertEquals(1, zaHeader.Bills.Count);
			var zaBill = zaHeader.Bills[0];
			AssertEquals(2, zaBill.Packs.Count);
			var zaPack1 = (AsycudaPack)zaBill.Packs.FindByPK(pack1.PK);
			AssertEquals(0, zaPack1.PackedItems.Count);
			var allPackedItems = zaPack1.GetAllPackedItems();
			AssertEquals(2, allPackedItems.Length);
			zaPack1.Delete();
			AssertEquals(true, allPackedItems[0].IsDeleted);
			AssertEquals(true, allPackedItems[1].IsDeleted);
			var zaPack2 = (AsycudaPack)zaBill.Packs.FindByPK(pack2.PK);
			AssertEquals(0, zaPack2.PackedItems.Count);
			zaPack2.Delete();
			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestAsycudaPackedItemIsNotSupported()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(true, pack.IsNonePackedItemRelationship);
			AssertNull(pack.PackedItem);
			var packedItem = ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(pack);
			Factory.Save();
			AssertEquals(true, packedItem.IsDeleted);
		}

		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestDuplicateGenPivotAddedForAsycudaBillABLEntryNumberAndLinkedPacks()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.RFM);
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "1234";
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 12;
			header.RunPreSaveValidationWithFetchHints();
			Factory.Save();
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 15;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertType<AsycudaPackLookups>(pack.Lookups);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
