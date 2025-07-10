using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public static class DtbDataObjectExtractor
	{
		public static OrganizationAddress GetAddressDO(Shipment dataObjWithAddressesAndRouting, DocAddressType docAddressType, DtbBookingDirection direction, Shipment consol = null, IEnumerable<ZInt> containerLinksFilter = null, IEnumerable<ZString> containerNumbersFilter = null)
		{
			var result = GetAddressFromContainers(dataObjWithAddressesAndRouting, docAddressType, direction, containerLinksFilter, containerNumbersFilter)   // Container Addresses (where we are looking for a particular containers Address)
				?? GetPickupDeliveryAddressFromDataObject(dataObjWithAddressesAndRouting, docAddressType, direction); // Pickup / Delivery Addresses

			if (result == null && consol != null)
			{
				result = GetPickupDeliveryAddressFromDataObject(consol, docAddressType, direction);   // Parent (Consol) Pickup / Delivery Addresses
			}

			if (result == null)
			{
				result = GetAddressFromRouting(dataObjWithAddressesAndRouting, docAddressType, direction, dataObjWithAddressesAndRouting);  // Routing Addresses (CTO, CNR and CNE only)
			}

			if (result == null && consol != null)
			{
				result = GetAddressFromRouting(consol, docAddressType, direction, dataObjWithAddressesAndRouting);
			}

			if (result == null)
			{
				result = GetDocumentaryAddressFromDataObject(dataObjWithAddressesAndRouting, docAddressType, direction);    // Documentary Addresses
			}

			if (result == null && consol != null)
			{
				result = GetDocumentaryAddressFromDataObject(consol, docAddressType, direction);  // Parent (Consol) Documentary Addresses	
			}

			return result;
		}

		static IEnumerable<string> GetFallbackPickupDeliveryAddressTypes(DocAddressType dtbAddressType, DtbBookingDirection direction)
		{
			yield return dtbAddressType.ToString();

			if (direction == DtbBookingDirection.PIC)
			{
				switch (dtbAddressType)
				{
					case DocAddressType.LocalCartageImporter:
						yield return nameof(DocAddressType.ConsigneePickupDeliveryAddress);
						yield return nameof(DocAddressType.ImporterPickupDeliveryAddress);
						break;
					case DocAddressType.LocalCartageExporter:
						yield return nameof(DocAddressType.SupplierPickupDeliveryAddress);
						yield return nameof(DocAddressType.ConsignorPickupDeliveryAddress);
						break;
					case DocAddressType.LocalCartageYard:
						yield return nameof(DocAddressType.DepartureCYDAddress);
						yield return nameof(DocAddressType.ContainerYardEmptyPickupAddress);
						yield return nameof(DocAddressType.CustomsContainerYardAddress);
						break;
					case DocAddressType.LocalCartageCFS:
						yield return nameof(DocAddressType.DepartureCFSAddress);
						yield return nameof(DocAddressType.CustomsDepotAddress);
						break;
					case DocAddressType.LocalCartageCTO:
						yield return nameof(DocAddressType.DepartureCTOAddress);
						yield return nameof(DocAddressType.CustomsContainerTerminalOperatorAddress);
						break;
					case DocAddressType.TransportCompanyDocumentaryAddress:
						yield return AddressTypes.PickupLocalCartage;
						yield return nameof(DocAddressType.DepartureCFSLocalTransportAddress);
						break;
					case DocAddressType.LocalCartageWarehouse:
						yield return nameof(DocAddressType.CustomsWarehouseAddress);
						yield return nameof(DocAddressType.Warehouse);
						break;
					case DocAddressType.LocalCartageService:
						break;
					case DocAddressType.LocalCartageMSC:
						break;
					default:
						throw new ArgumentException(string.Format("Address Type not known '{0}'", dtbAddressType.ToString()));
				}
			}
			else
			{
				switch (dtbAddressType)
				{
					case DocAddressType.LocalCartageImporter:
						yield return nameof(DocAddressType.ConsigneePickupDeliveryAddress);
						yield return nameof(DocAddressType.ImporterPickupDeliveryAddress);
						break;
					case DocAddressType.LocalCartageExporter:
						yield return nameof(DocAddressType.SupplierPickupDeliveryAddress);
						yield return nameof(DocAddressType.ConsignorPickupDeliveryAddress);
						break;
					case DocAddressType.LocalCartageYard:
						yield return nameof(DocAddressType.ArrivalCYDAddress);
						yield return nameof(DocAddressType.ContainerYardEmptyReturnAddress);
						yield return nameof(DocAddressType.CustomsContainerYardAddress);
						break;
					case DocAddressType.LocalCartageCFS:
						yield return nameof(DocAddressType.ArrivalCFSAddress);
						yield return nameof(DocAddressType.CustomsDepotAddress);
						break;
					case DocAddressType.LocalCartageCTO:
						yield return nameof(DocAddressType.ArrivalCTOAddress);
						yield return nameof(DocAddressType.CustomsContainerTerminalOperatorAddress);
						break;
					case DocAddressType.TransportCompanyDocumentaryAddress:
						yield return AddressTypes.DeliveryLocalCartage;
						yield return nameof(DocAddressType.ArrivalCFSLocalTransportAddress);
						break;
					case DocAddressType.LocalCartageWarehouse:
						yield return nameof(DocAddressType.CustomsWarehouseAddress);
						yield return nameof(DocAddressType.Warehouse);
						break;
					case DocAddressType.LocalCartageService:
						break;
					case DocAddressType.LocalCartageMSC:
						break;
					default:
						throw new ArgumentException(string.Format("Address Type not known '{0}'", dtbAddressType.ToString()));
				}
			}
		}

		static IEnumerable<string> GetFallbackDocumentaryAddressTypes(DocAddressType dtbAddressType, DtbBookingDirection direction)
		{
			if (direction == DtbBookingDirection.PIC)
			{
				switch (dtbAddressType)
				{
					case DocAddressType.LocalCartageImporter:
						yield return nameof(DocAddressType.ConsigneeDocumentaryAddress);
						yield return nameof(DocAddressType.ImporterDocumentaryAddress);
						yield return nameof(DocAddressType.ConsigneeAddress);
						break;
					case DocAddressType.LocalCartageExporter:
						yield return nameof(DocAddressType.SupplierDocumentaryAddress);
						yield return nameof(DocAddressType.ConsignorDocumentaryAddress);
						break;
					case DocAddressType.LocalCartageYard:
					case DocAddressType.LocalCartageCFS:
					case DocAddressType.LocalCartageCTO:
					case DocAddressType.TransportCompanyDocumentaryAddress:
					case DocAddressType.LocalCartageWarehouse:
					case DocAddressType.LocalCartageService:
					case DocAddressType.LocalCartageMSC:
						break;
					default:
						throw new ArgumentException(string.Format("Address Type not known '{0}'", dtbAddressType.ToString()));
				}
			}
			else
			{
				switch (dtbAddressType)
				{
					case DocAddressType.LocalCartageImporter:
						yield return nameof(DocAddressType.ConsigneeDocumentaryAddress);
						yield return nameof(DocAddressType.ImporterDocumentaryAddress);
						yield return nameof(DocAddressType.ConsigneeAddress);
						break;
					case DocAddressType.LocalCartageExporter:
						yield return nameof(DocAddressType.SupplierDocumentaryAddress);
						yield return nameof(DocAddressType.ConsignorDocumentaryAddress);
						break;
					case DocAddressType.LocalCartageYard:
					case DocAddressType.LocalCartageCFS:
					case DocAddressType.LocalCartageCTO:
					case DocAddressType.TransportCompanyDocumentaryAddress:
					case DocAddressType.LocalCartageWarehouse:
					case DocAddressType.LocalCartageService:
					case DocAddressType.LocalCartageMSC:
						break;
					default:
						throw new ArgumentException(string.Format("Address Type not known '{0}'", dtbAddressType.ToString()));
				}
			}
		}

		static OrganizationAddress GetDocumentaryAddressFromDataObject(Shipment dataObject, DocAddressType docAddressType, DtbBookingDirection direction)
		{
			return GetAddressFromDataObject(dataObject, GetFallbackDocumentaryAddressTypes(docAddressType, direction));
		}

		static OrganizationAddress GetPickupDeliveryAddressFromDataObject(Shipment dataObject, DocAddressType docAddressType, DtbBookingDirection direction)
		{
			return GetAddressFromDataObject(dataObject, GetFallbackPickupDeliveryAddressTypes(docAddressType, direction));
		}

		static OrganizationAddress GetAddressFromDataObject(Shipment dataObject, IEnumerable<string> addressTypes)
		{
			var addresses = dataObject.OrganizationAddressCollection;
			OrganizationAddress address = null;

			foreach (var addType in addressTypes)
			{
				address = addresses.FirstOrDefault(addType);
				if (address != null)
				{
					break;
				}
			}

			return address;
		}

		static OrganizationAddress GetAddressFromRouting(Shipment dataObjWithRouting, DocAddressType docAddressType, DtbBookingDirection direction, Shipment actualParentDO)
		{
			if (direction == DtbBookingDirection.PIC)
			{
				var legs = dataObjWithRouting.TransportLegCollection?.OrderBy(l => l.LegOrder);
				switch (docAddressType)
				{
					case DocAddressType.LocalCartageCTO:
						return GetRoutingLeg(legs, TransportMode.Sea)?.DepartureFrom;
					case DocAddressType.LocalCartageExporter:
						return GetRoutingLeg(legs, TransportMode.Road, actualParentDO.PortOfOrigin, checkIsNotMain: true, hasPickupAddress: true)?.DepartureFrom;
					case DocAddressType.TransportCompanyDocumentaryAddress:
						return GetRoutingLeg(legs, TransportMode.Road, actualParentDO.PortOfOrigin, checkIsNotMain: true, hasCarrierAddress: true)?.Carrier;
				}
			}
			else
			{
				var legs = dataObjWithRouting.TransportLegCollection?.OrderByDescending(l => l.LegOrder);
				switch (docAddressType)
				{
					case DocAddressType.LocalCartageCTO:
						return GetRoutingLeg(legs, TransportMode.Sea)?.ArrivalAt;
					case DocAddressType.LocalCartageImporter:
						return GetRoutingLeg(legs, TransportMode.Road, null, actualParentDO.PortOfDestination, checkIsNotMain: true, hasDeliveryAddress: true)?.ArrivalAt;
					case DocAddressType.TransportCompanyDocumentaryAddress:
						return GetRoutingLeg(legs, TransportMode.Road, null, actualParentDO.PortOfDestination, checkIsNotMain: true, hasCarrierAddress: true)?.Carrier;
				}
			}

			return null;
		}

		static TransportLeg GetRoutingLeg(IOrderedEnumerable<TransportLeg> orderedLegs, TransportMode transportMode, UNLOCO loadPort = null, UNLOCO dischargePort = null, bool checkIsNotMain = false, bool hasPickupAddress = false, bool hasCarrierAddress = false, bool hasDeliveryAddress = false)
		{
			TransportLeg leg = null;
			if (orderedLegs != null)
			{
				leg = orderedLegs.FirstOrDefault(r =>
				r.TransportMode.GetValueOrDefault() == transportMode &&
				(loadPort == null || (r.PortOfLoading != null && r.PortOfLoading.Code == loadPort.Code)) &&
				(dischargePort == null || (r.PortOfDischarge != null && r.PortOfDischarge.Code == dischargePort.Code)) &&
				(!checkIsNotMain || r.LegType.GetValueOrDefault() != LegType.Main) &&
				(!hasPickupAddress || r.DepartureFrom != null) &&
				(!hasCarrierAddress || r.Carrier != null) &&
				(!hasDeliveryAddress || r.ArrivalAt != null));
			}

			return leg;
		}

		static OrganizationAddress GetAddressFromContainers(Shipment dataObjWithContainers, DocAddressType docAddressType, DtbBookingDirection direction, IEnumerable<ZInt> containerLinksFilter, IEnumerable<ZString> containerNumbersfilter)
		{
			OrganizationAddress result = null;

			if (docAddressType == DocAddressType.LocalCartageYard && dataObjWithContainers.ContainerCollection != null)
			{
				var containerLinksFilterList = containerLinksFilter?.ToList();
				var containerNumberFilterList = containerNumbersfilter?.Where(cn => cn != ZString.Empty).ToList();
				var filterOnContainerLink = (containerLinksFilterList != null && containerLinksFilterList.Count > 0);
				var filterOnContainerNumber = (containerNumberFilterList != null && containerNumberFilterList.Count > 0);
				Func<Container, bool> filter;
				if (!filterOnContainerLink && !filterOnContainerNumber)
				{
					filter = c => true;
				}
				else
				{
					filter = c => (filterOnContainerLink && c.Link.HasValue && containerLinksFilterList.Contains(c.Link.Value)) || (filterOnContainerNumber && c.ContainerNumber.HasValue && containerNumberFilterList.Contains(c.ContainerNumber.Value));
				}

				var orgType = direction == DtbBookingDirection.PIC ? DocAddressType.ContainerYardEmptyPickupAddress : DocAddressType.ContainerYardEmptyReturnAddress;
				var addresses = dataObjWithContainers.ContainerCollection.Where(filter).SelectMany(c => c.OrganizationAddressCollection ?? new List<OrganizationAddress>());
				result = addresses.FirstOrDefault(orgType.ToString());
			}

			return result;
		}

		public static ZString GetContainerMode(this Shipment dataObject)
		{
			var result = dataObject.ContainerMode.GetCodeAsUpperCase();
			if (result.IsEmpty)
			{
				var containers = dataObject.ContainerCollection;
				if (containers != null)
				{
					if (containers.Any(c => IsFCL(c)))
					{
						result = Core.Constants.ContainerModes.FCL;
					}
				}
			}

			return result;
		}

		static bool IsFCL(Container container)
		{
			var containerMode = container.FCL_LCL_AIR.GetCodeAsUpperCase();
			return containerMode == Core.Constants.ContainerModes.FCL || containerMode == Core.Constants.ContainerModes.FCLMixedShipper;
		}

		public static ServiceLevel GetCarrierServiceLevelFromRouting(Shipment shipment, ZBool isPickupDirection)
		{
			return GetRoadCarrierRoutingLeg(shipment, isPickupDirection)?.CarrierServiceLevel; // agency
		}

		public static ZString? GetCarrierReferenceFromRouting(Shipment shipment, ZBool isPickupDirection)
		{
			return GetRoadCarrierRoutingLeg(shipment, isPickupDirection)?.CarrierBookingReference; // agency
		}

		static TransportLeg GetRoadCarrierRoutingLeg(Shipment shipment, ZBool isPickupDirection)
		{
			var orderedLegs = isPickupDirection ? shipment.TransportLegCollection?.OrderBy(l => l.LegOrder) : shipment.TransportLegCollection?.OrderByDescending(l => l.LegOrder);
			var originPortMatch = isPickupDirection ? shipment.PortOfOrigin : null;
			var destinationPortMatch = isPickupDirection ? null : shipment.PortOfDestination;
			return GetRoutingLeg(orderedLegs, TransportMode.Road, originPortMatch, destinationPortMatch, true);
		}
	}
}
