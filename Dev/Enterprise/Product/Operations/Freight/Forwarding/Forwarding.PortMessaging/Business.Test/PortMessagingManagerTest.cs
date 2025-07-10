using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestsSubclassesOf(typeof(PortMessagingManager))]
	abstract class PortMessagingManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewManager();
		}

		protected abstract PortMessagingManager GetNewManager();
		protected abstract PortMessagingManager GetNewManagerFromPopulatedBusinessObject(ForwardingConsol consol, ForwardingShipment shipment);

		public void TestDakosyRecipientCode()
		{
			AssertEquals("This code is used in eHub mapping and should not be changed w/o prior agreement with eHub and other involved parties", "DAKOSYHAM", PortMessagingManager.DakosyRecipientCode);
		}

		public void TestPreSendDataValidation_Entry_Type_SAC_ConsolContainerMode()
		{
			const string message = "Consol GLORIOUSCONSOL: Allowed container modes for entry type SAC are FCL, BCN, GRP.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GLORIOUSCONSOL";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipment = consol.Shipments.AddNew();

			var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			shipmentPortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_Entry_Type_SAC_AllowedContainerModes()
		{
			const string message = "Container 'CONB': Allowed container modes for entry type SAC are FCL, BCN, GRP.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONA";
			container1.JC_ContainerMode = Constants.ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONB";
			container2.JC_ContainerMode = Constants.ContainerModes.BuyersConsol;

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew().SetContainer(consol, container1);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container2);

			var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			shipmentPortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			container2.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentPortMessaging.JSM_EntryType = "XXX";
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_Entry_Type_SAC_AllShipmentPacklinesMustBePacked()
		{
			const string message = "Shipment SHIPPY: All packlines must be packed into containers for entry type SAC.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONA";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SHIPPY";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.SetContainer(consol, container1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol, container1);

			var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			shipmentPortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			packline2.SetContainer(consol, null);
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			packline2.SetContainer(consol, container1);
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		public void TestPreSendDataValidation_Entry_Type_SAC_ContainersMustHaveNumbers()
		{
			const string message = "All containers are required to have container number for entry type SAC.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONA";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONB";

			var shipment = consol.Shipments.AddNew();
			shipment.OuterPackLines.AddNew().SetContainer(consol, container1);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container2);

			var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			shipmentPortMessaging.JSM_MovementReferenceNumber = "123";
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			container2.JC_ContainerNum = "";
			AssertEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

			shipmentPortMessaging.JSM_EntryType = "XXX";
			AssertNotEquals(message, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}

		[TestDate(2023, 3, 1)]
		public void TestPreSendDataValidation_MustHasEORICodeOnConsolSendingForwarder()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 6, 6)))
			{
				const string expected = "When Entry Type is AEM, AE1 or DOX, the Consol > Sending Forwarder’s EORI number is mandatory.";

				var carrier = Factory.New<OrgHeader>();
				carrier.CustomsCodes.RemoveAndDeleteAll();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "ATVDD";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "ATVDD";
				transport1.JW_RL_NKDiscPort = "DEHAM";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "DEHAM";
				transport2.JW_RL_NKDiscPort = "AUSYD";
				transport2.JW_TransportMode = "SEA";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_SZB = "12345";

				var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
				var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

				consol.JK_OA_SendingForwarderAddress = carrier.MainAddress.PK;
				carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI01234567890123", Constants.CountryCodes.Germany);

				AssertNotEquals(expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

				var codes = new[]
				{
					EntryTypeList.Codes.AE1ExportDeclaration,
					EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
					EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
				};

				Action<string> assertValidationResult = msg =>
				{
					foreach (var code in codes)
					{
						shipmentPortMessaging.JSM_EntryType = code;
						AssertEquals(msg, expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
					}

					shipmentPortMessaging.JSM_EntryType = "XXX";
					AssertNotEquals(expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
				};

				consol.JK_OA_SendingForwarderAddress = Guid.Empty;

				var message = "Should be equal to the exptected message as the Consol's Sending Forwarder is null.";
				assertValidationResult(message);

				consol.JK_OA_SendingForwarderAddress = carrier.MainAddress.PK;
				carrier.CustomsCodes.RemoveAndDeleteAll();

				message = "Should be equal to the exptected message as the Consol's Sending Forwarder does not have EORI number.";
				assertValidationResult(message);

				carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI01234567890123", Constants.CountryCodes.France);

				foreach (var code in codes)
				{
					shipmentPortMessaging.JSM_EntryType = code;
					AssertNotEquals("Not equal to the exptected message as EORI number is provided", expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
				}
			}
		}

		[TestDate(2023, 3, 1)]
		public void TestPreSendDataValidation_MustHasEORICodeOnConsolSendingForwarder_EORIAndLRNEffectiveDate()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				const string expected = "When Entry Type is AEM or DOX, the Consol > Sending Forwarder’s EORI number is mandatory.";

				var carrier = Factory.New<OrgHeader>();
				carrier.CustomsCodes.RemoveAndDeleteAll();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "ATVDD";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transport1 = consol.Transports[0];
				transport1.JW_RL_NKLoadPort = "ATVDD";
				transport1.JW_RL_NKDiscPort = "DEHAM";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "DEHAM";
				transport2.JW_RL_NKDiscPort = "AUSYD";
				transport2.JW_TransportMode = "SEA";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_SZB = "12345";

				var manager = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
				var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

				consol.JK_OA_SendingForwarderAddress = carrier.MainAddress.PK;
				carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI01234567890123", Constants.CountryCodes.Germany);

				AssertNotEquals(expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));

				var codes = new[]
				{
					EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
					EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
				};

				Action<string> assertValidationResult = msg =>
				{
					foreach (var code in codes)
					{
						shipmentPortMessaging.JSM_EntryType = code;
						AssertEquals(msg, expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
					}

					shipmentPortMessaging.JSM_EntryType = "XXX";
					AssertNotEquals(expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
				};

				consol.JK_OA_SendingForwarderAddress = Guid.Empty;

				var message = "Should be equal to the exptected message as the Consol's Sending Forwarder is null.";
				assertValidationResult(message);

				consol.JK_OA_SendingForwarderAddress = carrier.MainAddress.PK;
				carrier.CustomsCodes.RemoveAndDeleteAll();

				message = "Should be equal to the exptected message as the Consol's Sending Forwarder does not have EORI number.";
				assertValidationResult(message);

				carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI01234567890123", Constants.CountryCodes.France);

				foreach (var code in codes)
				{
					shipmentPortMessaging.JSM_EntryType = code;
					AssertNotEquals("Not equal to the exptected message as EORI number is provided", expected, manager.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
				}
			}
		}

		[TestDate(2021, 6, 1)]
		public void TestPreSendDataValidation_ResendingAfterCancellation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "TWTPE";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_Code = "covfefe";
			sendingForwarder.OH_FullName = "Sending Forwarder Name";

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "maersk";
			shippingLine.OH_FullName = "Shipping Line Name";

			var cto = Factory.New<OrgHeader>();
			cto.OH_Code = "cto";

			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "voyageNo";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "DEHAM";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-7);

			var result = voyage.Sailings.AddNew();
			result.JX_JA = origin.PK;

			var transport = consol.Transports[0];
			transport.JW_JX = result.PK;
			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_ETD = ZDateTime.Today.AddDays(-7);

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.Addresses[0].PK;
			consol.JK_OA_ShippingLineAddress = shippingLine.Addresses[0].PK;
			consol.JK_OA_DepartureCTOAddress = cto.Addresses[0].PK;
			consol.Voyage.Origins[0].JA_DepartReference = "REF111111";
			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZZZ", Constants.CountryCodes.Germany);
			consol.ShippingLine.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "DDD", Constants.CountryCodes.Germany);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_SZB = "1245253";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;

			var messagingStatus = new PortMessagingStatusRetriever(consol);
			var portMessaging = GetNewManagerFromPopulatedBusinessObject(consol, shipment);
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "X123", "DE");
			sendingForwarder.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "ZAP", "DE");
			cto.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "CTO123", "DE");

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			consol.GetLogs().AddNew(Events.MessageWithdrawCancelRequest, "Cancel me|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			consol.GetLogs().AddNew(Events.MessagePendingProcessing, "Processing cancellation|DEP=Dakosy|MST=Port Order with HDS");
			consol.JK_SZB = "123512";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			shipment.GetLogs().AddNew(Events.MessageWithdrawCancelRequest, "Cancel me|DEP=Dakosy|MST=Port Order with HDS");
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			shipment.GetLogs().AddNew(Events.MessagePendingProcessing, "Processing cancellation|DEP=Dakosy|MST=Port Order with HDS");
			shipment.JS_SZB = "1245253";
			Factory.Save();

			Assert("Precondition: message sent status should include the word 'cancel'.", messagingStatus.SentStatus.Contains("cancel", StringComparison.OrdinalIgnoreCase));
			AssertEquals("Precondition: message should be 'pending processing'.", "Message Pending Processing", messagingStatus.MessageStatus);
			AssertEquals("Should not be allowed to send if cancellation is pending processing", "Dakosy status is Pending.\r\nAn Acceptance or Rejection must be received before you can send additional messages to Dakosy.", portMessaging.RunPreSendDataValidation(PortMessagingManager.MessageType.PortOrderWithHDS));
		}
	}
}
