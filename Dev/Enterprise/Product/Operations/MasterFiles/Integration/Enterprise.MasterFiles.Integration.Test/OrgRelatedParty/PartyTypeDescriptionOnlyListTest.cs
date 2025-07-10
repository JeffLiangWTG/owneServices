using System.Linq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Testing
{
	public class PartyTypeDescriptionOnlyListTest : TestCase
	{
		public void TestAllDescriptionsFromPrimaryListArePresentInThisListAndTheListIsInAlphabeticalOrder()
		{
			var primaryList = new RelatedPartyTypeList()
				.ToArray()
				.Select(o => o.Description)
				.OrderBy(o => o);

			var thisList = new PartyTypeDescriptionOnlyList()
				.ToArray()
				.Select(o => o.Description);

			AssertMultilineASCIIEquals("Contents of PartyTypeDescriptionOnlyList should have everything in the RelatedPartyTypeList"
				, string.Join("\r\n", primaryList)
				, string.Join("\r\n", thisList));
		}

		public void TestPartyTypeDescriptionOnlyList()
		{
			var list = new PartyTypeDescriptionOnlyList();
			AssertEquals(RelatedPartyTypeList.Descriptions.AccountingVATGSTGroup, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.AccountingVATGSTGroup));
			AssertEquals(RelatedPartyTypeList.Descriptions.APNettingGroup, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.APNettingGroup));
			AssertEquals(RelatedPartyTypeList.Descriptions.APSettlementGroup, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.APSettlementGroup));
			AssertEquals(RelatedPartyTypeList.Descriptions.ARNettingGroup, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ARNettingGroup));
			AssertEquals(RelatedPartyTypeList.Descriptions.ARSettlementGroup, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ARSettlementGroup));
			AssertEquals(RelatedPartyTypeList.Descriptions.JapanNotificationParty, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.JapanNotificationParty));
			AssertEquals(RelatedPartyTypeList.Descriptions.LocalTransport, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.LocalTransport));
			AssertEquals(RelatedPartyTypeList.Descriptions.LocalTransportBillTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.LocalTransportBillTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.ControllingAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ControllingAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.ControllingCustomer, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ControllingCustomer));
			AssertEquals(RelatedPartyTypeList.Descriptions.CustomsAgentBroker, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.CustomsAgentBroker));
			AssertEquals(RelatedPartyTypeList.Descriptions.CSAApprovedUltimateConsignee, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee));
			AssertEquals(RelatedPartyTypeList.Descriptions.CSAApprovedVendor, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.CSAApprovedVendor));
			AssertEquals(RelatedPartyTypeList.Descriptions.DeliveryAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.DeliveryAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.DeliveryTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.DeliveryTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.ForwarderCFS, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ForwarderCFS));
			AssertEquals(RelatedPartyTypeList.Descriptions.InvoiceCustomsJobsTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.InvoiceFreightJobsTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.InvoiceWarehouseJobsTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.ManagementGrouping, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals(RelatedPartyTypeList.Descriptions.Manufacturer, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.Manufacturer));
			AssertEquals(RelatedPartyTypeList.Descriptions.ReportRevenueTo, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ReportRevenueTo));
			AssertEquals(RelatedPartyTypeList.Descriptions.ReturnAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ReturnAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.ReceivingAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ReceivingAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.SendingAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.SendingAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.ServiceProvider, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ServiceProvider));
			AssertEquals(RelatedPartyTypeList.Descriptions.SourceOfSalesLead, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.SourceOfSalesLead));
			AssertEquals(RelatedPartyTypeList.Descriptions.ClientCFS, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ClientCFS));
			AssertEquals(RelatedPartyTypeList.Descriptions.ForwarderLocalTransport, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ForwarderLocalTransport));
			AssertEquals(RelatedPartyTypeList.Descriptions.WarehouseForwarder, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.WarehouseForwarder));
			AssertEquals(RelatedPartyTypeList.Descriptions.PickupAgent, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.PickupAgent));
			AssertEquals(RelatedPartyTypeList.Descriptions.PickupFrom, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.PickupFrom));
			AssertEquals(RelatedPartyTypeList.Descriptions.Warehouse, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.Warehouse));
			AssertEquals(RelatedPartyTypeList.Descriptions.NationalDistributionCentre, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.NationalDistributionCentre));
			AssertEquals(RelatedPartyTypeList.Descriptions.NotifyParty, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.NotifyParty));
			AssertEquals(RelatedPartyTypeList.Descriptions.CustomsPostponedVatAccounting, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting));
			AssertEquals(RelatedPartyTypeList.Descriptions.ServiceProviderCreditor, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ServiceProviderCreditor));
			AssertEquals(RelatedPartyTypeList.Descriptions.ForwarderCoLoadWith, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ForwarderCoLoadWith));
			AssertEquals(RelatedPartyTypeList.Descriptions.ProductRelationship, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.ProductRelationship));
			AssertEquals(RelatedPartyTypeList.Descriptions.SelfFilerForICS2, list.GetDescriptionFromCode(RelatedPartyTypeList.Codes.SelfFilerForICS2));
		}
	}
}
