using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CPT;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CPT
{
	sealed class CartaDePorteBuilderTest : TestCaseWithFactory
	{
		[TestDate(2021, 09, 17, 10, 0, 0)]
		public void TestHeader()
		{
			var shipment = CreateShipment();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var builder = new CartaDePorteBuilder(shipment, parameters);
			var cartaDePorte = builder.Build();

			AssertNotNull(cartaDePorte);
			AssertEquals("HouseBillNumber", "HOUSEBILL001", cartaDePorte.HouseBillNumber);
			AssertEquals("ShipmentNumber", "SH0001000", cartaDePorte.ShipmentNumber);
			AssertEquals("IsOriginal", ZBool.True, cartaDePorte.IsOriginal);
			AssertEquals("DateOfIssue", ZDateTime.Now, cartaDePorte.DateOfIssue);

			AssertEquals("TotalWeight.Value", 0m, cartaDePorte.TotalWeight.Value);
			AssertEquals("TotalWeight.Unit.Code", Constants.Weight.Kilograms, cartaDePorte.TotalWeight.Unit.Code);

			AssertEquals("PortOfOrigin.Code", "MXTIJ", cartaDePorte.PortOfOrigin.Code);
			AssertEquals("PortOfDestination.Code", "MXCJS", cartaDePorte.PortOfDestination.Code);
			AssertEquals("PortOfLoading.Code", "MXTCT", cartaDePorte.PortOfLoading.Code);
			AssertEquals("PortOfDischarge.Code", "MXELP", cartaDePorte.PortOfDischarge.Code);
		}

		public void TestHeaderCopy()
		{
			var shipment = CreateShipment();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "COPY"
			};
			var builder = new CartaDePorteBuilder(shipment, parameters);
			var cartaDePorte = builder.Build();

			AssertEquals("IsOriginal", ZBool.False, cartaDePorte.IsOriginal);
		}

		public void TestAddresses()
		{
			var shipment = CreateShipment();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "ORIGINAL"
			};
			var builder = new CartaDePorteBuilder(shipment, parameters);
			var cartaDePorte = builder.Build();

			AssertionHelper.AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, cartaDePorte.ForwardingAgent);
			AssertionHelper.AssertAddressData(shipment.ConsignorDocumentaryAddress, cartaDePorte.Consignor);
			AssertionHelper.AssertAddressData(shipment.ConsigneeDocumentaryAddress, cartaDePorte.Consignee);
			AssertionHelper.AssertAddressData(shipment.NotifyPartyDocumentaryAddress, cartaDePorte.NotifyParty);
			AssertionHelper.AssertAddressData(shipment.ExportBroker.MainAddress, cartaDePorte.ExportBroker);
			AssertionHelper.AssertAddressData(shipment.DepartureConsol.SendingForwarderAddress, cartaDePorte.SendingForwarder);
		}

		#region Implementation

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			shipment.JS_PackingMode = Constants.ContainerModes.FTL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CartaPorteSpanish;
			shipment.JS_RL_NKOrigin = "MXTIJ";
			shipment.JS_RL_NKDestination = "MXCJS";
			shipment.JS_RL_NKLoadPort = "MXTCT";
			shipment.JS_RL_NKDischargePort = "MXELP";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CY_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";

			shipment.JS_NoCopyBills = 1;
			shipment.JS_NoOriginalBills = 2;

			var deliveryOrderReceiptNote = shipment.Notes.AddNew();
			deliveryOrderReceiptNote.ST_Description = PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description;
			deliveryOrderReceiptNote.ST_NoteText = "Delivery Order Receipt Note";

			var mainTransport = shipment.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Constants.TransportModes.Road;
			mainTransport.JW_TransportType = Constants.TransportPlanningType.Other;
			mainTransport.JW_RL_NKLoadPort = "MXTCT";
			mainTransport.JW_RL_NKDiscPort = "MXELP";

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "CONSOL0001";
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "MXTCT";
			consol.JK_RL_NKDischargePort = "MXELP";
			consol.JK_NoOriginalBills = 3;
			consol.JK_NoCopyBills = 4;
			consol.JK_BookingReference = "BookingRef";
			consol.JK_CoLoadBookingReference = "CoLoadBookingRef";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
			consol.JK_MasterBillNum = "BILLNUMBER";
			consol.JK_UniqueConsignRef = "CON0001";

			var consolTransport = consol.Transports[0];
			consolTransport.JW_Vessel = "Truck";
			consolTransport.JW_VoyageFlight = "F9999";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
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
			container.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
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
			container.PackLines.Add(packline2);

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_PackageCount = 2;
			loosePackLine.JL_F3_NKPackType = Constants.PkgUnit.Pallet;
			loosePackLine.JL_ActualWeight = 200;
			loosePackLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			loosePackLine.JL_ActualVolume = 150;
			loosePackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			loosePackLine.JL_Length = 10;
			loosePackLine.JL_Width = 5;
			loosePackLine.JL_Height = 3;
			loosePackLine.JL_UnitOfDimension = "M";
			loosePackLine.JL_HarmonisedCode = "WHISKY";
			loosePackLine.JL_RefNumber = "AMR-64";
			loosePackLine.JL_ExportRefNumber = "AMRUT64%";
			loosePackLine.JL_Description = "I'm loose baby!";
			loosePackLine.Containers.RemoveAll();

			PopulateConsolAddresses(consol);

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
			consignor.OH_RL_NKClosestPort = "MXTIJ";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "MX";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "MXCJS";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "MX";

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

			var exportBroker = Factory.New<OrgHeader>();
			exportBroker.OH_FullName = "YUMMY";
			exportBroker.OH_RL_NKClosestPort = "FRPAR";
			exportBroker.MainAddress.Address1 = "Unit 200";
			exportBroker.MainAddress.Address2 = "55 Why Lane";
			exportBroker.MainAddress.City = "Paris";
			exportBroker.MainAddress.Postcode = "2000";
			exportBroker.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.JS_OH_ExportBroker = exportBroker.PK;

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
