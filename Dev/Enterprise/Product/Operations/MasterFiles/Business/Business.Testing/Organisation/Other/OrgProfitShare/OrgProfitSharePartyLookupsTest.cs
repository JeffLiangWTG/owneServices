using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing;

public class OrgProfitSharePartyLookupsTest : TestCaseWithFactory
{
	public void TestPartyTypesForGeneralPartyDetail()
	{
		var org = Factory.New<OrgHeader>();
		var relationship = Factory.New<OrgAgentRelationship>();
		relationship.O3_OH_SendingAgent = org.PK;
		var profitShare = relationship.ProfitShareDetails.AddNew();
		var party = profitShare.PartyDetails.AddNew();
		// party.Lookups (OrgProfitSharePartyLookups) has a parent: party (OrgProfitShareParty).
		// It is different from registry lookup, which has no parent and returns a different set of party types.

		profitShare.O4_JobType = JobTypesList.Codes.Blank;
		var actualTypes = party.Lookups.PartyTypes
			.OfType<CodeDescriptionPair>()
			.Select(x => (x.Code, x.Description));

		AssertContainsExactElementsInAnyOrder([
			("SEN", "Sending Agent"),
			("RCV", "Receiving Agent"),
			("CON", "Controlling Agent"),
			("HDF", "Head Office"),
			("PIC", "Pickup Agent"),
			("DLY", "Delivery Agent")
		], actualTypes);

		profitShare.O4_JobType = JobTypesList.Codes.GCN;
		actualTypes = party.Lookups.PartyTypes
			.OfType<CodeDescriptionPair>()
			.Select(x => (x.Code, x.Description));

		AssertContainsExactElementsInAnyOrder([
			("SEN", "Sending Agent"),
			("RCV", "Receiving Agent"),
			("HDF", "Head Office")
		], actualTypes);
	}

	public void TestPartyTypesForGatewayConsolProfitRedistribution()
	{
		var org = Factory.New<OrgHeader>();
		var relationship = Factory.New<OrgAgentRelationship>();
		relationship.O3_OH_SendingAgent = org.PK;
		var profitShare = relationship.ProfitShareDetails.AddNew();
		var party = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();

		var actualTypes = party.Lookups.PartyTypes
			.OfType<CodeDescriptionPair>()
			.Select(x => (x.Code, x.Description));

		AssertContainsExactElementsInAnyOrder([
			("SPA", "Shipment Pickup Agent"),
			("SDA", "Shipment Delivery Agent"),
		], actualTypes);
	}

	public void TestPartyTypesForRegistry()
	{
		// Registry refers to this OrgProfitSharePartyLookups as a standalone list - no parent.
		var lookups = new OrgProfitSharePartyLookups(null);
		var actualTypes = lookups.PartyTypes
			.OfType<CodeDescriptionPair>()
			.Select(x => (x.Code, x.Description));

		AssertContainsExactElementsInAnyOrder([
			("SEN", "Sending Agent"),
			("RCV", "Receiving Agent"),
			("CON", "Controlling Agent"),
			("HDF", "Head Office"),
			("GWA", "G/W Agent (G/W Consol)"),
			("LGA", "Lead G/W Agent"),
			("PIC", "Pickup Agent"),
			("DLY", "Delivery Agent"),
		], actualTypes);
	}
}
