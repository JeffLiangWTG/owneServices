using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class FinalManifestMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageAmendment_NotAccepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_Accepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.DOSExport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.DOSExport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal_NotAccepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_Accepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_NotAccepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can be reset as no response message(MAA) received", null, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_Accepted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new FinalManifestBuilder(consol, new DummyDocDataObjectParameters());
			var finalManifest = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(finalManifest);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.FinalContainerManifestLDE);

			var extensions = new FinalManifestMessagingExtensions(document.Object, consol, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can not reset when MAA event received.", false, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Once);
		}

		#region Implementation

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.FinalContainerManifestLDE;

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#endregion
	}
}
