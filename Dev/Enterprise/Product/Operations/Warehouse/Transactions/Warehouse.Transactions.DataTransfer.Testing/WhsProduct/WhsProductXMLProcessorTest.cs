using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class WhsProductXMLProcessorTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportExistingProduct()
		{
			var product = GetNewPopulatedProduct();
			var xsdProduct = MakeExport(product);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			AssertEquals(20m, product.OP_LastCost);
			AssertEquals(10m, product.OP_QtyInStock);
			AssertEquals(5m, product.OP_WeightedCost);
			AssertEquals("dfg", product.PartBarcodes[0].PH_Barcode);
			AssertEquals("KG", product.PartBarcodes[0].PH_F3_NKPackType);
			AssertEquals("Brand", product.OP_Brand);
			AssertEquals("DEP", product.OP_Department);
			AssertEquals("Division", product.OP_Division);
			AssertEquals(1m, product.OP_VendorPackQty);
			AssertEquals(2m, product.OP_OrderMultipleQty);
			AssertEquals((ZByte)1, product.OP_CountDecimalPlaces);
			AssertEquals("mod", product.OP_Model);
			AssertEquals("PART", product.OP_PartNum);
			AssertEquals("descr", product.OP_Desc);
			AssertEquals("1", product.OP_StockKeepingUnit);

			var whsProduct = WhsProduct.GetWhsProduct(product);
			AssertEquals("One ParamsByWhsAndClient in collection after import", 1, whsProduct.ParamsByWhsAndClient.Count);
			AssertEquals(8.2m, whsProduct.ParamsByWhsAndClient[0].W3_EconomicQuantity);
			AssertEquals(3, whsProduct.ParamsByWhsAndClient[0].W3_ExpiryNotificationPeriod);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMinimum);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMultiple);
			AssertEquals("AA", whsProduct.ParamsByWhsAndClient[0].W3_StockTakeCycle);

			AssertEquals("One PickFace in collection after import", 1, whsProduct.PickFaces.Count);
			AssertNotEquals(ZGuid.Empty, whsProduct.PickFaces[0].WF_OH_Client);
			AssertEquals("location", whsProduct.PickFaces[0].LocationString);
			AssertEquals(5m, whsProduct.PickFaces[0].WF_ReplenishMaximum);
			AssertEquals(1m, whsProduct.PickFaces[0].WF_ReplenishMinimum);
			AssertEquals(2.1m, product.OP_Depth);
			AssertEquals("HY", product.OP_MeasureUQ);
			AssertEquals(208m, product.OP_Weight);
			AssertEquals("LK", product.OP_WeightUQ);
			AssertEquals(32m, product.OP_Height);
			AssertEquals(200m, product.OP_NetWeight);
			AssertEquals(9m, product.OP_Cubic);
			AssertEquals("MM", product.OP_CubicUQ);
			AssertEquals(76m, product.OP_Width);

			AssertEquals("LB", product.RelatedOrganisations[0].OU_ClientUQ);
			AssertEquals((ZShort)4, product.RelatedOrganisations[0].OU_Hi);
			AssertEquals(12m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent1);
			AssertEquals(13m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent2);
			AssertEquals(14m, product.RelatedOrganisations[0].OU_LandedCostMarginPercent3);
			AssertEquals("local descr", product.RelatedOrganisations[0].OU_LocalPartDescription);
			AssertEquals("local PartNumber", product.RelatedOrganisations[0].OU_LocalPartNumber);
			AssertEquals("local PartNumber", product.RelatedOrganisations[0].OU_LocalPartNumber);
			AssertNotEquals(ZGuid.Empty, product.RelatedOrganisations[0].Organisation);
			AssertEquals("RTE", product.RelatedOrganisations[0].OU_Relationship);
			AssertEquals("NON", product.RelatedOrganisations[0].OU_RFAttributeConfirm);
			AssertEquals(34m, product.RelatedOrganisations[0].OU_RoyaltyFlatAmount);
			AssertEquals(36m, product.RelatedOrganisations[0].OU_RoyaltyPercent);
			AssertEquals((ZShort)6, product.RelatedOrganisations[0].OU_Ti);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(false, product.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(false, product.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(true, product.RelatedOrganisations[0].OU_UsePackingDate);
			AssertEquals(17m, product.PartUnits[0].OF_QuantityInParent);
			AssertEquals("KK", product.PartUnits[0].OF_PackType);
			AssertEquals("Kr", product.PartUnits[0].OF_ParentPackType);

			AssertEquals(1, product.BillOfMaterials.Count);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowAutoReplenishKit);

			whsProduct = WhsProduct.GetWhsProduct(product.BillOfMaterials[0].Component);

			AssertEquals("One ParamsByWhsAndClient in collection after import", 1, whsProduct.ParamsByWhsAndClient.Count);
			AssertEquals(8.2m, whsProduct.ParamsByWhsAndClient[0].W3_EconomicQuantity);
			AssertEquals(3, whsProduct.ParamsByWhsAndClient[0].W3_ExpiryNotificationPeriod);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMinimum);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMultiple);
			AssertEquals("AA", whsProduct.ParamsByWhsAndClient[0].W3_StockTakeCycle);

			//no PickFaces, because whs not specified
			AssertEquals("One PickFace in collection after import", 0, whsProduct.PickFaces.Count);
			AssertEquals(true, product.IsImportedFromXML);
		}

		public void TestImportNewProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "New Part";
			product.OP_Desc = "New descr";

			var fullyPopulatedProduct = GetNewPopulatedProduct();
			var whsForFullyPopulatedProduct = WhsProduct.GetWhsProduct(fullyPopulatedProduct);
			var row1 = Helper.CreateRowAndGenerateLocations(WhsForTest, "A", 2, 2, 2);
			whsForFullyPopulatedProduct.PickFaces[0].WF_WL = row1.Locations[0].PK;

			var xsdProduct = MakeExport(fullyPopulatedProduct);
			ProductDataAdapter.ImportFromValueObject(product, xsdProduct, Context);

			var whsProduct = WhsProduct.GetWhsProduct(product);
			AssertEquals("One ParamsByWhsAndClient in collection after import", 1, whsProduct.ParamsByWhsAndClient.Count);
			AssertEquals(8.2m, whsProduct.ParamsByWhsAndClient[0].W3_EconomicQuantity);
			AssertEquals(3, whsProduct.ParamsByWhsAndClient[0].W3_ExpiryNotificationPeriod);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMinimum);
			AssertEquals(2m, whsProduct.ParamsByWhsAndClient[0].W3_ReplenishmentMultiple);
			AssertEquals("AA", whsProduct.ParamsByWhsAndClient[0].W3_StockTakeCycle);

			AssertEquals("One PickFace in collection after import", 1, whsProduct.PickFaces.Count);
			AssertNotEquals(ZGuid.Empty, whsProduct.PickFaces[0].WF_OH_Client);
			AssertEquals("A-1-1-1", whsProduct.PickFaces[0].LocationString);
			AssertEquals(5m, whsProduct.PickFaces[0].WF_ReplenishMaximum);
			AssertEquals(1m, whsProduct.PickFaces[0].WF_ReplenishMinimum);

			WhsForTest.Rows.DeleteAll();  //Keep warehouse but remove all locations

			var anotherNewProduct = Factory.New<OrgSupplierPart>();
			anotherNewProduct.OP_PartNum = "New Part 2";
			anotherNewProduct.OP_Desc = "New descr 2";

			ProductDataAdapter.ImportFromValueObject(anotherNewProduct, xsdProduct, Context);

			whsProduct = WhsProduct.GetWhsProduct(anotherNewProduct);
			AssertEquals("One ParamsByWhsAndClient in collection after import", 1, whsProduct.ParamsByWhsAndClient.Count);
			AssertEquals("0 PickFaces after import", 0, whsProduct.PickFaces.Count);
			Assert(((NotificationBuffer)Context.Notifications).AsString
				.Contains($"Pick Faces from XML file cannot be added, because Pick Face Location not specified or doesn't exist in {Core.Constants.ProductName} or Pick Face not associated with Warehouse or associated Warehouse does not have default Location."));
		}

		#endregion

		#region TestImportNewProduct_ClientWarehouseDetailHasNoMatchingWarehouse

		public void TestImportNewProduct_ClientWarehouseDetailHasNoMatchingWarehouse()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "New Part 1";
			product1.OP_Desc = "New descr 1";

			var xsdProduct = MakeExport(GetNewPopulatedProduct());

			ProductDataAdapter.ImportFromValueObject(product1, xsdProduct, Context);

			var whsProduct1 = WhsProduct.GetWhsProduct(product1);
			AssertEquals("One ParamsByWhsAndClient in collection after import.", 1, whsProduct1.ParamsByWhsAndClient.Count);
			Assert("Import should have no errors.", !((NotificationBuffer)Context.Notifications).HasErrors);

			//remove warehouse
			var warehouse = whsProduct1.ParamsByWhsAndClient[0].Warehouse;
			warehouse.Delete();

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "New Part 2";
			product2.OP_Desc = "New descr 2";

			ProductDataAdapter.ImportFromValueObject(product2, xsdProduct, Context);

			var whsProduct2 = WhsProduct.GetWhsProduct(product2);
			AssertEquals("No ParamsByWhsAndClient in collection after import because warehouse doesn't exist.", 0, whsProduct2.ParamsByWhsAndClient.Count);
			Assert("Import should have error for missing Warehouse.", ((NotificationBuffer)Context.Notifications)
				.AsString.Contains("Client Warehouse Details from XML file cannot be added, because the warehouse specified in client warehouse detail does not exist or was not provided."));
		}

		#endregion

		#region Export

		public void TestExportProduct()
		{
			var product = GetNewPopulatedProduct();
			var xsdProduct = MakeExport(product);

			AssertEquals(20m, xsdProduct.BasicStockControl.LastCost);
			AssertEquals(10m, xsdProduct.BasicStockControl.QtyInStock);
			AssertEquals(5m, xsdProduct.BasicStockControl.WeightedCost);
			AssertEquals("dfg", xsdProduct.Barcodes[0].BarcodeString);
			AssertEquals("KG", xsdProduct.Barcodes[0].PackageUQ);
			AssertEquals("Brand", xsdProduct.BrandName);
			AssertEquals("DEP", xsdProduct.ClientDefinedDetails.Department);
			AssertEquals("Division", xsdProduct.ClientDefinedDetails.Division);
			AssertEquals(1m, xsdProduct.ClientDefinedDetails.VendorPack.Value);
			AssertEquals(2m, xsdProduct.ClientDefinedDetails.OrderMultipleQty.Value);
			AssertEquals("mod", xsdProduct.Model);
			AssertEquals("PART", xsdProduct.ProductCode);
			AssertEquals("descr", xsdProduct.ProductDescription);
			AssertEquals("1", xsdProduct.StockUnit);
			AssertEquals((ZShort)1, xsdProduct.DecimalPlaces);

			AssertNotEquals(null, xsdProduct.ClientWarehouseDetails[0].Client);
			AssertEquals(8.2m, xsdProduct.ClientWarehouseDetails[0].EconomicQty);
			AssertEquals(3, xsdProduct.ClientWarehouseDetails[0].ExpiryPeriod);
			AssertEquals(2m, xsdProduct.ClientWarehouseDetails[0].ReplenishMinimum);
			AssertEquals(2m, xsdProduct.ClientWarehouseDetails[0].ReplenishMultiple);
			AssertEquals("AA", xsdProduct.ClientWarehouseDetails[0].StockTakeCycle);

			AssertNotEquals(null, xsdProduct.PickfaceDetails[0].Client);
			AssertEquals("location", xsdProduct.PickfaceDetails[0].Location);
			AssertEquals(5, xsdProduct.PickfaceDetails[0].ReplenishMaximum);
			AssertEquals(1, xsdProduct.PickfaceDetails[0].ReplenishMinimum);

			AssertEquals(2.1m, xsdProduct.DimensionDetails.Depth);
			AssertEquals("HY", xsdProduct.DimensionDetails.DimensionUnit);
			AssertEquals(208m, xsdProduct.DimensionDetails.GrossWeight.Value);
			AssertEquals("LK", xsdProduct.DimensionDetails.GrossWeight.DimensionType);
			AssertEquals(32m, xsdProduct.DimensionDetails.Height);
			AssertEquals(200m, xsdProduct.DimensionDetails.NetWeight);
			AssertEquals(9m, xsdProduct.DimensionDetails.Volume.Value);
			AssertEquals("MM", xsdProduct.DimensionDetails.Volume.DimensionType);
			AssertEquals(76m, xsdProduct.DimensionDetails.Width);

			AssertEquals("LB", xsdProduct.RelatedOrganisations[0].ClientUQ);
			AssertEquals(4, xsdProduct.RelatedOrganisations[0].Hi);
			AssertEquals(12m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage1);
			AssertEquals(13m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage2);
			AssertEquals(14m, xsdProduct.RelatedOrganisations[0].LCMarkUpPercentage3);
			AssertEquals("local descr", xsdProduct.RelatedOrganisations[0].LocalProductDescription);
			AssertEquals("local PartNumber", xsdProduct.RelatedOrganisations[0].LocalProductNumber);
			AssertNotEquals(null, xsdProduct.RelatedOrganisations[0].Organisation);
			AssertEquals("RTE", xsdProduct.RelatedOrganisations[0].RelationshipType);
			AssertEquals("NON", RFAttrConfirmToXmlCodeMappings.Instance.GetEnterpriseCode(xsdProduct.RelatedOrganisations[0].RFAttributeConfirm.ToString(), "", Context));
			AssertEquals(34m, xsdProduct.RelatedOrganisations[0].RoyaltyFlatAmount.Value);
			AssertEquals(36m, xsdProduct.RelatedOrganisations[0].RoyaltyPercentage);
			AssertEquals(6, xsdProduct.RelatedOrganisations[0].Ti);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UseAttribute1);
			AssertEquals(false, xsdProduct.RelatedOrganisations[0].UseAttribute2);
			AssertEquals(false, xsdProduct.RelatedOrganisations[0].UseAttribute3);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UseExpiryDate);
			AssertEquals(true, xsdProduct.RelatedOrganisations[0].UsePackingDate);

			AssertEquals(17m, xsdProduct.UnitConversions[0].Package.Value);
			AssertEquals("KK", xsdProduct.UnitConversions[0].Package.DimensionType);
			AssertEquals("Kr", xsdProduct.UnitConversions[0].ParentUQ);

			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowResale);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowDisassemblyOfKit);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].AllowAutoReplenishKit);
			AssertEquals(false, xsdProduct.BillOfMaterials[0].AutoPrintAssemblyInstruction);

			AssertEquals(20m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.LastCost);
			AssertEquals(10m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.QtyInStock);
			AssertEquals(5m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BasicStockControl.WeightedCost);
			AssertEquals("dfg", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Barcodes[0].BarcodeString);
			AssertEquals("KG", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Barcodes[0].PackageUQ);
			AssertEquals("Brand", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.BrandName);
			AssertEquals("DEP", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.Department);
			AssertEquals("Division", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.Division);
			AssertEquals(1m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.VendorPack.Value);
			AssertEquals(2m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientDefinedDetails.OrderMultipleQty.Value);
			AssertEquals("mod", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.Model);
			AssertEquals("BOM PRODUCT", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ProductCode);
			AssertEquals("BOM Part descr", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ProductDescription);
			AssertEquals("1", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.StockUnit);
			AssertEquals((ZShort)1, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DecimalPlaces);

			AssertNotEquals(null, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].Client);
			AssertEquals(8.2m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].EconomicQty);
			AssertEquals(3, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].ExpiryPeriod);
			AssertEquals(2m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].ReplenishMinimum);
			AssertEquals(2m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].ReplenishMultiple);
			AssertEquals(true, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].ReplenishMultipleSpecified);
			AssertEquals("AA", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.ClientWarehouseDetails[0].StockTakeCycle);

			AssertNotEquals(null, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.PickfaceDetails[0].Client);
			AssertEquals("BOM location", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.PickfaceDetails[0].Location);
			AssertEquals(5, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.PickfaceDetails[0].ReplenishMaximum);
			AssertEquals(1, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.PickfaceDetails[0].ReplenishMinimum);

			AssertEquals(2.1m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Depth);
			AssertEquals("HY", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.DimensionUnit);
			AssertEquals(208m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.GrossWeight.Value);
			AssertEquals("LK", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.GrossWeight.DimensionType);
			AssertEquals(32m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Height);
			AssertEquals(200m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.NetWeight);
			AssertEquals(9m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Volume.Value);
			AssertEquals("MM", xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Volume.DimensionType);
			AssertEquals(76m, xsdProduct.BillOfMaterials[0].Components[0].ComponentPart.DimensionDetails.Width);
		}

		#endregion

		#region Setup

		Xsd.Product MakeExport(OrgSupplierPart product)
		{
			var xsdProduct = new Xsd.Product();

			ProductDataAdapter.ExportToValueObject(product, xsdProduct, new ValueObjectExportContext(new NotificationBuffer()));

			return xsdProduct;
		}

		public OrgSupplierPart GetNewPopulatedProduct()
		{
			var product = Factory.New<OrgSupplierPart>();

			product.OP_LastCost = 20;
			product.OP_QtyInStock = 10;
			product.OP_WeightedCost = 5;

			var bar = Factory.New<OrgSupplierPartBarcode>();
			bar.PH_Barcode = "dfg";
			bar.PH_F3_NKPackType = "KG";
			product.PartBarcodes.Add(bar);

			product.OP_Brand = "Brand";
			product.OP_Department = "DEP";
			product.OP_Division = "Division";
			product.OP_VendorPackQty = 1;
			product.OP_OrderMultipleQty = 2;
			product.OP_CountDecimalPlaces = 1;
			product.OP_Model = "mod";
			product.OP_PartNum = "Part";
			product.OP_Desc = "descr";
			product.OP_StockKeepingUnit = "1";

			WhsForTest = Helper.CreateWarehouse("2", "A", 2, 1);

			var whsProduct = WhsProduct.GetWhsProduct(product);
			var paramsByWhsAndClient = whsProduct.ParamsByWhsAndClient.AddNew();
			paramsByWhsAndClient.W3_OH = Organization.PK;
			paramsByWhsAndClient.W3_WW = WhsForTest.PK;
			paramsByWhsAndClient.W3_EconomicQuantity = 8.2;
			paramsByWhsAndClient.W3_ExpiryNotificationPeriod = 3;
			paramsByWhsAndClient.W3_ReplenishmentMinimum = 2;
			paramsByWhsAndClient.W3_ReplenishmentMultiple = 2;
			paramsByWhsAndClient.W3_StockTakeCycle = "AA";

			var pickFace = whsProduct.PickFaces.AddNew();
			pickFace.WF_OH_Client = Organization.PK;
			pickFace.WF_ReplenishMaximum = 5;
			pickFace.WF_ReplenishMinimum = 1;
			pickFace.LocationWhsGuid = WhsForTest.PK;
			pickFace.LocationString = "location";
			product.OP_Depth = 2.1;
			product.OP_MeasureUQ = "HY";
			product.OP_Weight = 208;
			product.OP_WeightUQ = "LK";
			product.OP_Height = 32;
			product.OP_NetWeight = 200;
			product.OP_Cubic = 9;
			product.OP_CubicUQ = "MM";
			product.OP_Width = 76;

			#region BOM
			product.OP_CanResell = true;
			product.OP_CanDisassembleKit = true;
			product.OP_KitIsAutoReplenished = true;
			product.OP_AutoPrintAssemblyInstructions = false;
			var bOM = product.BillOfMaterials.AddNew();
			var bOMproduct = Factory.New<OrgSupplierPart>();
			bOMproduct.RelatedOrganisations.AddOwner(Organization);
			bOM.OE_OP_Component = bOMproduct.PK;

			bOMproduct.OP_LastCost = 20;
			bOMproduct.OP_QtyInStock = 10;
			bOMproduct.OP_WeightedCost = 5;

			var bOMbar = Factory.New<OrgSupplierPartBarcode>();
			bOMbar.PH_Barcode = "dfg";
			bOMbar.PH_F3_NKPackType = "KG";
			bOMproduct.PartBarcodes.Add(bOMbar);

			bOMproduct.OP_Brand = "Brand";
			bOMproduct.OP_Department = "DEP";
			bOMproduct.OP_Division = "Division";
			bOMproduct.OP_VendorPackQty = 1;
			bOMproduct.OP_OrderMultipleQty = 2;
			bOMproduct.OP_CountDecimalPlaces = 1;
			bOMproduct.OP_Model = "mod";
			bOMproduct.OP_PartNum = "BOM Product";
			bOMproduct.OP_Desc = "BOM Part descr";
			bOMproduct.OP_StockKeepingUnit = "1";

			var bOMwhsProduct = WhsProduct.GetWhsProduct(bOMproduct);
			var bOMparamsByWhsAndClient = bOMwhsProduct.ParamsByWhsAndClient.AddNew();
			bOMparamsByWhsAndClient.W3_OH = Organization.PK;
			bOMparamsByWhsAndClient.W3_WW = WhsForTest.PK;
			bOMparamsByWhsAndClient.W3_EconomicQuantity = 8.2;
			bOMparamsByWhsAndClient.W3_ExpiryNotificationPeriod = 3;
			bOMparamsByWhsAndClient.W3_ReplenishmentMinimum = 2;
			bOMparamsByWhsAndClient.W3_ReplenishmentMultiple = 2;
			bOMparamsByWhsAndClient.W3_StockTakeCycle = "AA";

			var bOMpickFace = bOMwhsProduct.PickFaces.AddNew();
			bOMpickFace.WF_OH_Client = Organization.PK;
			bOMpickFace.LocationString = "BOM location";
			bOMpickFace.WF_ReplenishMaximum = 5;
			bOMpickFace.WF_ReplenishMinimum = 1;
			bOMproduct.OP_Depth = 2.1;
			bOMproduct.OP_MeasureUQ = "HY";
			bOMproduct.OP_Weight = 208;
			bOMproduct.OP_WeightUQ = "LK";
			bOMproduct.OP_Height = 32;
			bOMproduct.OP_NetWeight = 200;
			bOMproduct.OP_Cubic = 9;
			bOMproduct.OP_CubicUQ = "MM";
			bOMproduct.OP_Width = 76;
			#endregion

			var org = product.RelatedOrganisations.AddNew();
			org.OU_ClientUQ = "LB";
			org.OU_Hi = 4;
			org.OU_LandedCostMarginPercent1 = 12;
			org.OU_LandedCostMarginPercent2 = 13;
			org.OU_LandedCostMarginPercent3 = 14;
			org.OU_LocalPartDescription = "local descr";
			org.OU_LocalPartNumber = "local PartNumber";
			org.OU_OH = Organization.PK;
			org.OU_Relationship = "RTE";
			org.OU_RFAttributeConfirm = "NON";
			org.OU_RoyaltyFlatAmount = 34;
			org.OU_RX_NKRoyaltyCurrency = "USD";
			org.OU_RoyaltyPercent = 36;
			org.OU_Ti = 6;
			org.OU_UsePartAttrib1 = true;
			org.OU_UsePartAttrib2 = false;
			org.OU_UsePartAttrib3 = false;
			org.OU_UseSerialNumber = true;
			org.OU_UseExpiryDate = true;
			org.OU_UsePackingDate = true;

			var unit = product.PartUnits.AddNew();
			unit.OF_QuantityInParent = 17;
			unit.OF_PackType = "KK";
			unit.OF_ParentPackType = "Kr";

			return product;
		}
		WhsWarehouse WhsForTest;

		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					organization.FillWithValidTestData();
					organization.OH_FullName = "Test Org";

					var address1 = organization.Addresses.AddNew();
					address1.OA_Address1 = "Test 1";
					var address2 = organization.Addresses.AddNew();
					address2.OA_Address1 = "Test 2";
				}
				return organization;
			}
		}
		OrgHeader organization;

		ProductValueObjectDataAdapter ProductDataAdapter => productDataAdapter ?? (productDataAdapter = new ProductValueObjectDataAdapter());
		ProductValueObjectDataAdapter productDataAdapter;

		ValueObjectImportContext Context => context ?? (context = new ValueObjectImportContext(Factory, Notification));
		ValueObjectImportContext context;

		NotificationBuffer Notification => notify ?? (notify = new NotificationBuffer());
		NotificationBuffer notify;

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
