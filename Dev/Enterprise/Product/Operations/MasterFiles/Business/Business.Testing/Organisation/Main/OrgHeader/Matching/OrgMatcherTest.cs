using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgMatcherTest : NUnit.Framework.TransactionedTestCase
	{
		#region TestMatchToSimilarOrganisation

		public void TestMatchToSimilarOrganisations()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE 1";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_Address1 = "AddressTestOrg";
			org.OH_FullName = "TEST ORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org);

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "CODE 1";
			org1.MainAddress.OA_City = "City";
			org1.MainAddress.OA_Address1 = "AddressTestOrg";
			org1.OH_FullName = "TEST ORG 2";
			org1.OH_RL_NKClosestPort = "AUSYD";
			org1.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(org1);

			OrgHeader orgToMatch = Factory.New<OrgHeader>();
			orgToMatch.OH_FullName = "TEST ORG";
			orgToMatch.OH_RL_NKClosestPort = "AUSYD";
			orgToMatch.MainAddress.OA_Address1 = "AddressTestOrg";
			orgToMatch.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(orgToMatch);

			OrgMatcher matcher = new OrgMatcher(Factory);
			OrgMatchResult matchResult = matcher.MatchToSimilarOrganisations(orgToMatch);
			AssertNotNull(matchResult.MatchingOrg);
			AssertEquals(org.PK, matchResult.MatchingOrg.PK);
		}

		#endregion

		#region Match To Unmatched Organisation

		public void TestMatchToUnmatchedOrganisation_NoSimilarOrgsFound()
		{
			SetMatchToUNMATCHEDRegistryItem(false);
			AssertOrgMatchWithoutOverrides("HALLABALLOOZA YAH YAH NO MATCH", "", OrgMatchType.NoMatchesFound, null);
		}

		public void TestMatchToUnmatchedOrganisation_SimilarOrgsFound()
		{
			TestOrg.OH_FullName = "TEST ORG";
			TestOrg.OH_RL_NKClosestPort = "AUSYD";

			SetMatchToUNMATCHEDRegistryItem(true);
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.ExactMatch, TestOrg); // Even though the Registry setting is true, we got an exact match so the UNMATCHED org NOT shown
		}

		#endregion

		#region Match With and Without Overrides 

		public void TestOverrideMatch()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			AssertOrgMatch("Non existent org", OrgMatchType.NoMatchesFound, null);

			OrgPatternMatchOverride matchOverride = OrgWithOverrides.CreatePatternMatchOverrideForTest();

			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			matchOverride.OO_LocalGuid = TestOrg.PK;
			matchOverride.OO_ForeignCode = "Test Org";

			AssertOrgMatch("Test Org", OrgMatchType.NoMatchesFound, null); // OrgPatternMatchOverride is not of the right type

			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			matchOverride.OO_LocalGuid = TestOrg.PK;
			matchOverride.OO_ForeignCode = "Test Org";

			AssertOrgMatch("Non existent org", OrgMatchType.NoMatchesFound, null);
			AssertOrgMatch("Test Org", OrgMatchType.OverrideMatch, TestOrg);
			AssertOrgMatch("TEST ORG", OrgMatchType.OverrideMatch, TestOrg);
			AssertOrgMatch("TEST ORG WRONG", OrgMatchType.NoMatchesFound, null);
		}

		public void TestMatchWithoutOverrides()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			TestOrg.OH_FullName = "TEST ORG";
			TestOrg.OH_RL_NKClosestPort = "AUSYD";
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.NoMatchesFound, null);

			TestOrg.OH_FullName = "TEST ORG PTY LTD";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.NoMatchesFound, null);

			OrgHeader secondOrg = Factory.New<OrgHeader>();
			secondOrg.OH_FullName = "TEST ORG";
			secondOrg.OH_RL_NKClosestPort = "AUSYD";
			secondOrg.MainAddress.OA_Address1 = "Address1";
			secondOrg.MainAddress.OA_City = "City";
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.ExactMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "", OrgMatchType.ExactMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.NoMatchesFound, null);

			TestOrg.OH_FullName = "TEST ORG";
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.MultipleExactMatches, null);
			AssertOrgMatchWithoutOverrides("Test Org", "", OrgMatchType.MultipleExactMatches, null);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.NoMatchesFound, null);

			secondOrg.OH_FullName = "TEST ORG ltd";
			secondOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(secondOrg);
			TestOrg.OH_FullName = "TEST ORG PTY";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUSYD", OrgMatchType.MultipleFilteredNameMatches, null);
			AssertOrgMatchWithoutOverrides("Test Org", "", OrgMatchType.MultipleFilteredNameMatches, null);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.NoMatchesFound, null);

			secondOrg.OH_RL_NKClosestPort = "AUMEL";
			secondOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(secondOrg);
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);

			secondOrg.OH_FullName = "TEST ORG";
			AssertOrgMatchWithoutOverrides("Test Org", "AUMEL", OrgMatchType.ExactMatch, secondOrg);

			AssertOrgMatchWithoutOverrides("Test-Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test-Org Pty", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);

			secondOrg.OH_FullName = "TESTORG";
			secondOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(secondOrg);
			AssertOrgMatchWithoutOverrides("Test-Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test-Org PTY", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);

			AssertOrgMatchWithoutOverrides("Test---Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test --- Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test -  -  -  Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test  ---  Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test - Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
			AssertOrgMatchWithoutOverrides("Test  -  Org", "AUMEL", OrgMatchType.FilteredNameMatch, secondOrg);
		}

		#endregion

		#region Match Names

		public void TestExactNameMatch()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			AssertOrgMatch("Test Exact Match Org", OrgMatchType.NoMatchesFound, null);

			TestOrg.OH_FullName = "Test Exact Match Org";
			AssertOrgMatch("Test Exact Match Org", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatch("TEST EXACT MATCH ORG", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatch("Test Exact  Match Org", OrgMatchType.NoMatchesFound, null);
			AssertOrgMatch("Test Exact Match Org Wrong", OrgMatchType.NoMatchesFound, null);

			OrgHeader secondOrg = Factory.New<OrgHeader>();
			secondOrg.OH_FullName = "Test Exact Match Org";

			AssertOrgMatch("Test Exact Match Org", OrgMatchType.MultipleExactMatches, null);
		}

		public void TestSearchWithLongOrgName()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			// Tests for using a name longer than the max length of the OrgHeaderSchema.Constants.OH_FullName column
			string longName = "".PadRight(TestOrg.OH_FullNameInfo.MaxLength + 1, 'z');
			AssertOrgMatch(longName, OrgMatchType.NoMatchesFound, null);

			TestOrg.OH_FullName = "".PadRight(TestOrg.OH_FullNameInfo.MaxLength, 'z');
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);

			AssertOrgMatch(longName, OrgMatchType.FilteredNameMatch, TestOrg);
		}

		#endregion

		#region Filtered Org Match

		public void TestFilteredMatch()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			AssertOrgMatch("Test Org", OrgMatchType.NoMatchesFound, null);

			TestOrg.OH_FullName = "Test Org Header PTY";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);

			AssertOrgMatch("Test Org Header", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("Test Org Header LTD", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("Test Org  Header ", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("Test. Org Header", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch(" Test, Org, Header", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST ORG HEADER", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("Test Org Header PTY.LTD.", OrgMatchType.FilteredNameMatch, TestOrg);

			AssertOrgMatch("ZXY Test Org Header", OrgMatchType.NoMatchesFound, null);

			OrgHeader secondOrg = Factory.New<OrgHeader>();
			secondOrg.OH_FullName = "Test Not Org Header";
			secondOrg.OH_Code = "CODE";
			secondOrg.OH_RL_NKClosestPort = "AUSYD";
			secondOrg.OH_FullName = "FullName";
			secondOrg.MainAddress.OA_Address1 = "Address1";
			secondOrg.MainAddress.OA_City = "City";
			secondOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(secondOrg);
			AssertOrgMatch("Test Org Header", OrgMatchType.FilteredNameMatch, TestOrg);

			secondOrg.OH_FullName = "Test Org Header LTD INC";
			secondOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(secondOrg);
			AssertOrgMatch("Test Org Header LIMITED", OrgMatchType.MultipleFilteredNameMatches, null);
		}

		#endregion

		#region Character Influential Based Matching

		public void TestHyphensSplittingWords()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			TestOrg.OH_FullName = "TEST ORG DISTRIBUTORS LTD";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatch("TEST-ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST - ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST  -  ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST----ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);

			TestOrg.OH_FullName = "TESTORG DISTRIBUTORS LTD";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatch("TEST-ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST-ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST - ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST  -  ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TEST----ORG DISTRIBUTORS", OrgMatchType.FilteredNameMatch, TestOrg);
		}

		public void TestAmpersandMatchesAnd()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			TestOrg.OH_FullName = "TestOrg & Co Ltd";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatch("TESTORG", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TESTORG AND CO.", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TESTORG CO.", OrgMatchType.FilteredNameMatch, TestOrg);

			TestOrg.OH_FullName = "TestOrg & Sons Ltd";
			TestOrg.PatternMatchesForThisOrg.GeneratePatternMatchesFromOrg(TestOrg);
			AssertOrgMatch("TESTORG", OrgMatchType.NoMatchesFound, null);
			AssertOrgMatch("TESTORG AND SONS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TESTORG SONS", OrgMatchType.FilteredNameMatch, TestOrg);
			AssertOrgMatch("TESTORG & SONS", OrgMatchType.FilteredNameMatch, TestOrg);
		}

		#region Test Contacts Does Not Load Contacts If Filter Is Empty

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestSearchWithNoNameAndNoPort()
		{
			SetMatchToUNMATCHEDRegistryItem(false);

			TestOrg.OH_FullName = "aaTEST ORG";
			TestOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgWithOverrides.Delete();
			Factory.Save();

			AssertOrgMatchWithoutOverrides("aaTest Org", "AUSYD", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("aaTest Org", "", OrgMatchType.ExactMatch, TestOrg);
			AssertOrgMatchWithoutOverrides("aaTest Org", "AUMEL", OrgMatchType.NoMatchesFound, null);
			AssertOrgMatchWithoutOverrides("", "", OrgMatchType.NoMatchesFound, null);

			AssertNotNull("Precondition - ensure there are OrgHeaders in the DB.", Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_FullName, "aaTest Org")));

			ZQuery query = Matcher.PatternMatchFilter("", "");
			AssertEquals("Filter should force a NoResult status.", true, query.IsNoResultQuery);
			OrgPatternMatchCollection orgs = new OrgPatternMatchCollection(Factory, query);
			orgs.Load();
			AssertEquals("Should return no Orgs when empty strings are sent.", 0, orgs.Count);

			query = Matcher.PatternMatchFilter("aaTest Org", "");
			orgs = new OrgPatternMatchCollection(Factory, query);
			orgs.Load();
			AssertEquals("Should return original Org when unique name is sent.", true, orgs.Count > 0);

			query = Matcher.PatternMatchFilter("aaTest Org", "AUSYD");
			orgs = new OrgPatternMatchCollection(Factory, query);
			orgs.Load();
			AssertEquals("Should return original Org when unique name is sent.", true, orgs.Count > 0);

			query = Matcher.PatternMatchFilter("", "AUSYD");
			orgs = new OrgPatternMatchCollection(Factory, query);
			orgs.Load();
			AssertEquals("Should return original Org at least when unique name's Port is sent.", true, orgs.Count > 0);
		}

		#endregion

		#endregion

		#region Implementation

		BusinessObjectFactory Factory;
		OrgMatcher Matcher;
		OrgHeader OrgWithOverrides;
		OrgHeader TestOrg;

		protected override void SetUp()
		{
			base.SetUp();
			Factory = new BusinessObjectFactory();
			Matcher = new OrgMatcher(Factory);
			OrgWithOverrides = Factory.New<OrgHeader>();
			TestOrg = Factory.New<OrgHeader>();
			TestOrg.OH_Code = "CODE";
			TestOrg.OH_RL_NKClosestPort = "AUSYD";
			TestOrg.OH_FullName = "FullName";
			TestOrg.MainAddress.OA_Address1 = "Address1";
			TestOrg.MainAddress.OA_City = "City";
		}

		void AssertOrgMatch(string orgName, OrgMatchType expectedMatchType, OrgHeader expectedOrg)
		{
			OrgMatchResult match = Matcher.MatchOrgName(OrgWithOverrides, orgName);
			AssertEquals("Organisation match type for " + orgName, expectedMatchType, match.MatchType);
			AssertEquals("Matched organisation for " + orgName, expectedOrg, match.MatchingOrg);
		}

		void AssertOrgMatchWithoutOverrides(string orgName, string port, OrgMatchType expectedMatchType, OrgHeader expectedOrg)
		{
			OrgMatchResult match = Matcher.MatchOrgWithoutOverrides(orgName, port);
			AssertEquals("Organisation match type for " + orgName, expectedMatchType, match.MatchType);
			AssertEquals("Matched organisation for " + orgName, expectedOrg, match.MatchingOrg);

			if (port.Length > 0 && match.MatchingOrg != null)
			{
				AssertEquals("Port for " + orgName, port, match.MatchingOrg.OH_RL_NKClosestPort);
			}
		}

		void SetMatchToUNMATCHEDRegistryItem(bool allowUnmatched)
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = allowUnmatched;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
		}

		#endregion
	}
}
