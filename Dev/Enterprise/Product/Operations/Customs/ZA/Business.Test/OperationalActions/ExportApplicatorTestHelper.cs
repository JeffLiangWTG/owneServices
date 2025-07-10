using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	public static class ExportApplicatorTestHelper
	{
		public static (ZGuid warehouseAddressPK, ZGuid productOwnerPK, ZGuid warehousePK, List<CusWHSOperatorTransaction> transactions) SetupTestDataForRecordFilterTesting(BusinessObjectFactory factory)
		{
			var whsHelper = new WhsDataTestHelper(factory);
			var warehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");

			var batch = factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Test Batch 1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = warehouse.WW_OA_WarehouseAddress;

			var typeList = new List<string>
			{
				WarehouseOperatorTransactionExportTypeList.Codes.BLN,
				WarehouseOperatorTransactionExportTypeList.Codes.EXP,
				WarehouseOperatorTransactionTypeList.Codes.ADJ
			};
			var statusList = new List<string>
			{
				WarehouseOperatorTransactionStatusList.Codes.VAL,
				WarehouseOperatorTransactionStatusList.Codes.CLS,
				WarehouseOperatorTransactionStatusList.Codes.QUE
			};

			var transactions = new List<CusWHSOperatorTransaction>();
			var batchLineNo = 1;
			foreach (var exportType in typeList)
			{
				foreach (var status in statusList)
				{
					var transaction = factory.New<CusWHSOperatorTransaction>();
					transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
					transaction.WOT_BatchLineNo = batchLineNo++;
					if (exportType == WarehouseOperatorTransactionTypeList.Codes.ADJ)
					{
						transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ADJ;
					}
					else
					{
						transaction.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
						transaction.WOT_ExportType = exportType;
					}
					transaction.WOT_TransactionDate = ZDate.Today;
					transaction.WOT_OwnerReference = "RefFor" + status;
					transaction.WOT_OH_ProductOwner = whsHelper.Importer.PK;
					transaction.WOT_OP_Product = whsHelper.Part.PK;
					transaction.WOT_Quantity = 1;
					transaction.WOT_TotalValue = 100m;
					transaction.WOT_RX_NKCurrency = "ZAR";
					transaction.WOT_Status = status;
					transaction.WOT_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					transactions.Add(transaction);
				}
			}

			factory.Save();

			return (warehouse.WW_OA_WarehouseAddress, whsHelper.Importer.PK, warehouse.PK, transactions);
		}
	}
}
