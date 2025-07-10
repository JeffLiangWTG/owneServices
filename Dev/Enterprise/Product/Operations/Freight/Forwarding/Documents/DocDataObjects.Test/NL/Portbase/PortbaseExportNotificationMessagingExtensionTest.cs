using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	sealed class PortbaseExportNotificationMessagingExtensionTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessageWithdrawal_NotAccepted()
		{
			var consol = CreateConsol();

			var builder = new PortbaseExportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentNames.ExportNotification);

			var extensions = new PortbaseExportNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn only after you have received an acceptance response first.", false, res);
		}

		public void TestContinueWithSendingMessageWithdrawal_OneAccepted()
		{
			var consol = CreateConsol();

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877"));

			var builder = new PortbaseExportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentNames.ExportNotification);

			var extensions = new PortbaseExportNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn only after you have received an acceptance response for all selected documents.", false, res);
		}

		public void TestContinueWithSendingMessageWithdrawal_AllAccepted()
		{
			var consol = CreateConsol();

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877"));
			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094878"));

			var builder = new PortbaseExportNotificationBuilder(consol);
			var data = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentNames.ExportNotification);

			var extensions = new PortbaseExportNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn, received an acceptance response for all selected documents.", true, res);
		}

		public void TestContinueWithSendingMessageWithdrawal_SelectionAccepted()
		{
			var consol = CreateConsol();

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2021, 05, 19),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877"));

			var builder = new PortbaseExportNotificationBuilder(consol);
			var data = builder.Build();

			data.SelectAllToSend = ZBool.False;
			data.Documents.ElementAt(0).IsSelectedToSend = ZBool.True;
			data.Documents.ElementAt(1).IsSelectedToSend = ZBool.False;

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentNames.ExportNotification);

			var extensions = new PortbaseExportNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("Message can be withdrawn, the selected document was accepted before.", true, res);
		}

		public void TestIsSendingAmendment_LatestLogIsSTU()
		{
			var consol = CreateConsol();

			var builder = new PortbaseExportNotificationBuilder(consol);
			var data = builder.Build();

			data.SelectAllToSend = ZBool.False;
			data.Documents.ElementAt(0).IsSelectedToSend = ZBool.True;
			data.Documents.ElementAt(1).IsSelectedToSend = ZBool.False;

			var documentData = CreateDocumentData(consol) as IStmALogProvider;
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			dynamicData.SetupGet(d => d.Value).Returns(data);
			document.SetupGet(d => d.Data).Returns(dynamicData.Object);
			messageInstructions.Setup(m => m.DocumentName).Returns(ConsolDocumentNames.ExportNotification);

			var extensions = new PortbaseExportNotificationMessagingExtensions(document.Object, consol);

			var parametersWithoutType = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877")
			};

			var parametersWithType = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DutchPortsConstants.Departments.Portbase),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "0")
			};

			var parametersWithResetToOriginalType = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DutchPortsConstants.MessageTypes.ExportNotification),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, "DOC0094877"),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceMessageTypes.ResetToOriginal)
			};

			CreateLog(consol, Events.MessageSent, new ZDateTime(2022, 08, 01), parametersWithoutType);
			CreateLog(consol, Events.StatusUpdated, new ZDateTime(2022, 08, 02), parametersWithType);
			CreateLog(consol, Events.MessageAccepted, new ZDateTime(2022, 08, 02), parametersWithoutType);

			AssertEquals(true, extensions.IsSendingAmendment());

			CreateLog(consol, Events.MessageWithdrawCancelRequest, new ZDateTime(2022, 08, 03), parametersWithoutType);
			CreateLog(consol, Events.MessageWithdrawCancelAccepted, new ZDateTime(2022, 08, 04), parametersWithoutType);
			CreateLog(consol, Events.MessageSent, new ZDateTime(2022, 08, 05), parametersWithoutType);
			CreateLog(consol, Events.StatusUpdated, new ZDateTime(2022, 08, 06), parametersWithResetToOriginalType);

			AssertEquals(false, extensions.IsSendingAmendment());

			CreateLog(consol, Events.MessageSent, new ZDateTime(2022, 08, 07), parametersWithoutType);
			CreateLog(consol, Events.StatusUpdated, new ZDateTime(2022, 08, 08), parametersWithType);
			CreateLog(consol, Events.MessageAccepted, new ZDateTime(2022, 08, 08), parametersWithoutType);
			CreateLog(consol, Events.StatusUpdated, new ZDateTime(2022, 08, 09), parametersWithResetToOriginalType);

			AssertEquals(false, extensions.IsSendingAmendment());

			CreateLog(consol, Events.MessageSent, new ZDateTime(2022, 08, 10), parametersWithoutType);
			CreateLog(consol, Events.StatusUpdated, new ZDateTime(2022, 08, 11), parametersWithResetToOriginalType);

			AssertEquals(false, extensions.IsSendingAmendment());
		}

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = ConsolDocumentDataStoreNames.PortbaseExportNotification;

			return documentData;
		}

		void CreateLog(IStmALogProvider logParent, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), string.Empty, parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001100";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NLRTM";
			consol.JK_RL_NKDischargePort = "AUSYD";

			CreateTransport(consol);
			CreatePackingLinesAndContainers(consol);

			return consol;
		}

		void CreateTransport(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "NLRTM";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = "Ship";
			transport.JW_VoyageFlight = "111";
		}
		void CreatePackingLinesAndContainers(ForwardingConsol consol)
		{
			var shipmentA = consol.Shipments.AddNew();
			PopulateShipment(shipmentA, "S0700001557", Customs.Common.EU.ExportCommunityTransitStatusList.Codes.C, "DOC0094877", 5000, 5, "Goods desc A");
			var packLineA = shipmentA.OuterPackLines.AddNew();
			PopulatePackLine(packLineA, "DOC0094877", 5, "PLT", 5000, "KG");

			var shipmentB = consol.Shipments.AddNew();
			PopulateShipment(shipmentB, "S00001590", Customs.Common.EU.ExportCommunityTransitStatusList.Codes.T1, "DOC0094878", 5000, 5, "Goods desc B");
			var packLineB = shipmentB.OuterPackLines.AddNew();
			PopulatePackLine(packLineB, ZString.Empty, 5, "PLT", 5000, "KG");

			var shipmentC = consol.Shipments.AddNew();
			PopulateShipment(shipmentC, "S00001591", ZString.Empty, ZString.Empty, 5000, 5, "Goods desc C");
			var packLineC = shipmentC.OuterPackLines.AddNew();
			PopulatePackLine(packLineC, ZString.Empty, 5, "PLT", 5000, "KG");

			var containerX = consol.Containers.AddNew();
			PopulateContainer(containerX, "TGMU3039480", 17280.00, "KG");
			var containerY = consol.Containers.AddNew();
			PopulateContainer(containerY, "TRLU2456893", 17280.00, "KG");

			containerX.PackLines.Add(packLineA);
			containerX.PackLines.Add(packLineB);
			containerX.PackLines.Add(packLineC);

			containerY.PackLines.Add(packLineA);
			containerY.PackLines.Add(packLineB);
			containerY.PackLines.Add(packLineC);
		}

		void PopulateShipment(ForwardingShipment shipment, ZString shipmentNumber, ZString cusEntryNumType, ZString mawb, ZDecimal weight, ZInt packs, ZString desc)
		{
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_OuterPacks = packs;

			var number = shipment.CusEntryNumbers.AddNew();
			number.CE_EntryType = cusEntryNumType;
			number.CE_EntryNum = mawb;
			number.CE_ParentID = shipment.PK;
			number.CE_ParentTable = shipment.TableName;
		}

		void PopulatePackLine(ForwardingPackLine packLine, ZString exportRef, ZInt packs, ZString packtype, ZDecimal weight, ZString weightUnit)
		{
			packLine.JL_ExportRefNumber = exportRef;
			packLine.JL_PackageCount = packs;
			packLine.JL_F3_NKPackType = packtype;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUnit;
		}

		void PopulateContainer(ForwardingContainer container, ZString containerNum, ZDecimal weight, ZString weightUnit)
		{
			container.JC_ContainerNum = containerNum;
			container.JC_GrossWeight = weight;
			container.JC_GrossWeightUQ = weightUnit;
		}

		#endregion
	}
}
