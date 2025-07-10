using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	sealed class WarehouseOperatorTransactionReconReport : ReportFunctionalTestCase
	{
		protected override ZString ObjectName => "WarehouseOperatorTransactionReconReport";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder => new List<ReportSchemaColumn>
		{
			new ReportSchemaColumn(typeof(string), "TransactionType"),
			new ReportSchemaColumn(typeof(string), "ExportType"),
			new ReportSchemaColumn(typeof(string), "OwnerReference"),
			new ReportSchemaColumn(typeof(string), "ProductCode"),
			new ReportSchemaColumn(typeof(Guid), "ProductPK"),
			new ReportSchemaColumn(typeof(decimal), "OrderQuantity"),
			new ReportSchemaColumn(typeof(decimal), "CustomsBalance"),
			new ReportSchemaColumn(typeof(decimal), "AllocatedBondQuantity"),
			new ReportSchemaColumn(typeof(decimal), "FreeStoreBalance"),
			new ReportSchemaColumn(typeof(decimal), "AllocatedFreeStoreQuantity"),
			new ReportSchemaColumn(typeof(string), "DeclarationReference"),
			new ReportSchemaColumn(typeof(decimal), "InvoiceLineCountableQuantity"),
			new ReportSchemaColumn(typeof(string), "CustomsEntryNumber"),
		};

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override void SetUp()
		{
			effectiveDate = ZDateTime.Now;
			dutyHelper = new ZAWhsInventoryDutyAndTaxCalculatorTestHelper();
			whsHelper = new ZAWhsDataTestHelper(Factory);
			whsHelper.Warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			whsWarehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "WZ8");

			var arrivalDate = dutyHelper.StartDate.AddDays(30);

			Factory.Save();

			var batch1 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch1.WOB_Batch = "Batch1";
			batch1.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch1.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			var order1 = Factory.New<CusWHSOperatorTransaction>();
			order1.WOT_BatchLineNo = 1;
			order1.WOT_ExportType = WarehouseOperatorTransactionExportTypeList.Codes.EXP;
			order1.WOT_IsCustomsControlled = false;
			order1.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			order1.WOT_OP_Product = whsHelper.Part.PK;
			order1.WOT_OwnerReference = "OwnerReference";
			order1.WOT_Quantity = 100m;
			order1.WOT_RN_NKOrigin = ZString.Empty;
			order1.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order1.WOT_TotalValue = 1000m;
			order1.WOT_TransactionDate = arrivalDate.Date;
			order1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;

			var order2 = Factory.New<CusWHSOperatorTransaction>();
			order2.WOT_BatchLineNo = 2;
			order2.WOT_ExportType = ZString.Empty;
			order2.WOT_IsCustomsControlled = false;
			order2.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			order2.WOT_OP_Product = whsHelper.Part.PK;
			order2.WOT_OwnerReference = "OwnerReference";
			order2.WOT_Quantity = 100m;
			order2.WOT_RN_NKOrigin = ZString.Empty;
			order2.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order2.WOT_TotalValue = 1000m;
			order2.WOT_TransactionDate = arrivalDate.Date;
			order2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order2.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			order2.WOT_CustomsEntryNumber = "ENT3243";

			var order3 = Factory.New<CusWHSOperatorTransaction>();
			order3.WOT_BatchLineNo = 3;
			order3.WOT_ExportType = WarehouseOperatorTransactionExportTypeList.Codes.EXP;
			order3.WOT_IsCustomsControlled = false;
			order3.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			order3.WOT_OP_Product = whsHelper.Part.PK;
			order3.WOT_OwnerReference = "OwnerReference";
			order3.WOT_Quantity = 100m;
			order3.WOT_RN_NKOrigin = ZString.Empty;
			order3.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			order3.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			order3.WOT_TotalValue = 1000m;
			order3.WOT_TransactionDate = arrivalDate.Date;
			order3.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			order3.WOT_WOB_CusWHSTransactionBatch = batch1.PK;

			var receipt1 = Factory.New<CusWHSOperatorTransaction>();
			receipt1.WOT_BatchLineNo = 4;
			receipt1.WOT_ExportType = ZString.Empty;
			receipt1.WOT_IsCustomsControlled = true;
			receipt1.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt1.WOT_OP_Product = whsHelper.Part.PK;
			receipt1.WOT_OwnerReference = "OwnerReference";
			receipt1.WOT_Quantity = 500m;
			receipt1.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt1.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt1.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt1.WOT_TotalValue = 5000m;
			receipt1.WOT_TransactionDate = arrivalDate.Date;
			receipt1.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;

			var batch2 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch2.WOB_Batch = "Batch2";
			batch2.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch2.WOB_OA_Warehouse = whsWarehouse.WW_OA_WarehouseAddress;

			// Local Stock
			var receipt2 = Factory.New<CusWHSOperatorTransaction>();
			receipt2.WOT_BatchLineNo = 5;
			receipt2.WOT_ExportType = ZString.Empty;
			receipt2.WOT_IsCustomsControlled = false;
			receipt2.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt2.WOT_OP_Product = whsHelper.Part.PK;
			receipt2.WOT_OwnerReference = "OwnerReference";
			receipt2.WOT_Quantity = 111m;
			receipt2.WOT_RN_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			receipt2.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt2.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt2.WOT_TotalValue = 1111m;
			receipt2.WOT_TransactionDate = arrivalDate.Date;
			receipt2.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt2.WOT_WOB_CusWHSTransactionBatch = batch2.PK;
			receipt2.WOT_CustomsEntryNumber = "ENT3243";

			// Duty Paid Stock
			var receipt3 = Factory.New<CusWHSOperatorTransaction>();
			receipt3.WOT_BatchLineNo = 6;
			receipt3.WOT_ExportType = ZString.Empty;
			receipt3.WOT_IsCustomsControlled = false;
			receipt3.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			receipt3.WOT_OP_Product = whsHelper.Part.PK;
			receipt3.WOT_OwnerReference = "OwnerReference";
			receipt3.WOT_Quantity = 222m;
			receipt3.WOT_RN_NKOrigin = Core.Constants.CountryCodes.China;
			receipt3.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			receipt3.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			receipt3.WOT_TotalValue = 2222m;
			receipt3.WOT_TransactionDate = arrivalDate.Date;
			receipt3.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			receipt3.WOT_WOB_CusWHSTransactionBatch = batch2.PK;

			var orderTransactionLine1 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine1.WOL_Quantity = 100m;
			orderTransactionLine1.WOL_WOT_WHSOperatorTransactionOrder = order1.PK;
			orderTransactionLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var orderTransactionLine2 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine2.WOL_Quantity = 50m;
			orderTransactionLine2.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			orderTransactionLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			var entry = whsHelper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", whsHelper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", whsHelper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 25m);
			entry.InvoiceLines.First().JI_ParentID = orderTransactionLine2.PK;

			var orderTransactionLine3 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine3.WOL_Quantity = 25m;
			orderTransactionLine3.WOL_WOT_WHSOperatorTransactionOrder = order2.PK;
			orderTransactionLine3.WOL_WOT_WHSOperatorTransactionReceipt = receipt2.PK;
			var entry2 = whsHelper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001234", whsHelper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT1435", whsHelper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 10m);
			entry2.InvoiceLines.First().JI_ParentID = orderTransactionLine3.PK;

			var orderTransactionLine4 = Factory.New<CusWHSOperatorTransactionLine>();
			orderTransactionLine4.WOL_Quantity = 100m;
			orderTransactionLine4.WOL_WOT_WHSOperatorTransactionOrder = order3.PK;
			orderTransactionLine4.WOL_WOT_WHSOperatorTransactionReceipt = receipt1.PK;

			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE {CusWHSOperatorTransactionLineSchema.Constants.SqlSchemaName}.{CusWHSOperatorTransactionLineSchema.Constants.TableName} SET {CusWHSOperatorTransactionLine.Schema.WOL_SystemCreateTimeUtc} = '{effectiveDate.AddDays(1)}', {CusWHSOperatorTransactionLine.Schema.WOL_SystemLastEditTimeUtc} = '{effectiveDate.AddDays(1)}', {CusWHSOperatorTransactionLine.Schema.WOL_SystemLastEditUser} = '{GlbStaff.CurrentUser.GS_Code.ToString()}'");
		}

		protected override List<string> ParametersValuesList => new List<string>
		{
			QuoteParameter(GlbCompany.CurrentCompany.PK), // @CompanyPK
			QuoteParameter(whsWarehouse.PK), // @WarehousePK
			QuoteParameter(whsWarehouse.WW_OA_WarehouseAddress), // @WarehouseAddressPK
			QuoteParameter(whsHelper.Importer.PK), // @ClientPK
			QuoteParameter(effectiveDate) // @EffectiveDate
		};

		protected override void AssertTestResults(DataTable results)
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, results.Rows.Count);
				var row1 = FormatRowsValues(results.Rows[0], results, true);
				var row2 = FormatRowsValues(results.Rows[1], results, true);
				var row3 = FormatRowsValues(results.Rows[2], results, true);
				var row4 = FormatRowsValues(results.Rows[3], results, true);

				AssertMultilineASCIIEquals("Row1", "[TransactionType]='ORD'; [ExportType]=''; [OwnerReference]='OwnerReference'; [ProductCode]='~~1'; [OrderQuantity]='100.00000'; [CustomsBalance]='500.00000'; [AllocatedBondQuantity]='0.00000'; [FreeStoreBalance]='333.00000'; [AllocatedFreeStoreQuantity]='25.00000'; [DeclarationReference]='BZA00001234'; [InvoiceLineCountableQuantity]='10.00000'; [CustomsEntryNumber]='ENT3243'", row1);
				AssertMultilineASCIIEquals("Row2", "[TransactionType]='ORD'; [ExportType]=''; [OwnerReference]='OwnerReference'; [ProductCode]='~~1'; [OrderQuantity]='100.00000'; [CustomsBalance]='500.00000'; [AllocatedBondQuantity]='50.00000'; [FreeStoreBalance]='333.00000'; [AllocatedFreeStoreQuantity]='0.00000'; [DeclarationReference]='BZA00001230'; [InvoiceLineCountableQuantity]='25.00000'; [CustomsEntryNumber]=''", row2);
				AssertMultilineASCIIEquals("Row3", "[TransactionType]='ORD'; [ExportType]='EXP'; [OwnerReference]='OwnerReference'; [ProductCode]='~~1'; [OrderQuantity]='100.00000'; [CustomsBalance]='500.00000'; [AllocatedBondQuantity]='100.00000'; [FreeStoreBalance]='333.00000'; [AllocatedFreeStoreQuantity]='0.00000'; [DeclarationReference]=''; [InvoiceLineCountableQuantity]='0.00000'; [CustomsEntryNumber]=''", row3);
				AssertMultilineASCIIEquals("Row4", "[TransactionType]='ORD'; [ExportType]='EXP'; [OwnerReference]='OwnerReference'; [ProductCode]='~~1'; [OrderQuantity]='100.00000'; [CustomsBalance]='500.00000'; [AllocatedBondQuantity]='100.00000'; [FreeStoreBalance]='333.00000'; [AllocatedFreeStoreQuantity]='0.00000'; [DeclarationReference]=''; [InvoiceLineCountableQuantity]='0.00000'; [CustomsEntryNumber]=''", row4);
			});
		}

		protected override void PrepareTestData()
		{
		}

		ZAWhsDataTestHelper whsHelper;
		ZAWhsInventoryDutyAndTaxCalculatorTestHelper dutyHelper;
		IWhsWarehouse whsWarehouse;
		ZDateTime effectiveDate = ZDateTime.Now;
	}
}
