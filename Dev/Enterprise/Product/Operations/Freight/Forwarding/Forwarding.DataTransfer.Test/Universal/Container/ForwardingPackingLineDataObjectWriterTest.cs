using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ForwardingPackingLineDataObjectWriterTest : ShipmentDataObjectWriterTest
	{
		#region Products On Pack Line

		public void TestShipmentExportsPackLineProducts()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipmentBO.OuterPackLines.AddNew();
			var packLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);

			var packLine1Product1 = packLine1.Products.AddNew();
			packLine1Product1.D2_ProductCode = "MACARONS - SALTED CARAMEL";
			packLine1Product1.D2_ProductQuantity = 500m;
			packLine1Product1.D2_ProductUnitOfQty = "PLT";

			var packLine2Product1 = packLine2.Products.AddNew();
			packLine2Product1.D2_ProductCode = "MACARONS - GREEN TEA";
			packLine2Product1.D2_ProductQuantity = 100m;
			packLine2Product1.D2_ProductUnitOfQty = "BOX";

			var packLine2Product2 = packLine2.Products.AddNew();
			packLine2Product2.D2_ProductCode = "MACARONS - STRAWBERRY AND CREAM";
			packLine2Product2.D2_ProductQuantity = 200m;
			packLine2Product2.D2_ProductUnitOfQty = "BOX";

			var packLine2Product3 = packLine2.Products.AddNew();
			packLine2Product3.D2_ProductCode = "MACARONS - LEMON";
			packLine2Product3.D2_ProductQuantity = 300m;
			packLine2Product3.D2_ProductUnitOfQty = "BOX";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);
			AssertEquals("Expected an item in PackedItemCollection", 1, shipmentData.PackingLineCollection[0].PackedItemCollection.Count);

			var product = shipmentData.PackingLineCollection[0].PackedItemCollection[0];
			AssertEquals("Expected to match packline 1 product 1", 500m, product.PackedQuantity);
			AssertEquals("Expected to match packline 1 product 1", "PLT", product.UnitOfQuantity.Code);
			AssertEquals("Expected to match packline 1 product 1", "Pallet", product.UnitOfQuantity.Description);
			AssertEquals("Expected to match packline 1 product 1", "MACARONS - SALTED CARAMEL", product.Product.Code);

			AssertEquals("Expected 3 items in PackedItemCollection", 3, shipmentData.PackingLineCollection[1].PackedItemCollection.Count);

			product = shipmentData.PackingLineCollection[1].PackedItemCollection[0];
			AssertEquals("Expected to match packline 2 product 1", 100m, product.PackedQuantity);
			AssertEquals("Expected to match packline 2 product 1", "BOX", product.UnitOfQuantity.Code);
			AssertEquals("Expected to match packline 2 product 1", "Box", product.UnitOfQuantity.Description);
			AssertEquals("Expected to match packline 2 product 1", "MACARONS - GREEN TEA", product.Product.Code);

			product = shipmentData.PackingLineCollection[1].PackedItemCollection[1];
			AssertEquals("Expected to match packline 2 product 2", 200m, product.PackedQuantity);
			AssertEquals("Expected to match packline 2 product 2", "BOX", product.UnitOfQuantity.Code);
			AssertEquals("Expected to match packline 2 product 2", "Box", product.UnitOfQuantity.Description);
			AssertEquals("Expected to match packline 2 product 2", "MACARONS - STRAWBERRY AND CREAM", product.Product.Code);

			product = shipmentData.PackingLineCollection[1].PackedItemCollection[2];
			AssertEquals("Expected to match packline 2 product 3", 300m, product.PackedQuantity);
			AssertEquals("Expected to match packline 2 product 3", "BOX", product.UnitOfQuantity.Code);
			AssertEquals("Expected to match packline 2 product 3", "Box", product.UnitOfQuantity.Description);
			AssertEquals("Expected to match packline 2 product 3", "MACARONS - LEMON", product.Product.Code);
		}

		public void TestShipmentExportsPackLineProducts_NonStandardQuantities()
		{
			var shipmentBusinessObject = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipmentBusinessObject.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine);

			var productBusinessObject = packLine.Products.AddNew();
			productBusinessObject.D2_ProductCode = "ROSE WATER NOUGAT";
			productBusinessObject.D2_ProductQuantity = 500m;
			productBusinessObject.D2_ProductUnitOfQty = "MMM";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBusinessObject)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBusinessObject);

			AssertNotNull("Pre-condition", shipmentData.PackingLineCollection);
			AssertEquals("Pre-condition", 1, shipmentData.PackingLineCollection.Count);

			var createdProduct = shipmentData.PackingLineCollection[0].PackedItemCollection[0];
			AssertEquals(500m, createdProduct.PackedQuantity);
			AssertEquals("ROSE WATER NOUGAT", createdProduct.Product.Code);
			AssertEquals("MMM", createdProduct.UnitOfQuantity.Code);
			AssertNull("Expected no description as MMM is not a standard unit", createdProduct.UnitOfQuantity.Description);
		}

		public void TestShipmentExportsPackLineProducts_CreatedWithMissingValues()
		{
			var shipmentBusinessObject = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipmentBusinessObject.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine);

			var productBusinessObject = packLine.Products.AddNew();
			productBusinessObject.D2_ProductCode = "RED VELVET CUPCAKE";

			productBusinessObject = packLine.Products.AddNew();
			productBusinessObject.D2_ProductCode = "";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBusinessObject)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBusinessObject);

			AssertNotNull("Pre-condition", shipmentData.PackingLineCollection);
			AssertEquals("Pre-condition", 2, shipmentData.PackingLineCollection[0].PackedItemCollection.Count);

			var createdProduct = shipmentData.PackingLineCollection[0].PackedItemCollection[0];
			AssertNotNull("Expected to have created a product data object even though some values are empty", createdProduct);
			AssertEquals("RED VELVET CUPCAKE", createdProduct.Product.Code);
			AssertEquals(0m, createdProduct.PackedQuantity);
			AssertEquals("", createdProduct.UnitOfQuantity.Code);
			AssertNull(createdProduct.UnitOfQuantity.Description);

			createdProduct = shipmentData.PackingLineCollection[0].PackedItemCollection[1];
			AssertNotNull("Expected to have created a product data object even though values are all empty", createdProduct);
			AssertNull("", createdProduct.Product);
			AssertEquals(0m, createdProduct.PackedQuantity);
			AssertEquals("", createdProduct.UnitOfQuantity.Code);
			AssertNull(createdProduct.UnitOfQuantity.Description);
		}

		public void TestShipmentPackagesDoNotGetOverriddenByDeclaratinPackage()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();

				var declarationBO1 = SetUpDeclarationData(shipmentBO);
				var packLine1 = shipmentBO.OuterPackLines.AddNew();
				var packLine2 = shipmentBO.OuterPackLines.AddNew();
				PackingLineDataObjectWriterTest.PopulatePackLine(packLine1);
				PackingLineDataObjectWriterTest.PopulatePackLine(packLine2);

				var packLine1Product1 = packLine1.Products.AddNew();
				packLine1Product1.D2_ProductCode = "MACARONS - SALTED CARAMEL";
				packLine1Product1.D2_ProductQuantity = 500m;
				packLine1Product1.D2_ProductUnitOfQty = "PLT";

				var packLine2Product1 = packLine2.Products.AddNew();
				packLine2Product1.D2_ProductCode = "MACARONS - GREEN TEA";
				packLine2Product1.D2_ProductQuantity = 100m;
				packLine2Product1.D2_ProductUnitOfQty = "BOX";

				var packLine2Product2 = packLine2.Products.AddNew();
				packLine2Product2.D2_ProductCode = "MACARONS - STRAWBERRY AND CREAM";
				packLine2Product2.D2_ProductQuantity = 200m;
				packLine2Product2.D2_ProductUnitOfQty = "BOX";

				var packLine2Product3 = packLine2.Products.AddNew();
				packLine2Product3.D2_ProductCode = "MACARONS - LEMON";
				packLine2Product3.D2_ProductQuantity = 300m;
				packLine2Product3.D2_ProductUnitOfQty = "BOX";

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);

				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("PackingLineCollection should come from Shipment", 2, shipmentData.PackingLineCollection.Count);
			}
		}

		public void TestShipment_CusEntryNumberCollection()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(packLine);

			var panNumber = Factory.New<CusEntryNumber>();
			panNumber.Parent = packLine;
			panNumber.CE_EntryType = "PAN";
			panNumber.CE_EntryNum = "PAN0001";
			panNumber.CE_EntryIsSystemGenerated = true;
			panNumber.CE_RN_NKCountryCode = "AU";
			panNumber.CE_Category = "PRT";

			var ercNumber = Factory.New<CusEntryNumber>();
			ercNumber.Parent = packLine;
			ercNumber.CE_EntryType = "ERC";
			ercNumber.CE_EntryNum = "ERC0001";
			ercNumber.CE_EntryIsSystemGenerated = true;
			ercNumber.CE_RN_NKCountryCode = "CN";
			ercNumber.CE_Category = "OTH";

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals("Expected 1 pack lines found", 1, shipmentData.PackingLineCollection.Count);
			var portReferences = shipmentData.PackingLineCollection[0].PortReferenceCollection;
			var referenceNumbers = shipmentData.PackingLineCollection[0].ReferenceNumberCollection;

			AssertNotNull(portReferences);
			AssertNotNull(referenceNumbers);
			AssertEquals("Expected 1 port references found", 1, portReferences.Count);
			AssertEquals("Expected 1 references found", 1, referenceNumbers.Count);

			AssertEquals("PAN", portReferences[0].Type.Code);
			AssertEquals("PAN0001", portReferences[0].Reference);
			AssertEquals("AU", portReferences[0].Country.Code);
			AssertEquals("", portReferences[0].Status.Code);

			AssertEquals("ERC", referenceNumbers[0].Type.Code);
			AssertEquals("ERC0001", referenceNumbers[0].ReferenceNumber);
		}

		BusinessObject SetUpDeclarationData(ForwardingShipment shipment)
		{
			var declarationBO = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declarationBO.FillWithValidTestData();
			declarationBO[JobDeclarationSchema.JE_OverrideFreightDefaults] = ZBool.True;
			declarationBO[JobDeclarationSchema.JE_JS] = shipment.PK;
			declarationBO[JobDeclarationSchema.JE_MessageType] = "IMP";
			declarationBO[JobDeclarationSchema.JE_MasterBill] = "MB32423";
			declarationBO[JobDeclarationSchema.JE_HouseBill] = "HB78785";
			declarationBO[JobDeclarationSchema.JE_RL_NKOrigin] = "USLAX";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "USCHI";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "NZCHC";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = "NZAKL";
			declarationBO[JobDeclarationSchema.JE_RL_NKFinalDestination] = "AUBNE";
			declarationBO[JobDeclarationSchema.JE_GoodsDescription] = "DECLARATION GOODS";
			declarationBO[JobDeclarationSchema.JE_MergeBy] = "TRF";
			declarationBO[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2011, 6, 10);
			declarationBO[JobDeclarationSchema.JE_DateOfFirstArrival] = new ZDateTime(2011, 6, 11);
			declarationBO[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2011, 6, 12);
			declarationBO[JobDeclarationSchema.JE_DateAtFinalDestination] = new ZDateTime(2011, 7, 13);
			declarationBO[JobDeclarationSchema.JE_EntrySubmittedDate] = new ZDateTime(2011, 7, 14);
			declarationBO[JobDeclarationSchema.JE_EntryAuthorisationDate] = new ZDateTime(2011, 7, 15);
			declarationBO[JobDeclarationSchema.JE_WarehouseReleaseDate] = new ZDateTime(2011, 7, 16);
			declarationBO[JobDeclarationSchema.JE_DateAtOrigin] = new ZDateTime(2011, 7, 17);

			var masterBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationBO.PK);
			masterBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, "MB32423");
			var masterBillBO = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBill>(masterBillQuery);
			if (masterBillBO == null)
			{
				masterBillBO = Factory.BOFactory.New<Enterprise.Integration.Customs.IBill>();
				masterBillBO.CU_BillNum = "MB32423";
				masterBillBO.CU_BillType = "MB";
				masterBillBO.CU_JE = declarationBO.PK;
				masterBillBO.CU_GUIPresentationRecord = ZBool.True;
			}
			masterBillBO.CU_IssueDate = new ZDateTime(2011, 5, 20);

			var houseBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationBO.PK);
			houseBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, "HB78785");
			var houseBillBO = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBill>(houseBillQuery);
			if (houseBillBO == null)
			{
				houseBillBO = Factory.BOFactory.New<Enterprise.Integration.Customs.IBill>();
				houseBillBO.CU_BillNum = "HB78785";
				houseBillBO.CU_BillType = "HB";
				houseBillBO.CU_JE = declarationBO.PK;
				houseBillBO.CU_GUIPresentationRecord = ZBool.True;
				houseBillBO.CU_CU_ParentBill = masterBillBO.PK;
			}
			houseBillBO.CU_IssueDate = new ZDateTime(2011, 5, 21);

			var packingGroup = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBasePackingGroup>(new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, houseBillBO.PK));
			if (packingGroup == null)
			{
				packingGroup = Factory.BOFactory.New<Enterprise.Integration.Customs.IBasePackingGroup>();
				packingGroup.CR_CU_HouseBill = houseBillBO.PK;
			}

			var package = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBasePackage>(new ZQuery(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, packingGroup.PK));
			if (package == null)
			{
				package = Factory.BOFactory.New<Enterprise.Integration.Customs.IBasePackage>();
				package.CW_CR_HouseContainer = packingGroup.PK;
			}
			package.CW_MarksAndNos = "DEC MARKS NO";
			return declarationBO;
		}

		#endregion

		#region Inner Pack Lines

		void CheckInnerPackLine(PackingLine packingLine, ZLong packageCount, ZString packType, ZDecimal length, ZDecimal width, ZDecimal height,
			ZDecimal volume, ZString volumeUQ, ZDecimal weight, ZString weightUQ, ZString description, ZString refNumber)
		{
			AssertEquals(packageCount, packingLine.PackQty);
			AssertEquals(packType, packingLine.PackType.Code);
			AssertEquals(length, packingLine.Length);
			AssertEquals(width, packingLine.Width);
			AssertEquals(height, packingLine.Height);
			AssertEquals(volume, packingLine.Volume);
			AssertEquals(volumeUQ, packingLine.VolumeUnit.Code);
			AssertEquals(weight, packingLine.Weight);
			AssertEquals(weightUQ, packingLine.WeightUnit.Code);
			AssertEquals(description, packingLine.GoodsDescription);
			AssertEquals(refNumber, packingLine.ReferenceNumber);
		}

		ForwardingShipment PrepareTestDataForInnerPackLines()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var outerPackLine1 = shipmentBO.OuterPackLines.AddNew();
			var outerPackLine2 = shipmentBO.OuterPackLines.AddNew();
			PackingLineDataObjectWriterTest.PopulatePackLine(outerPackLine1);
			PackingLineDataObjectWriterTest.PopulatePackLine(outerPackLine2);
			outerPackLine1.JL_PackLineId = "PL001";
			outerPackLine2.JL_PackLineId = "PL002";

			var innerPackLine1 = shipmentBO.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 3;
			innerPackLine1.JL_F3_NKPackType = "PLT";
			innerPackLine1.JL_Length = 3.2;
			innerPackLine1.JL_UnitOfDimension = "M";
			innerPackLine1.JL_Width = 3.3;
			innerPackLine1.JL_Height = 3.4;
			innerPackLine1.JL_ActualWeight = 3.5;
			innerPackLine1.JL_ActualWeightUQ = "KG";
			innerPackLine1.JL_ActualVolumeUQ = "M3";
			innerPackLine1.JL_ActualVolume = 3.6;
			innerPackLine1.JL_Description = "test desc 3.1";
			innerPackLine1.JL_RefNumber = "RN003";

			var innerPackLine2 = shipmentBO.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 4;
			innerPackLine2.JL_F3_NKPackType = "PLT";
			innerPackLine2.JL_Length = 4.2;
			innerPackLine2.JL_UnitOfDimension = "M";
			innerPackLine2.JL_Width = 4.3;
			innerPackLine2.JL_Height = 4.4;
			innerPackLine2.JL_ActualWeight = 4.5;
			innerPackLine2.JL_ActualWeightUQ = "LB";
			innerPackLine2.JL_ActualVolumeUQ = "M3";
			innerPackLine2.JL_ActualVolume = 4.6;
			innerPackLine2.JL_Description = "test desc 4.1";
			innerPackLine2.JL_RefNumber = "RN004";

			var innerPackLine3 = shipmentBO.InnerPackLines.AddNew();
			innerPackLine3.JL_PackageCount = 5;
			innerPackLine3.JL_F3_NKPackType = "PLT";
			innerPackLine3.JL_Length = 5.2;
			innerPackLine3.JL_UnitOfDimension = "M";
			innerPackLine3.JL_Width = 5.3;
			innerPackLine3.JL_Height = 5.4;
			innerPackLine3.JL_ActualWeight = 5.5;
			innerPackLine3.JL_ActualWeightUQ = "KG";
			innerPackLine3.JL_ActualVolumeUQ = "L";
			innerPackLine3.JL_ActualVolume = 5.6;
			innerPackLine3.JL_Description = "test desc 5.1";
			innerPackLine3.JL_RefNumber = "RN005";
			return shipmentBO;
		}

		public void TestInnerPackLineCompletelyLinkedToOuterPackLines()
		{
			var shipmentBO = PrepareTestDataForInnerPackLines();
			shipmentBO.InnerPackLines[0].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[0].PK;
			shipmentBO.InnerPackLines[1].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[1].PK;
			shipmentBO.InnerPackLines[2].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[1].PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);

			var packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
			var packLine2Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL002");
			AssertEquals("Expected nested 1 inner pack lines found", 1, packLine1Data.PackingLineCollection.Count);
			CheckInnerPackLine(packLine1Data.PackingLineCollection[0], 3, "PLT", 3.2, 3.3, 3.4, 3.6, "M3", 3.5, "KG", "test desc 3.1", "RN003");
			AssertEquals("Expected nested 2 inner pack lines found", 2, packLine2Data.PackingLineCollection.Count);
			CheckInnerPackLine(packLine2Data.PackingLineCollection.First(line => line.ReferenceNumber.Value == "RN004"), 4, "PLT", 4.2, 4.3, 4.4, 4.6, "M3", 4.5, "LB", "test desc 4.1", "RN004");
			CheckInnerPackLine(packLine2Data.PackingLineCollection.First(line => line.ReferenceNumber.Value == "RN005"), 5, "PLT", 5.2, 5.3, 5.4, 5.6, "L", 5.5, "KG", "test desc 5.1", "RN005");
		}

		public void TestInnerPackLinePartiallyLinkedToOuterPackLines()
		{
			var shipmentBO = PrepareTestDataForInnerPackLines();
			shipmentBO.InnerPackLines[0].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[0].PK;
			shipmentBO.InnerPackLines[1].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[1].PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);

			var packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
			var packLine2Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL002");
			AssertEquals("Expected nested 1 inner pack lines found", 1, packLine1Data.PackingLineCollection.Count);
			CheckInnerPackLine(packLine1Data.PackingLineCollection[0], 3, "PLT", 3.2, 3.3, 3.4, 3.6, "M3", 3.5, "KG", "test desc 3.1", "RN003");
			AssertEquals("Expected nested 2 inner pack lines found", 1, packLine2Data.PackingLineCollection.Count);
			CheckInnerPackLine(packLine2Data.PackingLineCollection.First(line => line.ReferenceNumber.Value == "RN004"), 4, "PLT", 4.2, 4.3, 4.4, 4.6, "M3", 4.5, "LB", "test desc 4.1", "RN004");
		}

		public void TestInnerPackLineNotLinkedToOuterPackLines()
		{
			var shipmentBO = PrepareTestDataForInnerPackLines();
			shipmentBO.InnerPackLines[0].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[0].PK;
			shipmentBO.InnerPackLines[1].JL_JL_OuterPackLine = shipmentBO.OuterPackLines[1].PK;

			var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
			var shipmentData = writer.GetDataObject(shipmentBO);

			AssertNotNull(shipmentData.PackingLineCollection);
			AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);

			var packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
			var packLine2Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL002");
			AssertEquals("Expected nested 0 inner pack lines found", 1, packLine1Data.PackingLineCollection.Count);
			AssertEquals("Expected nested 0 inner pack lines found", 1, packLine2Data.PackingLineCollection.Count);
		}

		#endregion

		#region Related Entity Collection

		public void TestPackingLineRelatedEntityCollection_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
				var outerPackLine1 = shipmentBO.OuterPackLines.AddNew();
				var outerPackLine2 = shipmentBO.OuterPackLines.AddNew();
				outerPackLine1.JL_PackLineId = "PL001";
				outerPackLine2.JL_PackLineId = "PL002";

				var (order1, orderLine1, booking1, bookingLine1, loadListHeader1, loadListLine1, _, container1) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory);
				loadListLine1.CLL_JL_PackLine = outerPackLine1.PK;
				outerPackLine1.JL_JC = loadListLine1.CLL_JC_Container;

				var (order2, orderLine2, booking2, bookingLine2, loadListHeader2, loadListLine2, _, container2) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory, "Order 2");
				loadListLine2.CLL_JL_PackLine = outerPackLine2.PK;
				outerPackLine2.JL_JC = loadListLine2.CLL_JC_Container;

				Factory.SaveForTesting();

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);

				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);

				var packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
				var packLine2Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL002");
				AssertEquals(1, packLine1Data.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order1.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine1.GetUniversalDataContextManager().DataContextKey);
				AssertEquals(1, packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking1.JSB_BookingId, bookingLineKey: bookingLine1.JSL_BookingLineId);
				AssertEquals(1, packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader1.CLH_LoadListId, containerNumber: container1.JC_ContainerNum);

				AssertEquals(1, packLine2Data.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine2Data.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order2.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine2.GetUniversalDataContextManager().DataContextKey);
				AssertEquals(1, packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine2Data.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking2.JSB_BookingId, bookingLineKey: bookingLine2.JSL_BookingLineId);
				AssertEquals(1, packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine2Data.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, containerLoadListKey: loadListHeader2.CLH_LoadListId, containerNumber: container2.JC_ContainerNum);

				loadListHeader1.CLH_Status = Constants.ContainerLoadListHeaderStatus.Cancelled;
				Factory.SaveForTesting();
				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);
				packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
				AssertEquals(1, packLine1Data.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order1.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine1.GetUniversalDataContextManager().DataContextKey);
				AssertEquals(1, packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection[0].EntityKeyCollection, count: 2, bookingKey: booking1.JSB_BookingId, bookingLineKey: bookingLine1.JSL_BookingLineId);
				AssertNull(packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection[0].RelatedEntityCollection);

				booking1.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
				Factory.SaveForTesting();
				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);
				packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
				AssertEquals(1, packLine1Data.RelatedEntityCollection.Count);
				OrderManagerTestHelper.CheckKeyValuePairCollectionForUXML(packLine1Data.RelatedEntityCollection[0].EntityKeyCollection, count: 2, orderKey: order1.GetUniversalDataContextManager().DataContextKey, orderLineKey: orderLine1.GetUniversalDataContextManager().DataContextKey);
				AssertNull(packLine1Data.RelatedEntityCollection[0].RelatedEntityCollection);

				order1.JD_OrderStatus = Constants.OrderStatus.Cancelled;
				Factory.SaveForTesting();
				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				shipmentData = writer.GetDataObject(shipmentBO);
				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);
				packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
				AssertNull(packLine1Data.RelatedEntityCollection);
			});
		}

		public void TestPackingLineRelatedEntityCollection_Not_EnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
				var outerPackLine1 = shipmentBO.OuterPackLines.AddNew();
				var outerPackLine2 = shipmentBO.OuterPackLines.AddNew();
				outerPackLine1.JL_PackLineId = "PL001";
				outerPackLine2.JL_PackLineId = "PL002";

				var (_, _, _, _, _, loadListLine1, _, _) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory);
				loadListLine1.CLL_JL_PackLine = outerPackLine1.PK;
				outerPackLine1.JL_JC = loadListLine1.CLL_JC_Container;

				var (_, _, _, _, _, loadListLine2, _, _) = OrderManagerTestHelper.CreateBasicDataForUniversalObjectTest(Factory.BOFactory, "Order 2");
				loadListLine2.CLL_JL_PackLine = outerPackLine2.PK;
				outerPackLine2.JL_JC = loadListLine2.CLL_JC_Container;

				Factory.SaveForTesting();

				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, false);
				var shipmentData = writer.GetDataObject(shipmentBO);

				AssertNotNull(shipmentData.PackingLineCollection);
				AssertEquals("Expected 2 pack lines found", 2, shipmentData.PackingLineCollection.Count);

				var packLine1Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL001");
				var packLine2Data = shipmentData.PackingLineCollection.First(line => line.PackingLineID.Value == "PL002");
				AssertNull(packLine1Data.RelatedEntityCollection);
				AssertNull(packLine2Data.RelatedEntityCollection);
			});
		}

		#endregion
	}
}
