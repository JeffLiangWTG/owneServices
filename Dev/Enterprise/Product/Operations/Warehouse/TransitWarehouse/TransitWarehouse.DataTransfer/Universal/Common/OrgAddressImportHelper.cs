using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public static class OrgAddressImportHelper
	{
		public static void PopulateTransportOrg<T>(Shipment consolDO, Shipment sourceDO, OrgHeader bookingParty, T bizO,
			IXmlImportLogger logger, UniversalObjectFactory factory) where T : BusinessObject, IDocAddresses
		{
			OrganizationAddress transportOrg = null;
			if (consolDO != null)
			{
				transportOrg = GetOrgByAddressType<T>(consolDO, nameof(DocAddressType.DepartureCFSLocalTransportAddress), nameof(DocAddressType.ArrivalCFSLocalTransportAddress));
				// Getting transportOrg from Gate Shipment TransportCompanyDocumentaryAddress
				// Needs refactoring when integrating with Gate, maybe Gate can populate TransportCompanyDocumentaryAddress in DepartureCFSLocalTransportAddress
				if (transportOrg == null && consolDO.IsFromGateBooking())
				{
					transportOrg = consolDO.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress));
				}
			}
			else if (sourceDO != null)
			{
				transportOrg = GetOrgByAddressType<T>(sourceDO, AddressTypes.PickupLocalCartage, AddressTypes.DeliveryLocalCartage);
			}

			if (transportOrg != null)
			{
				new OrganisationDataObjectReader(transportOrg, logger, factory).GetMatchedOrNew(bizO, DocAddressType.TransportCompanyDocumentaryAddress);
			}
			else
			{
				PopulateTransportCompanyByBookingPartyOrWarehouse(consolDO ?? sourceDO, bookingParty, bizO, factory, logger);
			}
		}

		static void PopulateTransportCompanyByBookingPartyOrWarehouse<T>(Shipment dataObject, OrgHeader bookingParty, T bizO, UniversalObjectFactory factory, IXmlImportLogger logger) where T : BusinessObject, IDocAddresses
		{
			var canPopulate = dataObject != null;
			if (canPopulate)
			{
				var transportMode = dataObject.TransportMode?.Code.ToString() ?? string.Empty;
				var containerMode = dataObject.ContainerMode?.Code.ToString() ?? string.Empty;

				var warehouseAddress = GetOrgByAddressType<T>(dataObject, nameof(DocAddressType.DepartureCFSAddress), nameof(DocAddressType.ArrivalCFSAddress));
				var warehouseOrgHeader = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, warehouseAddress?.OrganizationCode?.ToString() ?? string.Empty)).FirstOrDefault();
				var warehouseUNLOCO = warehouseOrgHeader?.UNLOCO;

				canPopulate = warehouseUNLOCO != null;

				if (canPopulate)
				{
					var relatedParties = bookingParty?.AllRelatedParties.Where(p => p.PR_PartyType == RelatedPartyTypeList.Codes.LocalTransport);

					if (!PopulateTransportCompanyByRelatedParties(relatedParties, warehouseUNLOCO, transportMode, containerMode, dataObject, logger, factory, bizO))
					{
						relatedParties = warehouseOrgHeader.AllRelatedParties.Where(p => p.PR_PartyType == RelatedPartyTypeList.Codes.LocalTransport);
						PopulateTransportCompanyByRelatedParties(relatedParties, warehouseUNLOCO, transportMode, containerMode, dataObject, logger, factory, bizO);
					}
				}
			}
		}

		static bool PopulateTransportCompanyByRelatedParties<T>(IEnumerable<OrgRelatedParty> relatedParties, RefUNLOCO warehouseUNLOCO, string transportMode, string containerMode, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, T bizO) where T : BusinessObject, IDocAddresses
		{
			var result = relatedParties != null && relatedParties.Any();

			if (result)
			{
				if (dataObject.IsDepartureTransitWarehouse())
				{
					relatedParties = relatedParties.Where(p => p.PR_FreightDirection == RelatedPartyDirectionList.Codes.Pickup || p.PR_FreightDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery);
				}
				else if (dataObject.IsArrivalTransitWarehouse())
				{
					relatedParties = relatedParties.Where(p => p.PR_FreightDirection == RelatedPartyDirectionList.Codes.Delivery || p.PR_FreightDirection == RelatedPartyDirectionList.Codes.PickupAndDelivery);
				}
				else
				{
					result = false;
				}

				if (result)
				{
					relatedParties = relatedParties.Where(p => (p.PR_FreightTransportMode == transportMode || p.PR_FreightTransportMode == TransportModes.All) &&
													(p.PR_FreightTransportMode == TransportModes.Air || p.PR_FreightTransportMode == TransportModes.Road || p.PR_FreightTransportMode == TransportModes.All ||
													(p.PR_FreightTransportMode == TransportModes.Sea && p.PR_FreightContainerMode == containerMode && (p.PR_FreightContainerMode == ContainerModes.FCL || p.PR_FreightContainerMode == ContainerModes.LCL))));
					result = relatedParties.Any();
				}

				if (result)
				{
					var bestMatchedRelatedParty = relatedParties.MaxBy(p => CalculateLocationMatchScore(p, warehouseUNLOCO));
					result = CalculateLocationMatchScore(bestMatchedRelatedParty, warehouseUNLOCO) > 0;
					if (result)
					{
						var transportCompany = bestMatchedRelatedParty.RelatedParty;
						var organizationAddress = new OrganizationAddress();
						organizationAddress.OrganizationCode = transportCompany.OH_Code;

						new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatchedOrNew(bizO, transportCompany.MainAddress, DocAddressType.TransportCompanyDocumentaryAddress);
					}
				}
			}
			return result;
		}

		static int CalculateLocationMatchScore(OrgRelatedParty p, RefUNLOCO warehouseUNLOCO)
		{
			if (p.PR_Location == warehouseUNLOCO.RL_Code)
			{
				return 3;
			}
			else if (p.PR_Location == warehouseUNLOCO.RL_RN_NKCountryCode)
			{
				return 2;
			}
			else if (p.PR_Location.IsEmpty)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		}

		public static void PopulateBillToPartyOrg<T>(Shipment consolDO, T bizO,
			IXmlImportLogger logger, UniversalObjectFactory factory) where T : BusinessObject, IDocAddresses, ITransitJobForRating
		{
			OrganizationAddress billToPartyOrg = null;
			if (consolDO != null)
			{
				billToPartyOrg = GetOrgByAddressType<T>(consolDO, nameof(DocAddressType.SendingForwarderAddress), nameof(DocAddressType.ReceivingForwarderAddress));
			}

			if (billToPartyOrg != null)
			{
				new OrganisationDataObjectReader(billToPartyOrg, logger, factory).GetMatchedOrNew(bizO, DocAddressType.ClientRequestedBillingParty);
			}
		}

		static OrganizationAddress GetOrgByAddressType<T>(Shipment dataObject, string addressTypeInDepartureWarehouse, string addressTypeInArrivalWarehouse) where T : BusinessObject, IDocAddresses
		{
			OrganizationAddress org = null;
			if (dataObject != null)
			{
				org = dataObject.IsDepartureTransitWarehouse()
					? dataObject.OrganizationAddressCollection?.FirstOrDefault(addressTypeInDepartureWarehouse)
					: dataObject.OrganizationAddressCollection?.FirstOrDefault(addressTypeInArrivalWarehouse);
			}

			return org;
		}
	}
}
