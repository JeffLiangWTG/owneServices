using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsAdjustmentAndPickableDocketLineDataObjectReaderTest<TDocket, TDocketLine, TDocketLineReader> : WhsDocketLineDataObjectReaderTest<TDocket, TDocketLine, TDocketLineReader>
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
		where TDocketLineReader : WhsAdjustmentAndPickableDocketLineDataObjectReader<TDocket, TDocketLine>
	{
		#region TestLoadDocketLine

		public void TestLoadDocketLine()
		{
			var docket = GetNewDocketLineParent(Factory);
			var row = docket.Warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 10;
			row.WR_Levels = 8;
			row.WR_Trays = 5;

			Factory.SaveForTesting();

			var docketLineToLoad = docket.Lines.AddNew();
			docketLineToLoad.FillWithValidTestData();
			docketLineToLoad.WE_LineNo = new ZShort(2);
			docketLineToLoad.WE_SubLineNo = new ZShort(4);
			docketLineToLoad.WE_TransactionQuantity = 1m;
			docketLineToLoad.WE_LineComment = "I WILL CHANGE ON UPDATE";

			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			docketLineToLoad.WE_OP = part.PK;
			Helper.SetClientAllAttributeType(docket.Client, false);
			Helper.SetProductAllAttributeUse(docket.Client, part, true);
			Helper.SetClientAttributeType(docket.Client, AttributeNumber.Serial, true);

			Factory.SaveForTesting();

			var docketLineDataObject = GetNewDocketLineDataObject();
			docketLineDataObject.SerialNumber = "";
			var reader = GetNewReader(docketLineDataObject, Logger, docket);
			var docketLineBO = reader.ReadIntoBusinessObject();

			AssertNotNull("docketLineBO", docketLineBO);

			CombineAssertions(delegate
			{
				AssertDocketLineContents(docketLineBO);

				AssertEquals("docketLineBO.PK", docketLineToLoad.PK, docketLineBO.PK);
				AssertMultilineASCIIEquals("Logger.Logs", GetMatchingDocketLineInfoMessage(), Logger.Logs);
			});
		}

		protected virtual string GetMatchingDocketLineInfoMessage()
		{
			return string.Format(@"
Information - Successfully loaded matching {0}.
Information - Populating {0}...
					".Trim(), typeof(TDocketLine).Name);
		}

		protected abstract OrderLine GetNewDocketLineDataObject();
		protected abstract void AssertDocketLineContents(TDocketLine docketLine);

		#endregion

		#region TestPackUQFallback

		public void TestPackUQFallback()
		{
			var docket = GetNewDocketLineParent(Factory);
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT"));
			part.OP_StockKeepingUnit = "TST";
			Factory.SaveForTesting();

			var docketLineDataObject = new OrderLine();
			docketLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			docketLineDataObject.OrderedQty = 1m;
			docketLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "UNT", Description = "Unit" };
			docketLineDataObject.PackageQty = 1m;
			docketLineDataObject.PackageQtyUnit = new PackageType { Code = "", Description = "Unit" };
			var reader = GetNewReader(docketLineDataObject, Logger, docket);
			var docketLineBO = reader.ReadIntoBusinessObject();

			AssertEquals("Since there was no value for PackageQtyUnit on the dataObject, the value of WE_F3_NKPackType should have been defaulted from the Product", part.OP_StockKeepingUnit, docketLineBO.WE_F3_NKPackType);
		}

		#endregion

		#region TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLine

		public void TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLine_Picked()
		{
			TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLineCore(picked: true, customsSetup: false, expectErrorforOrderOnly: true);
		}

		public void TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLine_UnPicked()
		{
			TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLineCore(picked: false, customsSetup: false, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLine_Customs()
		{
			TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLineCore(picked: true, customsSetup: true, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLine_NotCustoms()
		{
			TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLineCore(picked: true, customsSetup: false, expectErrorforOrderOnly: true);
		}

		void TestCustomsShouldBeAbleUpdateProductPKWhenImportingOrderLineCore(bool picked, bool customsSetup, bool expectErrorforOrderOnly)
		{
			if (customsSetup)
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			}
			else
			{
				Data.CreateClientOrgCRAHOLSYDInDB();
			}

			var whs = Data.GetOrCreateWarehouseInDB();
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var supplier1 = Helper.CreateClient("Supplier1");
			var supplier2 = Helper.CreateClient("Supplier2");
			var docket = GetNewDocket(client, whs);
			var product1 = Helper.CreateProduct(client, "DupCode");
			var product2 = Helper.CreateProduct(client, "DupCode");
			product2.OP_IsActive = false;
			product1.RelatedOrganisations.AddSupplier(supplier1);
			product2.RelatedOrganisations.AddSupplier(supplier2);
			docket.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
			if (picked && docket is WhsPickableDocket pickableDocket)
			{
				Helper.CreatePickByAttachingOrders(pickableDocket);
			}
			Factory.SaveForTesting();

			ReadAndAssertProductPK("Should be able to save.", product1, false, false);

			product1.OP_IsActive = false;
			product2.OP_IsActive = true;
			var expectError = expectErrorforOrderOnly && IsPickableDocketLineReader || customsSetup && !SupportsCustomsDataSource;
			ReadAndAssertProductPK($"Should {(expectError ? "not " : "")}be able to update product.", product2, true, expectError);

			void ReadAndAssertProductPK(string assertMsg, OrgSupplierPart product, bool isColumnReadOnly, bool expectException)
			{
				var docketLineDataObject = new OrderLine { Link = 3, Product = new Product { Code = "DupCode" } };
				var reader = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);
				SetOrderLineColumnReadOnly(reader, isColumnReadOnly);
				docketLineDataObject.LineNumber = 1;
				docketLineDataObject.SubLineNumber = 1;
				if (expectException)
				{
					AssertExceptionThrown(assertMsg, typeof(DataObjectReadFailureException),
		$@"Cannot Import {GetDocketType()} Line 3
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
					AssertEquals(assertMsg, product.PK, docketLine.WE_OP);
					Factory.SaveForTesting();
				}
			}
		}

		protected virtual string ExpectedNotAllowedToChangeRestrictedFieldsMessage => throw new NotImplementedException();

		protected virtual bool SupportsCustomsDataSource => true;

		#endregion

		#region TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_PartAttribute1()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, PartAttribute1 = "AT1" }, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_PartAttribute2()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, PartAttribute2 = "AT2" }, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_PartAttribute3()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, PartAttribute3 = "AT3" }, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_ExpiryDate()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, ExpiryDate = ZDateTime.Today.AddMonths(1) }, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_PackingDate()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, PackingDate = ZDateTime.Today }, expectErrorforOrderOnly: false);
		}

		public void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLine_SerialNumber()
		{
			TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(new OrderLine { Link = 3, Product = new Product { Code = "P1" }, SerialNumber = "SN1" }, expectErrorforOrderOnly: false);
		}

		void TestCustomsShouldBeAbleUpdateAttributesWhenImportingOrderLineCore(OrderLine docketLineDataObjectIn, bool expectErrorforOrderOnly)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			var whs = Data.GetOrCreateWarehouseInDB();
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var docket = GetNewDocket(client, whs);
			if (docket is WhsPickableDocket pickableDocket)
			{
				Helper.CreatePickByAttachingOrders(pickableDocket);
			}
			Factory.SaveForTesting();

			ReadAndAssertProductPK("Should be able to save.", new OrderLine { Link = 3, Product = new Product { Code = "P1" } }, false, false);

			var expectError = expectErrorforOrderOnly && IsPickableDocketLineReader;
			ReadAndAssertProductPK($"Should {(expectError ? "not " : "")}be able to update.", docketLineDataObjectIn, true, expectError);

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
{ExpectedNotAllowedToChangeRestrictedFieldsMessage}.", () => reader.ReadIntoBusinessObject());
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

		protected virtual void AssertReadOnlyFields(OrderLine originalDataObject, OrderLine changedDataObject, TDocketLine modifiedDocketLine, bool isCustoms)
		{
			CombineAssertions("Following assertions failed for order/adjustment specific fields:", () =>
			{
				AssertReadOnlyFieldIsUnchanged(originalDataObject.ExpiryDate, changedDataObject.ExpiryDate, modifiedDocketLine.WE_ExpiryDateInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.LineComment, changedDataObject.LineComment, modifiedDocketLine.WE_LineCommentInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.OrderedQty, changedDataObject.OrderedQty, modifiedDocketLine.WE_TransactionQuantityInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PackageQtyUnit.Code, changedDataObject.PackageQtyUnit.Code, modifiedDocketLine.WE_F3_NKPackTypeInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PackingDate, changedDataObject.PackingDate, modifiedDocketLine.WE_PackingDateInfo, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute1, changedDataObject.PartAttribute1, modifiedDocketLine.WE_PartAttrib1Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute2, changedDataObject.PartAttribute2, modifiedDocketLine.WE_PartAttrib2Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.PartAttribute3, changedDataObject.PartAttribute3, modifiedDocketLine.WE_PartAttrib3Info, isCustoms);
				AssertReadOnlyFieldIsUnchanged(originalDataObject.SerialNumber, changedDataObject.SerialNumber, modifiedDocketLine.WE_SerialNumberInfo, isCustoms);
			});
		}

		protected virtual bool IsPickableDocketLineReader => false;

		protected virtual void SetOrderLineColumnReadOnly(TDocketLineReader reader, bool value)
		{
		}

		#endregion

		protected void EnableAllAttributeUse(OrgHeader client, string partNum)
		{
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNum));
			AssertNotNull($"Could not load product from database: {partNum}", part);
			Helper.SetClientAllAttributeType(client, false);
			Helper.SetProductAllAttributeUse(client, part, true);
		}
	}
}
