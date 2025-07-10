using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	class ExportBelnOperationalActionRunnerTest : ExportOperationalActionRunnerTest
	{
		public void TestRunner_BELN()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 30m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "ExpRef001", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "BlnRef001", 10m);

			CreateReceipt("Receipt01", 10m, customsEntryNumber: "EN00123-1", isCustomsControlled: true);
			CreateReceipt("Receipt02", 3m, "", isCustomsControlled: false, CountryCodes.SouthAfrica);
			CreateReceipt("Receipt03", 1m, "", isCustomsControlled: false, CountryCodes.UnitedKingdom);

			Factory.Save();

			var runner = new ExportBelnOperationalActionRunner(log, Factory, () => false);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "BlnRef001"
				}
			};
			runner.Run(selections);

			var shipments = LoadShipmentsByLoggedMessageNumbers();
			AssertNotNull("LoadShipmentsByLoggedMessageNumbers()", shipments);
			AssertEquals("Count of shipment messages", 2, shipments.Count);

			var shipment = shipments[0];

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLineCollection.Count", 3, invoice1.CommercialInvoiceLineCollection.Count);
			AssertEquals("EntryInstructionCollection.Count", 1, shipment.EntryInstructionCollection.Count);

			var invoiceLine1 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 6m);
			AssertNotNull("Find invoice line with quantity 6", invoiceLine1);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 6m, invoiceLine1.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine1", shipment, invoiceLine1.EntryInstructionLink, "Export of Goods", "NON", "61", null);

			var invoiceLine2 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 3m);
			AssertNotNull("Find invoice line with quantity 3", invoiceLine2);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine2/BondedWarehouseQuantity", null, invoiceLine2.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine2", shipment, invoiceLine2.EntryInstructionLink, "Export of Goods", "NON", "61", null);

			var invoiceLine3 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 1m);
			AssertNotNull("Find invoice line with quantity 1", invoiceLine3);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine3/BondedWarehouseQuantity", null, invoiceLine3.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine3", shipment, invoiceLine3.EntryInstructionLink, "Export of Goods", "NON", "61", null);

			var exbond = shipments[1];

			AssertNotNull("Exbond:CommercialInfo", exbond.CommercialInfo);
			AssertNotNull("Exbond:CommercialInfo/CommercialInvoiceCollection", exbond.CommercialInfo.CommercialInvoiceCollection);
			var exbondInvoice1 = exbond.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("Exbond:CommercialInfo/CommercialInvoiceCollection", exbondInvoice1.CommercialInvoiceLineCollection);
			AssertEquals("Exbond:CommercialInvoice1/CommercialInvoiceLineCollection.Count", 1, exbondInvoice1.CommercialInvoiceLineCollection.Count);
			AssertEquals("Exbond:EntryInstructionCollection.Count", 1, exbond.EntryInstructionCollection.Count);

			var exbondLine1 = exbondInvoice1.CommercialInvoiceLineCollection[0];
			AssertNotNull("Exbond:CommercialInvoice1/CommercialInvoiceLine1", exbondLine1);
			AssertEquals("Exbond:CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 6m, exbondLine1.BondedWarehouseQuantity);
			AssertEntryInstruction("Exbond:CommercialInvoice1/CommercialInvoiceLine1", exbond, exbondLine1.EntryInstructionLink, "Goods from Bonded warehouse", "NON", "11", warehouse);
		}

		public void TestRunner_BELN_LocalOnly()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 30m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "ExpRef001", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "BlnRef001", 10m);
			CreateReceipt("Receipt", 20m, isCustomsControlled: false);

			Factory.Save();

			var runner = new ExportBelnOperationalActionRunner(log, Factory, () => false);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "BlnRef001"
				}
			};
			runner.Run(selections);

			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			var invoiceLine = invoice1.CommercialInvoiceLineCollection[0];
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", null, invoiceLine.BondedWarehouseQuantity);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/InvoiceQuantity", 10m, invoiceLine.InvoiceQuantity);
		}

		public void TestRunner_WithInsufficientQuantities_Cancel()
		{
			TestRunner_WithInsufficientQuantities(new ExportBelnOperationalActionRunner(log, Factory, () => false), WarehouseOperatorTransactionExportTypeList.Codes.BLN, willBeConfirmed: false);
		}

		public void TestRunner_WithInsufficientQuantities_Confirm()
		{
			TestRunner_WithInsufficientQuantities(new ExportBelnOperationalActionRunner(log, Factory, () => true), WarehouseOperatorTransactionExportTypeList.Codes.BLN, willBeConfirmed: true);
		}
	}
}
