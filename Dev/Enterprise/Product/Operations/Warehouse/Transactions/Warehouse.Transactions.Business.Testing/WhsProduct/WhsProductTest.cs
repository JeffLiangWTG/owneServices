using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsProduct))]
	sealed class WhsProductTest : WhsNonPersistentBusinessObjectTestCase
	{
		public void TestGetWhsProduct_OnlyCreateOneWhsProductForSameOrgSupplierPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product1 = WhsProduct.GetWhsProduct(data.Part1);
			var product2 = WhsProduct.GetWhsProduct(data.Part1);
			var productWithPk = WhsProduct.GetWhsProduct(Factory, data.Part1.PK);

			AssertEquals(product1, product2);
			AssertEquals(product1, productWithPk);
		}

		public void TestGetWhsProduct_FromPK_ThrowsWithNoFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => WhsProduct.GetWhsProduct(null, data.Part1.PK));
		}

		public void TestGetWhsProduct_FromPK_DifferentFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var productWithPk = WhsProduct.GetWhsProduct(Factory, data.Part1.PK);
			var productWithPkOtherFactory = WhsProduct.GetWhsProduct(otherFactory, data.Part1.PK);
			AssertEquals(productWithPk.Parent.PK, productWithPkOtherFactory.Parent.PK);
			AssertNotEquals(productWithPk, productWithPkOtherFactory);

			AssertEquals(Factory, productWithPk.Parent.Factory);
			AssertEquals(otherFactory, productWithPkOtherFactory.Factory);
		}

		public void TestGetWhsProduct_FromPK_NoOrgSupplierPart()
		{
			AssertNull(WhsProduct.GetWhsProduct(Factory, ZGuid.Empty));
			AssertNull(WhsProduct.GetWhsProduct(Factory, ZGuid.Invalid));
			AssertNull(WhsProduct.GetWhsProduct(Factory, ZGuid.BrettsGuid));
		}

		public void TestGetWhsProduct_FromPK_DeletedBeforeCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.Delete();

			AssertNull(WhsProduct.GetWhsProduct(Factory, data.Part1.PK));
		}

		public void TestGetWhsProduct_FromPK_DeletedAfterCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsProduct.GetWhsProduct(Factory, data.Part1.PK);

			// This code is used in place of a load + call to GetWhsProduct, so we check delete for equivalent code
			data.Part1.Delete();
			AssertNull(WhsProduct.GetWhsProduct(Factory, data.Part1.PK));
		}

		public void TestOperationalActionsFieldToShowVisibility()
		{
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProduct).GetProperty("Warehouses")));
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProduct).GetProperty("PickFaces")));
		}

		public void TestDelete()
		{
			WhsPickFace face = Product.PickFaces.AddNew();
			WhsProductParamsByWhsAndClient @params = Product.ParamsByWhsAndClient.AddNew();

			AssertEquals(1, Product.PickFaces.Count);
			AssertEquals(1, Product.ParamsByWhsAndClient.Count);

			Product.Delete();

			AssertEquals(0, Product.PickFaces.Count);
			AssertEquals(0, Product.ParamsByWhsAndClient.Count);
			AssertEquals(true, face.IsDeleted);
			AssertEquals(true, @params.IsDeleted);
		}

		public void TestGetOwnerRelationship_Owner()
		{
			var client = Factory.New<OrgHeader>();

			var part = CreateProductWithoutRelationship();
			var product = WhsProduct.GetWhsProduct(part);

			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals(relation, product.GetOwnerRelationship(client));
		}

		public void TestGetOwnerRelationship_Both()
		{
			var client = Factory.New<OrgHeader>();

			var part = CreateProductWithoutRelationship();
			var product = WhsProduct.GetWhsProduct(part);

			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Both);

			AssertEquals(relation, product.GetOwnerRelationship(client));
		}

		public void TestGetOwnerRelationship_Supplier()
		{
			var client = Factory.New<OrgHeader>();

			var part = CreateProductWithoutRelationship();
			var product = WhsProduct.GetWhsProduct(part);

			part.RelatedOrganisations.AddOrganisationIfNotExist(client.PK, OrgPartRelation.RelationshipTypes.Supplier);

			AssertNull(product.GetOwnerRelationship(client));
		}

		public void TestGetOwnerRelationship_NoRelationship()
		{
			var client = Factory.New<OrgHeader>();

			var part = CreateProductWithoutRelationship();
			var product = WhsProduct.GetWhsProduct(part);

			AssertNull(product.GetOwnerRelationship(client));
		}

		public void TestGetOwnerRelationship_NullClient()
		{
			var part = CreateProductWithoutRelationship();
			var product = WhsProduct.GetWhsProduct(part);

			AssertExceptionThrown<ArgumentNullException>(() => product.GetOwnerRelationship(null));
		}

		public void TestGetOverridenCartonGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertNull(product.GetOverridenCartonGroup(data.Org1));

			var relationshipOwner = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
			AssertNull(product.GetOverridenCartonGroup(data.Org1));

			var cartonGroup1 = Helper.CreateWhsCartonGroup("1", "1");
			relationshipOwner.OU_WCG_CartonGroup = cartonGroup1.PK;
			AssertEquals(cartonGroup1, product.GetOverridenCartonGroup(data.Org1));

			var cartonGroup2 = Helper.CreateWhsCartonGroup("2", "2");
			var relationshipSupplier = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			relationshipSupplier.OU_WCG_CartonGroup = cartonGroup2.PK;
			AssertEquals("Supplier relationship should be ignored.", cartonGroup1, product.GetOverridenCartonGroup(data.Org1));

			var org2 = Helper.CreateClient("2");
			AssertNull(product.GetOverridenCartonGroup(org2));

			var cartonGroup3 = Helper.CreateWhsCartonGroup("3", "3");
			var relationshipBoth = data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(org2.PK, OrgPartRelation.RelationshipTypes.Both);
			relationshipBoth.OU_WCG_CartonGroup = cartonGroup3.PK;

			AssertEquals(cartonGroup3, product.GetOverridenCartonGroup(org2));
			AssertEquals(cartonGroup1, product.GetOverridenCartonGroup(data.Org1));
		}

		public void TestWarehouses()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A", 1, 1);
			AssertNotNull(Product.Warehouses);
			AssertEquals(true, Product.Warehouses.Contains(whs.PK));
			AssertEquals(true, Product.IsRegisteredEditableChildObject(Product.Warehouses));
		}

		public void TestPickFaces()
		{
			var whs = Helper.CreateWarehouse("1", "A", 1, 1);
			Factory.Save();

			var pickFace = Helper.CreateProductPickFace(Product, Client, whs, "A");

			var pickFaces = Product.PickFaces;
			AssertNotNull(pickFaces);
			AssertEquals(1, pickFaces.Count);
		}

		public void TestPickFaces_RegisterAsChildEditable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals("Precondition", false, product.HasChanges);

			product.PickFaces.AddNew();
			AssertEquals(true, product.HasChanges);
		}

		public void TestChangingDecimalPlacesUpdateReplenishmentMultiple()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var orgSupplierPart = Helper.CreateProduct(data.Org1, "P1");
			AssertEquals("Precondition", ZByte.Zero, orgSupplierPart.OP_CountDecimalPlaces);

			var product = WhsProduct.GetWhsProduct(orgSupplierPart);
			var pickFace1 = Helper.CreateProductPickFace(product, data.Org1, location1, 1m, 3m, 1m);
			var pickFace2 = Helper.CreateProductPickFace(product, data.Org1, location1, 1m, 4m, 2m);

			orgSupplierPart.OP_CountDecimalPlaces = 3;
			AssertEquals(1.000m, pickFace1.WF_ReplenishmentMultiple);
			AssertEquals(2.000m, pickFace2.WF_ReplenishmentMultiple);

			pickFace1.WF_ReplenishmentMultiple = 1.555m;
			pickFace2.WF_ReplenishmentMultiple = 2.444m;
			orgSupplierPart.OP_CountDecimalPlaces = 2;
			AssertEquals(1.56m, pickFace1.WF_ReplenishmentMultiple);
			AssertEquals(2.44m, pickFace2.WF_ReplenishmentMultiple);

			pickFace2.WF_ReplenishmentMultiple = 2.00m;
			orgSupplierPart.OP_CountDecimalPlaces = 1;
			AssertEquals(1.6m, pickFace1.WF_ReplenishmentMultiple);
			AssertEquals(2.0m, pickFace2.WF_ReplenishmentMultiple);

			orgSupplierPart.OP_CountDecimalPlaces = 3;
			AssertEquals(1.600m, pickFace1.WF_ReplenishmentMultiple);
			AssertEquals(2.000m, pickFace2.WF_ReplenishmentMultiple);
		}

		public void TestParamsByWhsAndClient()
		{
			WhsProductParamsByWhsAndClient o = Factory.New<WhsProductParamsByWhsAndClient>();
			o.W3_OH = Client.PK;
			o.W3_OP = Product.Parent.PK;

			WhsProductParamsByWhsAndClientCollection oo = Product.ParamsByWhsAndClient;
			AssertNotNull(oo);
			AssertEquals(1, oo.Count);
			AssertEquals(true, Product.IsRegisteredEditableChildObject(Product.ParamsByWhsAndClient));
		}

		public void TestParamsByWhsAndClient_RegisterAsChildEditable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals("Precondition", false, product.HasChanges);

			product.ParamsByWhsAndClient.AddNew();
			AssertEquals(true, product.HasChanges);
		}

		public void TestProductStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertNull(product.ProductStyle);

			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var size = style.Sizes.AddNew();
			product.ProductStylePK = style.PK;
			AssertEquals(style, product.ProductStyle);
		}

		public void TestIsAnyPartAttribReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(false, product.IsAnyPartAttribReleaseCaptured(data.Org1));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			AssertEquals(true, product.IsAnyPartAttribReleaseCaptured(data.Org1));
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: false); // clean up

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			AssertEquals(true, product.IsAnyPartAttribReleaseCaptured(data.Org1));
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: false); // clean up

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			AssertEquals(true, product.IsAnyPartAttribReleaseCaptured(data.Org1));
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: false); // clean up

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(true, product.IsAnyPartAttribReleaseCaptured(data.Org1));
		}

		public void TestIsPartAttribReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(false, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals(false, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals(false, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			AssertEquals(false, product.IsSerialNumberReleaseCaptured(data.Org1));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(true, product.IsPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals(true, product.IsPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals(true, product.IsPartAttribReleaseCaptured(data.Org1, 3));
			AssertEquals(true, product.IsSerialNumberReleaseCaptured(data.Org1));
		}

		public void TestIsMandatoryPartAttribReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, mandatoryAttributeType: true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, mandatoryAttributeType: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, mandatoryAttributeType: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, use: true, setReleaseCaptured: false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, use: true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, use: true, setReleaseCaptured: true);

			var product = WhsProduct.GetWhsProduct(data.Part1);

			AssertEquals(expected: false, product.IsMandatoryPartAttribReleaseCaptured(data.Org1, 1));
			AssertEquals(expected: false, product.IsMandatoryPartAttribReleaseCaptured(data.Org1, 2));
			AssertEquals(expected: true, product.IsMandatoryPartAttribReleaseCaptured(data.Org1, 3));
		}

		public void TestIsPartAttributeUsed()
		{
			var client = Helper.CreateClient("AA3");
			var product = WhsProduct.GetWhsProduct(Helper.CreateProduct(client, "P1"));
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMUseSerialNumber = true;

			AssertEquals(false, product.IsPartAttributeUsed(client, 1));
			AssertEquals(false, product.IsPartAttributeUsed(client, 2));
			AssertEquals(false, product.IsPartAttributeUsed(client, 3));
			AssertEquals(false, product.IsSerialNumberUsed(client));

			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 1, true);
			AssertEquals(true, product.IsPartAttributeUsed(client, 1));
			AssertEquals(false, product.IsPartAttributeUsed(client, 2));
			AssertEquals(false, product.IsPartAttributeUsed(client, 3));
			AssertEquals(false, product.IsSerialNumberUsed(client));

			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 2, true);
			AssertEquals(true, product.IsPartAttributeUsed(client, 1));
			AssertEquals(true, product.IsPartAttributeUsed(client, 2));
			AssertEquals(false, product.IsPartAttributeUsed(client, 3));
			AssertEquals(false, product.IsSerialNumberUsed(client));

			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 1, false);
			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 2, false);
			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 3, true);
			client.PartAttributeManager.SetProductToUseAttribute(product.Parent, 6, true);
			AssertEquals(false, product.IsPartAttributeUsed(client, 1));
			AssertEquals(false, product.IsPartAttributeUsed(client, 2));
			AssertEquals(true, product.IsPartAttributeUsed(client, 3));
			AssertEquals(true, product.IsSerialNumberUsed(client));
		}

		public void TestIsAnyAttributeUsed()
		{
			var client = Helper.CreateClient("AA3");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMUseExpiryDate = true;
			client.MiscServ.OM_IMUsePackingDate = true;
			client.MiscServ.OM_IMUseSerialNumber = true;

			AssertEquals(false, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 1, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 1, false);
			client.PartAttributeManager.SetProductToUseAttribute(part, 2, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 2, false);
			client.PartAttributeManager.SetProductToUseAttribute(part, 3, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 3, false);
			client.PartAttributeManager.SetProductToUseAttribute(part, 4, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 4, false);
			client.PartAttributeManager.SetProductToUseAttribute(part, 5, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
			client.PartAttributeManager.SetProductToUseAttribute(part, 5, false);
			client.PartAttributeManager.SetProductToUseAttribute(part, 6, true);
			AssertEquals(true, product.IsAnyAttributeUsed(client));
		}

		public void TestIsAnyAttributeUsed_SerialNumber()
		{
			var client = Helper.CreateClient("AA3");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);

			client.MiscServ.OM_IMUseSerialNumber = true;
			AssertEquals("Precondition: No attributes used", false, product.IsAnyAttributeUsed(client));

			client.PartAttributeManager.SetProductToUseAttribute(part, 6, true);
			AssertEquals("Serial Number used", true, product.IsAnyAttributeUsed(client));
		}

		public void TestIsPartAttributeAJulianBatchNumberAndUsed()
		{
			var client = Helper.CreateClient("CLIENT");
			var part = Helper.CreateProduct(client, "P1");
			var product = WhsProduct.GetWhsProduct(part);
			part.RelatedOrganisations.AddOwner(client);

			AssertIsPartAttributeAJulianBatchNumberAndUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, 1);
			AssertIsPartAttributeAJulianBatchNumberAndUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, 2);
			AssertIsPartAttributeAJulianBatchNumberAndUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, 3);

			var client2 = Factory.New<OrgHeader>();
			var product2 = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			AssertEquals("Should handle unrelated clients", false, product2.IsAJulianBatchNumberAttributeUsed(client2));
			AssertEquals("Should handle null clients", false, product2.IsAJulianBatchNumberAttributeUsed(null));
		}

		public void TestIsAJulianBatchNumberAttributeUsed()
		{
			var client = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			var product = WhsProduct.GetWhsProduct(part);
			part.RelatedOrganisations.AddOwner(client);

			AssertIsAJulianBatchNumberAttributeUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1);
			AssertIsAJulianBatchNumberAttributeUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2);
			AssertIsAJulianBatchNumberAttributeUsed(client, product, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3);

			var client2 = Factory.New<OrgHeader>();
			var product2 = WhsProduct.GetWhsProduct(Factory.New<OrgSupplierPart>());
			AssertEquals("Should handle unrelated clients", false, product2.IsAJulianBatchNumberAttributeUsed(client2));
			AssertEquals("Should handle null clients", false, product2.IsAJulianBatchNumberAttributeUsed(null));
		}

		public void TestIsAJulianBatchNumberAttributeUsedAndHasAnyStock()
		{
			var client = Helper.CreateClient("CLIENT");
			var whs = Helper.CreateWarehouse("WH1");
			Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);

			var part_NoJulian_NoStock = Helper.CreateProduct(client, "P1");
			var part_NoJulian_WithStock = Helper.CreateProduct(client, "P2");
			var part_Julian_NoStock = Helper.CreateProduct(client, "P3");
			var part_Julian_WithStock = Helper.CreateProduct(client, "P4");

			Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(client, part_Julian_NoStock, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(client, part_Julian_WithStock, AttributeNumber.One, true);

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part_NoJulian_WithStock, 10m, true, false);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", part_Julian_WithStock, 10m, true, false);

			Factory.Save();

			AssertEquals(false, WhsProduct.GetWhsProduct(part_NoJulian_NoStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(client, whs));
			AssertEquals(false, WhsProduct.GetWhsProduct(part_NoJulian_WithStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(client, whs));
			AssertEquals(false, WhsProduct.GetWhsProduct(part_Julian_NoStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(client, whs));
			AssertEquals(true, WhsProduct.GetWhsProduct(part_Julian_WithStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(client, whs));
			AssertEquals(false, WhsProduct.GetWhsProduct(part_Julian_WithStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(null, whs));
			AssertEquals(false, WhsProduct.GetWhsProduct(part_Julian_WithStock).IsAJulianBatchNumberAttributeUsedAndHasAnyStock(client, null));
		}

		public void TestIsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(false, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(null, null));
			AssertEquals(false, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(data.Org1, null));
			AssertEquals(false, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(null, data.Whs1));
			AssertEquals(false, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(data.Org1, data.Whs1));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			AssertEquals(true, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(data.Org1, data.Whs1));

			var productParams = product.ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			AssertEquals(true, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(data.Org1, data.Whs1));

			productParams.W3_MaximumShelfLife = 10;
			AssertEquals(false, product.IsAJulianBatchNumberUsedAndMaxShelfLifeNotSpecified(data.Org1, data.Whs1));
		}

		public void TestIsExpiryDateUsed()
		{
			Client.MiscServ.OM_IMUseExpiryDate = true;
			AssertEquals(false, Product.IsExpiryDateUsed(Client));
			Client.PartAttributeManager.SetProductToUseAttribute(Product.Parent, 4, true);
			AssertEquals(true, Product.IsExpiryDateUsed(Client));
		}

		public void TestIsPackingDateUsed()
		{
			Client.MiscServ.OM_IMUsePackingDate = true;
			AssertEquals(false, Product.IsPackingDateUsed(Client));
			Client.PartAttributeManager.SetProductToUseAttribute(Product.Parent, 5, true);
			AssertEquals(true, Product.IsPackingDateUsed(Client));
		}

		public void TestExpiryDateFormatString()
		{
			AssertEquals(ZString.Empty, Product.ExpiryDateFormatString(null));
			AssertEquals(ZString.Empty, Product.ExpiryDateFormatString(Client));
			Client.MiscServ.OM_IMUseExpiryDate = true;
			Client.PartAttributeManager.SetProductToUseAttribute(Product.Parent, 4, true);
			Product.Parent.RelatedOrganisations[0].OU_ExpiryDateFormatString = "YYMMDD";
			AssertEquals("YYMMDD", Product.ExpiryDateFormatString(Client));
			Product.Parent.RelatedOrganisations[0].OU_ExpiryDateFormatString = "YYmmDD";
			AssertEquals("YYMMDD", Product.ExpiryDateFormatString(Client));
		}

		public void TestPackingDateFormatString()
		{
			AssertEquals(ZString.Empty, Product.PackingDateFormatString(null));
			AssertEquals(ZString.Empty, Product.PackingDateFormatString(Client));
			Client.MiscServ.OM_IMUsePackingDate = true;
			Client.PartAttributeManager.SetProductToUseAttribute(Product.Parent, 5, true);
			Product.Parent.RelatedOrganisations[0].OU_PackingDateFormatString = "DDMMYY";
			AssertEquals("DDMMYY", Product.PackingDateFormatString(Client));
			Product.Parent.RelatedOrganisations[0].OU_PackingDateFormatString = "DDmmYY";
			AssertEquals("DDMMYY", Product.PackingDateFormatString(Client));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHasProductImage()
		{
			var part = Factory.New<OrgSupplierPart>();
			var product = WhsProduct.GetWhsProduct(part);

			var image = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.JPG";
			var docManager = ((IDocManagerSupport)product.Parent).DocManagerInfo;

			docManager.AddFileOrDocument(image, "NNN");
			docManager.AllEDocs[0].IsPublished = true;
			AssertEquals("Precondition:", 1, docManager.AllEDocs.Count);
			AssertNull("Precondition:", product.ProductImage);

			docManager.AddFileOrDocument(image, Constants.RefDocTypes.ImageFile);
			docManager.AllEDocs[1].IsPublished = true;
			AssertEquals("Precondition:", 2, docManager.AllEDocs.Count);
			AssertNotNull("Precondition:", product.ProductImage);
		}

		public void TestIsSerialNumberUsedAndNotReleaseCaptured_SerialIsReleaseCaptured()
		{
			AssertIsSerialNumberUsedAndNotReleaseCapturedCore(isSerialReleaseCaptured: true);
		}

		public void TestIsSerialNumberUsedAndNotReleaseCaptured_SerialIsNotReleaseCaptured()
		{
			AssertIsSerialNumberUsedAndNotReleaseCapturedCore(isSerialReleaseCaptured: false);
		}

		public void TestIsSerialNumberUsedAndNotReleaseCaptured_SerialNotUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(false, product.IsSerialNumberUsed(data.Org1));
			AssertEquals(false, product.IsSerialNumberUsedAndNotReleaseCaptured(data.Org1));
		}

		public void TestIsAttributeNeutralUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(false, product.IsAttributeNeutralUsed(null));

			AssertEquals(false, product.IsAttributeNeutralUsed(data.Org1));

			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertEquals(true, product.IsAttributeNeutralUsed(data.Org1));
		}

		public void TestIsBOMProductPickedOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			AssertEquals("When a BOM product has 'OP_IsComponentPickedOnSalesOrder' set to true, IsBOMProductPickedOnSalesOrder should be true.", true, WhsProduct.GetWhsProduct(mainProduct).IsBOMProductPickedOnSalesOrder);

			mainProduct.OP_IsComponentPickedOnSalesOrder = false;
			AssertEquals("When a BOM product has 'OP_IsComponentPickedOnSalesOrder' set to false, IsBOMProductPickedOnSalesOrder should be false.", false, WhsProduct.GetWhsProduct(mainProduct).IsBOMProductPickedOnSalesOrder);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true; // clean up

			mainProduct.BillOfMaterials.DeleteAll();
			AssertEquals("When a non-BOM product has 'OP_IsComponentPickedOnSalesOrder' set to true, IsBOMProductPickedOnSalesOrder should be false.", false, WhsProduct.GetWhsProduct(mainProduct).IsBOMProductPickedOnSalesOrder);
		}

		public void TestIsCompletePalletPickingUsed()
		{
			AssertEquals(false, Product.IsCompletePalletPickingUsed(Client));

			Product.Parent.RelatedOrganisations[0].OU_CompletePalletPicking = true;
			AssertEquals(true, Product.IsCompletePalletPickingUsed(Client));

			OrgHeader client2 = Helper.CreateClient();
			AssertEquals("Should handle unrelated Clients", false, Product.IsCompletePalletPickingUsed(client2));
			AssertEquals("Should handle null Clients", false, Product.IsCompletePalletPickingUsed(null));
		}

		public void TestIsPackTypeUsedByProduct()
		{
			var client = Helper.CreateClient("TestClient");
			var supplierPart = Helper.CreateProduct("PROD", client);
			AssertEquals(true, supplierPart.PartUnits.IsNullOrEmpty());

			var product = WhsProduct.GetWhsProduct(supplierPart);
			AssertEquals(1, product.GetPackTypes().Count);

			supplierPart.OP_StockKeepingUnit = Constants.PkgUnit.Box;
			AssertEquals(true, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Box));

			var partUnit1 = Helper.CreateProductUnit(supplierPart, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 10);
			var partUnit2 = Helper.CreateProductUnit(supplierPart, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 20);

			AssertEquals(true, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Box));
			AssertEquals(true, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Pallet));
			AssertEquals(true, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Unit));

			AssertEquals(false, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Carton));
			AssertEquals(false, product.IsPackTypeUsedByProduct(Constants.PkgUnit.Bundle));
		}

		public void TestGetPackTypes()
		{
			var client = Helper.CreateClient("TestClient");
			var supplierPart = Helper.CreateProduct("PROD", client);
			AssertEquals(true, supplierPart.PartUnits.IsNullOrEmpty());

			var product = WhsProduct.GetWhsProduct(supplierPart);
			AssertEquals(1, product.GetPackTypes().Count);

			supplierPart.OP_StockKeepingUnit = Constants.PkgUnit.Box;
			AssertContains(Constants.PkgUnit.Box, product.GetPackTypes().CodesAsString);

			var partUnit1 = Helper.CreateProductUnit(supplierPart, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 10);
			var partUnit2 = Helper.CreateProductUnit(supplierPart, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 20);
			var partUnit3 = Helper.CreateProductUnit(supplierPart, "ABC", "DEF", 20);

			AssertContainsExactElementsInAnyOrder(new[] { Constants.PkgUnit.Unit, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, "ABC", "DEF" },
				product.GetPackTypes().GetAllCodes());
		}

		public void TestIsAValidJulianBatchNumberFormat()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");
			Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(client, part, AttributeNumber.One, true);
			var product = WhsProduct.GetWhsProduct(part);

			part.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(null, ""));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, ""));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "123"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "1234"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "1234a"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "1234abcde"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "123abc"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "a1234"));

			part.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YYDDD_BatchNumber;
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "1234"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "12345"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "12345a"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "12345abc"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "1234a"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "a1234"));

			part.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "123"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "1234"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "1234a"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "a1234"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "abcd1234"));

			part.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "1234"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "12345"));
			AssertEquals(false, product.IsAValidJulianBatchNumberFormat(client, "12345a"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "a12345"));
			AssertEquals(true, product.IsAValidJulianBatchNumberFormat(client, "abcd12345"));
		}

		public void TestHasAnyStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, false, false);
			var productWithStock = WhsProduct.GetWhsProduct(data.Part1);
			var productWithoutStock = WhsProduct.GetWhsProduct(data.Part2);

			Factory.Save();

			AssertEquals(true, productWithStock.HasAnyStock(data.Whs1, data.Org1));
			AssertEquals(false, productWithoutStock.HasAnyStock(data.Whs1, data.Org1));

			var wrongClient = Helper.CreateClient("CLIENT2");
			AssertEquals(false, productWithStock.HasAnyStock(data.Whs1, wrongClient));

			var wrongWarehouse = Helper.CreateWarehouse("WH2");
			AssertEquals(false, productWithStock.HasAnyStock(wrongWarehouse, data.Org1));

			AssertEquals(false, productWithStock.HasAnyStock(wrongWarehouse, null));
			AssertEquals(false, productWithStock.HasAnyStock(null, data.Org1));
		}

		public void TestQtyToStringFormat()
		{
			Product.Parent.OP_CountDecimalPlaces = 0;
			AssertEquals("0", "0", Product.QtyToStringFormat);

			Product.Parent.OP_CountDecimalPlaces = 1;
			AssertEquals("1", "0.0", Product.QtyToStringFormat);

			Product.Parent.OP_CountDecimalPlaces = 2;
			AssertEquals("2", "0.00", Product.QtyToStringFormat);

			Product.Parent.OP_CountDecimalPlaces = 3;
			AssertEquals("3", "0.000", Product.QtyToStringFormat);

			Product.Parent.OP_CountDecimalPlaces = 4;
			AssertEquals("4", "0.0000", Product.QtyToStringFormat);

			Product.Parent.OP_CountDecimalPlaces = 9;
			AssertEquals("9", "0.000000000", Product.QtyToStringFormat);
		}

		public void TestFormattedQtyAndUnit()
		{
			Product.Parent.OP_StockKeepingUnit = "BOX";
			Product.Parent.OP_CountDecimalPlaces = 1;
			AssertEquals("15.5 Boxes", Product.FormattedQtyAndUnit(15.5m));

			Product.Parent.OP_CountDecimalPlaces = 2;
			AssertEquals("1.00 Boxes", Product.FormattedQtyAndUnit(1m));

			Product.Parent.OP_CountDecimalPlaces = 0;
			AssertEquals("1 Box", Product.FormattedQtyAndUnit(1m));

			Product.Parent.OP_StockKeepingUnit = "";
			AssertEquals("15 Units", Product.FormattedQtyAndUnit(15.49m));
			AssertEquals("16 Units", Product.FormattedQtyAndUnit(15.5m));
		}

		public void TestFirstUNDG()
		{
			AssertNull(Product.FirstUNDG);

			var undg1 = Product.Parent.UNDGs.AddNew();
			var undg2 = Product.Parent.UNDGs.AddNew();

			AssertEquals(undg1, Product.FirstUNDG);
		}

		public void TestProductStylePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(ZGuid.Empty, product.ProductStylePK);
			AssertNull(product.ProductStyle);

			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var size = style.Sizes.AddNew();
			product.ProductStylePK = style.PK;
			AssertEquals(style.PK, product.ProductStylePK);
			AssertEquals(style, product.ProductStyle);
		}

		public void TestProductStyleColourPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(ZGuid.Empty, product.ProductStyleColourPK);
			AssertEquals(true, product.ProductStyleColourPKInfo.ReadOnly);
			AssertNull(product.ProductStyleColour);

			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var size = style.Sizes.AddNew();
			product.ProductStylePK = style.PK;
			AssertEquals(ZGuid.Empty, product.ProductStyleColourPK);
			AssertEquals(false, product.ProductStyleColourPKInfo.ReadOnly);
			AssertNull(product.ProductStyleColour);

			product.ProductStyleColourPK = colour.PK;
			AssertEquals(product.ProductStyleColourPK, product.ProductStyleColourPK);
			AssertEquals(false, product.ProductStyleColourPKInfo.ReadOnly);
			AssertEquals(colour, product.ProductStyleColour);

			product.ProductStylePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, product.ProductStyleColourPK);
			AssertEquals(true, product.ProductStyleColourPKInfo.ReadOnly);
			AssertNull(product.ProductStyleColour);
		}

		public void TestProductStyleClassificationPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(ZGuid.Empty, product.ProductStyleClassificationPK);
			AssertEquals(true, product.ProductStyleClassificationPKInfo.ReadOnly);
			AssertNull(product.ProductStyleClassification);

			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var classification = style.Classifications.AddNew();
			var size = style.Sizes.AddNew();
			product.ProductStylePK = style.PK;
			AssertEquals(ZGuid.Empty, product.ProductStyleClassificationPK);
			AssertEquals(false, product.ProductStyleClassificationPKInfo.ReadOnly);
			AssertNull(product.ProductStyleClassification);

			product.ProductStyleClassificationPK = classification.PK;
			AssertEquals(product.ProductStyleClassificationPK, product.ProductStyleClassificationPK);
			AssertEquals(false, product.ProductStyleClassificationPKInfo.ReadOnly);
			AssertEquals(classification, product.ProductStyleClassification);

			product.ProductStylePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, product.ProductStyleClassificationPK);
			AssertEquals(true, product.ProductStyleClassificationPKInfo.ReadOnly);
			AssertNull(product.ProductStyleClassification);
		}

		public void TestProductStyleSizePK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(ZGuid.Empty, product.ProductStyleSizePK);
			AssertEquals(true, product.ProductStyleSizePKInfo.ReadOnly);
			AssertNull(product.ProductStyleSize);

			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var size = style.Sizes.AddNew();
			product.ProductStylePK = style.PK;
			AssertEquals(ZGuid.Empty, product.ProductStyleSizePK);
			AssertEquals(false, product.ProductStyleSizePKInfo.ReadOnly);
			AssertNull(product.ProductStyleSize);

			product.ProductStyleSizePK = size.PK;
			AssertEquals(product.ProductStyleSizePK, product.ProductStyleSizePK);
			AssertEquals(false, product.ProductStyleSizePKInfo.ReadOnly);
			AssertEquals(size, product.ProductStyleSize);

			product.ProductStylePK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, product.ProductStyleSizePK);
			AssertEquals(true, product.ProductStyleSizePKInfo.ReadOnly);
			AssertNull(product.ProductStyleSize);
		}

		public void TestProductStyleOwner()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2", "Organization 2");
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals("", product.ProductStyleOwner);

			var styleForOrg1 = Helper.CreateProductStyle("A", "X", data.Org1.PK);
			var styleForOrg2 = Helper.CreateProductStyle("B", "Y", org2.PK);
			product.ProductStylePK = styleForOrg1.PK;
			AssertEquals(data.Org1.OH_FullName, product.ProductStyleOwner);

			product.ProductStylePK = styleForOrg2.PK;
			AssertEquals(org2.OH_FullName, product.ProductStyleOwner);

			product.ProductStylePK = ZGuid.Empty;
			AssertEquals("", product.ProductStyleOwner);
		}

		public void TestGetVolumeInM3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			data.Part1.OP_Cubic = 5m;
			AssertEquals("Should multiply OP_Cubic by units passed in.", 0m, product.GetVolumeInM3(0));
			AssertEquals("Should multiply OP_Cubic by units passed in.", 5m, product.GetVolumeInM3(1));
			AssertEquals("Should multiply OP_Cubic by units passed in.", 10m, product.GetVolumeInM3(2));

			data.Part1.OP_Cubic = 0m;
			data.Part1.OP_StockKeepingUnit = Enterprise.Core.Constants.Volume.CubicMetres;
			AssertEquals("Should return units passed in.", 0m, product.GetVolumeInM3(0));
			AssertEquals("Should return units passed in.", 1m, product.GetVolumeInM3(1));
			AssertEquals("Should return units passed in.", 2m, product.GetVolumeInM3(2));

			data.Part1.OP_StockKeepingUnit = Enterprise.Core.Constants.Volume.CubicCentimeters;
			AssertEquals("Should return units passed in as M3.", 0m, product.GetVolumeInM3(0));
			AssertEquals("Should return units passed in as M3.", 1m, product.GetVolumeInM3(1000000));
			AssertEquals("Should return units passed in as M3.", 2m, product.GetVolumeInM3(2000000));
		}

		public void TestIsVolumeUnitInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_CubicUQ = "0";
			AssertEquals("Expecting invalid volume unit", true, product.IsVolumeUnitInvalid);

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);

			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			data.Part1.OP_CubicUQ = "0";
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);

			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);
		}

		public void TestGetInvalidVolumeUnitErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var expectedString =
@"Invalid Volume Unit in this Product Code: XYZ. Please use following valid unit types:
CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE";

			data.Part1.OP_PartNum = "XYZ";
			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_CubicUQ = "0";
			AssertEquals("Expecting invalid volume unit", true, product.IsVolumeUnitInvalid);
			AssertEquals(expectedString, product.GetInvalidVolumeUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);
			AssertEquals("", product.GetInvalidVolumeUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			data.Part1.OP_CubicUQ = "0";
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);
			AssertEquals("", product.GetInvalidVolumeUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			AssertEquals("Expecting valid volume unit", false, product.IsVolumeUnitInvalid);
			AssertEquals("", product.GetInvalidVolumeUnitErrorMessage());
		}

		public void TestGetWeightInKG()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			data.Part1.OP_Weight = 5m;
			AssertEquals("Should multiply OP_Weight by units passed in.", 0m, product.GetWeightInKG(0));
			AssertEquals("Should multiply OP_Weight by units passed in.", 5m, product.GetWeightInKG(1));
			AssertEquals("Should multiply OP_Weight by units passed in.", 10m, product.GetWeightInKG(2));

			data.Part1.OP_Weight = 0m;
			data.Part1.OP_StockKeepingUnit = Enterprise.Core.Constants.Weight.Kilograms;
			AssertEquals("Should return units passed in.", 0m, product.GetWeightInKG(0));
			AssertEquals("Should return units passed in.", 1m, product.GetWeightInKG(1));
			AssertEquals("Should return units passed in.", 2m, product.GetWeightInKG(2));

			data.Part1.OP_StockKeepingUnit = Enterprise.Core.Constants.Weight.Grams;
			AssertEquals("Should return units passed in as KG.", 0m, product.GetWeightInKG(0));
			AssertEquals("Should return units passed in as KG.", 1m, product.GetWeightInKG(1000));
			AssertEquals("Should return units passed in as KG.", 2m, product.GetWeightInKG(2000));
		}

		public void TestIsWeightUnitInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_WeightUQ = "0";
			AssertEquals("Expecting invalid weight unit", true, product.IsWeightUnitInvalid);

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);

			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			data.Part1.OP_WeightUQ = "0";
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);

			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);
		}

		public void TestGetInvalidWeightUnitErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var expectedString =
@"Invalid Weight Unit in this Product Code: XYZ. Please use following valid unit types:
DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN";

			data.Part1.OP_PartNum = "XYZ";
			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_WeightUQ = "0";
			AssertEquals("Expecting invalid weight unit", true, product.IsWeightUnitInvalid);
			AssertEquals(expectedString, product.GetInvalidWeightUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = "0";
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);
			AssertEquals("", product.GetInvalidWeightUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			data.Part1.OP_WeightUQ = "0";
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);
			AssertEquals("", product.GetInvalidWeightUnitErrorMessage());

			data.Part1.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			AssertEquals("Expecting valid weight unit", false, product.IsWeightUnitInvalid);
			AssertEquals("", product.GetInvalidWeightUnitErrorMessage());
		}

		public void TestGetParamsByWhsAndClient()
		{
			WhsProductParamsByWhsAndClient productParams = Factory.New<WhsProductParamsByWhsAndClient>();
			WhsWarehouse warehouse = Factory.New<WhsWarehouse>();
			productParams.W3_OH = Client.PK;
			productParams.W3_OP = Product.Parent.PK;
			productParams.W3_WW = warehouse.PK;

			AssertNull(Product.GetParamsByWhsAndClient(Factory.New<WhsWarehouse>(), client));
			AssertNull(Product.GetParamsByWhsAndClient(warehouse, Factory.New<OrgHeader>()));
			AssertEquals(productParams, Product.GetParamsByWhsAndClient(warehouse, Client));

			AssertNull(Product.GetParamsByWhsAndClient(Factory.New<WhsWarehouse>().PK, client.PK));
			AssertNull(Product.GetParamsByWhsAndClient(warehouse.PK, Factory.New<OrgHeader>().PK));
			AssertEquals(productParams, Product.GetParamsByWhsAndClient(warehouse.PK, Client.PK));
		}

		public void TestGetPackingDateFromJulianBatchNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(1), 1, 1).AddDays((234 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC1234"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(2), 1, 1).AddDays((015 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC2015"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(3), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC3001"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(4), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC4001")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(00), 1, 1).AddDays((234 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC00234"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(05), 1, 1).AddDays((015 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC05015"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(13), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC13001"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(20), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "ABC20001")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(1), 1, 1).AddDays((234 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "1234ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(2), 1, 1).AddDays((015 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "2015ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(3), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "3001ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByDecade(4), 1, 1).AddDays((001 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "4001ABC")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YYDDD_BatchNumber;
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(00), 1, 1).AddDays((234 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "00234ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(05), 1, 1).AddDays((015 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "05015ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(13), 1, 1), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "13001ABC"));
			AssertEquals(new ZDate(ClosestYearInPastRoundByCentury(20), 1, 1), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "20001ABC")); // Future date

			var client2 = Helper.CreateClient("CLIENT2");
			var whs2 = Helper.CreateWarehouse("WH2");
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(null, data.Whs1, "12345")); // no client
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(data.Org1, null, "12345")); // no warehouse
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(data.Org1, whs2, "12345")); // incorrect warehouse
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(client2, data.Whs1, "12345")); // incorrect client
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "")); // no value specified
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "10000")); // 0 days
			AssertEquals(ZDate.Empty, product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "10366")); // more than 365 days
			AssertEquals(new ZDate(2000 + 04, 1, 1).AddDays((366 - 1)), product.GetPackingDateFromJulianBatchNumber(data.Org1, data.Whs1, "04366")); // more than 365 days, leap year.
		}

		[TestDate(2013, 2, 26)]
		public void TestCalculateExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParam.W3_MaximumShelfLife = 10;

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
			AssertEquals(new ZDateTime(2010 + 1, 1, 1).AddDays((234 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC1234"));
			AssertEquals(new ZDateTime(2010 + 2, 1, 1).AddDays((015 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC2015"));
			AssertEquals(new ZDateTime(2010 + 3, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC3001"));
			AssertEquals(new ZDateTime(2000 + 4, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC4001")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;
			AssertEquals(new ZDateTime(2000 + 00, 1, 1).AddDays((234 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC00234"));
			AssertEquals(new ZDateTime(2000 + 05, 1, 1).AddDays((015 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC05015"));
			AssertEquals(new ZDateTime(2000 + 13, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC13001"));
			AssertEquals(new ZDateTime(1900 + 20, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "ABC20001")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;
			AssertEquals(new ZDateTime(2010 + 1, 1, 1).AddDays((234 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "1234ABC"));
			AssertEquals(new ZDateTime(2010 + 2, 1, 1).AddDays((015 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "2015ABC"));
			AssertEquals(new ZDateTime(2010 + 3, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "3001ABC"));
			AssertEquals(new ZDateTime(2000 + 4, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "4001ABC")); // Future date

			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YYDDD_BatchNumber;
			AssertEquals(new ZDateTime(2000 + 00, 1, 1).AddDays((234 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "00234ABC"));
			AssertEquals(new ZDateTime(2000 + 05, 1, 1).AddDays((015 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "05015ABC"));
			AssertEquals(new ZDateTime(2000 + 13, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "13001ABC"));
			AssertEquals(new ZDateTime(1900 + 20, 1, 1).AddDays((001 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "20001ABC")); // Future date

			var client2 = Helper.CreateClient("CLIENT2");
			var whs2 = Helper.CreateWarehouse("WH2");
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(null, data.Whs1, "12345")); // no client
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(data.Org1, null, "12345")); // no warehouse
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(data.Org1, whs2, "12345")); // incorrect warehouse
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(client2, data.Whs1, "12345")); // incorrect client
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(data.Org1, data.Whs1, "")); // no value specified
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(data.Org1, data.Whs1, "10000")); // 0 days
			AssertEquals(ZDateTime.Empty, product.CalculateExpiryDate(data.Org1, data.Whs1, "10366")); // more than 365 days
			AssertEquals(new ZDateTime(2000 + 04, 1, 1).AddDays((366 - 1) + 10), product.CalculateExpiryDate(data.Org1, data.Whs1, "04366")); // more than 365 days, leap year.
		}

		public void TestValidationType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(typeof(WhsProductValidation), product.Validation.GetType());
		}

		public void TestRunPreSaveValidationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = WhsProduct.GetWhsProduct(data.Part1);
			var style = Helper.CreateProductStyle("HEY", "YOU");
			var colour = style.Colours.AddNew();
			var size = style.Sizes.AddNew();

			product.ProductStylePK = style.PK;
			product.ProductStyleColourPK = colour.PK;
			product.RunPreSaveValidation();
			AssertEquals("Ensure RunPreSaveValidation validates all", true, product.ProductStyleSizePKInfo.HasErrors());
		}

		public void TestICartonisableItemDefinitionMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Height = 1m;
			data.Part1.OP_Width = 2m;
			data.Part1.OP_Depth = 3m;
			data.Part1.OP_Weight = 5m;
			data.Part1.OP_MeasureUQ = Constants.Length.Metres;
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			data.Part1.OP_Cubic = 4m;

			var product = WhsProduct.GetWhsProduct(data.Part1);
			var productAsICartonisableItemDefinition = (ICartonisableItemDefinition)product;
			AssertEquals("PK", data.Part1.PK, productAsICartonisableItemDefinition.PK);
			AssertEquals("Height", 1m, productAsICartonisableItemDefinition.Height);
			AssertEquals("Width", 2m, productAsICartonisableItemDefinition.Width);
			AssertEquals("Length", 3m, productAsICartonisableItemDefinition.Length);
			AssertEquals("Volume", 4m, productAsICartonisableItemDefinition.Volume);
			AssertEquals("Weight", 5m, productAsICartonisableItemDefinition.Weight);
			AssertEquals("DimensionUQ", Constants.Length.Metres, productAsICartonisableItemDefinition.DimensionUQ);
			AssertEquals("VolumeUQ", Constants.Volume.CubicMetres, productAsICartonisableItemDefinition.VolumeUQ);
			AssertEquals("WeightUQ", Constants.Weight.Kilograms, productAsICartonisableItemDefinition.WeightUQ);

			data.Part1.OP_Weight = 0m;
			data.Part1.OP_StockKeepingUnit = Constants.Weight.Grams;
			AssertEquals("Weight", 1m, productAsICartonisableItemDefinition.Weight);
			AssertEquals("WeightUQ", Constants.Weight.Grams, productAsICartonisableItemDefinition.WeightUQ);

			data.Part1.OP_Cubic = 0m;
			data.Part1.OP_StockKeepingUnit = Constants.Volume.CubicDecimetres;
			AssertEquals("Volume", 1m, productAsICartonisableItemDefinition.Volume);
			AssertEquals("VolumeUQ", Constants.Volume.CubicDecimetres, productAsICartonisableItemDefinition.VolumeUQ);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WhsProduct.GetWhsProduct(Helper.CreateProduct(Client, "P1"));
		}

		WhsProduct Product
		{
			get { return product ?? (product = (WhsProduct)GetNewBusinessObject()); }
		}

		OrgHeader Client
		{
			get { return client ?? (client = Factory.New<OrgHeader>()); }
		}

		OrgHeader client;
		WhsProduct product;

		OrgSupplierPart CreateProductWithoutRelationship()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "P1";
			orgSupplierPart.OP_Desc = "P1";
			orgSupplierPart.OP_StockKeepingUnit = "UNT";
			orgSupplierPart.OP_CountDecimalPlaces = 0;
			orgSupplierPart.OP_Weight = 2.0m;
			orgSupplierPart.OP_Cubic = 0.02m;
			orgSupplierPart.OP_WeightUQ = "KG";
			orgSupplierPart.OP_CubicUQ = "M3";

			Helper.CreateProductUnit(orgSupplierPart, "CTN", 12);
			return orgSupplierPart;
		}

		void AssertIsPartAttributeAJulianBatchNumberAndUsed(OrgHeader client, WhsProduct product, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn, int attributeNumber)
		{
			AssertEquals(false, product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(false, product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber));

			product.Parent.RelatedOrganisations[0][usePartAttributeColumn] = true;
			AssertEquals(false, product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(true, product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber));

			product.Parent.RelatedOrganisations[0][usePartAttributeColumn] = false;
			AssertEquals(false, product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber));
		}

		void AssertIsAJulianBatchNumberAttributeUsed(OrgHeader client, WhsProduct product, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn)
		{
			var part = product.Parent;
			AssertEquals(false, product.IsAJulianBatchNumberAttributeUsed(client));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertEquals(false, product.IsAJulianBatchNumberAttributeUsed(client));

			client.MiscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertEquals(false, product.IsAJulianBatchNumberAttributeUsed(client));

			part.RelatedOrganisations[0][usePartAttributeColumn] = true;
			AssertEquals(true, product.IsAJulianBatchNumberAttributeUsed(client));

			part.RelatedOrganisations[0][usePartAttributeColumn] = false;
			AssertEquals(false, product.IsAJulianBatchNumberAttributeUsed(client));
		}

		void AssertIsSerialNumberUsedAndNotReleaseCapturedCore(bool isSerialReleaseCaptured)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: isSerialReleaseCaptured);

			var product = WhsProduct.GetWhsProduct(data.Part1);
			AssertEquals(!isSerialReleaseCaptured, product.IsSerialNumberUsedAndNotReleaseCaptured(data.Org1));
		}

		static int ClosestYearInPastRoundByDecade(int addYear) => ClosestYearInPastRoundBy(addYear, 10);

		static int ClosestYearInPastRoundByCentury(int addYear) => ClosestYearInPastRoundBy(addYear, 100);

		static int ClosestYearInPastRoundBy(int addYear, int roundBy)
		{
			var year = (ZDate.Today.Year / roundBy) * roundBy + addYear;
			return year - (year > ZDate.Today.Year ? roundBy : 0);
		}
	}
}
