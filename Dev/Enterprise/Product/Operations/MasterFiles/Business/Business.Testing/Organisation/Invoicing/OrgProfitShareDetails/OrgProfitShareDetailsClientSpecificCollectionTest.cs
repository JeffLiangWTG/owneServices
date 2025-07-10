using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareDetailsClientSpecificCollection))]
	sealed class OrgProfitShareDetailsClientSpecificCollectionTest : OrgProfitShareDetailsDependentCollectionTest
	{
		public void TestGetProfitShareAgreement_ClientSpecific()
		{
			SetupProfitShareDetails();

			OrgProfitShareDetails profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "AUSYD", "INBOM", new OrganisationsWithTypes(Client), null);
			AssertEquals("Correct profit share agreement found", ClientSpecificPS1.PK, profitShare.PK);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "AUSYD", "INBOM", new OrganisationsWithTypes(Client2), null);
			AssertEquals("Correct profit share agreement found", ClientSpecificPS2.PK, profitShare.PK);

			profitShare = CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "AUSYD", "INBOM", new OrganisationsWithTypes(Client3), null);
			AssertEquals("Correct profit share agreement found", PS4.PK, profitShare.PK);
		}

		public void TestGetProfitShareAgreement_ClientFallback()
		{
			var psALL = AddOrgProfitShareDetails(CollectionForTest);
			var clientPS1 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code);
			var clientPS2 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
			var clientPS3 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client2, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code);
			var clientPS4 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client3, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code);
			var clientPS5 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client3, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
			var clientPS6 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client3, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.ALL.Code);
			var clientPS7 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client2, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
			var clientPS8 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client2, orgOverrideType: OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code, sendingPortOrCountry: "NZ");
			Factory.Save();

			Func<OrganisationsWithTypes, OrgProfitShareDetails> getProfitShare = (orgTypes) => CombinedCollection.GetProfitShareAgreement(new ZDateTime(2004, 4, 20), "AIR", "", "AUSYD", "INBOM", orgTypes, null);

			AssertEquals("LOC higher than ALL", clientPS1.PK, getProfitShare(new OrganisationsWithTypes(Client)).PK);
			AssertEquals("Fallback to ALL as no CNR Client rule.", clientPS2.PK, getProfitShare(new OrganisationsWithTypes(null) { Consignor = Client, ImportBroker = Client2 }).PK);
			AssertEquals("Choose CNE Clinet2 as Client is PUA and PUA if lower then CNE.", clientPS3.PK, getProfitShare(new OrganisationsWithTypes(null) { Consignee = Client2, ImportBroker = Client3, PickUpAgent = Client }).PK);
			AssertEquals("No rule for CNR or ALL Client2, so use Client3 and then PUA higher than IBR.", clientPS4.PK, getProfitShare(new OrganisationsWithTypes(null) { Consignor = Client2, ImportBroker = Client3, PickUpAgent = Client3 }).PK);
			AssertEquals("No rule for CNR or ALL Client2, so use Client3 and then IBR higher than EBR.", clientPS5.PK, getProfitShare(new OrganisationsWithTypes(null) { Consignor = Client2, ImportBroker = Client3, ExportBroker = Client3 }).PK);
			AssertEquals("Use Client3 as LOC is highest, but no rule for LOC Client3 so fallback to ALL.", clientPS6.PK, getProfitShare(new OrganisationsWithTypes(Client3) { Consignor = Client, ImportBroker = Client2 }).PK);
			AssertEquals("No rule for LOC or ALL Client2, so use Client3 and fallback to ALL as no rule for CNR.", clientPS6.PK, getProfitShare(new OrganisationsWithTypes(Client2) { Consignor = Client3, ImportBroker = Client2 }).PK);

			AssertEquals("No rule for CNR or ALL Client2 so fallback non client specific rule.", psALL.PK, getProfitShare(new OrganisationsWithTypes(null) { Consignor = Client2 }).PK);
		}

		protected override OrgProfitShareDetailsDependentCollection CreateCollectionForTest(OrgAgentRelationship agent)
		{
			return new OrgProfitShareDetailsClientSpecificCollection(agent);
		}

		protected override void SetupProfitShareDetails()
		{
			base.SetupProfitShareDetails();

			ClientSpecificPS1 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client);
			ClientSpecificPS2 = AddOrgProfitShareDetails(CollectionForTest, orgOverride: Client2);

			Factory.Save();
		}
	}
}
