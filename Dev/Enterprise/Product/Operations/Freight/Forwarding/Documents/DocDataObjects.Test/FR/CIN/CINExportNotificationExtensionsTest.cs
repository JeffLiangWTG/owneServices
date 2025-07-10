using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class CINExportNotificationExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageAmendment_NotAcceptedFFM()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));
			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal));
			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(extensions.IsSendingAmendment().Value);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);
			AssertEquals("Message can be amended", true, res);
		}

		public void TestContinueWithSendingMessageAmendment_AcceptedFFM_BeforeReset()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal));

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 22),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(extensions.IsSendingAmendment().Value);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);
			AssertEquals("Message can be amended.", true, res);
		}

		public void TestContinueWithSendingMessageAmendment_AcceptedFFM_AfterReset()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal));

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 22),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 23),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(extensions.IsSendingAmendment().Value);

			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);
			AssertEquals("Message can not be amended as FFM is received.", false, res);
			notifications.Verify(x => x.ShowMessage("Amendment will no longer be accepted by CIN once FFM and departure message are received.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);
			messageInstructions.SetupGet(m => m.OrderLogsByLocalTime).Returns(false);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			notifications.Verify(x => x.ShowMessage("There's no message to withdraw.", "Sending Withdraw/Cancel Request"), Times.Once);

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);

			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received a response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Once);

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal));

			notifications = new Mock<IUserNotifications>();
			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received a response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Once);

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 22),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.DataExport,
				new ZDateTime(2021, 05, 22));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 23),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "Message accepté"));

			notifications = new Mock<IUserNotifications>();
			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 23),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification));

			notifications = new Mock<IUserNotifications>();
			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 23),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 24),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "FFM and departure message received"));

			notifications = new Mock<IUserNotifications>();
			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			notifications.Verify(x => x.ShowMessage("Withdrawal will no longer be accepted by CIN once FFM and departure message are received.", "Warning"), Times.Once);

			CreateLog(documentData,
				Events.StatusUpdated,
				new ZDateTime(2021, 05, 25),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, EventReferenceMessageTypes.ResetToOriginal));

			notifications = new Mock<IUserNotifications>();
			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			notifications.Verify(x => x.ShowMessage("Withdrawal will no longer be accepted by CIN once FFM and departure message are received.", "Warning"), Times.Once);
		}

		public void TestContinueWithResetToOriginal_NotAccepted()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can be reset as no response message(MAA) received", null, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_Accepted()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can not reset when MAA event received.", false, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Once);
		}

		public void TestIsSendingMessageAmendment_whenIRJReceived()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotification = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(exportNotification);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.CINExportNotification);

			var extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			AssertEquals(extensions.IsSendingAmendment().Value, false);

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2024, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			AssertEquals(extensions.IsSendingAmendment().Value, true);

			CreateLog(documentData,
				Events.InterchangeRejected,
				new ZDateTime(2024, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			AssertEquals(extensions.IsSendingAmendment().Value, false);

			CreateLog(documentData,
				Events.InterchangeReceiptAcknowledged,
				new ZDateTime(2024, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.CINExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			extensions = new CINExportNotificationExtensions(document.Object, shipment, messageInstructions.Object);
			AssertEquals(extensions.IsSendingAmendment().Value, true);
		}

		#region Implementation

		IVisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.CINExportNotification;

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HouseBill = "H001";
			shipment.JS_ActualWeight = 20;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_UnitOfVolume = "D3";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_NoOriginalBills = 3;
			consol.JK_NoCopyBills = 4;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "FRNCE";
			consol.JK_BookingReference = "WhiskyTreasure";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
			consol.JK_MasterBillNum = "BILLNUMBER";
			consol.JK_AgentsReference = "AGTREF";
			consol.JK_UniqueConsignRef = "CON0001";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;

			PopulateConsolAddresses(consol);

			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number1.CE_EntryNum = "MRN01";

			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number2.CE_EntryNum = "MRN02";

			var number3 = shipment.Numbers.AddNew();
			number3.CE_EntryType = "COC";
			number3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number3.CE_EntryNum = "COC01";

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			unloco.RefLocoMaps.DeleteAll();

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "consignor Pickup org";
			consignorPickupAddress.OH_RL_NKClosestPort = "CNBZN";
			consignorPickupAddress.MainAddress.Address1 = "Unit 645";
			consignorPickupAddress.MainAddress.Address2 = "234 Drive";
			consignorPickupAddress.MainAddress.City = "unknown city";
			consignorPickupAddress.MainAddress.Postcode = "3243";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "consignee delivery org";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGJUR";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 563";
			consigneeDeliveryAddress.MainAddress.Address2 = "435 Drive";
			consigneeDeliveryAddress.MainAddress.City = "unknown city";
			consigneeDeliveryAddress.MainAddress.Postcode = "4356";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var pickupAgentAddress = Factory.New<OrgHeader>();
			pickupAgentAddress.OH_FullName = "pickup agent org";
			pickupAgentAddress.OH_RL_NKClosestPort = "FRNCE";
			pickupAgentAddress.MainAddress.Address1 = "Unit 283";
			pickupAgentAddress.MainAddress.Address2 = "283 Drive";
			pickupAgentAddress.MainAddress.City = "unknown city";
			pickupAgentAddress.MainAddress.Postcode = "2836";
			pickupAgentAddress.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgentAddress.MainAddress.PK;

			var docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.JP_OA_PickupCartageCoAddr = pickupAgentAddress.MainAddress.PK;
		}

		void PopulateConsolAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "410 10 10 10", "AU");

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "consol notify party";
			notifyParty.OH_RL_NKClosestPort = "GBLON";
			notifyParty.MainAddress.Address1 = "Unit 400";
			notifyParty.MainAddress.Address2 = "443 How Lane";
			notifyParty.MainAddress.City = "Angel";
			notifyParty.MainAddress.Postcode = "8888";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123 123 123", "GB");

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "consol notify party 2";
			notifyParty2.OH_RL_NKClosestPort = "CNCAN";
			notifyParty2.MainAddress.Address1 = "Unit 460";
			notifyParty2.MainAddress.Address2 = "333 How Lane";
			notifyParty2.MainAddress.City = "Wonderland";
			notifyParty2.MainAddress.Postcode = "7777";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1111111111", "CN");

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var carrierHandlingAgent = Factory.New<OrgHeader>();
			carrierHandlingAgent.OH_FullName = "consol carrier handling agent";
			carrierHandlingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierHandlingAgent.MainAddress.Address1 = "Unit 990";
			carrierHandlingAgent.MainAddress.Address2 = "245 Drive";
			carrierHandlingAgent.MainAddress.City = "unknown city";
			carrierHandlingAgent.MainAddress.Postcode = "4689";
			carrierHandlingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = carrierHandlingAgent.MainAddress.PK;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "consol carrier booking agent";
			carrierBookingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierBookingAgent.MainAddress.Address1 = "Unit 990";
			carrierBookingAgent.MainAddress.Address2 = "245 Drive";
			carrierBookingAgent.MainAddress.City = "unknown city";
			carrierBookingAgent.MainAddress.Postcode = "4689";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;
		}

		#endregion
	}
}
