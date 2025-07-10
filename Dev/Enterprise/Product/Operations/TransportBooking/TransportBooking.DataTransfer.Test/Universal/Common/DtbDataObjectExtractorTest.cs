using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	public class DtbDataObjectExtractorTest : TestCaseWithFactory
	{
		public void TestGetAddressDO_LocalCartageImporter()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.ConsigneeAddress,
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.ConsigneeDocumentaryAddress,
				DocAddressType.ImporterPickupDeliveryAddress,
				DocAddressType.ConsigneePickupDeliveryAddress,
				DocAddressType.LocalCartageImporter
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageImporter, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageImporter, DtbBookingDirection.PIC);
				var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageImporter, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
				AssertEquals(addedAddress, foundAddressDLV);
			}
		}

		public void TestGetAddressDO_LocalCartageImporter_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageImporter, DtbBookingDirection.DLV));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery);
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.ConsigneeDocumentaryAddress); // Should preference Routing Addresses over Documentary Address
			AssertEquals(delivery, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageImporter, DtbBookingDirection.DLV));
		}

		public void TestGetAddressDO_LocalCartageImporter_PreferencesConsolPickupDeliveryOverDocumentaryAddresses()
		{
			var docAddressTypes = new[] {
				DocAddressType.ConsigneeAddress,
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.ConsigneeDocumentaryAddress
			};

			var consolDocAddressTypes = new[] {
				DocAddressType.ImporterPickupDeliveryAddress,
				DocAddressType.ConsigneePickupDeliveryAddress,
				DocAddressType.LocalCartageImporter
			};

			AssertPreferencesConsolPickupDeliveryOverDocumentaryAddresses(DocAddressType.LocalCartageImporter, docAddressTypes, consolDocAddressTypes);
		}

		public void TestGetAddressDO_LocalCartageImporter_PreferencesDocumentaryAddressesOverConsolDocumentaryAddresses()
		{
			var docAddressTypes = new[] {
				DocAddressType.ConsigneeAddress,
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.ConsigneeDocumentaryAddress
			};

			AssertPreferencesDocumentaryAddressesOverConsolDocumentaryAddresses(DocAddressType.LocalCartageImporter, docAddressTypes);
		}

		public void TestGetAddressDO_LocalCartageExporter()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.ConsignorDocumentaryAddress,
				DocAddressType.SupplierDocumentaryAddress,
				DocAddressType.ConsignorPickupDeliveryAddress,
				DocAddressType.SupplierPickupDeliveryAddress,
				DocAddressType.LocalCartageExporter
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageExporter, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageExporter, DtbBookingDirection.PIC);
				var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageExporter, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
				AssertEquals(addedAddress, foundAddressDLV);
			}
		}

		public void TestGetAddressDO_LocalCartageExporter_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageExporter, DtbBookingDirection.PIC));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery);
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.ConsignorDocumentaryAddress); // Should preference Routing Addresses over Documentary Address
			AssertEquals(pickup, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageExporter, DtbBookingDirection.PIC));
		}

		public void TestGetAddressDO_LocalCartageExporter_PreferencesConsolPickupDeliveryOverDocumentaryAddresses()
		{
			var docAddressTypes = new[] {
				DocAddressType.ConsignorDocumentaryAddress,
				DocAddressType.SupplierDocumentaryAddress
			};

			var consolDocAddressTypes = new[] {
				DocAddressType.ConsignorPickupDeliveryAddress,
				DocAddressType.SupplierPickupDeliveryAddress,
				DocAddressType.LocalCartageExporter
			};

			AssertPreferencesConsolPickupDeliveryOverDocumentaryAddresses(DocAddressType.LocalCartageExporter, docAddressTypes, consolDocAddressTypes);
		}

		public void TestGetAddressDO_LocalCartageExporter_PreferencesDocumentaryAddressesOverConsolDocumentaryAddresses()
		{
			var docAddressTypes = new[] {
				DocAddressType.ConsignorDocumentaryAddress,
				DocAddressType.SupplierDocumentaryAddress
			};

			AssertPreferencesDocumentaryAddressesOverConsolDocumentaryAddresses(DocAddressType.LocalCartageExporter, docAddressTypes);
		}

		public void TestGetAddressDO_LocalCartageCFS_PIC()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsDepotAddress,
				DocAddressType.DepartureCFSAddress,
				DocAddressType.LocalCartageCFS
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCFS, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCFS, DtbBookingDirection.PIC);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageCFS_DLV()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsDepotAddress,
				DocAddressType.ArrivalCFSAddress,
				DocAddressType.LocalCartageCFS
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCFS, DtbBookingDirection.DLV));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCFS, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageCTO_PIC()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsContainerTerminalOperatorAddress,
				DocAddressType.DepartureCTOAddress,
				DocAddressType.LocalCartageCTO
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.PIC);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageCTO_PIC_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.PIC));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery);
			AssertEquals(pickupCTO, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.PIC));
		}

		public void TestGetAddressDO_LocalCartageCTO_DLV()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsContainerTerminalOperatorAddress,
				DocAddressType.ArrivalCTOAddress,
				DocAddressType.LocalCartageCTO
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.DLV));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageCTO_DLV_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.DLV));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery);
			AssertEquals(deliveryCTO, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageCTO, DtbBookingDirection.DLV));
		}

		public void TestGetAddressDO_LocalCartageYard_PIC()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsContainerYardAddress,
				DocAddressType.ContainerYardEmptyPickupAddress,
				DocAddressType.DepartureCYDAddress,
				DocAddressType.LocalCartageYard
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageYard_PIC_Container()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC));

			var pickupCYD = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress);
			var deliveryCYD = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress);

			shipment = GetNewShipmentWithContainers(pickupCYD, deliveryCYD);
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.LocalCartageYard); // Should preference address on containers
			AssertEquals(pickupCYD, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC));
		}

		public void TestGetAddressDO_LocalCartageYard_PIC_Container_ContainerFilterBehaviour()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC));

			var pickupCYD1 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "1 Pickup Road");
			var deliveryCYD1 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "1 Delivery Road");
			var pickupCYD2 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "2 Pickup Road");
			var deliveryCYD2 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "2 Delivery Road");

			shipment = GetNewShipmentWithContainersWithLinkAndNumber(pickupCYD1, deliveryCYD1, pickupCYD2, deliveryCYD2, 1, "CN1", 2, "CN2");
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.LocalCartageYard); // Should preference address on containers

			AssertEquals("Should match pickup on container link", pickupCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC, containerLinksFilter: new List<ZInt>() { 2 }));
			AssertEquals("Should match pickup on container number", pickupCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC, containerNumbersFilter: new List<ZString>() { "CN2" }));
			AssertEquals("Should match pickup on container link or number", pickupCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC, containerLinksFilter: new List<ZInt>() { 2 }, containerNumbersFilter: new List<ZString>() { "CN2" }));
			AssertEquals("If override lists are null or only contain empty elements then do not filter containers", pickupCYD1, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.PIC, containerLinksFilter: new List<ZInt>() { }, containerNumbersFilter: new List<ZString>() { ZString.Empty }));
		}

		public void TestGetAddressDO_LocalCartageYard_DLV()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.CustomsContainerYardAddress,
				DocAddressType.ContainerYardEmptyReturnAddress,
				DocAddressType.ArrivalCYDAddress,
				DocAddressType.LocalCartageYard
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
			}
		}

		public void TestGetAddressDO_LocalCartageYard_DLV_Container()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV));

			var pickupCYD = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress);
			var deliveryCYD = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress);

			shipment = GetNewShipmentWithContainers(pickupCYD, deliveryCYD);
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.LocalCartageYard); // Should preference address on containers
			AssertEquals(deliveryCYD, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV));
		}

		public void TestGetAddressDO_LocalCartageYard_DLV_Container_WithOverrideList()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV));

			var pickupCYD1 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "1 Pickup Road");
			var deliveryCYD1 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "1 Delivery Road");
			var pickupCYD2 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyPickupAddress, "2 Pickup Road");
			var deliveryCYD2 = GetNewOrganizationAddress(DocAddressType.ContainerYardEmptyReturnAddress, "2 Delivery Road");

			shipment = GetNewShipmentWithContainersWithLinkAndNumber(pickupCYD1, deliveryCYD1, pickupCYD2, deliveryCYD2, 1, "CN1", 2, "CN2");
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.LocalCartageYard); // Should preference address on containers
			AssertEquals("Should match delivery on container link", deliveryCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV, containerLinksFilter: new List<ZInt>() { 2 }));
			AssertEquals("Should match delivery on container number", deliveryCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV, containerNumbersFilter: new List<ZString>() { "CN2" }));
			AssertEquals("Should match delivery on container link or number", deliveryCYD2, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV, containerLinksFilter: new List<ZInt>() { 2 }, containerNumbersFilter: new List<ZString>() { "CN2" }));
			AssertEquals("If override lists are null or only contain empty elements then do not filter containers", deliveryCYD1, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageYard, DtbBookingDirection.DLV, containerLinksFilter: new List<ZInt>() { }, containerNumbersFilter: new List<ZString>() { ZString.Empty }));
		}

		public void TestGetAddressDO_TransportCompanyDocumentaryAddress()
		{
			var shipment = GetNewShipment();
			var consol = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC, consol));

			var addedPicAddress = AddAddress(consol, DocAddressType.DepartureCFSLocalTransportAddress);
			var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC, consol);
			AssertEquals("Should find pickup/delivery addresses from consols", addedPicAddress, foundAddressPIC);

			var addedDlvAddress = AddAddress(consol, DocAddressType.ArrivalCFSLocalTransportAddress);
			var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.DLV, consol);
			AssertEquals("Should find pickup/delivery addresses from consols", addedDlvAddress, foundAddressDLV);

			addedPicAddress = AddAddress(shipment, AddressTypes.PickupLocalCartage);
			foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC, consol);
			AssertEquals("Should preference pickup/delivery addresses from shipments over addresses from consols", addedPicAddress, foundAddressPIC);

			addedDlvAddress = AddAddress(shipment, AddressTypes.DeliveryLocalCartage);
			foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.DLV, consol);
			AssertEquals("Should preference pickup/delivery addresses from shipments over addresses from consols", addedDlvAddress, foundAddressDLV);

			var addedDocAddress = AddAddress(shipment, DocAddressType.TransportCompanyDocumentaryAddress);
			foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC, consol);
			foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.DLV, consol);
			AssertEquals("Should preference transport company documentary address over pickup/delivery addresses", addedDocAddress, foundAddressPIC);
			AssertEquals("Should preference transport company documentary address over pickup/delivery addresses", addedDocAddress, foundAddressDLV);
		}

		public void TestGetAddressDO_LocalCartageWarehouse()
		{
			var shipment = GetNewShipment();

			var docAddressTypes = new[] {
				DocAddressType.Warehouse,
				DocAddressType.CustomsWarehouseAddress,
				DocAddressType.LocalCartageWarehouse
			};

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageWarehouse, DtbBookingDirection.PIC));

			foreach (var docAddressType in docAddressTypes)
			{
				var addedAddress = AddAddress(shipment, docAddressType);
				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageWarehouse, DtbBookingDirection.PIC);
				var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageWarehouse, DtbBookingDirection.DLV);
				AssertEquals(addedAddress, foundAddressPIC);
				AssertEquals(addedAddress, foundAddressDLV);
			}
		}

		public void TestGetAddressDO_LocalCartageService()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageService, DtbBookingDirection.PIC));

			var addedAddress = AddAddress(shipment, DocAddressType.LocalCartageService);
			var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageService, DtbBookingDirection.PIC);
			var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageService, DtbBookingDirection.DLV);
			AssertEquals(addedAddress, foundAddressPIC);
			AssertEquals(addedAddress, foundAddressDLV);
		}

		public void TestGetAddressDO_LocalCartageMisc()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageMSC, DtbBookingDirection.PIC));

			var addedAddress = AddAddress(shipment, DocAddressType.LocalCartageMSC);
			var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageMSC, DtbBookingDirection.PIC);
			var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.LocalCartageMSC, DtbBookingDirection.DLV);
			AssertEquals(addedAddress, foundAddressPIC);
			AssertEquals(addedAddress, foundAddressDLV);
		}

		OrganizationAddress AddAddress(Shipment shipment, DocAddressType docAddressType)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			return shipment.AddOrgAddress(GetNewDataObjectWriter(), orgAddress, docAddressType);
		}

		OrganizationAddress AddAddress(Shipment shipment, string addressType)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			return shipment.AddOrgAddress(GetNewDataObjectWriter(), orgAddress, addressType);
		}

		OrganizationAddress GetNewOrganizationAddress(DocAddressType? addressType = null, ZString? address1 = null)
		{
			var addressTypeToUse = addressType?.ToString() ?? "MSC";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			if (address1.HasValue)
			{
				orgAddress.Address1 = address1.Value;
			}
			return new OrganizationDataObjectWriter(GetNewDataObjectWriter(), addressTypeToUse).GetDataObject(orgAddress);
		}

		public void TestCarrier_PIC_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();
			var carrier = GetNewOrganizationAddress();

			pickup.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			pickupCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			transCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			deliveryCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			delivery.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			carrier.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery, picCarrier: carrier, picCarrierRef: "PICAR", picCarrierSvcLvl: "PCR");
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals(carrier, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.PIC));
			AssertEquals("PICAR", DtbDataObjectExtractor.GetCarrierReferenceFromRouting(shipment, true).Value);
			AssertEquals("PCR", DtbDataObjectExtractor.GetCarrierServiceLevelFromRouting(shipment, true).Code);
		}

		public void TestCarrier_DLV_Routing()
		{
			var shipment = GetNewShipment();
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.DLV));

			var pickup = GetNewOrganizationAddress();
			var pickupCTO = GetNewOrganizationAddress();
			var transCTO = GetNewOrganizationAddress();
			var deliveryCTO = GetNewOrganizationAddress();
			var delivery = GetNewOrganizationAddress();
			var carrier = GetNewOrganizationAddress();

			pickup.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			pickupCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			transCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			deliveryCTO.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			delivery.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);
			carrier.AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress);

			shipment = GetNewShipmentWithRouting(pickup, pickupCTO, transCTO, deliveryCTO, delivery, dlvCarrier: carrier, dlvCarrierRef: "DLCAR", dlvCarrierSvcLvl: "DCR");
			shipment.AddOrgAddress(GetNewDataObjectWriter(), Factory.NewWithValidTestData<OrgAddress>(), DocAddressType.ConsigneeDocumentaryAddress);
			AssertEquals(carrier, DtbDataObjectExtractor.GetAddressDO(shipment, DocAddressType.TransportCompanyDocumentaryAddress, DtbBookingDirection.DLV));
			AssertEquals("DLCAR", DtbDataObjectExtractor.GetCarrierReferenceFromRouting(shipment, false).Value);
			AssertEquals("DCR", DtbDataObjectExtractor.GetCarrierServiceLevelFromRouting(shipment, false).Code);
		}

		public void TestGetContainerMode()
		{
			var shipment = GetNewShipment();
			AssertEquals("", DtbDataObjectExtractor.GetContainerMode(shipment));

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			AssertEquals("", DtbDataObjectExtractor.GetContainerMode(shipment));

			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.ContainerCollection.Add(container);
			AssertEquals("", DtbDataObjectExtractor.GetContainerMode(shipment));

			container.FCL_LCL_AIR = new ContainerMode() { Code = Core.Constants.ContainerModes.LCL };
			AssertEquals("", DtbDataObjectExtractor.GetContainerMode(shipment));

			var containerFCL = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			containerFCL.FCL_LCL_AIR = new ContainerMode() { Code = Core.Constants.ContainerModes.FCLMixedShipper };
			shipment.ContainerCollection.Add(containerFCL);
			AssertEquals(Core.Constants.ContainerModes.FCL, DtbDataObjectExtractor.GetContainerMode(shipment));

			containerFCL.FCL_LCL_AIR = new ContainerMode() { Code = Core.Constants.ContainerModes.FCL };
			AssertEquals(Core.Constants.ContainerModes.FCL, DtbDataObjectExtractor.GetContainerMode(shipment));

			shipment.ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.LCL };
			AssertEquals(Core.Constants.ContainerModes.LCL, DtbDataObjectExtractor.GetContainerMode(shipment));

			shipment.ContainerMode = new ContainerMode() { Code = Core.Constants.ContainerModes.FCL };
			AssertEquals(Core.Constants.ContainerModes.FCL, DtbDataObjectExtractor.GetContainerMode(shipment));
		}

		Shipment GetNewShipment()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");
			return shipment;
		}

		Shipment GetNewShipmentWithRouting(OrganizationAddress pickup, OrganizationAddress pickupCTO, OrganizationAddress transCTO, OrganizationAddress deliveryCTO, OrganizationAddress delivery, OrganizationAddress picCarrier = null, OrganizationAddress dlvCarrier = null, string picCarrierRef = "", string dlvCarrierRef = "", string picCarrierSvcLvl = "", string dlvCarrierSvcLvl = "")
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment.TransportLegCollection.Add(AddTransportLeg(1, TransportMode.Road, new UNLOCO() { Code = "AUBNE" }, new UNLOCO() { Code = "AUSYD" }, LegType.PreCarriage, pickup, pickupCTO, picCarrier, picCarrierRef, picCarrierSvcLvl));
			shipment.TransportLegCollection.Add(AddTransportLeg(2, TransportMode.Sea, new UNLOCO() { Code = "AUSYD" }, new UNLOCO() { Code = "NZAKL" }, LegType.Main, pickupCTO, transCTO));
			shipment.TransportLegCollection.Add(AddTransportLeg(3, TransportMode.Sea, new UNLOCO() { Code = "NZAKL" }, new UNLOCO() { Code = "USLAX" }, LegType.Main, transCTO, deliveryCTO));
			shipment.TransportLegCollection.Add(AddTransportLeg(4, TransportMode.Road, new UNLOCO() { Code = "USLAX" }, new UNLOCO() { Code = "USORD" }, LegType.Other, deliveryCTO, delivery, dlvCarrier, dlvCarrierRef, dlvCarrierSvcLvl));

			return shipment;
		}

		TransportLeg AddTransportLeg(ZByte legOrder, TransportMode transportMode, UNLOCO loading, UNLOCO discharge, LegType legType, OrganizationAddress departureFrom, OrganizationAddress arrivalAt, OrganizationAddress carrier = null, string carrierRef = "", string carrierServiceLevel = "")
		{
			var leg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			leg.LegOrder = legOrder;
			leg.TransportMode = transportMode;
			leg.PortOfLoading = loading;
			leg.PortOfDischarge = discharge;
			leg.LegType = legType;
			leg.DepartureFrom = departureFrom;
			leg.ArrivalAt = arrivalAt;
			leg.Carrier = carrier;
			leg.CarrierBookingReference = carrierRef;
			leg.CarrierServiceLevel = new ServiceLevel() { Code = carrierServiceLevel };

			return leg;
		}

		Shipment GetNewShipmentWithContainers(OrganizationAddress pickupEmpty, OrganizationAddress deliverEmpty)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(AddContainer(pickupEmpty, deliverEmpty));
			shipment.ContainerCollection.Add(AddContainer(pickupEmpty, deliverEmpty));

			return shipment;
		}

		Shipment GetNewShipmentWithContainersWithLinkAndNumber(OrganizationAddress pickupEmpty1, OrganizationAddress deliverEmpty1, OrganizationAddress pickupEmpty2, OrganizationAddress deliverEmpty2, ZInt? containerLink1, ZString? containerNumber1, ZInt? containerLink2, ZString? containerNumber2)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM123");

			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(AddContainer(pickupEmpty1, deliverEmpty1, containerLink1, containerNumber1));
			shipment.ContainerCollection.Add(AddContainer(pickupEmpty2, deliverEmpty2, containerLink2, containerNumber2));

			return shipment;
		}

		Container AddContainer(OrganizationAddress pickupEmpty, OrganizationAddress deliverEmpty)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { pickupEmpty, deliverEmpty });
			return container;
		}

		Container AddContainer(OrganizationAddress pickupEmpty, OrganizationAddress deliverEmpty, ZInt? containerLink, ZString? containerNumber)
		{
			var container = AddContainer(pickupEmpty, deliverEmpty);
			container.Link = containerLink;
			container.ContainerNumber = containerNumber;
			return container;
		}

		public DataWritingManager GetNewDataObjectWriter()
		{
			return new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
		}

		void AssertPreferencesConsolPickupDeliveryOverDocumentaryAddresses(DocAddressType addressType, DocAddressType[] docAddressTypes, DocAddressType[] consolDocAddressTypes)
		{
			var consol = GetNewShipment();
			var shipment = GetNewShipment();

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.PIC, consol));
			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.DLV, consol));

			foreach (var docAddressType in docAddressTypes.Union(consolDocAddressTypes))
			{
				var shipmentOrConsolToAddTo = docAddressTypes.Contains(docAddressType) ? shipment : consol;
				var addedAddress = AddAddress(shipmentOrConsolToAddTo, docAddressType);

				var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.PIC, consol);
				var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.DLV, consol);
				AssertEquals("Pickup Address type: " + docAddressType + " is preferenced incorrectly", addedAddress, foundAddressPIC);
				AssertEquals("Delivery Address type: " + docAddressType + " is preferenced incorrectly", addedAddress, foundAddressDLV);
			}
		}

		void AssertPreferencesDocumentaryAddressesOverConsolDocumentaryAddresses(DocAddressType addressType, DocAddressType[] consolDocAddressTypes)
		{
			var consol = GetNewShipment();
			var shipment = GetNewShipment();
			var universalShipments = new[] { consol, shipment };

			AssertNull(DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.PIC, consol));

			foreach (var universalShipment in universalShipments)
			{
				var shipmentOrConsolToAddTo = universalShipment;

				foreach (var docAddressType in consolDocAddressTypes)
				{
					var addedAddress = AddAddress(shipmentOrConsolToAddTo, docAddressType);

					var foundAddressPIC = DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.PIC, consol);
					var foundAddressDLV = DtbDataObjectExtractor.GetAddressDO(shipment, addressType, DtbBookingDirection.DLV, consol);
					AssertEquals(addedAddress, foundAddressPIC);
					AssertEquals(addedAddress, foundAddressDLV);
				}
			}
		}
	}
}
