using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsPickableDocketLineDataObjectReaderTest<TDocket, TDocketLine, TDocketLineDataObjectReader> : WhsAdjustmentAndPickableDocketLineDataObjectReaderTest<TDocket, TDocketLine, TDocketLineDataObjectReader>
		where TDocket : WhsPickableDocket
		where TDocketLine : WhsPickableDocketLine
		where TDocketLineDataObjectReader : WhsPickableDocketLineDataObjectReader<TDocket, TDocketLine>
	{
		#region TestBasicOrderLineLevelFieldMappings

		public void TestBasicOrderLineLevelFieldMappings()
		{
			var whsOrder = GetNewDocketLineParent(Factory);
			EnableAllAttributeUse(whsOrder.Client, "BOWLHAT");
			Helper.SetClientAttributeType(whsOrder.Client, AttributeNumber.Serial, true);

			Factory.SaveForTesting();

			var orderLineDataObject = SetupOrderLine();
			var reader = GetNewReader(orderLineDataObject, Logger, whsOrder);
			var orderLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull("orderLineBO", orderLineBO);

			CombineAssertions("Should populate value from inventory.", () =>
			{
				AssertContents(orderLineBO, useSerial: true);
				AssertMultilineASCIIEquals("Logger.Logs", GetNoMatchingDocketLineInfoMessage(), Logger.Logs);
			});
		}

		protected virtual string GetNoMatchingDocketLineInfoMessage()
		{
			return $@"
Information - No matching {typeof(TDocketLine).Name} found, creating new {typeof(TDocketLine).Name}.
Information - Populating {typeof(TDocketLine).Name}...
".Trim();
		}

		#endregion

		#region TestOrderedQtyPopulatingFromPackageQty

		public void TestOrderedQtyPopulatingFromPackageQty()
		{
			var whsOrder = GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")); // Get part created in GetNewDocketLineParent
			var partUnits = part.PartUnits;

			AddPartUnits(partUnits, "PEC", 1, "PCS");
			AddPartUnits(partUnits, "UNT", 5, "BOX");

			var orderLineDataObject = CreateOrderLineDataObject(0m, 2m, "TST", "Test");
			var orderLineBO = GetOrderLineBO(whsOrder, orderLineDataObject);
			AssertQuantityAndUnits(orderLineBO, "TST", 2m, 2m);

			var orderLineDataObject2 = CreateOrderLineDataObject(0m, 0m, "TST", "Test");
			var orderLineBO2 = GetOrderLineBO(whsOrder, orderLineDataObject2);
			AssertQuantityAndUnits(orderLineBO2, "TST", 0m, 0m);

			var orderLineDataObject3 = CreateOrderLineDataObject(0m, 2m, "BOX", "Box");
			var orderLineBO3 = GetOrderLineBO(whsOrder, orderLineDataObject3);
			AssertQuantityAndUnits(orderLineBO3, "BOX", 10m, 2m);

			var orderLineDataObject4 = CreateOrderLineDataObject(0m, 0m, "BOX", "Box");
			var orderLineBO4 = GetOrderLineBO(whsOrder, orderLineDataObject4);
			AssertQuantityAndUnits(orderLineBO4, "BOX", 0m, 0m);

			var orderLineDataObject5 = CreateOrderLineDataObject(null, 2m, "BOX", "Box");
			orderLineDataObject5.OrderedQtyUnit = null;
			var orderLineBO5 = GetOrderLineBO(whsOrder, orderLineDataObject5);
			AssertQuantityAndUnits(orderLineBO5, "BOX", 10m, 2m);
		}

		TDocketLine GetOrderLineBO(TDocket whsOrder, OrderLine orderLineDataObject)
		{
			var reader = GetNewReader(orderLineDataObject, Logger, whsOrder, useCleanFactory: false);
			var orderLineBO = reader.ReadIntoBusinessObject();
			return orderLineBO;
		}

		OrderLine CreateOrderLineDataObject(decimal? orderedQty, decimal packageQty, string packageQtyUnit, string packageQtyUnitDesc)
		{
			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.OrderedQty = orderedQty;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.PackageQty = packageQty;
			orderLineDataObject.PackageQtyUnit = new PackageType { Code = packageQtyUnit, Description = packageQtyUnitDesc };

			return orderLineDataObject;
		}

		OrgPartUnit AddPartUnits(OrgPartUnitCollection partUnits, string packType, int quantityInParent, string parentPackType)
		{
			var partUnit = partUnits.AddNew();
			partUnit.OF_PackType = packType;
			partUnit.OF_QuantityInParent = quantityInParent;
			partUnit.OF_ParentPackType = parentPackType;
			return partUnit;
		}

		void AssertQuantityAndUnits(TDocketLine orderLineBO, string packType, decimal wEUnits, decimal wEPackQuantity)
		{
			AssertNotNull("orderLineBO", orderLineBO);

			CombineAssertions(delegate
			{
				AssertEquals("orderLineBO.WE_F3_NKPackType", packType, orderLineBO.WE_F3_NKPackType);
				AssertEquals("orderLineBO.WE_PackQuantity", wEPackQuantity, orderLineBO.WE_PackQuantity);
				AssertEquals("orderLineBO.WE_TransactionQuantity", wEUnits, orderLineBO.WE_TransactionQuantity);
			});
		}

		#endregion

		#region TestErrorIfSuppliedOrderedQtyIsNegative

		public void TestErrorIfSuppliedOrderedQtyIsNegative()
		{
			var whsOrder = GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();

			var orderLineDataObject = CreateOrderLineDataObject(-1m, 2m, "TST", "Test");
			AssertExceptionThrown("Cannot Import Order Line 1:\r\nQuantity cannot be negative.", typeof(DataObjectReadFailureException), () => GetOrderLineBO(whsOrder, orderLineDataObject));
		}

		#endregion

		#region TestSuppliedOrderedQtyIsZero

		public void TestSuppliedOrderedQtyIsZero()
		{
			var whsOrder = GetNewDocketLineParent(Factory);
			Factory.SaveForTesting();

			var orderLineDataObject = CreateOrderLineDataObject(0m, 0m, "TST", "Test");
			var orderLineBO = GetOrderLineBO(whsOrder, orderLineDataObject);
			AssertQuantityAndUnits(orderLineBO, "TST", 0m, 0m);
		}

		#endregion

		#region TestReadOnlyFields

		public void TestReadOnlyFieldsForUnfinalizedOrders()
		{
			var whs1 = GetWarehouse();
			var order = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineBefore = SetupOrderLine();
			var readerLine = GetNewReader(orderLineBefore, Logger, order, useCleanFactory: false);
			var docketLineBizO = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Order line must be imported into BO.", docketLineBizO);

			Factory.SaveForTesting();

			var orderLineAfter = SetupOrderLine();
			ChangeOrderLineDataValues(orderLineAfter);

			var readerLineChanged = GetNewReader(orderLineAfter, Logger, order, useCleanFactory: false);
			var changedOrderLineBO = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", docketLineBizO.PK, changedOrderLineBO.PK);
			AssertReadOnlyFields(orderLineBefore, orderLineAfter, changedOrderLineBO, isCustoms: false);
		}

		public void TestReadOnlyFieldsForPickedOrderLine()
		{
			var whs1 = GetWarehouse();
			var whsOrder = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var product = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")); // Get part created in GetNewDocketLineParent
			CreateInventoryForPickableDocket(whsOrder.Client, whsOrder.Warehouse, product, 100m);
			Factory.SaveForTesting();

			var orderLineDataObject1 = SetupOrderLine();
			orderLineDataObject1.CustomsData.InwardsEntryKey = null;
			orderLineDataObject1.CustomsData.InwardsEntryLineNumber = null;

			var readerLine1 = GetNewReader(orderLineDataObject1, Logger, whsOrder, useCleanFactory: false);
			var orderLineBO1 = readerLine1.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Docket line 01 must be imported into BO.", orderLineBO1);
			AssertEquals("Transaction quantity for picked line was not set correctly.", GetExpectedQuantityForReadOnlyForPickedLineTest(), orderLineBO1.WE_TransactionQuantity);

			var pick = Helper.CreatePickByAttachingOrders(whsOrder);
			var pickLine = pick.GetAllPickLines().First();
			pickLine.WZ_Units = 1m;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;

			Factory.SaveForTesting();
			AssertEquals("Line must be picked.", true, pickLine.IsPickedFromPutawayLocation);

			var orderLineDataObject2 = SetupOrderLine();
			ChangeOrderLineDataValues(orderLineDataObject2);

			SetOrderLineColumnReadOnly(readerLine1, false);
			var orderLineBO2 = readerLine1.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", orderLineBO1.PK, orderLineBO2.PK);
			AssertEquals("Transaction quantity for picked line should be still the same since it is read only.", GetExpectedQuantityForReadOnlyForPickedLineTest(), orderLineBO2.WE_TransactionQuantity);
		}

		protected virtual decimal GetExpectedQuantityForReadOnlyForPickedLineTest() => 28.2m;

		protected abstract void CreateInventoryForPickableDocket(OrgHeader client, WhsWarehouse warehouse, OrgSupplierPart product, ZDecimal units);

		public void TestReadOnlyFieldsForFinalizedOrder()
		{
			var whs1 = GetWarehouse();
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

			SetOrderLineColumnReadOnly(readerLine, false);
			var orderLineBO2 = readerLine.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", orderLineBO1.PK, orderLineBO2.PK);
			AssertReadOnlyFields(orderLineBefore, orderLineAfter, orderLineBO2, isCustoms: false);
		}

		public void TestReadOnlyFieldsForCancelledOrder()
		{
			var whs1 = GetWarehouse();
			var order = GetNewDocketLineParent(Factory, whs1.PK);
			Factory.SaveForTesting();

			var orderLineBefore = SetupOrderLine();
			var readerLine = GetNewReader(orderLineBefore, Logger, order, useCleanFactory: false);
			var orderLineBO1 = readerLine.ReadIntoBusinessObject();
			AssertNotNull("Precondition: Order line must be imported into BO.", orderLineBO1);

			Factory.SaveForTesting();

			order.CancelReactivateDocket();
			Factory.SaveForTesting();
			Assert("Order must be cancelled", order.IsCancelled);
			Assert("Order Line must be cancelled", orderLineBO1.IsDocketCancelled);

			var orderLineAfter = SetupOrderLine();
			ChangeOrderLineDataValues(orderLineAfter);

			var readerLineChanged = GetNewReader(orderLineAfter, Logger, order, useCleanFactory: false);
			var orderLineBO2 = readerLineChanged.ReadIntoBusinessObject();

			AssertEquals("Same line must be modified instead of creating a new one.", orderLineBO1.PK, orderLineBO2.PK);
			AssertReadOnlyFields(orderLineBefore, orderLineAfter, orderLineBO2, isCustoms: false);
		}

		protected virtual WhsWarehouse GetWarehouse()
		{
			return Helper.CreateWarehouse("WHS1", "A", 2, 1);
		}

		#endregion

		#region TestIsColumnReadonly

		public void TestIsColumnReadonly()
		{
			var whs = Helper.CreateWarehouse("WBC", "A", 1, 1);
			var client = Helper.CreateClient("ABC");
			var product = Helper.CreateProduct(client, "PC");
			EnableAllAttributeUse(client, product.OP_PartNum);
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);

			var order = GetNewDocket(client, whs);
			Helper.CreateWhsPickableDocketLine(order, product, 1m);
			Factory.SaveForTesting();

			var orderLineBefore = SetupOrderLine();
			var reader1 = GetNewReader(orderLineBefore, Logger, order, useCleanFactory: false);
			Factory.SaveForTesting();

			var orderLine = (TDocketLine)order.Lines[0];
			AssertEquals("Predondition:WE_OP", false, orderLine.WE_OPInfo.ReadOnly && IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_OP.Name));
			AssertEquals("Predondition:WE_ExpiryDate", false, orderLine.WE_ExpiryDateInfo.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_ExpiryDate.Name));
			AssertEquals("Predondition:WE_PackingDate", false, orderLine.WE_PackingDateInfo.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_PackingDate.Name));
			AssertEquals("Predondition:WE_PartAttrib1", false, orderLine.WE_PartAttrib1Info.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_PartAttrib1.Name));
			AssertEquals("Predondition:WE_PartAttrib2", false, orderLine.WE_PartAttrib2Info.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_PartAttrib2.Name));
			AssertEquals("Predondition:WE_PartAttrib3", false, orderLine.WE_PartAttrib3Info.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_PartAttrib3.Name));
			AssertEquals("Predondition:WE_SerialNumber", false, orderLine.WE_SerialNumberInfo.ReadOnly || IsColumnReadonlyExposed(reader1, orderLine, WhsDocketLineSchema.WE_SerialNumber.Name));

			var pick = Factory.New<WhsPick>();
			order.WD_WP = pick.PK;

			var reader2 = GetNewReader(orderLineBefore, Logger, order, useCleanFactory: false);
			AssertEquals("IsColumnReadonly should match with prop info value.WE_OP", true, orderLine.WE_OPInfo.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_OP.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_ExpiryDate", true, orderLine.WE_ExpiryDateInfo.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_ExpiryDate.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_PackingDate", true, orderLine.WE_PackingDateInfo.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_PackingDate.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_PartAttrib1", true, orderLine.WE_PartAttrib1Info.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_PartAttrib1.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_PartAttrib2", true, orderLine.WE_PartAttrib2Info.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_PartAttrib2.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_PartAttrib3", true, orderLine.WE_PartAttrib3Info.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_PartAttrib3.Name));
			AssertEquals("IsColumnReadonly should match with prop info value.WE_SerialNumber", true, orderLine.WE_SerialNumberInfo.ReadOnly && IsColumnReadonlyExposed(reader2, orderLine, WhsDocketLineSchema.WE_SerialNumber.Name));
		}

		protected abstract bool IsColumnReadonlyExposed(TDocketLineDataObjectReader reader, TDocketLine line, string columnName);

		#endregion

		#region TestProduct_DefaultsPriceFields

		public void TestProduct_DefaultsPriceFields()
		{
			if (SupportsUnitPriceFields)
			{
				var whsOrder = GetNewDocketLineParent(Factory);
				Factory.SaveForTesting();
				var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")); // Get part created in GetNewDocketLineParent
				var relatedOrganisation1 = part.RelatedOrganisations[0];
				relatedOrganisation1.OU_UnitPrice = 10m;
				relatedOrganisation1.OU_RX_NKUnitPriceCurrency = "AUD";
				relatedOrganisation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				var relatedOrganisation2 = part.RelatedOrganisations.AddNew();
				relatedOrganisation2.OU_UnitPrice = 20m;
				relatedOrganisation2.OU_RX_NKUnitPriceCurrency = "USD";
				relatedOrganisation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
				relatedOrganisation2.OU_OH = relatedOrganisation1.OU_OH;
				var relatedOrganisation3 = part.RelatedOrganisations.AddNew();
				relatedOrganisation3.OU_UnitPrice = 30m;
				relatedOrganisation3.OU_RX_NKUnitPriceCurrency = "CNY";
				relatedOrganisation3.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
				relatedOrganisation3.OU_OH = relatedOrganisation1.OU_OH;

				var orderLineDataObject = CreateOrderLineDataObject(0m, 2m, "TST", "Test");
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceAfterDiscount);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceRecommended);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceCurrency);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceDiscountAmount);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceDiscountPercent);

				var orderLineBO = GetOrderLineBO(whsOrder, orderLineDataObject);
				CombineAssertions(() =>
				{
					AssertEquals("Price info values populated from product.", 20m, orderLineBO.WE_UnitPriceAfterDiscount);
					AssertEquals("Price info values populated from product.", 20m, orderLineBO.WE_RecommendedUnitPrice);
					AssertEquals("Price info values populated from product.", "USD", orderLineBO.WE_RX_NKUnitPriceCurrency);
					AssertEquals("Price info values populated from product except discount amount.", 0m, orderLineBO.WE_UnitDiscountAmount);
					AssertEquals("Price info values populated from product except discount amount.", 0m, orderLineBO.WE_UnitDiscountPercent);
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestProduct_DefaultsPriceFields_WithExistingPriceInfoValues()
		{
			if (SupportsUnitPriceFields)
			{
				var whsOrder = GetNewDocketLineParent(Factory);
				Factory.SaveForTesting();
				var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")); // Get part created in GetNewDocketLineParent
				var relatedOrganization = part.RelatedOrganisations[0];
				relatedOrganization.OU_UnitPrice = 10m;
				relatedOrganization.OU_RX_NKUnitPriceCurrency = "AUD";

				var orderLine = whsOrder.Lines.AddNew();
				orderLine.WE_LineNo = 1;
				orderLine.WE_SubLineNo = 1;
				orderLine.WE_RecommendedUnitPrice = 5m;
				orderLine.WE_UnitPriceAfterDiscount = 4m;
				orderLine.WE_RX_NKUnitPriceCurrency = "USD";
				orderLine.WE_UnitDiscountAmount = 1m;
				orderLine.WE_UnitDiscountPercent = 20m;

				var orderLineDataObject = CreateOrderLineDataObject(0m, 2m, "TST", "Test");
				orderLineDataObject.LineNumber = 1;
				orderLineDataObject.SubLineNumber = 1;
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceAfterDiscount);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceRecommended);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceCurrency);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceDiscountAmount);
				AssertEquals("Precondition: default data object price info values.", null, orderLineDataObject.UnitPriceDiscountPercent);

				var orderLineBO = GetOrderLineBO(whsOrder, orderLineDataObject);
				AssertEquals("Price info values populated from product.", 4m, orderLineBO.WE_UnitPriceAfterDiscount);
				AssertEquals("Price info values populated from product.", 5m, orderLineBO.WE_RecommendedUnitPrice);
				AssertEquals("Price info values populated from product.", "USD", orderLineBO.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Price info values populated from product except discount amount.", 1m, orderLineBO.WE_UnitDiscountAmount);
				AssertEquals("Price info values populated from product except discount amount.", 20m, orderLineBO.WE_UnitDiscountPercent);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestProduct_DefaultsPriceFields_LineDataObjectWithPriceValues()
		{
			if (SupportsUnitPriceFields)
			{
				var whsOrder = GetNewDocketLineParent(Factory);
				Factory.SaveForTesting();
				var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")); // Get part created in GetNewDocketLineParent
				var relatedOrganization = part.RelatedOrganisations[0];
				relatedOrganization.OU_UnitPrice = 10m;
				relatedOrganization.OU_RX_NKUnitPriceCurrency = "AUD";

				var orderLineDataObject = CreateOrderLineDataObject(0m, 2m, "TST", "Test");
				orderLineDataObject.UnitPriceAfterDiscount = 6m;
				orderLineDataObject.UnitPriceRecommended = 8m;
				orderLineDataObject.UnitPriceCurrency = new Currency() { Code = "USD", Description = "USD" };
				orderLineDataObject.UnitPriceDiscountAmount = 2m;
				orderLineDataObject.UnitPriceDiscountPercent = 25m;

				var orderLineBO = GetOrderLineBO(whsOrder, orderLineDataObject);
				AssertEquals("Price info values populated from dataObject.", 6m, orderLineBO.WE_UnitPriceAfterDiscount);
				AssertEquals("Price info values populated from dataObject.", 8m, orderLineBO.WE_RecommendedUnitPrice);
				AssertEquals("Price info values populated from dataObject.", "USD", orderLineBO.WE_RX_NKUnitPriceCurrency);
				AssertEquals("Price info values populated from dataObject.", 2m, orderLineBO.WE_UnitDiscountAmount);
				AssertEquals("Price info values populated from dataObject.", 25m, orderLineBO.WE_UnitDiscountPercent);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool SupportsUnitPriceFields => true;

		#endregion

		#region Implementation

		protected virtual OrderLine SetupOrderLine()
		{
			var orderLineDataObject = new OrderLine();
			orderLineDataObject.Commodity = new Commodity { Code = "CMM", Description = "Commm" };
			orderLineDataObject.CustomsData = WhsBondedWarehouseAttributeReadingHelperTest.GetCustomsEntryInfo();
			orderLineDataObject.ExpiryDate = new ZDate(2011, 1, 2);
			orderLineDataObject.LineComment = "COMM MENT";
			orderLineDataObject.LineNumber = new ZShort(2);
			SetOrderedQty(orderLineDataObject);
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			orderLineDataObject.PackageQty = 33.3m;
			orderLineDataObject.PackageQtyUnit = new PackageType { Code = "CTN", Description = "Carton" };
			orderLineDataObject.PackingDate = new ZDate(2011, 1, 3);
			orderLineDataObject.PartAttribute1 = "Colour";
			orderLineDataObject.PartAttribute2 = "Size";
			orderLineDataObject.PartAttribute3 = "Batch Number";
			orderLineDataObject.SerialNumber = "Serial Number";
			orderLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			orderLineDataObject.QuantityMet = 11.1m;
			orderLineDataObject.ReservedQuantity = 44.4m;
			orderLineDataObject.ShortfallQuantity = 55.5m;
			orderLineDataObject.SubLineNumber = new ZShort(4);
			if (SupportsUnitPriceFields)
			{
				orderLineDataObject.ExtendedLinePrice = 66.6m;
				orderLineDataObject.UnitPriceAfterDiscount = 66.6m;
				orderLineDataObject.UnitPriceCurrency = new Currency { Code = "USD", Description = "US Dollars" };
				orderLineDataObject.UnitPriceDiscountAmount = 77.7m;
				orderLineDataObject.UnitPriceDiscountPercent = 88.8m;
				orderLineDataObject.UnitPriceRecommended = 99.9m;
			}
			return orderLineDataObject;
		}

		protected virtual void SetOrderedQty(OrderLine orderLineDataObject, bool isUpdate = false)
		{
			orderLineDataObject.OrderedQty = isUpdate ? 20.2m : 28.2m;
		}

		protected void ChangeOrderLineDataValues(OrderLine orderLineDataObject)
		{
			orderLineDataObject.ExpiryDate = new ZDate(2015, 12, 31);
			orderLineDataObject.ExtendedLinePrice = 10.1;
			orderLineDataObject.LineComment = "ChangedComment";
			SetOrderedQty(orderLineDataObject, isUpdate: true);
			orderLineDataObject.PackageQty = 30.3m;
			orderLineDataObject.PackageQtyUnit = new PackageType { Code = "BOX", Description = "Box" };
			orderLineDataObject.PackingDate = new ZDate(2015, 11, 30);
			orderLineDataObject.PartAttribute1 = "Changed01";
			orderLineDataObject.PartAttribute2 = "Changed02";
			orderLineDataObject.PartAttribute3 = "Changed03";
			orderLineDataObject.SerialNumber = "Changed04";
			orderLineDataObject.UnitPriceAfterDiscount = 40.4m;
			orderLineDataObject.UnitPriceCurrency = new Currency { Code = "AUD", Description = "Australian Dollar" };
			orderLineDataObject.UnitPriceDiscountAmount = 50.5m;
			orderLineDataObject.UnitPriceDiscountPercent = 60.6m;
			orderLineDataObject.UnitPriceRecommended = 70.7m;
		}

		protected void AssertContents(TDocketLine orderLine, bool useSerial)
		{
			AssertEquals("orderLineBO.WE_ExpiryDate", new ZDate(2011, 1, 2), orderLine.WE_ExpiryDate);
			AssertEquals("orderLineBO.WE_ExtendedLinePrice", SupportsUnitPriceFields ? 66.6m : 0m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("orderLineBO.WE_F3_NKPackType", "CTN", orderLine.WE_F3_NKPackType);
			AssertEquals("orderLineBO.WE_LineComment", "COMM MENT", orderLine.WE_LineComment);
			AssertEquals("orderLineBO.WE_LineNo", new ZShort(2), orderLine.WE_LineNo);
			AssertEquals("orderLineBO.ProductCode", "BOWLHAT", orderLine.ProductCode);
			AssertEquals("orderLineBO.ProductDesc", "Bowler Hat", orderLine.ProductDesc);
			AssertEquals("orderLineBO.WE_PackingDate", new ZDate(2011, 1, 3), orderLine.WE_PackingDate);
			AssertOrderedQuantity(orderLine);
			AssertEquals("orderLineBO.WE_PartAttrib1", "Colour", orderLine.WE_PartAttrib1);
			AssertEquals("orderLineBO.WE_PartAttrib2", "Size", orderLine.WE_PartAttrib2);
			AssertEquals("orderLineBO.WE_PartAttrib3", "Batch Number", orderLine.WE_PartAttrib3);
			AssertEquals("orderLineBO.WE_SerialNumber", useSerial ? "Serial Number" : "", orderLine.WE_SerialNumber);
			AssertEquals("orderLineBO.WE_RecommendedUnitPrice", SupportsUnitPriceFields ? 99.9m : 0m, orderLine.WE_RecommendedUnitPrice);
			AssertEquals("orderLineBO.WE_RX_NKUnitPriceCurrency", SupportsUnitPriceFields ? "USD" : string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("orderLineBO.WE_SubLineNo", new ZShort(4), orderLine.WE_SubLineNo);
			AssertEquals("orderLineBO.WE_UnitDiscountAmount", SupportsUnitPriceFields ? 77.7m : 0m, orderLine.WE_UnitDiscountAmount);
			AssertEquals("orderLineBO.WE_UnitDiscountPercent", SupportsUnitPriceFields ? 88.8m : 0m, orderLine.WE_UnitDiscountPercent);
			AssertEquals("orderLineBO.WE_UnitPriceAfterDiscount", SupportsUnitPriceFields ? 66.6m : 0m, orderLine.WE_UnitPriceAfterDiscount);
			AssertEquals("orderLineBO.ProductUQ", "UNT", orderLine.ProductUQ);
			AssertEquals("orderLineBO.CommodityCode", "CMM", orderLine.CommodityCode);

			var customsDataBO = orderLine.CustomsData;
			if (SupportsCustomData)
			{
				AssertCustomsData(orderLine, customsDataBO);
			}
			else
			{
				AssertEquals("orderLineBO.WE_BondedEntryKey", "", orderLine.WE_BondedEntryKey);

				AssertEquals("customsDataBO.WB_EntryKey", "", customsDataBO.WB_EntryKey);
				AssertEquals("customsDataBO.WB_EntryLineNo", ZShort.Zero, customsDataBO.WB_EntryLineNo);
				AssertEquals("customsDataBO.OutwardType", "", customsDataBO.WB_OutwardType);

				AssertEquals("customsDataBO.WB_AddInfo", "", customsDataBO.WB_AddInfo);
				AssertEquals("customsDataBO.WB_CustomsQty", 0m, customsDataBO.WB_CustomsQty);
				AssertEquals("customsDataBO.WB_CustomsUnitOfQty", "", customsDataBO.WB_CustomsUnitOfQty);
				AssertEquals("customsDataBO.WB_DeclarationReference", "", customsDataBO.WB_DeclarationReference);
				AssertEquals("customsDataBO.WB_EntryDate", ZDateTime.Empty, customsDataBO.WB_EntryDate);
				AssertEquals("customsDataBO.WB_RN_NKCountryOfOrigin", "", customsDataBO.WB_RN_NKCountryOfOrigin);
				AssertEquals("customsDataBO.WB_TILV", 0m, customsDataBO.WB_TILV);
				AssertEquals("customsDataBO.WB_ValueForDuty", 0m, customsDataBO.WB_ValueForDuty);
				AssertEquals("customsDataBO.WB_CustomsSecondQuantity", 0m, customsDataBO.WB_CustomsSecondQuantity);
				AssertEquals("customsDataBO.WB_CustomsSecondUnitQty", "", customsDataBO.WB_CustomsSecondUnitQty);
				AssertEquals("customsDataBO.WB_Tariff", "", customsDataBO.WB_Tariff);
				AssertEquals("customsDataBO.WB_PrimaryPreference", "", customsDataBO.WB_PrimaryPreference);
				AssertEquals("customsDataBO.WB_CustomsThirdQuantity", 0m, customsDataBO.WB_CustomsThirdQuantity);
				AssertEquals("customsDataBO.WB_CustomsThirdUnitQty", "", customsDataBO.WB_CustomsThirdUnitQty);
				AssertEquals("customsDataBO.WB_ZoneStatus", "", customsDataBO.WB_ZoneStatus);
				AssertEquals("customsDataBO.WB_IsFromAnotherFTZWhs", false, customsDataBO.WB_IsFromAnotherFTZWhs);
			}
		}

		protected virtual void AssertOrderedQuantity(TDocketLine orderLine)
		{
			AssertEquals("orderLineBO.WE_PackQuantity", 28.2m, orderLine.WE_PackQuantity);
			AssertEquals("orderLineBO.WE_TransactionQuantity", 28.2m, orderLine.WE_TransactionQuantity);
		}

		protected virtual void AssertCustomsData(TDocketLine orderLine, WhsBondedWarehouseAttribute customsDataBO)
		{
			AssertEquals("orderLineBO.WE_BondedEntryKey should be inwards entry", "WHATNIP-9", orderLine.WE_BondedEntryKey);

			AssertEquals("customsDataBO.WB_EntryKey", "KEYZOR", customsDataBO.WB_EntryKey);
			AssertEquals("customsDataBO.WB_EntryLineNo", (ZShort)5, customsDataBO.WB_EntryLineNo);
			AssertEquals("customsDataBO.OutwardType", "CNN", customsDataBO.WB_OutwardType);

			AssertEquals("customsDataBO.WB_AddInfo", "This was information that wasn't useful", customsDataBO.WB_AddInfo);
			AssertEquals("customsDataBO.WB_CustomsQty", 13.3m, customsDataBO.WB_CustomsQty);
			AssertEquals("customsDataBO.WB_CustomsUnitOfQty", "KG", customsDataBO.WB_CustomsUnitOfQty);
			AssertEquals("customsDataBO.WB_DeclarationReference", "Polo", customsDataBO.WB_DeclarationReference);
			AssertEquals("customsDataBO.WB_EntryDate", new ZDateTime(2011, 1, 1), customsDataBO.WB_EntryDate);
			AssertEquals("customsDataBO.WB_RN_NKCountryOfOrigin", "AU", customsDataBO.WB_RN_NKCountryOfOrigin);
			AssertEquals("customsDataBO.WB_TILV", 32.9m, customsDataBO.WB_TILV);
			AssertEquals("customsDataBO.WB_ValueForDuty", 24.5m, customsDataBO.WB_ValueForDuty);
			AssertEquals("customsDataBO.WB_CustomsSecondQuantity", 64.8m, customsDataBO.WB_CustomsSecondQuantity);
			AssertEquals("customsDataBO.WB_CustomsSecondUnitQty", "GRM", customsDataBO.WB_CustomsSecondUnitQty);
			AssertEquals("customsDataBO.WB_Tariff", "TRF", customsDataBO.WB_Tariff);
			AssertEquals("customsDataBO.WB_PrimaryPreference", "STANDARD", customsDataBO.WB_PrimaryPreference);
			AssertEquals("customsDataBO.WB_CustomsThirdQuantity", 12.3m, customsDataBO.WB_CustomsThirdQuantity);
			AssertEquals("customsDataBO.WB_CustomsThirdUnitQty", "GRM", customsDataBO.WB_CustomsThirdUnitQty);
			AssertEquals("customsDataBO.WB_ZoneStatus", "D", customsDataBO.WB_ZoneStatus);
			AssertEquals("customsDataBO.WB_IsFromAnotherFTZWhs", true, customsDataBO.WB_IsFromAnotherFTZWhs);
		}

		protected virtual bool SupportsCustomData => true;

		protected override void AssertReadOnlyFields(OrderLine originalDataObject, OrderLine changedDataObject, TDocketLine modifiedDocketLine, bool isCustoms)
		{
			base.AssertReadOnlyFields(originalDataObject, changedDataObject, modifiedDocketLine, isCustoms);
			if (SupportsUnitPriceFields)
			{
				CombineAssertions("Following assertions failed for order specific fields:", () =>
				{
					AssertReadOnlyFieldIsUnchanged(originalDataObject.ExtendedLinePrice, changedDataObject.ExtendedLinePrice, modifiedDocketLine.WE_ExtendedLinePriceInfo, isCustoms);
					AssertReadOnlyFieldIsUnchanged(originalDataObject.UnitPriceAfterDiscount, changedDataObject.UnitPriceAfterDiscount, modifiedDocketLine.WE_UnitPriceAfterDiscountInfo, isCustoms);
					AssertReadOnlyFieldIsUnchanged(originalDataObject.UnitPriceCurrency?.Code, changedDataObject.UnitPriceCurrency?.Code, modifiedDocketLine.WE_RX_NKUnitPriceCurrencyInfo, isCustoms);
					AssertReadOnlyFieldIsUnchanged(originalDataObject.UnitPriceDiscountAmount, changedDataObject.UnitPriceDiscountAmount, modifiedDocketLine.WE_UnitDiscountAmountInfo, isCustoms);
					AssertReadOnlyFieldIsUnchanged(originalDataObject.UnitPriceDiscountPercent, changedDataObject.UnitPriceDiscountPercent, modifiedDocketLine.WE_UnitDiscountPercentInfo, isCustoms);
				});
			}
		}

		protected override OrderLine GetNewDocketLineDataObject() => SetupOrderLine();

		protected override void AssertDocketLineContents(TDocketLine docketLine) => AssertContents(docketLine, useSerial: false);

		protected override bool NeedLocation => false;

		protected override bool IsPickableDocketLineReader => true;

		#endregion
	}
}
