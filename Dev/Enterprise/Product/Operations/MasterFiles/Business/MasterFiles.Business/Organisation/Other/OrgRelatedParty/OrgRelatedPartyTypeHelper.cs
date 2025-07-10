using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgRelatedPartyTypeHelper
	{
		public static bool ShouldHaveDirection(ZString partyTypeCode)
		{
			return partyTypeCode != RelatedPartyTypeList.Codes.ShipperBroker
				&& partyTypeCode != RelatedPartyTypeList.Codes.ExportConsolidationDepot
				&& partyTypeCode != RelatedPartyTypeList.Codes.ReturnAgent
				&& partyTypeCode != RelatedPartyTypeList.Codes.CSAApprovedVendor
				&& partyTypeCode != RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee
				&& partyTypeCode != RelatedPartyTypeList.Codes.AccountingVATGSTGroup
				&& partyTypeCode != RelatedPartyTypeList.Codes.Manufacturer
				&& partyTypeCode != RelatedPartyTypeList.Codes.ContainerYard
				&& partyTypeCode != RelatedPartyTypeList.Codes.LocalForwarder
				&& partyTypeCode != RelatedPartyTypeList.Codes.AuthorizedCargoReporter
				&& partyTypeCode != RelatedPartyTypeList.Codes.ProductRelationship
				&& partyTypeCode != RelatedPartyTypeList.Codes.SelfFilerForICS2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool ShouldCalculateDirection(ZString partyTypeCode)
		{
			switch (partyTypeCode)
			{
				case RelatedPartyTypeList.Codes.CustomsAgentBroker:
				case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
				case RelatedPartyTypeList.Codes.LocalTransport:
				case RelatedPartyTypeList.Codes.LocalTransportBillTo:
				case RelatedPartyTypeList.Codes.NationalDistributionCentre:
				case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
				case RelatedPartyTypeList.Codes.ReportRevenueTo:
				case RelatedPartyTypeList.Codes.ReceivingAgent:
				case RelatedPartyTypeList.Codes.SendingAgent:
				case RelatedPartyTypeList.Codes.ControllingCustomer:
				case RelatedPartyTypeList.Codes.ClientCFS:
				case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
				case RelatedPartyTypeList.Codes.ShipperBroker:
				case RelatedPartyTypeList.Codes.ExportConsolidationDepot:
				case RelatedPartyTypeList.Codes.Warehouse:
				case RelatedPartyTypeList.Codes.JapanNotificationParty:
				case RelatedPartyTypeList.Codes.Manufacturer:
				case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
				case RelatedPartyTypeList.Codes.NotifyParty:
					return false;
				default:
					return ShouldHaveDirection(partyTypeCode);
			}
		}

		public static ZString GetDefaultDirection(ZString partyTypeCode)
		{
			if (ShouldCalculateDirection(partyTypeCode))
			{
				if (IsForwarder(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.Forwarder;
				}
				else if (IsAP(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.AP;
				}
				else if (IsAR(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.AR;
				}
				else if (IsSales(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.Sales;
				}
				else if (IsDeliveryAgent(partyTypeCode) || IsDeliveryTo(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.Delivery;
				}
				else if (IsPickupAgent(partyTypeCode) || IsPickupFrom(partyTypeCode))
				{
					return RelatedPartyDirectionList.Codes.Pickup;
				}
			}

			return ZString.Empty;
		}

		static bool IsForwarder(ZString partyTypeCode)
		{
			return GetForwarderPartyTypeCodes().Any(c => c == partyTypeCode);
		}

		public static IEnumerable<string> GetForwarderPartyTypeCodes()
		{
			return new[]
			{
				RelatedPartyTypeList.Codes.ARNettingGroup,
				RelatedPartyTypeList.Codes.APNettingGroup,
				RelatedPartyTypeList.Codes.ForwarderCFS,
				RelatedPartyTypeList.Codes.ForwarderCoLoadWith,
				RelatedPartyTypeList.Codes.ManagementGrouping,
				RelatedPartyTypeList.Codes.ForwarderGroup,
				RelatedPartyTypeList.Codes.CustomsOffice,
				RelatedPartyTypeList.Codes.ForwarderLocalTransport,
				RelatedPartyTypeList.Codes.WarehouseForwarder,
				RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting
			};
		}

		static bool IsAP(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.APSettlementGroup;
		}

		static bool IsAR(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.ARSettlementGroup;
		}

		static bool IsSales(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.SourceOfSalesLead
				|| partyTypeCode == RelatedPartyTypeList.Codes.ControllingAgent;
		}

		static bool IsDeliveryAgent(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.DeliveryAgent;
		}

		static bool IsDeliveryTo(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.DeliveryTo;
		}

		static bool IsPickupAgent(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.PickupAgent;
		}

		static bool IsPickupFrom(ZString partyTypeCode)
		{
			return partyTypeCode == RelatedPartyTypeList.Codes.PickupFrom;
		}
	}
}
