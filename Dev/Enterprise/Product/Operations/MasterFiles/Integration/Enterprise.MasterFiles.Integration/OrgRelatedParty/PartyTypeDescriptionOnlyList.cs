using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public class PartyTypeDescriptionOnlyList : CodeDescriptionPairList
	{
		/// <summary>
		/// Shows description only in the drop down edit box
		/// </summary>
		public PartyTypeDescriptionOnlyList()
		{
			AddPair(RelatedPartyTypeList.Codes.AccountingVATGSTGroup, RelatedPartyTypeList.Descriptions.AccountingVATGSTGroup);
			AddPair(RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyTypeList.Descriptions.APNettingGroup);
			AddPair(RelatedPartyTypeList.Codes.APSettlementGroup, RelatedPartyTypeList.Descriptions.APSettlementGroup);
			AddPair(RelatedPartyTypeList.Codes.ARNettingGroup, RelatedPartyTypeList.Descriptions.ARNettingGroup);
			AddPair(RelatedPartyTypeList.Codes.ARSettlementGroup, RelatedPartyTypeList.Descriptions.ARSettlementGroup);
			AddPair(RelatedPartyTypeList.Codes.AuthorizedCargoReporter, RelatedPartyTypeList.Descriptions.AuthorizedCargoReporter);
			AddPair(RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyTypeList.Descriptions.ClientCFS);
			AddPair(RelatedPartyTypeList.Codes.ContainerYard, RelatedPartyTypeList.Descriptions.ContainerYard);
			AddPair(RelatedPartyTypeList.Codes.ControllingAgent, RelatedPartyTypeList.Descriptions.ControllingAgent);
			AddPair(RelatedPartyTypeList.Codes.ControllingCustomer, RelatedPartyTypeList.Descriptions.ControllingCustomer);
			AddPair(RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee, RelatedPartyTypeList.Descriptions.CSAApprovedUltimateConsignee);
			AddPair(RelatedPartyTypeList.Codes.CSAApprovedVendor, RelatedPartyTypeList.Descriptions.CSAApprovedVendor);
			AddPair(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyTypeList.Descriptions.CustomsAgentBroker);
			AddPair(RelatedPartyTypeList.Codes.CustomsOffice, RelatedPartyTypeList.Descriptions.CustomsOffice);
			AddPair(RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting, RelatedPartyTypeList.Descriptions.CustomsPostponedVatAccounting);
			AddPair(RelatedPartyTypeList.Codes.DeliveryAgent, RelatedPartyTypeList.Descriptions.DeliveryAgent);
			AddPair(RelatedPartyTypeList.Codes.DeliveryTo, RelatedPartyTypeList.Descriptions.DeliveryTo);
			AddPair(RelatedPartyTypeList.Codes.ExportConsolidationDepot, RelatedPartyTypeList.Descriptions.ExportConsolidationDepot);
			AddPair(RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyTypeList.Descriptions.ForwarderCFS);
			AddPair(RelatedPartyTypeList.Codes.ForwarderCoLoadWith, RelatedPartyTypeList.Descriptions.ForwarderCoLoadWith);
			AddPair(RelatedPartyTypeList.Codes.ForwarderGroup, RelatedPartyTypeList.Descriptions.ForwarderGroup);
			AddPair(RelatedPartyTypeList.Codes.ForwarderLocalTransport, RelatedPartyTypeList.Descriptions.ForwarderLocalTransport);
			AddPair(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyTypeList.Descriptions.InvoiceCustomsJobsTo);
			AddPair(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyTypeList.Descriptions.InvoiceFreightJobsTo);
			AddPair(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo, RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo);
			AddPair(RelatedPartyTypeList.Codes.JapanNotificationParty, RelatedPartyTypeList.Descriptions.JapanNotificationParty);
			AddPair(RelatedPartyTypeList.Codes.LocalForwarder, RelatedPartyTypeList.Descriptions.LocalForwarder);
			AddPair(RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyTypeList.Descriptions.LocalTransport);
			AddPair(RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyTypeList.Descriptions.LocalTransportBillTo);
			AddPair(RelatedPartyTypeList.Codes.ManagementGrouping, RelatedPartyTypeList.Descriptions.ManagementGrouping);
			AddPair(RelatedPartyTypeList.Codes.Manufacturer, RelatedPartyTypeList.Descriptions.Manufacturer);
			AddPair(RelatedPartyTypeList.Codes.NationalDistributionCentre, RelatedPartyTypeList.Descriptions.NationalDistributionCentre);
			AddPair(RelatedPartyTypeList.Codes.NotifyParty, RelatedPartyTypeList.Descriptions.NotifyParty);
			AddPair(RelatedPartyTypeList.Codes.PickupAgent, RelatedPartyTypeList.Descriptions.PickupAgent);
			AddPair(RelatedPartyTypeList.Codes.PickupFrom, RelatedPartyTypeList.Descriptions.PickupFrom);
			AddPair(RelatedPartyTypeList.Codes.ProductRelationship, RelatedPartyTypeList.Descriptions.ProductRelationship);
			AddPair(RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyTypeList.Descriptions.ReceivingAgent);
			AddPair(RelatedPartyTypeList.Codes.ReportRevenueTo, RelatedPartyTypeList.Descriptions.ReportRevenueTo);
			AddPair(RelatedPartyTypeList.Codes.ReturnAgent, RelatedPartyTypeList.Descriptions.ReturnAgent);
			AddPair(RelatedPartyTypeList.Codes.SelfFilerForICS2, RelatedPartyTypeList.Descriptions.SelfFilerForICS2);
			AddPair(RelatedPartyTypeList.Codes.SendingAgent, RelatedPartyTypeList.Descriptions.SendingAgent);
			AddPair(RelatedPartyTypeList.Codes.ServiceProvider, RelatedPartyTypeList.Descriptions.ServiceProvider);
			AddPair(RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyTypeList.Descriptions.ServiceProviderCreditor);
			AddPair(RelatedPartyTypeList.Codes.ShipperBroker, RelatedPartyTypeList.Descriptions.ShipperBroker);
			AddPair(RelatedPartyTypeList.Codes.SourceOfSalesLead, RelatedPartyTypeList.Descriptions.SourceOfSalesLead);
			AddPair(RelatedPartyTypeList.Codes.Warehouse, RelatedPartyTypeList.Descriptions.Warehouse);
			AddPair(RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyTypeList.Descriptions.WarehouseForwarder);
		}
	}
}
