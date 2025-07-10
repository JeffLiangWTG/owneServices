using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.ZArchitecture.Business;
using Moq;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.ZA.Testing
{
	sealed class CargoDuesMessagingExtensionsTest : TestCaseWithFactory
	{
		#region ContinueWithSendingMessageAmendment

		public void TestContinueWithSendingMessageAmendment_IsAwaitingResponse()
		{
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwise, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwise, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImportQuotation, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExportQuotation, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwiseQuotation, true);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwiseQuotation, true);
		}

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		public void TestContinueWithSendingMessageWithdrawal_IsAwaitingResponse()
		{
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwise, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwise, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImportQuotation, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExportQuotation, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwiseQuotation, false);
			AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwiseQuotation, false);
		}

		#endregion

		#region ContinueWithResetToOriginal

		public void TestContinueWithResetToOriginal_IsTNPAOrderNumberNotEmpty()
		{
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImport);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExport);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwise);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwise);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesImport, DocumentNames.CargoDuesImportQuotation);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesExport, DocumentNames.CargoDuesExportQuotation);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise, DocumentNames.CargoDuesLoadCoastwiseQuotation);
			AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise, DocumentNames.CargoDuesDischargeCoastwiseQuotation);
		}

		#endregion

		#region Assert

		void AssertIsAwaitingResponseForMessagingAmendmentAndWithdrawal(string dataStoreName, string documentName, bool isMessageAmendment)
		{
			var consol = Factory.New<ForwardingConsol>();
			var doucmentData = PrepareDocumentData(consol, dataStoreName);
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var cargoDues = new CargoDues(nameof(ForwardingConsol), consol.JK_UniqueConsignRef, DataContext.ShippingInstruction)
			{
				IsQuotationDocument = documentName.EndsWith("(Quotation)", StringComparison.Ordinal),
				TNPAOrderNumber = ZString.Empty
			};

			document.Setup(d => d.Data.Value).Returns(cargoDues);
			document.Setup(d => d.Name).Returns(documentName);

			var parameters = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			CreateEvent(doucmentData, Events.MessageSent, new ZDateTime(2022, 04, 01), parameters);

			var extensions = new CargoDuesMessagingExtensions(document.Object, consol);

			AssertEquals(2, doucmentData.CalculateDataVersion(documentName, false));

			var message = "A message has previously been sent and is awaiting reply from TNPA. Once TNPA reply, the TNPA Order Number will be populated here and then you can send further messages - either resend (as an amendment) or Withdraw/Cancel.";
			var comfirmation = "Confirmation";

			if (cargoDues.IsQuotationDocument)
			{
				AssertEquals(null, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);
			}
			else
			{
				AssertEquals(false, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);
			}

			CreateEvent(doucmentData, Events.MessageAccepted, new ZDateTime(2022, 04, 02), parameters);
			CreateEvent(doucmentData, Events.MessageWithdrawCancelRequest, new ZDateTime(2022, 04, 03), parameters);

			AssertEquals(2, doucmentData.CalculateDataVersion(documentName, false));

			notifications = new Mock<IUserNotifications>();

			if (cargoDues.IsQuotationDocument)
			{
				AssertEquals(null, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);
			}
			else
			{
				AssertEquals(false, isMessageAmendment ? extensions.ContinueWithSendingMessageAmendment(notifications.Object) : extensions.ContinueWithSendingMessageWithdrawal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);
			}
		}

		void AssertIsTNPAOrderNumberNotEmptyForResettingToOriginal(string dataStoreName, string documentName)
		{
			var consol = Factory.New<ForwardingConsol>();
			var doucmentData = PrepareDocumentData(consol, dataStoreName);
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var cargoDues = new CargoDues(nameof(ForwardingConsol), consol.JK_UniqueConsignRef, DataContext.ShippingInstruction)
			{
				IsQuotationDocument = documentName.EndsWith("(Quotation)", StringComparison.Ordinal),
				TNPAOrderNumber = ZString.Empty
			};

			document.Setup(d => d.Data.Value).Returns(cargoDues);
			document.Setup(d => d.Name).Returns(documentName);

			CreateEvent(doucmentData, Events.MessageSent, new ZDateTime(2022, 04, 15), new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName));

			var extensions = new CargoDuesMessagingExtensions(document.Object, consol);

			AssertEquals(2, doucmentData.CalculateDataVersion(documentName, false));

			var message = "This function is not possible because TNPA have already responded to your original message and provided their Order No.";
			var comfirmation = "Confirmation";

			if (cargoDues.IsQuotationDocument)
			{
				AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);
			}
			else
			{
				AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);
			}

			notifications = new Mock<IUserNotifications>();

			cargoDues.TNPAOrderNumber = "TNPANO";

			if (cargoDues.IsQuotationDocument)
			{
				AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Never);
			}
			else
			{
				AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, comfirmation), Times.Once);
			}
		}

		#endregion

		#region Implementation

		void CreateEvent(IVisualizerDocumentData documentData, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			(documentData as IStmALogParent).Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);

			Factory.Save();
			Thread.Sleep(1);
		}

		IVisualizerDocumentData PrepareDocumentData(ForwardingConsol consol, string documentDataStoreName)
		{
			var documentData = consol.Factory.New<VisualizerDocumentData>();

			using (documentData.SuspendSettingHasChanges())
			{
				documentData.Parent = consol;
				documentData.JDD_Name = documentDataStoreName;
			}

			return documentData;
		}

		#endregion
	}
}
