using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRelatedPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRelatedParties()
		{
			AssertNotNull(Lookups.RelatedParties);
			AssertEquals(typeof(OrgHeaderCollection), Lookups.RelatedParties.GetType());
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedVendor;
			AssertEquals(typeof(ConsignorCollection), Lookups.RelatedParties.GetType());
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			AssertEquals(typeof(SelfFilerCollection), Lookups.RelatedParties.GetType());
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			RelatedPartyRecord.PR_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(typeof(CreditorCollection), Lookups.RelatedParties.GetType()); //for SPC, Only for COM level the related party should be payable
			RelatedPartyRecord.PR_GC = ZGuid.Empty;
			AssertEquals(typeof(OrgHeaderCollection), Lookups.RelatedParties.GetType());
		}

		public void TestAllParentTypeList()
		{
			CodeDescriptionPairList list = Lookups.AllParentTypeList;

			AssertEquals("CustomsAgentBroker", true, list.ContainsCode(RelatedPartyTypeList.Codes.CustomsAgentBroker));
			AssertEquals("LocalTransport", true, list.ContainsCode(RelatedPartyTypeList.Codes.LocalTransport));
			AssertEquals("LocalTransportBillTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.LocalTransportBillTo));
			AssertEquals("InvoiceCustomsJobsTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo));
			AssertEquals("InvoiceFreightJobsTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.InvoiceFreightJobsTo));
			AssertEquals("ReportRevenueTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.ReportRevenueTo));
			AssertEquals("ControllingCustomer", true, list.ContainsCode(RelatedPartyTypeList.Codes.ControllingCustomer));
			AssertEquals("ClientCFS", true, list.ContainsCode(RelatedPartyTypeList.Codes.ClientCFS));
			AssertEquals("ARNettingGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.ARNettingGroup));
			AssertEquals("APNettingGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.APNettingGroup));
			AssertEquals("DefaultCFSDepot", true, list.ContainsCode(RelatedPartyTypeList.Codes.ForwarderCFS));
			AssertEquals("ManagementGrouping", true, list.ContainsCode(RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals("ForwarderGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.ForwarderGroup));
			AssertEquals("APSettlementGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.APSettlementGroup));
			AssertEquals("ARSettlementGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.ARSettlementGroup));
			AssertEquals("ControllingAgent", true, list.ContainsCode(RelatedPartyTypeList.Codes.ControllingAgent));
			AssertEquals("CustomsOffice", true, list.ContainsCode(RelatedPartyTypeList.Codes.CustomsOffice));
			AssertEquals("DeliveryAgent", true, list.ContainsCode(RelatedPartyTypeList.Codes.DeliveryAgent));
			AssertEquals("DeliveryTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.DeliveryTo));
			AssertEquals("InvoiceWarehouseJobsTo", true, list.ContainsCode(RelatedPartyTypeList.Codes.InvoiceWarehouseJobsTo));
			AssertEquals("ServiceProvider", true, list.ContainsCode(RelatedPartyTypeList.Codes.ServiceProvider));
			AssertEquals("SourceOfSalesLead", true, list.ContainsCode(RelatedPartyTypeList.Codes.SourceOfSalesLead));
			AssertEquals("RecievingAgnet", true, list.ContainsCode(RelatedPartyTypeList.Codes.ReceivingAgent));
			AssertEquals("SendingAgent", true, list.ContainsCode(RelatedPartyTypeList.Codes.SendingAgent));
			AssertEquals("PickupAgent", true, list.ContainsCode(RelatedPartyTypeList.Codes.PickupAgent));
			AssertEquals("PickupFrom", true, list.ContainsCode(RelatedPartyTypeList.Codes.PickupFrom));
			AssertEquals("NotifyParty", true, list.ContainsCode(RelatedPartyTypeList.Codes.NotifyParty));

			AssertEquals("WRP is reserved for cargowise use only. It should not be used", false, list.ContainsCode("WRP"));
		}

		public void TestAllPartyTypeList()
		{
			CodeDescriptionPairList list = Lookups.AllPartyTypeList;

			var expectedCodes = new[]
			{
				"ACG", "ACR", "APN", "APS", "ARN", "ARS", "CCF",
				"CTY", "CAG", "CCB", "CAU", "CAV",
				"CAB", "COF", "CPV", "DAG", "ECD",
				"CFS", "FCW", "FGO", "FLT", "ICT",
				"IFT", "IWT", "JNP", "LFW", "LTT",
				"LTB", "MNG", "MAN", "NDC", "PAG",
				"AGR", "RRT", "RTA", "AGS", "SRV",
				"SPC", "SHB", "SOL", "WHS", "WAF",
				"PUF", "DET", "NFP", "PPT", "ICS"
			};

			var allCodes = Lookups.AllPartyTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(expectedCodes, allCodes);

			AssertEquals("WRP is reserved for cargowise use only. It should not be used", false, list.ContainsCode("WRP"));
		}

		public void TestContainerModeList()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ExportConsolidationDepot;
			RelatedPartyRecord.PR_FreightTransportMode = "000";
			AssertEquals(0, Lookups.ContainerModeList.Count);

			RelatedPartyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(true, Lookups.ContainerModeList.Count > 0);
			AssertEquals(true, Lookups.ContainerModeList.ContainsCode(Core.Constants.ContainerModes.FCL));
			AssertEquals(false, Lookups.ContainerModeList.ContainsCode(Core.Constants.ContainerModes.Groupage));
			AssertEquals(false, Lookups.ContainerModeList.ContainsCode(Core.Constants.ContainerModes.Other));

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			RelatedPartyRecord.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(true, Lookups.ContainerModeList.ContainsCode(Core.Constants.ContainerModes.Groupage));
			AssertEquals(true, Lookups.ContainerModeList.ContainsCode(Core.Constants.ContainerModes.Other));
		}

		public void TestTransportModeList()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ExportConsolidationDepot;
			AssertEquals(7, Lookups.TransportModeList.Count);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			AssertEquals(5, Lookups.TransportModeList.Count);
			Assert(Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.All));
			Assert(Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Air));
			Assert(Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Sea));
			Assert(Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Rail));
			Assert(Lookups.TransportModeList.ContainsCode(Core.Constants.TransportModes.Road));

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.AuthorizedCargoReporter;
			AssertEquals("PR_PartyType 'ACR'", "AIR", Lookups.TransportModeList.CodesAsString);
		}

		public void TestForwarderPartyTypeList()
		{
			var list = Lookups.ForwarderPartyTypeList;

			AssertEquals("ARNettingGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.ARNettingGroup));
			AssertEquals("APNettingGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.APNettingGroup));
			AssertEquals("DefaultCFSDepot", true, list.ContainsCode(RelatedPartyTypeList.Codes.ForwarderCFS));
			AssertEquals("ManagementGrouping", true, list.ContainsCode(RelatedPartyTypeList.Codes.ManagementGrouping));
			AssertEquals("ForwarderGroup", true, list.ContainsCode(RelatedPartyTypeList.Codes.ForwarderGroup));
			AssertEquals("CustomsOffice", true, list.ContainsCode(RelatedPartyTypeList.Codes.CustomsOffice));
			AssertEquals("ForwarderLocalTransport", true, list.ContainsCode(RelatedPartyTypeList.Codes.ForwarderLocalTransport));
			AssertEquals("WarehouseForwarder", true, list.ContainsCode(RelatedPartyTypeList.Codes.WarehouseForwarder));
		}

		public void TestCompanyLevelList()
		{
			AssertNotNull("CompanyLevelList", Lookups.CompanyLevelList);
			AssertEquals(typeof(CompanyLevelList), Lookups.CompanyLevelList.GetType());
		}

		public void TestPartyTypeList()
		{
			AssertEquals("PartyTypeList", typeof(PartyTypeDescriptionOnlyList), Lookups.PartyTypeList.GetType());
		}

		public void TestFreightDirectionList()
		{
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ExportConsolidationDepot;
			var list = Lookups.FreightDirectionList;
			AssertEquals("Delivery", true, list.ContainsCode(RelatedPartyDirectionList.Codes.Delivery));
			AssertEquals("Pickup", true, list.ContainsCode(RelatedPartyDirectionList.Codes.Pickup));
			AssertEquals("PickupAndDelivery", true, list.ContainsCode(RelatedPartyDirectionList.Codes.PickupAndDelivery));

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ForwarderCoLoadWith;
			list = Lookups.FreightDirectionList;
			AssertEquals(1, list.Count);
			AssertEquals("Pickup", true, list.ContainsCode(RelatedPartyDirectionList.Codes.Pickup));
		}

		public void TestAddressesList() => CombineAssertions(() =>
		{
			var org1 = Factory.New<OrgHeader>();
			org1.Addresses.AddNew();
			RelatedPartyRecord.PR_OH_Parent = org1.PK;
			AssertContainsExactElementsInAnyOrder(org1.Addresses, RelatedPartyRecord.Lookups.Addresses);

			var org2 = Factory.New<OrgHeader>();
			org2.Addresses.AddNew();
			org2.Addresses.AddNew();
			RelatedPartyRecord.PR_OH_RelatedParty = org2.PK;
			AssertContainsExactElementsInAnyOrder(org1.Addresses, RelatedPartyRecord.Lookups.Addresses);

			RelatedPartyRecord.PR_OH_RelatedParty = ZGuid.Empty;
			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
			AssertEquals(0, RelatedPartyRecord.Lookups.Addresses.Count);

			RelatedPartyRecord.PR_OH_RelatedParty = org2.PK;
			AssertContainsExactElementsInAnyOrder(org2.Addresses, RelatedPartyRecord.Lookups.Addresses);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.PickupFrom;
			AssertContainsExactElementsInAnyOrder(org2.Addresses, RelatedPartyRecord.Lookups.Addresses);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.DeliveryTo;
			AssertContainsExactElementsInAnyOrder(org2.Addresses, RelatedPartyRecord.Lookups.Addresses);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
			AssertContainsExactElementsInAnyOrder(org2.Addresses, RelatedPartyRecord.Lookups.Addresses);

			RelatedPartyRecord.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
			AssertContainsExactElementsInAnyOrder(org2.Addresses, RelatedPartyRecord.Lookups.Addresses);
		});

		OrgRelatedParty RelatedPartyRecord
		{
			get { return orgRelatedParty ?? (orgRelatedParty = Factory.New<OrgRelatedParty>()); }
		}
		OrgRelatedParty orgRelatedParty;

		OrgRelatedPartyLookups Lookups
		{
			get { return lookups ?? (lookups = new OrgRelatedPartyLookups(RelatedPartyRecord)); }
		}
		OrgRelatedPartyLookups lookups;

		public void TestLocationsList()
		{
			OrgRelatedParty party = Factory.New<OrgRelatedParty>();

			party.Lookups.Locations.LoadUNLoco(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			AssertEquals("UNLOCOs are in the list", 1, party.Lookups.Locations.Count);

			party.Lookups.Locations.LoadCountry(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			AssertEquals("Countries are in the list", 1, party.Lookups.Locations.Count);

			party.Lookups.Locations.LoadZone(new ZQuery());
			AssertEquals("Zones are not in the list", 0, party.Lookups.Locations.Count);
		}

		public void TestConsignorPartyTypeList()
		{
			var expectedCodes = new[]
			{
				"CCF", "CCB", "CAB", "FCW",
				"ICT", "IFT", "JNP", "LTT",
				"LTB", "NDC", "PAG", "AGR",
				"RRT", "AGS", "WHS", "WAF",
				"PUF", "NFP",
			};

			var allowedCodes = Lookups.ConsignorPartyTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedCodes, allowedCodes);
		}

		public void TestConsigneePartyTypeList()
		{
			var expectedCodes = new[]
			{
				"CCF", "CCB", "CAB", "DAG",
				"FCW", "ICT", "IFT", "JNP",
				"LTT", "LTB", "NDC", "AGR",
				"RRT", "AGS", "WHS", "WAF",
				"DET", "NFP",
			};

			var allowedCodes = Lookups.ConsigneePartyTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedCodes, allowedCodes);
		}
	}
}
