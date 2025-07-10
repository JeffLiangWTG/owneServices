using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class CarrierHouseBillBuilderTest : TestCaseWithFactory
	{
		[SnailTest]
		public void TestBuild()
		{
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

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipment();
				AddDataLinkedEvent(shipment, UXMLMessage);

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "COPY"
				};

				var houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();

				CombineAssertions(() =>
				{
					// From shipment/consol
					AssertEquals("IsDraft", true, houseBill.IsDraft);
					AssertEquals("IsOriginal", false, houseBill.IsOriginal);
					AssertEquals("HouseBillNumber", "HOUSEBILL001", houseBill.HouseBillNumber);
					AssertEquals("ShipmentNumber", "SH0001", houseBill.ShipmentNumber);
					AssertEquals("ShippersReference", "BKG000001", houseBill.ShippersReference);
					AssertEquals(nameof(HouseBill.CarrierBookingReference), "BookingRef",
						houseBill.CarrierBookingReference);
					AssertEquals(nameof(HouseBill.CoLoadBookingReference), "CoLoadBookingRef",
						houseBill.CoLoadBookingReference);
					AssertEquals("GoodsDescription", "Refer to Pack Lines", houseBill.GoodsDescription);
					AssertEquals("MarksAndNumbers", "marks & numbers", houseBill.MarksAndNumbers);

					AssertEquals("ShipperLoadAndCount.Code", "SLC", houseBill.ShipperLoadAndCount.Code);
					AssertEquals("ShipperLoadAndCount.Description", "Shipper Load and Count",
						houseBill.ShipperLoadAndCount.Description);

					AssertEquals("INCO.Code", "CFR", houseBill.INCO.Code);
					AssertEquals("INCO.Description", "Cost And Freight", houseBill.INCO.Description);

					AssertEquals("CustomsEntryNumber", "T7HRTXGXT", houseBill.CustomsEntryNumber.Value);
					AssertEquals("CustomsEntryNumber", CANType.ContingencyCustomsAuthorityNumber.Code,
						houseBill.CustomsEntryNumber.Type.Code);
					AssertEquals("CustomsEntryNumber", CANType.ContingencyCustomsAuthorityNumber.Description,
						houseBill.CustomsEntryNumber.Type.Description);

					AssertEquals("MoveTypeList", "CFS, CY, DOOR", houseBill.MoveTypeList.CodesAsString);

					AssertEquals("NumberOfCopies", 1, houseBill.NumberOfCopies);
					AssertEquals("NumberOfOriginals", 2, houseBill.NumberOfOriginals);

					AssertEquals("ReleaseType.Code", ShipmentReleaseTypes.SeaWaybill, houseBill.ReleaseType.Code);
					AssertEquals("ReleaseType.Description", "Sea Waybill", houseBill.ReleaseType.Description);

					AssertEquals("HouseBillOfLadingType.Code", "FIA", houseBill.HouseBillOfLadingType.Code);

					AssertEquals("ContainerMode.Code", Core.Constants.ContainerModes.FCL, houseBill.ContainerMode.Code);
					AssertEquals("ContainerMode.Description", "Full Container Load",
						houseBill.ContainerMode.Description);

					AssertEquals("MainTransport.Vessel.Name", "Vessel", houseBill.Transports.Main.Vessel.Name);
					AssertEquals("MainTransport.Vessel.LloydsIMO", ZString.Empty,
						houseBill.Transports.Main.Vessel.LloydsIMO);
					AssertEquals("MainTransport.VoyageFlightNumber", "F9999",
						houseBill.Transports.Main.VoyageFlightNumber);

					AssertEquals("AUSYD", houseBill.PlaceOfReceipt.Code);
					AssertEquals("NZAKL", houseBill.PlaceOfDelivery.Code);

					AssertEquals("PortOfLoading.Code", "AUSYD", houseBill.PortOfLoading.Code);
					AssertEquals("PortOfDischarge.Code", "NZAKL", houseBill.PortOfDischarge.Code);

					AssertEquals("PortOfOrigin.Code", "AUSYD", houseBill.PortOfOrigin.Code);
					AssertEquals("PortOfDestination.Code", "NZAKL", houseBill.PortOfDestination.Code);

					AssertEquals("PaymentTerms.Code", "CCX", houseBill.PaymentTerms.Code);
					AssertEquals("PaymentTerms.Description", "Freight Collect", houseBill.PaymentTerms.Description);

					AssertEquals("FreightPayableAt.Code", shipment.Destination.Code, houseBill.FreightPayableAt.Code);

					AssertEquals("ShippedOnBoard.Code", "SHP", houseBill.ShippedOnBoard.Code);
					AssertEquals("ShippedOnBoard.Description", "Shipped", houseBill.ShippedOnBoard.Description);
					AssertEquals("ShippedOnBoard.Date", ZDate.Today, houseBill.ShippedOnBoard.Date);

					AssertEquals("DepartureDate", ZDate.Today.AddDays(1), houseBill.DepartureDate);
					AssertEquals("ArrivalDate", ZDate.Today.AddDays(2), houseBill.ArrivalDate);

					AssertEquals("GoodsDetailsTextOverride", ZString.Empty, houseBill.GoodsDetailsTextOverride);
					AssertEquals("ChargesTextOverride", ZString.Empty, houseBill.ChargesTextOverride);
					AssertEquals("FollowOnTextOverride", ZString.Empty, houseBill.FollowOnTextOverride);

					AssertEquals("AsAgentDetail exists", ZString.Empty, houseBill.AsAgentDetail);
					AssertEquals("FreightNominee exists", ZString.Empty, houseBill.FreightNominee);
					AssertEquals("CarrierAgent exists", ZString.Empty, houseBill.CarrierAgent);
					AssertEquals("HIR Reference", "SHP001", houseBill.HIRReference.Value);

					// From UXML
					AssertEquals("Shipper.CompanyName", "WiseTech 13 Ocean ST", houseBill.Shipper.CompanyName);
					AssertEquals("Consignee.CompanyName", "WiseTech Global China", houseBill.Consignee.CompanyName);
					AssertEquals("NotifyParty.CompanyName", "WiseTech Global China Bejing", houseBill.NotifyParty.CompanyName);
					AssertEquals("Notes - Payment Handling Instructions", "Freight - Prepaid", houseBill.Notes.FirstOrDefault(n => n.Description == "Payment Handling Instructions").Text);

					AssertEquals("Shipper.CompanyName", "WiseTech 13 Ocean ST", houseBill.Shipper.CompanyName);
					AssertEquals("Consignee.CompanyName", "WiseTech Global China", houseBill.Consignee.CompanyName);
					AssertEquals("NotifyParty.CompanyName", "WiseTech Global China Bejing", houseBill.NotifyParty.CompanyName);
				});

				AssertEquals("Containers.Count", 1, houseBill.Containers.Count);
				AssertEquals("Container PackingLines.Count", 2, houseBill.Containers.Single().PackingLines.Count);
				AssertEquals("LoosePackingLines.Count", 1, houseBill.LoosePackingLines.Count);
				AssertEquals("I'm loose baby!", houseBill.LoosePackingLines.Single().GoodsDescription);
				AssertContainerData(shipment.DepartureConsol.Containers.OfType<ForwardingContainer>().Single(),
					houseBill.Containers.Single());

				AssertEquals("expected no SubHouseBills", 0, houseBill.SubHouseBills.Count);
			}
		}

		[SnailTest]
		public void TestBuildWithColoadShipments()
		{
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipment();

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "COPY"
				};

				shipment.JS_ShipmentType = ShipmentTypes.CoLoadMaster;
				var subShipment1 = shipment.CoLoadShipments.AddNew();
				subShipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				subShipment1.JS_UniqueConsignRef = "S00001400";
				subShipment1.JS_GoodsDescription = "Desc S00001400";

				var subShipment2 = shipment.CoLoadShipments.AddNew();
				subShipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
				subShipment2.JS_UniqueConsignRef = "S00001401";
				subShipment2.JS_GoodsDescription = "Desc S00001401";

				AddDataLinkedEvent(shipment, UXMLMessageWithSubShipments);
				var houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();

				AssertEquals("Goods Desc. Master", houseBill.GoodsDescription);
				AssertEquals(2, houseBill.SubHouseBills.Count);

				var subHouseBill1 = houseBill.SubHouseBills.FirstOrDefault(s => s.ShipmentNumber == "S00001400");
				var subHouseBill2 = houseBill.SubHouseBills.FirstOrDefault(s => s.ShipmentNumber == "S00001401");

				AssertNotNull(subHouseBill1);
				AssertNotNull(subHouseBill2);
				AssertEquals("Goods Desc. S00001400", subHouseBill1.GoodsDescription);
				AssertEquals("Goods Desc. S00001401", subHouseBill2.GoodsDescription);
			}
		}

		[SnailTest]
		public void TestBuildWithoutUXML()
		{
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

			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incoTermDefinitions))
			using (FreightDataRegistry.Instance.BOLClause.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Oh my gut."))
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipment();

				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "COPY"
				};

				var houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();

				CombineAssertions(() =>
				{
					AssertEquals("GoodsDescription", "goods description", houseBill.GoodsDescription);
					AssertEquals("Shipper.CompanyName", "MAERSK", houseBill.Shipper.CompanyName);
					AssertEquals("Consignee.CompanyName", "DUMMY", houseBill.Consignee.CompanyName);
					AssertEquals("NotifyParty.CompanyName", "FUNNY", houseBill.NotifyParty.CompanyName);
				});
			}
		}

		#region TestIsElectronicBOL

		public void TestIsElectronicBOL()
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GNACC";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.IsEditingElectronicBOL = true;

			var houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();
			Assert(!houseBill.IsElectronicBOL);

			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();
			Assert(houseBill.IsElectronicBOL);

			shipment.IsEditingElectronicBOL = false;
			houseBill = new CarrierHouseBillBuilder(shipment, parameters).Build();
			Assert(!houseBill.IsElectronicBOL);
		}

		#endregion

		public void TestAddMissingMainTransportValidation()
		{
			var errorMessage = "Main Sea transport leg is missing in the Routing tab. Please enter it in Consol or Shipment.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BRABT";
			shipment.JS_HouseBillOfLadingType = HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.JS_TransportMode = TransportModes.Sea;

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};

			var houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertHasMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "iddqd";
			vessel.RV_Name = "vessel1";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "BRABT";
			transport.JW_Vessel = "vessel1";

			houseBill = new HouseBillBuilder(shipment, parameters).Build();

			AssertNoMessageError(houseBill.ErrorPlaceHolderInfo, errorMessage);
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_HouseBillOfLadingType = "FIA";

			shipment.CustomsEntryNumber = "T7HRTXGXT";
			shipment.CustomsEntryNumberType = CANType.ContingencyCustomsAuthorityNumber.Code;

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var mainTransport = shipment.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = TransportModes.Sea;
			mainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "AUSYD";
			mainTransport.JW_RL_NKDiscPort = "NZAKL";

			var otherTransport = shipment.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "NZAKL";
			otherTransport.JW_RL_NKDiscPort = "NZALR";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = "SEA";
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "CNCAN";
			departureConsol.JK_BookingReference = "BookingRef";
			departureConsol.JK_CoLoadBookingReference = "CoLoadBookingRef";

			var consolTransport = departureConsol.Transports[0];
			consolTransport.JW_Vessel = "Vessel";
			consolTransport.JW_VoyageFlight = "F9999";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerMode = ContainerModes.FCL;

			var packline1 = shipment.OuterPackLines.Single();
			container.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_ActualWeight = 2000;
			packline2.JL_ActualWeightUQ = Weight.Kilograms;
			packline2.JL_ActualVolume = 1.3;
			packline2.JL_ActualVolumeUQ = Volume.CubicMetres;
			container.PackLines.Add(packline2);

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_ActualWeight = 3000;
			loosePackLine.JL_ActualWeightUQ = Weight.Kilograms;
			loosePackLine.JL_ActualVolume = 1.3;
			loosePackLine.JL_ActualVolumeUQ = Volume.CubicMetres;
			loosePackLine.JL_Description = "I'm loose baby!";

			loosePackLine.Containers.RemoveAll();

			Assert("prerequisite: loose packline should be loose", !loosePackLine.Containers.Any());

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_TransportMode = "SEA";
			arrivalConsol.JK_RL_NKLoadPort = "CNCAN";
			arrivalConsol.JK_RL_NKDischargePort = "NZAKL";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			departureConsol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "JIMMY";
			receivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Auckland";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";

			departureConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "MANY";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "TOO MUCH";
			notifyParty3.OH_RL_NKClosestPort = "NZAKL";
			notifyParty3.MainAddress.Address1 = "Unit 686";
			notifyParty3.MainAddress.Address2 = "99 How Lane";
			notifyParty3.MainAddress.City = "Auckland";
			notifyParty3.MainAddress.Postcode = "5038";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 2;

			var hir = Factory.New<CusEntryNumber>();
			hir.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			hir.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			hir.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			hir.CE_EntryNum = "SHP001";
			shipment.Numbers.Add(hir);

			return shipment;
		}

		void AddDataLinkedEvent(ForwardingShipment shipment, string xml)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					ConsolDocumentNames.ShippingInstruction)
			};

			shipment.Logs.CreateOrRecreateEventLog(
				Events.DataLinked,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddDays(-1),
				ZString.Empty,
				eventParameters);

			var dataLinkedLog = shipment.Logs.Find(log => log.SL_SE_NKEvent == Events.DataLinkedCode).First();

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			message.Content = XElement.Parse(xml);

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dataLinkedLog.PK;
			pivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
		}

		const string UXMLMessage = @"<UniversalShipment>
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<TriggerDate>2021-11-10T18:20:16.917</TriggerDate>
			<DocumentaryOverride>
				<DataVersion>1</DataVersion>
				<DocumentName>Shipping Instruction</DocumentName>
				<Purpose>
					<Code>ORG</Code>
					<Description>Original</Description>
				</Purpose>
			</DocumentaryOverride>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>NVO</Code>
					<Description>NVOCC</Description>
					<ServiceCode>SIN</ServiceCode>
					<ServiceDescription>Shipping Instruction</ServiceDescription>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<AgentsReference>BKG000001</AgentsReference>
		<ContainerMode>
			<Code>LCL</Code>
			<Description>Less Container Load</Description>
		</ContainerMode>
		<CoLoadBookingConfirmationReference>SH0001</CoLoadBookingConfirmationReference>
		<CoLoadMasterBillNumber>HOUSEBILL001</CoLoadMasterBillNumber>
		<GoodsDescription>Refer to Pack Lines</GoodsDescription>
		<GoodsValue>0</GoodsValue>
		<GoodsValueCurrency>
			<Code>AUD</Code>
			<Description>Australian Dollar</Description>
		</GoodsValueCurrency>
		<HBLContainerPackModeOverride>DOOR/DOOR</HBLContainerPackModeOverride>
		<NoCopyBills>3</NoCopyBills>
		<NoOriginalBills>3</NoOriginalBills>
		<PaymentMethod>
			<Code>PPD</Code>
			<Description>Prepaid</Description>
		</PaymentMethod>
		<PlaceOfIssue>
			<Code>HKHKG</Code>
			<Name>Hong Kong</Name>
		</PlaceOfIssue>
		<PlaceOfDelivery>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PlaceOfDelivery>
		<PlaceOfReceipt>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PlaceOfReceipt>
		<PortOfOrigin>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfOrigin>
		<PortOfDestination>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PortOfDestination>
		<PortOfLoading>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfLoading>
		<PortOfDischarge>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PortOfDischarge>
		<ReleaseType>
			<Code>BOL</Code>
			<Description>BOL Original</Description>
		</ReleaseType>
		<ShipmentIncoTerm>
			<Code>CFR</Code>
			<Description>Cost And Freight</Description>
		</ShipmentIncoTerm>
		<ShipmentType>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ShipmentType>
		<TransportMode>
			<Code>SEA</Code>
			<Description>Sea</Description>
		</TransportMode>
		<LocalProcessing>
			<DeliveryRequiredBy>2021-11-20T16:30:00</DeliveryRequiredBy>
			<PickupRequiredBy />
		</LocalProcessing>
		<AdditionalReferenceCollection>
			<AdditionalReference>
				<Type>
					<Code>BOL</Code>
					<Description>Bill Of Lading Number</Description>
				</Type>
				<ReferenceNumber>232434I454</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type>
					<Code>FFW</Code>
					<Description>Freight Forwarder Reference</Description>
				</Type>
				<ReferenceNumber>CYYY00693208</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type>
					<Code>HIR</Code>
					<Description>eHub Interchange Reference</Description>
				</Type>
				<ReferenceNumber>CON0000001213</ReferenceNumber>
			</AdditionalReference>
		</AdditionalReferenceCollection>
		<BillOfLadingClauseCollection>
			<BillOfLadingClause>
				<Type>
					<Code>FPP</Code>
					<Description>Freight Prepaid</Description>
				</Type>
			</BillOfLadingClause>
			<BillOfLadingClause>
				<Type>
					<Code>SLC</Code>
					<Description>Shipper Load and Count</Description>
				</Type>
			</BillOfLadingClause>
		</BillOfLadingClauseCollection>
		<ContainerCollection>
			<Container>
				<AirVentFlow>0</AirVentFlow>
				<ContainerCount>1</ContainerCount>
				<ContainerNumber>TTSU1234566</ContainerNumber>
				<ContainerType>
					<Code>20GP</Code>
					<Category>
						<Code>DRY</Code>
						<Description>Dry Storage</Description>
					</Category>
					<Description>twenty foot insulated container</Description>
					<ISOCode>22G0</ISOCode>
				</ContainerType>
				<DeliveryMode>CFS/CFS</DeliveryMode>
				<GrossWeight>36323.000</GrossWeight>
				<GrossWeightVerificationType>
					<Code>NON</Code>
					<Description>Not Verified</Description>
				</GrossWeightVerificationType>
				<HumidityPercent>0</HumidityPercent>
				<IsControlledAtmosphere>false</IsControlledAtmosphere>
				<IsEmptyContainer>false</IsEmptyContainer>
				<IsShipperOwned>false</IsShipperOwned>
				<LengthUnit>
					<Code>FT</Code>
					<Description>Feet</Description>
				</LengthUnit>
				<Link>1</Link>
				<OrganizationAddressCollection />
				<OverhangBack>0</OverhangBack>
				<OverhangFront>0</OverhangFront>
				<OverhangHeight>0</OverhangHeight>
				<OverhangLeft>0</OverhangLeft>
				<OverhangRight>0</OverhangRight>
				<Seal>3343</Seal>
				<SealPartyType>
					<Code>CAR</Code>
					<Description>Carrier</Description>
				</SealPartyType>
				<SetPointTemp>0.000</SetPointTemp>
				<TareWeight>1980.000</TareWeight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
			</Container>
		</ContainerCollection>
		<DateCollection>
			<Date>
				<Type>BillIssued</Type>
				<Value>2021-11-10T00:00:00</Value>
			</Date>
			<Date>
				<Type>Arrival</Type>
				<IsEstimate>true</IsEstimate>
				<Value>2021-11-20T16:30:00</Value>
			</Date>
			<Date>
				<Type>Departure</Type>
				<IsEstimate>true</IsEstimate>
				<Value>2021-11-10T16:30:00</Value>
			</Date>
		</DateCollection>
		<EntryNumberCollection />
		<NoteCollection>
			<Note>
				<Description>Payment Handling Instructions</Description>
				<NoteText>Freight - Prepaid</NoteText>
				<IsCustomDescription>false</IsCustomDescription>
			</Note>
		</NoteCollection>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<Address1>13 Ocean St</Address1>
				<City>Sydney</City>
				<CompanyName>WiseTech 13 Ocean ST</CompanyName>
				<Contact>Waldo</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>Waldo@Email.com</Email>
				<Phone>+496121234567</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2100</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<Address1>33 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China</CompanyName>
				<Contact>Operations</Contact>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Email>operations.cnbjs@youragent.com</Email>
				<Fax>+861065262222</Fax>
				<Phone>+861065261111</Phone>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ShippingLineAddress</AddressType>
				<Address1>6/f Block 4</Address1>
				<Address2>Golden Dragon Industrial Centre, Tsuen Wan</Address2>
				<City>New Territories</City>
				<CompanyName>OOCL (AUST) PTY LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>CCC</Code>
							<Description>Standard Carrier Alpha Code (Sea)</Description>
						</Type>
						<Value>OOLU</Value>
					</RegistrationNumber>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>C1C</Code>
							<Description>CargoWiseOne Carrier Code</Description>
						</Type>
						<Value>C1OO</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>CoLoadWith</AddressType>
				<Address1>100 Kowloon Bay Road</Address1>
				<Address2>Kowloon Bay</Address2>
				<City>Hong kong</City>
				<CompanyName>WISETECH GLOBAL HK</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>C1C</Code>
							<Description>CargoWiseOne Carrier Code</Description>
						</Type>
						<Value>C1CR</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>Forwarder</AddressType>
				<Address1>13 Ocean St</Address1>
				<City>Sydney</City>
				<CompanyName>WiseTech 13 Ocean ST</CompanyName>
				<Contact>Waldo</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>Waldo@Email.com</Email>
				<Phone>+496121234567</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2100</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorPickupDeliveryAddress</AddressType>
				<Address1>2 GEORGE STREET</Address1>
				<City>PADDINGTON</City>
				<CompanyName>AR MAILING 2</CompanyName>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2021</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneePickupDeliveryAddress</AddressType>
				<Address1>33 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China</CompanyName>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>BookingPartyDocumentaryAddress</AddressType>
				<OrganizationCode>BKGPARTY</OrganizationCode>
				<Address1>Booking Party Address 1</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty</AddressType>
				<Address1>66 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China Bejing</CompanyName>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<PackingLineCollection>
			<PackingLine>
				<ContainerLink>1</ContainerLink>
				<ContainerNumber>TTSU1234566</ContainerNumber>
				<DetailedDescription>SPARE PARTS</DetailedDescription>
				<GoodsDescription>SPARE PARTS</GoodsDescription>
				<MarksAndNos>novcc test pls do not use</MarksAndNos>
				<PackQty>343</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
				<ReferenceNumber />
				<Volume>3.000</Volume>
				<VolumeUnit>
					<Code>M3</Code>
					<Description>Cubic Meters</Description>
				</VolumeUnit>
				<Weight>34343.000</Weight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
				<ClassificationCollection />
				<UNDGCollection />
			</PackingLine>
		</PackingLineCollection>
		<TransportLegCollection>
			<TransportLeg>
				<Carrier>
					<AddressType>Carrier</AddressType>
					<Address1>6/f Block 4</Address1>
					<Address2>Golden Dragon Industrial Centre, Tsuen Wan</Address2>
					<City>New Territories</City>
					<CompanyName>OOCL (AUST) PTY LTD</CompanyName>
					<Country>
						<Code>HK</Code>
						<Name>Hong Kong</Name>
					</Country>
					<Port>
						<Code>HKHKG</Code>
						<Name>Hong Kong</Name>
					</Port>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<CountryOfIssue>
								<Code>US</Code>
								<Name>United States</Name>
							</CountryOfIssue>
							<Type>
								<Code>CCC</Code>
								<Description>Standard Carrier Alpha Code (Sea)</Description>
							</Type>
							<Value>OOLU</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<CountryOfIssue>
								<Code>US</Code>
								<Name>United States</Name>
							</CountryOfIssue>
							<Type>
								<Code>C1C</Code>
								<Description>CargoWiseOne Carrier Code</Description>
							</Type>
							<Value>C1OO</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</Carrier>
				<PortOfDischarge>
					<Code>CNYTN</Code>
					<Name>Yantian Pt</Name>
				</PortOfDischarge>
				<PortOfLoading>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</PortOfLoading>
				<LegOrder>1</LegOrder>
				<EstimatedArrival>2021-11-20T16:30:00</EstimatedArrival>
				<EstimatedDeparture>2021-11-10T16:30:00</EstimatedDeparture>
				<LegType>Main</LegType>
				<TransportMode>Sea</TransportMode>
				<VesselLloydsIMO>0000022</VesselLloydsIMO>
				<VesselName>R WISDOM</VesselName>
				<VoyageFlightNo>7954</VoyageFlightNo>
			</TransportLeg>
		</TransportLegCollection>
	</Shipment>
</UniversalShipment>";

		const string UXMLMessageWithSubShipments = @"<UniversalShipment>
	<Shipment>
		<DataContext>
			<Action>LinkOnly</Action>
			<TriggerDate>2021-11-10T18:20:16.917</TriggerDate>
			<DocumentaryOverride>
				<DataVersion>1</DataVersion>
				<DocumentName>Shipping Instruction</DocumentName>
				<Purpose>
					<Code>ORG</Code>
					<Description>Original</Description>
				</Purpose>
			</DocumentaryOverride>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>NVO</Code>
					<Description>NVOCC</Description>
					<ServiceCode>SIN</ServiceCode>
					<ServiceDescription>Shipping Instruction</ServiceDescription>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<AgentsReference>BKG000001</AgentsReference>
		<ContainerMode>
			<Code>LCL</Code>
			<Description>Less Container Load</Description>
		</ContainerMode>
		<CoLoadBookingConfirmationReference>SH0001</CoLoadBookingConfirmationReference>
		<CoLoadMasterBillNumber>HOUSEBILL001</CoLoadMasterBillNumber>
		<GoodsDescription>Goods Desc. Master</GoodsDescription>
		<GoodsValue>0</GoodsValue>
		<GoodsValueCurrency>
			<Code>AUD</Code>
			<Description>Australian Dollar</Description>
		</GoodsValueCurrency>
		<HBLContainerPackModeOverride>DOOR/DOOR</HBLContainerPackModeOverride>
		<NoCopyBills>3</NoCopyBills>
		<NoOriginalBills>3</NoOriginalBills>
		<PaymentMethod>
			<Code>PPD</Code>
			<Description>Prepaid</Description>
		</PaymentMethod>
		<PlaceOfIssue>
			<Code>HKHKG</Code>
			<Name>Hong Kong</Name>
		</PlaceOfIssue>
		<PlaceOfDelivery>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PlaceOfDelivery>
		<PlaceOfReceipt>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PlaceOfReceipt>
		<PortOfOrigin>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfOrigin>
		<PortOfDestination>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PortOfDestination>
		<PortOfLoading>
			<Code>AUSYD</Code>
			<Name>Sydney</Name>
		</PortOfLoading>
		<PortOfDischarge>
			<Code>CNYTN</Code>
			<Name>Yantian Pt</Name>
		</PortOfDischarge>
		<ReleaseType>
			<Code>BOL</Code>
			<Description>BOL Original</Description>
		</ReleaseType>
		<ShipmentIncoTerm>
			<Code>CFR</Code>
			<Description>Cost And Freight</Description>
		</ShipmentIncoTerm>
		<ShipmentType>
			<Code>STD</Code>
			<Description>Standard</Description>
		</ShipmentType>
		<TransportMode>
			<Code>SEA</Code>
			<Description>Sea</Description>
		</TransportMode>
		<LocalProcessing>
			<DeliveryRequiredBy>2021-11-20T16:30:00</DeliveryRequiredBy>
			<PickupRequiredBy />
		</LocalProcessing>
		<AdditionalReferenceCollection>
			<AdditionalReference>
				<Type>
					<Code>BOL</Code>
					<Description>Bill Of Lading Number</Description>
				</Type>
				<ReferenceNumber>232434I454</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type>
					<Code>FFW</Code>
					<Description>Freight Forwarder Reference</Description>
				</Type>
				<ReferenceNumber>CYYY00693208</ReferenceNumber>
			</AdditionalReference>
			<AdditionalReference>
				<Type>
					<Code>HIR</Code>
					<Description>eHub Interchange Reference</Description>
				</Type>
				<ReferenceNumber>CON0000001213</ReferenceNumber>
			</AdditionalReference>
		</AdditionalReferenceCollection>
		<BillOfLadingClauseCollection>
			<BillOfLadingClause>
				<Type>
					<Code>FPP</Code>
					<Description>Freight Prepaid</Description>
				</Type>
			</BillOfLadingClause>
			<BillOfLadingClause>
				<Type>
					<Code>SLC</Code>
					<Description>Shipper Load and Count</Description>
				</Type>
			</BillOfLadingClause>
		</BillOfLadingClauseCollection>
		<SubShipmentCollection>
			<SubShipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Key>S00001400</Key>
							<Type>ForwardingShipment</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<GoodsDescription>Goods Desc. S00001400</GoodsDescription>
				<CoLoadBookingConfirmationReference>SSYD54624781</CoLoadBookingConfirmationReference>
			</SubShipment>
			<SubShipment>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Key>S00001401</Key>
							<Type>ForwardingShipment</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<GoodsDescription>Goods Desc. S00001401</GoodsDescription>
				<CoLoadBookingConfirmationReference>SSYD54624782</CoLoadBookingConfirmationReference>
			</SubShipment>
		</SubShipmentCollection>
		<ContainerCollection>
			<Container>
				<AirVentFlow>0</AirVentFlow>
				<ContainerCount>1</ContainerCount>
				<ContainerNumber>TTSU1234566</ContainerNumber>
				<ContainerType>
					<Code>20GP</Code>
					<Category>
						<Code>DRY</Code>
						<Description>Dry Storage</Description>
					</Category>
					<Description>twenty foot insulated container</Description>
					<ISOCode>22G0</ISOCode>
				</ContainerType>
				<DeliveryMode>CFS/CFS</DeliveryMode>
				<GrossWeight>36323.000</GrossWeight>
				<GrossWeightVerificationType>
					<Code>NON</Code>
					<Description>Not Verified</Description>
				</GrossWeightVerificationType>
				<HumidityPercent>0</HumidityPercent>
				<IsControlledAtmosphere>false</IsControlledAtmosphere>
				<IsEmptyContainer>false</IsEmptyContainer>
				<IsShipperOwned>false</IsShipperOwned>
				<LengthUnit>
					<Code>FT</Code>
					<Description>Feet</Description>
				</LengthUnit>
				<Link>1</Link>
				<OrganizationAddressCollection />
				<OverhangBack>0</OverhangBack>
				<OverhangFront>0</OverhangFront>
				<OverhangHeight>0</OverhangHeight>
				<OverhangLeft>0</OverhangLeft>
				<OverhangRight>0</OverhangRight>
				<Seal>3343</Seal>
				<SealPartyType>
					<Code>CAR</Code>
					<Description>Carrier</Description>
				</SealPartyType>
				<SetPointTemp>0.000</SetPointTemp>
				<TareWeight>1980.000</TareWeight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
			</Container>
		</ContainerCollection>
		<DateCollection>
			<Date>
				<Type>BillIssued</Type>
				<Value>2021-11-10T00:00:00</Value>
			</Date>
			<Date>
				<Type>Arrival</Type>
				<IsEstimate>true</IsEstimate>
				<Value>2021-11-20T16:30:00</Value>
			</Date>
			<Date>
				<Type>Departure</Type>
				<IsEstimate>true</IsEstimate>
				<Value>2021-11-10T16:30:00</Value>
			</Date>
		</DateCollection>
		<EntryNumberCollection />
		<NoteCollection>
			<Note>
				<Description>Payment Handling Instructions</Description>
				<NoteText>Freight - Prepaid</NoteText>
				<IsCustomDescription>false</IsCustomDescription>
			</Note>
		</NoteCollection>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>ConsignorDocumentaryAddress</AddressType>
				<Address1>13 Ocean St</Address1>
				<City>Sydney</City>
				<CompanyName>WiseTech 13 Ocean ST</CompanyName>
				<Contact>Waldo</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>Waldo@Email.com</Email>
				<Phone>+496121234567</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2100</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneeDocumentaryAddress</AddressType>
				<Address1>33 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China</CompanyName>
				<Contact>Operations</Contact>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Email>operations.cnbjs@youragent.com</Email>
				<Fax>+861065262222</Fax>
				<Phone>+861065261111</Phone>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ShippingLineAddress</AddressType>
				<Address1>6/f Block 4</Address1>
				<Address2>Golden Dragon Industrial Centre, Tsuen Wan</Address2>
				<City>New Territories</City>
				<CompanyName>OOCL (AUST) PTY LTD</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>CCC</Code>
							<Description>Standard Carrier Alpha Code (Sea)</Description>
						</Type>
						<Value>OOLU</Value>
					</RegistrationNumber>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>C1C</Code>
							<Description>CargoWiseOne Carrier Code</Description>
						</Type>
						<Value>C1OO</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>CoLoadWith</AddressType>
				<Address1>100 Kowloon Bay Road</Address1>
				<Address2>Kowloon Bay</Address2>
				<City>Hong kong</City>
				<CompanyName>WISETECH GLOBAL HK</CompanyName>
				<Country>
					<Code>HK</Code>
					<Name>Hong Kong</Name>
				</Country>
				<Port>
					<Code>HKHKG</Code>
					<Name>Hong Kong</Name>
				</Port>
				<RegistrationNumberCollection>
					<RegistrationNumber>
						<CountryOfIssue>
							<Code>US</Code>
							<Name>United States</Name>
						</CountryOfIssue>
						<Type>
							<Code>C1C</Code>
							<Description>CargoWiseOne Carrier Code</Description>
						</Type>
						<Value>C1CR</Value>
					</RegistrationNumber>
				</RegistrationNumberCollection>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>Forwarder</AddressType>
				<Address1>13 Ocean St</Address1>
				<City>Sydney</City>
				<CompanyName>WiseTech 13 Ocean ST</CompanyName>
				<Contact>Waldo</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>Waldo@Email.com</Email>
				<Phone>+496121234567</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2100</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsignorPickupDeliveryAddress</AddressType>
				<Address1>2 GEORGE STREET</Address1>
				<City>PADDINGTON</City>
				<CompanyName>AR MAILING 2</CompanyName>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>2021</Postcode>
				<State>NSW</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>ConsigneePickupDeliveryAddress</AddressType>
				<Address1>33 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China</CompanyName>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>BookingPartyDocumentaryAddress</AddressType>
				<OrganizationCode>BKGPARTY</OrganizationCode>
				<Address1>Booking Party Address 1</Address1>
			</OrganizationAddress>
			<OrganizationAddress>
				<AddressType>NotifyParty</AddressType>
				<Address1>66 EAST CHANG AN AVENUE</Address1>
				<City>BEIJING</City>
				<CompanyName>WiseTech Global China Bejing</CompanyName>
				<Country>
					<Code>CN</Code>
					<Name>China</Name>
				</Country>
				<Port>
					<Code>CNBJS</Code>
					<Name>Beijing</Name>
				</Port>
				<Postcode>100004</Postcode>
				<State>11</State>
				<RegistrationNumberCollection />
			</OrganizationAddress>
		</OrganizationAddressCollection>
		<PackingLineCollection>
			<PackingLine>
				<ContainerLink>1</ContainerLink>
				<ContainerNumber>TTSU1234566</ContainerNumber>
				<DetailedDescription>SPARE PARTS</DetailedDescription>
				<GoodsDescription>SPARE PARTS</GoodsDescription>
				<MarksAndNos>novcc test pls do not use</MarksAndNos>
				<PackQty>343</PackQty>
				<PackType>
					<Code>PLT</Code>
					<Description>Pallet</Description>
				</PackType>
				<ReferenceNumber />
				<Volume>3.000</Volume>
				<VolumeUnit>
					<Code>M3</Code>
					<Description>Cubic Meters</Description>
				</VolumeUnit>
				<Weight>34343.000</Weight>
				<WeightUnit>
					<Code>KG</Code>
					<Description>Kilograms</Description>
				</WeightUnit>
				<ClassificationCollection />
				<UNDGCollection />
			</PackingLine>
		</PackingLineCollection>
		<TransportLegCollection>
			<TransportLeg>
				<Carrier>
					<AddressType>Carrier</AddressType>
					<Address1>6/f Block 4</Address1>
					<Address2>Golden Dragon Industrial Centre, Tsuen Wan</Address2>
					<City>New Territories</City>
					<CompanyName>OOCL (AUST) PTY LTD</CompanyName>
					<Country>
						<Code>HK</Code>
						<Name>Hong Kong</Name>
					</Country>
					<Port>
						<Code>HKHKG</Code>
						<Name>Hong Kong</Name>
					</Port>
					<RegistrationNumberCollection>
						<RegistrationNumber>
							<CountryOfIssue>
								<Code>US</Code>
								<Name>United States</Name>
							</CountryOfIssue>
							<Type>
								<Code>CCC</Code>
								<Description>Standard Carrier Alpha Code (Sea)</Description>
							</Type>
							<Value>OOLU</Value>
						</RegistrationNumber>
						<RegistrationNumber>
							<CountryOfIssue>
								<Code>US</Code>
								<Name>United States</Name>
							</CountryOfIssue>
							<Type>
								<Code>C1C</Code>
								<Description>CargoWiseOne Carrier Code</Description>
							</Type>
							<Value>C1OO</Value>
						</RegistrationNumber>
					</RegistrationNumberCollection>
				</Carrier>
				<PortOfDischarge>
					<Code>CNYTN</Code>
					<Name>Yantian Pt</Name>
				</PortOfDischarge>
				<PortOfLoading>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</PortOfLoading>
				<LegOrder>1</LegOrder>
				<EstimatedArrival>2021-11-20T16:30:00</EstimatedArrival>
				<EstimatedDeparture>2021-11-10T16:30:00</EstimatedDeparture>
				<LegType>Main</LegType>
				<TransportMode>Sea</TransportMode>
				<VesselLloydsIMO>0000022</VesselLloydsIMO>
				<VesselName>R WISDOM</VesselName>
				<VoyageFlightNo>7954</VoyageFlightNo>
			</TransportLeg>
		</TransportLegCollection>
	</Shipment>
</UniversalShipment>";
	}
}
