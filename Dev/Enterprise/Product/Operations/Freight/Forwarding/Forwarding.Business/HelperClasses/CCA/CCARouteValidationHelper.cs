using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CCARouteValidationHelper
	{
		#region Common

		public static bool IsRouteAssignmentMissingCarrierContract(ICCACommonAssignmentValidationData validationData)
		{
			return validationData.CarrierContract == null;
		}

		public static bool IsInvalidAllocationRouteAssigned(ZGuid allocationRoutePK, BusinessObjectFactory factory)
		{
			if (allocationRoutePK.IsEmpty)
			{
				return false;
			}

			var loadedRoute = factory.Load<IRatingContractAllocationLine>(allocationRoutePK);
			if (loadedRoute == null)
			{
				return true;
			}

			return false;
		}

		public static bool IsRouteAssignedUnderParentContractAssigned(ZGuid allocationRoutePK, IRatingContract contract)
		{
			if (contract == null)
			{
				return false;
			}

			var contractAllocations = contract.Allocations.OfType<IRatingContractAllocationLine>();
			return contractAllocations.Any(contractAllocation => contractAllocation.PK == allocationRoutePK);
		}

		public static bool IsETDBeforeRouteStartDate(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return IsDateBeforeRouteStartDate(allocationRoute, validationData.ETD);
		}

		public static bool IsETDAfterRouteExpiryDate(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return IsDateAfterRouteExpiryDate(allocationRoute, validationData.ETD);
		}

		static bool IsDateBeforeRouteStartDate(IRatingContractAllocationLine allocationRoute, ZDateTime dateTime)
		{
			return !dateTime.IsEmpty && allocationRoute.StartDateWithContractFallback > dateTime.Date;
		}

		static bool IsDateAfterRouteExpiryDate(IRatingContractAllocationLine allocationRoute, ZDateTime dateTime)
		{
			return !dateTime.IsEmpty && allocationRoute.ExpiryDateWithContractFallback < dateTime.Date;
		}

		public static bool IsConsolValidForRouteLoadLocation(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (PortMatchesRouteLoad(consol.JK_RL_NKLoadPort, allocationRoute))
			{
				return true;
			}

			foreach (var legList in GetRelatedLegLists(consol))
			{
				var foundLegWithValidMode = false;
				var foundLegWithValidLoad = false;
				foreach (var leg in legList)
				{
					foundLegWithValidMode = foundLegWithValidMode || TransportLegIsValidForRouteMode(allocationRoute, leg);
					foundLegWithValidLoad = foundLegWithValidLoad || PortMatchesRouteLoad(leg.JW_RL_NKLoadPort, allocationRoute);
					if (foundLegWithValidMode && foundLegWithValidLoad)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsConsolValidForRouteDischargeLocation(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (PortMatchesRouteDischarge(consol.JK_RL_NKDischargePort, allocationRoute))
			{
				return true;
			}

			foreach (var legList in GetRelatedLegLists(consol))
			{
				var foundLegWithValidMode = false;
				var foundLegWithValidDischarge = false;
				foreach (var leg in legList)
				{
					foundLegWithValidMode = foundLegWithValidMode || TransportLegIsValidForRouteMode(allocationRoute, leg);
					foundLegWithValidDischarge = foundLegWithValidDischarge || PortMatchesRouteDischarge(leg.JW_RL_NKDiscPort, allocationRoute);
					if (foundLegWithValidMode && foundLegWithValidDischarge)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsConsolValidForRouteValidDateRanges(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol, bool isAttaching = false)
		{
			var modeValidLegLists = GetRelatedLegLists(consol).Where(list => list.Any(leg => TransportLegIsValidForRouteMode(allocationRoute, leg)));
			var dateAndModeValidLegLists = modeValidLegLists.Where(list => list.Any(leg => TransportLegIsValidForRouteDates(allocationRoute, leg)));
			if (!dateAndModeValidLegLists.Any())
			{
				return false;
			}

			var validLoadLegLists = dateAndModeValidLegLists
				.Where(list => list.Any(leg => PortMatchesRouteLoad(leg.JW_RL_NKLoadPort, allocationRoute)));

			var willConsolLoadBeSet = isAttaching && ContractAllocationHelper.ConsolLoadWillBeDefaulted(consol, allocationRoute);
			var consolMatchesRouteLoad = PortMatchesRouteLoad(consol.JK_RL_NKLoadPort, allocationRoute);
			var needValidLoadList = !willConsolLoadBeSet && !consolMatchesRouteLoad;
			if (needValidLoadList && !validLoadLegLists.Any())
			{
				return false;
			}

			var validDischargeLegLists = modeValidLegLists
				.Where(list => list.Any(leg => PortMatchesRouteDischarge(leg.JW_RL_NKDiscPort, allocationRoute)));

			var willConsolDischargeBeSet = isAttaching && ContractAllocationHelper.ConsolDischargeWillBeDefaulted(consol, allocationRoute);
			var consolMatchesRouteDischarge = PortMatchesRouteDischarge(consol.JK_RL_NKDischargePort, allocationRoute);
			var needValidDischargeList = !willConsolDischargeBeSet && !consolMatchesRouteDischarge;

			// if we do not need to find a valid discharge route set, then validation has passed
			if (!needValidDischargeList)
			{
				return true;
			}

			// if we do need to find a valid discharge route set, and there are none, then validation has failed
			if (!validDischargeLegLists.Any())
			{
				return false;
			}

			// find the latest departing discharge leg
			var latestDischargeLeg = validDischargeLegLists
				.SelectMany(list => list)
				.Where(leg => PortMatchesRouteDischarge(leg.JW_RL_NKDiscPort, allocationRoute))
				.MaxBy(leg => leg.JW_ETD);

			// if we did not need to find a valid load route set, then we just need to check if any
			// route set departs on-or-before the latest departing discharge leg
			if (!needValidLoadList)
			{
				return dateAndModeValidLegLists.Any(list => list[0].JW_ETD <= latestDischargeLeg.JW_ETD);
			}

			// if we did need to find a valid load route set, then we need to check if any of the load
			// route sets validly depart on-or-before the latest discharge route set
			return AnyLoadListsDepartOnOrBeforeLatestDischarge(validLoadLegLists, latestDischargeLeg, allocationRoute);
		}

		static IEnumerable<IReadOnlyList<Transport>> GetRelatedLegLists(ForwardingConsol consol)
		{
			var routeSets = ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets;
			return routeSets.Count == 0
				? consol.Transports.Select(transport => new List<Transport>() { transport })
				: routeSets.Select(set => set.Legs);
		}

		static bool AnyLoadListsDepartOnOrBeforeLatestDischarge(
			IEnumerable<IReadOnlyList<Transport>> validLoadLegLists,
			Transport latestDischargeLeg,
			IRatingContractAllocationLine allocationRoute)
		{
			foreach (var list in validLoadLegLists)
			{
				// if this route set contains the latest discharge leg
				if (list.Contains(latestDischargeLeg))
				{
					// then the load leg must depart on-or-before the discharge leg
					var earliestLoadLeg = list.First(leg => PortMatchesRouteLoad(leg.JW_RL_NKLoadPort, allocationRoute));
					if (earliestLoadLeg.JW_ETD <= latestDischargeLeg.JW_ETD)
					{
						return true;
					}
				}
				// otherwise we just need to check that the earliest leg in the route set
				// departs on-or-before the latest discharge leg
				else if (list[0].JW_ETD <= latestDischargeLeg.JW_ETD)
				{
					return true;
				}
			}

			return false;
		}

		static bool PortMatchesRouteLoad(string port, IRatingContractAllocationLine route)
			=> ContractAllocationHelper.IsPortCoveredByLocation(route.Factory, port, route.RCA_LoadLocation, route.RCA_AllowRelatedPorts);

		static bool PortMatchesRouteDischarge(string port, IRatingContractAllocationLine route)
			=> ContractAllocationHelper.IsPortCoveredByLocation(route.Factory, port, route.RCA_DischargeLocation, route.RCA_AllowRelatedPorts);

		public static bool IsConsolValidForRouteScheduleDetails(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (allocationRoute.RCA_VoyageNumber.IsEmpty &&
				allocationRoute.RCA_RV_NKVessel.IsEmpty &&
				allocationRoute.RCA_ServiceLoop.IsEmpty)
			{
				return true;
			}

			var loadDischargeMatchingLegsOfConsol = consol.Transports
				.OfType<Transport>()
				.Where(leg =>
					TransportLegIsValidForRouteMode(allocationRoute, leg) &&
					(TransportLegIsValidForRouteDates(allocationRoute, leg) ||
					!BookingEmptyAndMBLEmptyOrAllocationChanged(consol)) &&
					(ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, leg.JW_RL_NKLoadPort, allocationRoute.RCA_LoadLocation, allocationRoute.RCA_AllowRelatedPorts) ||
						ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, leg.JW_RL_NKDiscPort, allocationRoute.RCA_DischargeLocation, allocationRoute.RCA_AllowRelatedPorts)));

			return loadDischargeMatchingLegsOfConsol.Any(leg =>
				(allocationRoute.RCA_VoyageNumber.IsEmpty || string.Equals(allocationRoute.RCA_VoyageNumber, leg.JW_VoyageFlight, StringComparison.OrdinalIgnoreCase)) &&
				(allocationRoute.RCA_RV_NKVessel.IsEmpty || string.Equals(allocationRoute.RCA_RV_NKVessel, leg.JW_Vessel, StringComparison.OrdinalIgnoreCase)) &&
				(allocationRoute.RCA_ServiceLoop.IsEmpty || string.Equals(allocationRoute.RCA_ServiceLoop, leg.JW_ServiceString, StringComparison.OrdinalIgnoreCase)));
		}

		public static bool BookingEmptyAndMBLEmptyOrAllocationChanged(ForwardingConsol consol)
		{
			return !consol.IsInDatabase
				|| consol.JK_RCA_AllocationLineInfo.HasChanges
				|| consol.JK_CarrierContractNumberInfo.HasChanges
				|| consol.JK_OA_ShippingLineAddressInfo.HasChanges
				|| (consol.JK_BookingReference.IsEmpty && consol.JK_MasterBillNum.IsEmpty);
		}

		static bool TransportLegIsValidForRouteMode(IRatingContractAllocationLine allocationRoute, Transport transportLeg)
		{
			return string.Equals(allocationRoute.Contract?.RCT_TransportMode, transportLeg.JW_TransportMode, StringComparison.OrdinalIgnoreCase);
		}

		public static bool TransportLegIsValidForRouteDates(IRatingContractAllocationLine allocationRoute, Transport transportLeg)
		{
			if (transportLeg == null)
			{
				return false;
			}

			if (transportLeg.JW_ETD.IsEmpty)
			{
				return true;
			}

			return allocationRoute.StartDateWithContractFallback <= transportLeg.JW_ETD.Date
				&& (allocationRoute.ExpiryDateWithContractFallback.IsEmpty || allocationRoute.ExpiryDateWithContractFallback >= transportLeg.JW_ETD.Date);
		}

		public static bool IsLoadPortNotCoveredByRouteLoadLocation(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return !allocationRoute.RCA_LoadLocation.IsEmpty
				&& !validationData.LoadPort.IsEmpty
				&& !ContractAllocationHelper.IsPortCoveredByLocation(validationData.Factory, validationData.LoadPort, allocationRoute.RCA_LoadLocation, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool IsDischargePortNotCoveredByRouteDischargeLocation(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return !allocationRoute.RCA_DischargeLocation.IsEmpty
				&& !validationData.DischargePort.IsEmpty
				&& !ContractAllocationHelper.IsPortCoveredByLocation(validationData.Factory, validationData.DischargePort, allocationRoute.RCA_DischargeLocation, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool IsVoyageNumberMismatch(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return !allocationRoute.RCA_VoyageNumber.IsEmpty
				&& !validationData.VoyageFlight.IsEmpty
				&& !string.Equals(validationData.VoyageFlight, allocationRoute.RCA_VoyageNumber, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsVesselMismatch(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return !allocationRoute.RCA_RV_NKVessel.IsEmpty
				&& !validationData.Vessel.IsEmpty
				&& !string.Equals(validationData.Vessel, allocationRoute.RCA_RV_NKVessel, StringComparison.OrdinalIgnoreCase);
		}

		public static bool JobHasContainersNotMatchingAllocationContainerType(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			return validationData.Containers.OfType<ForwardingContainer>()
				.Any(container => !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute));
		}

		#endregion

		#region ForwardingContainer

		public static bool IsContainerTypeInvalidForContract(IRatingContract contract, ForwardingContainer container)
		{
			var contractContainerType = contract?.RCT_ContainerType ?? ZString.Empty;
			if (contractContainerType.IsEmpty)
			{
				return false;
			}

			return !string.Equals(contractContainerType, container.RefContainer?.RC_ContainerType ?? ZString.Empty, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsValidContainerOwner(IRatingContractAllocationLine allocationRoute, ForwardingContainer container)
		{
			if (allocationRoute is null || container is null)
			{
				return true;
			}

			return (allocationRoute.RCA_ContainerOwner == Core.Constants.ContainerOwnership.Codes.ShipperOwned && container.JC_IsShipperOwned)
				|| (allocationRoute.RCA_ContainerOwner == Core.Constants.ContainerOwnership.Codes.CarrierOwned && !container.JC_IsShipperOwned);
		}

		#endregion

		#region FowardingConsol

		public static bool IsConsolFirstLoadValidForPlaceOfReceipt(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, allocationRoute.RCA_PlaceOfReceipt, consol.JK_RL_NKLoadPort, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool IsConsolDischargeValidForPlaceOfDelivery(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, allocationRoute.RCA_PlaceOfDelivery, consol.JK_RL_NKDischargePort, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool IsBookingLordPortValidForPlaceOfReceipt(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			if (booking.LoadPort.IsEmpty)
			{
				return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, booking.Origin, allocationRoute.RCA_PlaceOfReceipt, allocationRoute.RCA_AllowRelatedPorts);
			}

			return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, booking.LoadPort, allocationRoute.RCA_PlaceOfReceipt, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool IsBookingDischargePortValidForPlaceOfDelievery(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			if (booking.DischargePort.IsEmpty)
			{
				return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, booking.Destination, allocationRoute.RCA_PlaceOfDelivery, allocationRoute.RCA_AllowRelatedPorts);
			}

			return ContractAllocationHelper.IsPortCoveredByLocation(allocationRoute.Factory, booking.DischargePort, allocationRoute.RCA_PlaceOfDelivery, allocationRoute.RCA_AllowRelatedPorts);
		}

		public static bool HasNoMatchingServiceStringTransportLeg(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (allocationRoute?.RCA_ServiceLoop.IsEmpty ?? true)
			{
				return false;
			}

			return !consol
				.Transports
				.OfType<Transport>()
				.Any(transport => string.Equals(allocationRoute.RCA_ServiceLoop, transport.JW_ServiceString, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsConsolInvalidForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			var shipments = consol.Shipments.OfType<ForwardingShipment>().ToArray();
			var namedAccounts = ContractAllocationHelper.GetAllocationRouteNamedAccountsWithFallback(allocationRoute);
			if (ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts))
			{
				return false;
			}

			return true;
		}

		public static bool IsGatewayAgentAssigned(ForwardingConsol consol)
		{
			var gatewayAgentTypes = new List<string>()
				{
					AgentStatusList.Codes.GatewayAgent,
					AgentStatusList.Codes.GatewayAgentWithTariff,
				};

			return gatewayAgentTypes.Contains(consol.JK_SendingForwarderHandlingType) || gatewayAgentTypes.Contains(consol.JK_ReceivingForwarderHandlingType);
		}

		public static bool IsValidForwarderForAllocationRouteAgents(ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			var agents = allocationRoute.AgentPivots.GetAllAgents().Select(agent => agent.PK);
			if (agents.IsNullOrEmpty())
			{
				return true;
			}

			var forwarders = new List<ZGuid>
			{
				consol.SendingForwarderPK,
				consol.ReceivingForwarderPK
			};

			if (agents.Any(forwarders.Contains))
			{
				return true;
			}

			var orgProxies = GetAllCompanyOrgProxiesFromBranchOrgProxy(consol.Factory, forwarders);

			return orgProxies.Any(orgHeader => agents.Contains(orgHeader.GC_OH_OrgProxy));
		}

		static IReadOnlyCollection<GlbCompany> GetAllCompanyOrgProxiesFromBranchOrgProxy(BusinessObjectFactory factory, List<ZGuid> forwarders)
		{
			var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
			branchQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, forwarders);
			companyQuery.AddSubQuery(branchQuery, JoinCondition.And);

			var orgProxies = factory.Load<GlbCompany>(companyQuery);

			return orgProxies.Distinct().ToList();
		}

		#endregion

		public static bool IsBookingInvalidForAllocationRouteNamedAccounts(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			var namedAccounts = allocationRoute.NamedAccountPivots.GetAllNamedAccounts();

			return !CCABookingValidationHelper.AnyNamedAccountMatchesBooking(booking, ContractAllocationHelper.ToOrgHeaderCollection(namedAccounts));
		}

		public static bool DoAnyConsolTransportLegsMatchAllocationRouteSchedule(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			return consol
				.Transports.OfType<Transport>()
				.Any(transport => DoesTransportLegMatchAllocationRouteDetails(allocationRoute, transport));
		}

		static bool DoesTransportLegMatchAllocationRouteDetails(IRatingContractAllocationLine allocationRoute, Transport transportLeg)
		{
			if (transportLeg.JW_IsLinked)
			{
				return transportLeg.JW_JX == allocationRoute.RCA_JX_SailingSchedule;
			}

			var schedule = allocationRoute.JobSailing as JobSailing;
			var origin = schedule.Origin;
			var destination = schedule.Destination;
			var voyage = schedule.Voyage;

			return transportLeg.JW_ETD == origin.JA_E_DEP
				&& transportLeg.JW_RL_NKLoadPort == origin.JA_RL_NKPortOfLoading
				&& transportLeg.JW_RL_NKDiscPort == destination.JB_RL_NKPortOfDischarge
				&& transportLeg.JW_VoyageFlight == voyage.JV_VoyageFlight
				&& transportLeg.JW_Vessel == voyage.JV_RV_NKVessel
				&& transportLeg.JW_ServiceString == schedule.JX_ServiceString
				&& transportLeg.CarrierPK == voyage.JV_OH_Line;
		}
	}
}
