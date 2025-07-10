using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	abstract class ExportOperationalActionRunnerTest : TestCaseWithFactory
	{
		protected UniversalShipment LoadShipmentByLoggedMessageNumber()
		{
			const string expectedPrefix = "WARNING: Created message ";
			var messageNum = log.messages.FindLast(m => m.StartsWith(expectedPrefix));
			AssertNotNullOrEmpty("'Created message' response" + (log.messages.Count > 0 ? $" (last message was {log.messages.Last()})" : ""), messageNum);
			messageNum = messageNum.Substring(expectedPrefix.Length, messageNum.Length - expectedPrefix.Length);
			return LoadShipmentByMessageNumber(messageNum);
		}

		protected UniversalShipment LoadShipmentByMessageNumber(string messageNum)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNum)
				.AddToFilter(EDIMessageSchema.EM_ApplicationCode, "UDM")
				.AddToFilter(EDIMessageSchema.EM_MessageType, "XDC")
				.AddToFilter(EDIMessageSchema.EM_MessageSubType, "XUS")
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, "RCV")
				.AddToFilter(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
			var message = Factory.LoadTop1<EDIMessage>(query);
			AssertNotNull($"Should be able to load the message created by CreateEDIMessage with message number {messageNum}", message);

			UniversalShipment shipment = null;
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(message.EM_MessageData.ToUTF8())))
			{
				shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
			}

			AssertNotNull("Should be able to create a Shipment from the message text", shipment);
			return shipment;
		}

		protected List<UniversalShipment> LoadShipmentsByLoggedMessageNumbers()
		{
			const string expectedPrefix = "WARNING: Created messages ";
			var message = log.messages.FindLast(m => m.StartsWith(expectedPrefix));
			AssertNotNullOrEmpty("'Created messages' response" + (log.messages.Count > 0 ? $" (last message was {log.messages.Last()})" : ""), message);
			var messageNumbersString = message.Substring(expectedPrefix.Length, message.Length - expectedPrefix.Length);
			var messageNumbers = messageNumbersString.Split(',');
			return messageNumbers.Select(x => x.Trim()).Select(x => LoadShipmentByMessageNumber(x)).ToList();
		}

		protected CusWHSOperatorTransaction CreateOrder(ZString exportType, ZString ownerReference, ZDecimal quantity)
			=> CreateOperatorTransaction(WarehouseOperatorTransactionTypeList.Codes.ORD, exportType, ownerReference, quantity, isCustomsControlled: false);
		protected CusWHSOperatorTransaction CreateReceipt(ZString ownerReference, ZDecimal quantity, string customsEntryNumber = "", bool isCustomsControlled = true, string countryOfOrigin = null)
			=> CreateOperatorTransaction(WarehouseOperatorTransactionTypeList.Codes.REC, ZString.Empty, ownerReference, quantity, customsEntryNumber, isCustomsControlled, countryOfOrigin);

		protected CusWHSOperatorTransaction CreateOperatorTransaction(string transactionType, ZString exportType, ZString ownerReference, ZDecimal quantity,
			string customsEntryNumber = "", bool isCustomsControlled = true, string countryOfOrigin = null)
		{
			var transaction = Factory.New<CusWHSOperatorTransaction>();
			transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
			transaction.WOT_BatchLineNo = batchLineNo++;
			transaction.WOT_TransactionType = transactionType;
			transaction.WOT_ExportType = exportType;
			transaction.WOT_TransactionDate = today;
			transaction.WOT_OwnerReference = ownerReference;
			transaction.WOT_OH_ProductOwner = whsHelper.Importer.PK;
			transaction.WOT_OP_Product = whsHelper.Part.PK;
			transaction.WOT_Quantity = quantity;
			transaction.WOT_TotalValue = quantity * 100m;
			transaction.WOT_RX_NKCurrency = "ZAR";
			transaction.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			transaction.WOT_SystemLastEditTimeUtc = middayYesterdayUtc;
			transaction.WOT_CustomsEntryNumber = customsEntryNumber;
			transaction.WOT_IsCustomsControlled = isCustomsControlled;
			if (countryOfOrigin != null)
			{
				transaction.WOT_RN_NKOrigin = countryOfOrigin;
			}
			return transaction;
		}

		protected void AssertEntryInstruction(string invoiceLineName, UniversalShipment shipment, ZInt? entryInstructionLink,
			string expectedDescription, string expectedMergeBy, string expectedEntryStyle, IWhsWarehouse expectedWarehouse)
		{
			AssertNotNull($"{invoiceLineName})/EntryInstructionLink", entryInstructionLink);

			var entryInstruction = shipment.EntryInstructionCollection.Single(x => x.Link == entryInstructionLink);
			var prefix = $"(Entry for {invoiceLineName})/";
			AssertNotNull($"{prefix}(Link={entryInstructionLink})", entryInstruction);

			AssertEquals(prefix + "Description", expectedDescription, entryInstruction.Description);
			AssertNotNull(prefix + "MergeBy", entryInstruction.MergeBy);
			AssertEquals(prefix + "MergeBy.Code", expectedMergeBy, entryInstruction.MergeBy.Code);
			AssertEquals(prefix + "Style", expectedEntryStyle, entryInstruction.Style);
			if (expectedWarehouse == null)
			{
				AssertNull(prefix + "OrganizationAddressCollection", entryInstruction.OrganizationAddressCollection);
			}
			AssertEquals(prefix + "Description", expectedDescription, entryInstruction.Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			log = new DummyOperationalActionSectionLog();
			today = ZDate.Today;
			middayYesterdayUtc = new ZDateTime(today.Year, today.Month, today.Day, 12, 0, 0, DateTimeKind.Utc).AddDays(-1);
			whsHelper = new WhsDataTestHelper(Factory);

			warehouse = whsHelper.GetNewWhsWarehouse(whsHelper.Warehouse.MainAddress.PK, isVirtualWarehouse: true, "N10");
			var receive = whsHelper.GetNewWhsReceive(warehouse.PK, whsHelper.Importer.PK, "RCV1", middayYesterdayUtc.ToOffset());
			var inventory = whsHelper.GetNewReceiveInventory(receive, whsHelper.Part, "PACKAGE1", 10m, 90m, 90m, bondedEntryKey: "EN00123-1");
			whsHelper.WhsHelper.WhsReceiveAllocateLocationsMock(receive.PK);
			receive.FinaliseDocketWithoutUserConfirmation();
			whsHelper.GetNewWhsBondedWarehouseAttribute(inventory.PK, 300m, 0m, "", 0m, "", 0m, "", Core.Constants.CountryCodes.SouthAfrica, 90m, "PKT", "", "EN00123-1", 1);

			batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Test Batch 1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = warehouse.WW_OA_WarehouseAddress;
		}

		protected void TestRunner_WithInsufficientQuantities(ExportOperationalActionRunner runner, string exportType, bool willBeConfirmed)
		{
			var order = CreateOrder(exportType, "OwnRef001", 30m);
			Factory.Save();

			var selections = new List<OperatorTransactionSelection>
			{
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "OwnRef001"
				},
				new OperatorTransactionSelection
				{
					WarehouseAddress = warehouse.WW_OA_WarehouseAddress,
					ProductOwner = whsHelper.Importer.PK,
					OwnerReference = "OwnRef001"
				},
			};

			runner.Run(selections);

			if (willBeConfirmed)
			{
				AssertCollectionNotContains("Won't create any EDI message even when confirmed due to no bonded stock", log.messages, m => m.StartsWith("WARNING: Created message "));
			}
			else
			{
				AssertCollectionNotContains("Should not create any EDI message when cancelled", log.messages, m => m.StartsWith("WARNING: Created message "));
			}
			AssertEquals("WARNING: Product code: ~~1 Qty Requested of 30 is greater than Qty available of 0", log.messages.LastOrDefault(m => m.StartsWith("WARNING: P")));

			var receipt = CreateReceipt("Receipt001", 20m, isCustomsControlled: false);
			var orderLine1 = order.TransactionLines.AddNew();
			orderLine1.WOL_Quantity = 3m;
			orderLine1.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			orderLine1.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			var orderLine2 = order.TransactionLines.AddNew();
			orderLine2.WOL_Quantity = 7m;
			orderLine2.WOL_WOT_WHSOperatorTransactionOrder = order.PK;
			orderLine2.WOL_WOT_WHSOperatorTransactionReceipt = receipt.PK;
			Factory.Save();

			runner.Run(selections);

			if (willBeConfirmed)
			{
				AssertCollectionContains("Should create EDI message when confirmed", log.messages, m => m.StartsWith("WARNING: Created message "));
			}
			else
			{
				AssertCollectionNotContains("Should not create any EDI message", log.messages, m => m.StartsWith("WARNING: Created message "));
			}
			AssertEquals("WARNING: Product code: ~~1 Qty Requested of 20 is greater than Qty available of 10", log.messages.LastOrDefault(m => m.StartsWith("WARNING: P")));
		}

		protected DummyOperationalActionSectionLog log;
		protected ZDate today;
		protected ZDateTime middayYesterdayUtc;
		protected WhsDataTestHelper whsHelper;
		protected IWhsWarehouse warehouse;
		protected CusWHSOperatorTransactionBatch batch;
		protected int batchLineNo = 1;
	}
}
