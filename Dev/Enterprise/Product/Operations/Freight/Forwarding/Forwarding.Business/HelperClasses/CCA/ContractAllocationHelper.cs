using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ContractAllocationHelper
	{
		public static RatingContract GetCarrierContract(BusinessObjectFactory factory, ZString contractNumber, ZGuid contractServiceProviderPK)
		{
			if (contractNumber.IsEmpty || contractServiceProviderPK.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, contractNumber);
			query.AddToFilter(RatingContractSchema.RCT_OH, contractServiceProviderPK);
			query.AddToFilter(RatingContractSchema.RCT_ContractType, Constants.RatingContractTypes.Provider);
			query.AddToFilter(RatingContractSchema.RCT_IsActive, true);

			return factory.LoadTop1<RatingContract>(query);
		}

		public static ZDecimal CalculateOutstandingUtilisation(BusinessObjectFactory factory, IRatingContractAllocationLine allocationLine)
		{
			return CalculatePermittedUtilisation(factory, allocationLine) - CalculateUtilisation(factory, allocationLine);
		}

		static ZDecimal CalculatePermittedUtilisation(BusinessObjectFactory factory, IRatingContractAllocationLine allocationLine)
		{
			var quantity = (ZDecimal)allocationLine.RCA_AllocatedQuantity;
			if (FreightConfigurationRegistry.Instance.EnableBlankQuantityForSubAllocations.Value)
			{
				if (!allocationLine.RCA_RCA_ParentAllocationRoute.IsEmpty && quantity == 0)
				{
					quantity = CalculateAllowableQuantity(factory, allocationLine);
				}
			}

			var capacityWithVariance = quantity * (1 + allocationLine.RCA_BookingVariance / 100);
			return Math.Max(quantity, capacityWithVariance);
		}

		static ZDecimal CalculateAllowableQuantity(BusinessObjectFactory factory, IRatingContractAllocationLine allocationLine)
		{
			var parentAllocationRoute = allocationLine.ParentAllocationRoute;
			var childAllocationRoutes = parentAllocationRoute.AllocationDistributions.OfType<IRatingContractAllocationLine>().ToList();

			var allocationRoutesWithBlankQty = new List<IRatingContractAllocationLine>();
			var allocationRoutesWithNonBlankQty = new List<IRatingContractAllocationLine>();
			foreach (var route in childAllocationRoutes)
			{
				if (route.RCA_AllocatedQuantity > 0)
				{
					allocationRoutesWithNonBlankQty.Add(route);
				}
				else if (route.RCA_AllocatedQuantity == 0 && route.PK != allocationLine.PK)
				{
					allocationRoutesWithBlankQty.Add(route);
				}
			}

			var containerQuery = new ZQuery(JobContainerSchema.JC_RCA_AllocationLine, allocationRoutesWithBlankQty.Select(line => line.PK).ToList());
			var allocatedContainers = factory.Load<ForwardingContainer>(containerQuery);

			return parentAllocationRoute.RCA_AllocatedQuantity - allocationRoutesWithNonBlankQty.Sum(line => line.RCA_AllocatedQuantity) - GetTotalContainerQuantity(allocationLine.RCA_AllocatedUQ, allocatedContainers);
		}

		static ZDecimal CalculateUtilisation(BusinessObjectFactory factory, IRatingContractAllocationLine allocationLine)
		{
			var containerQuery = new ZQuery(JobContainerSchema.JC_RCA_AllocationLine, allocationLine.PK);
			var allocatedContainers = factory.Load<ForwardingContainer>(containerQuery);

			return GetTotalContainerQuantity(allocationLine.RCA_AllocatedUQ, allocatedContainers);
		}

		static ZDecimal GetTotalContainerQuantity(ZString unitQuantity, IEnumerable<ForwardingContainer> allocatedContainers)
		{
			return unitQuantity == Constants.AllocationQuantityUnits.TwentyFootUnits
				? allocatedContainers.Sum(container => container.JC_ContainerCount * container.RefContainer?.RC_TEU ?? 0)
				: allocatedContainers.Sum(container => container.JC_ContainerCount);
		}

		public static IReadOnlyCollection<OrgHeader> GetAllocationRouteNamedAccountsWithFallback(IRatingContractAllocationLine allocationLine)
		{
			var namedAccounts = allocationLine.NamedAccountPivots.GetAllNamedAccounts();

			return namedAccounts.Count > 0
				? ToOrgHeaderCollection(namedAccounts)
				: ToOrgHeaderCollection(allocationLine.Contract?.NamedAccountPivots.GetAllNamedAccounts()) ?? Array.Empty<OrgHeader>();
		}

		public static bool AllShipmentsHaveMatchingOrgInNamedAccountCollection(IReadOnlyCollection<ForwardingShipment> shipments, IReadOnlyCollection<OrgHeader> namedAccounts)
		{
			return shipments.All(shipment => DoesShipmentHaveMatchingOrgInNamedAccountCollection(shipment, namedAccounts));
		}

		static bool DoesShipmentHaveMatchingOrgInNamedAccountCollection(ForwardingShipment shipment, IReadOnlyCollection<OrgHeader> namedAccounts)
		{
			return namedAccounts.Count == 0 || namedAccounts.Any(namedAccount => DoesShipmentMatchNamedAccount(shipment, namedAccount));
		}

		static bool DoesShipmentMatchNamedAccount(ForwardingShipment shipment, OrgHeader namedAccount)
			=> shipment.ConsigneePK == namedAccount.PK
			|| shipment.ConsignorPK == namedAccount.PK
			|| shipment.ShipmentJobHeader?.LocalChargesPK == namedAccount.PK
			|| shipment.ControllingCustomer?.PK == namedAccount.PK;

		public static bool IsPortCoveredByLocation(BusinessObjectFactory factory, ZString port, ZString location, bool allowRelatedPorts)
		{
			return location.Length switch
			{
				0 => true,
				2 => IsPortInCountry(port, location),
				4 => IsPortInZone(factory, port, location, allowRelatedPorts),
				5 => PortsMatch(factory, port, location, allowRelatedPorts),
				_ => false,
			};
		}

		static bool IsPortInCountry(ZString port, ZString country) => port.Substring(0, 2).EqualsIgnoringCase(country);

		static bool IsPortInZone(BusinessObjectFactory factory, ZString port, ZString zoneCode, bool allowRelatedPorts)
		{
			var zoneTypes = new string[] { RefZoneHeaderLookups.ZoneTypeCodes.Contract, RefZoneHeaderLookups.ZoneTypeCodes.All };
			var query = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, zoneTypes);
			query.AddToFilter(RefZoneHeaderSchema.FZ_Code, zoneCode);

			var zone = factory.LoadTop1<RefZoneHeader>(query);

			return (zone?.Countries.Cast<RefCountry>().Any(country => IsPortInCountry(port, country.Code)) ?? false)
				|| (zone?.UNLOCOs.Cast<RefUNLOCO>().Any(unloco => PortsMatch(factory, port, unloco.Code, allowRelatedPorts)) ?? false);
		}

		static bool PortsMatch(BusinessObjectFactory factory, ZString port1, ZString port2, bool allowRelatedPorts)
		{
			return port1.EqualsIgnoringCase(port2) || (allowRelatedPorts && ArePortsRelated(factory, port1, port2));
		}

		static bool ArePortsRelated(BusinessObjectFactory factory, ZString port1, ZString port2)
		{
			var relatedPorts = GetRelatedPorts(factory, port1);
			return relatedPorts.Any((relatedPort) => relatedPort.RLR_RL_NKRelatedPort.EqualsIgnoringCase(port2));
		}

		static RefUNLOCORelatedPort[] GetRelatedPorts(BusinessObjectFactory factory, ZString port)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(RefUNLOCORelatedPort), RefUNLOCORelatedPortSchema.RLR_GroupNumber);
			subQuery.AddToFilter(RefUNLOCORelatedPortSchema.RLR_RL_NKRelatedPort, port);

			var query = new ZDBOnlyQuery(typeof(RefUNLOCORelatedPort));
			query.AddSubQuery(RefUNLOCORelatedPortSchema.RLR_GroupNumber, subQuery, JoinCondition.And);

			return factory.Load<RefUNLOCORelatedPort>(query);
		}

		public static bool DoesContainerTypeMatchAllocation(ForwardingContainer container, IRatingContractAllocationLine allocationLine)
		{
			if (container.RefContainer is not RefContainer refContainer)
			{
				return false;
			}

			var doesRefContainerMatch = container.JC_RC == allocationLine.RCA_RC_ContainerType;
			var doesContainerClassMatch = refContainer.RC_StorageClass.EqualsIgnoringCase(allocationLine.RCA_StorageOrFreightRateClass)
				|| refContainer.RC_FreightRateClass.EqualsIgnoringCase(allocationLine.RCA_StorageOrFreightRateClass);

			return (allocationLine.RCA_RC_ContainerType.IsEmpty || doesRefContainerMatch)
				&& (allocationLine.RCA_StorageOrFreightRateClass.IsEmpty || doesContainerClassMatch);
		}

		public static bool ConsolWillHaveLoadDetailsDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			return ConsolLoadWillBeDefaulted(consol, allocationRoute)
				|| SingleLegLoadWillBeDefaulted(consol, allocationRoute);
		}

		public static bool ConsolLoadWillBeDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			var isRouteLoadUNLOCO = allocationRoute.RCA_LoadLocation.Length == 5;
			return isRouteLoadUNLOCO && consol.JK_RL_NKLoadPort.IsEmpty && consol.Transports.Count == 1;
		}

		public static bool SingleLegLoadWillBeDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			var isRouteLoadUNLOCO = allocationRoute.RCA_LoadLocation.Length == 5;
			if (consol.Transports.Count != 1 || !isRouteLoadUNLOCO)
			{
				return false;
			}

			var leg = consol.Transports[0];
			if (!string.Equals(leg.JW_TransportMode, allocationRoute.Contract?.RCT_TransportMode, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return leg.JW_RL_NKLoadPort.IsEmpty;
		}

		public static bool ConsolWillHaveDischargeDetailsDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			return ConsolDischargeWillBeDefaulted(consol, allocationRoute)
				|| SingleLegDischargeWillBeDefaulted(consol, allocationRoute);
		}

		public static bool ConsolDischargeWillBeDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			var isRouteDischargeUNLOCO = allocationRoute.RCA_DischargeLocation.Length == 5;
			return isRouteDischargeUNLOCO && consol.JK_RL_NKDischargePort.IsEmpty && consol.Transports.Count == 1;
		}

		public static bool SingleLegDischargeWillBeDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			var isRouteDischargeUNLOCO = allocationRoute.RCA_DischargeLocation.Length == 5;
			if (consol.Transports.Count != 1 || !isRouteDischargeUNLOCO)
			{
				return false;
			}

			var leg = consol.Transports[0];
			if (!string.Equals(leg.JW_TransportMode, allocationRoute.Contract?.RCT_TransportMode, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return leg.JW_RL_NKDiscPort.IsEmpty;
		}

		public static bool CanSingleLegScheduleDetailsBeDefaulted(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			if (consol.Transports.Count != 1)
			{
				return false;
			}

			var leg = consol.Transports[0];
			if (!string.Equals(leg.JW_TransportMode, allocationRoute.Contract?.RCT_TransportMode, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return (leg.JW_Vessel.IsEmpty || leg.JW_Vessel.EqualsIgnoringCase(allocationRoute.RCA_RV_NKVessel))
				&& (leg.JW_VoyageFlight.IsEmpty || leg.JW_VoyageFlight.EqualsIgnoringCase(allocationRoute.RCA_VoyageNumber))
				&& (leg.JW_ServiceString.IsEmpty || leg.JW_ServiceString.EqualsIgnoringCase(allocationRoute.RCA_ServiceLoop));
		}

		public static IReadOnlyCollection<OrgHeader> ToOrgHeaderCollection(IReadOnlyCollection<IOrgHeader> collection)
		{
			return collection.Cast<OrgHeader>().ToList();
		}
	}
}
