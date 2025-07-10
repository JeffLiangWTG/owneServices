using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class EntrySummaryMessagingWithBondedWarehouseIntegrationTest : TestCaseWithFactory
	{
		#region Process Inward

		public const string ACEEntrySummaryAcceptedMessageText =
"B001101SV9AX                                               HYEDUSCMT_151199     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100541500100B00162063   " +
"Y  1101SV9AX00002";

		public void TestProcessInwardOriginalClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText);

				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				declaration.PublishShipmentForWHSInward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entryHeader.CH_Status);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BINW000001 / XJ5-4000000-7"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardOriginalClearWithWHSTransactionForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText);

				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				declaration.PublishShipmentForWHSInward(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entryHeader.CH_Status);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BINW000001 / XJ5-4000000-7"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public const string ACEEntrySummaryAcceptedMessage =
"B001101SV9AX                                               HYEDUSCMT_196742     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100541500100B00162063   " +
"Y  1101SV9AX00002";

		public const string ACEEntrySummaryReleaseRejectedMessage =
"B001101SV9AX                                               HYEDUSCMT_196743     " +
"E0 SUMMRY 000001 REF ID: SV9 71005415 B00162063                                 " +
"E1 FB11 BROKER NOT AUTHORIZED IN PORT           SV9  71005415     B00162063     " +
"E1RF998   TRANSACTION DATA REJECTED SV9  71005415     B00162063                 " +
"Y  1101SV9AX00003";

		public void TestProcessReplaceMessageWithEntryType21ButAlreadyAcceptedWithEntryType01()
		{
			var declaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B00162063", "WH1", "SV9", "71005415", 100m);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.DoMerge();

			MQEDIMessage incomingMessage1;
			var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage1, "HYEDUSCMT_196742", ACEEntrySummaryAcceptedMessage);
			incomingMessage1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage1);
			AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
			AssertEquals("entryHeader.CH_Status", MessageStatusListENS.Codes.ClearEntrySummaryOriginal, entryHeader.CH_Status);  //SUMMARY HAS BEEN ADDED

			declaration.PublishShipmentForWHSInward(true);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertHasErrorContaining(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;

			MQEDIMessage incomingMessage2;
			var entryHeader2 = SetupEntrySummaryMessages(declaration, out incomingMessage2, "HYEDUSCMT_196743", ACEEntrySummaryReleaseRejectedMessage);
			incomingMessage2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage2);
			Factory.Save();

			AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
			AssertEquals("entryHeader.CH_Status", MessageStatusListENS.Codes.ErrorEntrySummaryOriginal, entryHeader2.CH_Status); // Replacement message has been rejected.

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertNoErrorContaining(declaration.US_EntryTypeInfo, FormalImportAddInfoJobDeclarationValidation.InvalidEntryTypeForInward);
		}

		public void TestProcessInwardAmendmentClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryReplace);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);

				declaration.PublishShipmentForWHSInward(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdatedPending, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryReplace, entryHeader.CH_Status);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardUpdated, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BINW000001 / XJ5-4000000-7"; }));
				AssertContains("Stock Levels have been updated. (WHS Receipt: ", email.Body);
			}
		}

		public void TestProcessInwardWithdrawalClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var declaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryDelete);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = declaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				AssertEquals("WHSTransactionStatus", ZString.Empty, declaration.WarehouseTransactionStatus);
				declaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);

				declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryDelete, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, declaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCanceled, declaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 0m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BINW000001 / XJ5-4000000-7"; }));
				AssertContains("Stock Levels Update has been canceled.", email.Body);
			}
		}

		#endregion

		#region Process Outward

		public void TestProcessOutwardOriginalClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				var outwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.WarehouseWithdrawalConsumption, "BOUTW00002", "WH2", "XJ5", "40000007", 60m);
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				inwardDeclaration.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.InwardCreated, inwardDeclaration.WarehouseTransactionStatus);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release can be finalized. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardAmendmentClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var outwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.WarehouseWithdrawalConsumption, "BOUTW00002", "WH2", "XJ5", "40000007", 60m);
				var outwardDeclarationInvoiceLine1 = outwardDeclaration.InvoiceLines[0];
				var outwardDeclarationInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 150m;
				outwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				outwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				outwardDeclarationInvoiceLine2.JI_CustomsQuantity = 1500m;
				outwardDeclarationInvoiceLine2.JI_LinePrice = 15000m;
				outwardDeclarationInvoiceLine2.US_WHSEntryLineNo = 2;
				outwardDeclaration.DoMerge();

				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryReplace);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				outwardDeclarationInvoiceLine1.JI_InvoiceQuantity = 70m;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 140m;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryReplace, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 60m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release has been updated. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardWithdrawalClearForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var outwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.WarehouseWithdrawalConsumption, "BOUTW00002", "WH2", "XJ5", "40000007", 60m);
				var outwardDeclarationInvoiceLine1 = outwardDeclaration.InvoiceLines[0];
				var outwardDeclarationInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 150m;
				outwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				outwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				outwardDeclarationInvoiceLine2.JI_CustomsQuantity = 1500m;
				outwardDeclarationInvoiceLine2.JI_LinePrice = 15000m;
				outwardDeclarationInvoiceLine2.US_WHSEntryLineNo = 2;
				outwardDeclaration.DoMerge();

				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryDelete);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				outwardDeclarationInvoiceLine1.JI_InvoiceQuantity = 70m;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 140m;
				Factory.Save();
				outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryDelete, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
			}
		}

		#endregion

		#region Process Outward

		public void TestProcessOutwardOriginalClearForConsumptionFTZForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m, WarehouseConstants.WarehouseType.FreeTradeZone);
				var outwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.ConsumptionFTZ, "BOUTW00002", "WH2", "XJ5", "40000007", 60m, WarehouseConstants.WarehouseType.FreeTradeZone);
				outwardDeclaration.US_EnableENS = true;
				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release can be finalized. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardAmendmentClearForConsumptionFTZForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m, WarehouseConstants.WarehouseType.FreeTradeZone);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var outwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.ConsumptionFTZ, "BOUTW00002", "WH2", "XJ5", "40000007", 60m, WarehouseConstants.WarehouseType.FreeTradeZone);
				var outwardDeclarationInvoiceLine1 = outwardDeclaration.InvoiceLines[0];
				var outwardDeclarationInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 150m;
				outwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				outwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				outwardDeclarationInvoiceLine2.JI_CustomsQuantity = 1500m;
				outwardDeclarationInvoiceLine2.JI_LinePrice = 15000m;
				outwardDeclarationInvoiceLine2.US_WHSEntryNumber = "XJ5-40000007";
				outwardDeclarationInvoiceLine2.US_WHSEntryLineNo = 2;
				outwardDeclaration.DoMerge();

				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryReplace);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				outwardDeclarationInvoiceLine1.JI_InvoiceQuantity = 70m;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 140m;
				Factory.Save();
				outwardDeclaration.PublishShipmentForWHSOutwardWithPreAmendmentData();
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryReplace, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardUpdated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 30m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 60m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release has been updated. (WHS Order: ", email.Body);
			}
		}

		public void TestProcessOutwardWithdrawalClearForConsumptionFTZForACE()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);

			using (whsHelper.UsePutawayEngineManagerMock())
			using (whsHelper.UseAllocationEngineMock())
			{
				var inwardDeclaration = GetNewACEDeclaration(EntryTypeList.Codes.Warehouse, "BINW000001", "WH1", "XJ5", "40000007", 100m, WarehouseConstants.WarehouseType.FreeTradeZone);
				var inwardDeclarationInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				inwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 200m;
				inwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardDeclarationInvoiceLine2.JI_CustomsQuantity = 2000m;
				inwardDeclarationInvoiceLine2.JI_LinePrice = 20000m;
				var whsPack = inwardDeclaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 20;
				var whsPackLine = inwardDeclaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = inwardDeclarationInvoiceLine2.PK;
				inwardDeclaration.DoMerge();
				var outwardDeclaration = GetNewDeclaration(EntryTypeList.Codes.ConsumptionFTZ, "BOUTW00002", "WH2", "XJ5", "40000007", 60m, WarehouseConstants.WarehouseType.FreeTradeZone);
				var outwardDeclarationInvoiceLine1 = outwardDeclaration.InvoiceLines[0];
				var outwardDeclarationInvoiceLine2 = outwardDeclaration.InvoiceLines.AddNew();
				outwardDeclarationInvoiceLine2.JI_PartNo = Part2.OP_PartNum;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 150m;
				outwardDeclarationInvoiceLine2.JI_InvoiceUQ = "NO";
				outwardDeclarationInvoiceLine2.JI_CustomsUnitQty = "KG";
				outwardDeclarationInvoiceLine2.JI_CustomsQuantity = 1500m;
				outwardDeclarationInvoiceLine2.JI_LinePrice = 15000m;
				outwardDeclarationInvoiceLine2.US_WHSEntryNumber = "XJ5-40000007";
				outwardDeclarationInvoiceLine2.US_WHSEntryLineNo = 2;
				outwardDeclaration.DoMerge();

				MQEDIMessage incomingMessage;
				var entryHeader = SetupEntrySummaryMessages(outwardDeclaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText, EM_MessageSubTypeList.Codes.EntrySummaryDelete);
				incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);

				result = outwardDeclaration.PublishShipmentForWHSOutward(true);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsOrder = (IWhsOrder)result.FindJobIfExists();
				AssertNotNull(whsOrder);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("entryHeader.CH_Status", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreatedPending, outwardDeclaration.WarehouseTransactionStatus);
				outwardDeclaration.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				outwardDeclarationInvoiceLine1.JI_InvoiceQuantity = 70m;
				outwardDeclarationInvoiceLine2.JI_InvoiceQuantity = 140m;
				Factory.Save();
				outwardDeclaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);
				AssertEquals("Email creation should be delayed", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("entryHeader.CH_Status", ImportMessageStatusList.Codes.ClearEntrySummaryDelete, entryHeader.CH_Status);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 40m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 50m);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardHolding, outwardDeclaration.WarehouseTransactionStatus);
				Factory.Save();
				AssertEquals(MQEDIMessage.Status.Received, incomingMessage.EM_Status);
				AssertEquals("WHSTransactionStatus", WarehouseTransactionStatusList.Codes.OutwardCanceled, outwardDeclaration.WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-1", 100m);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("XJ5-40000007-2", 200m);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => { return emailToMatched.Subject == "Entry Summary Response for BOUTW00002 / XJ5-0000001-4"; }));
				AssertContains("Stock Release has been canceled. (WHS Order: ", email.Body);
			}
		}

		#endregion

		public void TestPermitClosedWhenEntrySummaryIsAccepted()
		{
			// - create FTZ declaration

			JobDeclaration declaration = GetNewACEDeclaration(EntryTypeList.Codes.ConsumptionFTZ, "BOUTW00002", "WH1", "XJ5", "40000007", 60m);
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_PresentationDate = new ZDateTime(2017, 10, 14);
			declaration.ImportEntryNumber = "40000007";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);

			Factory.Save();

			// - check that permit created and is not closed

			List<CusPermitHeader> permits = declaration.FindRelatedPermits();
			AssertEquals(1, permits.Count);

			CusPermitHeader permit = permits[0];
			AssertEquals(false, permit.CPH_IsClosed);

			// - send entry summary
			// - create and process response message accepting entry summary

			MQEDIMessage incomingMessage;
			var entryHeader = SetupEntrySummaryMessages(declaration, out incomingMessage, "402232", ACEEntrySummaryAcceptedMessageText);
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			declaration.DeriveDeclarationStatus();
			Factory.Save();

			new ABIMessageProcessorFactory(new LoggingInformation()).ProcessMessage(incomingMessage);

			// - check that permit is closed

			AssertEquals(true, permit.CPH_IsClosed);
			Assert("has closed log entry", permit.Logs.HasLogWith(
				l => l.SL_SE_NKEvent == "PCL" && l.SL_Reference == "Permit closed"
			));
		}

		protected override void SetUp()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}

		CusEntryHeader SetupEntrySummaryMessages(JobDeclaration declaration, out MQEDIMessage incomingMessage, ZString messageNumber, ZString messageText, string messageSubType = null)
		{
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var outgoingMessage = mock.Object;
			outgoingMessage.EM_MessageNum = messageNumber;
			entryHeader.Messages.Add(outgoingMessage);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = messageNumber;
			incomingMessage.EM_MessageText = messageText;
			if (messageSubType != null)
			{
				outgoingMessage.EM_MessageSubType = messageSubType;
				incomingMessage.EM_MessageSubType = messageSubType;
			}
			return entryHeader;
		}

		JobDeclaration GetNewACEDeclaration(ZString entryType, ZString declarationReference, string whsCode, ZString entryFilerCode, ZString entryNumber, ZDecimal quantity, string warehouseType = WarehouseConstants.WarehouseType.Product)
		{
			var result = GetNewDeclaration(entryType, declarationReference, whsCode, entryFilerCode, entryNumber, quantity, warehouseType);
			result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			result.DoMerge();
			return result;
		}

		JobDeclaration GetNewDeclaration(ZString entryType, ZString declarationReference, string whsCode, ZString entryFilerCode, ZString entryNumber, ZDecimal quantity, string warehouseType = WarehouseConstants.WarehouseType.Product)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = declarationReference;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse(Warehouse.MainAddress.OA_Address1, whsCode, "BOND");
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_WarehouseType = warehouseType;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.US_EntryType = entryType;
			declaration.US_EnableENS = true;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			if (declaration.IsExWarehouseEntryType)
			{
				declaration.US_WHSEntryFilerCode = entryFilerCode;
				declaration.US_WHSEntryNumber = entryNumber;
				invoiceLine.US_WHSEntryLineNo = 1;
			}
			else if (declaration.IsENSFormalImportAndConsumptionFTZ)
			{
				invoiceLine.US_WHSEntryNumber = entryFilerCode + "-" + entryNumber;
				invoiceLine.US_WHSEntryLineNo = 1;
			}
			else
			{
				declaration.US_EntryFilerCode = entryFilerCode;
				declaration.ImportEntryNumber = entryNumber;
				var whsPack = declaration.WHSPacks.AddNew();
				whsPack.US_PackageQty = 10;
				var whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
				whsPackLine.US_JI_InvoiceLine = invoiceLine.PK;
			}
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration;
		}

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}
				return importer;
			}
		}

		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.OH_RL_NKClosestPort = "USLAX";
					warehouse.OH_FullName = "WAREHOUSE ORG";
					warehouse.MainAddress.OA_Address1 = "ADDRESS 1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}

		OrgHeader warehouse;

		OrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<OrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					var pivot = Pivot; // Force the creation of the pivot
				}
				return part;
			}
		}

		OrgSupplierPart part;

		OrgSupplierPart Part2
		{
			get
			{
				if (part2 == null)
				{
					part2 = Factory.New<OrgSupplierPart>();
					part2.OP_PartNum = "~~2";
					part2.OP_StockKeepingUnit = "NO";
					part2.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					var pivot = Pivot2; // Force the creation of the pivot
				}
				return part2;
			}
		}

		OrgSupplierPart part2;

		CusClassification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<CusClassification>();
					classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
					classification.CC_LookupCode = "@#$34";
					classification.CC_TariffNum = "1010101010";
				}
				return classification;
			}
		}

		CusClassification classification;

		CusClassPartPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					pivot = Part.PivotsForBinding.AddNew();
					pivot.CI_UsageComment = "U1";
					pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot.CI_TariffNum = "1010101010";
					pivot.CI_CC = Classification.PK;
					pivot.CD_LicenceNo = "13";
				}
				return pivot;
			}
		}

		CusClassPartPivot pivot;

		CusClassPartPivot Pivot2
		{
			get
			{
				if (pivot2 == null)
				{
					pivot2 = Part2.PivotsForBinding.AddNew();
					pivot2.CI_UsageComment = "U1";
					pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
					pivot2.CI_OH = Part.RelatedOrganisations[0].OU_OH;
					pivot2.CI_TariffNum = "2010101010";
					pivot2.CI_CC = Classification.PK;
					pivot2.CD_LicenceNo = "13";
				}
				return pivot2;
			}
		}

		CusClassPartPivot pivot2;
	}
}
