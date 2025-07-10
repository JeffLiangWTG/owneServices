using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgProfitShareParty))]
	sealed class OrgProfitSharePartyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesOrgProfitShareParty()
		{
			var org = Factory.New<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var profitShare = relationship.ProfitShareDetails.AddNew();
			var party = profitShare.PartyDetails.AddNew();

			var localList = new List<string>
			{
				nameof(party.PS_PartyMinimum)
			};

			var percentList = new List<string>
			{
				nameof(party.PS_PartyProfitSharePercent)
			};

			var rateList = new List<string>
			{
				nameof(party.PS_PartyRate)
			};

			var tester = new DecimalPlacesAttributeTester(party);
			tester.CheckLocalCurrency(localList, nameof(party.CurrencyDecimals));
			tester.CheckConstant(percentList, nameof(party.PercentDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
			tester.CheckConstant(rateList, nameof(party.RateDecimals), 2);
		}

		public void TestCalculation()
		{
			var org = Factory.New<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var profitShare = relationship.ProfitShareDetails.AddNew();
			var party = profitShare.PartyDetails.AddNew();

			// only for shortening casting code
			void AssertCalculation((ZDecimal, ZString) expectedCalculation, (ZDecimal, ZString) actualCalculation)
				=> AssertEquals(expectedCalculation, actualCalculation);

			party.PS_PartyProfitSharePercent = 20m;
			AssertCalculation((20m, "20.00% of profit of 100.00"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;
			AssertCalculation((400m, "20.00% of gross revenue of 2000.00"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyRateBasis = "";
			party.PS_PartyMinimum = 15m;
			AssertCalculation((20m, "20.00% of profit of 100.00"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyMinimum = 26m;
			AssertCalculation((26m, "Minimum Profit Share overrides 20.00% of profit of 100.00"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;
			party.PS_PartyRate = 2m;
			AssertCalculation((32m, "20.00% of profit of 100.00 + (2.00 * 6 Per Chargeable Unit)"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyMinimum = 35m;
			AssertCalculation((35m, "Minimum Profit Share overrides 20.00% of profit of 100.00 + (2.00 * 6 Per Chargeable Unit)"), party.CalculateProfitShare(100m, 2000m, 6m, 0));

			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer;
			party.PS_PartyRate = 2m;
			AssertCalculation((40m, "20.00% of profit of 100.00 + (2.00 * 10 Per Container)"), party.CalculateProfitShare(100m, 2000m, 6m, 10));

			party.PS_PartyMinimum = 45m;
			AssertCalculation((45m, "Minimum Profit Share overrides 20.00% of profit of 100.00 + (2.00 * 10 Per Container)"), party.CalculateProfitShare(100m, 2000m, 6m, 10));
		}

		public void TestReadonly()
		{
			var org = Factory.New<OrgHeader>();

			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var profitShare = relationship.ProfitShareDetails.AddNew();

			var sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			AssertEquals(false, sendParty.ReadOnly);

			var conParty = profitShare.PartyDetails.AddNew();
			conParty.PS_PartyType = "CON";
			AssertEquals(false, conParty.ReadOnly);

			profitShare.O4_OH_ControllingAgent = org.PK;
			AssertEquals(true, conParty.ReadOnly);
			AssertEquals(false, sendParty.ReadOnly);

			relationship.O3_OH_SendingAgent = ZGuid.Empty;
			AssertEquals(false, conParty.ReadOnly);
			AssertEquals(false, sendParty.ReadOnly);

			conParty.ReadOnly = true;
			AssertEquals(true, conParty.ReadOnly);
		}

		public void TestValuesPropagateToRelatedParty()
		{
			var org = Factory.New<OrgHeader>();

			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			var profitShare = relationship.ProfitShareDetails.AddNew();

			var sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";

			var conParty = profitShare.PartyDetails.AddNew();
			conParty.PS_PartyType = "CON";

			sendParty.PS_PartyRate = 100m;
			sendParty.PS_PartyRateBasis = "XXX";
			sendParty.PS_PartyProfitSharePercent = 20m;
			sendParty.PS_PartyMinimum = 100m;

			conParty.PS_PartyRate = 300m;
			conParty.PS_PartyRateBasis = "ZZZ";
			conParty.PS_PartyProfitSharePercent = 40m;

			AssertEquals(100m, sendParty.PS_PartyRate);
			AssertEquals("XXX", sendParty.PS_PartyRateBasis);
			AssertEquals(20m, sendParty.PS_PartyProfitSharePercent);
			AssertEquals(100m, sendParty.PS_PartyMinimum);

			AssertEquals(300m, conParty.PS_PartyRate);
			AssertEquals("ZZZ", conParty.PS_PartyRateBasis);
			AssertEquals(40m, conParty.PS_PartyProfitSharePercent);

			profitShare.O4_OH_ControllingAgent = org.PK;

			sendParty.PS_PartyRate = 100m;
			sendParty.PS_PartyProfitSharePercent = 73m;

			AssertEquals("Should be the same as the controlling agent", 100m, conParty.PS_PartyRate);

			AssertEquals("Should be the same as the sending agent", 73m, conParty.PS_PartyProfitSharePercent);

			sendParty.PS_PartyType = "TRA";
			sendParty.PS_PartyRate = 500m;
			AssertEquals("Should not propagate", 100m, conParty.PS_PartyRate);

			sendParty.PS_PartyType = "SEN";
			AssertEquals("Propagates immediately", 500m, conParty.PS_PartyRate);
		}

		public void TestLogging()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var relationship = Factory.New<OrgAgentRelationship>();
			relationship.O3_OH_SendingAgent = org.PK;
			relationship.O3_OH_ReceivingAgent = org.PK;
			var profitShare = relationship.ProfitShareDetails.AddNew();
			var party1 = profitShare.PartyDetails.AddNew();
			party1.PS_PartyType = "XXX";
			var party2 = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party2.PS_PartyType = "YYY";

			Factory.Save();
			AssertEquals("Autolog event reference description", "Party - XXX", party1.Logs.AutoCreatedLog.SL_Reference);
			AssertEquals("Autolog event reference description", "Party - YYY", party2.Logs.AutoCreatedLog.SL_Reference);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurityGeneralCollection() => AssertReadOnlySecurity(true);
		public void TestReadOnlySecurityRedistributionCollection() => AssertReadOnlySecurity(false);

		void AssertReadOnlySecurity(bool isGeneralCollection)
		{
			var oldProfitShareValue = Env.Security.OrgForwarderModifyProfitShare.IsAllowed;

			try
			{
				var testRelationship = OrgInDB.AgentRelationships.AddNew();
				OrgInDB.AgentRelationships.SetOrganisationReadOnly(OrgInDB);
				var testProfitShare = testRelationship.ProfitShareDetails.AddNew();
				var collection = isGeneralCollection ? (OrgProfitSharePartyCollection)testProfitShare.PartyDetails : testProfitShare.PartyDetailsForGatewayProfitShareRedistribution;
				var party1 = collection.AddNew();

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !party1.PS_PartyMinimumInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !party1.PS_PartyProfitSharePercentInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !party1.PS_PartyRateBasisInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !party1.PS_PartyRateInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !party1.PS_PartyTypeInfo.ReadOnly);

				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = false;
				party1 = collection.AddNew();
				Assert("Access NOT Allowed - ReadOnly", party1.PS_PartyMinimumInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", party1.PS_PartyProfitSharePercentInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", party1.PS_PartyRateBasisInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", party1.PS_PartyRateInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", party1.PS_PartyTypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgForwarderModifyProfitShare.IsAllowed = oldProfitShareValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var relationship = Factory.New<OrgAgentRelationship>();
			var profitShare = relationship.ProfitShareDetails.AddNew();
			return profitShare.PartyDetails.AddNew();
		}
	}
}
