using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using static NUnit.Framework.Assertion;
using static NUnit.Framework.AssertionWithHtml;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using CusPollingTransaction = Enterprise.Customs.Business.CusPollingTransaction;
using CusPollingTransactionStatuses = Enterprise.Core.Constants.Customs.CusPollingTransactionStatus.Codes;
using CusPollingTransactionTypes = Enterprise.Core.Constants.Customs.CusPollingTransactionType.Codes;
using EDIMessageStatuses = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes;
using InterchangeStatus = Enterprise.Messaging.Business.EDIInterchange.Status;

namespace Enterprise.Customs.PL.Business.Testing;

public sealed class CusPollingTransactionTestHelper : IDisposable
{
	readonly BusinessObjectFactory factory;
	readonly GlbStaff staff;
	readonly GlbExternalPassword_PL externalPassword;
	readonly GlbBranch branch;
	readonly ZDateTime utcNow;
	readonly ProcessingActionDelegate processingAction;

	public delegate void ProcessingActionDelegate(GlbBranch branch);

	public CusPollingTransactionTestHelper(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		this.factory = factory;
		staff = factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "asd", isActive: true, mailBox: "abc", password: "123");
		externalPassword = GlbStaffWrapper.Get(staff).GlbExternalPassword;
		branch = staff.HomeBranch;
		utcNow = ZDateTime.UtcNow.TrimSeconds();
		this.processingAction = processingAction;
	}

	public void Dispose()
	{
	}

	public static void AssertNothingHappensWhenNowAboveNextAttempt(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertNothingHappensWhenNowAboveNextAttempt();
	}

	void AssertNothingHappensWhenNowAboveNextAttempt()
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		plcTransaction.CPT_StatusTimeUtc = utcNow.AddMinutes(30);
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = plcTransaction.CPT_StatusTimeUtc.AddHours(1);

		factory.Save();
		processingAction(branch);

		CombineAssertions(() =>
		{
			AssertEquals("Status not changed", CusPollingTransactionStatuses.OPN, plcTransaction.CPT_Status);
			AssertEquals("No backlog created", 0, plcTransaction.GetBacklogs().Length);
			AssertEquals("No messages created", 0, plcTransaction.GetMessages().Length);
		});
	}

	public static void AssertMessageSentWhenNowEqualToNextAttempt(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertMessageSentWhenNowEqualToNextAttempt();
	}

	void AssertMessageSentWhenNowEqualToNextAttempt()
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		plcTransaction.CPT_StatusTimeUtc = utcNow.AddHours(-1);
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = utcNow;
		factory.Save();

		processingAction(branch);

		plcTransaction.Reload();
		CombineAssertions(() =>
		{
			AssertEquals("Status changed to AWR", CusPollingTransactionStatuses.AWR, plcTransaction.CPT_Status);
			AssertEquals("No backlog created", 0, plcTransaction.GetBacklogs().Length);
			var messages = plcTransaction.GetMessages();
			AssertEquals("One message created", 1, messages.Length);
			var message = messages.Single();
			AssertMessageCreatedForCusPollingTransaction("PLC", plcTransaction, message, branch.PK,
				expectedDateOdUTC: plcTransaction.CPT_StatusTimeUtc.TrimSeconds().ToDateTime(),
				expectedDateDoUTC: (plcTransaction.CPT_EarliestTimeOfNextAttemptUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry).TrimSeconds().ToDateTime());
		});
	}

	public static void AssertMessageSentWhenNowBelowNextAttempt(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertMessageSentWhenNowBelowNextAttempt();
	}

	void AssertMessageSentWhenNowBelowNextAttempt()
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		plcTransaction.CPT_StatusTimeUtc = utcNow.AddMinutes(-90);
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-30);
		factory.Save();

		processingAction(branch);

		plcTransaction.Reload();
		CombineAssertions(() =>
		{
			AssertEquals("Status changed to AWR", CusPollingTransactionStatuses.AWR, plcTransaction.CPT_Status);
			AssertEquals("No backlog created", 0, plcTransaction.GetBacklogs().Length);
			var messages = plcTransaction.GetMessages();
			AssertEquals("One message created", 1, messages.Length);
			var message = messages.Single();
			AssertMessageCreatedForCusPollingTransaction("PLC", plcTransaction, message, branch.PK,
				expectedDateOdUTC: plcTransaction.CPT_StatusTimeUtc.TrimSeconds().ToDateTime(),
				expectedDateDoUTC: (plcTransaction.CPT_EarliestTimeOfNextAttemptUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry).TrimSeconds().ToDateTime());
		});
	}

	public static void AssertBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour();
	}

	void AssertBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour()
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		var originalStatusTimeUtc = utcNow.AddMinutes(-210);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-150);
		plcTransaction.CPT_StatusTimeUtc = originalStatusTimeUtc;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;
		factory.Save();

		processingAction(branch);

		plcTransaction.Reload();
		CombineAssertions(() =>
		{
			var backlogs = AssertBacklogsCreatedFromPlcAndPlcUpdated("Blg created", plcTransaction, originalStatusTimeUtc);
			foreach (var (transaction, index) in backlogs.Append(plcTransaction).Select((x, i) => (x, i)))
			{
				AssertEquals($"Transaction [{index}] Type={transaction.CPT_Type} CPT_Status", CusPollingTransactionStatuses.AWR, transaction.CPT_Status);
				var messages = transaction.GetMessages();
				AssertEquals("One message created", 1, messages.Length);
				var message = messages.Single();
				var dataOdUTC = transaction.CPT_StatusTimeUtc.TrimSeconds().ToDateTime();
				var dataDoUTC = (transaction.CPT_Type == CusPollingTransactionTypes.PLC
					? transaction.CPT_EarliestTimeOfNextAttemptUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry
					: transaction.CPT_EarliestTimeOfNextAttemptUtc).TrimSeconds().ToDateTime();
				AssertMessageCreatedForCusPollingTransaction($"Transaction [{index}] Type={transaction.CPT_Type} message", transaction, message, branch.PK, dataOdUTC, dataDoUTC);
			}
		});
	}

	public static void AssertTransactionInterchangeReceived(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertTransactionInterchangeReceived();
	}

	void AssertTransactionInterchangeReceived()
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		var originalStatusTimeUtc = utcNow.AddMinutes(-210);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-150);
		plcTransaction.CPT_StatusTimeUtc = originalStatusTimeUtc;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;

		factory.Save();

		processingAction(branch);
		plcTransaction.Reload();

		var backlogs = plcTransaction.GetBacklogs();
		AssertEquals("Backlogs count", 3, backlogs.Length);
		backlogs.ForEach(x =>
		{
			x.CPT_StatusTimeUtc = utcNow.AddMinutes(-1);
			x.CPT_EarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-2);
		});

		CombineAssertions(() =>
		{
			var transactionWithMessageList = new List<(CusPollingTransaction Transaction, EDIMessage Message)>();
			foreach (var (transaction, index) in backlogs.Append(plcTransaction).Select((x, i) => (x, i)))
			{
				AssertEquals($"Transaction [{index}] Type={transaction.CPT_Type} CPT_Status", CusPollingTransactionStatuses.AWR, transaction.CPT_Status);
				var messages = transaction.GetMessages();
				AssertEquals("One message created", 1, messages.Length);
				var message = messages.Single();
				transactionWithMessageList.Add((transaction, message));
			}
			AssertEquals("Transaction with paired message count", 4, transactionWithMessageList.Count);

			var transmittedInterchange0 = CreateOutboundInterchange(transactionWithMessageList[0].Message);
			transmittedInterchange0.EI_Status = InterchangeStatus.Sent;
			var receivedInterchange0 = CreateInboundInterchange(transmittedInterchange0);
			receivedInterchange0.EI_Status = InterchangeStatus.Received;
			var transmittedInterchange1 = CreateOutboundInterchange(transactionWithMessageList[1].Message);
			transmittedInterchange1.EI_Status = InterchangeStatus.Sent;
			var transmittedInterchange2 = CreateOutboundInterchange(transactionWithMessageList[3].Message);
			transmittedInterchange2.EI_Status = InterchangeStatus.Sent;
			var receivedInterchange2 = CreateInboundInterchange(transmittedInterchange2);
			receivedInterchange2.EI_Status = InterchangeStatus.Received;

			factory.Save();

			processingAction(branch);

			plcTransaction.Reload();
			backlogs.ForEach(x => x.Reload());
			AssertEquals("Received BLG interchange Closed", CusPollingTransactionStatuses.CLS, transactionWithMessageList[0].Transaction.CPT_Status);
			AssertEquals("Not received BLG interchange still awaiting result", CusPollingTransactionStatuses.AWR, transactionWithMessageList[1].Transaction.CPT_Status);
			AssertEquals("BLG without interchange still awaiting result", CusPollingTransactionStatuses.AWR, transactionWithMessageList[2].Transaction.CPT_Status);
			AssertEquals("Received PLC interchange reopened", CusPollingTransactionStatuses.OPN, plcTransaction.CPT_Status);
			AssertEquals("Received PLC new StatusTime", utcNow, plcTransaction.CPT_StatusTimeUtc);
			AssertEquals("Received PLC new EarliestTimeOfNextAttempt", utcNow.AddMinutes(5), plcTransaction.CPT_EarliestTimeOfNextAttemptUtc);
		});
	}

	public static void AssertTransactionInterchangeFailed(BusinessObjectFactory factory, ProcessingActionDelegate processingAction, ZString failedInterchangeStatus)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertTransactionInterchangeFailed(failedInterchangeStatus);
	}

	void AssertTransactionInterchangeFailed(ZString failedInterchangeStatus)
	{
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, status: CusPollingTransactionStatuses.OPN, type: CusPollingTransactionTypes.PLC);
		var originalStatusTimeUtc = utcNow.AddMinutes(-210);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-150);
		plcTransaction.CPT_StatusTimeUtc = originalStatusTimeUtc;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;
		factory.Save();

		processingAction(branch);
		plcTransaction.Reload();

		var backlogs = plcTransaction.GetBacklogs();
		AssertEquals("Backlogs count", 3, backlogs.Length);
		backlogs.Append(plcTransaction).ForEach(x =>
		{
			x.CPT_StatusTimeUtc = utcNow.AddMinutes(-1);
			x.CPT_EarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-2);
		});

		CombineAssertions(() =>
		{
			var transactionWithMessageList = new List<(CusPollingTransaction Transaction, EDIMessage Message, ZByte OriginalNumberOfAttempts)>();
			foreach (var (transaction, index) in backlogs.Append(plcTransaction).Select((x, i) => (x, i)))
			{
				AssertEquals($"Transaction [{index}] Type={transaction.CPT_Type} CPT_Status", CusPollingTransactionStatuses.AWR, transaction.CPT_Status);
				var messages = transaction.GetMessages();
				AssertEquals("One message created", 1, messages.Length);
				var message = messages.Single();
				transactionWithMessageList.Add((transaction, message, transaction.CPT_NumberOfAttempts));
			}
			AssertEquals("Transaction with paired message count", 4, transactionWithMessageList.Count);

			var transmittedInterchange0 = CreateOutboundInterchange(transactionWithMessageList[0].Message);
			transmittedInterchange0.EI_Status = failedInterchangeStatus;
			var transmittedInterchange1 = CreateOutboundInterchange(transactionWithMessageList[1].Message);
			transmittedInterchange1.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			var receivedInterchange1 = CreateInboundInterchange(transmittedInterchange1);
			receivedInterchange1.EI_Status = failedInterchangeStatus;
			transactionWithMessageList[1].Transaction.CPT_NumberOfAttempts = (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value;
			var transmittedInterchange2 = CreateOutboundInterchange(transactionWithMessageList[3].Message);
			transmittedInterchange2.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			var receivedInterchange2 = CreateInboundInterchange(transmittedInterchange2);
			receivedInterchange2.EI_Status = failedInterchangeStatus;
			originalStatusTimeUtc = transactionWithMessageList[3].Transaction.CPT_StatusTimeUtc;
			originalEarliestTimeOfNextAttemptUtc = transactionWithMessageList[3].Transaction.CPT_EarliestTimeOfNextAttemptUtc;
			factory.Save();

			processingAction(branch);

			transactionWithMessageList.ForEach(x => {
				x.Transaction.Reload();
				x.Message.Reload();
			});
			const string expectedFaultLog = "Interchange sent failed";
			var testItem = transactionWithMessageList[0];
			AssertBacklogFaultResult("BLG with default CPT_NumberOfAttempts", testItem.Transaction, testItem.Message, expectedFaultLog, testItem.OriginalNumberOfAttempts);
			testItem = transactionWithMessageList[1];
			AssertBacklogFaultResult("BLG with max CPT_NumberOfAttempts", testItem.Transaction, testItem.Message, expectedFaultLog, (ZByte)PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value);
			AssertEquals("Not received BLG interchange still awaiting result", CusPollingTransactionStatuses.AWR, transactionWithMessageList[2].Transaction.CPT_Status);
			testItem = transactionWithMessageList[3];
			AssertPlcFaultResult("PLC", testItem.Transaction, backlogs, testItem.Message, expectedFaultLog, originalStatusTimeUtc, originalEarliestTimeOfNextAttemptUtc);
		});
	}

	public static void AssertPlcTimeout(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertPlcTimeout();
	}

	void AssertPlcTimeout()
	{
		RegistryTester.SetValue(PLCustomsDataRegistry.Instance.PUESCRequestTimeOut, 10);
		var plcTransaction = factory.CreatePollingTransactionForStaff(staff, CusPollingTransactionStatuses.AWR, type: CusPollingTransactionTypes.PLC);
		var originalStatusTimeUtc = utcNow.AddMinutes(-210);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-150);
		plcTransaction.CPT_StatusTimeUtc = originalStatusTimeUtc;
		plcTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;
		factory.Save();

		EDIMessage message;
		EDIInterchange interchange;
		using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
		{
			var cusPollingTransactionMessageSender = new CusPollingTransactionMessageSender(factory);
			message = cusPollingTransactionMessageSender.SendInContext(plcTransaction);
			interchange = CreateOutboundInterchange(message);
			interchange.EI_Status = EDIInterchange.Status.Queued;
			factory.Save();
		}

		processingAction(branch);
		plcTransaction.Reload();
		message.Reload();

		CombineAssertions(() =>
		{
			var backlogs = plcTransaction.GetBacklogs();
			AssertEquals("Backlog created", 1, backlogs.Length);
			AssertEquals("reopened", CusPollingTransactionStatuses.OPN, plcTransaction.CPT_Status);
			AssertEquals("CPT_StatusTimeUtc", originalEarliestTimeOfNextAttemptUtc, plcTransaction.CPT_StatusTimeUtc);
			AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", utcNow , plcTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			AssertEquals("Message status failed", EDIMessage.Status.Failed, message.EM_Status);

			const string expectedLog = "The response timeout has expired; the total waiting time was 3 hours, 30 minutes.";
			message.AssertHasLogMessagePart("message log", Events.ErrorReport, expectedLog);
			plcTransaction.AssertHasLogMessagePart("transaction log", Events.ErrorReport, expectedLog);
		});
	}

	public static void AssertBlgTimeout(BusinessObjectFactory factory, ProcessingActionDelegate processingAction)
	{
		using var instance = new CusPollingTransactionTestHelper(factory, processingAction);
		instance.AssertBlgTimeout();
	}

	void AssertBlgTimeout()
	{
		RegistryTester.SetValue(PLCustomsDataRegistry.Instance.PUESCRequestTimeOut, 10);
		var blgTransaction = factory.CreatePollingTransactionForStaff(staff, CusPollingTransactionStatuses.AWR, type: CusPollingTransactionTypes.BLG);
		var originalStatusTimeUtc = utcNow.AddMinutes(-210);
		var originalEarliestTimeOfNextAttemptUtc = utcNow.AddMinutes(-150);
		blgTransaction.CPT_StatusTimeUtc = originalStatusTimeUtc;
		blgTransaction.CPT_EarliestTimeOfNextAttemptUtc = originalEarliestTimeOfNextAttemptUtc;
		factory.Save();

		EDIMessage message;
		using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
		{
			var cusPollingTransactionMessageSender = new CusPollingTransactionMessageSender(factory);
			message = cusPollingTransactionMessageSender.SendInContext(blgTransaction);
			factory.Save();
		}

		processingAction(branch);
		blgTransaction.Reload();
		message.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("CPT_NumberOfAttempts increased", (ZByte)1, blgTransaction.CPT_NumberOfAttempts);
			AssertEquals("Message status failed", EDIMessage.Status.Failed, message.EM_Status);

			const string expectedLog = "The response timeout has expired; the total waiting time was 3 hours, 30 minutes.";
			message.AssertHasLogMessagePart("message log", Events.ErrorReport, expectedLog);
			blgTransaction.AssertHasLogMessagePart("transaction log", Events.ErrorReport, expectedLog);
		});
	}

	internal static void AssertMessageCreatedForCusPollingTransaction(string descriptionPrefix, CusPollingTransaction transaction, EDIMessage sentMessage, ZGuid branchPK, DateTime expectedDateOdUTC, DateTime expectedDateDoUTC)
	{
		AssertEquals($"{descriptionPrefix}: EM_IsActive", ZBool.True, sentMessage.EM_IsActive);
		AssertEquals($"{descriptionPrefix}: EM_IsTestMessage", ZBool.False, sentMessage.EM_IsTestMessage);
		AssertEquals($"{descriptionPrefix}: EM_ApplicationCode", "PLC", sentMessage.EM_ApplicationCode);
		AssertEquals($"{descriptionPrefix}: EM_MessageType", "CPT", sentMessage.EM_MessageType);
		AssertEquals($"{descriptionPrefix}: EM_MessageSubType", "CPT", sentMessage.EM_MessageSubType);
		AssertEquals($"{descriptionPrefix}: EM_ReceiveTransmit", "TRX", sentMessage.EM_ReceiveTransmit);
		AssertEquals($"{descriptionPrefix}: EM_SendWithMessageErrors", ZBool.False, sentMessage.EM_SendWithMessageErrors);
		AssertEquals($"{descriptionPrefix}: EM_LinkTable", "CusPollingTransaction", sentMessage.EM_LinkTable);
		AssertEquals($"{descriptionPrefix}: EM_LinkUniqueID", transaction.PK, sentMessage.EM_LinkUniqueID);
		AssertEquals($"{descriptionPrefix}: EM_GP", transaction.CPT_TransactionID, sentMessage.EM_GP.ToString());
		AssertEquals($"{descriptionPrefix}: EM_GB", branchPK, sentMessage.EM_GB);

		var polandUNLOCO = new RefUNLOCO.Loader(sentMessage.Factory).Load(Constants.PolishTimeZoneUnloco);
		var plTimeZone = polandUNLOCO.TimeZoneSet.GetCalculationTimeZone();
		var expectedXml = $"""
			<GetDocumentsRequest xmlns="http://www.mf.gov.pl/uslugiBiznesowe/WsPull/Usluga/2014/01_v2_0">
				<pobrany>0</pobrany>
				<dataOd>{UTCToPLLocalTime(expectedDateOdUTC).ToISO8601String()}</dataOd>
				<dataDo>{UTCToPLLocalTime(expectedDateDoUTC).ToISO8601String()}</dataDo>
				<allEmployees>true</allEmployees>
			</GetDocumentsRequest>
			""";
		Assert($"{descriptionPrefix}: EM_MessageText", XmlReaderComparer.CompareElementsOrderSensitive(expectedXml, sentMessage.EM_MessageText));

		ZDateTime UTCToPLLocalTime(ZDateTime utcZDateTime) => plTimeZone.ToLocalTime(utcZDateTime.ToDateTime());
	}

	internal static CusPollingTransaction[] AssertBacklogsCreatedFromPlcAndPlcUpdated(string descriptionPrefix, CusPollingTransaction plcTransaction, ZDateTime originalStatusTimeUtc)
	{
		var backlogs = plcTransaction.GetBacklogs();

		var utcNow = ZDateTime.UtcNow.TrimSeconds();
		var expectedBacklogsCount = (utcNow.AddSeconds(-1) - originalStatusTimeUtc).Hours;
		AssertEquals($"{descriptionPrefix}: Expected created backlogs count", expectedBacklogsCount, backlogs.Length);

		if (expectedBacklogsCount <= 0)
		{
			return backlogs;
		}

		var expectedStatusTime = plcTransaction.CPT_StatusTimeUtc;
		var oneHourTimeSpan = new TimeSpan(days: 0, hours: 1, minutes: 0, seconds: 0);
		var backlogIndex = 0;

		while (utcNow - expectedStatusTime > oneHourTimeSpan)
		{
			var blgTransaction = backlogs[backlogIndex];
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_ApplicationCode", ApplicationCodes.PLCustoms, blgTransaction.CPT_ApplicationCode);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_Type", CusPollingTransactionTypes.BLG, blgTransaction.CPT_Type);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_StatusReason", ZString.Empty, blgTransaction.CPT_StatusReason);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_StatusTimeUtc", expectedStatusTime, blgTransaction.CPT_StatusTimeUtc);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_EarliestTimeOfNextAttemptUtc", expectedStatusTime.AddHours(1), blgTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_NumberOfAttempts", (ZByte)0, blgTransaction.CPT_NumberOfAttempts);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_ParentTableCode", GlbExternalPasswordSchema.Constants.Prefix, blgTransaction.CPT_ParentTableCode);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_ParentID", plcTransaction.CPT_ParentID, blgTransaction.CPT_ParentID);
			AssertEquals($"{descriptionPrefix}: [{backlogIndex}] CPT_TransactionID", plcTransaction.CPT_TransactionID, blgTransaction.CPT_TransactionID);

			expectedStatusTime += oneHourTimeSpan;
			backlogIndex++;
		}

		AssertEquals($"{descriptionPrefix}: PLC CPT_StatusTimeUtc", expectedStatusTime, plcTransaction.CPT_StatusTimeUtc);
		var expectedEarliestTimeOfNextAttemptUtc = utcNow - expectedStatusTime < Constants.CusPollingTransaction.TimeBufferFor1stRetry
			? plcTransaction.CPT_StatusTimeUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry
			: utcNow;
		AssertEquals($"{descriptionPrefix}: PLC CPT_EarliestTimeOfNextAttemptUtc", expectedEarliestTimeOfNextAttemptUtc, plcTransaction.CPT_EarliestTimeOfNextAttemptUtc);

		return backlogs;
	}

	public static void AssertBacklogFaultResult(
		string descriptionPrefix,
		CusPollingTransaction blgTransaction,
		EDIMessage message,
		string expectedFaultLog,
		ZByte originalNumberOfAttempts)
	{
		blgTransaction.AssertHasExactLogMessage($"{descriptionPrefix}: Transaction fault log", Events.ErrorReport, expectedFaultLog);
		message.AssertHasExactLogMessage($"{descriptionPrefix}: Transmit message log", Events.ErrorReport, expectedFaultLog);

		if (originalNumberOfAttempts < PLCustomsDataRegistry.Instance.PUESCSendMaxRetryCount.Value)
		{
			AssertEquals($"{descriptionPrefix}: CPT_NumberOfAttempts", originalNumberOfAttempts + 1, blgTransaction.CPT_NumberOfAttempts);

			var newMessages = blgTransaction.GetMessages().Where(x => x.PK != message.PK).ToArray();
			AssertEquals($"{descriptionPrefix}: new message count", 1, newMessages.Length);
			var createdMessageCopy = newMessages.Single();
			AssertMessageCopy(descriptionPrefix, createdMessageCopy, message, blgTransaction);
		}
		else
		{
			AssertEquals($"{descriptionPrefix}: CPT_Status", CusPollingTransactionStatuses.ERR, blgTransaction.CPT_Status);
		}
	}

	public static void AssertPlcFaultResult(
		string descriptionPrefix,
		CusPollingTransaction plcTransaction,
		CusPollingTransaction[] originalBacklogs,
		EDIMessage message,
		string expectedFaultLog,
		ZDateTime originalPlcStatusTime,
		ZDateTime originalPlcNextAttempt)
	{
		plcTransaction.AssertHasExactLogMessage($"{descriptionPrefix}: Transaction fault log", Events.ErrorReport, expectedFaultLog);
		message.AssertHasExactLogMessage($"{descriptionPrefix}: Transmit message log", Events.ErrorReport, expectedFaultLog);

		var newBacklogs = plcTransaction.GetBacklogs().Where(x => originalBacklogs.All(backlog => backlog.PK != x.PK)).ToArray();
		AssertEquals($"{descriptionPrefix}: Created single backlog", 1, newBacklogs.Length);
		var newBacklog = newBacklogs.Single();
		AssertEquals($"{descriptionPrefix}: CPT_NumberOfAttempts", (ZByte)1, newBacklog.CPT_NumberOfAttempts);
		AssertEquals($"{descriptionPrefix}: CPT_StatusTimeUtc", originalPlcStatusTime, newBacklog.CPT_StatusTimeUtc);
		AssertEquals($"{descriptionPrefix}: CPT_EarliestTimeOfNextAttemptUtc", originalPlcNextAttempt, newBacklog.CPT_EarliestTimeOfNextAttemptUtc);

		var newMessages = newBacklog.GetMessages();
		AssertEquals($"{descriptionPrefix}: new message count", 1, newMessages.Length);
		var createdMessageCopy = newMessages.Single();
		AssertMessageCopy(descriptionPrefix, createdMessageCopy, message, newBacklog);
		message.AssertHasExactLogMessage($"{descriptionPrefix}: original message log", Events.ErrorReport, $"Message copy generated for backlog {newBacklog.PK}");

		AssertEquals($"{descriptionPrefix}: reopened", CusPollingTransactionStatuses.OPN, plcTransaction.CPT_Status);
		AssertEquals($"{descriptionPrefix}: CPT_StatusTimeUtc", originalPlcNextAttempt, plcTransaction.CPT_StatusTimeUtc);
		var utcNow = ZDateTime.UtcNow.TrimSeconds();
		var expectedEarliestTimeOfNextAttemptUtc = utcNow - plcTransaction.CPT_StatusTimeUtc < Constants.CusPollingTransaction.TimeBufferFor1stRetry
			? plcTransaction.CPT_StatusTimeUtc + Constants.CusPollingTransaction.TimeBufferFor1stRetry
			: utcNow;
		AssertEquals($"{descriptionPrefix}: CPT_EarliestTimeOfNextAttemptUtc", expectedEarliestTimeOfNextAttemptUtc, plcTransaction.CPT_EarliestTimeOfNextAttemptUtc);
	}

	public static void EmulateXtProcessing(string loginName)
	{
		var factory = new BusinessObjectFactory { RefreshEnabled = false };

		using var userContext = Env.SetTemporaryUserContext(loginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
		var query = new ZQuery { OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending }
			.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodes.PLCustoms)
			.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
			.AddToFilter(EDIInterchangeSchema.EI_Status, new[] { InterchangeStatus.Queued });
		var interchanges = factory.Load<EDIInterchange>(query);

		foreach (var interchange in interchanges)
		{
			interchange.EI_Status = InterchangeStatus.Sent;
		}

		factory.Save();
	}

	static void AssertMessageCopy(string descriptionPrefix, EDIMessage messageCopy, EDIMessage originalMessage, CusPollingTransaction blgTransaction)
	{
		var utcNow = ZDateTime.UtcNow.TrimSeconds();
		AssertEquals($"{descriptionPrefix}: EM_Status", EDIMessageStatuses.Queued, messageCopy.EM_Status);
		AssertEquals($"{descriptionPrefix}: EM_RetryCount", blgTransaction.CPT_NumberOfAttempts, messageCopy.EM_RetryCount);
		AssertEquals($"{descriptionPrefix}: EM_HeldUntilDate", messageCopy.EM_HeldUntilDate, utcNow.Add(GetHeldUntilDateBuffer()));
		AssertEquals($"{descriptionPrefix}: EM_IsActive", ZBool.True, messageCopy.EM_IsActive);
		AssertEquals($"{descriptionPrefix}: EM_IsTestMessage", originalMessage.EM_IsTestMessage, messageCopy.EM_IsTestMessage);
		AssertEquals($"{descriptionPrefix}: EM_GP", originalMessage.EM_GP, messageCopy.EM_GP);
		AssertEquals($"{descriptionPrefix}: EM_LinkUniqueID", "CusPollingTransaction", messageCopy.EM_LinkTable);
		AssertEquals($"{descriptionPrefix}: EM_LinkUniqueID", blgTransaction.PK, messageCopy.EM_LinkUniqueID);
		AssertEquals($"{descriptionPrefix}: EM_MessageText", originalMessage.EM_MessageText, messageCopy.EM_MessageText);
		AssertEquals($"{descriptionPrefix}: EM_SendWithMessageErrors", originalMessage.EM_SendWithMessageErrors, messageCopy.EM_SendWithMessageErrors);
		AssertEquals($"{descriptionPrefix}: EM_ApplicationCode", originalMessage.EM_ApplicationCode, messageCopy.EM_ApplicationCode);
		AssertEquals($"{descriptionPrefix}: EM_ApplicationReference", originalMessage.EM_ApplicationReference, messageCopy.EM_ApplicationReference);
		AssertEquals($"{descriptionPrefix}: EM_MessageType", originalMessage.EM_MessageType, messageCopy.EM_MessageType);
		AssertEquals($"{descriptionPrefix}: EM_MessageSubType", originalMessage.EM_MessageSubType, messageCopy.EM_MessageSubType);
		AssertEquals($"{descriptionPrefix}: EM_MessageOwner", originalMessage.EM_MessageOwner, messageCopy.EM_MessageOwner);
		AssertEquals($"{descriptionPrefix}: EM_ReceiveTransmit", originalMessage.EM_ReceiveTransmit, messageCopy.EM_ReceiveTransmit);
		AssertEquals($"{descriptionPrefix}: EM_GB", originalMessage.EM_GB, messageCopy.EM_GB);
		AssertEquals($"{descriptionPrefix}: EM_GE", originalMessage.EM_GE, messageCopy.EM_GE);

		TimeSpan GetHeldUntilDateBuffer() => (byte)blgTransaction.CPT_NumberOfAttempts switch
		{
			1 => Constants.CusPollingTransaction.TimeBufferFor1stRetry,
			2 => Constants.CusPollingTransaction.TimeBufferFor2ndRetry,
			3 or 4 => Constants.CusPollingTransaction.TimeBufferFor3rdOr4thRetry,
			_ => Constants.CusPollingTransaction.TimeBufferForOtherRetry
		};
	}

	EDIInterchange CreateOutboundInterchange(EDIMessage message)
	{
		message.EM_Status = EDIMessage.Status.Sent;

		var result = factory.New<EDIInterchange>();
		result.EI_ApplicationCode = ApplicationCodes.PLCustoms;
		result.EI_IsActive = ZBool.True;
		result.EI_InterchangeType = "IMP";
		result.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		result.EI_From = "WTLDPLDPL";
		result.EI_To = "PLCustomsPUESCTest";
		result.EI_Priority = "HGH";
		result.EI_Status = InterchangeStatus.Queued;
		result.EI_SessionGUID = ZGuid.NewZGuid();
		result.EI_HeaderText = "{\"custom.PL.User\":\"\",\"custom.PL.Password\":\"\"}";
		result.EI_BodyText = "TEST";
		result.EI_GB = branch.PK;
		result.EI_TransportType = "XTT";
		result.EI_GP = externalPassword.PK;
		message.EM_EI = result.PK;

		return result;
	}

	EDIInterchange CreateInboundInterchange(EDIInterchange message)
	{
		var result = factory.New<EDIInterchange>();
		result.EI_ApplicationCode = message.EI_ApplicationCode;
		result.EI_IsActive = ZBool.True;
		result.EI_InterchangeType = message.EI_InterchangeType;
		result.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		result.EI_From = message.EI_To;
		result.EI_To = message.EI_From;
		result.EI_Priority = message.EI_Priority;
		result.EI_Status = InterchangeStatus.Queued;
		result.EI_SessionGUID = message.EI_SessionGUID;
		result.EI_BodyText = "TEST";
		result.EI_GB = message.EI_GB;
		result.EI_TransportType = message.EI_TransportType;

		return result;
	}
}
