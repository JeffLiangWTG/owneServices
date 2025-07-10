using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	sealed class PortbaseImportNotificationMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageAmendment_NotAccepted()
		{
			var consol = CreateConsol();

			var builder = new PortbaseImportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			messageInstructions.SetupGet(m => m.DocumentName).Returns("Import Notification");

			var extensions = new PortbaseImportNotificationMessagingExtensions(consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals(false, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received a response to the previous message.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_Accepted()
		{
			var consol = CreateConsol();
			var builder = new PortbaseImportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			messageInstructions.SetupGet(m => m.DocumentName).Returns("Import Notification");

			var extensions = new PortbaseImportNotificationMessagingExtensions(consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received a response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_HasBeenRecepted()
		{
			var consol = CreateConsol();
			var builder = new PortbaseImportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			CreateLog(documentData,
				Events.MessageRejected,
				new ZDateTime(2021, 05, 22),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, "Import Notification"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			messageInstructions.SetupGet(m => m.DocumentName).Returns("Import Notification");

			var extensions = new PortbaseImportNotificationMessagingExtensions(consol, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received a response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithResetToOriginal()
		{
			var consol = CreateConsol();

			var builder = new PortbaseImportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ""),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase));

			dynamicData.SetupGet(d => d.Value).Returns(data);
			messageInstructions.SetupGet(m => m.DocumentName).Returns("Import Notification");

			var extensions = new PortbaseImportNotificationMessagingExtensions(consol, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals(null, res);
		}

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = "PortbaseImportNotification";

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();

			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_BookingReference = "BKG123";

			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NLRTM";

			var ctoAddress = Factory.New<OrgHeader>();
			ctoAddress.OH_FullName = "CTO";
			ctoAddress.OH_Code = "TestOrg";
			ctoAddress.MainAddress.OA_RN_NKCountryCode = "NL";
			ctoAddress.MainAddress.CustomsCodes.AddNew(OrgCusCode.NetherlandsCodeTypes.FenexLocationCode, "D3039R56", Core.Constants.CountryCodes.Netherlands);
			consol.JK_OA_ArrivalCTOAddress = ctoAddress.MainAddress.PK;

			return consol;
		}
	}
}
