using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	class ExportNonBelnOperationalActionRunnerTest : ExportOperationalActionRunnerTest
	{
		public void TestRunner_NonBELN()
		{
			CreateOrder(ZString.Empty, "OwnRef001", 30m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef002", 15m);
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.BLN, "OwnRef003", 10m);

			CreateReceipt("Receipt01", 6m, customsEntryNumber: "EN00123-1", isCustomsControlled: true);
			CreateReceipt("Receipt02", 5m, "", isCustomsControlled: false, CountryCodes.SouthAfrica);
			CreateReceipt("Receipt03", 4m, "", isCustomsControlled: false, CountryCodes.UnitedKingdom);

			Factory.Save();

			var runner = new ExportNonBelnOperationalActionRunner(log, Factory, () => false);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "OwnRef002"
				},
			};
			runner.Run(selections);
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLineCollection.Count", 3, invoice1.CommercialInvoiceLineCollection.Count);
			AssertEquals("EntryInstructionCollection.Count", 3, shipment.EntryInstructionCollection.Count);

			var invoiceLine1 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 6m);
			AssertNotNull("Find invoice line with quantity 6", invoiceLine1);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine1/BondedWarehouseQuantity", 6m, invoiceLine1.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine1", shipment, invoiceLine1.EntryInstructionLink, "Goods from Bonded warehouse", "NON", "67", warehouse);

			var invoiceLine2 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 5m);
			AssertNotNull("Find invoice line with quantity 5", invoiceLine2);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine2/BondedWarehouseQuantity", null, invoiceLine2.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine2", shipment, invoiceLine2.EntryInstructionLink, "Local Goods", "NON", "60", null);

			var invoiceLine3 = invoice1.CommercialInvoiceLineCollection.FirstOrDefault(x => x.InvoiceQuantity == 4m);
			AssertNotNull("Find invoice line with quantity 4", invoiceLine3);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLine3/BondedWarehouseQuantity", null, invoiceLine3.BondedWarehouseQuantity);
			AssertEntryInstruction("CommercialInvoiceLine3", shipment, invoiceLine3.EntryInstructionLink, "Imported Goods", "NON", "60", null);
		}

		public void TestRun_ReceiptOrder()
		{
			CreateOrder(WarehouseOperatorTransactionExportTypeList.Codes.EXP, "OwnRef001", 15m);

			CreateReceipt("Receipt01", 5m, "", isCustomsControlled: false, CountryCodes.UnitedKingdom);
			CreateReceipt("Receipt02", 15m, customsEntryNumber: "EN00123-1", isCustomsControlled: true);
			CreateReceipt("Receipt03", 5m, "", isCustomsControlled: false, CountryCodes.SouthAfrica);

			Factory.Save();

			var runner = new ExportNonBelnOperationalActionRunner(log, Factory, () => false);
			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "OwnRef001"
				},
			};
			runner.Run(selections);
			var shipment = LoadShipmentByLoggedMessageNumber();

			AssertNotNull("CommercialInfo", shipment.CommercialInfo);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", shipment.CommercialInfo.CommercialInvoiceCollection);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", invoice1.CommercialInvoiceLineCollection);
			AssertEquals("CommercialInvoice1/CommercialInvoiceLineCollection.Count", 1, invoice1.CommercialInvoiceLineCollection.Count);
			AssertEntryInstruction("CommercialInvoiceLine1", shipment, invoice1.CommercialInvoiceLineCollection[0].EntryInstructionLink, "Goods from Bonded warehouse", "NON", "67", warehouse);
		}

		public void TestRunner_WithInsufficientQuantities_Cancel()
		{
			TestRunner_WithInsufficientQuantities(new ExportNonBelnOperationalActionRunner(log, Factory, () => false), WarehouseOperatorTransactionExportTypeList.Codes.EXP, willBeConfirmed: false);
		}

		public void TestRunner_WithInsufficientQuantities_Confirm()
		{
			TestRunner_WithInsufficientQuantities(new ExportNonBelnOperationalActionRunner(log, Factory, () => true), WarehouseOperatorTransactionExportTypeList.Codes.EXP, willBeConfirmed: true);
		}
	}
}
