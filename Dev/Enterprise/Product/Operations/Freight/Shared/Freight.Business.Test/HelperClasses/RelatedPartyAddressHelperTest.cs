using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RelatedPartyAddressHelperTest : TestCaseWithFactory
	{
		public void TestMatchRelatedParty()
		{
			var relatedPartyRank1 = Factory.New<OrgRelatedParty>();
			relatedPartyRank1.PR_FreightContainerMode = Core.Constants.ContainerModes.Loose;
			relatedPartyRank1.PR_Location = "AUSYD";
			relatedPartyRank1.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedPartyRank1.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedPartyRank1.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank2 = Factory.New<OrgRelatedParty>();
			relatedPartyRank2.PR_FreightContainerMode = Core.Constants.ContainerModes.Loose;
			relatedPartyRank2.PR_Location = "AUSYD";
			relatedPartyRank2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedPartyRank2.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank3 = Factory.New<OrgRelatedParty>();
			relatedPartyRank3.PR_Location = "AUSYD";
			relatedPartyRank3.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
			relatedPartyRank3.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank4 = Factory.New<OrgRelatedParty>();
			relatedPartyRank4.PR_Location = "AUSYD";
			relatedPartyRank4.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			relatedPartyRank4.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank5 = Factory.New<OrgRelatedParty>();
			relatedPartyRank5.PR_Location = "AU";
			relatedPartyRank5.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			relatedPartyRank5.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank6 = Factory.New<OrgRelatedParty>();
			relatedPartyRank6.PR_Location = string.Empty;
			relatedPartyRank6.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			relatedPartyRank6.PR_RN_NKImporterCountry = "AU";

			var relatedPartyRank7 = Factory.New<OrgRelatedParty>();
			relatedPartyRank7.PR_Location = string.Empty;
			relatedPartyRank7.PR_FreightTransportMode = Core.Constants.TransportModes.All;
			relatedPartyRank7.PR_RN_NKImporterCountry = string.Empty;

			var relatedPartyNoneScore1 = Factory.New<OrgRelatedParty>();
			relatedPartyNoneScore1.PR_FreightContainerMode = Core.Constants.ContainerModes.All;
			relatedPartyNoneScore1.PR_Location = "AUSYD";
			relatedPartyNoneScore1.PR_FreightTransportMode = string.Empty;

			var relatedPartyNoneScore2 = Factory.New<OrgRelatedParty>();
			relatedPartyNoneScore2.PR_FreightContainerMode = Core.Constants.ContainerModes.FTL;
			relatedPartyNoneScore2.PR_Location = "AUSYD";
			relatedPartyNoneScore2.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;

			var relatedPartyNoneScore3 = Factory.New<OrgRelatedParty>();
			relatedPartyNoneScore3.PR_FreightContainerMode = Core.Constants.ContainerModes.FCL;
			relatedPartyNoneScore3.PR_Location = "NZAKL";
			relatedPartyNoneScore3.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;

			var relatedParties = new List<OrgRelatedParty>
			{
				relatedPartyNoneScore2,
				relatedPartyRank2,
				relatedPartyRank7,
				relatedPartyNoneScore3,
				relatedPartyRank4,
				relatedPartyRank6,
				relatedPartyRank1,
				relatedPartyRank5,
				relatedPartyRank3,
				relatedPartyNoneScore1
			};

			var matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank1.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank1);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank2.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank2);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank3.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank3);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank4.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank4);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank5.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank5);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank6.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank6);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertEquals(relatedPartyRank7.PK, matchedParty.PK);

			relatedParties.Remove(relatedPartyRank7);
			matchedParty = GetMatchedRelatedParty(relatedParties.ToArray());
			AssertNull(matchedParty);
		}

		OrgRelatedParty GetMatchedRelatedParty(OrgRelatedParty[] relatedParties) => RelatedPartyAddressHelper.MatchRelatedParty(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.Loose, "AUSYD", "AU", relatedParties);
	}
}
