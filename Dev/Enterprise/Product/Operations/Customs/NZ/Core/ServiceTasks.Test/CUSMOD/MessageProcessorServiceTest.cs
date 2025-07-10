using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.NZ.ServiceTasks.CUSMOD.Testing
{
	[TestedType(typeof(MessageProcessorService))]
	sealed class MessageProcessorServiceTest : ServiceTaskTestCase<MessageProcessorService>
	{
		public void TestProcessMessages()
		{
			var branch = SetupValidEnvironment();

			var nzcMessage = Factory.New<NZCMessage>();
			nzcMessage.EM_MessageNum = "1";
			nzcMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			nzcMessage.EM_Status = NZCMessage.Status.Queued;
			nzcMessage.EM_GB = branch.PK;
			Factory.Save();

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageNum = "2";
			ocrMessage.EM_ReceiveTransmit = TSWMessage.Direction.Receive;
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_Status = TSWMessage.Status.Queued;
			ocrMessage.EM_GB = branch.PK;
			Factory.Save();

			LoggerForTesting logger = new LoggerForTesting();
			MessageProcessorService serviceTask = new MessageProcessorService();
			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();

			nzcMessage.Reload();
			ocrMessage.Reload();
			AssertEquals("nzcMessage.EM_Status", NZCMessage.Status.Error, nzcMessage.EM_Status);
			AssertEquals("ocrMessage.EM_Status", TSWMessage.Status.Failed, ocrMessage.EM_Status);
			AssertMultilineASCIIEquals("logger.ToString()", @"
Information:- Processing Message #1
Information:- Corrupted or Malformed response message. Message does not conform to UN-EDIFACT standard. Cannot process.
Information:- Processing Message #2
Information:- Message contains invalid XML
Information:- Saving...
Information:- 2 messages processed
".Trim(), logger.ToString());
		}

		public void TestCorrectNumberofHeaders()
		{
			var branch = SetupValidEnvironment();

			var declaration = Factory.New<JobDeclaration>();
			SetupMergedFormalEntry(declaration);
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var serviceTask = new MessageProcessorService();
			serviceTask.ServiceLogger = new LoggerForTesting();

			var receivedMessage = GetNZCMessage(declaration, EDIFACTMessageReceived, branch);
			serviceTask.RunTask();

			var originalHeader = declaration.CusEntryHeader;
			receivedMessage.Reload();
			originalHeader.Reload();
			AssertEquals("message.EM_Status", NZCMessage.Status.Received, receivedMessage.EM_Status);
			AssertEquals("declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("originalHeader.EntryNumber", "77013452", originalHeader.EntryNumber);
			AssertEquals("originalHeader.CH_EntryStatus", FormalEntryStatusList.Codes.InspectionsAuditRequirements, originalHeader.CH_EntryStatus);
			AssertEquals("originalHeader.CH_IsActive", true, originalHeader.CH_IsActive);

			var entryCancelMessage = GetNZCMessage(declaration, EDIFACTMessageEntryCancel, branch);
			serviceTask.RunTask();

			entryCancelMessage.Reload();
			originalHeader.Reload();
			AssertEquals("message.EM_Status", NZCMessage.Status.Received, entryCancelMessage.EM_Status);
			AssertEquals("originalHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalHeader.CH_EntryStatus);
			AssertEquals("originalHeader.CH_IsActive", false, originalHeader.CH_IsActive);

			var entryRestoreMessage = GetNZCMessage(declaration, EDIFACTMessageEntryRestore, branch);
			serviceTask.RunTask();

			declaration.CustomsEntryHeaders.Reload(true, true);
			var restoredHeader = declaration.CusEntryHeader;
			entryRestoreMessage.Reload();
			originalHeader.Reload();
			restoredHeader.Reload();
			AssertEquals("message.EM_Status", NZCMessage.Status.Received, entryRestoreMessage.EM_Status);
			AssertEquals("declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("restoredHeader", originalHeader, restoredHeader);
			AssertEquals("restoredHeader.EntryNumber", "77013452", restoredHeader.EntryNumber);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);

			var deliveryOrderMessage = GetNZCMessage(declaration, EDIFACTMessageDeliveryOrderReceived, branch);
			serviceTask.RunTask();

			deliveryOrderMessage.Reload();
			restoredHeader.Reload();
			AssertEquals("message.EM_Status", NZCMessage.Status.Received, deliveryOrderMessage.EM_Status);
			AssertEquals("declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("declaration.CusEntryHeader", restoredHeader, declaration.CusEntryHeader);
			AssertEquals("restoredHeader.EntryNumber", "77013452", restoredHeader.EntryNumber);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"NZ Customs Response Messages",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive        + "=Y",
						EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NewZealandCustoms),
				};
			}
		}

		const string EDIFACTMessageReceived =
@"UNH+82995+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:02'
FTX+ICN+++ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.'
GIS+805:120:143'
UNT+5+82995'";

		const string EDIFACTMessageEntryCancel =
@"UNH+83031+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:03'
GIS+814:120:143'
UNT+4+83031'";

		const string EDIFACTMessageEntryRestore =
@"UNH+83032+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:04'
GIS+815:120:143'
UNT+4+83032'";

		const string EDIFACTMessageDeliveryOrderReceived =
@"UNH+83033+CUSRES:D:96B:UN+B01001001'
BGM+932+77013452:04'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:3924.25'
GIS+D:134:143'
UNT+8+83033'";

		void SetupMergedFormalEntry(JobDeclaration declaration)
		{
			declaration.JE_DeclarationReference = "B01001001";
			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m, "AU", "AU", "Q");
			decCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "", "", "", 1000m);
			decCreator.MergeDeclaration();
		}

		NZCMessage GetNZCMessage(JobDeclaration declaration, ZString messageText, GlbBranch branch)
		{
			var message = declaration.CusEntryHeader.Messages.AddNew();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_Status = NZCMessage.Status.Queued;
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_GB = branch.PK;
			Factory.Save();
			return message;
		}

		GlbBranch SetupValidEnvironment()
		{
			Env.Registry.MailServer = "Something";
			Env.Registry.MailboxUserName = "Something";
			Env.Registry.MailboxPassword = "Something";
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "AAA";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var branch = company.Branches.AddNew();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00001234Z");
			Factory.Save();

			return branch;
		}
	}
}
