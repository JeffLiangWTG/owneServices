using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsPickableDocketLineDataObjectWriterTest<TDocketLine, TDocketLineWriter> : WhsDocketLineDataObjectWriterTest<TDocketLine, TDocketLineWriter>
		where TDocketLine : WhsPickableDocketLine
		where TDocketLineWriter : WhsPickableDocketLineDataObjectWriter<TDocketLine>
	{
		#region TestBasicOrderLineLevelFieldMappings

		public void TestBasicOrderLineLevelFieldMappings()
		{
			var line = GetOrderLine(Factory.BOFactory);
			var lineData = GetNewDataObjectWriter(line).GetDataObject(line);

			AssertNotNull("lineData", lineData);

			CombineAssertions(() =>
			{
				AssertContents(lineData);
				AssertEquals("lineData.QuantityMet", 0m, lineData.QuantityMet);
			});
		}

		#endregion

		#region Implementation

		protected sealed override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType);
		protected abstract DataContextType DataContextType { get; }

		protected abstract WhsPickableDocket GetDocket(BusinessObjectFactory factory);

		protected internal TDocketLine GetOrderLine(BusinessObjectFactory factory)
		{
			var docket = GetDocket(factory);

			var line = docket.Lines.AddNew() as TDocketLine;
			line.WE_ExtendedLinePrice = 34.5m;
			line.WE_F3_NKPackType = "PLT";
			line.WE_LineComment = "This is my BOOMSTICK!";
			line.WE_LineNo = 2;

			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "ROLLDOLL";
			product.OP_Desc = "Roll this doll on the soil";
			product.OP_StockKeepingUnit = "NO";
			product.OP_RH_NKCommodityCode = "HAZ";

			line.WE_OP = product.PK;
			line.WE_ExpiryDate = new ZDate(2011, 1, 1);
			line.WE_PackingDate = new ZDate(2011, 1, 2);
			line.WE_PartAttrib1 = "COLOR";
			line.WE_PartAttrib2 = "TUSH";
			line.WE_PartAttrib3 = "SPEED";
			line.WE_SerialNumber = "SERIAL";
			line.WE_RecommendedUnitPrice = 34.2m;
			line.WE_RX_NKUnitPriceCurrency = "USD";
			line.WE_SubLineNo = 3;
			line.WE_UnitDiscountAmount = 21.2m;
			line.WE_UnitDiscountPercent = 2.1m;
			line.WE_UnitPriceAfterDiscount = 14.7m;
			line.WE_TransactionQuantity = 11.2m;

			var helper = new WhsTestHelperFunctions(factory);
			helper.CreateProductUnit(product, Constants.PkgUnit.Pallet, 1000);

			GetOrderLineCore(helper, line);

			return line;
		}

		protected virtual void GetOrderLineCore(WhsTestHelperFunctions helper, TDocketLine docketLine)
		{
		}

		internal void AssertContents(OrderLine lineData)
		{
			AssertEquals("lineData.Commodity.Code", "HAZ", lineData.Commodity.Code);
			AssertEquals("lineData.Commodity.Description", "HAZARDOUS GOODS", lineData.Commodity.Description);
			AssertEquals("lineData.ExpiryDate", new ZDateTime(2011, 1, 1), lineData.ExpiryDate);
			AssertEquals("lineData.ExtendedLinePrice", 34.5m, lineData.ExtendedLinePrice);
			AssertEquals("lineData.LineComment", "This is my BOOMSTICK!", lineData.LineComment);
			AssertEquals("lineData.LineNumber", 2, lineData.LineNumber);
			AssertEquals("lineData.PackingDate", new ZDateTime(2011, 1, 2), lineData.PackingDate);
			AssertEquals("lineData.PackageQty", 11.2m, lineData.PackageQty);
			AssertEquals("lineData.PackageQtyUnit.Code", "PLT", lineData.PackageQtyUnit.Code);
			AssertEquals("lineData.PackageQtyUnit.Description", "Pallet", lineData.PackageQtyUnit.Description);
			AssertEquals("lineData.PartAttribute1", "COLOR", lineData.PartAttribute1);
			AssertEquals("lineData.PartAttribute2", "TUSH", lineData.PartAttribute2);
			AssertEquals("lineData.PartAttribute3", "SPEED", lineData.PartAttribute3);
			AssertEquals("lineData.SerialNumber", "SERIAL", lineData.SerialNumber);
			AssertEquals("lineData.Product.Code", "ROLLDOLL", lineData.Product.Code);
			AssertEquals("lineData.Product.Description", "Roll this doll on the soil", lineData.Product.Description);
			AssertEquals("lineData.UnitPriceRecommended", 34.2m, lineData.UnitPriceRecommended);
			AssertEquals("lineData.SubLineNumber", 3, lineData.SubLineNumber);
			AssertEquals("lineData.UnitPriceDiscountAmount", 21.2m, lineData.UnitPriceDiscountAmount);
			AssertEquals("lineData.UnitPriceDiscountPercent", 2.1m, lineData.UnitPriceDiscountPercent);
			AssertEquals("lineData.UnitPriceAfterDiscount", 14.7m, lineData.UnitPriceAfterDiscount);
			AssertEquals("lineData.UnitPriceCurrency.Code", "USD", lineData.UnitPriceCurrency.Code);
			AssertEquals("lineData.UnitPriceCurrency.Description", "United States Dollar", lineData.UnitPriceCurrency.Description);
			AssertEquals("lineData.OrderedQty", 11.2m, lineData.OrderedQty);
			AssertEquals("lineData.OrderedQtyUnit.Code", "NO", lineData.OrderedQtyUnit.Code);
			AssertEquals("lineData.OrderedQtyUnit.Description", "Number", lineData.OrderedQtyUnit.Description);

			AssertContentsCore(lineData);
		}

		protected virtual void AssertContentsCore(OrderLine lineData)
		{
		}

		#endregion
	}
}
