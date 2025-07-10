using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class CresaMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessage_NotAccepted()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.ClearanceCompleted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "ECV123"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Message can only be sent when there is no SCM event with a matching CFR reference.", false, res);
			notifications.Verify(x => x.ShowMessage("The Goods Received (CRESA) message cannot be sent once Clearance Completed (SCM) received from Port Community System.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessage_Accepted()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("Message can only be sent when there is no SCM event with a matching CFR reference.", null, res);
			notifications.Verify(x => x.ShowMessage("The Goods Received (CRESA) message cannot be sent once Clearance Completed (SCM) received from Port Community System.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageAmendment_NotAccepted()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageAmendment_Accepted()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be amended only after you have received an accepted response to the previous message.", "Warning"), Times.Never);
		}

		public void TestContinueWithSendingMessageWithdrawal_NotAccepted()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn only when MAA is received", false, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_NotAccepted_WithSCMEvent()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.ClearanceCompleted,
				new ZDateTime(2021, 05, 21),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "ECV123"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be only withdrawn when accepted and no SCM event is present with a matching CFR reference", false, res);
			notifications.Verify(x => x.ShowMessage("The Goods Received (CRESA) message cannot be withdrawn/canceled once Clearance Completed (SCM) received from Port Community System", "Warning"), Times.Once);
		}

		public void TestContinueWithSendingMessageWithdrawal_Accepted()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be amended as MAA is received.", null, res);
			notifications.Verify(x => x.ShowMessage("Message can be withdrawn only after you have received an accepted response to the previous message.", "Sending Withdraw/Cancel Request"), Times.Never);
			notifications.Verify(x => x.ShowMessage("The Goods Received (CRESA) message cannot be withdrawn/canceled once Clearance Completed (SCM) received from Port Community System", "Warning"), Times.Never);
		}

		public void TestContinueWithResetToOriginal_NotAccepted()
		{
			var shipment = CreateShipment(false);
			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 20),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can not reset when MAA event received.", false, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Once);
		}

		public void TestContinueWithResetToOriginal_NotAccepted_WithSCMEvent()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			CreateLog(documentData,
				Events.ClearanceCompleted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "ECV123"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can not reset when MAA there is a SCM event with matich CRF reference", false, res);
			notifications.Verify(x => x.ShowMessage("The Goods Received (CRESA) message cannot be reset to original once Clearance Completed (SCM) received from Port Community System.", "Warning"), Times.Once);
		}

		public void TestContinueWithResetToOriginal_Accepted()
		{
			var shipment = CreateShipment(false);
			var ecvReference = shipment.Numbers.AddNew();
			ecvReference.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ExportConventional;
			ecvReference.CE_EntryNum = "ECV123";
			ecvReference.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;

			var builder = new CresaBuilder(shipment);
			var cresa = builder.Build();

			var documentData = CreateDocumentData(shipment) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			CreateLog(documentData,
				Events.MessageSent,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Terminal"));

			dynamicData.SetupGet(d => d.Value).Returns(cresa);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.SetupGet(m => m.DocumentName).Returns(FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);

			var extensions = new CresaMessagingExtensions(document.Object, shipment, messageInstructions.Object);
			var res = extensions.ContinueWithResetToOriginal(notifications.Object);

			AssertEquals("Message can be reset as no response message(MAA) received", null, res);
			notifications.Verify(x => x.ShowMessage("You cannot change the status to \"Reset to Original\" as the original message has already been accepted.", "Information"), Times.Never);
		}

		IVisualizerDocumentData CreateDocumentData(ForwardingShipment shipment)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#region Implementation

		ForwardingShipment CreateShipment(ZBool isStandAloneShipment)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			var deliveryOrderReceiptNote = shipment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Delivery Order Receipt Note";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_ExportRefNumber = "AMRUT57%";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_ExportRefNumber = "AMRUT43%";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			// No Detailed Description
			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 3;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 300;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 225;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 5;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-50";
			packline3.JL_ExportRefNumber = "AMRUT50%";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers3";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 50%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			// No Detailed Description, No Description, No Marks And Numbers
			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_PackageCount = 2;
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_ActualWeight = 200;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 150;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_Length = 10;
			packline4.JL_Width = 5;
			packline4.JL_Height = 3;
			packline4.JL_UnitOfDimension = "M";
			packline4.JL_HarmonisedCode = "WHISKY";
			packline4.JL_RefNumber = "AMR-64";
			packline4.JL_ExportRefNumber = "AMRUT64%";
			packline4.JL_LastKnownTransitWarehouseStatus = "RCV";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			if (!isStandAloneShipment)
			{
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

				var transport1 = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport1.JW_LegOrder = 1;
				transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
				transport1.JW_RL_NKLoadPort = "AUMEL";
				transport1.JW_RL_NKDiscPort = "AUSYD";
				transport1.JW_Vessel = "Dragon";
				transport1.JW_VoyageFlight = "666";
				transport1.JW_ETD = ZDate.Today.AddDays(1);
				transport1.JW_ETA = ZDate.Today.AddDays(6);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport2.JW_RL_NKLoadPort = "AUSYD";
				transport2.JW_RL_NKDiscPort = "FRMRS";
				transport2.JW_Vessel = "StarShip";
				transport2.JW_VoyageFlight = "666";
				transport2.JW_ETD = ZDate.Today.AddDays(3);
				transport2.JW_ETA = ZDate.Today.AddDays(4);

				var transport3 = consol.Transports.AddNew();
				transport3.JW_LegOrder = 3;
				transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transport3.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				transport3.JW_RL_NKLoadPort = "FRMRS";
				transport3.JW_RL_NKDiscPort = "FRNCE";
				transport3.JW_Vessel = "Enterprise";
				transport3.JW_VoyageFlight = "666";
				transport3.JW_ETD = ZDate.Today.AddDays(5);
				transport3.JW_ETA = ZDate.Today.AddDays(6);

				var refContainer = Factory.NewWithValidTestData<RefContainer>();
				refContainer.RC_ISOType = "22P1";
				refContainer.RC_ContainerType = "RFG";
				refContainer.RC_TareWeight = 222;

				var container1 = consol.Containers.AddNew();
				container1.JC_ContainerNum = "AAAA0000007";
				container1.JC_RC = refContainer.PK;
				container1.JC_DeliveryMode = "CFS/CY";
				container1.JC_IsShipperOwned = true;
				container1.JC_GrossWeightUQ = "KG";
				container1.JC_TareWeight = 1000;
				container1.JC_DunnageWeight = 1000;

				container1.PackLines.Add(packline1);
				container1.PackLines.Add(packline2);
				container1.PackLines.Add(packline3);
				container1.PackLines.Add(packline4);
			}

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRMAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
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

			ForwardingDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.JP_OA_PickupCartageCoAddr = pickupAgentAddress.MainAddress.PK;
		}

		void PopulateConsolAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
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
