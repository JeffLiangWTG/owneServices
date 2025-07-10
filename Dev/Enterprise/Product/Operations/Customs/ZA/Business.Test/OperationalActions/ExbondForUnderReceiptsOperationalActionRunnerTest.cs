using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	class ExbondForUnderReceiptsOperationalActionRunnerTest : ExportOperationalActionRunnerTest
	{
		public void TestRunner()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);
			var transaction = CreateOperatorTransaction(WarehouseOperatorTransactionTypeList.Codes.ADJ, ZString.Empty, "OwnRef004", 40m, "EN00123-1");
			Factory.Save();

			var runner = new ExbondForUnderReceiptsOperationalActionRunner(log, Factory);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					TransactionType = transaction.WOT_TransactionType,
					TransactionDate = transaction.WOT_TransactionDate,
					OwnerReference = transaction.WOT_OwnerReference,
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = transaction.WOT_OH_ProductOwner,
					Batch = batch.PK,
				}
			};
			runner.Run(selections);
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			var invoiceLine = invoice1.CommercialInvoiceLineCollection[0];
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 40m, invoiceLine.BondedWarehouseQuantity);
			AssertNotNull("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantityUnit", invoiceLine.BondedWarehouseQuantityUnit);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantityUnit/Code", "PKT", invoiceLine.BondedWarehouseQuantityUnit.Code);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/DataImportMatchingKey", transaction.PK.ToString(), invoiceLine.DataImportMatchingKey);

			transaction.Reload();
			AssertEquals(WarehouseOperatorTransactionStatusList.Codes.CLS, transaction.WOT_Status);
		}
	}
}
