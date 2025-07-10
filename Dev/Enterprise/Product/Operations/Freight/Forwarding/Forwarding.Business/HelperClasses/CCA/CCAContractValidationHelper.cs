using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CCAContractValidationHelper
	{
		#region Common

		public static bool DoesJobTransportModeMatchContract(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			return string.Equals(validationData.TransportMode, contract.RCT_TransportMode, StringComparison.OrdinalIgnoreCase);
		}

		public static bool DoesContractNumberExistWithAnyCarrier(ZString contractNumber, BusinessObjectFactory factory)
		{
			var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, contractNumber);
			query.AddToFilter(RatingContractSchema.RCT_ContractType, Constants.RatingContractTypes.Provider);
			query.AddToFilter(RatingContractSchema.RCT_IsActive, true);

			return factory.Load<IRatingContract>(query).Length > 0;
		}

		public static bool DoesContractNumberExistWithCarrier(ZString contractNumber, OrgHeader carrier, BusinessObjectFactory factory)
		{
			if (carrier == null)
			{
				return false;
			}

			var query = new ZQuery(RatingContractSchema.RCT_ContractNumber, contractNumber);
			query.AddToFilter(RatingContractSchema.RCT_ContractType, Constants.RatingContractTypes.Provider);
			query.AddToFilter(RatingContractSchema.RCT_OH, carrier.PK);
			query.AddToFilter(RatingContractSchema.RCT_IsActive, true);

			return factory.Load<IRatingContract>(query).FirstOrDefault() != null;
		}

		public static bool IsETDBeforeContractStart(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			return (!validationData.ETD.IsEmpty && contract.RCT_StartDate > validationData.ETD.Date);
		}

		public static bool IsETDAfterContractExpiry(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			return (!validationData.ETD.IsEmpty && contract.RCT_EndDate < validationData.ETD.Date);
		}

		public static bool AreAnyContainerTypesInvalidForContract(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			var containers = validationData.Containers.OfType<ForwardingContainer>();
			return !contract.RCT_ContainerType.IsEmpty
				&& containers.Any(container => !string.Equals(container.RefContainer?.RC_ContainerType, contract.RCT_ContainerType, StringComparison.OrdinalIgnoreCase));
		}

		public static bool AreAnyContainerCommoditiesInvalidForNonHazardousContract(IRatingContract contract, ICCACommonAssignmentValidationData validationData)
		{
			if (contract.RCT_AllowHazardousCommodities)
			{
				return false;
			}

			var containers = validationData.Containers.OfType<ForwardingContainer>();
			return containers.FirstOrDefault(container => container.ContainerCommodityCode?.RH_IsHazardous ?? false) != null;
		}

		#endregion

		#region Consol

		public static bool IsConsolValidForContractValidDateRanges(IRatingContract contract, ForwardingConsol consol)
		{
			foreach (var legList in GetRelatedLegLists(consol))
			{
				var foundLegWithValidMode = false;
				var foundLegWithValidDates = false;
				foreach (var leg in legList)
				{
					foundLegWithValidMode = foundLegWithValidMode || TransportLegIsValidForContractMode(contract, leg);
					foundLegWithValidDates = foundLegWithValidDates || TransportLegIsValidForContractDates(contract, leg);
					if (foundLegWithValidMode && foundLegWithValidDates)
					{
						return true;
					}
				}
			}

			return false;
		}

		static IEnumerable<IEnumerable<Transport>> GetRelatedLegLists(ForwardingConsol consol)
		{
			var routeSets = ((IRoutingSupport)consol).TransportsIncludingRelated.RouteSets;
			return routeSets.Count == 0
				? consol.Transports.Select(transport => new List<Transport>() { transport })
				: routeSets.Select(set => set.Legs);
		}

		static bool TransportLegIsValidForContractMode(IRatingContract contract, Transport transportLeg)
		{
			return string.Equals(contract.RCT_TransportMode, transportLeg.JW_TransportMode, StringComparison.OrdinalIgnoreCase);
		}

		static bool TransportLegIsValidForContractDates(IRatingContract contract, Transport transportLeg)
		{
			if (transportLeg.JW_ETD.IsEmpty)
			{
				return true;
			}

			return contract.RCT_StartDate <= transportLeg.JW_ETD.Date
				&& (contract.RCT_EndDate.IsEmpty || contract.RCT_EndDate >= transportLeg.JW_ETD.Date);
		}

		public static bool IsConsolInvalidForContractNamedAccounts(IRatingContract contract, ForwardingConsol consol)
		{
			var shipments = consol.Shipments.OfType<ForwardingShipment>().ToArray();
			var namedAccounts = contract.NamedAccountPivots.GetAllNamedAccounts();

			return !ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, ContractAllocationHelper.ToOrgHeaderCollection(namedAccounts));
		}

		public static bool IsConsolInvalidForNonHazardousContract(IRatingContract contract, ForwardingConsol consol)
		{
			return !contract.RCT_AllowHazardousCommodities
				&& consol.JK_IsHazardous;
		}

		#endregion

		#region QuotedBooking

		public static bool IsBookingValidForContractNamedAccounts(IRatingContract contract, IQuotedBooking booking)
		{
			var namedAccounts = contract.NamedAccountPivots.GetAllNamedAccounts();
			return contract == null || CCABookingValidationHelper.AnyNamedAccountMatchesBooking(booking, ContractAllocationHelper.ToOrgHeaderCollection(namedAccounts));
		}

		#endregion
	}
}
