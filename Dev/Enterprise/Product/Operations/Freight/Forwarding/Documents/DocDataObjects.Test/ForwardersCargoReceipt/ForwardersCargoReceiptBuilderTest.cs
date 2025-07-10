using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	abstract class ForwardersCargoReceiptBuilderTest : TestCaseWithFactory
	{
		protected abstract ForwardersCargoReceipt Build(ForwardingShipment shipment);

		protected abstract IDocDataObjectParameters Parameters { get; }

		public virtual void TestBuild()
		{
			var consol = CreateConsol();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CTN0001";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CTN0002";
			var shipment = CreateShipment(consol);
			var supplierBooking1 = Factory.NewWithValidTestData<Orders.Business.JobSupplierBooking>();
			supplierBooking1.JSB_BookingId = "SBK100";
			var supplierBooking2 = Factory.NewWithValidTestData<Orders.Business.JobSupplierBooking>();
			supplierBooking2.JSB_BookingId = "SBK200";
			container1.JC_JSB_SupplierBooking = supplierBooking1.PK;
			container2.JC_JSB_SupplierBooking = supplierBooking2.PK;
			shipment.OuterPackLines.AddNew().SetContainer(consol, container1);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container2);

			var builder = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters);
			var fcr = builder.Build();
			AssertType<ForwardersCargoReceiptDetail>(fcr);
			AssertAddressData(shipment.ConsignorDocumentaryAddress, fcr.Shipper);
			AssertAddressData(shipment.ConsigneeDocumentaryAddress, fcr.Buyer);
			AssertAddressData(shipment.ControllingCustomerAddress, fcr.ControllingCustomer);
			AssertAddressData(shipment.NotifyPartyDocumentaryAddress, fcr.NotifyParty);
			AssertAddressData(shipment.NotifyParty2DocumentaryAddress, fcr.NotifyParty2);
			AssertAddressData(shipment.NotifyParty3DocumentaryAddress, fcr.NotifyParty3);
			AssertEquals("1001", fcr.Shipper.Postcode);
			AssertEquals("1002", fcr.Buyer.Postcode);
			AssertEquals("2001", fcr.NotifyParty.Postcode);
			AssertEquals("2002", fcr.NotifyParty2.Postcode);
			AssertEquals("", fcr.FCRInstructions);
			AssertEquals("VesselData", fcr.VesselName);
			AssertEquals("VF1001", fcr.VoyageFlightNumber);
			AssertEquals("3001", fcr.Carrier.Postcode);
			AssertEquals("EXW", fcr.IncotermCodeDescription.Code);
			AssertEquals("Ex Works", fcr.IncotermCodeDescription.Description);
			AssertEquals("SBK100, SBK200", fcr.SupplierBookingNumber);
			AssertEquals("SGSIN", fcr.Origin.Code);
			AssertEquals("SG", fcr.Origin.Country.Code);
			AssertEquals("US", fcr.Destination.Country.Code);
			AssertEquals("SGSIN", fcr.PortOfLoading.Code);
			AssertEquals("AUSYD", fcr.PortOfDischarge.Code);
			AssertEquals(new ZDateTime(2022, 3, 15), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 6), fcr.OnBoardDate);
			AssertEquals("Marks & Numbers", fcr.MarksAndNumbers);
			AssertEquals(false, fcr.Delivered);
		}

		public virtual void TestDelivered()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);
			var fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(false, fcr.Delivered);

			var documentData = shipment.Factory.New<IVisualizerDocumentData>();
			documentData.Parent = shipment;
			documentData.Name = Parameters.DataStoreName;
			var log = (documentData as EnterpriseBusinessObject).Logs.AddNew(Events.DocumentDelivered);
			log.UpdateReference($"|NAM={Parameters.DocumentTitle}");
			Factory.Save();

			fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(true, fcr.Delivered);
		}

		public virtual void TestContainerRelated()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);
			var fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(new ZDateTime(2022, 3, 15), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 6), fcr.OnBoardDate);

			var container = consol.Containers.AddNew();
			container.JC_FCLWharfGateIn = new ZDateTime(2022, 3, 17);
			container.JC_FCLOnBoardVessel = new ZDateTime(2022, 3, 18);
			fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(new ZDateTime(2022, 3, 15), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 6), fcr.OnBoardDate);

			container = consol.Containers.AddNew();
			container.JC_FCLWharfGateIn = new ZDateTime(2022, 3, 19);
			container.JC_FCLOnBoardVessel = new ZDateTime(2022, 3, 20);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container);
			fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(new ZDateTime(2022, 3, 19), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 20), fcr.OnBoardDate);

			container = consol.Containers.AddNew();
			container.JC_FCLWharfGateIn = new ZDateTime(2022, 3, 21);
			container.JC_FCLOnBoardVessel = new ZDateTime(2022, 3, 22);
			shipment.OuterPackLines.AddNew().SetContainer(consol, container);
			fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(new ZDateTime(2022, 3, 21), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 22), fcr.OnBoardDate);

			foreach (ForwardingContainer c in consol.Containers)
			{
				c.JC_FCLWharfGateIn = ZDateTime.Empty;
				c.JC_FCLOnBoardVessel = ZDateTime.Empty;
			}

			fcr = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertEquals(new ZDateTime(2022, 3, 15), fcr.CargoReceiptDate);
			AssertEquals(new ZDateTime(2022, 3, 6), fcr.OnBoardDate);
		}

		public virtual void TestValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var wrapper = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();

			AssertAsciiCharactersValidation("Shipper", wrapper.Shipper);
			AssertAsciiCharactersValidation("Buyer", wrapper.Buyer);
			AssertAsciiCharactersValidation("Carrier", wrapper.Carrier);
			AssertAsciiCharactersValidation("Destination", wrapper.Destination);

			AssertMessageErrorIfEmpty(wrapper, wrapper.IncotermDescriptionInfo, (ZString)"Ex Works", "Incoterm is required.");
			AssertMessageErrorIfEmpty(wrapper, wrapper.ContainerSummaryInfo, (ZString)"XXX", "Container Numbers, Types, and Seals is required.");
			AssertMessageErrorIfEmpty(wrapper, wrapper.SupplierBookingNumberInfo, (ZString)"XXX", "Supplier Booking Number is required.");

			AssertMessageErrorIfEmpty(wrapper, wrapper.Origin.NameInfo, (ZString)"XXX", "Origin is required.");
			AssertMessageErrorIfEmpty(wrapper, wrapper.PortOfLoading.NameInfo, (ZString)"XXX", "Load Port is required.");
			AssertMessageErrorIfEmpty(wrapper, wrapper.PortOfDischarge.NameInfo, (ZString)"XXX", "Discharge Port is required.");

			AssertMessageErrorIfEmpty(wrapper, wrapper.CargoReceiptDateInfo, new ZDateTime(2022, 2, 2), "Cargo Receipt Date is required.");

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_BookingReference = "BR0001";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "NZAKL";
			wrapper = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();

			consol = CreateConsol();
			consol.Transports[0].JW_Vessel = "";
			consol.Transports[0].JW_VoyageFlight = "";
			shipment = CreateShipment(consol);
			wrapper = new ForwardersCargoReceiptDetailBuilder(shipment, Parameters).Build();
			AssertMessageErrorIfEmpty(wrapper, wrapper.VesselNameInfo, (ZString)"XXX", "Vessel is required.");
			AssertMessageErrorIfEmpty(wrapper, wrapper.VoyageFlightNumberInfo, (ZString)"XXX", "Voyage is required.");
		}

		void AssertMessageErrorIfEmpty(ForwardersCargoReceiptDetail wrapper, ZPropertyInfo info, IZType value, ZString errorMessage)
		{
			Assert(info.HasMessageError(errorMessage));

			info.Value = value;
			wrapper.ValidateAllIncludingChildren();

			Assert(!info.HasMessageError(errorMessage));
		}

		protected virtual ForwardingShipment CreateShipment(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_BookingReference = "BR0001";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2022, 3, 2);
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_INCO = "EXW";
			shipment.JS_MarksAndNumbers = "Marks & Numbers";
			shipment.JS_GoodsDescription = "Goods Description";
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "SGSIN";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "1001";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "1002";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			buyer.OH_RL_NKClosestPort = "SGSIN";
			buyer.MainAddress.Address1 = "Buyer Address1";
			buyer.MainAddress.Address2 = "Buyer Address2";
			buyer.MainAddress.City = "Somewhere";
			buyer.MainAddress.Postcode = "1003";
			buyer.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.BuyerDocAddress.E2_OA_Address = buyer.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Stay in touch";
			notifyParty.OH_RL_NKClosestPort = "AUSYD";
			notifyParty.MainAddress.Address1 = "Unit 205";
			notifyParty.MainAddress.Address2 = "128 Why Lane";
			notifyParty.MainAddress.City = "Sydney";
			notifyParty.MainAddress.Postcode = "2001";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "AU";
			notifyParty.MainAddress.OA_Email = "stayintouch@test.com";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "I'm Notifying About Stuff";
			notifyParty2.OH_RL_NKClosestPort = "AUSYD";
			notifyParty2.MainAddress.Address1 = "Unit 2";
			notifyParty2.MainAddress.Address2 = "60 What Lane";
			notifyParty2.MainAddress.City = "Sydney";
			notifyParty2.MainAddress.Postcode = "2002";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.OH_FullName = "Notify Party 3 Full Name";
			notifyParty3.OH_RL_NKClosestPort = "AUSYD";
			notifyParty3.MainAddress.Address1 = "Notify Party 3 Address1";
			notifyParty3.MainAddress.Address2 = "Notify Party 3 Address2";
			notifyParty3.MainAddress.City = "Sydney";
			notifyParty3.MainAddress.Postcode = "2003";
			notifyParty3.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var controllingCustomer = Factory.New<OrgHeader>();
			controllingCustomer.OH_FullName = "Controlling Customer Full Name";
			controllingCustomer.OH_RL_NKClosestPort = "AUSYD";
			controllingCustomer.MainAddress.Address1 = "Controlling Customer Address1";
			controllingCustomer.MainAddress.Address2 = "Controlling Customer Address2";
			controllingCustomer.MainAddress.City = "Sydney";
			controllingCustomer.MainAddress.Postcode = "2004";
			controllingCustomer.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ControllingCustomerAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			return shipment;
		}

		protected virtual ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_BookingReference = "BR100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MR Carrier";
			carrier.OH_RL_NKClosestPort = "CNSHA";
			carrier.MainAddress.Address1 = "Unit 1";
			carrier.MainAddress.Address2 = "4 What Lane";
			carrier.MainAddress.City = "Shanghay";
			carrier.MainAddress.Postcode = "3001";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var mainTransport = consol.Transports.Count > 0 ? consol.Transports[0] : consol.Transports.AddNew();
			mainTransport.JW_LegOrder = 1;
			mainTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = "SGSIN";
			mainTransport.JW_RL_NKDiscPort = "AUSYD";
			mainTransport.JW_Vessel = "VesselData";
			mainTransport.JW_VoyageFlight = "VF1001";
			mainTransport.JW_TerminalReceivalCommences = new ZDateTime(2022, 3, 15);
			mainTransport.JW_ETD = new ZDateTime(2022, 3, 6);

			var otherTransport = consol.Transports.AddNew();
			otherTransport.JW_LegOrder = 2;
			otherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			otherTransport.JW_RL_NKLoadPort = "AUSYD";
			otherTransport.JW_RL_NKDiscPort = "NZAKL";

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "NZAKL";

			return consol;
		}
	}
}
