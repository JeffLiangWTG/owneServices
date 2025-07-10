using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDeleteExistingPackedItems()
		{
			var header = (ManifestBase.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var packedItem1 = pack1.PackedItems.AddNewPackedItem();
			var packedItem2 = pack1.PackedItems.AddNewPackedItem();
			var pack2 = bill.Packs.AddNew();
			var packedItem3 = pack2.PackedItems.AddNewPackedItem();
			var packedItem4 = pack2.PackedItems.AddNewPackedItem();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			header.AMA_ApplicationCode = AsycudaManifestHeader.ApplicationCode_Out;
			if (ErrorReporter.LastMessageReported.StartsWith("Changing GlobalManifestApplicationKey from 'ALH' to 'ZAALH'"))
			{
				ErrorReporter.Clear();
			}

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var zaHeader = newFactory.Load<ManifestBase.AsycudaManifestHeader>(header.PK);
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
			var packedItem = pack.PackedItems.AddNewPackedItem();
			Factory.Save();
			AssertEquals(true, packedItem.IsDeleted);
		}

		public void TestIOutturnableLine()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.APA_LineNo = 1;
			pack.APA_PackQty = 0;
			AssertEquals("Pack: 1", ((IOutturnableLine)pack).UnderbondHumanReadableName);
			AssertEquals("", ((IOutturnableLine)pack).CargoStatus);
			AssertEquals(0, ((IOutturnableLine)pack).PackagesManifested);
		}

		public void TestHeader()
		{
			var pack1 = Factory.New<AsycudaPack>();
			AssertNull("Null header", pack1.Header);

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack2 = bill.Packs.AddNew();
			AssertEquals("Header", header, pack2.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			return pack;
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
