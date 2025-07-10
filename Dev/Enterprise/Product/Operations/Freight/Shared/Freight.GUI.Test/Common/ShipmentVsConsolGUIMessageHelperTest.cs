using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ShipmentVsConsolGUIMessageHelperTest : TestCaseWithFactory
	{
		public void TestOnShipmentMasterChanged()
		{
			AssertNoExceptionThrown(() => Helper.OnShipmentMasterChanged(null, null, null, null));

			var consol = Factory.New<CommonConsol>();
			var oldMaster = Factory.New<CommonShipment>();
			var newMaster = Factory.New<CommonShipment>();
			var shipment = Factory.New<CommonShipment>();

			var eventArgs = new MasterChangedEventArgs(oldMaster.PK, newMaster.PK);

			Factory.Save();

			var messageHelper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			var message = "";
			var consolsToDetach = Array.Empty<CommonConsol>();
			messageHelper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message, consol, oldMaster, newMaster, new[] { shipment })).Returns(consolsToDetach);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Helper.OnShipmentMasterChanged(messageHelper.Object, shipment, consol, eventArgs);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			// Asked to detach -> NO
			consolsToDetach = new CommonConsol[] { consol };
			string message2 = "DETACH CONSOLS?";
			messageHelper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message2, consol, oldMaster, newMaster, new[] { shipment })).Returns(consolsToDetach);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			Helper.OnShipmentMasterChanged(messageHelper.Object, shipment, consol, eventArgs);
			AssertEquals("Question DETACH CONSOLS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Asked to detach -> YES -> Detach is NOT allowed
			messageHelper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message2, consol, oldMaster, newMaster, new[] { shipment })).Returns(consolsToDetach);
			var detachRequest = new ShipmentConsolDetachRequest("NOT ALLOWED!", () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); });
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, new[] { consol })).Returns(detachRequest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			Helper.OnShipmentMasterChanged(messageHelper.Object, shipment, consol, eventArgs);
			AssertEquals("Question DETACH CONSOLS?", UnitTestUserNotification.Instance.PreviousMessages[1].ToString());
			AssertEquals("Information NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Asked to detach -> YES -> Detach is allowed
			messageHelper.Setup(m => m.GetConsolsToDetachFromSubShipments(out message2, consol, oldMaster, newMaster, new[] { shipment })).Returns(consolsToDetach);

			var detachRequest2 = new ShipmentConsolDetachRequest("", () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); });
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, new[] { consol })).Returns(detachRequest2);
			messageHelper.Setup(m => m.DetachShipmentsFromConsols(new[] { shipment }, new[] { consol }));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			Helper.OnShipmentMasterChanged(messageHelper.Object, shipment, consol, eventArgs);
			AssertEquals("Question DETACH CONSOLS?", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestIsAllowedToDetachShipments()
		{
			AssertEquals("Default result for null arguments", false, Helper.IsAllowedToDetachShipments(null, null, null));

			var consol = Factory.New<CommonConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var shipments = new[] { shipment1, shipment2 };

			Factory.Save();

			var messageHelper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			// NOT Allowed

			var detachRequest = new ShipmentConsolDetachRequest("NOT ALLOWED!", () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); });
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Error NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed, no additional dialogs
			var detachRequest2 = new ShipmentConsolDetachRequest("", null, null, null);
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest2);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("No dialogs", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			// Allowed -> CutOff confirmation -> NO

			var detachRequest3 = new ShipmentConsolDetachRequest("", () => "CUTOFF?", null, null);
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest3);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question CUTOFF?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> CutOff confirmation -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest3);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question CUTOFF?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> NO
			var detachRequest5 = new ShipmentConsolDetachRequest("", null, () => "DETACH SUBS?", null);
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest5);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> CANCEL
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest5);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest5);
			messageHelper.Setup(m => m.DetachShipmentsFromConsols(shipments, new[] { consol }));
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedAcceptedMessage -> NO
			var detachRequest6 = new ShipmentConsolDetachRequest("", null, null, () => (ZString.Empty, "ReceivedAcceptedMessage?", ZString.Empty));
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest6);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question ReceivedAcceptedMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedAcceptedMessage -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest6);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question ReceivedAcceptedMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 AwaitingResponseMessage -> NO
			var detachRequest7 = new ShipmentConsolDetachRequest("", null, null, () => (ZString.Empty, ZString.Empty, "AwaitingResponseMessage?"));
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest7);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question AwaitingResponseMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 AwaitingResponseMessage -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest7);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question AwaitingResponseMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedFFMAndDepartureMessage
			var detachRequest8 = new ShipmentConsolDetachRequest("", null, null, () => ("ReceivedFFMAndDepartureMessage", ZString.Empty, ZString.Empty));
			messageHelper.Setup(m => m.IsAllowedToDetachShipments(consol, shipments)).Returns(detachRequest8);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals(true, Helper.IsAllowedToDetachShipments(messageHelper.Object, consol, shipments));
			AssertEquals("Question ReceivedFFMAndDepartureMessage", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestIsAllowedToDetachConsols()
		{
			AssertEquals("Default result for null arguments", false, Helper.IsAllowedToDetachConsols(null, null, null));

			var shipment = Factory.New<CommonShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var consols = new[] { consol1, consol2 };

			Factory.Save();

			var messageHelper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);

			// NOT Allowed
			var detachRequest = new ShipmentConsolDetachRequest("NOT ALLOWED!", () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); }, () => { throw new Exception("Should not be accessed"); });
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Error NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed, no additional dialogs
			var detachRequest2 = new ShipmentConsolDetachRequest("", null, null, null);
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest2);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("No dialogs", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			// Allowed -> CutOff confirmation -> NO
			var detachRequest3 = new ShipmentConsolDetachRequest("", () => "CUTOFF?", null, null);
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest3);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question CUTOFF?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> CutOff confirmation -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest3);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question CUTOFF?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> NO
			var detachRequest4 = new ShipmentConsolDetachRequest("", null, () => "DETACH SUBS?", null);
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest4);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> CANCEL
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest4);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> Detach SubShipments confirmation -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest4);
			messageHelper.Setup(m => m.DetachShipmentsFromConsols(new[] { shipment }, consols));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question DETACH SUBS?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedAcceptedMessage -> NO
			var detachRequest5 = new ShipmentConsolDetachRequest("", null, null, () => (ZString.Empty, "ReceivedAcceptedMessage?", ZString.Empty));
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest5);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question ReceivedAcceptedMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedAcceptedMessage -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest5);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question ReceivedAcceptedMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 AwaitingResponseMessage -> NO
			var detachRequest6 = new ShipmentConsolDetachRequest("", null, null, () => (ZString.Empty, ZString.Empty, "AwaitingResponseMessage?"));
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest6);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question AwaitingResponseMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 AwaitingResponseMessage -> YES
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest6);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertEquals(false, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question AwaitingResponseMessage?", UnitTestUserNotification.Instance.LastMessage.ToString());

			// Allowed -> ExportNotification755 ReceivedFFMAndDepartureMessage
			var detachRequest7 = new ShipmentConsolDetachRequest("", null, null, () => ("ReceivedFFMAndDepartureMessage", ZString.Empty, ZString.Empty));
			messageHelper.Setup(m => m.IsAllowedToDetachConsols(shipment, consols)).Returns(detachRequest7);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals(true, Helper.IsAllowedToDetachConsols(messageHelper.Object, shipment, consols));
			AssertEquals("Question ReceivedFFMAndDepartureMessage", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		#region TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenCCTHouseManifestIsSentFromConsol_WithAirTransportModeAndNonBRDestination()
		{
			var consol = CreateAndPopulateConsol("NZAKL", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest1"),
				Events.MessageSent,
				true,
				false,
				string.Empty);
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenCCTHouseManifestIsSentFromShipment_WithNonAirTransportModeAndNonBRDestination()
		{
			var consol = CreateAndPopulateConsol("NZAKL", Core.Constants.TransportModes.Sea);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest2"),
				Events.MessageSent,
				false,
				true,
				string.Empty);
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenCCTHouseManifestIsSentBothConsolAndShipment_WithAirTransportModeAndBRDestination()
		{
			var consol = CreateAndPopulateConsol("BRRIO", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest3"),
				Events.MessageSent,
				true,
				true,
				$"CCT House Manifest has been sent from Consol {consol.JK_UniqueConsignRef}. Please withdraw the message sent from above Consol prior to detaching shipment(s).");
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenCCTHouseManifestIsSentFromConsolOnly_WithAirTransportModeAndBRDestination()
		{
			var consol = CreateAndPopulateConsol("BRSAO", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest4"),
				Events.MessageSent,
				true,
				false,
				string.Empty);
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenLastEventSentFromTheShipmentIsIRJ_WithAirTransportModeAndBRDestination()
		{
			var consol = CreateAndPopulateConsol("BRSAO", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest5"),
				Events.InterchangeRejected,
				true,
				true,
				string.Empty);
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_WhenCCTReportWasSentFromConsolAndLastEventSentFromTheShipmentIsMAA_WithAirTransportModeAndBRDestination()
		{
			var consol = CreateAndPopulateConsol("BRRIO", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest6"),
				Events.MessageAccepted,
				true,
				true,
				$"CCT House Manifest has been sent from Consol {consol.JK_UniqueConsignRef}. Please withdraw the message sent from above Consol prior to detaching shipment(s).");
		}

		public void TestPreventDetachingShipmentWhenCCTHouseManifestHasBeenSent_LastEventSentFromTheShipmentIsMRJ_WithAirTransportModeAndBRDestination()
		{
			var consol = CreateAndPopulateConsol("BRRIO", Core.Constants.TransportModes.Air);

			TestShipmentDetachment(
				(CommonConsol)consol,
				AddShipmentToConsol(consol, "ShipmentTest7"),
				Events.MessageRejected,
				true,
				true,
				string.Empty);
		}

		void TestShipmentDetachment(
			CommonConsol consol,
			CommonShipment shipment,
			Event eventType,
			bool isCCTManifestEventSentFromConsol = false,
			bool isCCTManifestEventSentFromShipment = false,
			string expectedRestrictedMessage = null)
		{
			using (Factory.AddDisposableService())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(shipment.JS_RL_NKDestination.SubstringSafe(0, 2)))
			{
				var messageHelper = new ShipmentVsConsolMessageHelperTest();
				if (isCCTManifestEventSentFromShipment)
				{
					var documentData = CreateDocumentData(JobShipmentSchema.Constants.Prefix, shipment.PK, "AdvancedCargoReportBR");
					AddLog(documentData, eventType, ZDateTimeOffset.UtcToday.AddHours(-5), "Advanced Cargo Report");
				}

				if (isCCTManifestEventSentFromConsol)
				{
					var documentData = CreateDocumentData(JobConsolSchema.Constants.Prefix, consol.PK, "AdvancedManifestBR");
					AddLog(documentData, Events.MessageSent, ZDateTimeOffset.UtcToday.AddHours(-5), "Advanced Manifest");
				}

				var shipmentConsolDetachRequest = messageHelper.IsAllowedToDetachShipments(consol, new[] { shipment });

				if (expectedRestrictedMessage != null)
				{
					AssertEquals(expectedRestrictedMessage, shipmentConsolDetachRequest.RestrictedMessage);
				}
			}
		}

		#region CreateDocumentData

		IVisualizerDocumentData CreateDocumentData(string parentTableCode, ZGuid parentID, string dataStoreName)
		{
			var documentData = Factory.New<IVisualizerDocumentData>();
			var bizObj = (BusinessObject)documentData;
			bizObj[JobDocumentDataSchema.JDD_ParentTableCode] = parentTableCode;
			bizObj[JobDocumentDataSchema.JDD_ParentID] = parentID;
			bizObj[JobDocumentDataSchema.JDD_Name] = dataStoreName;

			return documentData;
		}

		#endregion

		#region AddLog

		void AddLog(IVisualizerDocumentData documentData, Event evenType, ZDateTimeOffset eventDateTime, string documentName)
		{
			var paramList = new List<KeyValuePair<string, string>>
							{
								new KeyValuePair<string, string>("MST", documentName),
								new KeyValuePair<string, string>("LOC", "BR"),
								new KeyValuePair<string, string>("DEP", "Customs")
							};

			((IStmALogProvider)documentData).Logs.AddNew(evenType, eventDateTime, paramList.ToArray());

			Thread.Sleep(1);
			Factory.Save();
		}

		#endregion

		#region CreateAndPopulateConsol

		IForwardingConsol CreateAndPopulateConsol(ZString dischargePort, ZString transportMode)
		{
			var consol = Factory.New<IForwardingConsol>();
			PopulateConsol(consol, dischargePort, transportMode);
			return consol;
		}

		#region PopulateConsol

		void PopulateConsol(IForwardingConsol consol, ZString dischargePort, ZString transportMode)
		{
			consol.JK_TransportMode = transportMode;
			consol.JK_MasterBillNum = "215-98757411";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_UniqueConsignRef = Guid.NewGuid().ToString().Substring(0, 9);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "ReceivingForwarder";
			receivingForwarder.OH_RL_NKClosestPort = dischargePort;
			receivingForwarder.MainAddress.Address1 = "Av Paulista 291";
			receivingForwarder.MainAddress.Address2 = "Consolacao";
			receivingForwarder.MainAddress.City = "Sao Paulo";
			receivingForwarder.MainAddress.Postcode = "11157802";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = dischargePort.SubstringSafe(0, 2);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SendingForwarder";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Av Paulista 291";
			sendingForwarder.MainAddress.Address2 = "Consolacao";
			sendingForwarder.MainAddress.City = "Salvador";
			sendingForwarder.MainAddress.Postcode = "11157802";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = dischargePort.SubstringSafe(0, 2);
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var transportLeg1 = consol.Transports_AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = consol.Transports_AddNew();
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "CLSCL";

			var transportLeg3 = consol.Transports_AddNew();
			transportLeg3.JW_RL_NKLoadPort = "CLSCL";
			transportLeg3.JW_RL_NKDiscPort = dischargePort;
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		#endregion

		CommonShipment AddShipmentToConsol(IForwardingConsol consol, ZString shipmentDescription)
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment = PopulateShipment(
				shipment,
				Guid.NewGuid().ToString("N").Substring(0, 15),
				12,
				25,
				shipmentDescription,
				consol.JK_TransportMode,
				consol.JK_RL_NKDischargePort);
			consol.AddShipment(shipment);
			Factory.Save();
			return (CommonShipment)shipment;
		}

		#region PopulateShipment

		IForwardingShipment PopulateShipment(IForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString desc, ZString transportMode, ZString dischargePort)
		{
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = desc;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;
			shipment.JS_RL_NKDestination = dischargePort;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = dischargePort;
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = dischargePort.SubstringSafe(0, 2);

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			return shipment;
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		readonly IShipmentVsConsolGUIMessageHelper Helper = ShipmentVsConsolGUIMessageHelper.Instance;

		class ShipmentVsConsolMessageHelperTest : ShipmentVsConsolMessageHelper
		{
			public ShipmentVsConsolMessageHelperTest() { }

			protected override string ConsolName_PluralLower => throw new NotImplementedException();

			protected override string ConsolName_SingularLower => throw new NotImplementedException();
		}
		#endregion
	}
}
