using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(CarrierMessageData))]
	sealed class CarrierMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		#region Basic Population

		public void TestShippingInstruction_PopulateFromForwardingConsol_Agent_Sea()
		{
			agentType = AgentType.Agent;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.FCL;
			builderType = CarrierBuilder.ShippingInstruction;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();
			AssertWrapperIsPopulated(wrapper);
		}

		public void TestBookingRequest()
		{
			agentType = AgentType.Agent;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.FCL;
			builderType = CarrierBuilder.BookingRequest;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();
			AssertWrapperIsPopulated(wrapper);
		}

		void AssertWrapperIsPopulated(CarrierMessageData wrapper)
		{
			AssertEquals("NumberOfCopies", 3, wrapper.NumberOfCopies);
			AssertEquals("NumberOfOriginals", 4, wrapper.NumberOfOriginals);

			AssertEquals("ContainerMode", "FCL", wrapper.ContainerMode.Code);
			AssertEquals("DateOfIssue", new ZDateTime(2018, 2, 19), wrapper.DateOfIssue);

			AssertEquals("PickupFrom", ZBool.True, wrapper.IsDoorPickup);
			AssertEquals("DeliverTo", ZBool.True, wrapper.IsDoorDelivery);

			AssertEquals("ReleaseType.Code", "BOL", wrapper.ReleaseType.Code);
			AssertEquals("ReleaseType.Description", "BOL Original", wrapper.ReleaseType.Description);

			AssertEquals("BookingReference.Code", "BOOKINGREF01", wrapper.BookingReference);

			#region Ports

			AssertEquals("VoyageFlightNumber", "F9999", wrapper.Transports.Main.VoyageFlightNumber);
			AssertEquals("Vessel.Name", "BUNGA XYLIMA", wrapper.Transports.Main.Vessel.Name);
			AssertEquals("Vessel.LloydsIMO", "8907993", wrapper.Transports.Main.Vessel.LloydsIMO);
			AssertEquals("PortOfLoading.Code", "AUSYD", wrapper.Transports.Main.PortOfLoading.Code);
			AssertEquals("PortOfDischarge.Code", "NZAKL", wrapper.Transports.Main.PortOfDischarge.Code);

			AssertEquals("Origin.Code", "AUSYD", wrapper.Origin.Code);
			AssertEquals("Destination.Code", "NZAKL", wrapper.Destination.Code);
			AssertEquals("PlaceOfReceipt.Code", "AUSYD", wrapper.PlaceOfReceipt.Code);
			AssertEquals("PlaceOfIssue.Code", "AUSYD", wrapper.PlaceOfIssue.Code);
			AssertEquals("PlaceOfDelivery.Code", "NZAKL", wrapper.PlaceOfDelivery.Code);

			#endregion

			#region Addresses

			AssertAddressData(consol.SendingForwarder, wrapper.Shipper, false);
			AssertAddressData(consol.ShippingLine, wrapper.Carrier, false);
			AssertAddressData(consol.ReceivingForwarder, wrapper.Consignee, false);
			AssertAddressData(consol.NotifyParty, wrapper.NotifyParty);
			AssertAddressData(consol.NotifyParty2, wrapper.NotifyParty2);
			AssertAddressData(consol.NotifyParty3, wrapper.NotifyParty3);
			AssertAddressData(consol.SendingForwarder, wrapper.Forwarder, false);
			AssertEquals("Buyer should be empty address", ZString.Empty, wrapper.Buyer.CompanyName);
			AssertEquals("FreightPayer should be empty address", ZString.Empty, wrapper.FreightPayer.CompanyName);
			AssertAddressData(shipment.ConsignorPickupAddress, wrapper.PickupFrom);
			AssertAddressData(shipment.ConsigneeDeliveryAddress, wrapper.DeliverTo);

			#endregion
		}

		#endregion

		#region Correct Recipient

		public void TestCorrectRecipientIsChosen_Default()
		{
			agentType = AgentType.Agent;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.FCL;
			builderType = CarrierBuilder.ShippingInstruction;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Carrier);
			AssertEquals(wrapper.RecipientType, "Carrier");

			isShippingLine = true;
			isNVO = false;
			wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Carrier);
			AssertEquals(wrapper.RecipientType, "Carrier");

			isNVO = true;
			wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Carrier);
			AssertEquals(wrapper.RecipientType, "Carrier");
		}

		public void TestCorrectRecipientIsChosen_CoLoad()
		{
			agentType = AgentType.CoLoad;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.FCL;
			builderType = CarrierBuilder.ShippingInstruction;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Creditor);
			AssertEquals(wrapper.RecipientType, "Co-Load With");
		}

		public void TestCorrectRecipientIsChosen_CarrierNVOCC()
		{
			agentType = AgentType.Agent;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.LCL;
			builderType = CarrierBuilder.ShippingInstruction;
			isNVO = true;
			isShippingLine = false;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Carrier);
			AssertEquals(wrapper.RecipientType, "Carrier (NVOCC)");

			isShippingLine = true;
			wrapper = (CarrierMessageData)GetNewBusinessObject();

			AssertEquals(wrapper.Recipient, wrapper.Carrier);
			AssertEquals(wrapper.RecipientType, "Carrier (NVOCC)");
		}

		#endregion

		#region Validation

		public void TestSameAsConsigneeToOrderValidation()
		{
			consol = Factory.New<ForwardingConsol>();
			var wrapper = GetBuilder().Build();
			wrapper.Consignee.CompanyName = "TO ORDER";
			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";

			var consigneeAddress = wrapper.Consignee;
			var notifyAddress = wrapper.NotifyParty;

			AssertHasMessageError(consigneeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertHasMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			wrapper.Consignee.CompanyName = "Consignee A";
			wrapper.Consignee.AddressLine1 = "consignee address 1";
			wrapper.Consignee.Country.Name = "Heaven";

			AssertNoMessageError(consigneeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertNoMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			wrapper.Consignee.CompanyName = "TO ORDER";

			AssertHasMessageError(consigneeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertHasMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");

			wrapper.NotifyParty.CompanyName = "notify name A";
			wrapper.NotifyParty.AddressLine1 = "notify address 1";
			wrapper.NotifyParty.Country.Name = "Super Heaven";

			AssertNoMessageError(consigneeAddress.CompanyNameInfo, "Consignee name and address information is required, when Notify Party is empty or SAME AS CONSIGNEE.");
			AssertNoMessageError(notifyAddress.CompanyNameInfo, "Notify Party name and address information is required, when Consignee is empty or TO ORDER.");
		}

		#endregion

		#region TestInvalidIssueDateOverrideDoesNotThrowAnException

		public void TestInvalidIssueDateOverrideDoesNotThrowAnException()
		{
			agentType = AgentType.Agent;
			transportMode = TransportModes.Sea;
			containerMode = ContainerModes.FCL;
			builderType = CarrierBuilder.BookingRequest;

			var wrapper = (CarrierMessageData)GetNewBusinessObject();
			wrapper.DateOfIssue = new ZDateTime(2021, 1, 26);
			var dynamicData = wrapper.MakeDocDataDynamic();

			var dateOfIssue = dynamicData.GetDynamicProperty(nameof(wrapper.DateOfIssue));
			dateOfIssue.SetValue(ZDateTime.Invalid);

			var changeset = dynamicData.GetOverriddenValuesXml();

			dynamicData = wrapper.MakeDocDataDynamic();
			dynamicData.MergeDataFromXml(changeset);

			dateOfIssue = dynamicData.GetDynamicProperty(nameof(wrapper.DateOfIssue));
			AssertEquals("invalid date has been set", ZDateTime.Invalid, dateOfIssue.Value);
		}
		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = agentType;
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = containerMode;
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_AgentsReference = "AgentRef002";
			consol.JK_MasterBillNum = "1112222222";
			consol.JK_CoLoadMasterBill = "COLOAD004";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_ReleaseType = ZString.Empty;
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 4;
			consol.JK_BookingReference = "BOOKINGREF01";
			consol.JK_CoLoadBookingReference = "COLOADREF02";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 2, 19);

			var mainTransport = consol.Transports[0];
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "BUNGA XYLIMA";
			vessel.RV_LloydsNumber = "8907993";

			mainTransport.JW_Vessel = vessel.RV_FK;
			mainTransport.JW_VoyageFlight = "F9999";

			var otherTransport = consol.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CFS/CFS";

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			sendingForwarder.MainAddress.OA_Email = "yummy@test.com";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "JIMMY";
			receivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Auckland";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyPartyDocumentaryAddress = Factory.New<OrgHeader>();
			notifyPartyDocumentaryAddress.OH_FullName = "DHL1";
			notifyPartyDocumentaryAddress.OH_RL_NKClosestPort = "NZAKL";
			notifyPartyDocumentaryAddress.MainAddress.Address1 = "Unit 399";
			notifyPartyDocumentaryAddress.MainAddress.Address2 = "41 pitt st";
			notifyPartyDocumentaryAddress.MainAddress.City = "Auckland";
			notifyPartyDocumentaryAddress.MainAddress.Postcode = "5023";
			notifyPartyDocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "NZ";
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPartyDocumentaryAddress.MainAddress.PK;

			var notifyParty2DocumentaryAddress = Factory.New<OrgHeader>();
			notifyParty2DocumentaryAddress.OH_FullName = "DHL2";
			notifyParty2DocumentaryAddress.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2DocumentaryAddress.MainAddress.Address1 = "Unit 400";
			notifyParty2DocumentaryAddress.MainAddress.Address2 = "42 pitt st";
			notifyParty2DocumentaryAddress.MainAddress.City = "Auckland";
			notifyParty2DocumentaryAddress.MainAddress.Postcode = "5023";
			notifyParty2DocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "NZ";
			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2DocumentaryAddress.MainAddress.PK;

			var notifyParty3DocumentaryAddress = Factory.New<OrgHeader>();
			notifyParty3DocumentaryAddress.OH_FullName = "DHL3";
			notifyParty3DocumentaryAddress.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3DocumentaryAddress.MainAddress.Address1 = "Unit 401";
			notifyParty3DocumentaryAddress.MainAddress.Address2 = "43 pitt st";
			notifyParty3DocumentaryAddress.MainAddress.City = "Auckland";
			notifyParty3DocumentaryAddress.MainAddress.Postcode = "5023";
			notifyParty3DocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "NZ";
			consol.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3DocumentaryAddress.MainAddress.PK;

			var departureCFSDocAddress = Factory.New<OrgHeader>();
			departureCFSDocAddress.OH_FullName = "DEPTCFS";
			departureCFSDocAddress.OH_RL_NKClosestPort = "AUSYD";
			departureCFSDocAddress.MainAddress.Address1 = "Unit 16";
			departureCFSDocAddress.MainAddress.Address2 = "7 Lost Lane";
			departureCFSDocAddress.MainAddress.City = "Sydney";
			departureCFSDocAddress.MainAddress.Postcode = "2000";
			departureCFSDocAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = departureCFSDocAddress.MainAddress.PK;

			var arrivalCFSDocAddress = Factory.New<OrgHeader>();
			arrivalCFSDocAddress.OH_FullName = "ARRIVALCFS";
			arrivalCFSDocAddress.OH_RL_NKClosestPort = "AUSYD";
			arrivalCFSDocAddress.MainAddress.Address1 = "Unit 19";
			arrivalCFSDocAddress.MainAddress.Address2 = "10 Lost Lane";
			arrivalCFSDocAddress.MainAddress.City = "Sydney";
			arrivalCFSDocAddress.MainAddress.Postcode = "2000";
			arrivalCFSDocAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_UnpackDepotAddress = arrivalCFSDocAddress.MainAddress.PK;

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPLINE";
			shippingLine.OH_RL_NKClosestPort = "AUSYD";
			shippingLine.MainAddress.Address1 = "Unit 201";
			shippingLine.MainAddress.Address2 = "56 why Lane";
			shippingLine.MainAddress.City = "Sydney";
			shippingLine.MainAddress.Postcode = "2000";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";
			shippingLine.MainAddress.OA_Email = "yummy@test.com";
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			if (isNVO || isShippingLine)
			{
				var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				refShippingLine.RSL_IsNVO = isNVO;
				refShippingLine.RSL_IsShippingLine = isShippingLine;
				shippingLine.OH_RSL_ShippingLine = refShippingLine.PK;
			}

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "CREDITOR";
			creditor.OH_RL_NKClosestPort = "AUSYD";
			creditor.MainAddress.Address1 = "Unit 202";
			creditor.MainAddress.Address2 = "56 why Lane";
			creditor.MainAddress.City = "Sydney";
			creditor.MainAddress.Postcode = "2000";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";
			creditor.MainAddress.OA_Email = "yummy@test.com";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "ShipBookRef003";
			shipment.JS_HouseBill = "3334444444";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_ReleaseType = "EBL";
			shipment.JS_GoodsValue = 9200.13m;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_Email = "maersk@test.com";
			shipment.ConsignorDocumentaryAddress.E2_Contact = "Maersk Contact";

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_Email = "DUMMY@test.com";
			shipment.ConsigneeDocumentaryAddress.E2_Contact = "DUMMY Contact";

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_Email = "DUMMY@test.com";
			shipment.NotifyPartyDocumentaryAddress.E2_Contact = "DUMMY Contact";

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "NotifyParty2";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 889";
			notifyParty2.MainAddress.Address2 = "9 Pitt St";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5012";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "NotifyParty3";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 890";
			notifyParty3.MainAddress.Address2 = "10 Pitt St";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5012";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.OH_RL_NKClosestPort = "NZAKL";
			buyer.MainAddress.Address1 = "Unit 891";
			buyer.MainAddress.Address2 = "11 Pitt St";
			buyer.MainAddress.City = "Auckland";
			buyer.MainAddress.Postcode = "5012";
			buyer.MainAddress.OA_RN_NKCountryCode = "NZ";
			shipment.BuyerDocAddress.E2_OA_Address = buyer.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "CONSPA";
			consignorPickupAddress.OH_RL_NKClosestPort = "AUSYD";
			consignorPickupAddress.MainAddress.Address1 = "Unit 15";
			consignorPickupAddress.MainAddress.Address2 = "5 Lost Lane";
			consignorPickupAddress.MainAddress.City = "Sydney";
			consignorPickupAddress.MainAddress.Postcode = "2000";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "CONDELA";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "AUSYD";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 17";
			consigneeDeliveryAddress.MainAddress.Address2 = "7 Lost Lane";
			consigneeDeliveryAddress.MainAddress.City = "Sydney";
			consigneeDeliveryAddress.MainAddress.Postcode = "2000";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 2;

			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.Origin = PaymentParty.Consignee;
			def.Loading = PaymentParty.Consignee;
			def.Freight = PaymentParty.Consignee;
			def.Insurance = PaymentParty.Consignee;
			def.Unloading = PaymentParty.Consignee;
			def.Destination = PaymentParty.Consignee;
			def.Brokerage = PaymentParty.Consignee;
			def.CustomsDuty = PaymentParty.Consignee;
			def.OriginBrokerage = PaymentParty.Consignee;
			def.IncoTerm = IncoTerms.CostAndFreight;

			parameters = new DummyDocDataObjectParameters()
			{
				LogProvider = Factory.New<DummyEnterpriseBusinessObject>()
			};

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			{
				var data = GetBuilder().Build();
				return data;
			}
		}

		CarrierMessageDataBuilder GetBuilder()
		{
			switch (builderType)
			{
				case CarrierBuilder.ShippingInstruction:
					return new ShippingInstructionBuilder(consol);

				case CarrierBuilder.BookingRequest:
					return new BookingRequestBuilder(consol, parameters);

				default:
					throw new InvalidOperationException("builderType is not set.");
			}
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		IDocDataObjectParameters parameters;
		ZString agentType;
		ZString transportMode;
		ZString containerMode;
		ZBool isNVO;
		ZBool isShippingLine;

		enum CarrierBuilder { ShippingInstruction, BookingRequest }
		CarrierBuilder builderType;

		#endregion
	}
}
