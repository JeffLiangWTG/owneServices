using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ConsolidationAdviceBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var shipment = CreateShipment();

			var builder = new ConsolidationAdviceBuilder(shipment);
			var data = builder.Build();

			AssertNotNull(data);

			CombineAssertions(() =>
			{
				AssertEquals("Origin", "AUSYD", data.Origin.Code);
				AssertEquals("Destination", "SGSIN", data.Destination.Code);

				AssertEquals("BookingReference", "S000001000", data.BookingReference);
				AssertEquals("MasterBillNumber", "HOUSEBILL001", data.MasterBillNumber);
				AssertEquals("ContractNumber", "4350006342", data.ContractNumber);

				AssertEquals("PortOfLoading", "AUSYD", data.PortOfLoading.Code);
				AssertEquals("PortOfDischarge", "SGSIN", data.PortOfDischarge.Code);
				AssertEquals("VesselName", "SHIP1", data.VesselName);
				AssertEquals("VoyageNumber", "VOYAGE1", data.VoyageNumber);

				AssertEquals("Transports.Count", 4, data.Transports.Count);
				AssertEquals("SubShipments.Count", 2, data.SubShipments.Count);

				AssertEquals("BookingConfirmationNotes", "booking confirmation note\r\n", data.BookingConfirmationNotes);
			});

			AssertAddressData(shipment.ConsignorDocumentaryAddress, data.SendingForwarder);
			AssertAddressData(shipment.ConsigneeDocumentaryAddress, data.ReceivingForwarder);
			AssertAddressData(shipment.BookingPartyDocumentaryAddress, data.BookingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, data.CurrentUser);

			var transport = data.Transports.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Transport LegOrder", 1, transport.LegOrder);
				AssertEquals("Transport Mode", "SEA", transport.Mode.Code);
				AssertEquals("Transport Type", "MAI", transport.Type.Code);
				AssertEquals("Transport Vessel", "SHIP1", transport.Vessel.Name);
				AssertEquals("Transport Voyage", "VOYAGE1", transport.VoyageFlightNumber);
				AssertEquals("Transport PoL", "AUSYD", transport.PortOfLoading.Code);
				AssertEquals("Transport PoD", "AUMEL", transport.PortOfDischarge.Code);
			});

			var subshipment = data.SubShipments.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("ShippersRef", "ShipperRef001", subshipment.ShippersRef);
				AssertEquals("MessageRef", "SHP001", subshipment.MessageRef);
				AssertEquals("ShipmentNumber", "S000001001", subshipment.ShipmentNumber);
				AssertEquals("Origin", "AUSYD", subshipment.Origin.Code);
				AssertEquals("Destination", "SGSIN", subshipment.Destination.Code);
			});

			CombineAssertions(() =>
			{
				AssertEquals("TotalQuantity", 21, data.TotalQuantity);
				AssertEquals("TotalWeight", 70M, data.TotalWeight);
				AssertEquals("TotalVolume", 70M, data.TotalVolume);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Container count", 3, data.Containers.Count);
				AssertEquals("Have C1C1 Container", true, data.Containers.Any(x => x.Number.EqualsIgnoringCase("C1C1")));
				AssertEquals("Have C1C2 Container", true, data.Containers.Any(x => x.Number.EqualsIgnoringCase("C1C2")));
				AssertEquals("Not have C1C3 Container", false, data.Containers.Any(x => x.Number.EqualsIgnoringCase("C1C3")));
				AssertEquals("Not have C2C1 Container", false, data.Containers.Any(x => x.Number.EqualsIgnoringCase("C2C1")));
				AssertEquals("Have C2C2 Container", true, data.Containers.Any(x => x.Number.EqualsIgnoringCase("C2C2")));

				var c1c1Container = data.Containers.First(x => x.Number.EqualsIgnoringCase("C1C1"));
				AssertEquals("Code of c1c1Container", "RCCODE", c1c1Container.Type.Code);
				AssertEquals("ISOCode of c1c1Container", "RCIS", c1c1Container.Type.ISOCode);

				var c2c2Container = data.Containers.First(x => x.Number.EqualsIgnoringCase("C2C2"));
				AssertEquals("PackLine count in C2C2 Container", 2, c2c2Container.PackingLines.Count);

				AssertEquals("PackLine P1 in C2C2 Container", true, c2c2Container.PackingLines.Any(x => x.ImportReferenceNumber.EqualsIgnoringCase("P1")));
				AssertEquals("PackLine P2 in C2C2 Container", true, c2c2Container.PackingLines.Any(x => x.ImportReferenceNumber.EqualsIgnoringCase("P2")));
				AssertEquals("PackLine P3 not in C2C2 Container", false, c2c2Container.PackingLines.Any(x => x.ImportReferenceNumber.EqualsIgnoringCase("P3")));
			});
		}

		public void TestShipmentTypeAlwaysIsCLD()
		{
			var shipment = CreateShipment();

			shipment.JS_TransportMode = Core.Constants.ShipmentTypes.StandardHouse;

			var builder = new ConsolidationAdviceBuilder(shipment);

			AssertEquals(Core.Constants.ShipmentTypes.CoLoadMaster, builder.Build().ShipmentType.Code);

			shipment.JS_TransportMode = Core.Constants.ShipmentTypes.CoLoadMaster;

			AssertEquals(Core.Constants.ShipmentTypes.CoLoadMaster, builder.Build().ShipmentType.Code);
		}

		#region Implementation

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.JS_UniqueConsignRef = "S000001000";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "SGSIN";

			shipment.JS_E_DEP = ZDateTime.UtcToday.AddDays(2);
			shipment.JS_E_ARV = ZDateTime.UtcToday.AddDays(5);

			var note = shipment.Notes.AddNew();
			note.ST_Description = "Booking Confirmation Notes";
			note.ST_NoteDataAsText = "booking confirmation note";

			var bookingReference = Factory.New<CusEntryNumber>();
			bookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			bookingReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			bookingReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			bookingReference.CE_EntryNum = "10207000067891";
			shipment.Numbers.Add(bookingReference);

			var carrierContractNumber = Factory.New<CusEntryNumber>();
			carrierContractNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			carrierContractNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			carrierContractNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			carrierContractNumber.CE_EntryNum = "4350006342";
			shipment.Numbers.Add(carrierContractNumber);

			PopulateShipmentAddresses(shipment);

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_ETD = ZDateTime.UtcToday.AddDays(2);
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETA = ZDateTime.UtcToday.AddDays(2);
			transport1.JW_Vessel = "SHIP1";
			transport1.JW_VoyageFlight = "VOYAGE1";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_LegOrder = 1;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_ETD = ZDateTime.UtcToday.AddDays(2);
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETA = ZDateTime.UtcToday.AddDays(4);
			transport2.JW_Vessel = "SHIP2";
			transport2.JW_VoyageFlight = "VOYAGE2";

			PopulateCoLoadShipments(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_FullName = "Booking Party";
			bookingParty.OH_RL_NKClosestPort = "SGSIN";
			bookingParty.MainAddress.Address1 = "Unit 1";
			bookingParty.MainAddress.Address2 = "4 What Lane";
			bookingParty.MainAddress.City = "Auckland";
			bookingParty.MainAddress.Postcode = "5022";
			bookingParty.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var deliverTo = Factory.New<OrgHeader>();
			deliverTo.OH_FullName = "DeliverToCo";
			deliverTo.OH_RL_NKClosestPort = "SGSIN";
			deliverTo.MainAddress.Address1 = "Unit 1";
			deliverTo.MainAddress.Address2 = "4 What Lane";
			deliverTo.MainAddress.City = "Auckland";
			deliverTo.MainAddress.Postcode = "5022";
			deliverTo.MainAddress.OA_RN_NKCountryCode = "SG";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliverTo.MainAddress.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "MAERSK";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "Unit 13";
			consignor.MainAddress.Address2 = "4 Lost Lane";
			consignor.MainAddress.City = "Sydney";
			consignor.MainAddress.Postcode = "2000";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var pickupFrom = Factory.New<OrgHeader>();
			pickupFrom.OH_FullName = "PickupFromCo";
			pickupFrom.OH_RL_NKClosestPort = "AUSYD";
			pickupFrom.MainAddress.Address1 = "Unit 13";
			pickupFrom.MainAddress.Address2 = "4 Lost Lane";
			pickupFrom.MainAddress.City = "Sydney";
			pickupFrom.MainAddress.Postcode = "2000";
			pickupFrom.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupFrom.MainAddress.PK;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MR Carrier";
			carrier.OH_RL_NKClosestPort = "CNSHA";
			carrier.MainAddress.Address1 = "Unit 1";
			carrier.MainAddress.Address2 = "4 What Lane";
			carrier.MainAddress.City = "Shanghay";
			carrier.MainAddress.Postcode = "5022";
			carrier.MainAddress.OA_RN_NKCountryCode = "CN";
			shipment.JS_OA_BookedShippingLineAddress = carrier.MainAddress.PK;
		}

		void PopulateCoLoadShipments(ForwardingShipment shipment)
		{
			var subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.JS_UniqueConsignRef = "S000001001";
			subshipment1.JS_BookingReference = "ShipperRef001";
			subshipment1.JS_RL_NKOrigin = "AUSYD";
			subshipment1.JS_RL_NKDestination = "SGSIN";

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.OH_FullName = "Booking Party";
			bookingParty.OH_RL_NKClosestPort = "SGSIN";
			bookingParty.MainAddress.Address1 = "Unit 1";
			bookingParty.MainAddress.Address2 = "4 What Lane";
			bookingParty.MainAddress.City = "Auckland";
			bookingParty.MainAddress.Postcode = "5022";
			bookingParty.MainAddress.OA_RN_NKCountryCode = "SG";
			subshipment1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var messageReference = Factory.New<CusEntryNumber>();
			messageReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			messageReference.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			messageReference.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			messageReference.CE_EntryNum = "SHP001";
			subshipment1.Numbers.Add(messageReference);

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C00000069";

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "RCCODE";
			refContainer.RC_ISOType = "RCIS";

			var consol1Container1 = consol1.Containers.AddNew();
			consol1Container1.JC_ContainerNum = "C1C1";
			consol1Container1.JC_ContainerCount = 3;
			consol1Container1.JC_RC = refContainer.PK;

			var consol1Container2 = consol1.Containers.AddNew();
			consol1Container2.JC_ContainerNum = "C1C2";
			consol1Container2.JC_ContainerCount = 3;

			var consol1Container3 = consol1.Containers.AddNew();
			consol1Container3.JC_ContainerNum = "C1C3";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C00000070";

			var consol2Container1 = consol2.Containers.AddNew();
			consol2Container1.JC_ContainerNum = "C2C1";
			consol2Container1.JC_ContainerCount = 6;

			var consol2Container2 = consol2.Containers.AddNew();
			consol2Container2.JC_ContainerNum = "C2C2";
			consol2Container2.JC_ContainerCount = 6;

			var packingLine1 = subshipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 5;
			packingLine1.JL_ImportRefNumber = "P1";

			packingLine1.JL_ActualWeight = 10;
			packingLine1.JL_ActualWeightUQ = "KG";

			packingLine1.JL_ActualVolume = 10;
			packingLine1.JL_ActualVolumeUQ = "M3";

			packingLine1.SetContainer(consol1, consol1Container1);
			packingLine1.SetContainer(consol2, consol2Container2);

			var packingLine2 = subshipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 3;

			packingLine2.JL_ImportRefNumber = "P2";

			packingLine2.JL_ActualWeight = 20;
			packingLine2.JL_ActualWeightUQ = "KG";

			packingLine2.JL_ActualVolume = 20;
			packingLine2.JL_ActualVolumeUQ = "M3";

			packingLine2.SetContainer(consol1, consol1Container2);
			packingLine2.SetContainer(consol2, consol2Container2);

			var packingLine3 = subshipment1.OuterPackLines.AddNew();

			packingLine3.JL_PackageCount = 3;

			packingLine3.JL_ImportRefNumber = "P3";

			packingLine3.JL_ActualWeight = 30;
			packingLine3.JL_ActualWeightUQ = "KG";

			packingLine3.JL_ActualVolume = 30;
			packingLine3.JL_ActualVolumeUQ = "M3";

			packingLine3.SetContainer(consol1, null);
			packingLine3.SetContainer(consol2, null);

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_UniqueConsignRef = "S000001002";
			subshipment2.JS_BookingReference = "ShipperRef002";
			subshipment2.JS_RL_NKOrigin = "AUSYD";
			subshipment2.JS_RL_NKDestination = "SGSIN";

			var s2PackingLine1 = subshipment2.OuterPackLines.AddNew();
			s2PackingLine1.JL_PackageCount = 5;
			s2PackingLine1.JL_ImportRefNumber = "P1";

			s2PackingLine1.JL_ActualWeight = 10;
			s2PackingLine1.JL_ActualWeightUQ = "KG";

			s2PackingLine1.JL_ActualVolume = 10;
			s2PackingLine1.JL_ActualVolumeUQ = "M3";

			s2PackingLine1.SetContainer(consol1, consol1Container1);
			s2PackingLine1.SetContainer(consol2, null);

			var shipment1OnlyInConsol1 = Factory.New<ForwardingShipment>();
			shipment1OnlyInConsol1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1OnlyInConsol1.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment1OnlyInConsol1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1OnlyInConsol1.JS_UniqueConsignRef = "UC0000112";
			shipment1OnlyInConsol1.JS_BookingReference = "BR0000112";
			shipment1OnlyInConsol1.Consols.Add(consol1);

			var s1PackingLine1 = shipment1OnlyInConsol1.OuterPackLines.AddNew();
			s1PackingLine1.JL_PackageCount = 5;
			s1PackingLine1.JL_ImportRefNumber = "P1";

			s1PackingLine1.JL_ActualWeight = 10;
			s1PackingLine1.JL_ActualWeightUQ = "KG";

			s1PackingLine1.JL_ActualVolume = 10;
			s1PackingLine1.JL_ActualVolumeUQ = "M3";

			s1PackingLine1.SetContainer(consol1, consol1Container1);
		}

		#endregion
	}
}
