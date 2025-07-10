using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	sealed class ExWarehouseOperationalActionRunnerTest : ExportOperationalActionRunnerTest
	{
		public void TestRunner_ExWarehouse()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);
			CreateReceipt("Receipt01", 30m, customsEntryNumber: "EN00123-1");
			Factory.Save();

			var runner = new ExWarehouseOperationalActionRunner(log, Factory);
			runner.Run(new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
				}
			});
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			var invoiceLine = invoice1.CommercialInvoiceLineCollection[0];
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 20m, invoiceLine.BondedWarehouseQuantity);
			AssertNotNull("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantityUnit", invoiceLine.BondedWarehouseQuantityUnit);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantityUnit/Code", "PKT", invoiceLine.BondedWarehouseQuantityUnit.Code);
		}

		public void TestRunner_ProductOwner()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);
			CreateReceipt("Receipt01", 30m, customsEntryNumber: "EN00123-1");

			var otherOwner = Factory.NewWithValidTestData<OrgHeader>();
			var otherPart = whsHelper.CreateProduct(otherOwner.PK, "~~1");
			var receive = whsHelper.GetNewWhsReceive(warehouse.PK, otherOwner.PK, "RCV1", middayYesterdayUtc.ToOffset());
			var inventory = whsHelper.GetNewReceiveInventory(receive, otherPart, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			whsHelper.GetNewWhsBondedWarehouseAttribute(inventory.PK, 300m, 0m, "", 0m, "", 0m, "", Core.Constants.CountryCodes.SouthAfrica, 90m, "PKT", "", "EN00123-1", 1);
			var otherOrder = CreateOrder(ZString.Empty, "OwnRefX01", 99m);
			otherOrder.WOT_OP_Product = otherPart.PK;
			otherOrder.WOT_OH_ProductOwner = otherOwner.PK;
			var otherReceipt = CreateReceipt("Receipt02", 30m, customsEntryNumber: "EN00123-1");
			otherReceipt.WOT_OP_Product = otherPart.PK;
			otherReceipt.WOT_OH_ProductOwner = otherOwner.PK;

			Factory.Save();

			var runner = new ExWarehouseOperationalActionRunner(log, Factory);
			runner.Run(new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
				}
			});
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, invoice1.CommercialInvoiceLineCollection.Count);
			var invoiceLine = invoice1.CommercialInvoiceLineCollection[0];
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 20m, invoiceLine.BondedWarehouseQuantity);
		}

		public void TestRunner_ByApplicatorWithEmptyGridSelection()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);
			CreateReceipt("Receipt01", 30m, customsEntryNumber: "EN00123-1");
			Factory.Save();

			var applicator = new ExWarehouseApplicator(Factory, () => true, (_) => { });
			applicator.Apply(log, Array.Empty<CusWHSOperatorTransaction>());
			AssertContains("No records available, please check filters and fetch before trying to run this action.", log.MessagesString());
		}

		public void TestRunner_ExWarehouse_Empty()
		{
			var log = new DummyOperationalActionSectionLog();
			var runner = new ExWarehouseOperationalActionRunner(log, Factory);
			runner.Run(new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = ZGuid.NewZGuid(),
					ProductOwner = ZGuid.NewZGuid(),
				}
			});
			AssertContains("No Bond store records affected", log.MessagesString());
		}

		public void TestRunner_ExWarehouse_WhenOnlyNonBondedGoodsAreUsed()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);
			CreateReceipt("Receipt01", 30m, isCustomsControlled: false);
			Factory.Save();

			var runner = new ExWarehouseOperationalActionRunner(log, Factory);
			runner.Run(new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
				}
			});

			AssertContains("No Bond store records affected", log.MessagesString());
		}

		public void TestRunner_ExWarehouse_MultipleMessages()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateOrder(ZString.Empty, "OwnRef001", 20m);
			CreateReceipt("Receipt01", 60m, customsEntryNumber: "EN00123-1");
			Factory.Save();

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2))
			{
				var runner = new ExWarehouseOperationalActionRunner(log, Factory);
				runner.Run(new List<OperatorTransactionSelection>
				{
					new OperatorTransactionSelection
					{
						WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
						ProductOwner = whsHelper.Importer.PK,
					}
				});
			}
			var shipments = LoadShipmentsByLoggedMessageNumbers();
			AssertEquals("shipments.Count", 2, shipments.Count);
		}
	}
}
