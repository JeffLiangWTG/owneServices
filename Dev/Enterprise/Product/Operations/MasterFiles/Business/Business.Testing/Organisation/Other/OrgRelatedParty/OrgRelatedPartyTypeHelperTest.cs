using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRelatedPartyTypeHelperTest : TestCase
	{
		public void TestShouldCalculateDirection()
		{
			foreach (ICodeDescription item in new RelatedPartyTypeList())
			{
				switch (item.Code)
				{
					case RelatedPartyTypeList.Codes.CustomsAgentBroker:
					case RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo:
					case RelatedPartyTypeList.Codes.LocalTransport:
					case RelatedPartyTypeList.Codes.LocalTransportBillTo:
					case RelatedPartyTypeList.Codes.InvoiceFreightJobsTo:
					case RelatedPartyTypeList.Codes.ReportRevenueTo:
					case RelatedPartyTypeList.Codes.ReceivingAgent:
					case RelatedPartyTypeList.Codes.SendingAgent:
					case RelatedPartyTypeList.Codes.ControllingCustomer:
					case RelatedPartyTypeList.Codes.ClientCFS:
					case RelatedPartyTypeList.Codes.ForwarderCoLoadWith:
					case RelatedPartyTypeList.Codes.ExportConsolidationDepot:
					case RelatedPartyTypeList.Codes.ShipperBroker:
					case RelatedPartyTypeList.Codes.Warehouse:
					case RelatedPartyTypeList.Codes.NationalDistributionCentre:
					case RelatedPartyTypeList.Codes.ReturnAgent:
					case RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee:
					case RelatedPartyTypeList.Codes.CSAApprovedVendor:
					case RelatedPartyTypeList.Codes.AccountingVATGSTGroup:
					case RelatedPartyTypeList.Codes.JapanNotificationParty:
					case RelatedPartyTypeList.Codes.Manufacturer:
					case RelatedPartyTypeList.Codes.ContainerYard:
					case RelatedPartyTypeList.Codes.LocalForwarder:
					case RelatedPartyTypeList.Codes.ServiceProviderCreditor:
					case RelatedPartyTypeList.Codes.NotifyParty:
					case RelatedPartyTypeList.Codes.AuthorizedCargoReporter:
					case RelatedPartyTypeList.Codes.ProductRelationship:
					case RelatedPartyTypeList.Codes.SelfFilerForICS2:
						AssertEquals($"{item.Code} - {item.Description} should NOT calculate Direction", false, OrgRelatedPartyTypeHelper.ShouldCalculateDirection(item.Code));
						break;
					default:
						AssertEquals($"{item.Code} - {item.Description} should calculate Direction", true, OrgRelatedPartyTypeHelper.ShouldCalculateDirection(item.Code));
						break;
				}
			}
		}

		public void TestGetDefaultDirection()
		{
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ARNettingGroup));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.APNettingGroup));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ForwarderCFS));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ForwarderGroup));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.CustomsOffice));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ForwarderLocalTransport));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.WarehouseForwarder));
			AssertEquals(RelatedPartyDirectionList.Codes.Forwarder, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.CustomsPostponedVatAccounting));

			AssertEquals(RelatedPartyDirectionList.Codes.AP, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.APSettlementGroup));
			AssertEquals(RelatedPartyDirectionList.Codes.AR, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ARSettlementGroup));

			AssertEquals(RelatedPartyDirectionList.Codes.Sales, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.SourceOfSalesLead));
			AssertEquals(RelatedPartyDirectionList.Codes.Sales, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ControllingAgent));

			AssertEquals(RelatedPartyDirectionList.Codes.Delivery, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.DeliveryAgent));
			AssertEquals(RelatedPartyDirectionList.Codes.Delivery, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.DeliveryTo));
			AssertEquals(RelatedPartyDirectionList.Codes.Pickup, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.PickupAgent));
			AssertEquals(RelatedPartyDirectionList.Codes.Pickup, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.PickupFrom));

			AssertEquals(ZString.Empty, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.Warehouse));
			AssertEquals(ZString.Empty, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ClientCFS));
			AssertEquals(ZString.Empty, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.Manufacturer));
			AssertEquals(ZString.Empty, OrgRelatedPartyTypeHelper.GetDefaultDirection(RelatedPartyTypeList.Codes.ForwarderCoLoadWith));
		}
	}
}
