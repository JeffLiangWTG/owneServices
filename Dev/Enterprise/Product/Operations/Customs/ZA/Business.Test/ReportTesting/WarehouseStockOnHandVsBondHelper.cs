using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	internal static class WarehouseStockOnHandVsBondHelper
	{
		internal static CusWHSOperatorTransaction CreateOrder(BusinessObjectFactory factory, ZAWhsDataTestHelper whsHelper, CusWHSOperatorTransactionBatch batch, ZDateTime transactionDate, int batchLineNo, string exportType, decimal quantity, decimal totalValue)
		{
			var result = factory.New<CusWHSOperatorTransaction>();
			result.WOT_BatchLineNo = batchLineNo;
			result.WOT_ExportType = exportType;
			result.WOT_IsCustomsControlled = false;
			result.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			result.WOT_OP_Product = whsHelper.Part.PK;
			result.WOT_OwnerReference = "OwnerReference";
			result.WOT_Quantity = quantity;
			result.WOT_RN_NKOrigin = ZString.Empty;
			result.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			result.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			result.WOT_TotalValue = totalValue;
			result.WOT_TransactionDate = transactionDate.Date;
			result.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			result.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			return result;
		}

		internal static CusWHSOperatorTransaction CreateReceipt(BusinessObjectFactory factory, ZAWhsDataTestHelper whsHelper, CusWHSOperatorTransactionBatch batch, ZDateTime transactionDate, int batchLineNo, decimal quantity, decimal totalValue, string origin, bool isCustomsControlled)
		{
			var result = factory.New<CusWHSOperatorTransaction>();
			result.WOT_BatchLineNo = batchLineNo;
			result.WOT_ExportType = ZString.Empty;
			result.WOT_IsCustomsControlled = isCustomsControlled;
			result.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			result.WOT_OP_Product = whsHelper.Part.PK;
			result.WOT_OwnerReference = "OwnerReference";
			result.WOT_Quantity = quantity;
			result.WOT_RN_NKOrigin = origin;
			result.WOT_RX_NKCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			result.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			result.WOT_TotalValue = totalValue;
			result.WOT_TransactionDate = transactionDate.Date;
			result.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.REC;
			result.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			result.WOT_CustomsEntryNumber = "EN00124";
			return result;
		}

		internal static CusWHSOperatorTransactionLine CreateTransactionLine(BusinessObjectFactory factory, CusWHSOperatorTransaction order, CusWHSOperatorTransaction receipt, decimal quantity)
		{
			var transactionLine = factory.New<CusWHSOperatorTransactionLine>();
			transactionLine.WOL_Quantity = quantity;
			transactionLine.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			transactionLine.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			return transactionLine;
		}
	}
}
