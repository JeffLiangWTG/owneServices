using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class PackProductMergerTest
			: BusinessObjectMergerTest<PackProduct>
	{
		public override void TestMerge()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "AB";
			subs.DG_Variant = "C";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			orgSupplierPart.OP_PartNum = "DGSupplierPart";

			var undgDataItem = orgSupplierPart.UNDGs.AddNew();
			undgDataItem.LinkDefault(subs);
			undgDataItem.DI_DGFlashPoint = 20m;
			undgDataItem.DI_TechnicalName = "DGProductCode";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = orgPartRelationOwner.OU_OH;
			var packLine = shipment.OuterPackLines.AddNew();
			var newPackLine = shipment.OuterPackLines.AddNew();

			var product1 = packLine.Products.AddNew();
			product1.D2_ProductQuantity = 54m;
			product1.D2_ProductUnitOfQty = "BAG";

			var product2 = packLine.Products.AddNew();
			product2.D2_ProductQuantity = 13m;
			product2.D2_ProductUnitOfQty = "BAG";

			var product3 = packLine.Products.AddNew();
			product3.D2_ProductQuantity = 24m;
			product3.D2_ProductUnitOfQty = "BBG";

			var product4 = packLine.Products.AddNew();
			product4.D2_ProductQuantity = 3m;
			product4.D2_ProductUnitOfQty = "BAG";
			product4.D2_ProductCode = orgSupplierPart.OP_PartNum;

			var product5 = packLine.Products.AddNew();
			product5.D2_ProductQuantity = 3m;
			product5.D2_ProductUnitOfQty = "BAG";
			product5.D2_ProductCode = orgSupplierPart.OP_PartNum;

			var merger = new PackProductMerger(packLine.Products, newPackLine);
			merger.DoMerge();

			AssertEquals("Should remove all products from old packline", 0, packLine.Products.Count);
			AssertEquals("Should merge into the new packline", 4, newPackLine.Products.Count);

			var allPKs = newPackLine.Products.GetPKs();
			AssertCollectionContains("Product1 should merge with product2 as they have same parameteres", product1.PK, allPKs);
			AssertCollectionNotContains("Product2 should be deleted", product2.PK, allPKs);

			AssertEquals(67m, product1.D2_ProductQuantity);
			AssertEquals("BAG", product1.D2_ProductUnitOfQty);

			AssertCollectionContains("Product3 Should not be merged as it has different parametere with other products", product3.PK, allPKs);
			AssertCollectionContains("Product4 Should not be merged as its product has one DG item", product4.PK, allPKs);
			AssertCollectionContains("Product5 Should not be merged as its product has one DG item", product5.PK, allPKs);
		}

		public override void TestCheckMerge()
		{
			var merger = GetNewMerger();
			AssertEquals("There is no check for PackProductMerger", string.Empty, merger.CheckMerger());
		}

		protected override IEnumerable<SchemaColumn> GetAllSchemaColumns()
		{
			return JobPackProductSchema.All.Cast<SchemaColumn>();
		}

		protected override BusinessObjectMerger<PackProduct> GetNewMerger()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var newPackLine = shipment.OuterPackLines.AddNew();

			var list = new[]
			{
				packLine.Products.AddNew(),
				packLine.Products.AddNew()
			};

			return new PackProductMerger(list, newPackLine);
		}
	}
}
