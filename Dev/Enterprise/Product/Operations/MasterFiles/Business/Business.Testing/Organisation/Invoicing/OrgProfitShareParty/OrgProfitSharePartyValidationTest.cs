using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using PartyTypeCodes = Enterprise.MasterFiles.Business.OrgProfitSharePartyLookups.PartyTypeCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgProfitSharePartyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPartyType_General()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.ProfitShareDetails.AddNew();
			var party = details.PartyDetails.AddNew();

			party.PS_PartyType = "XXX";
			AssertHasError(party.PS_PartyTypeInfo, "Enter a valid Party.");

			party.PS_PartyType = "SEN";
			AssertNoErrors(party.PS_PartyTypeInfo);

			party.PS_PartyType = "";
			AssertHasError(party.PS_PartyTypeInfo, "Please enter a Party.");

			party.PS_PartyType = "SEN";
			AssertNoErrors(party.PS_PartyTypeInfo);

			var party2 = details.PartyDetails.AddNew();
			party2.PS_PartyType = "SEN";
			AssertHasError(party2.PS_PartyTypeInfo, "The Party has been duplicated and must be unique.");

			party2.PS_PartyType = "RCV";
			AssertNoErrors(party2.PS_PartyTypeInfo);

			var headOffice = Factory.New<OrgHeader>();
			relationship = Factory.New<OrgAgentRelationship>();
			details = relationship.ProfitShareDetails.AddNew();
			party = details.PartyDetails.AddNew();
			relationship.O3_OH_GroupNetworkOrFranchise = ZGuid.Empty;
			party.PS_PartyType = PartyTypeCodes.HeadOfficeFranchisor;
			AssertHasError(party.PS_PartyTypeInfo, "You can use 'HDF' type only if 'Head Office' is set");

			party.PS_PartyType = PartyTypeCodes.ReceivingAgent;
			AssertNoErrors(party.PS_PartyTypeInfo);

			relationship.O3_OH_GroupNetworkOrFranchise = headOffice.PK;
			party.PS_PartyType = PartyTypeCodes.HeadOfficeFranchisor;
			AssertNoErrors(party.PS_PartyTypeInfo);
		}

		public void TestPartyType_Redistribution()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.ProfitShareDetails.AddNew();
			var party = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();

			party.PS_PartyType = "XXX";
			AssertHasError(party.PS_PartyTypeInfo, "Enter a valid Party.");

			party.PS_PartyType = "SDA";
			AssertNoErrors(party.PS_PartyTypeInfo);

			party.PS_PartyType = "";
			AssertHasError(party.PS_PartyTypeInfo, "Please enter a Party.");

			party.PS_PartyType = "SPA";
			AssertNoErrors(party.PS_PartyTypeInfo);
		}

		public void TestPartyType_AddCodeToOtherTypeCollection_ShouldShowError()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.ProfitShareDetails.AddNew();
			var generalCollection = details.PartyDetails;
			var redistributionCollection = details.PartyDetailsForGatewayProfitShareRedistribution;

			var party = generalCollection.AddNew();
			var testCodes = new[]
			{
				PartyTypeCodes.ShipmentPickupAgent,
				PartyTypeCodes.ShipmentDeliveryAgent,
			};
			foreach (var testCode in testCodes)
			{
				party.PS_PartyType = testCode;
				AssertHasError(party.PS_PartyTypeInfo, "Enter a valid Party.");
			}

			party = redistributionCollection.AddNew();
			testCodes = new[]
			{
				PartyTypeCodes.SendingAgent,
				PartyTypeCodes.ReceivingAgent,
				PartyTypeCodes.HeadOfficeFranchisor,
				PartyTypeCodes.ControllingAgent,
			};
			foreach (var testCode in testCodes)
			{
				party.PS_PartyType = testCode;
				AssertHasError(party.PS_PartyTypeInfo, "Enter a valid Party.");
			}
		}

		public void TestPartyRateBasis()
		{
			OrgProfitShareDetails details = Factory.New<OrgProfitShareDetails>();
			OrgProfitShareParty party = details.PartyDetails.AddNew();

			party.PS_PartyRateBasis = "XXX";
			AssertHasErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRateBasis = "";
			AssertNoErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRate = 100m;
			party.PS_PartyRateBasis = "";
			AssertHasErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRateBasis = "XXX";
			AssertHasErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee;
			AssertNoErrors(party.PS_PartyRateBasisInfo);
		}

		public void TestPartyRateBasis_GrossRevenue()
		{
			OrgProfitShareDetails details = Factory.New<OrgProfitShareDetails>();
			OrgProfitShareParty party = details.PartyDetails.AddNew();

			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;
			AssertNoErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRate = 30m;
			AssertHasErrors(party.PS_PartyRateBasisInfo);

			party.PS_PartyRate = 0m;
			AssertNoErrors(party.PS_PartyRateBasisInfo);
		}

		public void TestPartyPercentage_CodesShouldBelongToEitherGeneralOrRedistributionCollection()
		{
			var allCodes = typeof(PartyTypeCodes)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(fi => (string)fi.GetRawConstantValue())
				.ToHashSet();

			var generalCodes = PartyTypeCodes.GeneralTypes;
			var redistributionCodes = PartyTypeCodes.GatewayConsolProfitRedistributionTypes;

			var exclusions = new[]
			{
				PartyTypeCodes.GatewayAgent,
				PartyTypeCodes.LeadGatewayAgent
			};
			var message = @"When this test fails, please review the PartyTypeCodes class and ensure
that all codes are added to either GeneralTypes or GatewayConsolProfitRedistributionTypes.
If a code is not applicable to either collection, it can be added to the above exclusion list.";

			AssertContainsExactElementsInAnyOrder(
				message,
				allCodes.Except(exclusions),
				generalCodes.Concat(redistributionCodes));
		}

		public void TestPartyPercentage()
		{
			var sendAgent = Factory.New<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
			var details = relationship.ProfitShareDetails.AddNew();

			// General party details
			var party1 = details.PartyDetails.AddNew();
			var party2 = details.PartyDetails.AddNew();
			var party3 = details.PartyDetails.AddNew();
			party1.PS_PartyType = "SEN";
			party2.PS_PartyType = "RCV";
			party3.PS_PartyType = "CON";

			party1.PS_PartyProfitSharePercent = 20m;
			party2.PS_PartyProfitSharePercent = 30m;
			party3.PS_PartyProfitSharePercent = 50m;

			// party1 + party2 + party3 = 100%
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party3.PS_PartyProfitSharePercentInfo);

			relationship.O3_OH_SendingAgent = sendAgent.PK;
			details.O4_OH_ControllingAgent = sendAgent.PK;

			party1.PS_PartyProfitSharePercent = 20m;
			party2.PS_PartyProfitSharePercent = 30m;
			party3.PS_PartyProfitSharePercent = 50m;

			// party1 + party2 = 50%
			AssertHasErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors("Readonly - as it's the controlling agent", party3.PS_PartyProfitSharePercentInfo);

			// party1 + party2 = 100%
			party1.PS_PartyProfitSharePercent = 70m;
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party3.PS_PartyProfitSharePercentInfo);

			var party4 = details.PartyDetails.AddNew();
			party4.PS_PartyType = "PIC";
			party4.PS_PartyProfitSharePercent = 10m;

			// party1 + party2 + party4 = 110%
			AssertHasErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party4.PS_PartyProfitSharePercentInfo);

			var party5 = details.PartyDetails.AddNew();
			party5.PS_PartyType = "DLY";
			party5.PS_PartyProfitSharePercent = 20m;

			// party1 + party2 + party4 + party5 = 130%
			AssertHasErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party4.PS_PartyProfitSharePercentInfo);
			AssertHasErrors(party5.PS_PartyProfitSharePercentInfo);

			// party1 + party2 + party4 + party5 = 100%
			party1.PS_PartyProfitSharePercent = 50m;
			party2.PS_PartyProfitSharePercent = 20m;
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party4.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party5.PS_PartyProfitSharePercentInfo);
		}

		public void TestPartyPercentage_AgencyProfile()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var details = relationship.ProfitShareDetails.AddNew();
			var party1 = details.PartyDetails.AddNew();
			var party2 = details.PartyDetails.AddNew();
			var party3 = details.PartyDetails.AddNew();
			party1.PS_PartyType = "SEN";
			party2.PS_PartyType = "RCV";
			party3.PS_PartyType = "CON";

			party1.PS_PartyProfitSharePercent = 20m;
			party2.PS_PartyProfitSharePercent = 10m;
			party3.PS_PartyProfitSharePercent = 30m;

			// GW Consol Profit Share Redistribution party details
			// Should be independent from the 3 General party details above.
			var party4 = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			var party5 = details.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party4.PS_PartyType = "SDA";
			party5.PS_PartyType = "SPA";

			// party1 + party2 + party3 = 60%
			AssertHasError(party1.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
			AssertHasError(party2.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
			AssertHasError(party3.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");

			party1.PS_PartyProfitSharePercent = 20m;
			party2.PS_PartyProfitSharePercent = 10m;
			party3.PS_PartyProfitSharePercent = 70m;
			// party1 + party2 + party3 = 100%
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party3.PS_PartyProfitSharePercentInfo);

			// AgencyProfile does not check general party details for total percentage
			relationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			party1.PS_PartyProfitSharePercent = 20m;
			party2.PS_PartyProfitSharePercent = 10m;
			party3.PS_PartyProfitSharePercent = 30m;
			// party1 + party2 + party3 = 60%
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party3.PS_PartyProfitSharePercentInfo);

			party1.PS_PartyProfitSharePercent = 0m;
			AssertHasError(party1.PS_PartyProfitSharePercentInfo, "Please enter a 'Percent' greater than 0.");
			party1.PS_PartyProfitSharePercent = -10m;
			AssertHasError(party1.PS_PartyProfitSharePercentInfo, "Please enter a 'Percent' greater than 0.");
			party1.PS_PartyProfitSharePercent = 101m;
			AssertHasError(party1.PS_PartyProfitSharePercentInfo, "Please enter a 'Percent' less than or equal to 100.");
			party1.PS_PartyProfitSharePercent = 100m;
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);

			// AgencyProfile however checks redistribution party details for total percentage
			// party4 + party5 = 80%
			party4.PS_PartyProfitSharePercent = 35;
			party5.PS_PartyProfitSharePercent = 45;
			AssertHasError(party4.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
			AssertHasError(party5.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
			// shouldn't affect General party details
			AssertNoErrors(party1.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party2.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party3.PS_PartyProfitSharePercentInfo);

			// party4 + party5 = 100%
			party4.PS_PartyProfitSharePercent = 55;
			party5.PS_PartyProfitSharePercent = 45;
			AssertNoErrors(party4.PS_PartyProfitSharePercentInfo);
			AssertNoErrors(party5.PS_PartyProfitSharePercentInfo);

			// party4 + party5 = 120%
			party4.PS_PartyProfitSharePercent = 55;
			party5.PS_PartyProfitSharePercent = 65;
			AssertHasError(party4.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
			AssertHasError(party5.PS_PartyProfitSharePercentInfo, "The profit share percentages entered do not add to 100.");
		}
	}
}
