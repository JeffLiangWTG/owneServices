using System.ComponentModel;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SimilarOrgMatchForApprovalTest : TestCaseWithDummyOrgMatchApproval
	{
		public void TestConstructor()
		{
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			SimilarOrgMatchForApproval similarOrg = SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch);

			AssertEquals("MatchApproval set correctly", DummyMatchApproval.PK, similarOrg.MatchApproval.PK);
			AssertEquals("OrgPatternMatch set correctly", orgPatternMatch.PK, similarOrg.OrgPatternMatch.PK);
		}

		#region New Bound Properties

		public void TestClosestPortCode()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			orgPatternMatch.OS_OH = organisation.PK;
			SimilarOrgMatchForApproval similarOrg = SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch);

			AssertEquals("ClosestPortCode with no UNLOCO", "", similarOrg.ClosestPortCode);
			organisation.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("ClosestPortCode with populated UNLOCO", "AUSYD", similarOrg.ClosestPortCode);
		}

		public void TestMatchedByUserInitials()
		{
			OrgHeader similarOrg1 = CreateSimilarOrgMatch();
			OrgHeader similarOrg2 = CreateSimilarOrgMatch();
			OrgHeader similarOrg3 = CreateSimilarOrgMatch();
			Factory.Save();

			DummyMatchApproval.Match(similarOrg1.PK, "XXX");
			DummyMatchApproval.Match(similarOrg2.PK, "YYY");

			DummyMatchApproval.SimilarOrgMatchesSortedByRank.Sort("MatchedByUserInitials", ListSortDirection.Ascending);
			AssertEquals("There should be 3 similar org matches for the test", 3, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);

			AssertEquals("When no user has matched", true, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].MatchedByUserInitials.IsEmpty);
			AssertEquals("Match user 1", "XXX", DummyMatchApproval.SimilarOrgMatchesSortedByRank[1].MatchedByUserInitials);
			AssertEquals("Match user 2", "YYY", DummyMatchApproval.SimilarOrgMatchesSortedByRank[2].MatchedByUserInitials);
		}

		public void TestMatchedByUserFullName()
		{
			OrgHeader similarOrg = CreateSimilarOrgMatch();
			Factory.Save();

			AssertEquals("Should be no user matched before we perform the match", "", DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].MatchedByUserFullName);
			DummyMatchApproval.Match(similarOrg.PK); // match by the current user
			AssertEquals("Should return the full name of the user that performed the match", Env.CurrentUser.FullName, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].MatchedByUserFullName);
		}

		#endregion

		public void TestAddresses()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			orgPatternMatch.OS_OH = organisation.PK;
			SimilarOrgMatchForApproval similarOrgMatch = SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch);

			AssertNotNull("Addresses should return something", similarOrgMatch.Addresses);
			AssertEquals("Addresses should be read-only", true, similarOrgMatch.Addresses.ReadOnly);
		}

		public void TestSchema()
		{
			AssertEquals("OwnerCode", SimilarOrgMatchForApproval.Schema.OwnerCode);
		}

		public void TestDelegatedConstruction()
		{
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			AssertEquals(typeof(SimilarOrgMatchForApproval), SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch).GetType());

			OverrideSimilarOrgMatchForApproval.RegisterThisSubTypeOverride();
			AssertEquals(typeof(OverrideSimilarOrgMatchForApproval), SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch).GetType());

			OverrideSimilarOrgMatchForApproval.UnregisterThisSubTypeOverride();
			AssertEquals(typeof(SimilarOrgMatchForApproval), SimilarOrgMatchForApproval.New(DummyMatchApproval, orgPatternMatch).GetType());
		}

		#region ClassTestDelegation

		class OverrideSimilarOrgMatchForApproval : SimilarOrgMatchForApproval
		{
			protected OverrideSimilarOrgMatchForApproval(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch) : base(matchApproval, patternMatch)
			{
			}

			public new static SimilarOrgMatchForApproval New(OrgMatchApproval matchApproval, OrgPatternMatch patternMatch)
			{
				return new OverrideSimilarOrgMatchForApproval(matchApproval, patternMatch);
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			public static void UnregisterThisSubTypeOverride()
			{
				OverridableNewDelegate.ResetValue();
			}
		}

		#endregion
	}
}
