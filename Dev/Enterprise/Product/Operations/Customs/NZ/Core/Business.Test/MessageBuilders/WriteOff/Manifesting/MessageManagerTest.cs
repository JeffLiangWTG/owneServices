using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader;
using TestManifestCreator = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing.TestManifestCreator;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(MessageManager))]
	public class MessageManagerTest : ECIWriteOff.Testing.MessageManagerTest
	{
		public override void TestCanQueueForManifesting()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader);
			JobDeclaration declaration = manifestCreator.AddDeclaration();
			MessageManager messageManager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);

			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);

			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			AssertEquals("MessageManager.CanQueueForManifesting", false, messageManager.CanQueueForManifesting);
		}

		public void TestIsOKToSendWithMessagingErrors()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			Factory.Save();
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader);
			JobDeclaration declaration1 = manifestCreator.AddDeclaration();
			declaration1.MessageInitiator = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration();
			declaration2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);
			MessageManager messageManager = new MessageManager(declaration1, MessageManager.OperationType.SubmitMessage);
			AssertEquals("MessageManager.IsOKToSendWithMessagingErrors()", true, messageManager.IsOKToSendWithMessagingErrors());
			AssertMultilineEquals("Should have messaging errors", ExpectedAlphabeticallyOrderedMessageErrors, GetSortedMessageErrors(declaration1.Notifications.GetMessageErrors()), '\r');
			AssertMultilineEquals("Should have messaging errors", ExpectedAlphabeticallyOrderedMessageErrors, GetSortedMessageErrors(declaration2.Notifications.GetMessageErrors()), '\r');
		}

		public void TestOriginal2ConsignmentMessageCanBeSent()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);

			MessageManager messageManager = new MessageManager(declaration1, MessageManager.OperationType.SubmitMessage);
			Assert("Should be able to Send an Original Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Original Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public void TestReplaceHeaderAndLines2ConsignmentMessageCanBeSent()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);

			entryHeader.EntryNumber = "12435687";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;

			MessageManager messageManager = new MessageManager(declaration1, MessageManager.OperationType.SubmitMessage);
			Assert("Should be able to Send a Replacement Header and Lines Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Replacement Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public void TestCancellation2ConsignmentMessageCanBeSent()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);

			entryHeader.EntryNumber = "12435687";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestAccepted;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;

			MessageManager messageManager = new MessageManager(declaration1, MessageManager.OperationType.CancelMessage);
			Assert("Should be able to Send a Cancellation Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Cancel Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		public override void TestExecuteReplaceRejectedEntry()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2005, 12, 3));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);

			entryHeader.EntryNumber = "12435687";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestRejected;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			declaration1.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			declaration2.JE_ECI_LastResponseStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;

			MessageManager messageManager = new MessageManager(declaration2, MessageManager.OperationType.SubmitMessage);
			Assert("Should be able to Send a Replacement Header and Lines Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Replacement Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration1.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration1.JE_ECI_LastResponseStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
			AssertEquals("Declaration2.JE_ECI_LastResponseStatus", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration2.JE_ECI_LastResponseStatus);
		}

		public void TestReplacementDoesNotSendConsignmentsChangedToFormalEntries()
		{
			/*
			 *	create/send an original manifest with multiple consignments
			 *	change some consignments to formal entries
			 *	re-send manifest as replacement
			 *	ensure entries that were changed to formal are not sent in the replacement message and their status is not changed.
			 */
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2020, 08, 2), new ZDateTime(2020, 08, 3));
			var declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "Paper", 10m, 4, 120m);
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "Books", 40m, 2, 570m);
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Import, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL3", "Magazines", 25m, 2, 350m);
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			entryHeader.EntryNumber = "12435687";
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.ManifestInError;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentInError;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ImportDeclarationRequired;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;

			// Change declaration2 to a formal entry
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Pre-condition: Declaration2 EntryStatus should now be set to Not Sent for formal entry", FormalEntryStatusList.Codes.NotSentToCustoms, declaration2.JE_EntryStatus);

			var additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Replace, new BusinessObjectFactory(), MessageTypeList.Codes.ICR);
			additionalMessageInformation.AM_SendManifest = true;
			var messageManager = new MessageManager(declaration1, MessageManager.OperationType.SubmitMessage, additionalMessageInformation, "GAZ");
			Assert("Should be able to Send a Replacement Header and Lines Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Replacement Manifest Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2 - Replacement Manifest Entry should not have updated this Formal entry status", FormalEntryStatusList.Codes.NotSentToCustoms, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration3.JE_EntryStatus);
		}

		public void TestExecuteICREntry()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-22222222", "QF223", "USSFO", "NZWLG", new ZDateTime(2005, 12, 2), new ZDateTime(2017, 11, 3));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "HOUSEBILL1", "RATS HEADS", 15.2m, 4, 45.54m);
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "HOUSEBILL2", "RATS TEETH", 13.4m, 2, 27.72m);
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			MessageManager messageManager = new MessageManager(declaration1, MessageManager.OperationType.SubmitMessage);
			Assert("Should be able to Send an Original TSW ICR Message", messageManager.Execute());
			AssertEquals("MessageManager.LastHumanReadableStatus", "ICR Original Entry Message " + MessageManager.MessageReportingImmediateSend, messageManager.LastHumanReadableStatus);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration2.JE_EntryStatus);
		}

		#region Overridden Tests because you cannot Reset a Manifest Entry to Original Ever
		public override void TestExecuteResetToOriginal()
		{
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManager.OperationType.ResetToOriginal);
			Assert("Should not be able to Reset a Manifest Entry to Original", !manager.Execute());
			AssertEquals("Cannot Reset to Original - You Cannot Reset a Manifest Entry to Original Once it has been Manifested", manager.LastHumanReadableStatus);
		}

		public override void TestExecuteResetToOriginalWhenNoResponseReceived()
		{
			SetDeclarationToSentToCustoms();
			Declaration.DeclarationNumber = "";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManager.OperationType.ResetToOriginal);
			Assert("Should not be able to Reset a Manifest Entry to Original", !manager.Execute());
			AssertEquals("Cannot Reset to Original - You Cannot Reset a Manifest Entry to Original Once it has been Manifested", manager.LastHumanReadableStatus);
		}

		public override void TestIsOkToExecuteCannotResetToOriginal()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.ResetToOriginal);
			Assert("Should not be able to Reset a Manifest Entry to Original", !manager.Execute());
			AssertEquals("Cannot Reset to Original - You Cannot Reset a Manifest Entry to Original Once it has been Manifested", manager.LastHumanReadableStatus);
		}

		public override void TestIsOkToExecuteResetToOriginal()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.ResetToOriginal);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Assert("Should not be able to Reset a Manifest Entry to Original", !manager.Execute());
			AssertEquals("Cannot Reset to Original - You Cannot Reset a Manifest Entry to Original Once it has been Manifested", manager.LastHumanReadableStatus);
		}

		public override void TestMessageTypeToBeSentResetToOriginal()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.ResetToOriginal);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals(MessageManager.MessageType.None, manager.MessageTypeToBeSent);
		}
		#endregion

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewManager(MessageBuilders.MessageManager.OperationType.SubmitMessage);
		}

		protected override MessageBuilders.MessageManager GetNewMessageManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return new MessageManager(Declaration, operationType);
		}

		MessageManager GetNewManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return (MessageManager)GetNewMessageManager(operationType);
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader);
			JobDeclaration declaration = manifestCreator.AddDeclaration();
			Factory.Save();
			return declaration;
		}
		#endregion
	}
}
