using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PackProduct))]
	sealed class PackProductTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingPackLine packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct result = packline.Products.AddNew();
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var packline = factory.NewWithValidTestData<ForwardingPackLine>();
			var result = packline.Products.AddNew();
			return result;
		}

		#endregion

		public void TestProductForImportPackLineWithMatchedConsignee()
		{
			var orgSupplierPartFirstSaved = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPartFirstSaved.OP_PartNum = "Ice cream";
			orgSupplierPartFirstSaved.OP_Desc = "First product";
			Factory.Save();
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Ice cream";
			orgSupplierPart.OP_Desc = "Second product";
			Factory.Save();
			Factory.CleanUp();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			var product = packline.Products.AddNew();
			var parentShipment = packline.Shipment;

			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var orgPartRelationSupplier = orgSupplierPartFirstSaved.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			parentShipment.ConsigneePK = orgPartRelationOwner.OU_OH;
			parentShipment.ConsignorPK = orgPartRelationSupplier.OU_OH;
			product.D2_ProductCode = "Ice cream";

			AssertEquals("Second product", product.Product.OP_Desc);
		}

		public void TestProductForExportPackLineWithMatchedConsignor()
		{
			var orgSupplierPartFirstSaved = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPartFirstSaved.OP_PartNum = "Ice cream";
			orgSupplierPartFirstSaved.OP_Desc = "First product";
			Factory.Save();
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Ice cream";
			orgSupplierPart.OP_Desc = "Second product";
			Factory.Save();
			Factory.CleanUp();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			var product = packline.Products.AddNew();
			var parentShipment = packline.Shipment;
			parentShipment.JS_RL_NKOrigin = $"{GlbBranch.CurrentBranch.Country.Code}AAA";
			parentShipment.JS_RL_NKDestination = "BBB";

			var orgPartRelationOwner = orgSupplierPartFirstSaved.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var orgPartRelationSupplier = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			parentShipment.ConsigneePK = orgPartRelationOwner.OU_OH;
			parentShipment.ConsignorPK = orgPartRelationSupplier.OU_OH;
			product.D2_ProductCode = "Ice cream";

			AssertEquals("Second product", product.Product.OP_Desc);
		}

		public void TestProductForImportPackLineWithoutConsignee()
		{
			var orgSupplierPartFirstSaved = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPartFirstSaved.OP_PartNum = "Ice cream";
			orgSupplierPartFirstSaved.OP_Desc = "First product";
			Factory.Save();
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Ice cream";
			orgSupplierPart.OP_Desc = "Second product";
			Factory.Save();
			Factory.CleanUp();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			var product = packline.Products.AddNew();
			product.D2_JO = Factory.NewWithValidTestData<OrderLine>().PK;
			var parentOrder = product.OrderLine.Order;

			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var orgPartRelationSupplier = orgSupplierPartFirstSaved.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			parentOrder.BuyerPK = orgPartRelationOwner.OU_OH;
			parentOrder.SupplierPK = orgPartRelationSupplier.OU_OH;
			product.D2_ProductCode = "Ice cream";

			AssertEquals("Second product", product.Product.OP_Desc);
		}

		public void TestProductForExportPackLineWithoutConsignor()
		{
			var orgSupplierPartFirstSaved = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPartFirstSaved.OP_PartNum = "Ice cream";
			orgSupplierPartFirstSaved.OP_Desc = "First product";
			Factory.Save();
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Ice cream";
			orgSupplierPart.OP_Desc = "Second product";
			Factory.Save();
			Factory.CleanUp();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			var product = packline.Products.AddNew();
			product.D2_JO = Factory.NewWithValidTestData<OrderLine>().PK;
			var parentOrder = product.OrderLine.Order;
			var parentShipment = packline.Shipment;
			parentOrder.JD_JS = parentShipment.PK;
			parentShipment.JS_RL_NKOrigin = $"{GlbBranch.CurrentBranch.Country.Code}AAA";
			parentShipment.JS_RL_NKDestination = "BBB";

			var orgPartRelationOwner = orgSupplierPartFirstSaved.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var orgPartRelationSupplier = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationSupplier.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			parentOrder.BuyerPK = orgPartRelationOwner.OU_OH;
			parentOrder.SupplierPK = orgPartRelationSupplier.OU_OH;
			product.D2_ProductCode = "Ice cream";

			AssertEquals("Second product", product.Product.OP_Desc);
		}

		public void TestUNGDsFromProductAreAddedToPackline()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "ABC";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs1 = UNDGSubstanceLoader.LoadSubstances(Factory, "ABC", standard: "IMO").FirstOrDefault();
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "DEF";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs3 = Factory.New<UNDGSubstance>();
			subs3.DG_UNNO = "JKL";
			subs3.DG_Variant = "";
			subs3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			OrgSupplierPart orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var supplierOrgPK1 = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault().OU_OH;

			orgSupplierPart.OP_PartNum = "Ice cream";

			UNDGDataItem undg1 = orgSupplierPart.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_DGFlashPoint = 32m;
			undg1.DI_TechnicalName = "dangerous";
			subs1.DG_MP = UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code;
			undg1.DI_MPMarinePollutant = "ABC";

			UNDGDataItem undg2 = orgSupplierPart.UNDGs.AddNew();
			undg2.DI_DG = subs2.PK;
			undg2.LinkDefault(subs2);

			orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var supplierOrgPK2 = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault().OU_OH;
			orgSupplierPart.OP_PartNum = "Cookies";
			UNDGDataItem undg3 = orgSupplierPart.UNDGs.AddNew();
			undg3.DI_DG = subs3.PK;
			undg3.LinkDefault(subs3);

			ForwardingPackLine packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct product = packline.Products.AddNew();

			AssertEquals("No UNDGs for packline", 0, packline.UNDGs.Count);

			var shipment = packline.Shipment;
			shipment.ConsignorPK = supplierOrgPK1;
			product.D2_ProductCode = "Ice cream";
			AssertEquals("UNDGs have been copied from product to packline", 2, packline.UNDGs.Count);

			AssertEquals("UNDGs details dave been copied from product to packline", 32m, packline.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("UNDGs details dave been copied from product to packline", "dangerous", packline.UNDGs[0].DI_TechnicalName);
			AssertEquals("UNDGs details dave been copied from product to packline", "ABC", packline.UNDGs[0].DI_MPMarinePollutant);

			shipment.ConsignorPK = supplierOrgPK2;
			product.D2_ProductCode = "Cookies";
			AssertEquals("UNDGs have been copied from product to packline", 3, packline.UNDGs.Count);

			shipment.ConsignorPK = supplierOrgPK1;
			product.D2_ProductCode = "Ice cream";
			AssertEquals("UNDGs should not copied from product to packline, bacause already exists on Packline.UNDGs", 3, packline.UNDGs.Count);
		}

		public void TestInfoIsCopiedFromOrderLineToPackProduct()
		{
			Order order1 = Factory.New<Order>();
			Order order2 = Factory.New<Order>();

			OrderLine line11 = order1.OrderLines.AddNew();
			line11.JO_Partno = "Product11";
			line11.JO_Quantity = 11m;
			line11.JO_F3_NKPackType = "BOX";

			OrderLine line12 = order1.OrderLines.AddNew();
			line12.JO_Partno = "Product12";
			line12.JO_Quantity = 12m;
			line12.JO_F3_NKPackType = "PLT";

			OrderLine line21 = order2.OrderLines.AddNew();
			line21.JO_Partno = "Product21";
			line21.JO_Quantity = 21m;
			line21.JO_F3_NKPackType = "UNT";

			ForwardingPackLine packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			PackProduct product = packline.Products.AddNew();

			product.D2_JO = line11.PK;
			AssertEquals("Product code", "Product11", product.D2_ProductCode);
			AssertEquals("Product quantity", 11m, product.D2_ProductQuantity);
			AssertEquals("Product unit of quantity", "BOX", product.D2_ProductUnitOfQty);

			product.D2_JO = line12.PK;
			AssertEquals("Product code", "Product12", product.D2_ProductCode);
			AssertEquals("Product quantity", 12m, product.D2_ProductQuantity);
			AssertEquals("Product unit of quantity", "PLT", product.D2_ProductUnitOfQty);
		}

		public void TestUNGDsFromProductAreAddedToPackline_WhenUNDGNotExistsMoreThanOne()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "XXX";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "999";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Ice cream";

			var partUndg1 = orgSupplierPart.UNDGs.AddNew();
			partUndg1.DI_DG = subs.PK;
			partUndg1.LinkDefault(subs);
			partUndg1.DI_TechnicalName = "123";

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			var product = packline.Products.AddNew();
			product.D2_ProductCode = "Cookies";
			AssertEquals("No UNDGs for packline", 0, packline.UNDGs.Count);

			var undg1 = packline.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.DI_TechnicalName = "ABC";

			var undg2 = packline.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			undg2.DI_TechnicalName = "DEF";

			product.D2_ProductCode = "Ice cream";
			AssertEquals("Should have 2 lines of UNDG", 2, packline.UNDGs.Count);
			AssertEquals(true, packline.UNDGs.Any(x => x.DI_TechnicalName == "ABC"));
			AssertEquals(true, packline.UNDGs.Any(x => x.DI_TechnicalName == "DEF"));
			AssertEquals(false, packline.UNDGs.Any(x => x.DI_TechnicalName == "123"));

			product.D2_ProductCode = "Cookies";
			AssertEquals("Should have 2 lines of UNDG", 2, packline.UNDGs.Count);

			packline.Shipment.ConsignorPK = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault().OU_OH;
			partUndg1.DI_DG = subs2.PK;
			partUndg1.LinkDefault(subs2);
			product.D2_ProductCode = "Ice cream";
			AssertEquals("Should have 3 lines of UNDG", 3, packline.UNDGs.Count);
			AssertEquals(true, packline.UNDGs.Any(x => x.DI_TechnicalName == "ABC"));
			AssertEquals(true, packline.UNDGs.Any(x => x.DI_TechnicalName == "DEF"));
			AssertEquals(true, packline.UNDGs.Any(x => x.DI_TechnicalName == "123"));
		}
	}
}
