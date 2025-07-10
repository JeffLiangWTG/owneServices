using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.WarehouseIntegration;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.Business.Testing
{
	class ExbondShipmentUsxmlTest : TestCaseWithFactory
	{
		public void TestGetUsxmlForWarehouseTransactions()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>
			{
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "SE",
					Quantity = 1,
					ProductCode = "PROD001",
					PriceExbond = 1234.56m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = "ABC123",
					MRNLine = 1,
					CustomsQuantity = 11,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 1,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = "06bf895d-caed-4f63-811a-8ea527014f51"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference2",
					Currency = "GBP",
					InvoiceDate = today.AddDays(-1),
					CountryOfOrigin = "GB",
					Quantity = 2,
					ProductCode = "PROD002",
					PriceExbond = 789.98m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345677",
					MRN = "ABC124",
					MRNLine = 1,
					CustomsQuantity = 22,
					AdditionalQty1 = 1,
					AdditionalQty2 = 2,
					CountableQty = 1,
					CountableUom = "BOX",
					PreviousProcedure = "40",
					DataImportMatchingKey = "3bc5f39f-9094-408d-8927-42bf8288145b"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today.AddDays(-2),
					CountryOfOrigin = "FI",
					Quantity = 3,
					ProductCode = "PROD003",
					PriceExbond = 123.45m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345679",
					MRN = "ABC123",
					MRNLine = 2,
					CustomsQuantity = 33,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 12,
					CountableUom = "KG",
					PreviousProcedure = "01",
					DataImportMatchingKey = "bff9eecc-536f-4f79-b101-62bb20ed5a6b"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = false
				}
			};

			var mode = ExbondShipmentUsxml.Mode.ExWarehouse;
			var shipment = CreateMessageAndGetShipment(testData, mode, 1)[0];

			AssertDataTargets(shipment, new[] { "CustomsDeclaration" });
			AssertCommonCompanyBranchDepartment(shipment);
			AssertCommercialInfo(shipment.CommercialInfo, 2);

			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference1", 1234.56m + 123.45m, "ZAR", today.AddDays(-2), 2);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice1/CommercialInvoiceLine1/", invoice1.CommercialInvoiceLineCollection[0],
				1, 1, "11", testData[0]);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice1/CommercialInvoiceLine2/", invoice1.CommercialInvoiceLineCollection[1],
				2, 1, "11", testData[2]);

			var invoice2 = shipment.CommercialInfo.CommercialInvoiceCollection[1];
			AssertCommercialInvoice("CommercialInvoice2/", invoice2, "OwnerReference2", 789.98m, "GBP", today.AddDays(-1), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice2/CommercialInvoiceLine1/", invoice2.CommercialInvoiceLineCollection[0],
				1, 1, "11", testData[1]);

			AssertMiscellaneousFields(shipment, "EXW");
			AssertEntryInstructionCount(shipment, 1);
			AssertEntryInstruction(shipment.EntryInstructionCollection[0], "Goods from Bonded warehouse", 1, "11", "Warehouse1", "4 NEW RD", "GLECOR_ZA");

			AssertOrganizationAddress(string.Empty, shipment.OrganizationAddressCollection, "ConsigneeDocumentaryAddress", "3 NEW RD", "GLECOR_ZA");
		}

		public void TestGetUsxmlForWarehouseTransactions_TwoWarehouses()
		{
			var warehouseAddress2 = ownerOrg.Addresses.AddNew();
			warehouseAddress2.OA_Address1 = "5 NEW RD";
			Factory.Save();

			var testData = new List<ExbondEntryLine>
			{
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					CountryOfOrigin = "SE",
					Quantity = 1,
					ProductCode = "PROD001",
					Price = 6543.21m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = "ABC123",
					MRNLine = 1,
					CustomsQuantity = 11,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 1,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = "28bbdc63-d85c-4386-81b8-dae3b014c1a4"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					CountryOfOrigin = "GB",
					Quantity = 2,
					ProductCode = "PROD001",
					Price = 567.89m,
					Warehouse = warehouseAddress2.PK,
					TariffCode = "12345677",
					MRN = "ABC124",
					MRNLine = 1,
					CustomsQuantity = 22,
					AdditionalQty1 = 1,
					AdditionalQty2 = 2,
					CountableQty = 1,
					CountableUom = "BOX",
					PreviousProcedure = "40",
					DataImportMatchingKey = "6afb28e8-72fe-4654-a534-e15eb900cb5c"
				}
			};

			var shipment = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 1)[0];

			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals("CommercialInfo/CommercialInvoiceLineCollection.Count", 2, invoice1.CommercialInvoiceLineCollection.Count);

			var invoiceLine1 = invoice1.CommercialInvoiceLineCollection[0];
			var invoiceLine2 = invoice1.CommercialInvoiceLineCollection[1];
			AssertEquals("EntryInstructionLink", 1, invoiceLine1.EntryInstructionLink);
			AssertEquals("EntryInstructionLink", 2, invoiceLine2.EntryInstructionLink);

			AssertEntryInstructionCount(shipment, 2);
			AssertEntryInstruction(shipment.EntryInstructionCollection[0], "Goods from Bonded warehouse", 1, "11", "Warehouse1", "4 NEW RD", "GLECOR_ZA");
			AssertEntryInstruction(shipment.EntryInstructionCollection[1], "Goods from Bonded warehouse", 2, "11", "Warehouse1", "5 NEW RD", "GLECOR_ZA");
		}

		public void TestGetUsxmlForWarehouseTransactions_MaxNumberJobInvoiceLines_FiveInvoicesOneHeader()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>(5);
			for (var i = 0; i < 5; ++i)
			{
				testData.Add(new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "ZA",
					Quantity = 1m,
					ProductCode = "PROD001",
					PriceExbond = 111m * i,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = $"ABC123{i}",
					MRNLine = (short)i,
					CustomsQuantity = 11m * i,
					AdditionalQty1 = 0m,
					AdditionalQty2 = 0m,
					CountableQty = 1m,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = new System.Guid().ToString(),
				});
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 0))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 1);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipments[0].CommercialInfo.CommercialInvoiceCollection.Count);
				var invoice1 = shipments[0].CommercialInfo.CommercialInvoiceCollection[0];
				AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference", testData.Sum(x => x.PriceExbond), "ZAR", today, 5);
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 2))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 3);
				for (var i = 0; i < 3; ++i)
				{
					AssertEquals($"Message[{i}] CommercialInfo/CommercialInvoiceCollection.Count", 1, shipments[i].CommercialInfo.CommercialInvoiceCollection.Count);
					var invoice1 = shipments[i].CommercialInfo.CommercialInvoiceCollection[0];
					var expectedPriceSum = testData[i * 2].PriceExbond + (i == 2 ? 0 : testData[i * 2 + 1].PriceExbond);
					AssertCommercialInvoice($"Message[{i}] CommercialInvoice1/", invoice1, "OwnerReference", expectedPriceSum, "ZAR", today, i == 2 ? 1 : 2);
				}
			}
		}

		public void TestGetUsxmlForWarehouseTransactions_MaxNumberJobInvoiceLines_FiveHeaders()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>(5);
			for (var i = 0; i < 5; ++i)
			{
				testData.Add(new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = $"OwnerReference{i}",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "ZA",
					Quantity = 1m,
					ProductCode = $"PROD00{i}",
					PriceExbond = 111m * i,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = $"ABC123{i}",
					MRNLine = 1,
					CustomsQuantity = 11m * i,
					AdditionalQty1 = 0m,
					AdditionalQty2 = 0m,
					CountableQty = 1m,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = new System.Guid().ToString(),
				});
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 0))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 1);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 5, shipments[0].CommercialInfo.CommercialInvoiceCollection.Count);
				for (var i = 0; i < 5; ++i)
				{
					AssertCommercialInvoice($"CommercialInvoice{i + 1}/", shipments[0].CommercialInfo.CommercialInvoiceCollection[i], testData[i].OwnerReference, testData[i].PriceExbond, "ZAR", today, 1);
				}
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 2))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 3);
				var j = 0;
				for (var i = 0; i < 3; ++i)
				{
					AssertEquals($"Message[{i}] CommercialInfo/CommercialInvoiceCollection.Count", i == 2 ? 1 : 2, shipments[i].CommercialInfo.CommercialInvoiceCollection.Count);
					AssertCommercialInvoice($"Message[{i}] CommercialInvoice1/", shipments[i].CommercialInfo.CommercialInvoiceCollection[0], $"OwnerReference{j}", testData[j].PriceExbond, "ZAR", today, 1);
					++j;
					if (i < 2)
					{
						AssertCommercialInvoice($"Message[{i}] CommercialInvoice2/", shipments[i].CommercialInfo.CommercialInvoiceCollection[1], $"OwnerReference{j}", testData[j].PriceExbond, "ZAR", today, 1);
						++j;
					}
				}
			}
		}

		public void TestGetUsxmlForWarehouseTransactions_MaxNumberJobInvoiceLines_SplitHeaders()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>(5);
			for (var i = 0; i < 5; ++i)
			{
				testData.Add(new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = $"OwnerReference{(i < 3 ? 1 : 2)}",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "ZA",
					Quantity = 1m,
					ProductCode = $"PROD001",
					PriceExbond = 111m * i,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = $"ABC123{i}",
					MRNLine = (short)(i < 3 ? i : i - 3),
					CustomsQuantity = 11m * i,
					AdditionalQty1 = 0m,
					AdditionalQty2 = 0m,
					CountableQty = 1m,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = new System.Guid().ToString(),
				});
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 0))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 1);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 2, shipments[0].CommercialInfo.CommercialInvoiceCollection.Count);
				AssertCommercialInvoice("CommercialInvoice1/", shipments[0].CommercialInfo.CommercialInvoiceCollection[0], "OwnerReference1", testData[0].PriceExbond + testData[1].PriceExbond + testData[2].PriceExbond, "ZAR", today, 3);
				AssertCommercialInvoice("CommercialInvoice2/", shipments[0].CommercialInfo.CommercialInvoiceCollection[1], "OwnerReference2", testData[3].PriceExbond + testData[4].PriceExbond, "ZAR", today, 2);
			}

			using (ZACustomsRegistry.Instance.MaxNumberLinesPerHomeConsumptionFile.SetTemporaryValue(Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty, 2))
			{
				var shipments = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.ExWarehouse, 3);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipments[0].CommercialInfo.CommercialInvoiceCollection.Count);
				AssertCommercialInvoice("CommercialInvoice1/", shipments[0].CommercialInfo.CommercialInvoiceCollection[0], "OwnerReference1", testData[0].PriceExbond + testData[1].PriceExbond, "ZAR", today, 2);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 2, shipments[1].CommercialInfo.CommercialInvoiceCollection.Count);
				AssertCommercialInvoice("CommercialInvoice1/", shipments[1].CommercialInfo.CommercialInvoiceCollection[0], "OwnerReference1", testData[2].PriceExbond, "ZAR", today, 1);
				AssertCommercialInvoice("CommercialInvoice2/", shipments[1].CommercialInfo.CommercialInvoiceCollection[1], "OwnerReference2", testData[3].PriceExbond, "ZAR", today, 1);
				AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", 1, shipments[2].CommercialInfo.CommercialInvoiceCollection.Count);
				AssertCommercialInvoice("CommercialInvoice1/", shipments[2].CommercialInfo.CommercialInvoiceCollection[0], "OwnerReference2", testData[4].PriceExbond, "ZAR", today, 1);
			}
		}

		public void TestGetUsxmlForNonBeln()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>
			{
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "SE",
					Quantity = 1,
					ProductCode = "PROD001",
					Price = 1234.56m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = "ABC123",
					MRNLine = 1,
					CustomsQuantity = 11,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 1,
					CountableUom = "PLT",
					PreviousProcedure = "40",
					DataImportMatchingKey = "472d2f03-bc91-433c-adcb-a2f918bc87c4"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = false,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference2",
					Currency = "GBP",
					InvoiceDate = today.AddDays(-1),
					CountryOfOrigin = "ZA",
					Quantity = 2,
					ProductCode = "PROD002",
					Price = 789.98m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345677",
					MRN = "ABC124",
					MRNLine = 1,
					AdditionalQty1 = 1,
					AdditionalQty2 = 2,
					CountableQty = 1,
					CountableUom = "BOX",
					DataImportMatchingKey = "e17bf7e3-da52-4597-9344-8e9f7b2465d4"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = false,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference3",
					Currency = "ZAR",
					InvoiceDate = today.AddDays(-2),
					CountryOfOrigin = "FI",
					Quantity = 3,
					ProductCode = "PROD003",
					Price = 123.45m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345679",
					MRN = "ABC123",
					MRNLine = 2,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 12,
					CountableUom = "KG",
					DataImportMatchingKey = "b04a4e73-7a21-47d6-bafd-71addc2bb75b"
				}
			};

			var mode = ExbondShipmentUsxml.Mode.NonBeln;
			var shipment = CreateMessageAndGetShipment(testData, mode, 1)[0];
			AssertDataTargets(shipment, new[] { "ForwardingShipment", "CustomsDeclaration" });
			AssertCommonCompanyBranchDepartment(shipment);
			AssertCommercialInfo(shipment.CommercialInfo, 3);

			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference1", 1234.56m, "ZAR", today, 1);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice1/CommercialInvoiceLine1/", invoice1.CommercialInvoiceLineCollection[0],
				1, 1, "67", testData[0]);

			var invoice2 = shipment.CommercialInfo.CommercialInvoiceCollection[1];
			AssertCommercialInvoice("CommercialInvoice2/", invoice2, "OwnerReference2", 789.98m, "GBP", today.AddDays(-1), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice2/CommercialInvoiceLine1/", invoice2.CommercialInvoiceLineCollection[0],
				1, 2, "60", testData[1]);

			var invoice3 = shipment.CommercialInfo.CommercialInvoiceCollection[2];
			AssertCommercialInvoice("CommercialInvoice3/", invoice3, "OwnerReference3", 123.45m, "ZAR", today.AddDays(-2), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice3/CommercialInvoiceLine1/", invoice3.CommercialInvoiceLineCollection[0],
				1, 3, "60", testData[2]);

			AssertMiscellaneousFields(shipment, "EXP");
			AssertEntryInstructionCount(shipment, 3);
			AssertEntryInstruction(shipment.EntryInstructionCollection[0], "Goods from Bonded warehouse", 1, "67", "Warehouse1", "4 NEW RD", "GLECOR_ZA");
			AssertEntryInstruction(shipment.EntryInstructionCollection[1], "Local Goods", 2, "60");
			AssertEntryInstruction(shipment.EntryInstructionCollection[2], "Imported Goods", 3, "60");

			AssertOrganizationAddress(string.Empty, shipment.OrganizationAddressCollection, "ConsignorDocumentaryAddress", "3 NEW RD", "GLECOR_ZA");
		}

		public void TestGetUsxmlForBeln()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>
			{
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "SE",
					Quantity = 1,
					ProductCode = "PROD001",
					Price = 1234.56m,
					PriceExbond = 1000m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = "ABC123",
					MRNLine = 1,
					CustomsQuantity = 11,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 1,
					CountableUom = "PLT",
					PreviousProcedure = "00",
					DataImportMatchingKey = "5cf3fec1-2a8b-47aa-a700-9436c3105e10"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = false,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference2",
					Currency = "GBP",
					CountryOfOrigin = "ZA",
					InvoiceDate = today.AddDays(-1),
					Quantity = 2,
					ProductCode = "PROD002",
					Price = 789.98m,
					PriceExbond = 0m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345677",
					MRN = "ABC124",
					MRNLine = 1,
					AdditionalQty1 = 1,
					AdditionalQty2 = 2,
					CountableQty = 1,
					CountableUom = "BOX",
					PreviousProcedure = "40",
					DataImportMatchingKey = "5ae07290-66ab-443e-af2b-05425939f031"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = false,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference3",
					Currency = "ZAR",
					CountryOfOrigin = "FI",
					InvoiceDate = today.AddDays(-2),
					Quantity = 3,
					ProductCode = "PROD003",
					Price = 123.45m,
					PriceExbond = 0m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345679",
					MRN = "ABC123",
					MRNLine = 2,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 12,
					CountableUom = "KG",
					PreviousProcedure = "01",
					DataImportMatchingKey = "03ee3305-0f90-4ce1-96e2-f4c954b20e5d"
				}
			};

			var shipmentExport = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.BelnShipment, 1)[0];
			var shipmentExbond = CreateMessageAndGetShipment(testData, ExbondShipmentUsxml.Mode.BelnExbond, 1)[0];

			AssertDataTargets(shipmentExport, new[] { "ForwardingShipment", "CustomsDeclaration" });
			AssertCommonCompanyBranchDepartment(shipmentExport);
			AssertCommercialInfo(shipmentExport.CommercialInfo, 3);

			var invoice1 = shipmentExport.CommercialInfo.CommercialInvoiceCollection[0];
			AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference1", 1234.56m, "ZAR", today, 1);
			AssertCommercialInvoiceLineFromExbondShipmentLineExbondExport("CommercialInvoice1/CommercialInvoiceLine1/", invoice1.CommercialInvoiceLineCollection[0],
				1, 1, "61", testData[0]);

			var invoice2 = shipmentExport.CommercialInfo.CommercialInvoiceCollection[1];
			AssertCommercialInvoice("CommercialInvoice2/", invoice2, "OwnerReference2", 789.98m, "GBP", today.AddDays(-1), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLineExbondExport("CommercialInvoice2/CommercialInvoiceLine1/", invoice2.CommercialInvoiceLineCollection[0],
				1, 1, "61", testData[1]);

			var invoice3 = shipmentExport.CommercialInfo.CommercialInvoiceCollection[2];
			AssertCommercialInvoice("CommercialInvoice3/", invoice3, "OwnerReference3", 123.45m, "ZAR", today.AddDays(-2), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLineExbondExport("CommercialInvoice3/CommercialInvoiceLine1/", invoice3.CommercialInvoiceLineCollection[0],
				1, 1, "61", testData[2]);

			AssertMiscellaneousFields(shipmentExport, "EXP");
			AssertEntryInstructionCount(shipmentExport, 1);
			AssertEntryInstruction(shipmentExport.EntryInstructionCollection[0], "Export of Goods", 1, "61");

			AssertOrganizationAddress(string.Empty, shipmentExport.OrganizationAddressCollection, "ConsignorDocumentaryAddress", "3 NEW RD", "GLECOR_ZA");

			AssertDataTargets(shipmentExbond, new[] { "CustomsDeclaration" });
			AssertCommonCompanyBranchDepartment(shipmentExbond);
			AssertCommercialInfo(shipmentExbond.CommercialInfo, 1);

			invoice1 = shipmentExbond.CommercialInfo.CommercialInvoiceCollection[0];
			AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference1", 1000m, "ZAR", today, 1);
			AssertCommercialInvoiceLineFromExbondShipmentLineExbondExbond("CommercialInvoice1/CommercialInvoiceLine1/", invoice1.CommercialInvoiceLineCollection[0],
				1, 1, "11", testData[0]);

			AssertMiscellaneousFields(shipmentExbond, "EXW");
			AssertEntryInstructionCount(shipmentExbond, 1);
			AssertEntryInstruction(shipmentExbond.EntryInstructionCollection[0], "Goods from Bonded warehouse", 1, "11", "Warehouse1", "4 NEW RD", "GLECOR_ZA");

			AssertOrganizationAddress(string.Empty, shipmentExbond.OrganizationAddressCollection, "ConsigneeDocumentaryAddress", "3 NEW RD", "GLECOR_ZA");
		}

		public void TestGetUsxmlForUnderReceipts()
		{
			var today = ZDate.Today;
			var testData = new List<ExbondEntryLine>
			{
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today,
					CountryOfOrigin = "SE",
					Quantity = 1,
					ProductCode = "PROD001",
					PriceExbond = 1234.56m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345678",
					MRN = "ABC123",
					MRNLine = 1,
					CustomsQuantity = 11,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 1,
					CountableUom = "PLT",
					PreviousProcedure = "00"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference2",
					Currency = "GBP",
					InvoiceDate = today.AddDays(-1),
					CountryOfOrigin = "GB",
					Quantity = 2,
					ProductCode = "PROD002",
					PriceExbond = 789.98m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345677",
					MRN = "ABC124",
					MRNLine = 1,
					CustomsQuantity = 22,
					AdditionalQty1 = 1,
					AdditionalQty2 = 2,
					CountableQty = 1,
					CountableUom = "BOX",
					PreviousProcedure = "40"
				},
				new ExbondEntryLine
				{
					IsCustomsControlled = true,
					Owner = ownerOrg.PK,
					OwnerReference = "OwnerReference1",
					Currency = "ZAR",
					InvoiceDate = today.AddDays(-2),
					CountryOfOrigin = "FI",
					Quantity = 3,
					ProductCode = "PROD003",
					PriceExbond = 123.45m,
					Warehouse = batchWarehouseAddress.PK,
					TariffCode = "12345679",
					MRN = "ABC123",
					MRNLine = 2,
					CustomsQuantity = 33,
					AdditionalQty1 = 0,
					AdditionalQty2 = 0,
					CountableQty = 12,
					CountableUom = "KG",
					PreviousProcedure = "01"
				}
			};

			var mode = ExbondShipmentUsxml.Mode.UnderReceipts;
			var shipment = CreateMessageAndGetShipment(testData, mode, 1)[0];

			AssertDataTargets(shipment, new[] { "CustomsDeclaration" });
			AssertCommonCompanyBranchDepartment(shipment);
			AssertCommercialInfo(shipment.CommercialInfo, 2);

			var invoice1 = shipment.CommercialInfo.CommercialInvoiceCollection[0];
			AssertCommercialInvoice("CommercialInvoice1/", invoice1, "OwnerReference1", 1234.56m + 123.45m, "ZAR", today.AddDays(-2), 2);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice1/CommercialInvoiceLine1/", invoice1.CommercialInvoiceLineCollection[0],
				1, 1, "11", testData[0]);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice1/CommercialInvoiceLine2/", invoice1.CommercialInvoiceLineCollection[1],
				2, 1, "11", testData[2]);

			var invoice2 = shipment.CommercialInfo.CommercialInvoiceCollection[1];
			AssertCommercialInvoice("CommercialInvoice2/", invoice2, "OwnerReference2", 789.98m, "GBP", today.AddDays(-1), 1);
			AssertCommercialInvoiceLineFromExbondShipmentLine(mode, "CommercialInvoice2/CommercialInvoiceLine1/", invoice2.CommercialInvoiceLineCollection[0],
				1, 1, "11", testData[1]);

			AssertMiscellaneousFields(shipment, "EXW");
			AssertEntryInstructionCount(shipment, 1);
			AssertEntryInstruction(shipment.EntryInstructionCollection[0], "Goods from Bonded warehouse", 1, "11", "Warehouse1", "4 NEW RD", "GLECOR_ZA", mergeByIsNull: true);

			AssertOrganizationAddress(string.Empty, shipment.OrganizationAddressCollection, "ConsigneeDocumentaryAddress", "3 NEW RD", "GLECOR_ZA");
		}

		public void TestCustomsQuantityRounding()
		{
			var testValues = new List<(decimal value, decimal roundedValue)>
			{
				(0.001m, 0.01m),
				(0.49499m, 0.49m),
				(0.505m, 0.51m),
			};
			foreach (var (testValue, roundedTestValue) in testValues)
			{
				foreach (var (testcaseMode, testcaseIsCustomsControlled, testcaseShouldBeRounded) in RoundingTestCases)
				{
					var testData = new List<ExbondEntryLine>
					{
						new ExbondEntryLine
						{
							IsCustomsControlled = testcaseIsCustomsControlled,
							Owner = ownerOrg.PK,
							OwnerReference = "OwnerReference1",
							CustomsQuantity = testValue,
						},
					};

					var shipment = CreateMessageAndGetShipment(testData, testcaseMode, 1)[0];

					AssertCommercialInfo(shipment.CommercialInfo, 1);
					var invoice = shipment.CommercialInfo.CommercialInvoiceCollection[0];
					AssertEquals("CommercialInvoice1/CommercialInvoiceLineCollection.Count", 1, invoice.CommercialInvoiceLineCollection.Count);
					var expectedQuantity = testcaseShouldBeRounded ? roundedTestValue : testValue;
					AssertEquals($"CustomsQuantity (test case: mode {testcaseMode}, isCustomsControlled {testcaseIsCustomsControlled}, should{(testcaseIsCustomsControlled ? "" : " not")} be rounded) value {testValue}",
						expectedQuantity, invoice.CommercialInvoiceLineCollection[0].CustomsQuantity);
				}
			}
		}

		public void TestLinePriceRounding()
		{
			var testValues = new List<(decimal value, decimal roundedValue)>
			{
				(0.001m, 0m),
				(0.49499m, 0.49m),
				(0.505m, 0.51m),
			};
			foreach (var (testValue, roundedTestValue) in testValues)
			{
				foreach (var (testcaseMode, testcaseIsCustomsControlled, testcaseShouldBeRounded) in RoundingTestCases)
				{
					var testData = new List<ExbondEntryLine>
					{
						new ExbondEntryLine
						{
							IsCustomsControlled = testcaseIsCustomsControlled,
							Owner = ownerOrg.PK,
							OwnerReference = "OwnerReference1",
							Price = testValue,
							PriceExbond = testValue,
						},
					};

					var shipment = CreateMessageAndGetShipment(testData, testcaseMode, 1)[0];

					AssertCommercialInfo(shipment.CommercialInfo, 1);
					var invoice = shipment.CommercialInfo.CommercialInvoiceCollection[0];
					AssertEquals("CommercialInvoice1/CommercialInvoiceLineCollection.Count", 1, invoice.CommercialInvoiceLineCollection.Count);
					var expectedPrice = testcaseShouldBeRounded ? roundedTestValue : testValue;
					AssertEquals($"LinePrice (test case: mode {testcaseMode}, isCustomsControlled {testcaseIsCustomsControlled}, should{(testcaseIsCustomsControlled ? "" : " not")} be rounded) value {testValue}",
						expectedPrice, invoice.CommercialInvoiceLineCollection[0].LinePrice);
				}
			}
		}

		IEnumerable<(ExbondShipmentUsxml.Mode mode, bool isCustomsControlled, bool shouldBeRounded)> RoundingTestCases
		{
			get
			{
				yield return (ExbondShipmentUsxml.Mode.ExWarehouse, true, true);
				yield return (ExbondShipmentUsxml.Mode.BelnExbond, true, true);
				yield return (ExbondShipmentUsxml.Mode.UnderReceipts, true, true);
				yield return (ExbondShipmentUsxml.Mode.NonBeln, true, true);
				yield return (ExbondShipmentUsxml.Mode.NonBeln, false, false);
			}
		}

		UniversalShipment[] CreateMessageAndGetShipment(List<ExbondEntryLine> testData, ExbondShipmentUsxml.Mode mode, int expectedMessagesCount)
		{
			var messageNumbers = ExbondShipmentUsxml.CreateEDIMessage(Factory, batchWarehouseAddress.PK, testData, mode);
			AssertNotNull("CreateEDIMessage should never return null", messageNumbers);
			AssertEquals("CreateEDIMessage result length", expectedMessagesCount, messageNumbers.Length);
			Assert($"CreateEDIMessage failed to create one or more messages:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, messageNumbers)}", !messageNumbers.Any(x => x == null));

			var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNumbers)
				.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "UDM")
				.AddToFilter(EDIMessageSchema.EM_MessageType, "XDC")
				.AddToFilter(EDIMessageSchema.EM_MessageSubType, "XUS")
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "RCV")
				.AddToFilter(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
			var messages = Factory.Load<EDIMessage>(query);
			AssertContainsExactElementsInAnyOrder("Should be able to load all message(s) created by CreateEDIMessage", messageNumbers, messages.Select(x => x.EM_MessageNum));

			var shipments = new UniversalShipment[expectedMessagesCount];
			for (var i = 0; i < expectedMessagesCount; ++i)
			{
				using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(messages[i].EM_MessageData.ToUTF8())))
				{
					shipments[i] = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
					var xmlReader = ObjectFactory.Get<IXmlReader>();
					var logger = new TestErrorLogger();
					xmlReader.ReadXML(shipments[i], stream, logger);
				}
				AssertNotNull("Should be able to create a Shipment from the message text", shipments[i]);
			}

			return shipments;
		}

		void AssertDataTargets(UniversalShipment shipment, string[] targets)
		{
			var dataContext = (DataContext)shipment.DataContext;
			AssertNotNull("DataContext", dataContext);
			AssertNotNull("DataContext/DataTargetCollection", dataContext.DataTargetCollection);
			AssertEquals("DataContext/DataTargetCollection.Count", targets.Length, dataContext.DataTargetCollection.Count);
			foreach (var target in targets)
			{
				var dataTarget = dataContext.DataTargetCollection.FirstOrDefault(x => (string)x.Type == target);
				AssertNotNull($"DataContext/DataTargetCollection: DataTarget '{target}'", dataTarget);
				AssertNullOrEmpty($"DataContext/DataTargetCollection/DataTarget '{target}'/Key", dataTarget.Key);
			}
		}

		void AssertCommonCompanyBranchDepartment(UniversalShipment shipment)
		{
			var dataContext = (DataContext)shipment.DataContext;
			AssertNotNull("DataContext/Company", dataContext.Company);
			AssertEquals("DataContext/Company/Code", GlbCompany.CurrentCompany.GC_Code, dataContext.Company.Code);
			AssertEquals("DataContext/Company/Country/Code", GlbCompany.CurrentCompany.Country.RN_Code, dataContext.Company.Country.Code);
			AssertEquals("DataContext/DataProvider", GlbCompany.CurrentCompany.GetLicenceCode(), dataContext.DataProvider);
			AssertEquals("DataContext/EnterpriseID", GlbCompany.CurrentCompany.LicenceEnterpriseCode, dataContext.EnterpriseID);
			AssertNotNull("DataContext/EventBranch", dataContext.EventBranch);
			AssertEquals("DataContext/EventBranch/Code", GlbBranch.CurrentBranch.GB_Code, dataContext.EventBranch.Code);
			AssertNotNull("DataContext/EventDepartment", dataContext.EventDepartment);
			AssertEquals("DataContext/EventDepartment/Code", GlbDepartment.CurrentDepartment.GE_Code, dataContext.EventDepartment.Code);
			AssertNullOrEmpty("AdditionalTerms", shipment.AdditionalTerms);
			AssertNullOrEmpty("AgentsReference", shipment.AgentsReference);
			AssertNotNull("Branch", shipment.Branch);
			AssertEquals("Branch/Code", GlbBranch.CurrentBranch.GB_Code, shipment.Branch.Code);
		}

		void AssertCommercialInfo(CommercialInfo commercialInfo, int expectedCount)
		{
			AssertNotNull("CommercialInfo", commercialInfo);
			AssertEquals("CommercialInfo/Name", "All Invoices", commercialInfo.Name);
			AssertNotNull("CommercialInfo/CommercialInvoiceCollection", commercialInfo.CommercialInvoiceCollection);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Content", CollectionContent.Complete, commercialInfo.CommercialInvoiceCollection.Content);
			AssertEquals("CommercialInfo/CommercialInvoiceCollection.Count", expectedCount, commercialInfo.CommercialInvoiceCollection.Count);
		}

		void AssertMiscellaneousFields(UniversalShipment shipment, string expectedMessageType)
		{
			AssertNotNull("CustomsOffice", shipment.CustomsOffice);
			AssertEquals("CustomsOffice/Code", "JNB", shipment.CustomsOffice.Code);
			AssertNull("MergeBy", shipment.MergeBy);
			AssertNotNull("MessageType", shipment.MessageType);
			AssertEquals("MessageType/Code", expectedMessageType, shipment.MessageType.Code);
			AssertNotNull("MessagingApplicationCode", shipment.MessagingApplicationCode);
			AssertEquals("MessagingApplicationCode/Code", "ZAA", shipment.MessagingApplicationCode.Code);
			AssertNullOrEmpty("OwnerRef", shipment.OwnerRef);
			AssertNotNull("PaymentMethod", shipment.PaymentMethod);
			AssertEquals("PaymentMethod/Code", "BRK", shipment.PaymentMethod.Code);
			AssertNotNull("ServiceLevel", shipment.ServiceLevel);
			AssertEquals("ServiceLevel/Code", "STD", shipment.ServiceLevel.Code);
			AssertNotNull("ShipmentIncoTerm", shipment.ShipmentIncoTerm);
			AssertEquals("ShipmentIncoTerm/Code", "FOB", shipment.ShipmentIncoTerm.Code);
			AssertNotNull("TransportMode", shipment.TransportMode);
			AssertNullOrEmpty("TransportMode/Code", shipment.TransportMode.Code);
		}

		void AssertOrganizationAddress(string prefix, List<OrganizationAddress> organizationAddresses, string expectedAddressType, string expectedAddressShortCode, string expectedOrganizationCode)
		{
			AssertNotNull(prefix + "OrganizationAddressCollection", organizationAddresses);
			AssertEquals(prefix + "OrganizationAddressCollection.Count", 1, organizationAddresses.Count);
			var organizationAddress = organizationAddresses[0];
			AssertEquals(prefix + "OrganizationAddressCollection/AddressType", expectedAddressType, organizationAddress.AddressType);
			AssertEquals(prefix + "OrganizationAddressCollection/AddressShortCode", expectedAddressShortCode, organizationAddress.AddressShortCode);
			AssertEquals(prefix + "OrganizationAddressCollection/OrganizationCode", expectedOrganizationCode, organizationAddress.OrganizationCode);
		}

		void AssertCommercialInvoice(string prefix, CommercialInvoiceHeader header, string expectedInvoiceNumber, decimal expectedInvoiceAmount,
			string expectedInvoiceCurrency, ZDate expectedInvoiceDate, int expectedLinesCount)
		{
			AssertEquals(prefix + "InvoiceNumber", expectedInvoiceNumber, header.InvoiceNumber);
			AssertEquals(prefix + "InvoiceAmount", expectedInvoiceAmount, header.InvoiceAmount);
			AssertEquals(prefix + "InvoiceCurrency/Code", expectedInvoiceCurrency, header.InvoiceCurrency.Code);
			AssertEquals(prefix + "InvoiceDate", expectedInvoiceDate, header.InvoiceDate);
			AssertEquals(prefix + "IncoTerm", "FOB", header.IncoTerm.Code);
			AssertEquals(prefix + "CommercialInvoiceLineCollection.Count", expectedLinesCount, header.CommercialInvoiceLineCollection.Count);
		}

		void AssertCommercialInvoiceLine(string prefix, CommercialInvoiceLine invoiceLine, int expectedLineNo, decimal? expectedBondedWarehouseQuantity,
			string expectedBondedWarehouseQuantityUnit, string expectedCountryOfOrigin, decimal? expectedCustomsQuantity, decimal? expectedCustomsSecondQuantity,
			decimal? expectedCustomsThirdQuantity, int expectedEntryInstructionLink, string expectedHarmonisedCode, decimal expectedInvoiceQuantity,
			decimal expectedLinePrice, string expectedPartNo, short? expectedPreviousEntryLineNumber, string expectedPreviousEntryNumber, string expectedProcedure,
			string expectedDataImportMatchingKey
			)
		{
			AssertEquals(prefix + "LineNo", expectedLineNo, invoiceLine.LineNo);
			AssertEquals(prefix + "BondedWarehouseQuantity", expectedBondedWarehouseQuantity, invoiceLine.BondedWarehouseQuantity);
			if (string.IsNullOrEmpty(expectedBondedWarehouseQuantityUnit))
			{
				AssertNull(prefix + "BondedWarehouseQuantityUnit", invoiceLine.BondedWarehouseQuantityUnit);
			}
			else
			{
				AssertEquals(prefix + "BondedWarehouseQuantityUnit/Code", expectedBondedWarehouseQuantityUnit, invoiceLine.BondedWarehouseQuantityUnit?.Code);
			}
			AssertEquals(prefix + "CountryOfOrigin/Code", expectedCountryOfOrigin, invoiceLine.CountryOfOrigin.Code);
			AssertEquals(prefix + "CustomsQuantity", expectedCustomsQuantity, invoiceLine.CustomsQuantity);
			AssertEquals(prefix + "CustomsSecondQuantity", expectedCustomsSecondQuantity, invoiceLine.CustomsSecondQuantity);
			AssertEquals(prefix + "CustomsThirdQuantity", expectedCustomsThirdQuantity, invoiceLine.CustomsThirdQuantity);
			AssertEquals(prefix + "EntryInstructionLink", expectedEntryInstructionLink, invoiceLine.EntryInstructionLink);
			AssertEquals(prefix + "HarmonisedCode", expectedHarmonisedCode, invoiceLine.HarmonisedCode);
			AssertEquals(prefix + "InvoiceQuantity", expectedInvoiceQuantity, invoiceLine.InvoiceQuantity);
			AssertEquals(prefix + "LinePrice", expectedLinePrice, invoiceLine.LinePrice);
			AssertEquals(prefix + "PartNo", expectedPartNo, invoiceLine.PartNo);
			AssertEquals(prefix + "PreviousEntryLineNumber", expectedPreviousEntryLineNumber, invoiceLine.PreviousEntryLineNumber);
			AssertEquals(prefix + "PreviousEntryNumber", expectedPreviousEntryNumber, invoiceLine.PreviousEntryNumber);
			AssertNullOrEmpty("PrimaryPreference", invoiceLine.PrimaryPreference);
			AssertEquals(prefix + "Procedure", expectedProcedure, invoiceLine.Procedure);
			AssertEquals(prefix + "TaxType/Code", "VAT", invoiceLine.TaxType.Code);
			AssertEquals(prefix + "DataImportMatchingKey", expectedDataImportMatchingKey, invoiceLine.DataImportMatchingKey);
		}

		void AssertCommercialInvoiceLineFromExbondShipmentLine(ExbondShipmentUsxml.Mode mode, string prefix, CommercialInvoiceLine invoiceLine, int expectedLineNo,
			int expectedEntryInstructionLink, string expectedEntryStyle, ExbondEntryLine shipmentLine)
		{
			var price = mode == ExbondShipmentUsxml.Mode.ExWarehouse || mode == ExbondShipmentUsxml.Mode.BelnExbond || mode == ExbondShipmentUsxml.Mode.UnderReceipts ? shipmentLine.PriceExbond : shipmentLine.Price;
			var quantityUnit = shipmentLine.IsCustomsControlled ? shipmentLine.CountableUom : null;
			AssertCommercialInvoiceLine(prefix, invoiceLine, expectedLineNo, mode == ExbondShipmentUsxml.Mode.UnderReceipts ? shipmentLine.Quantity : shipmentLine.CountableQty,
				quantityUnit, shipmentLine.CountryOfOrigin, shipmentLine.CustomsQuantity, shipmentLine.AdditionalQty1, shipmentLine.AdditionalQty2,
				expectedEntryInstructionLink, shipmentLine.TariffCode, shipmentLine.Quantity,
				price, shipmentLine.ProductCode,
				shipmentLine.IsCustomsControlled ? shipmentLine.MRNLine : null, shipmentLine.IsCustomsControlled ? shipmentLine.MRN : null,
				shipmentLine.IsCustomsControlled ? expectedEntryStyle + (shipmentLine.PreviousProcedure ?? "00") : expectedEntryStyle + "00",
				shipmentLine.DataImportMatchingKey);
		}

		void AssertCommercialInvoiceLineFromExbondShipmentLineExbondExport(string prefix, CommercialInvoiceLine invoiceLine, int expectedLineNo,
			int expectedEntryInstructionLink, string expectedEntryStyle, ExbondEntryLine shipmentLine)
		{
			var quantityUnit = shipmentLine.IsCustomsControlled ? shipmentLine.CountableUom : null;
			AssertCommercialInvoiceLine(prefix, invoiceLine, expectedLineNo, shipmentLine.CountableQty, quantityUnit,
				shipmentLine.CountryOfOrigin, shipmentLine.CustomsQuantity, shipmentLine.AdditionalQty1, shipmentLine.AdditionalQty2,
				expectedEntryInstructionLink, shipmentLine.TariffCode, shipmentLine.Quantity, shipmentLine.Price, shipmentLine.ProductCode,
				null, null, expectedEntryStyle + "00",
				shipmentLine.DataImportMatchingKey);
		}

		void AssertCommercialInvoiceLineFromExbondShipmentLineExbondExbond(string prefix, CommercialInvoiceLine invoiceLine, int expectedLineNo,
			int expectedEntryInstructionLink, string expectedEntryStyle, ExbondEntryLine shipmentLine)
		{
			var quantityUnit = shipmentLine.IsCustomsControlled ? shipmentLine.CountableUom : null;
			AssertCommercialInvoiceLine(prefix, invoiceLine, expectedLineNo, shipmentLine.CountableQty, quantityUnit,
				shipmentLine.CountryOfOrigin, shipmentLine.CustomsQuantity, shipmentLine.AdditionalQty1, shipmentLine.AdditionalQty2,
				expectedEntryInstructionLink, shipmentLine.TariffCode, shipmentLine.Quantity, shipmentLine.PriceExbond, shipmentLine.ProductCode,
				shipmentLine.MRNLine, shipmentLine.MRN, expectedEntryStyle + (shipmentLine.PreviousProcedure ?? "00"),
				shipmentLine.DataImportMatchingKey);
		}

		void AssertEntryInstructionCount(UniversalShipment shipment, int expectedCount)
		{
			AssertNotNull("EntryInstructionCollection", shipment.EntryInstructionCollection);
			AssertEquals("EntryInstructionCollection.Count", expectedCount, shipment.EntryInstructionCollection.Count);
		}

		void AssertEntryInstruction(EntryInstruction entryInstruction, string expectedDescription, int expectedLink, string expectedStyle,
			string expectedAddressType = null, string expectedAddressShortCode = null, string expectedOrganizationCode = null,
			bool mergeByIsNull = false)
		{
			var prefix = $"EntryInstruction{expectedLink}/";
			AssertEquals(prefix + "Description", expectedDescription, entryInstruction.Description);
			AssertEquals(prefix + "Link", expectedLink, entryInstruction.Link);
			if (mergeByIsNull)
			{
				AssertNull(prefix + "MergeBy", entryInstruction.MergeBy);
			}
			else
			{
				AssertNotNull(prefix + "MergeBy", entryInstruction.MergeBy);
				AssertEquals(prefix + "MergeBy/Code", "NON", entryInstruction.MergeBy.Code);
			}
			AssertEquals(prefix + "Style", expectedStyle, entryInstruction.Style);
			if (expectedAddressType == null)
			{
				AssertNull(prefix + "OrganizationAddressCollection", entryInstruction.OrganizationAddressCollection);
			}
			else
			{
				AssertOrganizationAddress(prefix + "EntryInstructionCollection/", entryInstruction.OrganizationAddressCollection, expectedAddressType, expectedAddressShortCode, expectedOrganizationCode);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ownerOrg = Factory.New<OrgHeader>();
			ownerOrg.OH_Code = "GLECOR_ZA";
			ownerOrg.MainAddress.OA_Address1 = "3 NEW RD";

			batchWarehouseAddress = ownerOrg.Addresses.AddNew();
			batchWarehouseAddress.OA_Address1 = "4 NEW RD";
			var officeCode = batchWarehouseAddress.CustomsCodes.AddNew();
			officeCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			officeCode.OK_CustomsRegNo = "JNB123";
			var officeCode2 = batchWarehouseAddress.CustomsCodes.AddNew();
			officeCode2.OK_CodeType = OrgCusCode.CodeTypes.AgentCode;
			officeCode2.OK_CustomsRegNo = "ABC999";

			Factory.Save();
		}

		OrgHeader ownerOrg;
		OrgAddress batchWarehouseAddress;
	}
}
