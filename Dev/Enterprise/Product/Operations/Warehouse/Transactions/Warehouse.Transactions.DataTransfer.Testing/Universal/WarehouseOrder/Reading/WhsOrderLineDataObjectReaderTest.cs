using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsOrderLineDataObjectReaderTest : WhsPickableDocketLineDataObjectReaderTest<WhsOrder, WhsOrderLine, WhsOrderLineDataObjectReader>
	{
		#region TestPickGroupDefaultedFromParameters

		public void TestPickGroupDefaultedFromParameters()
		{
			var whsOrder = GetNewDocketLineParent(Factory);
			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			var productParams = new WhsProductParamsByWhsAndClientCollection(product, Factory.BOFactory);
			var productWhsClientParams = productParams.AddNew();
			productWhsClientParams.W3_OH = whsOrder.WD_OH_Client;
			productWhsClientParams.W3_WW = whsOrder.WD_WW_Whs;
			productWhsClientParams.W3_PickGroup = 13;

			Factory.SaveForTesting();

			var orderLineDataObject = SetupOrderLine();
			var reader = new WhsOrderLineDataObjectReader(orderLineDataObject, Logger, Factory, whsOrder, Array.Empty<WhsOrderLine>());
			var orderLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull("orderLineBO", orderLineBO);
			AssertEquals((short)13, orderLineBO.WE_PickGroup);
		}

		#endregion

		#region TestReadOnlyFields

		public void TestReadOnlyFieldsForCustomsOrder()
		{
			var shipmentDataObject = new Shipment();
			shipmentDataObject.DataContext = DataContextFactory.New();
			shipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DEC123");
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR } } });
			Logger.TopLevelDataObject = shipmentDataObject;
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var order = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineBefore = SetupOrderLine();
			var readerLine = GetNewReader(orderLineBefore, Logger, order, useCleanFactory: false);
			var orderLineBO1 = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Order line must be imported into BO.", orderLineBO1);

			Helper.CreateWhsReceiveWithInventory(order.Client, whs1, "R1", orderLineBO1.SupplierPart, 100m, whs1.FindLocation("A-1"), "");
			Factory.SaveForTesting();

			Helper.CreatePickByAttachingOrders(order);
			order.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			Assert("Order must be finalized", order.IsFinalised);
			Assert("Order line must be finalized", orderLineBO1.IsFinalised);

			var orderLineAfter = SetupOrderLine();
			ChangeOrderLineDataValues(orderLineAfter);

			var readerLineChanged = GetNewReader(orderLineAfter, Logger, order, useCleanFactory: false);
			SetOrderLineColumnReadOnly(readerLineChanged, false);
			var orderLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", orderLineBO1.PK, orderLineBO2.PK);
			AssertReadOnlyFields(orderLineBefore, orderLineAfter, orderLineBO2, isCustoms: true);
		}

		public void TestBreakProductScenario()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whsOrder1 = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineDataObject1 = SetupOrderLine();
			orderLineDataObject1.LineNumber = 0;
			orderLineDataObject1.CustomsData.InwardsEntryKey = null;
			orderLineDataObject1.CustomsData.InwardsEntryLineNumber = null;

			var readerLine1 = new WhsOrderLineDataObjectReader(orderLineDataObject1, Logger, Factory, whsOrder1, Array.Empty<WhsOrderLine>());
			var orderLineBO1 = readerLine1.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Docket line 01 must be imported into BO.", orderLineBO1);

			var client = whsOrder1.Client;
			var product = orderLineBO1.SupplierPart;

			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", product, 100m, whsOrder1.Warehouse.FindLocation("A-1"), "");
			var whsOrder2 = Helper.CreateWhsOrderWithOrderLine(client, whs1, product, 1m);

			Factory.SaveForTesting();

			Helper.CreatePickByAttachingOrders(whsOrder1, whsOrder2);

			Factory.SaveForTesting();

			var orderLineDataObject2 = SetupOrderLine();
			orderLineDataObject2.LineNumber = 1;
			orderLineDataObject2.CustomsData.InwardsEntryKey = null;
			orderLineDataObject2.CustomsData.InwardsEntryLineNumber = null;

			var readerLine2 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject2, Logger, Factory, whsOrder1, Array.Empty<WhsOrderLine>());
			var orderLineBO2 = readerLine2.ReadIntoBusinessObject();

			AssertNotEquals("New line should be created.", orderLineBO1.PK, orderLineBO2.PK);
			AssertEquals("Product should match on both lines.", orderLineBO1.WE_OP, orderLineBO2.WE_OP);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID_ImportPalletID()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whsOrder1 = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineDataObject1 = SetupOrderLine();
			orderLineDataObject1.LineNumber = 0;
			orderLineDataObject1.CustomsData.InwardsEntryKey = null;
			orderLineDataObject1.CustomsData.InwardsEntryLineNumber = null;
			orderLineDataObject1.PalletID = "PLT12345";
			var readerLine2 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject1, Logger, Factory, whsOrder1, Array.Empty<WhsOrderLine>());
			var orderLineBO1 = readerLine2.ReadIntoBusinessObject();

			AssertNotNull("New line should be created.", orderLineBO1.PK);
			AssertEquals("Pallet ID should be imported.", "PLT12345", orderLineBO1.WE_PalletID);
		}

		public void TestPalletID_ImportPalletID_ReadOnly()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 1);
			var whsOrder1 = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineDataObject1 = SetupOrderLine();
			orderLineDataObject1.LineNumber = 0;
			orderLineDataObject1.CustomsData.InwardsEntryKey = null;
			orderLineDataObject1.CustomsData.InwardsEntryLineNumber = null;
			orderLineDataObject1.PalletID = "PLT12345";

			var readerLine2 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject1, Logger, Factory, whsOrder1, Array.Empty<WhsOrderLine>());
			var orderLineBO1 = readerLine2.ReadIntoBusinessObject();

			AssertNotNull("New line should be created.", orderLineBO1.PK);
			AssertEquals("Pallet ID should be imported.", "PLT12345", orderLineBO1.WE_PalletID);

			var product = orderLineBO1.SupplierPart;
			Helper.CreateWhsReceiveWithInventory(whsOrder1.Client, whs1, "R1", product, 100m, whsOrder1.Warehouse.FindLocation("A-1"), "");
			Factory.SaveForTesting();

			Helper.CreatePickByAttachingOrders(whsOrder1);
			whsOrder1.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			orderLineDataObject1.PalletID = "PLT-1";
			var readerLineChanged = GetNewReader(orderLineDataObject1, Logger, whsOrder1, useCleanFactory: false);
			AssertExceptionThrown("Cannot Import Order Line\r\nCannot change product or attributes for allocated order.", typeof(DataObjectReadFailureException), () => readerLineChanged.ReadIntoBusinessObject());
		}

		#endregion

		#region TestHoldCode

		public void TestHoldCode() => TestHoldCode_Core(NewPair(InventoryHoldCodes.Codes.Damaged), InventoryHoldCodes.Codes.Damaged);
		public void TestHoldCode_InvalidCodeError() => TestHoldCode_Core(NewPair("FOO"), "FOO", expectedErrorType: typeof(DataObjectReadFailureException), expectedErrorMessage: "Imported Hold Code 'FOO' is not valid on this system.");
		public void TestHoldCode_NullCode() => TestHoldCode_Core(NewPair(null), ZString.Empty);
		public void TestHoldCode_EmptyCode() => TestHoldCode_Core(NewPair(ZString.Empty), ZString.Empty, expectedErrorType: null, expectedErrorMessage: null);
		public void TestHoldCode_NullPair() => TestHoldCode_Core(null, ZString.Empty);
		public void TestHoldCode_CustomHoldCode()
		{
			Helper.CreateInventoryHeldCode("CUSTOM", "Custom Hold code");
			TestHoldCode_Core(NewPair("CUSTOM"), "CUSTOM", expectedErrorType: null, expectedErrorMessage: null);
		}

		public void TestHoldCode_HeldGoodsForOrdersDisabled() => TestHoldCode_Core(NewPair(InventoryHoldCodes.Codes.Damaged), ZString.Empty, enableHeldGoodsForOrders: false);
		public void TestHoldCode_InvalidCodeError_HeldGoodsForOrdersDisabled() => TestHoldCode_Core(NewPair("FOO"), ZString.Empty, expectedErrorType: null, expectedErrorMessage: null, enableHeldGoodsForOrders: false);
		public void TestHoldCode_NullCode_HeldGoodsForOrdersDisabled() => TestHoldCode_Core(NewPair(null), ZString.Empty, enableHeldGoodsForOrders: false);
		public void TestHoldCode_EmptyCode_HeldGoodsForOrdersDisabled() => TestHoldCode_Core(NewPair(ZString.Empty), ZString.Empty, enableHeldGoodsForOrders: false);
		public void TestHoldCode_NullPair_HeldGoodsForOrdersDisabled() => TestHoldCode_Core(null, ZString.Empty, enableHeldGoodsForOrders: false);
		public void TestHoldCode_CustomHoldCode_HeldGoodsForOrdersDisabled()
		{
			Helper.CreateInventoryHeldCode("CUSTOM", "Custom Hold code");
			TestHoldCode_Core(NewPair("CUSTOM"), ZString.Empty, expectedErrorType: null, expectedErrorMessage: null, enableHeldGoodsForOrders: false);
		}

		void TestHoldCode_Core(CodeDescriptionPair9Char xmlCurrentHoldCodePair, ZString expectedOrderedHeldCode, Type expectedErrorType = null, string expectedErrorMessage = null, bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				Logger.ClearLogs();
				var whs = Helper.CreateWarehouse("WHS1", "A", 2, 1);
				var whsOrder = GetNewDocketLineParent(Factory, whs.PK);
				Factory.SaveForTesting();

				var orderLineDataObject = SetupOrderLine();
				orderLineDataObject.CurrentHoldCode = xmlCurrentHoldCodePair;
				orderLineDataObject.LineNumber = 0;
				orderLineDataObject.Link = 0;
				orderLineDataObject.ExpiryDate = null;
				orderLineDataObject.PartAttribute1 = null;
				orderLineDataObject.PartAttribute2 = null;
				orderLineDataObject.PartAttribute3 = null;
				orderLineDataObject.SerialNumber = null;
				orderLineDataObject.PackingDate = null;

				var reader = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject, Logger, Factory, whsOrder, Array.Empty<WhsOrderLine>());

				if (expectedErrorType != null)
				{
					AssertExceptionThrown(expectedErrorType, expectedErrorMessage, () => reader.ReadIntoBusinessObject());
				}
				else
				{
					var whsOrderLine = reader.ReadIntoBusinessObject();
					AssertNotNull("New line should be created.", whsOrderLine.PK);
					AssertEquals(expectedOrderedHeldCode, whsOrderLine.WE_WHC_NKOrderedHeldCode);
				}
			}
		}

		public void TestHoldCode_ReadOnly() => TestHoldCode_ReadOnly_Core(isSourceCustoms: false, isOrderFinalised: false);
		public void TestHoldCode_ReadOnly_Finalised() => TestHoldCode_ReadOnly_Core(isSourceCustoms: false, isOrderFinalised: true);
		public void TestHoldCode_ReadOnly_Customs() => TestHoldCode_ReadOnly_Core(isSourceCustoms: true, isOrderFinalised: false);
		public void TestHoldCode_ReadOnly_CustomsFinalised() => TestHoldCode_ReadOnly_Core(isSourceCustoms: true, isOrderFinalised: true);

		void TestHoldCode_ReadOnly_Core(bool isSourceCustoms, bool isOrderFinalised)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				if (isSourceCustoms)
				{
					var shipmentDataObject = new Shipment();
					shipmentDataObject.DataContext = DataContextFactory.New();
					shipmentDataObject.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DEC123");
					shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.BWR } } });
					Logger.TopLevelDataObject = shipmentDataObject;
					Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				}

				var whs = Helper.CreateWarehouse("WHS1", "A", 2, 1);
				var whsOrder = GetNewDocketLineParent(Factory, whs.PK);
				Factory.SaveForTesting();

				var orderLineDataObject = SetupOrderLine();
				orderLineDataObject.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Held, Description = InventoryHoldCodes.Descriptions.Held };
				orderLineDataObject.LineNumber = 0;
				orderLineDataObject.Link = 0;
				orderLineDataObject.ExpiryDate = null;
				orderLineDataObject.PartAttribute1 = null;
				orderLineDataObject.PartAttribute2 = null;
				orderLineDataObject.PartAttribute3 = null;
				orderLineDataObject.SerialNumber = null;
				orderLineDataObject.PackingDate = null;

				var reader1 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject, Logger, Factory, whsOrder, Array.Empty<WhsOrderLine>());
				var orderLineBO = reader1.ReadIntoBusinessObject();
				Factory.SaveForTesting();
				AssertNotNull("Precondition: WhsOrderLine created.", orderLineBO.PK);
				AssertEquals("Precondition: Hold Code imported.", InventoryHoldCodes.Codes.Held, orderLineBO.WE_WHC_NKOrderedHeldCode);

				orderLineDataObject.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Damaged, Description = InventoryHoldCodes.Descriptions.Damaged };

				var reader2 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject, Logger, Factory, whsOrder, Array.Empty<WhsOrderLine>());
				AssertNoExceptionThrown("Should be able to save.", () => reader2.ReadIntoBusinessObject());
				AssertEquals("Same orderline", orderLineBO.PK, orderLineBO.PK);
				AssertEquals("Will update when not readonly", InventoryHoldCodes.Codes.Damaged, orderLineBO.WE_WHC_NKOrderedHeldCode);

				// Attach to pick
				var product = orderLineBO.SupplierPart;
				var receive = Helper.CreateWhsReceive(whsOrder.Client, whs, "R1");
				var loc = whsOrder.Warehouse.FindLocation("A-1");
				AssertNotNull(loc);
				Helper.CreateWhsReceiveLine(receive, product, 100m, loc, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				receive.FinaliseDocketWithoutUserConfirmation();
				Factory.SaveForTesting();
				Helper.CreatePickByAttachingOrders(whsOrder);
				if (isOrderFinalised)
				{
					whsOrder.FinaliseDocketWithoutUserConfirmation();
				}
				Factory.SaveForTesting();
				Logger.ClearLogs();

				orderLineDataObject.CurrentHoldCode = new CodeDescriptionPair9Char { Code = InventoryHoldCodes.Codes.Held, Description = InventoryHoldCodes.Descriptions.Held };

				var reader3 = new WhsOrderLineDataObjectReaderForTesting(orderLineDataObject, Logger, Factory, whsOrder, Array.Empty<WhsOrderLine>());
				AssertExceptionThrown("Should not be able to save.", typeof(DataObjectReadFailureException), $@"Cannot Import Order Line 0
{ExpectedNotAllowedToChangeRestrictedFieldsMessage}", () => reader3.ReadIntoBusinessObject());
			}
		}

		static CodeDescriptionPair9Char NewPair(ZString? code) => new CodeDescriptionPair9Char { Code = code, Description = code };

		#endregion

		#region TestAbilityToUpdateAttributesWhenImportingOrderLine

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PartAttribute1()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PartAttribute1 = "AT1", shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PartAttribute2()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PartAttribute2 = "AT2", shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PartAttribute3()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PartAttribute3 = "AT3", shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_SerialNumber()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.SerialNumber = "SN1", shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_ExpiryDate_EmptyToValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.ExpiryDate = ZDateTime.Today.AddMonths(1), shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_ExpiryDate_ValueToDifferentValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.ExpiryDate = ZDateTime.Today.AddMonths(1), ol => ol.ExpiryDate = ZDateTime.Today.AddMonths(3), shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_ExpiryDate_EmptyToEmpty()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.ExpiryDate = ZDateTime.Empty, shouldFailToImport: false);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_ExpiryDate_ValueToSameValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.ExpiryDate = ZDateTime.Today.AddMonths(1), ol => { }, shouldFailToImport: false);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PackingDate_EmptyToValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PackingDate = ZDateTime.Today.AddMonths(1), shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PackingDate_ValueToDifferentValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PackingDate = ZDateTime.Today.AddMonths(1), ol => ol.ExpiryDate = ZDateTime.Today.AddMonths(3), shouldFailToImport: true);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PackingDate_EmptyToEmpty()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PackingDate = ZDateTime.Empty, shouldFailToImport: false);
		}

		public void TestAbilityToUpdateAttributesWhenImportingOrderLine_PackingDate_ValueToSameValue()
		{
			TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => ol.PackingDate = ZDateTime.Today.AddMonths(1), ol => { }, shouldFailToImport: false);
		}

		void TestAbilityToUpdateAttributesWhenImportingOrderLineCore(Action<OrderLine> modifyOrderLineForSecondImport, bool shouldFailToImport = true)
			=> TestAbilityToUpdateAttributesWhenImportingOrderLineCore(ol => { }, modifyOrderLineForSecondImport, shouldFailToImport);

		void TestAbilityToUpdateAttributesWhenImportingOrderLineCore(Action<OrderLine> modifyOrderLineForBothImports, Action<OrderLine> modifyOrderLineForSecondImport, bool shouldFailToImport = true)
		{
			var whs = Data.GetOrCreateWarehouseInDB();
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var product = Data.ProductCRAHOLSYD;
			var docket = GetNewDocket(client, whs);
			Helper.CreatePickByAttachingOrders(docket);
			Factory.SaveForTesting();

			var miscServ = client.MiscServ;
			miscServ.OM_IMUseExpiryDate = true;
			miscServ.OM_IMUsePackingDate = true;

			var relation = product.RelatedOrganisations[0];
			relation.OU_UseExpiryDate = true;
			relation.OU_UsePackingDate = true;
			Factory.SaveForTesting();

			var docketLineDataObject = new OrderLine { Link = 3, Product = new Product { Code = "P1" } };
			modifyOrderLineForBothImports(docketLineDataObject);
			ReadAndAssertProductPK("Should be able to save.", docketLineDataObject, false, false);

			modifyOrderLineForSecondImport(docketLineDataObject);
			ReadAndAssertProductPK("Should not be able to update.", docketLineDataObject, true, shouldFailToImport);

			void ReadAndAssertProductPK(string assertMsg, OrderLine docketLineDataObject, bool isColumnReadOnly, bool expectException)
			{
				var reader = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				SetOrderLineColumnReadOnly(reader, isColumnReadOnly);
				docketLineDataObject.LineNumber = 1;
				docketLineDataObject.SubLineNumber = 1;
				if (expectException)
				{
					AssertExceptionThrown(assertMsg, typeof(DataObjectReadFailureException),
		$@"Cannot Import Order Line 3
{ExpectedNotAllowedToChangeRestrictedFieldsMessage}", () => reader.ReadIntoBusinessObject());
				}
				else
				{
					var docketLine = reader.ReadIntoBusinessObject();
					docketLine.WE_TransactionQuantity = 1m;
					docketLine.IsImportingData = true;
					if (NeedLocation)
					{
						docketLine.WE_WL = whs.DefaultLocation.PK;
					}
					AssertNoExceptionThrown(Factory.SaveForTesting);
				}
			}
		}

		#endregion

		#region Implementation

		public static OrderLine SetupStandardOrderLine() => new WhsOrderLineDataObjectReaderTest().SetupOrderLine();
		public static void AssertStandardOrderLineContents(WhsOrderLine orderLine, bool useSerial) => new WhsOrderLineDataObjectReaderTest().AssertContents(orderLine, useSerial);

		protected override string GetDocketType() => "Order";

		protected override string ExpectedNotAllowedToChangeRestrictedFieldsMessage => "Cannot change product or attributes for allocated order.";

		protected override WhsOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse) => Helper.CreateWhsOrder(client, warehouse);

		protected override WhsOrderLineDataObjectReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, WhsOrder order, bool useCleanFactory = true)
			=> new WhsOrderLineDataObjectReaderForTesting(docketLineDataObject, logger, useCleanFactory ? new UniversalObjectFactory() : Factory, order, Array.Empty<WhsOrderLine>());

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);

		protected override void CreateInventoryForPickableDocket(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units)
		{
			Helper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m, warehouse.FindLocation("A-1"), "");
		}

		protected override bool SupportsProductCreation => true;

		protected override void SetOrderLineColumnReadOnly(WhsOrderLineDataObjectReader reader, bool value)
		{
			((WhsOrderLineDataObjectReaderForTesting)reader).IsColumnReadonlySetValueForTesting = value;
		}

		protected override bool IsColumnReadonlyExposed(WhsOrderLineDataObjectReader reader, WhsOrderLine line, string columnName)
		{
			return ((WhsOrderLineDataObjectReaderForTesting)reader).IsColumnReadonlyExposed(line, columnName);
		}

		#endregion
	}

	#region WhsOrderLineDataObjectReaderForTesting

	class WhsOrderLineDataObjectReaderForTesting : WhsOrderLineDataObjectReader
	{
		internal WhsOrderLineDataObjectReaderForTesting(OrderLine orderLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsOrder parent, IEnumerable<WhsOrderLine> matchedLines)
			: base(orderLineDataObject, logger, factory, parent, matchedLines)
		{
		}

		protected override bool IsColumnReadonly(WhsOrderLine line, string columnName) => IsColumnReadonlySetValueForTesting ?? base.IsColumnReadonly(line, columnName);

		public bool IsColumnReadonlyExposed(WhsOrderLine line, string columnName) => IsColumnReadonly(line, columnName);

		public bool? IsColumnReadonlySetValueForTesting { set; private get; }
	}

	#endregion
}
