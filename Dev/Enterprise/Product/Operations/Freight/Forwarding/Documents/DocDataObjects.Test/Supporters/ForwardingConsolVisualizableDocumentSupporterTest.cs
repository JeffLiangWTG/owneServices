using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using BelgianPortsConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.BelgianPortsConstants;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using EventConstants = CargoWise.EventReference.Constants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ForwardingConsolVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		#region TestOnMessageSent

		const string verifiedGrossContainerWeight = "Verified Gross Container Weight";

		public void TestOnMessageSent()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);

			var @event = Events.MessageSent;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var msn = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has MSN event", msn);

			var uxmlShipment = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, uxmlShipment);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMSN = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MSN event found on container", containerMSN);
				AssertEquals("consol MSN event SL_IsEstimate matches container MSN SL_IsEstimate",
					msn.SL_IsEstimate, containerMSN.SL_IsEstimate);
				AssertEquals("consol MSN event SL_EventTime matches container MSN SL_EventTime",
					msn.SL_EventTime, containerMSN.SL_EventTime);
				AssertEquals("consol MSN event SL_Reference matches container MSN SL_Reference",
					msn.SL_Reference, containerMSN.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region TestOnMessageWithdrawalSent

		public void TestOnMessageWithdrawalSent()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);

			var @event = Events.MessageWithdrawCancelRequest;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}");

			var mwr = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has MWR event", mwr);

			var uxmlShipment = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, uxmlShipment);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnMessageWithdrawalSent();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerMWR = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("MWR event found on container", containerMWR);
				AssertEquals("consol MWR event SL_IsEstimate matches container MWR SL_IsEstimate",
					mwr.SL_IsEstimate, containerMWR.SL_IsEstimate);
				AssertEquals("consol MWR event SL_EventTime matches container MWR SL_EventTime",
					mwr.SL_EventTime, containerMWR.SL_EventTime);
				AssertEquals("consol MWR event SL_Reference matches container MWR SL_Reference",
					mwr.SL_Reference, containerMWR.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region TestOnResetToOriginal

		public void TestOnResetToOriginal()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent);

			var @event = Events.StatusUpdated;

			consol.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Estimate,
				ZDateTimeOffset.Now,
				$"some ref|MST={verifiedGrossContainerWeight}|TYP={Core.Constants.EventReferenceMessageTypes.ResetToOriginal}");

			var stu = consol.Logs.MostRecentLogByEventTime(@event);

			AssertNotNull("prerequisite; consol has STU event", stu);

			var uxmlShipment = CreateShipment(consol);
			var dynamicData = CreateDynamicData(consol, uxmlShipment);

			var document = new Mock<IDocument>();
			document.SetupGet(d => d.Name).Returns(verifiedGrossContainerWeight);
			document.SetupGet(d => d.DataContext).Returns(DataContext.UXML);
			document.SetupGet(d => d.Data).Returns(dynamicData);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			supporter.GetMessageEventsProcessor(document.Object).OnResetToOriginal();

			foreach (var containerBizObj in consol.Containers.OfType<ForwardingContainer>())
			{
				var containerSTU = consol.Logs.MostRecentLogByEventTime(@event);

				AssertNotNull("STU event found on container", containerSTU);
				AssertEquals("consol STU event SL_IsEstimate matches container STU SL_IsEstimate",
					stu.SL_IsEstimate, containerSTU.SL_IsEstimate);
				AssertEquals("consol STU event SL_EventTime matches container STU SL_EventTime",
					stu.SL_EventTime, containerSTU.SL_EventTime);
				AssertEquals("consol STU event SL_Reference matches container STU SL_Reference",
					stu.SL_Reference, containerSTU.SL_Reference);

				AssertEquals("container JC_GrossWeightVerificationStatus has been updated",
					Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, containerBizObj.JC_GrossWeightVerificationStatus);
			}
		}

		#endregion

		#region TestGetAdditionalData_VGM

		public void TestGetAdditionalData_VGM()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);
			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = new VGMMenuItem(Factory, VGMMessageAction.SendOriginal, "caption");

			var selector = new Mock<IContainerSelector>();

			var selectedConsolContainers = consol
				.Containers
				.Skip(1)
				.OfType<ForwardingContainer>()
				.ToArray();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedConsolContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedConsolContainers,
						containers);
				}
				else
				{
					Fail("should return container selection");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_LDE

		public void TestGetAdditionalData_LDE()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.FRFinalContainerManifestLDE);

			var selectedContainers = new ForwardingContainer[] { container1, container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_LPD

		public void TestGetAdditionalData_LPD()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.FRImportManifestLPD);

			var selectedContainers = new ForwardingContainer[] { container1, container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_ContainerLoadPlan

		public void TestGetAdditionalData_ContainerLoadPlan_CustomMenuItem()
		{
			var customCLPMenuItem = Factory.New<DocumentCommand>();

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = DataContext.ContainerLoadPlan;

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = customCLPMenuItem.PK;

			AssertGetAdditionalData_ContainerLoadPlan(customCLPMenuItem);
		}

		void AssertGetAdditionalData_ContainerLoadPlan(DocumentCommand menuItem)
		{
			AssertNotNull("prerequisite; menu item exists", menuItem);

			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);
			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			var selector = new Mock<IContainerSelector>();

			var selectedConsolContainers = consol
				.Containers
				.Skip(1)
				.OfType<ForwardingContainer>()
				.ToArray();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedConsolContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is ForwardingContainer[] containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedConsolContainers,
						containers);
				}
				else
				{
					Fail("should return container selection");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_ACAS

		public void TestGetAdditionalData_ACAS_SystemMenuItem()
		{
			var systemACASMenuItem = Factory.Load<DocumentCommand>(ConsolSystemFormMenuItems.DocumentMenuACASHouseChecklistUSPK);
			AssertGetAdditionalData_ACAS(systemACASMenuItem);
		}

		public void TestGetAdditionalData_ACAS_CustomMenuItem()
		{
			var customACASMenuItem = Factory.New<DocumentCommand>();

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = DataContext.ACASHouseChecklist;

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = customACASMenuItem.PK;

			AssertGetAdditionalData_ACAS(customACASMenuItem);
		}

		void AssertGetAdditionalData_ACAS(DocumentCommand menuItem)
		{
			AssertNotNull("prerequisite; menu item exists", menuItem);

			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent);
			consol.JK_MasterBillNum = "";

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			var result = supporter.GetAdditionalData(consol, menuItem);
			Assert("Should be error", result.IsLeft);
			AssertEquals("The Consol has no Shipments.", result.Left);

			consol.Shipments.AddNew();

			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("No error", result.IsRight);
			AssertEquals("No errors", null, result.Right);
		}

		#endregion

		#region TestGetAdditionalData_BR

		public void TestGetAdditionalData_BR_ShipmentSentBR()
		{
			var errorMessage = @"A Booking Request has been sent from Shipment S000001, S000002. To send a Booking Request from this Consol, either:
	1. Withdraw/Cancel the Booking Request/s from the Shipment/s or;
	2. Reset Shipment/s Booking Request to Original and advise NVOCC accordingly.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "CNSHA";
			shipment1.JS_RL_NKDestination = "MYABU";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_UniqueConsignRef = "S000001";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "CNNJG";
			shipment2.JS_RL_NKDestination = "MYABU";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment2.JS_UniqueConsignRef = "S000002";

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BookingRequest);

			var result = supporter.GetAdditionalData(consol, menuItem);
			Assert("No popup message", result.IsRight);
			AssertEquals("No popup message", null, result.Right);

			CreateEvent(shipment1, Events.MessageSent);
			CreateEvent(shipment2, Events.MessageSent);

			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("popup message", result.IsLeft);
			AssertMultilineASCIIEquals("popup message", errorMessage, result.Left);
		}

		public void TestGetAdditionalData_BR_WaitingMWA()
		{
			var errorMessage = @"A confirmation for the Withdrawal/Cancellation of the Booking Request for Shipment S000001 has not yet been received. The Consol Booking Request can be generated after the confirmation has been received.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BookingRequest);

			var result = supporter.GetAdditionalData(consol, menuItem);
			Assert("No popup message", result.IsRight);
			AssertEquals("No popup message", null, result.Right);

			CreateEvent(shipment, Events.MessageSent);
			CreateEvent(shipment, Events.MessageAccepted);
			CreateEvent(shipment, Events.MessageWithdrawCancelRequest);

			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("popup message", result.IsLeft);
			AssertEquals("popup message", errorMessage, result.Left);

			CreateEvent(shipment, Events.MessageWithdrawCancelAccepted);
			result = supporter.GetAdditionalData(consol, menuItem);

			Assert("No popup message", result.IsRight);
			AssertEquals("No popup message as MWA received", null, result.Right);
		}

		void CreateEvent(IStmALogParent logParent, ZArchitecture.Business.Event @event)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.BookingRequest));

			logParent.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		#endregion

		#region TestGetAdditionalData_BE_CertifiedPickup

		public void TestGetAdditionalData_BE_CertifiedPickup_NotApplicable()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BECertifiedPickup, BelgianPortsConstants.CertifiedPickupMenuItemName.Transfer);

			var selectedContainers = new ForwardingContainer[] { container1, container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Never());

				if (data.IsLeft
					&& !string.IsNullOrEmpty(data.Left))
				{
					AssertEquals("There are no applicable containers.", data.Left);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		public void TestGetAdditionalData_BE_CertifiedPickup_Accepted()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON00001";
			container1.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CON00001")
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline));
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BECertifiedPickup, BE.BelgianPortsConstants.CertifiedPickupMenuItemName.Transfer);

			var selectedContainers = new ForwardingContainer[] { container1 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		public void TestGetAdditionalData_BE_CertifiedPickup_Assigned()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON00002";
			container2.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CON00002")
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, CertifiedPickupConstants.ParameterTypes.ContainerRelease)
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight));

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BECertifiedPickup, BE.BelgianPortsConstants.CertifiedPickupMenuItemName.AcceptDecline);

			var selectedContainers = new ForwardingContainer[] { container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_CargoControlAndTransitHouseManifest

		public void TestGetAdditionalData_CargoControlAndTransitHouseManifest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.CargoControlAndTransitHouseManifest, "CCT House Manifest");

			var selector = new Mock<IContainerSelector>();

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				if (data.IsLeft
					&& !string.IsNullOrEmpty(data.Left))
				{
					AssertEquals("Advanced Manifest is not available from Consolidations with type DRT.", data.Left);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_TMining_SecureContainerRelease

		public void TestGetAdditionalData_TMining_SecureContainerRelease_NotApplicable()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.TMiningSecureContainerRelease, TMiningConstants.SecureContainerReleaseMenuItemName.Transfer);

			var selectedContainers = new ForwardingContainer[] { container1, container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Never());

				if (data.IsLeft
					&& !string.IsNullOrEmpty(data.Left))
				{
					AssertEquals("There are no applicable containers.", data.Left);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		public void TestGetAdditionalData_TMining_SecureContainerRelease_Revoke()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON00001";
			container1.Logs.AddNew(Events.MessageAccepted
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CON00001")
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, Business.TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer));
			var container2 = consol.Containers.AddNew();

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.TMiningSecureContainerRelease, TMiningConstants.SecureContainerReleaseMenuItemName.Revoke);

			var selectedContainers = new ForwardingContainer[] { container1 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		public void TestGetAdditionalData_TMining_SecureContainerRelease_Transfer()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON00002";
			container2.Logs.AddNew(Events.Authorised
				, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.EquipmentReferenceNumber, "CON00002")
				, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, Business.TMiningConstants.ParameterMessageTypes.SecureContainerRelease));

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.TMiningSecureContainerRelease, TMiningConstants.SecureContainerReleaseMenuItemName.Transfer);

			var selectedContainers = new ForwardingContainer[] { container2 };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is IReadOnlyCollection<CommonContainer> containers)
				{
					AssertContainsExactElementsInAnyOrder("containers",
						selectedContainers,
						containers);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		#endregion

		#region TestAdditionalData_MultimodalDangerousGoods

		public void TestGetAdditionalData_MultimodalDangerousGoods()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Containers.AddNew(); // container with no packline exists on the consol
			var containerWithDG = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packlineWithDG = shipment.OuterPackLines.AddNew();
			packlineWithDG.UNDGs.AddNew();
			packlineWithDG.SetContainer(containerWithDG.PK);

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.MultimodalDangerousGoodsDeclaration);

			var containers = new ForwardingContainer[] { containerWithDG };
			var selector = new Mock<IContainerSelector>();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(containers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight
					&& data.Right is ForwardingContainer container)
				{
					AssertEquals("containers should match", containerWithDG, container);
				}
				else
				{
					Fail("AdditionalData returned incorrect data");
				}
			}
		}

		public void TestGetAdditionalData_MultimodalDangerousGoods_OnlyFindsDGContainer()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Containers.AddNew(); // container with no packline exists on the consol
				var containerWithDG = consol.Containers.AddNew();
				var shipment = consol.Shipments.AddNew();
				var packlineWithDG = shipment.OuterPackLines.AddNew();
				packlineWithDG.UNDGs.AddNew();
				packlineWithDG.SetContainer(containerWithDG.PK);

				Factory.Save();

				var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
				var menuItem = CreateMenuItem(DataContext.MultimodalDangerousGoodsDeclaration);
				AssertEquals("Correct Container is found", containerWithDG, (ForwardingContainer)supporter.GetAdditionalData(consol, menuItem).Right);
			}
		}

		StmMenuItem CreateMenuItem(string context, string menuName = "")
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = menuName;
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		#endregion

		#region TestGetAdditionalData_ExportPreAdviceNotification

		public void TestGetAdditionalData_ExportPreAdviceNotification()
		{
			var consol = CreateConsol(Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified);
			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.ExportPreAdviceNotification);

			var selector = new Mock<IContainerSelector>();

			var selectedConsolContainers = consol
				.Containers
				.Skip(1)
				.OfType<ForwardingContainer>()
				.ToArray();

			selector
				.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedConsolContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				var data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight && data.Right is ForwardingContainer[] containers)
				{
					AssertContainsExactElementsInAnyOrder("containers", selectedConsolContainers, containers);
				}
				else
				{
					Fail("should return container selection");
				}
			}
		}

		#endregion

		#region TestGetMessageLogCreator_AdvancedManifest

		public void TestGetMessageLogCreator_AdvancedManifest_US() => AssertGetMessageLogCreator_AdvancedManifest(Core.Constants.CountryCodes.UnitedStates, DataContext.ACASHouseChecklist);
		public void TestGetMessageLogCreator_AdvancedManifest_BR() => AssertGetMessageLogCreator_AdvancedManifest(Core.Constants.CountryCodes.Brazil, DataContext.CargoControlAndTransitHouseManifest);

		void AssertGetMessageLogCreator_AdvancedManifest(string countryCode, string dataContext)
		{
			var consol = Factory.New<ForwardingConsol>();
			var documentData = Factory.New<VisualizerDocumentData>();

			var document = new Mock<IDocument>();
			var dynamicData = new Mock<IDynamicData>();

			document.SetupGet(d => d.DataContext).Returns(dataContext);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			var logCreator = supporter.GetMessageLogCreator(document.Object);

			AssertNotNull($"{nameof(IMessageLogCreator)} for {countryCode}", logCreator);
			Assert("log has been created",
				logCreator.CreateMessageSentLog(documentData, dynamicData.Object, DocumentNames.AdvancedManifest, "Customs"));

			var msn = documentData.Logs.MostRecentLogByEventTime(Events.MessageSent);

			AssertNotNull("MSN event has been created", msn);
			AssertContainsExactElementsInAnyOrder("MSN event parameters",
				new[]
				{
					"MST=Advanced Manifest",
					$"LOC={countryCode}",
					"DEP=Customs"
				},
				msn.Parameters.Select(p => $"{p.Key}={p.Value}"));
		}

		#endregion

		#region TestGetEventParent
		public void TestGetEventParent_FormOfUndertakingForInterRAAWBHandling() => AssertGetEventParent(ConsolDocumentNames.FormOfUndertakingForInterRAAWBHandling, ConsolDocumentDataStoreNames.CargoSecurityDeclaration);
		public void TestGetEventParent_DeclarationOfExportConsignmentBulk() => AssertGetEventParent(ConsolDocumentNames.DeclarationOfExportConsignmentBulk, ConsolDocumentDataStoreNames.CargoSecurityDeclaration);
		public void TestGetEventParent_RegulatedAgentAviationSecurityDeclaration() => AssertGetEventParent(ConsolDocumentNames.RegulatedAgentAviationSecurityDeclaration, ConsolDocumentDataStoreNames.CargoSecurityDeclaration);
		public void TestGetEventParent_DeclarationOfExportConsignmentPrepackedUnit() => AssertGetEventParent(ConsolDocumentNames.DeclarationOfExportConsignmentPrepackedUnit, ConsolDocumentDataStoreNames.CargoSecurityDeclaration);
		public void TestGetEventParent_ExportNotification()
		{
			AssertGetEventParent(ConsolDocumentNames.ExportNotification, ConsolDocumentDataStoreNames.PortbaseExportNotification);
		}
		public void TestGetEventParent_ImportNotification()
		{
			AssertGetEventParent(ConsolDocumentNames.ImportNotification, ConsolDocumentDataStoreNames.PortbaseImportNotification);
		}
		public void TestGetEventParent_ContainerLoadPlan() => AssertGetEventParent(ConsolDocumentNames.ContainerLoadPlan, ConsolDocumentDataStoreNames.ContainerLoadPlan);
		public void TestGetEventParent_ShippingOrder() => AssertGetEventParent(ConsolDocumentNames.ShippingOrder, ConsolDocumentDataStoreNames.ShippingOrder);
		public void TestGetEventParent_ETerminalReleaseManifest() => AssertGetEventParent(ConsolDocumentNames.ETerminalReleaseManifest, ConsolDocumentDataStoreNames.ETerminalReleaseManifest);
		public void TestGetEventParent_VerifiedGrossContainerWeight() => AssertGetEventParent(ConsolDocumentNames.VerifiedGrossContainerWeight, ConsolDocumentDataStoreNames.ContainerGrossWeightVerification);
		public void TestGetEventParent_AdvancedManifest_BR() => AssertGetEventParent(ConsolDocumentNames.AdvancedManifest, ConsolDocumentDataStoreNames.AdvancedManifestBR, Core.Constants.CountryCodes.Brazil);
		public void TestGetEventParent_AdvancedManifest_BR_UNLOCO() => AssertGetEventParent(ConsolDocumentNames.AdvancedManifest, ConsolDocumentDataStoreNames.AdvancedManifestBR, Core.Constants.CountryCodes.Brazil, "BRSAA");
		public void TestGetEventParent_AdvancedManifest_US() => AssertGetEventParent(ConsolDocumentNames.AdvancedManifest, ConsolDocumentDataStoreNames.AdvancedManifestUS, Core.Constants.CountryCodes.UnitedStates);
		public void TestGetEventParent_AdvancedManifest_US_UNLOCO() => AssertGetEventParent(ConsolDocumentNames.AdvancedManifest, ConsolDocumentDataStoreNames.AdvancedManifestUS, Core.Constants.CountryCodes.UnitedStates, "USLAX");

		public void TestGetEventParent_BookingRequest_SeaBookingRequest2()
		{
			AssertGetEventParent(ConsolDocumentNames.BookingRequest, ConsolDocumentDataStoreNames.SeaBookingRequest2);
		}

		public void TestGetEventParent_ShippingInstruction_SeaBookingRequest2()
		{
			AssertGetEventParent(ConsolDocumentNames.ShippingInstruction, ConsolDocumentDataStoreNames.SeaBookingRequest2);
		}

		public void TestGetEventParent_ILGatePassMovement() => AssertGetEventParent(ConsolDocumentNames.ILGatePassMovement, ConsolDocumentDataStoreNames.ILGatePassMovement, Core.Constants.CountryCodes.Israel, "ILASH");

		void AssertGetEventParent(string documentName, string dataStoreName, string countryCode = null, string unloco = null)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = $"{countryCode ?? "NZ"}ZZZ";
			consol.JK_UniqueConsignRef = "C00000069";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_ParentTableCode = consol.TablePrefix;
			documentData.JDD_Name = dataStoreName;

			Factory.Save();

			var universalEvent = new Event
			{
				DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
				{
					DocumentaryOverride = new DocumentaryOverride
					{
						DocumentName = documentName
					},
					DataTargetCollection = new List<DataTarget>
					{
						new DataTarget
						{
							Key = "C00000069",
							Type = "ForwardingConsol"
						}
					}
				},
				EventTime = new ZDateTimeOffset(2019, 12, 04),
				EventType = "MAA"
			};

			if (!string.IsNullOrEmpty(countryCode) || !string.IsNullOrEmpty(unloco))
			{
				universalEvent.EventParameters = new EventParameters
				{
					Location = !string.IsNullOrEmpty(unloco) ? unloco : countryCode
				};
			}

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var eventParent = supporter.GetEventParent(universalEvent);

			AssertEquals("found document data", documentData, eventParent);
		}

		#endregion

		#region TestGetCustomCommands

		public void TestGetCustomCommands_HasMaximumCarrierShipperReferenceNumberOnConsol_ShippingInstruction()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00000069-V9";

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);

			Assert(commands.OfType<DisabledCommand>().Any());

			var parameters = new Dictionary<string, string>
			{
				["MST"] = "Booking Request"
			};

			consol.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, ZDateTimeOffset.Now, "", parameters.ToArray());
			commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);

			Assert("Booking Request had been send then SI is not allowed to reset", !commands.OfType<DisabledCommand>().Any());

			consol.Logs.CancelAll();
			parameters = new Dictionary<string, string>
			{
				["MST"] = "Shipping Order"
			};

			consol.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, ZDateTimeOffset.Now, "", parameters.ToArray());
			commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);

			Assert("Shipping Order had been send then SI is not allowed to reset", !commands.OfType<DisabledCommand>().Any());
		}

		public void TestGetCustomCommands_HasMaximumCarrierShipperReferenceNumberOnConsol_BookingRequest()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_UniqueConsignRef = "C00000069";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00000069-V9";

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert(commands.OfType<DisabledCommand>().Any());
		}

		#region CCTHouseManifest

		public void TestGetCustomCommands_CCTHouseManifest()
		{
			var consol = CreateConsol(false);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.CargoControlAndTransitHouseManifest);

			Assert(commands.OfType<SendWithdrawCargoControlAndTransitCommand>().Any());
		}

		#endregion

		#region TestDeliveryOceanCarrierMessagingAsPDFCommand

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_BookingRequest()
		{
			var consol = CreateConsol(false);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.ShippingLine.ShippingLine.RSL_BookingRequestAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.BookingRequest);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_BookingRequest_IsCoload()
		{
			var consol = CreateConsol(true);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.BookingRequest);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.Creditor.ShippingLine.RSL_BookingRequestAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.BookingRequest);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_ShippingOrder()
		{
			var consol = CreateConsol(false);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ShippingOrder);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.ShippingLine.ShippingLine.RSL_ShippingOrderAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.ShippingOrder);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_ShippingOrder_IsCoload()
		{
			var consol = CreateConsol(true);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ShippingOrder);

			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_ShippingInstruction()
		{
			var consol = CreateConsol(false);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.ShippingLine.ShippingLine.RSL_ShippingInstructionAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_ShippingInstruction_IsCoload()
		{
			var consol = CreateConsol(true);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.Creditor.ShippingLine.RSL_ShippingInstructionAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.ShippingInstruction);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_VGM()
		{
			var consol = CreateConsol(false);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.VerifiedGrossMass);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.ShippingLine.ShippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.VerifiedGrossMass);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_GetDeliveryOceanCarrierMessagingAsPDFCommand_VGM_IsCoload()
		{
			var consol = CreateConsol(true);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.VerifiedGrossMass);

			Assert(commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());

			consol.Creditor.ShippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			commands = supporter.GetCustomCommands(DataContext.VerifiedGrossMass);
			Assert(!commands.OfType<DeliveryOceanCarrierMessagingAsPDFCommand>().Any());
		}

		public void TestGetCustomCommands_ILGPMSendMessageCommand()
		{
			var consol = CreateConsol(true);

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var commands = supporter.GetCustomCommands(DataContext.ILGatePassMovement);

			Assert(commands.OfType<ILGPMSendMessageCommand>().Any());
			Assert(commands.OfType<ILGPMSendWithdrawalMessageCommand>().Any());
			Assert(commands.OfType<ILGPMResetToOriginalMessageCommand>().Any());
		}

		ForwardingConsol CreateConsol(bool isCoload)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_AgentType = isCoload ? Core.Constants.AgentType.CoLoad : Core.Constants.AgentType.Agent;

			var carrier = Factory.New<OrgHeader>();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "9001";
			shippingLine.RSL_IsNVO = false;

			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsSeaWholesaler = false;
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			if (isCoload)
			{
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			}
			else
			{
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			}

			Factory.Save();

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";

			return consol;
		}

		#endregion

		#endregion

		#region TestGetBookingConfirmationAdditionalData

		public void TestGetBookingConfirmationAdditionalData()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01329220";
			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BookingConfirmation);
			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When no booking confirmation message exists, supporter should show error message.",
				"Data required for Booking Confirmation not yet received from the Carrier.",
				data.Left);

			ImportBookingConfirmation();
			data = supporter.GetAdditionalData(consol, menuItem);
			Assert(data.IsRight);
			Assert("When booking confirmation message exists, supporter should add it to Additional Data.", data.Right is UniversalShipment);
		}

		void ImportBookingConfirmation()
		{
			var builder = new InterchangeBuilder(Factory);
			if (!builder.TryParseUniversalXml(bookingConfirmationUSXML.WrapInInterchange(), InterchangeBuilder.MessageDirection.Receive, out var interchange))
			{
				Fail("Interchange could not be created for booking confirmation message");
			}

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			messageQuery.FetchOnlyFromLocalCache = true;

			var message = Factory.Load<Messaging.Integration.IEDIMessage>(messageQuery).Single();

			var logger = new UniversalXmlImportLogger();
			var universalFactory = new UniversalObjectFactory(Factory);
			message.ProcessUniversalMessage(universalFactory, logger);

			var logs = string.Join("\r\n", logger.Logs.Select(log => log.Message));
			AssertContains("Universal Shipment data was linked to Consol C01329220.", logs, true);
		}

		const string bookingConfirmationUSXML = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<DocumentaryOverride>
				<DocumentName>Booking Confirmation</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>C01329220</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
	</Shipment>
</UniversalShipment>";

		class UniversalXmlImportLogger : ISimpleLogger
		{
			public IEnumerable<ISimpleLog> Logs => logs;

			readonly List<ISimpleLog> logs = new List<ISimpleLog>();

			public void Log(LogType type, string message)
			{
				if (!string.IsNullOrWhiteSpace(message))
				{
					logs.Add(new SimpleLog(type, message));
				}
			}
		}

		#endregion

		#region TestGetAdditionalData_CheckRelevantShippingLineOption

		public void TestBookingRequest_CheckRelevantShippingLineOption()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_BookingRequestAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.BookingRequest);

			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(!data.IsRight);
			AssertEquals("This Carrier does not support electronic messaging. Do you want to send this Booking Request as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shippingLine.RSL_BookingRequestAvailable = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			shippingLine.RSL_BookingRequestAvailable = false;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertEquals("This Co-loader does not support electronic messaging. Do you want to send this Booking Request as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			shippingLine.RSL_BookingRequestAvailable = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShippingOrder_CheckRelevantShippingLineOption()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ShippingOrderAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.ShippingOrder);

			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(!data.IsRight);
			AssertEquals("This Carrier does not support electronic messaging. Do you want to send this Shipping Order as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shippingLine.RSL_ShippingOrderAvailable = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShippingInstruction_CheckRelevantShippingLineOption()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ShippingInstructionAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.ShippingInstruction);

			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(!data.IsRight);
			AssertEquals("This Carrier does not support electronic messaging. Do you want to send this Shipping Instruction as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shippingLine.RSL_ShippingInstructionAvailable = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			shippingLine.RSL_ShippingInstructionAvailable = false;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertEquals("This Co-loader does not support electronic messaging. Do you want to send this Shipping Instruction as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			shippingLine.RSL_ShippingInstructionAvailable = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestVerifiedGrossMass_CheckRelevantShippingLineOption()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			consol.Containers.AddNew();

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			var menuItem = CreateMenuItem(DataContext.VerifiedGrossMass);

			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(!data.IsRight);
			AssertEquals("This Carrier does not support electronic messaging. Do you want to send this Verified Gross Container Weight as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = false;

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_RSL_ShippingLine = shippingLine.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertEquals("This Co-loader does not support electronic messaging. Do you want to send this Verified Gross Container Weight as a PDF via email?", UnitTestUserNotification.Instance.LastMessage.Text);

			shippingLine.RSL_VerifiedGrossContainerWeightAvailable = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			data = supporter.GetAdditionalData(consol, menuItem);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestGetDraftBillOfLadingAdditionalData

		public void TestGetDraftBillOfLadingAdditionalData_DLIEventDoesNotExist()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C01329220";
			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.DraftHouseBill);
			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When no booking confirmation message exists, supporter should show error message.",
				"Data required for Draft Bill of Lading not yet received from the Carrier.",
				data.Left);
		}

		#endregion

		#region IL Gatepass Movement

		public void TestILGatepassMovement_IsReceivingAgentProvidedAndVATValid()
		{
			const string expectedMessage = "Consol Receiving Agent VAT # is not configured for country IL, update Organization – Config tab";
			var factory = Factory;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var containerWithDG = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packlineWithDG = shipment.OuterPackLines.AddNew();
			packlineWithDG.UNDGs.AddNew();
			packlineWithDG.SetContainer(containerWithDG.PK);
			consol.JK_ConsolMode = ContainerModes.Groupage;

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.ILGatePassMovement);

			var result = supporter.GetAdditionalData(consol, menuItem);
			Assert("There should be an error message when Receiving Forwarder is null", result.IsLeft);
			AssertEquals("When Receiving Forwarder is null", expectedMessage, result.Left);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("There should be an error message when Receiving Forwarder has no VAT", result.IsLeft);
			AssertEquals("When Receiving Forwarder has no VAT", expectedMessage, result.Left);

			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "", "IL");
			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("There should be an error message when Receiving Forwarder is null", result.IsLeft);
			AssertEquals("When Receiving Forwarder has not valid IL VAT", expectedMessage, result.Left);

			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400006", "IL");
			result = supporter.GetAdditionalData(consol, menuItem);
			Assert("There should not be an error message when Receiving Forwarder has valid IL VAT", !result.IsLeft && result.IsRight);
		}

		public void TestILGatepassMovement_IsConsolContainerModeValid()
		{
			var factory = Factory;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var containerWithDG = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packlineWithDG = shipment.OuterPackLines.AddNew();
			packlineWithDG.UNDGs.AddNew();
			packlineWithDG.SetContainer(containerWithDG.PK);
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "400006", "IL");
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			Factory.Save();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.ILGatePassMovement);

			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.All, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.AgentConsol, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.AIR, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.BreakBulk, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Bulk, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Liquid, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.BuyersConsol, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.FCL, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.FTL, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Groupage, false);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.LCL, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Loose, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.LTL, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Mail, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.OnBoardCourier, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Other, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.ULD, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Unaccompanied, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.FreightAllKind, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.FCLMixedShipper, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Empty, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.RollOnRollOff, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Combination, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.Containerised, true);
			AssertAdditionalDataResult(consol, supporter, menuItem, ContainerModes.NonContainerised, true);
		}

		static void AssertAdditionalDataResult(ForwardingConsol consol, ForwardingConsolVisualizableDocumentSupporter supporter, StmMenuItem menuItem, ZString consolMode, bool expectError)
		{
			const string expectedMessage = "Consol Container Mode doesn't match, message must be sent from the relevant Shipment";

			consol.JK_ConsolMode = consolMode;
			var result = supporter.GetAdditionalData(consol, menuItem);

			if (expectError)
			{
				Assert("There should be an error message when Container Mode is not Groupage", result.IsLeft);
				AssertEquals("When Container Mode is not Groupage", expectedMessage, result.Left);
			}
			else
			{
				result = supporter.GetAdditionalData(consol, menuItem);
				Assert("There should not be an error message when Container Mode is Groupage", !result.IsLeft && result.IsRight);
			}
		}

		#endregion IL Gatepass Movement

		#region TestCMRConsignmentNote

		public void TestGetAdditionalData_CMRConsignment()
		{
			var factory = Factory;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var selector = new Mock<IContainerSelector>();
			selector.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>())).Returns(Array.Empty<ForwardingContainer>());

			// consol doesn't have any shipment or containers
			consol.Shipments.RemoveAll();
			consol.Containers.RemoveAll();

			var supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
			var menuItem = CreateMenuItem(DataContext.CMRConsignmentNote);
			var data = supporter.GetAdditionalData(consol, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When consol doesn't have any shipment or containers, supporter should show error message.",
				"Consol does not contain any shipments or containers.",
				data.Left);

			// consol has only shipments
			var shipment = consol.Shipments.AddNew();

			using (ObjectFactory.Substitute(selector.Object))
			{
				supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);
				data = supporter.GetAdditionalData(consol, menuItem);

				// When consol has only shipments, supporter should not call selector
				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Never());

				// When consol has only shipments, supporter should return null
				Assert(data.IsRight);
				AssertNull(data.Right);
			}

			// consol has only one container
			var container1 = consol.Containers.AddNew();
			supporter = new ForwardingConsolVisualizableDocumentSupporter(consol);

			using (ObjectFactory.Substitute(selector.Object))
			{
				data = supporter.GetAdditionalData(consol, menuItem);

				// When consol has only one container, supporter should not call selector
				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Never());

				Assert(data.IsRight);
				AssertEquals("When consol has only one container, supporter should return that container.", container1, data.Right);
			}

			// consol has multiple containers
			var container2 = consol.Containers.AddNew();

			var selectedConsolContainers = consol
				.Containers.Skip(1)
				.OfType<ForwardingContainer>()
				.ToArray();

			selector.Setup(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()))
				.Returns(selectedConsolContainers);

			using (ObjectFactory.Substitute(selector.Object))
			{
				data = supporter.GetAdditionalData(consol, menuItem);

				selector.Verify(s => s.SelectContainers(It.IsAny<CommonContainer[]>(), It.IsAny<ContainerSelectorMode>()), Times.Once());

				if (data.IsRight && data.Right is CommonContainer selectedContainer)
				{
					AssertEquals("When consol has multiple containers, supporter should return the selected container.", container2, selectedContainer);
				}
				else
				{
					Fail("Should return selected container");
				}
			}
		}

		#endregion

		#region Implementation

		IDynamicData CreateDynamicData(ForwardingConsol consol, UniversalShipment shipment)
		{
			var metaDataProvider = new MetaDataProviderForTest(consol);
			var dynamicData = shipment.MakeDynamic(metaDataProvider);
			return dynamicData;
		}

		UniversalShipment CreateShipment(ForwardingConsol consol)
		{
			var uxmlContainers = consol
				.Containers
				.Cast<ForwardingContainer>()
				.Select(container => new UniversalDataBuss.DataObjects.Universal.Container
				{
					ContainerNumber = container.JC_ContainerNum
				});

			var uxmlShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uxmlShipment.SetContainerCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>(uxmlContainers));

			return uxmlShipment;
		}

		ForwardingConsol CreateConsol(ZString containerGrossWeightVerificationStatus)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.AgentConsol;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var mainTransport = consol.Transports[0];
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_FK;
			mainTransport.JW_VoyageFlight = "F9999";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container1.JC_GrossWeight = 1000;
			container1.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container1.JC_GrossWeightVerificationStatus = containerGrossWeightVerificationStatus;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			container2.JC_GrossWeight = 2000;
			container2.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_GrossWeightVerificationDateTime = ZDateTime.Today;
			container2.JC_GrossWeightVerificationStatus = containerGrossWeightVerificationStatus;

			Factory.Save();

			return consol;
		}

		sealed class MetaDataProviderForTest : IMetaDataProvider
		{
			public MetaDataProviderForTest(ForwardingConsol consol)
			{
				this.consol = consol;
			}

			readonly ForwardingConsol consol;

			object IMetaDataProvider.GetMetaData(IDynamicData dynamicData, MetaDataType metaDataType)
			{
				if (metaDataType == MetaDataType.Identifier
					&& dynamicData.Value is UniversalDataBuss.DataObjects.Universal.Container container)
				{
					var containerBizObj = consol
						.Containers
						.OfType<ForwardingContainer>()
						.FirstOrDefault(c => string.CompareOrdinal(c.JC_ContainerNum, container.ContainerNumber) == 0);

					if (containerBizObj != null)
					{
						return containerBizObj.PK;
					}
				}

				return null;
			}
		}

		#endregion
	}
}
