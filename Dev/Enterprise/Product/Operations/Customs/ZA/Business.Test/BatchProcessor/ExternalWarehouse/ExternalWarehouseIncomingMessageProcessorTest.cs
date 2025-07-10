using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class ExternalWarehouseIncomingMessageProcessorTest : BranchMessageProcessorTest
	{
		public void TestProcessMissingBatch()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageMissingBatch();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Batch number is missing."));
		}

		public void TestProcessDuplicateBatch()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValid();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			var batch = Factory.New<CusWHSOperatorTransactionBatch>();
			batch.WOB_Batch = "Batch1";
			batch.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch.WOB_OA_Warehouse = owner.MainAddress.PK;
			Factory.Save();

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Duplicate Batch name Batch1 – a batch with this name has already been added."));
		}

		public void TestProcessMissingOrInvalidBatchLineNo()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageMissingAndZeroBatchLineNo();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Batch Line No for record number 1 is missing."));
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Batch Line No for record number 2 is invalid."));
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Batch Line No for record number 3 is invalid."));
		}

		public void TestProcessDuplicatedBatchLineNo()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageDuplicatedBatchLineNo();
			ediMessage.EM_MessageNum = "0001";
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Batch Line No 1 for record number 2 is duplicated."));
			AssertEquals("Only 1 line should be logged as duplicated", 1, logger.Logs.Count(x => Regex.IsMatch(x.Message, @"Batch Line No \d+ for record number \d+ is duplicated\.")));
		}

		public void TestProcessMissingOwnerReference()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageMissingOwnerReference();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Owner Reference for Line Number 1 is missing."));
		}

		public void TestProcessInvalidProductOwner()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageInvalidProductOwner();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Product code INVALIDPRODUCT with owner FORDORG cannot be found."));
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Product code AB3921971ABSMR3 with owner INVALIDOWNER cannot be found."));
		}

		public void TestProcessInactiveProduct()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageInactiveProduct();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Product code AB3921971ABSMR4 with owner FORDORG cannot be found."));
		}

		public void TestProcessInvalidWarehouse()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);

			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageInvalidWarehouse();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.ProcessMessage(ediMessage);

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'FAL'", EDIMessageStatusList.Codes.Failed, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate reason for failure", expected: true, logger.Logs.Any(x => x.Message == "Cannot resolve to a warehouse with name FORDWHS and address INVALIDWHS"));
		}

		public void TestProcess()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValid();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.PreProcessMessage(ediMessage);
			processor.ProcessMessage(ediMessage);
			ediMessage.Factory.Save();

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate there are no errors", expected: true, !logger.Logs.Any(x => x.Type == Integration.LogType.Error));

			var batchQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
			batchQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_Batch, "Batch1");
			var batch = Factory.Load<CusWHSOperatorTransactionBatch>(batchQuery).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("CusWHSOperatorTransactionBatch does not exist", batch);
				AssertNotNull("CusWHSOperatorTransactionBatch.Warehouse", batch.Warehouse);
				AssertEquals("Should be 3 transactions", 3, batch.WarehouseOperatorTransactions.Count);
				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[0].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[0].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.EXP);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[0].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[0].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[0].WOT_LineReference, "LineRef1");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[0].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[0].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[0].WOT_Quantity, 22m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[0].WOT_TotalValue, 1800.2389m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[0].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[0].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[0].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[0].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[0].WOT_BatchLineNo, 1);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[0].WOT_IsFinal, false);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[1].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[1].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.BLN);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[1].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[1].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[1].WOT_LineReference, "LineRef2");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[1].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[1].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[1].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[1].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[1].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[1].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[1].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[1].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[1].WOT_BatchLineNo, 2);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[1].WOT_IsFinal, false);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[2].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.REC);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[2].WOT_ExportType, ZString.Empty);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[2].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[2].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[2].WOT_LineReference, "LineRef3");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[2].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[2].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[2].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[2].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[2].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[2].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.QUE);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[2].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[2].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[2].WOT_BatchLineNo, 3);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[2].WOT_IsFinal, false);
			});
		}

		public void TestProcessWithIsFinalReceipt()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValidWithIsFinalReceipt();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.PreProcessMessage(ediMessage);
			processor.ProcessMessage(ediMessage);
			ediMessage.Factory.Save();

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate there are no errors", expected: true, !logger.Logs.Any(x => x.Type == Integration.LogType.Error));

			var batchQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
			batchQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_Batch, "Batch1");
			var batch = Factory.Load<CusWHSOperatorTransactionBatch>(batchQuery).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("CusWHSOperatorTransactionBatch does not exist", batch);
				AssertNotNull("CusWHSOperatorTransactionBatch.Warehouse", batch.Warehouse);
				AssertEquals("Should be 3 transactions", 3, batch.WarehouseOperatorTransactions.Count);
				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[0].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[0].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.EXP);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[0].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[0].WOT_OwnerReference, "111089");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[0].WOT_LineReference, "LineRef1");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[0].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[0].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[0].WOT_Quantity, 22m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[0].WOT_TotalValue, 1800.2389m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[0].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[0].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[0].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[0].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[0].WOT_BatchLineNo, 1);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[0].WOT_IsFinal, false);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[1].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[1].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.BLN);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[1].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[1].WOT_OwnerReference, "111090");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[1].WOT_LineReference, "LineRef2");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[1].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[1].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[1].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[1].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[1].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[1].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[1].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[1].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[1].WOT_BatchLineNo, 2);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[1].WOT_IsFinal, false);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[2].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.REC);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[2].WOT_ExportType, ZString.Empty);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[2].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[2].WOT_OwnerReference, "111091");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[2].WOT_LineReference, "LineRef3");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[2].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[2].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[2].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[2].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[2].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[2].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.QUE);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[2].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[2].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[2].WOT_BatchLineNo, 3);
				AssertEquals("WOT_IsFinal", batch.WarehouseOperatorTransactions[2].WOT_IsFinal, true);
			});
		}

		public void TestProcessMultipleMessages()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessor(logger);
			var ediInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage = Factory.New<EWHMessage>();
			ediMessage.EM_EI = ediInterchange.PK;
			ediMessage.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValid();
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;

			var ediInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			ediInterchange2.EI_GB = GlbBranch.CurrentBranch.PK;
			var ediMessage2 = Factory.New<EWHMessage>();
			ediMessage2.EM_EI = ediInterchange.PK;
			ediMessage2.EM_MessageText = ExternalWarehouseInterchangeTestHelper.GetTestMessageValid2();
			ediMessage2.EM_Status = EDIMessageStatusList.Codes.Queued;

			processor.PreProcessMessage(ediMessage);
			processor.PreProcessMessage(ediMessage2);
			processor.ProcessMessage(ediMessage);
			processor.ProcessMessage(ediMessage2);
			ediMessage.Factory.Save();

			var reloadedMessage = Factory.Load<EWHMessage>(ediMessage.PK);
			AssertEquals("EM_Status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, reloadedMessage.EM_Status);
			AssertEquals("Logger should indicate there are no errors", expected: true, !logger.Logs.Any(x => x.Type == Integration.LogType.Error));

			var batchQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
			batchQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_Batch, "Batch1");
			var batch = Factory.Load<CusWHSOperatorTransactionBatch>(batchQuery).FirstOrDefault();

			AssertNotNull("CusWHSOperatorTransactionBatch does not exist", batch);

			CombineAssertions(() =>
			{
				AssertNotNull("CusWHSOperatorTransactionBatch.Warehouse", batch.Warehouse);
				AssertEquals("Should be 3 transactions", 3, batch.WarehouseOperatorTransactions.Count);
				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[0].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[0].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.EXP);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[0].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[0].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[0].WOT_LineReference, "LineRef1");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[0].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[0].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[0].WOT_Quantity, 22m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[0].WOT_TotalValue, 1800.2389m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[0].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[0].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[0].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[0].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[0].WOT_BatchLineNo, 1);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[1].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[1].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.BLN);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[1].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[1].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[1].WOT_LineReference, "LineRef2");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[1].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[1].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[1].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[1].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[1].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[1].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[1].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[1].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[1].WOT_BatchLineNo, 2);

				AssertEquals("WOT_TransactionType", batch.WarehouseOperatorTransactions[2].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.REC);
				AssertEquals("WOT_ExportType", batch.WarehouseOperatorTransactions[2].WOT_ExportType, ZString.Empty);
				AssertEquals("WOT_TransactionDate", batch.WarehouseOperatorTransactions[2].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch.WarehouseOperatorTransactions[2].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch.WarehouseOperatorTransactions[2].WOT_LineReference, "LineRef3");
				AssertEquals("WOT_OP_Product", batch.WarehouseOperatorTransactions[2].WOT_OP_Product, part.PK);
				AssertEquals("WOT_OH_ProductOwner", batch.WarehouseOperatorTransactions[2].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch.WarehouseOperatorTransactions[2].WOT_Quantity, 20m);
				AssertEquals("WOT_TotalValue", batch.WarehouseOperatorTransactions[2].WOT_TotalValue, 35234.00m);
				AssertEquals("WOT_RX_NKCurrency", batch.WarehouseOperatorTransactions[2].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch.WarehouseOperatorTransactions[2].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.QUE);
				AssertEquals("WOT_IsCustomsControlled", batch.WarehouseOperatorTransactions[2].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch.WarehouseOperatorTransactions[2].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch.WarehouseOperatorTransactions[2].WOT_BatchLineNo, 3);
			});

			var batch2Query = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
			batch2Query.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_Batch, "Batch2");
			var batch2 = Factory.Load<CusWHSOperatorTransactionBatch>(batch2Query).FirstOrDefault();

			AssertNotNull("CusWHSOperatorTransactionBatch does not exist", batch2);

			CombineAssertions(() =>
			{
				AssertNotNull("CusWHSOperatorTransactionBatch.Warehouse", batch2.Warehouse);
				AssertEquals("Should be 1 transaction", 1, batch2.WarehouseOperatorTransactions.Count);
				AssertEquals("WOT_TransactionType", batch2.WarehouseOperatorTransactions[0].WOT_TransactionType, WarehouseOperatorTransactionTypeList.Codes.ORD);
				AssertEquals("WOT_ExportType", batch2.WarehouseOperatorTransactions[0].WOT_ExportType, WarehouseOperatorTransactionExportTypeList.Codes.EXP);
				AssertEquals("WOT_TransactionDate", batch2.WarehouseOperatorTransactions[0].WOT_TransactionDate, new ZDate(2022, 11, 23));
				AssertEquals("WOT_OwnerReference", batch2.WarehouseOperatorTransactions[0].WOT_OwnerReference, "111088");
				AssertEquals("WOT_LineReference", batch2.WarehouseOperatorTransactions[0].WOT_LineReference, "LineRef1");
				AssertEquals("WOT_OP_Product", batch2.WarehouseOperatorTransactions[0].WOT_OP_Product, part2.PK);
				AssertEquals("WOT_OH_ProductOwner", batch2.WarehouseOperatorTransactions[0].WOT_OH_ProductOwner, owner.PK);
				AssertEquals("WOT_Quantity", batch2.WarehouseOperatorTransactions[0].WOT_Quantity, 5m);
				AssertEquals("WOT_TotalValue", batch2.WarehouseOperatorTransactions[0].WOT_TotalValue, 200m);
				AssertEquals("WOT_RX_NKCurrency", batch2.WarehouseOperatorTransactions[0].WOT_RX_NKCurrency, "ZAR");
				AssertEquals("WOT_Status", batch2.WarehouseOperatorTransactions[0].WOT_Status, WarehouseOperatorTransactionStatusList.Codes.VAL);
				AssertEquals("WOT_IsCustomsControlled", batch2.WarehouseOperatorTransactions[0].WOT_IsCustomsControlled, false);
				AssertEquals("WOT_RN_NKOrigin", batch2.WarehouseOperatorTransactions[0].WOT_RN_NKOrigin, "");
				AssertEquals("WOT_BatchLineNo", batch2.WarehouseOperatorTransactions[0].WOT_BatchLineNo, 1);
			});
		}

		public void TestGetWarehouseAddressDoesNotThrowExceptionWhenAddressIsNull()
		{
			var logger = new LoggingInformationForTesting();
			var processor = new ExternalWarehouseMessageProcessorForTest(logger);
			AssertNoExceptionThrown("No exception should be thrown when address is NULL", () => processor.GetWarehouseAddressExposed(Factory, null));
		}

		protected override void SetUp()
		{
			base.SetUp();

			(part, part2, owner) = ExternalWarehouseInterchangeTestHelper.SetupWarehouseAndPart(Factory);
		}

		OrgSupplierPart part, part2;
		OrgHeader owner;
	}

	class ExternalWarehouseMessageProcessorForTest : ExternalWarehouseMessageProcessor
	{
		public ExternalWarehouseMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public ZGuid GetWarehouseAddressExposed(BusinessObjectFactory factory, WarehouseOrganizationAddress address) => GetWarehouseAddress(factory, address);
	}
}
