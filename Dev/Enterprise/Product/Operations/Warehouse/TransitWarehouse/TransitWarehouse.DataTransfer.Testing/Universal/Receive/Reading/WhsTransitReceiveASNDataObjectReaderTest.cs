using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveASNDataObjectReaderTest : TransitUniversalTestCase
	{
		#region TestDataContextType

		public void TestDataContextType()
		{
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, Data.Orgs.INTHEMSYD, Data.Warehouse, Logger, Factory);
			AssertEquals(DataContextType.TransitReceiveASN, reader.DataContextType);
		}

		#endregion

		#region TestPopulateBizO_CreateNewASN

		public void TestPopulateBizO_CreateNewASN()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var containerDO = Data.CreateContainer("CNT-1", 0);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN reference should be generated from number fountain.", "TRT00000001", asn.WRP_ReferenceNumber);
			AssertEquals("ASN should set its reference to the container number.", "CNT-1", asn.WRP_VehicleReference);
			AssertEquals("Warehouse must be set to the one that was passed into the reader.", Data.Warehouse.PK, asn.WRP_WW_IntendedWarehouse);
			AssertEquals("Booking party should be set.", bookingPartyOrg.MainAddress, asn.BookingPartyDocAddress.Address);
		}

		public void TestPopulateBizO_CreateNewASN_Reference()
		{
			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.WayBillNumber = "HB1";
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";
			consolDO.VoyageFlightNo = "VFN";
			var containerDO = Data.CreateContainer("CNT-1", 0);

			Logger.TopLevelDataObject = consolDO;
			var reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, null, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should use container number if provided.", "CNT-1", asn.WRP_VehicleReference);

			containerDO.ContainerNumber = "";
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to MasterBill.", "MB1", asn.WRP_VehicleReference);

			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to MasterBill.", "MB1", asn.WRP_VehicleReference);

			consolDO.WayBillNumber = null;
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to Consol Number.", "C1000000", asn.WRP_VehicleReference);

			Data.SetupNewDataContextWithDataSource(consolDO, runSheet: "RS1");
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to Voyage Flight Number.", "VFN", asn.WRP_VehicleReference);

			Logger.TopLevelDataObject = shipmentDO;
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to House Bill.", "HB1", asn.WRP_VehicleReference);

			shipmentDO.WayBillNumber = null;
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should fallback to Shipment Number.", "S1000000", asn.WRP_VehicleReference);

			shipmentDO.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment).Key = "";
			reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			asn = reader.ReadIntoBusinessObject();
			AssertEquals("ASN should set an empty vehicle reference.", string.Empty, asn.WRP_VehicleReference);
		}

		#region TestExpectedArrival

		// Transport leg collection tested in ForwardingToTWIntegrationTest.cs
		[TestDate(1999, 9, 23)]
		public void TestExpectedArrival_DifferentPort()
		{
			Data.Orgs.WUFSHIJNB.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			var transportLegPort = "NZAKL";

			var consolDO = Data.HeaderDataObject;
			SetInboundAndOutboundTransportLegs(consolDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), transportLegPort);
			var shipmentDO = consolDO.SubShipmentCollection[0];
			SetInboundAndOutboundTransportLegs(shipmentDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), transportLegPort);

			Logger.TopLevelDataObject = consolDO;
			var asn = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ASN.WRP_ETA should be empty if no transport leg has a matching port", ZDateTime.Empty, asn.WRP_ETA);
		}

		[TestDate(1999, 9, 23)]
		public void TestExpectedArrival_NoPort()
		{
			var relatedPort = "NZAKL";
			Data.Orgs.WUFSHIJNB.MainAddress.OA_RL_NKRelatedPortCode = relatedPort;

			var consolDO = Data.HeaderDataObject;
			SetInboundAndOutboundTransportLegs(consolDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), null);
			var shipmentDO = consolDO.SubShipmentCollection[0];
			SetInboundAndOutboundTransportLegs(shipmentDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), null);

			Logger.TopLevelDataObject = consolDO;
			var asn = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ASN.WRP_ETA should be empty if transport legs do not have ports", ZDateTime.Empty, asn.WRP_ETA);
		}

		[TestDate(1999, 9, 23)]
		public void TestExpectedArrival_FromShipmentTransportLeg_IgnoresShipmentETD()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var consolDO = Data.HeaderDataObject;
			var shipmentDO = consolDO.SubShipmentCollection[0];

			var eta = new ZDateTime(2002, 3, 3);
			SetInboundAndOutboundTransportLegs(consolDO, new ZDateTime(2000, 1, 1), eta, homePort);
			var shipmentETDDate = new Date { Type = DateType.Departure, Value = new ZDateTime(2002, 2, 2) };
			shipmentDO.SetDateCollection(() => new List<Date> { shipmentETDDate });

			Logger.TopLevelDataObject = consolDO;
			var asn = new WhsTransitReceiveASNDataObjectReader(consolDO, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ASN.WRP_ETA should be read from the shipment's transport legs", eta, asn.WRP_ETA);
		}

		[TestDate(1999, 9, 23)]
		public void TestExpectedArrival_FromConsolTransportLeg_IgnoresShipmentETD()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();

			var consolDO = Data.HeaderDataObject;
			SetInboundAndOutboundTransportLegs(consolDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), homePort);
			var shipmentDO = consolDO.SubShipmentCollection[0];
			SetInboundAndOutboundTransportLegs(shipmentDO, new ZDateTime(2000, 1, 1), new ZDateTime(2002, 3, 3), homePort);
			var shipmentETDDate = new Date { Type = DateType.Departure, Value = new ZDateTime(2002, 2, 2) };
			shipmentDO.SetDateCollection(() => new List<Date> { shipmentETDDate });

			Logger.TopLevelDataObject = consolDO;
			var asn = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ASN.WRP_ETA should be read from the consol's transport legs", new ZDateTime(2002, 3, 3), asn.WRP_ETA);
		}

		void SetInboundAndOutboundTransportLegs(Shipment shipmentDO, ZDateTime outboundEstimatedArrival, ZDateTime inboundEstimatedArrival, string port)
		{
			var outboundShipmentRoutingLeg = new TransportLeg
			{
				EstimatedArrival = outboundEstimatedArrival,
				PortOfLoading = port != null ? new UNLOCO { Code = port } : null
			};
			var inboundShipmentRoutingLeg = new TransportLeg
			{
				EstimatedArrival = inboundEstimatedArrival,
				PortOfDischarge = port != null ? new UNLOCO { Code = port } : null
			};
			shipmentDO.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { outboundShipmentRoutingLeg, inboundShipmentRoutingLeg });
		}

		[TestDate(2020, 9, 23)]
		public void TestExpectedArrival_ShouldTakeInboundLegEta()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var expectedDate = new ZDateTime(2002, 2, 2);
			var inboundLeg = Helper.CreateTransportLeg(portOfDischarge: homePort, estimatedArrivalDate: expectedDate);

			var consol = Data.HeaderDataObject;
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundLeg });
			Logger.TopLevelDataObject = consol;

			var asn = new WhsTransitReceiveASNDataObjectReader(consol, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(expectedDate, asn.WRP_ETA);
		}

		public void TestExpectedArrival_ShouldTakeEarliestEstimatedPickupAsFallback()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var inboundLeg = Helper.CreateTransportLeg(portOfDischarge: homePort);
			var estimatedPickupDate = new ZDateTime(2002, 2, 2);

			var consol = Data.HeaderDataObject;
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundLeg });

			var shipmentWithIgnoredDates = Data.ShipmentDataObject;
			Helper.AddTestDataToShipment(
				shipmentWithIgnoredDates, inboundLeg,
				portOfOrigin: homePort,
				estimatedPickup: estimatedPickupDate.AddHours(5), // This date should be ignored because it's not the earliest
				pickupRequiredFrom: estimatedPickupDate.AddHours(-3) // This date should be ignored even if it is earlier overall
			);

			var shipmentWithEarliestEstimatedPickup = Data.CreateShipmentWithPackages("HSB2", "Pack2");
			Helper.AddTestDataToShipment(shipmentWithEarliestEstimatedPickup, inboundLeg, portOfOrigin: homePort, estimatedPickup: estimatedPickupDate);

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { shipmentWithIgnoredDates, shipmentWithEarliestEstimatedPickup });
			Logger.TopLevelDataObject = consol;

			var asn = new WhsTransitReceiveASNDataObjectReader(consol, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ETA should take the earliest Shipment Estimated Pickup", estimatedPickupDate, asn.WRP_ETA);
		}

		public void TestExpectedArrival_ShouldTakeEarliestPickupRequiredFromAsFallback()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var pickupRequiredFrom = new ZDateTime(2002, 2, 2);
			var inboundLeg = Helper.CreateTransportLeg(portOfDischarge: homePort);

			var consol = Data.HeaderDataObject;
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundLeg });

			var shipmentWithIgnoredDates = Data.ShipmentDataObject;
			Helper.AddTestDataToShipment(shipmentWithIgnoredDates, inboundLeg, portOfOrigin: homePort, pickupRequiredFrom: pickupRequiredFrom.AddHours(5));

			var shipmentWithExpectedPickupRequiredFromDate = Data.CreateShipmentWithPackages("HSB2", "Pack2");
			Helper.AddTestDataToShipment(shipmentWithExpectedPickupRequiredFromDate, inboundLeg, portOfOrigin: homePort, pickupRequiredFrom: pickupRequiredFrom);

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { shipmentWithIgnoredDates, shipmentWithExpectedPickupRequiredFromDate });
			Logger.TopLevelDataObject = consol;

			var asn = new WhsTransitReceiveASNDataObjectReader(consol, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ETA should take the earliest Shipment Pickup Required By", pickupRequiredFrom, asn.WRP_ETA);
		}

		public void TestExpectedArrival_WithConsol_ShouldTakeCFSReceivalAsFallback()
		{
			var homePort = Data.Warehouse.RelatedCompanyBranch.HomePort.GetUNLOCO();
			var cfsReceival = new ZDateTime(2004, 4, 4);

			var inboundLeg = Helper.CreateTransportLeg(portOfDischarge: homePort);
			var outboundLeg = Helper.CreateTransportLeg(portOfLoading: homePort, cfsReceivalDate: cfsReceival);

			var consol = Data.HeaderDataObject;
			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { inboundLeg, outboundLeg });

			var nonOriginShipment = Data.ShipmentDataObject;
			Helper.AddTestDataToShipment(nonOriginShipment, inboundLeg, outboundLeg, portOfOrigin: null, estimatedPickup: new ZDateTime(2002, 2, 2));

			var shipmentWithNoDate = Data.CreateShipmentWithPackages("HSB2", "Pack2");
			Helper.AddTestDataToShipment(nonOriginShipment, inboundLeg, outboundLeg, portOfOrigin: homePort, estimatedPickup: null);
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { nonOriginShipment, shipmentWithNoDate });
			Logger.TopLevelDataObject = consol;

			var asn = new WhsTransitReceiveASNDataObjectReader(consol, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("ASN ETA should fallback to Outbound Leg CFS Receival if no better options", cfsReceival, asn.WRP_ETA);
		}

		#endregion

		#region Booking Party

		public void TestPopulateBizO_DoNotPopulateBookingParty_ViaSeaCargoOutturn()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "premise123", "lloyds123", "voyage123", "vessel1");
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "premise123");

			Logger.TopLevelDataObject = consolDO;
			var asn = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			var bookingPartyDocAddress = asn.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			AssertNull("ASN created from Sea Cargo Outturn should not have booking party.", bookingPartyDocAddress);
		}

		public void TestPopulateBizO_PopulateBookingParty_ViaShipment()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var containerDO = Data.CreateContainer("CNT-1", 0);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();

			var bookingPartyDocAddress = asn.DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			AssertNotNull("ASN created from shipment should have booking party.", bookingPartyDocAddress);
			AssertEquals(bookingPartyOrg.MainAddress.PK, bookingPartyDocAddress.E2_OA_Address);
		}

		#endregion

		#region Transport Company

		public void TestPopulateBizO_PopulatePickupTransportCompany_ViaShipment()
		{
			var org = Data.Orgs.Warehouse_WUFSHIJNB;
			org.AddressType = AddressTypes.PickupLocalCartage;
			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org, Data.Orgs.Warehouse_WUFSHIJNB });

			Logger.TopLevelDataObject = shipmentDO;
			var reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			// In production we currently do not create ASNs for shipments.
			var asn = reader.ReadIntoBusinessObject();

			var transportDocAddress = asn.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull("Transport company in ASN should be same as Shipment's PickupLocalCartage as the Consol has no DepartureCFSLocalTransportAddress", transportDocAddress);
			AssertEquals("Transport company in ASN should be same as Shipment's PickupLocalCartage as the Consol has no DepartureCFSLocalTransportAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, transportDocAddress.E2_OA_Address);
		}

		public void TestPopulateBizO_PopulateDeliveryTransportCompany_ViaShipment()
		{
			var org = Data.Orgs.Warehouse_WUFSHIJNB;
			org.AddressType = AddressTypes.DeliveryLocalCartage;
			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });

			shipmentDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org, Data.Orgs.Warehouse_WUFSHIJNB });

			Logger.TopLevelDataObject = shipmentDO;
			var reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();

			var transportDocAddress = asn.TransportCompany;
			AssertNotNull("Transport company in ASN should be same as Shipment's DeliveryLocalCartage as the Consol has no ArrivalCFSLocalTransportAddress", transportDocAddress);
			AssertEquals("Transport company in ASN should be same as Shipment's DeliveryLocalCartage as the Consol has no ArrivalCFSLocalTransportAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, transportDocAddress.E2_OA_Address);
		}

		public void TestPopulateBizO_PopulateTransportCompany_ViaConsol()
		{
			var orgOnShipment = Data.Orgs.Warehouse_CRAHOLSYD;
			orgOnShipment.AddressType = AddressTypes.PickupLocalCartage;
			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgOnShipment, Data.Orgs.Warehouse_CRAHOLSYD });

			var orgOnConsol = Data.Orgs.Warehouse_WUFSHIJNB;
			orgOnConsol.AddressType = nameof(DocAddressType.DepartureCFSLocalTransportAddress);
			var consolDO = Data.HeaderDataObject;
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { orgOnConsol });
			Logger.TopLevelDataObject = consolDO;
			var reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();

			var transportDocAddress = asn.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNotNull("Transport company in ASN should be same as Consol's DepartureCFSLocalTransportAddress", transportDocAddress);
			AssertEquals("Transport company in ASN should be same as Consol's DepartureCFSLocalTransportAddress", Data.Orgs.WUFSHIJNB.MainAddress.PK, transportDocAddress.E2_OA_Address);
		}

		public void TestImportASN_ASNTransport_ViaConsol_WithoutDepartureTransportCompany()
		{
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, null, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();

			var transportDocAddress = asn.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertNull("ASN should not have Transport company as there is no PickupLocalCartage in Shipment", transportDocAddress);
		}

		#endregion

		#region Additional References

		public void TestPopulateBizO_PopulateAdditionalReferences_Arrival() => TestPopulateBizO_PopulateAdditionalReferences(true);
		public void TestPopulateBizO_PopulateAdditionalReferences_Departure() => TestPopulateBizO_PopulateAdditionalReferences(false);

		void TestPopulateBizO_PopulateAdditionalReferences(bool isArrival)
		{
			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000", runSheet: "R1", isArrival: isArrival);
			consolDO.WayBillNumber = "MB1";
			consolDO.BookingConfirmationReference = "Booking1";
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var transportLeg = consolDO.TransportLegCollection.First();
			AssertEquals("Precondition.", "VES1", transportLeg.VesselName);
			AssertEquals("Precondition.", "VF1", transportLeg.VoyageFlightNo);

			Logger.TopLevelDataObject = consolDO;
			var receiveASN = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			var additionalReferencesForASN = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveASN.PK));
			if (isArrival)
			{
				Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MB1", WarehouseAdditionalReferenceTypes.Descriptions.MasterBill);
			}
			else
			{
				AssertEquals(additionalReferencesForASN.Count(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.MasterBill), 0);
			}
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingConsolNumber);
			// Vessel and Voyage Flight Number are pulled from outbound transport leg
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.Vessel, "VES1", WarehouseAdditionalReferenceTypes.Descriptions.Vessel);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VF1", WarehouseAdditionalReferenceTypes.Descriptions.VoyageFlightNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "R1", WarehouseAdditionalReferenceTypes.Descriptions.RunSheetNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "Booking1", WarehouseAdditionalReferenceTypes.Descriptions.CarrierBookingReference);
		}

		public void TestPopulateBizO_PopulateAdditionalReferences_SeaCargoOutturn()
		{
			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "premise123", "lloyds123", "voyage123", "vessel1");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "premise123");

			Logger.TopLevelDataObject = consolDO;
			var receiveASN = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			var additionalReferencesForASN = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, receiveASN.PK));

			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, "lloyds123", WarehouseAdditionalReferenceTypes.Descriptions.VesselLloyds);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "voyage123", WarehouseAdditionalReferenceTypes.Descriptions.VoyageFlightNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.Vessel, "vessel1", WarehouseAdditionalReferenceTypes.Descriptions.Vessel);
		}

		void PopulateShipmentObjectWithSeaCargoOutturnData(Shipment shipment, string premiseId, string lloydsNumber, string voyageFlightNumber, string vesselName)
		{
			shipment.LloydsIMO = lloydsNumber;
			shipment.VoyageFlightNo = voyageFlightNumber;
			shipment.VesselName = vesselName;
			shipment.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = premiseId
							},
						});
		}

		#endregion

		#region Transport Mode

		public void TestPopulateBizO_PopulatesTransportMode()
		{
			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000", runSheet: "R1");
			consolDO.SubShipmentCollection.Add(shipmentDO);
			consolDO.WayBillNumber = "MB1";
			consolDO.TransportMode = new CodeDescriptionPair { Code = TransportModes.Sea };

			var transportLeg = consolDO.TransportLegCollection.First();
			AssertEquals("Precondition.", "VES1", transportLeg.VesselName);
			AssertEquals("Precondition.", "VF1", transportLeg.VoyageFlightNo);

			Factory.SaveForTesting();

			Logger.TopLevelDataObject = consolDO;
			var newFactory = new UniversalObjectFactory();
			var receiveASN = new WhsTransitReceiveASNDataObjectReader(consolDO, null, null, null, Data.Warehouse, Logger, newFactory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			AssertEquals("Populated Transport Mode.", TransportModes.Sea, receiveASN.WRP_TransportMode);
		}

		#endregion

		#endregion

		#region Matching

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_FromShipment_MatchesOnHouseBill()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			matchingASN.WRP_VehicleReference = "HB1";

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.WayBillNumber = "HB1";
			AssertMatchedASN(shipmentDO, null, null, bookingPartyOrg, warehouse, matchingASN.PK, "Should match ASN with housebill in reference");
		}

		// Requirement to reject complete ASNs removed in WI00440350
		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_MatchingASNIsCompleted_ShouldNotReject()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			matchingASN.WRP_VehicleReference = "HB1";
			matchingASN.WRP_CompleteTime = DateTimeOffset.UtcNow;

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.WayBillNumber = "HB1";

			var reader = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, bookingPartyOrg, warehouse, Logger, Factory);
			AssertNoExceptionThrown("Matching ASN is completed but should not throw.", () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_FromConsol_DoesNotMatchOnHouseBill()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			matchingASN.WRP_VehicleReference = "HB1";

			Factory.SaveForTesting();

			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = null;
			consolDO.VoyageFlightNo = null;
			Data.SetupNewDataContextWithDataSource(consolDO, runSheet: "RS1");
			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.WayBillNumber = "HB1";
			AssertNotMatchedASN(shipmentDO, consolDO, null, bookingPartyOrg, warehouse, matchingASN.PK, "Should not match ASN on house bill if a consol is provided");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingASN_MandatoryFields()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var otherBookingPartyOrg = Data.Orgs.CRAHOLSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			matchingASN.WRP_VehicleReference = "CNT-1";

			Factory.SaveForTesting();

			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Precondition.", 1, asns.Length);

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, matchingASN.PK, "ASN with matching Warehouse, Booking Party, and Container Number should be used.");
			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", otherBookingPartyOrg, warehouse, matchingASN.PK, "ASN with different booking party should be matched.");
			AssertNotMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, Data.WarehouseCRAHOLSYD, matchingASN.PK, "ASN with different warehouse should not be matched.");
			AssertNotMatchedASN(shipmentDO, consolDO, "CNT-2", bookingPartyOrg, warehouse, matchingASN.PK, "ASN with different container number should not be matched.");

			asns = new UniversalObjectFactory().Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("There should be 2 new ASNs that haven't matched each other and been new created.", 3, asns.Length);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AdditionalReference_DBHits()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingASN, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			matchingASN.WRP_VehicleReference = "MB1";
			var matchingASN2 = Helper.CreateReceiveASN("2", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN2, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingASN2, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			matchingASN2.WRP_VehicleReference = "MB1";

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";

			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = consolDO;
			new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, bookingPartyOrg, warehouse, Logger, newFactory).ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			AssertDbHits(new Dictionary<string, int>() { { CusEntryNumSchema.Constants.TableName, 1 } }, newFactory.BOFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingASN_IgnoresASNsOlderThan30Days()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var oldASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(oldASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(oldASN, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			oldASN.WRP_VehicleReference = "C1";

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";

			AssertMatchedASN(shipmentDO, consolDO, "C1", bookingPartyOrg, warehouse, oldASN.PK, "Precondition: ASN should match");

			oldASN.WRP_SystemCreateTimeUtc = new ZDateTime(ZDateTime.UtcNow.AddDays(-32));
			Factory.SaveForTesting();

			AssertNotMatchedASN(shipmentDO, consolDO, "C1", bookingPartyOrg, warehouse, oldASN.PK, "ASNs older than 30 days should not be matched.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingASN_VehicleReferenceFallback()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var asnWithContainerNumber = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithContainerNumber, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithContainerNumber.WRP_VehicleReference = "CNT-1";
			asnWithContainerNumber.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			// Setup ASNs with a weaker but more recent match
			var asnWithMasterBill = Helper.CreateReceiveASN("2", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithMasterBill, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithMasterBill.WRP_VehicleReference = "MB1";
			asnWithMasterBill.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11);

			var asnWithConsolNumber = Helper.CreateReceiveASN("3", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithConsolNumber.WRP_VehicleReference = "C1000000";
			asnWithConsolNumber.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 12);

			// Setup ASNs that should not match
			var asnWithVoyageFlightNumber = Helper.CreateReceiveASN("4", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithVoyageFlightNumber, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithVoyageFlightNumber.WRP_VehicleReference = "VFN";
			asnWithVoyageFlightNumber.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 13);

			var asnWithHouseBill = Helper.CreateReceiveASN("5", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithHouseBill, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithHouseBill.WRP_VehicleReference = "HB1";
			asnWithHouseBill.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 14);

			var asnWithEmptyReference = Helper.CreateReceiveASN("6", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithEmptyReference, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithEmptyReference.WRP_VehicleReference = string.Empty;
			asnWithEmptyReference.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 14);

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			shipmentDO.WayBillNumber = "HB1";
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";
			consolDO.VoyageFlightNo = "VFN";

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, asnWithContainerNumber.PK, "ASN should match by container number.");
			AssertMatchedASN(shipmentDO, consolDO, null, bookingPartyOrg, warehouse, asnWithMasterBill.PK, "ASN should match by MasterBill.");
			consolDO.WayBillNumber = null;
			AssertMatchedASN(shipmentDO, consolDO, null, bookingPartyOrg, warehouse, asnWithConsolNumber.PK, "ASN should match by Consol Number.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingASN_UsesMostRecent()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var oldASNWithin30Days = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(oldASNWithin30Days, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			oldASNWithin30Days.WRP_VehicleReference = "CNT-1";
			oldASNWithin30Days.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			var recentASN = Helper.CreateReceiveASN("2", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(recentASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			recentASN.WRP_VehicleReference = "CNT-1";
			recentASN.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11);

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, recentASN.PK, "The most recent ASN is matched.");
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_LinkMatchingASN_Fallback()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var asnWithAllReferences = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithAllReferences, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(asnWithAllReferences, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithAllReferences.WRP_VehicleReference = "CNT-1";
			asnWithAllReferences.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			// Setup ASNs with a weaker but more recent match
			var asnWithoutMasterBill = Helper.CreateReceiveASN("2", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithoutMasterBill, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithoutMasterBill.WRP_VehicleReference = "CNT-1";
			asnWithoutMasterBill.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 11);

			var asnWithoutConsolNumber = Helper.CreateReceiveASN("3", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithoutConsolNumber, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			asnWithoutConsolNumber.WRP_VehicleReference = "CNT-1";
			asnWithoutConsolNumber.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 12);

			// Setup ASNs that should not match
			var asnWithDifferentMasterBill = Helper.CreateReceiveASN("4", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithDifferentMasterBill, "MB2", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(asnWithDifferentMasterBill, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithDifferentMasterBill.WRP_VehicleReference = "CNT-1";
			asnWithDifferentMasterBill.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 13);

			var asnWithDifferentConsolNumber = Helper.CreateReceiveASN("5", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(asnWithDifferentConsolNumber, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(asnWithDifferentConsolNumber, "C2000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			asnWithDifferentConsolNumber.WRP_VehicleReference = "CNT-1";
			asnWithDifferentConsolNumber.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 14);

			var asnWithNoReferences = Helper.CreateReceiveASN("6", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			asnWithNoReferences.WRP_VehicleReference = "CNT-1";
			asnWithNoReferences.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 15);

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, asnWithAllReferences.PK, "ASN with both MasterBill and ConsolNumber should be matched.");
			asnWithAllReferences.Delete();
			Factory.SaveForTesting();

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, asnWithoutMasterBill.PK, "ASN should match by Consol Number.");
			asnWithoutMasterBill.Delete();
			Factory.SaveForTesting();

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, asnWithoutConsolNumber.PK, "ASN should match by Master Bill.");
			asnWithoutConsolNumber.Delete();
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Precondition.", 3, asns.Length);

			var containerDO = Data.CreateContainer("CNT-1", 0);
			Logger.TopLevelDataObject = consolDO;
			new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, warehouse, Logger, newFactory).ReadIntoBusinessObject();
			newFactory.SaveForTesting();

			asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("ASNs with contradicting MasterBill or Consol Number should not be matched.", 4, asns.Length);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_Reimport_DoesNotDuplicateReferences()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingASN, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			Helper.CreateAdditionalReference(matchingASN, "1", WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);
			Helper.CreateAdditionalReference(matchingASN, "2", WarehouseAdditionalReferenceTypes.Codes.Vessel);
			Helper.CreateAdditionalReference(matchingASN, "3", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			matchingASN.WRP_VehicleReference = "CNT-1";

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000", runSheet: "R1");
			consolDO.WayBillNumber = "MB1";

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, matchingASN.PK, "ASN shouldbe matched.");

			var newFactory = new UniversalObjectFactory();
			var loadedASN = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			var additionalReferencesForASN = newFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadedASN.PK));
			Helper.AssertAdditionalReferences(additionalReferencesForASN, AdditionalReferenceTypes.Codes.MasterBill, "MB1", AdditionalReferenceTypes.Descriptions.MasterBill);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, "C1000000", WarehouseAdditionalReferenceTypes.Descriptions.ForwardingConsolNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "R1", WarehouseAdditionalReferenceTypes.Descriptions.RunSheetNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.Vessel, "VES1", WarehouseAdditionalReferenceTypes.Descriptions.Vessel);
			Helper.AssertAdditionalReferences(additionalReferencesForASN, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VF1", WarehouseAdditionalReferenceTypes.Descriptions.VoyageFlightNumber);

			AssertEquals("Booking party should not be duplicated.", 2, loadedASN.DocAddresses.Count);
		}

		[TestDate(2021, 6, 1)]
		public void TestPopulateBizO_Import_SeaCargoOutturnShipment_MatchingToASN()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN, "Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			Helper.CreateAdditionalReference(matchingASN, "Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			matchingASN.WRP_VehicleReference = "CNT-1";

			var nonMatchingASN = Helper.CreateReceiveASN("2", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			nonMatchingASN.WRP_VehicleReference = "CNT-2";

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "PremiseID", "Lloyds", "Voyage", "Vessel");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PremiseID");

			// save required for ASN DBOnlyZQuery
			Factory.SaveForTesting();

			AssertMatchedASN(shipmentDO, consolDO, "CNT-1", bookingPartyOrg, warehouse, matchingASN.PK, "ASN should be matched.");
			AssertContains("ASN '1' was matched for the Consignment - It was created within the last 30 days and has the provided Vessel Lloyds/IMO 'Lloyds' and Voyage Flight Number 'Voyage' and Container Number 'CNT-1'.", Logger.Logs);
		}

		[TestDate(2021, 6, 1)]
		public void TestPopulateBizO_Import_SeaCargoOutturnShipment_MatchingASNOlderThan30DaysNotUsed()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var matchingASNOlderThan30Days = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			matchingASNOlderThan30Days.WRP_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-31);
			Helper.CreateAdditionalReference(matchingASNOlderThan30Days, "Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			Helper.CreateAdditionalReference(matchingASNOlderThan30Days, "Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			matchingASNOlderThan30Days.WRP_VehicleReference = "CNT-1";

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consolDO;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "PremiseID", "Lloyds", "Voyage", "Vessel");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PremiseID");

			Factory.SaveForTesting();

			new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("ASN matching for the Consignment failed - No ASN created within the last 30 days could be found with the provided Vessel Lloyds/IMO 'Lloyds', Voyage Flight Number 'Voyage' and Container Number 'CNT-1'.", Logger.Logs);
		}

		#endregion

		#region TestImportIsRejectedIfMatchedASNHasArrivedPackages

		public void TestImportIsRejectedIfMatchedASNHasArrivedPackages()
		{
			TestImportIsRejectedIfMatchedASNHasArrivedPackages(true);
		}

		public void TestImportDetachesPackagesIfMatchedASNHasBookedPackages()
		{
			TestImportIsRejectedIfMatchedASNHasArrivedPackages(false);
		}

		public void TestImportIsRejectedIfMatchedASNHasArrivedPackages(bool packagesAreArrived)
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var rcn = Helper.CreateReceiveConsignment("HB1", warehouse.PK);
			var matchingASN = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreateAdditionalReference(matchingASN, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(matchingASN, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			matchingASN.WRP_VehicleReference = "CNT-1";
			var package1 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: matchingASN);
			var package2 = Helper.CreatePackageState(rcn, 1, "PLT", "PKG-2", TransitWarehouseStatuses.Codes.Booked, receiveASN: matchingASN);

			if (packagesAreArrived)
			{
				var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, ZGuid.Empty);
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;
				Helper.CreatePackageState(rtu, 1, "PLT", "PKG-3", TransitWarehouseStatuses.Codes.Arrived, receiveASN: matchingASN);
				Helper.CreatePackageState(rtu, 1, "PLT", "PKG-4", TransitWarehouseStatuses.Codes.Arrived, receiveASN: matchingASN);
			}

			Factory.SaveForTesting();

			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Logger.TopLevelDataObject = consolDO;

			if (packagesAreArrived)
			{
				AssertExceptionThrown(typeof(DataObjectReadFailureException),
@"Cannot attach the following packages to Advanced Shipping Notice 1. These Packages were already received into the Warehouse. System cannot create a Receive Instruction for this ASN.
Package        RCN            Status
PKG-3          -              Arrived
PKG-4          -              Arrived",
					() => new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, warehouse, Logger, Factory).ReadIntoBusinessObject());
			}
			else
			{
				var newFactory = new UniversalObjectFactory();
				AssertNoExceptionThrown("The import should not be rejected if no packages have arrived on the ASN.",
					() => new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, warehouse, Logger, newFactory).ReadIntoBusinessObject());
				newFactory.SaveForTesting();
				var packages = newFactory.Load<WhsItemPackageState>(new ZQuery());
				AssertContainsExactElementsInAnyOrder("Booked packages should be detached from a matched ASN.", new[] { ZGuid.Empty, ZGuid.Empty }, packages.Select(p => p.WPS_WRP_ReceiveExpectedPacking));
			}
		}

		#endregion

		#region TestImportRTU

		public void TestImportRTU()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Logger.TopLevelDataObject = Data.HeaderDataObject;

			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);
			reader.ReadIntoBusinessObject();

			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("An RTU should be created if the import includes a consol and a container with a container number", 1, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("An RTU should be linked to the ASN", 1, pivots.Length);

			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var readerForReimport = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, newFactory);
			readerForReimport.ReadIntoBusinessObject();

			var rtusAfterReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("The RTU should be matched", 1, rtus.Length);
			AssertEquals("The RTU should be created for the container", "CNT-1", rtus[0].WRH_VehicleReference);
			var pivotsAfterReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("The pivot should not be duplicated", 1, pivots.Length);

			AssertNoExceptionThrown(() => Factory.SaveForTesting());
		}

		public void TestImportRTU_GivenSeaCargoOutturn_CreateRTU()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });

			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "PremiseID", "Lloyds", "Voyage", "Vessel");
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			consolDO.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = "PremiseID"
							},
						});

			// set the current warehouse premise ID to match to the shipment
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");
			Factory.SaveForTesting();
			var newFactory = new UniversalObjectFactory();
			var readerForReimport = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, newFactory);
			readerForReimport.ReadIntoBusinessObject();

			var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("1 RTU is created.", 1, rtus.Length);
			var pivots = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("An RTU should be linked to the ASN.", 1, pivots.Length);
		}

		public void TestImportRTU_GivenDepartureTransitWarehouse_DoesNotCreateRTU()
		{
			AssertNoRTUCreated(Data.HeaderDataObject, null, false,
				"No RTU should be created if the import is not for an arrival transit warehouse");
		}

		public void TestImportRTU_GivenNullContainer_DoesNotCreateRTU()
		{
			AssertNoRTUCreated(Data.HeaderDataObject, null, true,
				"No RTU should be created if the import does not include a container");
		}

		public void TestImportRTU_GivenEmptyContainerNumber_DoesNotCreateRTU()
		{
			AssertNoRTUCreated(Data.HeaderDataObject, Data.CreateContainer("", 0), true,
				"No RTU should be created if the import does not include a container number");
		}

		public void TestImportRTU_GivenNullConsol_DoesNotCreateRTU()
		{
			AssertNoRTUCreated(null, Data.CreateContainer("CNT-1", 0), true,
				"No RTU should be created if the import does not include a consol");
		}

		void AssertNoRTUCreated(Shipment consolDO, Container containerDO, bool isArrivalTransitWarehouse, string errorMessage)
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var recipientRoleType = isArrivalTransitWarehouse ? RecipientRoleType.ATW : RecipientRoleType.DTW;
			consolDO?.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });
			Logger.TopLevelDataObject = consolDO;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);
			var asn = reader.ReadIntoBusinessObject();
			AssertNotNull(asn);
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var newFactory = new UniversalObjectFactory();
			var rtus = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());

			AssertEquals(errorMessage, 0, rtus.Length);
		}

		#endregion

		#region TestParameters

		public void TestParametersNotNull()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			var packageStates = Array.Empty<IColumnIndexer>();
			AssertExceptionThrown<ArgumentNullException>("ShipmentDO must not be null", () => new WhsTransitReceiveASNDataObjectReader(null, new Container[] { containerDO }, packageStates, bookingPartyOrg, Data.Warehouse, Logger, Factory));
			var shipmentDO = Data.ShipmentDataObject;
			AssertExceptionThrown<ArgumentException>("WarehousePK cannot be empty", () => new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, packageStates, bookingPartyOrg, null, Logger, Factory));
		}

		public void TestPopulateBizO_CreateNewASN_NullParams()
		{
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, null, Data.Warehouse, Logger, Factory);
			AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Logs

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromContainer()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to Container Number CNT-1.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromMasterBill()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			Data.HeaderDataObject.WayBillNumber = "MB1";
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to Master Bill MB1.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromConsolNumber()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			Data.HeaderDataObject.WayBillNumber = null;
			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000");
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to Consol Number C1000000.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromHouseBill()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.ShipmentDataObject.WayBillNumber = "HB1";
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to House Bill HB1.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromShipmentNumber()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.ShipmentDataObject.WayBillNumber = "";
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to Shipment Number S1000000.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedFromRunSheet()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, runSheet: "RS1");
			Data.HeaderDataObject.WayBillNumber = null;
			Data.HeaderDataObject.VoyageFlightNo = "VFN";
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "ASN reference set to Voyage Flight Number VFN.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_CreatedWithoutReferences()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject);
			Data.HeaderDataObject.WayBillNumber = null;
			Data.HeaderDataObject.VoyageFlightNo = null;
			Data.ShipmentDataObject.WayBillNumber = null;
			Data.ShipmentDataObject.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment).Key = "";
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when setting the ASN reference.", "A Container Number, Consol Number, Master Bill, House Bill, Shipment Number, or Voyage Flight Number is required to set the ASN reference.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_ContainerNumberEmpty_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.LloydsIMO = "Lloyds";
			consolDO.VoyageFlightNo = "Voyage";
			consolDO.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = "PremiseID"
							},
						});

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveASNDataObjectReader(shipment, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an ASN because of below errors. Correct them and try again.
Container Number is not found.
", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_PremiseIDNotMatched_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;
			Data.SetupSeaCargoOutturnDataContextOnShipmentObject(consolDO, "SEA");
			PopulateShipmentObjectWithSeaCargoOutturnData(consolDO, "RandomID", "Lloyds", "Voyage", "Vessel");

			// set the intended warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveASNDataObjectReader(shipment, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Premised ID not matched to intended warehouse.",
@"The premise ID 'RandomID' did not match the intended warehouse 'Transit Warehouse' premise ID (CCP) 'PREMISEID'.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_PremiseIDAndVoyageFlightNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.LloydsIMO = "Lloyds";

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveASNDataObjectReader(shipment, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an ASN because of below errors. Correct them and try again.
Premise ID is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_VesselLloydsAndPremiseIDNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.VoyageFlightNo = "Voyage";

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveASNDataObjectReader(shipment, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an ASN because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Premise ID is not found.
", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_VesselLloydsAndVoyageFlightNotFound_FromSeaCargoOutturn()
		{
			var shipment = Data.ShipmentDataObject;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			shipment.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			consolDO.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "SEA");
			consolDO.SetAdditionalReferenceCollection(() =>
						new DataObjectList<AdditionalReference>
						{
							new AdditionalReference
							{
								Type = new EntryType
								{
									Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
									Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
								},
								ContextInformation = GlbCompany.CurrentCompany.Country.RN_Code,
								ReferenceNumber = "PremiseID"
							},
						});

			// set the current warehouse premise ID
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID");

			var reader = new WhsTransitReceiveASNDataObjectReader(shipment, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory);

			AssertExceptionThrown<DataObjectReadFailureException>("Premise ID not found.",
@"UXML received could not be used to match to an ASN because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPopulateBizO_AddsLogForNewASN_WithoutConsolReferences()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, runSheet: "RS1");
			Data.HeaderDataObject.WayBillNumber = null;
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when the Consol has no Master Bill or Consol Number.", "ASN matching failed - A Master Bill or Consol Number was not provided for the Consolidation of Consignments.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_WithoutConsolOrHouseBill()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.ShipmentDataObject.WayBillNumber = null;
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when the Shipment has no House Bill.", "ASN matching failed - A House Bill was not provided for the Consignment.", Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_FromShipment()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.ShipmentDataObject.WayBillNumber = "HB1";
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when no matching ASN is found.",
				  "ASN matching for the Consignment failed - No ASN created within the last 30 days could be found with the provided Warehouse TWH, and reference HB1.",
				  Logger.Logs);
		}

		public void TestPopulateBizO_AddsLogForNewASN_FromConsol()
		{
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			Data.ShipmentDataObject.WayBillNumber = null;
			var consolDO = Data.HeaderDataObject;
			consolDO.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();

			AssertContains("Should add a log when no matching ASN is found.",
				  "ASN matching for the Consolidation of Consignments failed - No ASN created within the last 30 days could be found with the provided Warehouse TWH, Master Bill MB1, Consol Number C1000000, and reference CNT-1.",
				  Logger.Logs);
		}

		void AssertLogsForMatchedASN(bool hasMasterBill, bool hasConsolNumber, string errorMessage, string expectedLog)
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var asn = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			asn.WRP_VehicleReference = "CNT-1";
			asn.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			if (hasConsolNumber)
			{
				Helper.CreateAdditionalReference(asn, "C1000000", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			}

			if (hasMasterBill)
			{
				Helper.CreateAdditionalReference(asn, "MB1", AdditionalReferenceTypes.Codes.MasterBill);
			}

			Factory.SaveForTesting();

			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000");
			Data.HeaderDataObject.WayBillNumber = "MB1";
			var containerDO = Data.CreateContainer("CNT-1", 0);
			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, new Container[] { containerDO }, null, bookingPartyOrg, warehouse, Logger, newFactory);
			var matchedASN = reader.ReadIntoBusinessObject();

			AssertEquals("Precondition", asn.PK, matchedASN.PK);
			AssertContains(errorMessage, expectedLog, Logger.Logs);
		}

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedASN_WithConsolNumberAndMasterBill() => AssertLogsForMatchedASN(
			hasMasterBill: true,
			hasConsolNumber: true,
			errorMessage: "Should add a log when matching an ASN with both Consol Number and Master Bill.",
			expectedLog: "ASN 1 was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse TWH, Master Bill MB1, Consol Number C1000000, and reference CNT-1.");

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedASN_WithoutMasterBill() => AssertLogsForMatchedASN(
			hasMasterBill: false,
			hasConsolNumber: true,
			errorMessage: "Should add a log when matching an ASN without a Master Bill.",
			expectedLog: "ASN 1 was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse TWH, Consol Number C1000000, and reference CNT-1.");

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedASN_WithoutConsolNumber() => AssertLogsForMatchedASN(
			hasMasterBill: true,
			hasConsolNumber: false,
			errorMessage: "Should add a log when matching an ASN without a Consol Number.",
			expectedLog: "ASN 1 was matched for the Consolidation of Consignments - It was created within the last 30 days and has the provided Warehouse TWH, Master Bill MB1, and reference CNT-1.");

		[TestDate(2020, 9, 23)]
		public void TestPopulateBizO_AddsLogForMatchedASN_FromShipment()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;

			var asn = Helper.CreateReceiveASN("1", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			asn.WRP_VehicleReference = "HB1";
			asn.WRP_SystemCreateTimeUtc = new ZDateTime(2020, 9, 10);

			Factory.SaveForTesting();

			Data.ShipmentDataObject.WayBillNumber = "HB1";
			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var reader = new WhsTransitReceiveASNDataObjectReader(Data.ShipmentDataObject, null, null, bookingPartyOrg, warehouse, Logger, newFactory);
			var matchedASN = reader.ReadIntoBusinessObject();

			AssertEquals("Precondition", asn.PK, matchedASN.PK);
			AssertContains("Should add a log when matching an ASN with House Bill", "ASN 1 was matched for the Consignment - It was created within the last 30 days and has the provided Warehouse TWH, and reference HB1.", Logger.Logs);
		}

		#endregion

		#region TestImportNotes

		public void TestPopulateBizO_ImportNoteInXML()
		{
			var warehouse = Data.Warehouse;
			var bookingPartyOrg = Data.Orgs.INTHEMSYD;
			var shipmentDO = Data.ShipmentDataObject;
			var consolDO = Data.HeaderDataObject;

			consolDO.WayBillNumber = "MB1";
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C00000001", runSheet: "R1");
			consolDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			shipmentDO.SetNoteCollection(() => Helper.CreateNotes("shipment", "test shipment"));

			Logger.TopLevelDataObject = consolDO;
			var receiveASN = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, bookingPartyOrg, warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var notes = receiveASN.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Should import all note visibility types.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note", StmNoteDescription.Int, true);

			consolDO.SetNoteCollection(() => new DataObjectList<Note>()
			{
				Helper.CreateNote("Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, StmNoteDescription.PubDescriptive),
				Helper.CreateNote("Private Note", "Private Note UPDATED", StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive),
				Helper.CreateNote("Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive),
				Helper.CreateNote("Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, StmNoteDescription.IntDescriptive)
			});
			shipmentDO.SetNoteCollection(() => Helper.CreateNotes("shipment", "test shipment 1"));

			receiveASN = new WhsTransitReceiveASNDataObjectReader(shipmentDO, null, null, bookingPartyOrg, warehouse, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			notes = receiveASN.Notes.GetAllNotes().Cast<StmNote>();
			AssertEquals("Notes should have been updated.", 4, notes.Count());
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Pub), "Client-Visible Note", "Client-Visible Note UPDATED", StmNoteDescription.Pub, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Prv), "Private Note", "Private Note UPDATED", StmNoteDescription.Prv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Agv), "Agent-Visible Note", "Agent-Visible Note UPDATED", StmNoteDescription.Agv, true);
			Helper.AssertNoteContents(notes.Single(n => n.ST_NoteType == StmNoteDescription.Int), "Internal Note", "Internal Note UPDATED", StmNoteDescription.Int, true);
		}

		#endregion

		#region Test Populate Universal Job Links to Consol

		public void TestUniversalLinks()
		{
			var bookingParty = Data.Orgs.INTHEMSYD;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			var asn = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this ASN.", 1, links.Count(l => l.UCL_ParentID == asn.PK));
			AssertUniversalJobLink(links, bookingParty, asn.PK, asn.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
			Factory.SaveForTesting();

			var universalFactoryAfterResending = new UniversalObjectFactory();
			var asnAfterResending = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty, Data.Warehouse, Logger, universalFactoryAfterResending).ReadIntoBusinessObject();
			universalFactoryAfterResending.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same ASN.", asn.PK, asnAfterResending.PK);

			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this ASN.", 1, linksInNewFactory.Count(l => l.UCL_ParentID == asnAfterResending.PK));
			AssertUniversalJobLink(linksInNewFactory, bookingParty, asnAfterResending.PK, asnAfterResending.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_ResendWithDifferentDataSource()
		{
			var bookingParty = Data.Orgs.INTHEMSYD;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;

			var asn = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only one universal link being created for this ASN.", 1, links.Count(l => l.UCL_ParentID == asn.PK));
			AssertUniversalJobLink(links, bookingParty, asn.PK, asn.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");

			links.Single(l => l.UCL_ParentID == asn.PK).UCL_SourceType = nameof(DataContextType.LandTransportConsignmentConsol);
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var newASN = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty, Data.Warehouse, Logger, newUniversalFactory).ReadIntoBusinessObject();
			newUniversalFactory.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same ASN.", asn.PK, newASN.PK);

			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be only two universal links being created for this ASN.", 2, linksInNewFactory.Count(l => l.UCL_ParentID == newASN.PK));
			AssertUniversalJobLink(linksInNewFactory, bookingParty, asn.PK, asn.TablePrefix, DataContextType.LandTransportConsignmentConsol, "C1000000", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(linksInNewFactory, bookingParty, newASN.PK, newASN.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_ResendWithDifferentBookingParty()
		{
			var bookingParty1 = Data.Orgs.INTHEMSYD;
			var bookingParty2 = Data.Orgs.CRAHOLSYD;
			var consolDO = Data.HeaderDataObject;
			var containerDO = Data.CreateContainer("CNT-1", 0);
			Data.SetupNewDataContextWithDataSource(consolDO, consolNumber: "C1000000");
			Logger.TopLevelDataObject = consolDO;
			var asnWithoutBookingParty = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, null, Data.Warehouse, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var linksWithoutBookingParty = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("There must not be any universal links being created for this ASN", 0, linksWithoutBookingParty.Count(l => l.UCL_ParentID == asnWithoutBookingParty.PK));

			var universalFactoryForBookingParty = new UniversalObjectFactory();
			var asnWithBookingParty = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty1, Data.Warehouse, Logger, universalFactoryForBookingParty).ReadIntoBusinessObject();
			universalFactoryForBookingParty.SaveForTesting();

			AssertEquals(asnWithoutBookingParty.PK, asnWithBookingParty.PK);

			var linksWithBookingParty = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be one universal link being created for this ASN", 1, linksWithBookingParty.Count(l => l.UCL_ParentID == asnWithBookingParty.PK));
			AssertUniversalJobLink(linksWithBookingParty, bookingParty1, asnWithBookingParty.PK, asnWithBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");

			var universalFactoryForAnotherBookingParty = new UniversalObjectFactory();
			var tempCompany = universalFactoryForAnotherBookingParty.New<GlbCompany>();
			tempCompany.GC_Code = "FWD";
			consolDO.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var asnWithAnotherBookingParty = new WhsTransitReceiveASNDataObjectReader(consolDO, new Container[] { containerDO }, null, bookingParty2, Data.Warehouse, Logger, universalFactoryForAnotherBookingParty).ReadIntoBusinessObject();
			universalFactoryForAnotherBookingParty.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same ASN.", asnWithoutBookingParty.PK, asnWithAnotherBookingParty.PK);

			var linksWithAnotherBookingParty = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: There must be two universal links being created for this ASN", 2, linksWithAnotherBookingParty.Count(l => l.UCL_ParentID == asnWithAnotherBookingParty.PK));
			AssertUniversalJobLink(linksWithAnotherBookingParty.Where(l => l.UCL_CompanyCode == "EDI").ToArray(), bookingParty1, asnWithAnotherBookingParty.PK, asnWithAnotherBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(linksWithAnotherBookingParty.Where(l => l.UCL_CompanyCode == "FWD").ToArray(), bookingParty2, asnWithAnotherBookingParty.PK, asnWithAnotherBookingParty.TablePrefix, DataContextType.ForwardingConsol, "C1000000", "EDI", "DAT", "FWD");
		}

		#endregion

		void AssertMatchedASN(Shipment shipmentDO, Shipment consolDO, ZString containerNumber, IOrgHeader bookingPartyOrg, IColumnIndexer warehouse, ZGuid expectedASNPK, string errorMessage)
		{
			var containerDO = Data.CreateContainer(containerNumber, 0);
			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = consolDO;
			var warehouseBizo = Factory.Load<WhsWarehouse>(warehouse.GetValue(WhsWarehouseSchema.PK));
			var asn = new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, warehouseBizo, Logger, newFactory).ReadIntoBusinessObject();
			AssertEquals(errorMessage, expectedASNPK, asn.PK);
			newFactory.SaveForTesting();
		}

		void AssertNotMatchedASN(Shipment shipmentDO, Shipment consolDO, ZString containerNumber, IOrgHeader bookingPartyOrg, IColumnIndexer warehouse, ZGuid expectedASNPK, string errorMessage)
		{
			var containerDO = Data.CreateContainer(containerNumber, 0);
			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = consolDO;
			var warehouseBizo = Factory.Load<WhsWarehouse>(warehouse.GetValue(WhsWarehouseSchema.PK));
			var asn = new WhsTransitReceiveASNDataObjectReader(shipmentDO, new Container[] { containerDO }, null, bookingPartyOrg, warehouseBizo, Logger, newFactory).ReadIntoBusinessObject();
			AssertNotEquals(errorMessage, expectedASNPK, asn.PK);
			newFactory.SaveForTesting();
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
