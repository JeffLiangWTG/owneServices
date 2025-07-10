using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgMatchApproval))]
	sealed class OrgMatchApprovalTest : EnterpriseBusinessObjectTestCase
	{
		#region TestDeleteRelatedMatchApprovals

		public void TestDeleteRelatedMatchApprovals()
		{
			AssertEquals("OrgMatchApproval should not be deleted yet for the test", false, DummyMatchApproval.IsDeleted);
			OrgMatchApproval.DeleteRelatedMatchApprovalsAndAddresses(DummyParent);
			AssertEquals("OrgMatchApproval should be deleted now", true, DummyMatchApproval.IsDeleted);
		}

		#endregion

		#region Business Object Overrides

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName correct", (NoResString)"Organization Match Approval", DummyMatchApproval.HumanReadableName);
		}

		#endregion

		#region Bound Organisation Properties

		public void TestOwnerCode()
		{
			TestSettableBoundOrganisationProperty("OwnerCode", "OwnerCode");
		}

		public void TestCompanyName()
		{
			TestSettableBoundOrganisationProperty("CompanyName", "Organisation CompanyName");
		}

		public void TestStreet()
		{
			TestSettableBoundOrganisationProperty("Street", "Street");
		}

		public void TestStreet2()
		{
			TestSettableBoundOrganisationProperty("Street2", "Street2");
		}

		public void TestCity()
		{
			TestSettableBoundOrganisationProperty("City", "City");
		}

		public void TestState()
		{
			TestSettableBoundOrganisationProperty("State", "State");
		}

		public void TestPostCode()
		{
			TestSettableBoundOrganisationProperty("PostCode", "PostCode");
		}

		public void TestPhone()
		{
			TestSettableBoundOrganisationProperty("Phone", "Phone");
		}

		public void TestFax()
		{
			TestSettableBoundOrganisationProperty("Fax", "Fax");
		}

		void TestSettableBoundOrganisationProperty(string propertyName, string expectedInitialValue)
		{
			AssertEquals("Initial value of " + propertyName, expectedInitialValue, DummyMatchApproval[propertyName]);

			SimilarOrgMatchForApprovalCollection similarOrgMatches = DummyMatchApproval.SimilarOrgMatchesSortedByRank;
			AssertEquals("Similar org match collection should be cached for the test", true, DummyMatchApproval.SimilarOrgMatchesSortedByRank == similarOrgMatches);

			DummyMatchApproval[propertyName] = "splat";
			AssertEquals(propertyName + " should be set correctly", "splat", DummyMatchApproval[propertyName]);
			AssertEquals("Similar org match collection cache should be invalidated when an org detail property has changed", true, DummyMatchApproval.SimilarOrgMatchesSortedByRank != similarOrgMatches);
		}

		public void TestMasterBill()
		{
			AssertEquals("MasterBill correct", "MasterBill", DummyMatchApproval.MasterBill);
		}

		#endregion

		#region Match Org / User Name Bound Properties

		public void TestP2_MatchUserFullName1()
		{
			AssertEquals("No user has performed any matching yet", "-", DummyMatchApproval.P2_MatchUserFullName1);
			DummyMatchApproval.Match(TestOrganisation.PK);
			AssertEquals("User 1 has performed a match", Env.CurrentUser.FullName, DummyMatchApproval.P2_MatchUserFullName1);
		}

		public void TestP2_MatchUserFullName2()
		{
			AssertEquals("No user has performed any matching yet", "-", DummyMatchApproval.P2_MatchUserFullName2);
			DummyMatchApproval.Match(TestOrganisation.PK, "OP1");
			AssertEquals("Second user has not performed any matching yet", "-", DummyMatchApproval.P2_MatchUserFullName2);
			DummyMatchApproval.Match(TestOrganisation.PK);
			AssertEquals("User 2 has performed a match", Env.CurrentUser.FullName, DummyMatchApproval.P2_MatchUserFullName2);
		}

		public void TestP2_MatchOrgCode1()
		{
			TestOrganisation.OH_Code = "SPLATY";

			AssertEquals("No match has been performed by user 1 yet", "-", DummyMatchApproval.P2_MatchOrgCode1);
			DummyMatchApproval.NotifyNoMatchFound("OP2");
			AssertEquals("No suitable matches by match user 1", "-None Found-", DummyMatchApproval.P2_MatchOrgCode1);
			DummyMatchApproval.Match(TestOrganisation.PK, "OP2");
			AssertEquals("Match made by match user 1", "SPLATY", DummyMatchApproval.P2_MatchOrgCode1);
		}

		public void TestP2_MatchOrgCode2()
		{
			TestOrganisation.OH_Code = "SPLATY";

			DummyMatchApproval.Match(TestOrganisation.PK, "OP1");
			AssertEquals("No match has not been performed by user 2 yet", "-", DummyMatchApproval.P2_MatchOrgCode2);
			DummyMatchApproval.NotifyNoMatchFound("OP2");
			AssertEquals("No suitable matches by match user 2", "-None Found-", DummyMatchApproval.P2_MatchOrgCode2);
			DummyMatchApproval.P2_MatchUser2 = "";
			DummyMatchApproval.Match(TestOrganisation.PK, "OP2");
			AssertEquals("Match made by match user 2", "SPLATY", DummyMatchApproval.P2_MatchOrgCode2);
		}

		#region Match

		public void TestMatch()
		{
			DummyMatchApproval.Match(TestOrganisation.PK);

			AssertEquals("First match should be recorded", TestOrganisation.PK, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("First match should be recorded for the current user", GlbStaff.CurrentUser.GS_Code, DummyMatchApproval.P2_MatchUser1);
			AssertEquals("No second match made yet", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);
			AssertEquals("No second match made yet", ZString.Empty, DummyMatchApproval.P2_MatchUser2);

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP2"))
			{
				DummyMatchApproval.Match(TestOrganisation.PK);
				AssertEquals("Second match should be recorded", TestOrganisation.PK, DummyMatchApproval.P2_OH_MatchOrg2);
				AssertEquals("Second match should be recorded for the current user", "OP2", DummyMatchApproval.P2_MatchUser2);
			}
		}

		public void TestSameUserCantApproveTwice()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			DummyMatchApproval.Match(organisation1.PK);
			AssertEquals("The match should be recorded successfully", organisation1.PK, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("The match should not be recorded twice", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);
			DummyMatchApproval.Match(organisation1.PK);
			AssertEquals("The match should still be recorded", organisation1.PK, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("The match should not be recorded twice for the same user", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);
			DummyMatchApproval.Match(organisation2.PK);
			AssertEquals("The same match should be overwritten if the match is for the same user", organisation2.PK, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("The match should not be recorded twice for the same user", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);

			DummyMatchApproval.Match(organisation1.PK, "2ND");
			AssertEquals("A second match should be recorded for a different user", organisation1.PK, DummyMatchApproval.P2_OH_MatchOrg2);
			DummyMatchApproval.P2_MatchUser1 = "";
			DummyMatchApproval.Match(organisation2.PK, "2ND");
			AssertEquals("The same second match should be overwritten if it is for the same user", organisation2.PK, DummyMatchApproval.P2_OH_MatchOrg2);
		}

		public void TestCantMatchAfterMatchAlreadyMadeWithoutConflict()
		{
			DummyMatchApproval.Match(TestOrganisation.PK, "OP1");
			DummyMatchApproval.Match(TestOrganisation.PK, "OP2");
			try
			{
				DummyMatchApproval.Match(TestOrganisation.PK, "OP1");
				Fail("Expected an exception as 2 operators have made a match without conflict");
			}
			catch (OrgMatchApproval.MatchingException)
			{
				Assert("Got the exception I expected", true);
			}
		}

		public void TestCantMatchAfterMatchAlreadyMadeByOtherUsers()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			DummyMatchApproval.Match(organisation1.PK, "OP1");
			DummyMatchApproval.Match(organisation2.PK, "OP2");

			try
			{
				DummyMatchApproval.Match(organisation1.PK, "OPX");
				Fail("Expected an exception as 2 other operators have already made a match");
			}
			catch (OrgMatchApproval.MatchingException)
			{
				Assert("Got the exception I expected", true);
			}
		}

		public void TestMatchAndSaveAtomically()
		{
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgMatchApproval matchApprovalInFactory1 = (OrgMatchApproval)factory1.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			OrgMatchApproval matchApprovalInFactory2 = (OrgMatchApproval)factory2.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
			{
				matchApprovalInFactory1.MatchAndSaveAtomically(TestOrganisation.PK);
			}
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP2"))
			{
				matchApprovalInFactory2.MatchAndSaveAtomically(TestOrganisation.PK);
			}

			AssertEquals("Match should be approved", true, matchApprovalInFactory2.IsApproved);

			try
			{
				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
				{
					matchApprovalInFactory1.MatchAndSaveAtomically(TestOrganisation.PK);
				}
				Fail("Expected an exception due to the match being already approved");
			}
			catch (OrgMatchApproval.MatchingException)
			{
			}
		}

		[ExpectNoExceptions]
		public void TestMatchAndSaveAtomically_NoConcurrencyFailure()
		{
			Factory.Save();
			DummyParent.Z0_Description = "changed"; // change parent to HasChanges=true

			DummyOrgMatchApprovalSimulateConcurrencyProblem approvalWithSimulatedConcurrencyProblem =
				(DummyOrgMatchApprovalSimulateConcurrencyProblem)Factory.Load(typeof(DummyOrgMatchApprovalSimulateConcurrencyProblem), DummyMatchApproval.PK);
			approvalWithSimulatedConcurrencyProblem.MatchAndSaveAtomically(TestOrganisation.PK);
		}

		public void TestMatchAndSaveAtomically_AfterException()
		{
			Factory.Save();
			ThrowOnSaveBusinessObjectFactory throwOnSaveFactory = new ThrowOnSaveBusinessObjectFactory();
			OrgMatchApproval dummyMatchApprovalInThrowFactory = (OrgMatchApproval)throwOnSaveFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			try
			{
				dummyMatchApprovalInThrowFactory.MatchAndSaveAtomically(TestOrganisation.PK);
				Fail("Expected an exception due to the match being already approved");
			}
			catch
			{
			}
			AssertEquals("OrgMatchApproval should HasChanges=false after a save error so the form doesnt ask u to save", false, dummyMatchApprovalInThrowFactory.HasChanges);
		}

		#endregion

		#region NotifyNoMatchFound

		public void TestNotifyNoMatchFound()
		{
			DummyMatchApproval.NotifyNoMatchFound("OP1");
			AssertEquals("Should mark the first match operator", "OP1", DummyMatchApproval.P2_MatchUser1);
			AssertEquals("Should not mark any organisation", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("Should not mark the user a second time", "", DummyMatchApproval.P2_MatchUser2);
			AssertEquals("Should not mark any organisation", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);

			DummyMatchApproval.NotifyNoMatchFound("OP1");
			AssertEquals("Should not mark the first user a second time if already marked", "OP1", DummyMatchApproval.P2_MatchUser1);
			AssertEquals("Should not mark any organisation", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("Should not mark the user a second time", "", DummyMatchApproval.P2_MatchUser2);
			AssertEquals("Should not mark any organisation", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);

			DummyMatchApproval.NotifyNoMatchFound(); // current user
			AssertEquals("Should mark the second match operator", GlbStaff.CurrentUser.GS_Code, DummyMatchApproval.P2_MatchUser2);
			AssertEquals("Should not mark any organisation", ZGuid.Empty, DummyMatchApproval.P2_OH_MatchOrg2);
		}

		public void TestCantNotifyNoMatchFoundAfter2MatchesAlreadyMade()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			DummyMatchApproval.NotifyNoMatchFound("OP1");
			DummyMatchApproval.NotifyNoMatchFound("OP2");

			try
			{
				DummyMatchApproval.NotifyNoMatchFound("OP1");
				Fail("Expected an exception as 2 other operators have already made the match");
			}
			catch (OrgMatchApproval.MatchingException)
			{
				Assert("Got the exception I expected", true);
			}
		}

		public void TestNotifyNoMatchFoundAndSaveAtomically()
		{
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = true;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = true;

			OrgMatchApproval matchApprovalInFactory1 = (OrgMatchApproval)factory1.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			OrgMatchApproval matchApprovalInFactory2 = (OrgMatchApproval)factory2.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
			{
				matchApprovalInFactory1.NotifyNoMatchFoundAndSaveAtomically();
			}
			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP2"))
			{
				matchApprovalInFactory2.NotifyNoMatchFoundAndSaveAtomically();
			}

			AssertEquals("Unmatch should be logged by both users and no concurrency exception should have been thrown above", false, matchApprovalInFactory2.P2_MatchUser1.IsEmpty);
			AssertEquals("Unmatch should be logged by both users and no concurrency exception should have been thrown above", false, matchApprovalInFactory2.P2_MatchUser2.IsEmpty);
		}

		[ExpectNoExceptions]
		public void TestNotifyNoMatchFoundAndSaveAtomically_NoConcurrencyFailure()
		{
			Factory.Save();
			DummyParent.Z0_Description = "changed"; // modify parent

			DummyOrgMatchApprovalSimulateConcurrencyProblem approvalWithSimulatedConcurrencyProblem =
				(DummyOrgMatchApprovalSimulateConcurrencyProblem)Factory.Load(typeof(DummyOrgMatchApprovalSimulateConcurrencyProblem), DummyMatchApproval.PK);
			approvalWithSimulatedConcurrencyProblem.NotifyNoMatchFoundAndSaveAtomically();
		}

		public void TestNotifyNoMatchFoundAndSaveAtomically_AfterException()
		{
			Factory.Save();
			ThrowOnSaveBusinessObjectFactory throwOnSaveFactory = new ThrowOnSaveBusinessObjectFactory();
			OrgMatchApproval dummyMatchApprovalInThrowFactory = (OrgMatchApproval)throwOnSaveFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			try
			{
				dummyMatchApprovalInThrowFactory.NotifyNoMatchFoundAndSaveAtomically();
				Fail("Expected an exception due to the match being already approved");
			}
			catch
			{
			}
			AssertEquals("OrgMatchApproval should HasChanges=false after a save error so the form doesnt ask u to save", false, dummyMatchApprovalInThrowFactory.HasChanges);
		}

		#endregion

		#region ApproveMatchBySupervisor

		public void TestApproveMatchBySupervisor()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			DummyMatchApproval.ApproveMatchBySupervisor(organisation);

			AssertEquals("Organisation should be completely matched; supervisor is chief", organisation.PK, DummyMatchApproval.P2_OH_MatchOrg1);
			AssertEquals("Organisation is matched by the current user who is the supervisor", GlbStaff.CurrentUser.GS_Code, DummyMatchApproval.P2_MatchUser1);
			AssertEquals("Organisation should be completely matched; supervisor is chief", organisation.PK, DummyMatchApproval.P2_OH_MatchOrg2);
			AssertEquals("Organisation is matched by the current user who is the supervisor", GlbStaff.CurrentUser.GS_Code, DummyMatchApproval.P2_MatchUser2);
		}

		public void TestApproveMatchBySupervisor_CanApproveIfMatchConflicting()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			DummyMatchApproval.Match(organisation1.PK);
			DummyMatchApproval.Match(organisation2.PK);

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("XXX"))
			{
				DummyMatchApproval.ApproveMatchBySupervisor(organisation2);
			}
			AssertEquals("Organisation should be re-approved successfully", true, DummyMatchApproval.IsApproved);
		}

		public void TestApproveMatchBySupervisorAndSaveAtomically()
		{
			OrgHeader organisation = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			OrgMatchApproval matchApprovalInFactory1 = (OrgMatchApproval)factory1.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			OrgMatchApproval matchApprovalInFactory2 = (OrgMatchApproval)factory2.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);

			using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
			{
				matchApprovalInFactory1.ApproveMatchBySupervisorAndSaveAtomically(organisation);
			}
			AssertEquals("Match should be approved", true, matchApprovalInFactory1.IsApproved);

			matchApprovalInFactory1.P2_OH_MatchOrg1 = ZGuid.Empty;
			matchApprovalInFactory1.P2_MatchUser1 = "";
			matchApprovalInFactory1.P2_OH_MatchOrg2 = ZGuid.Empty;
			matchApprovalInFactory1.P2_MatchUser2 = "";
			matchApprovalInFactory1.Match(organisation.PK, "OP1");
			matchApprovalInFactory1.Match(organisation.PK, "OP2");
			Factory.Save();

			try
			{
				using (CurrentUserInitialsChanger.ChangeCurrentUserInitials("OP1"))
				{
					matchApprovalInFactory2.ApproveMatchBySupervisorAndSaveAtomically(organisation);
				}
				Fail("Expected an exception due to the match already being approved");
			}
			catch (OrgMatchApproval.MatchingException)
			{
			}
		}

		public void TestApproveMatchBySupervisorAndSaveAtomically_AfterException()
		{
			Factory.Save();
			ThrowOnSaveBusinessObjectFactory throwOnSaveFactory = new ThrowOnSaveBusinessObjectFactory();
			OrgMatchApproval dummyMatchApprovalInThrowFactory = (OrgMatchApproval)throwOnSaveFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			try
			{
				dummyMatchApprovalInThrowFactory.ApproveMatchBySupervisorAndSaveAtomically(TestOrganisation);
				Fail("Expected an exception due to the match being already approved");
			}
			catch
			{
			}
			AssertEquals("OrgMatchApproval should HasChanges=false after a save error so the form doesnt ask u to save", false, dummyMatchApprovalInThrowFactory.HasChanges);
		}

		[ExpectNoExceptions]
		public void TestApproveMatchBySupervisorAndSaveAtomically_NoConcurrencyFailure()
		{
			Factory.Save();
			DummyParent.Z0_Description = "changed"; // modify parent

			DummyOrgMatchApprovalSimulateConcurrencyProblem approvalWithSimulatedConcurrencyProblem =
				(DummyOrgMatchApprovalSimulateConcurrencyProblem)Factory.Load(typeof(DummyOrgMatchApprovalSimulateConcurrencyProblem), DummyMatchApproval.PK);
			approvalWithSimulatedConcurrencyProblem.ApproveMatchBySupervisorAndSaveAtomically(TestOrganisation);
		}

		#endregion

		#endregion

		#region P2_MatchStatus

		public void TestP2_MatchStatus()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			AssertEquals(OrgMatchApproval.MatchStatus.Unmatched, DummyMatchApproval.P2_MatchStatus);
			DummyMatchApproval.Match(organisation.PK, "OP1");
			AssertEquals(OrgMatchApproval.MatchStatus.PartiallyMatched, DummyMatchApproval.P2_MatchStatus);
			DummyMatchApproval.Match(organisation.PK, "OP2");
			AssertEquals(OrgMatchApproval.MatchStatus.MatchApproved, DummyMatchApproval.P2_MatchStatus);

			DummyMatchApproval.P2_MatchUser1 = "";
			DummyMatchApproval.P2_OH_MatchOrg1 = ZGuid.Empty;
			DummyMatchApproval.Match(organisation2.PK, "OP1");
			AssertEquals(OrgMatchApproval.MatchStatus.MatchMadeWithConflict, DummyMatchApproval.P2_MatchStatus);

			DummyMatchApproval.P2_MatchUser1 = "";
			DummyMatchApproval.P2_OH_MatchOrg1 = ZGuid.Empty;
			DummyMatchApproval.NotifyNoMatchFound("OP1");
			AssertEquals(OrgMatchApproval.MatchStatus.MatchMadeWithConflict, DummyMatchApproval.P2_MatchStatus);
			DummyMatchApproval.P2_MatchUser2 = "";
			DummyMatchApproval.P2_OH_MatchOrg2 = ZGuid.Empty;
			DummyMatchApproval.NotifyNoMatchFound("OP2");
			AssertEquals(OrgMatchApproval.MatchStatus.NoMatchFound, DummyMatchApproval.P2_MatchStatus);
		}

		public void TestP2_MatchStatus_PartiallyMatchedByYou()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals("Unmatched", DummyMatchApproval.P2_MatchStatus);
			DummyMatchApproval.Match(organisation.PK);
			AssertEquals("Partially matched by you", DummyMatchApproval.P2_MatchStatus);
		}

		public void TestP2_MatchStatus_MatchConflict()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			AssertEquals("Unmatched", DummyMatchApproval.P2_MatchStatus);
			DummyMatchApproval.Match(organisation1.PK);
			AssertEquals("Partially matched by you", DummyMatchApproval.P2_MatchStatus);
		}

		#endregion

		#region IsApproved / ApprovedOrgMatch

		public void TestIsApproved()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals("Match not approved yet", false, DummyMatchApproval.IsApproved);
			DummyMatchApproval.Match(organisation.PK, "OP1");
			AssertEquals("Match not approved yet", false, DummyMatchApproval.IsApproved);
			DummyMatchApproval.Match(organisation.PK, "OP2");
			AssertEquals("Match is agreed on by both operators", true, DummyMatchApproval.IsApproved);
		}

		public void TestApprovedOrgMatch()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals("Match not approved yet", null, DummyMatchApproval.ApprovedOrgMatch);
			DummyMatchApproval.Match(organisation.PK, "OP1");
			AssertEquals("Match not approved yet", null, DummyMatchApproval.ApprovedOrgMatch);
			DummyMatchApproval.Match(organisation.PK, "OP2");
			AssertEquals("Match is agreed on by both operators", organisation.PK, DummyMatchApproval.ApprovedOrgMatch.PK);
		}

		#endregion

		#region SimilarOrgMatchesSortedByRank

		public void TestSimilarOrgMatchesSortedByRank()
		{
			UnmatchedOrganisation org = new UnmatchedOrganisation(Factory);
			org.IsEnabled = false;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);

			OrgHeader similarOrgRank2 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank2.OH_FullName = "Organisation CompanyName ltd";
			similarOrgRank2.MainAddress.OA_Address1 = "Street";
			similarOrgRank2.MainAddress.OA_Address2 = "Street2";
			similarOrgRank2.MainAddress.OA_City = "City";
			similarOrgRank2.MainAddress.OA_State = "State";
			similarOrgRank2.MainAddress.OA_PostCode = "PostCode";
			OrgHeader similarOrgRank1 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank1.OH_FullName = "Organisation CompanyName";
			similarOrgRank1.MainAddress.OA_Address1 = "Street";
			similarOrgRank1.MainAddress.OA_Address2 = "Street2";
			similarOrgRank1.MainAddress.OA_City = "City";
			similarOrgRank1.MainAddress.OA_State = "State";
			similarOrgRank1.MainAddress.OA_PostCode = "PostCode";
			Factory.Save();

			AssertEquals("There should be 2 similar organisations found and loaded", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);
			AssertEquals("Similar organisations should be correctly sorted by rank", similarOrgRank1.PK, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].OrgPatternMatch.OS_OH);
			AssertEquals("Similar organisations should be correctly sorted by rank", 1, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].OrgPatternMatch.OS_Rank);
			AssertEquals("Similar organisations should be correctly sorted by rank", similarOrgRank2.PK, DummyMatchApproval.SimilarOrgMatchesSortedByRank[1].OrgPatternMatch.OS_OH);
			AssertEquals("Similar organisations should be correctly sorted by rank", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank[1].OrgPatternMatch.OS_Rank);

			DummyMatchApproval.CompanyName = "no longer matched";
			AssertEquals("There should be 2 similar organisations found and loaded (Addresses STILL match - rank is irrelevant because addresses are identical)", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);

			DummyMatchApproval.Street = "New Name";
			DummyMatchApproval.Street2 = "New Name";
			DummyMatchApproval.City = "New Name";
			AssertEquals("Similar organisations should be updated to reflect organisation detail changes", 0, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);
		}

		public void TestSimilarOrgMatchesSortedByRank_UpdatedWhenOrganisationSaved()
		{
			OrgHeader similarOrg1 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrg1.OH_FullName = "Organisation CompanyName";
			similarOrg1.MainAddress.OA_Address1 = "Street";
			similarOrg1.MainAddress.OA_Address2 = "Street2";
			similarOrg1.MainAddress.OA_City = "City";
			OrgHeader similarOrg2 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrg2.OH_FullName = "Organisation CompanyName ltd";
			similarOrg2.MainAddress.OA_Address1 = "Street";
			similarOrg2.MainAddress.OA_Address2 = "Street2";
			similarOrg2.MainAddress.OA_City = "City";
			Factory.Save();

			AssertEquals("There should be 2 similar organisations found and loaded", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);

			similarOrg2.OH_FullName = "nomatch";
			similarOrg2.MainAddress.OA_Address1 = "nomatch";
			similarOrg2.MainAddress.OA_City = "nomatch";
			Factory.Save();

			foreach (SimilarOrgMatchForApproval org in DummyMatchApproval.SimilarOrgMatchesSortedByRank)
			{
				AssertEquals("Collection should have been refreshed due to OrgPatternMatch being deleted", false, org.OrgPatternMatch.IsDeleted);
			}
		}

		#endregion

		#region UpdatedWithDataRefresh

		public void TestUpdatedWithDataRefresh()
		{
			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			DummyOrgMatchApproval dummyMatchApprovalInOtherFactory = (DummyOrgMatchApproval)otherFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			dummyMatchApprovalInOtherFactory.P2_Reference = "splaty";

			DummyMatchApproval.UpdatedWithDataRefresh += new EventHandler(OnDummyMatchApproval_UpdatedWithDataRefresh);
			otherFactory.Save();
			AssertEquals("Should fire the UpdatedWithDataRefresh event", true, OnDummyMatchApproval_UpdatedWithDataRefreshCalled);
		}

		bool OnDummyMatchApproval_UpdatedWithDataRefreshCalled;
		void OnDummyMatchApproval_UpdatedWithDataRefresh(object sender, EventArgs e)
		{
			OnDummyMatchApproval_UpdatedWithDataRefreshCalled = true;
		}

		#endregion

		public void TestParent()
		{
			DummyMatchApproval.P2_ParentID = DummyParent.PK;
			AssertEquals("Parent property should retrieve the correct parent", DummyParent.PK, DummyMatchApproval.Parent.PK);
		}

		public void TestIsCurrentUserSupervisor()
		{
			OrgMatchApproval matchApproval = Factory.New<OrgMatchApproval>();
			bool oldOrgMatchApprovalSupervisorAllowed = Env.Security.OrgMatchApproval.IsAllowed;
			try
			{
				Env.Security.OrgMatchApprovalSupervisor.IsAllowed = true;
				AssertEquals("If the security check point is allowed then the user is a match supervisor", true, matchApproval.IsCurrentUserSupervisor);
				Env.Security.OrgMatchApprovalSupervisor.IsAllowed = false;
				AssertEquals("If the security check point is not allowed then the user is not a match supervisor", false, matchApproval.IsCurrentUserSupervisor);
			}
			finally
			{
				Env.Security.OrgMatchApproval.IsAllowed = oldOrgMatchApprovalSupervisorAllowed;
			}
		}

		public void TestIsApprovedByOtherUsers()
		{
			OrgHeader organisation = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			AssertEquals("Not approved yet", false, DummyMatchApproval.IsApprovedByOtherUsers);

			DummyMatchApproval.Match(organisation.PK, "OP1");
			AssertEquals("Not approved yet", false, DummyMatchApproval.IsApprovedByOtherUsers);

			DummyMatchApproval.Match(organisation.PK, "OP2");
			AssertEquals("The match approval has been approved by other users", true, DummyMatchApproval.IsApprovedByOtherUsers);

			DummyMatchApproval.P2_OH_MatchOrg1 = ZGuid.Empty;
			DummyMatchApproval.P2_MatchUser1 = "";
			DummyMatchApproval.Match(organisation.PK, GlbStaff.CurrentUser.GS_Code);
			AssertEquals("The match approval has been matched by me", false, DummyMatchApproval.IsApprovedByOtherUsers);
		}

		public void TestShouldCreateTemporaryOrganisation_WhenBothUsersAgreeOnNoMatch()
		{
			OrgHeader similarOrgRank = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank.OH_FullName = "Organisation CompanyName ltd";
			similarOrgRank.MainAddress.OA_Address1 = "Street";
			similarOrgRank.MainAddress.OA_Address2 = "Street2";
			similarOrgRank.MainAddress.OA_City = "City";
			Factory.Save();

			AssertEquals("Should not create a temporary org yet as no match has been made", false, DummyMatchApproval.ShouldCreateTemporaryOrganisation());
			DummyMatchApproval.NotifyNoMatchFound("OP1");
			AssertEquals("Should not create a temporary org yet as only 1 match user has matched", false, DummyMatchApproval.ShouldCreateTemporaryOrganisation());
			DummyMatchApproval.NotifyNoMatchFound("OP2");
			AssertEquals("Should create a temporary org yet as both match users has matched", true, DummyMatchApproval.ShouldCreateTemporaryOrganisation());
		}

		public void TestShouldCreateTemporaryOrganisation_WhenNoSimilarOrgsFound()
		{
			OrgHeader similarOrgRank = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank.OH_FullName = "Organisation CompanyName ltd";
			similarOrgRank.MainAddress.OA_Address1 = "Street";
			similarOrgRank.MainAddress.OA_Address2 = "Street2";
			similarOrgRank.MainAddress.OA_City = "City";
			Factory.Save();

			AssertEquals("Should not need to create a temporary org as a similar org exists", false, DummyMatchApproval.ShouldCreateTemporaryOrganisation());
			DummyMatchApproval.SimilarOrgMatchesSortedByRank.RemoveAndDeleteAll();
			AssertEquals("Should need to create a temporary org now that there are no similar orgs", true, DummyMatchApproval.ShouldCreateTemporaryOrganisation());
		}

		public void TestCopyDetailsToOrganisation()
		{
			OrgHeader newOrganisation = Factory.New<OrgHeader>();
			DummyMatchApproval.CopyDetailsToOrganisation(newOrganisation);

			RefCountry currentCountry = (RefCountry)Factory.Load(typeof(RefCountry), Env.CurrentCompany.Country.PK);
			AssertEquals("FullName set correctly", "Organisation CompanyName", newOrganisation.OH_FullName);
			AssertEquals("Street set correctly", "Street", newOrganisation.MainAddress.OA_Address1);
			AssertEquals("Street2 set correctly", "Street2", newOrganisation.MainAddress.OA_Address2);
			AssertEquals("City set correctly", "City", newOrganisation.MainAddress.OA_City);
			AssertEquals("State set correctly", "State", newOrganisation.MainAddress.OA_State);
			AssertEquals("PostCode set correctly", "PostCode", newOrganisation.MainAddress.OA_PostCode);
			AssertEquals("Phone set correctly", "Phone", newOrganisation.MainAddress.OA_Phone);
			AssertEquals("Fax set correctly", "Fax", newOrganisation.MainAddress.OA_Fax);
			AssertEquals("UNLoco", "AUSYD", newOrganisation.OH_RL_NKClosestPort);
			AssertEquals("OrgCode", "ORGCOMSYD", newOrganisation.OH_Code);
		}

		[ExpectNoExceptions]
		public void TestCopyDetailsToOrganisation_WithLongFields()
		{
			DummyOrgMatchApprovalWithLongStringAddressDetails dummyWithLongFields = (DummyOrgMatchApprovalWithLongStringAddressDetails)Factory.Load(typeof(DummyOrgMatchApprovalWithLongStringAddressDetails), DummyMatchApproval.PK);
			OrgHeader newOrganisation = Factory.New<OrgHeader>();
			dummyWithLongFields.CopyDetailsToOrganisation(newOrganisation);
		}

		public void TestRegistrationNumberNotCreatedIfOwnerCodeIsEmpty()
		{
			DummyOrgMatchApprovalWithEmptyOwnerCode matchApproval = Factory.New<DummyOrgMatchApprovalWithEmptyOwnerCode>();
			OrgPatternMatchAddress orgPatternMatchAddress = Factory.New<OrgPatternMatchAddress>();
			matchApproval.P2_ParentID = orgPatternMatchAddress.PK;

			AssertEquals("OwnerCode should be empty for this test", "", matchApproval.OwnerCode);
			OrgHeader newOrganisation = Factory.New<OrgHeader>();
			matchApproval.CopyDetailsToOrganisation(newOrganisation);

			RefCountry currentCountry = (RefCountry)Factory.Load(typeof(RefCountry), Env.CurrentCompany.Country.PK);
			AssertEquals("Registration number not created if owner code empty", 0, newOrganisation.CustomsCodes.Count);
		}

		class DummyOrgMatchApprovalWithEmptyOwnerCode : DummyOrgMatchApproval
		{
			public DummyOrgMatchApprovalWithEmptyOwnerCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString ParentOwnerCode
			{
				get { return ""; }
			}
		}

		public void TestOnMatchApproved()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals("Match is not approved yet", false, DummyMatchApproval.OnMatchApprovedCalled);
			DummyMatchApproval.Match(organisation.PK, "OP1");
			AssertEquals("Match is not approved yet", false, DummyMatchApproval.OnMatchApprovedCalled);
			DummyMatchApproval.Match(organisation.PK, "OP2");

			AssertEquals("Match is agreed on by both operators, the foreign key on the OrgPatternMatchAddress should be populated now", false, DummyMatchApproval.AddressToBeMatched.P3_OH_MatchOrg.IsEmpty);
			AssertEquals("The date of successful match should be set", false, DummyMatchApproval.P2_RelatedDateForPatternMatch.IsEmpty);
		}

		public void TestOnMatchApproved_NotCalledIfMatchesConflict()
		{
			OrgHeader organisation1 = Factory.New<OrgHeader>();
			OrgHeader organisation2 = Factory.New<OrgHeader>();

			AssertEquals("Match is not approved yet", false, DummyMatchApproval.OnMatchApprovedCalled);
			DummyMatchApproval.Match(organisation1.PK, "OP1");
			AssertEquals("Match is not approved yet", false, DummyMatchApproval.OnMatchApprovedCalled);
			DummyMatchApproval.Match(organisation2.PK, "OP2");

			AssertEquals("Match is not agreed on by both operators, the foreign key on the OrgPatternMatchAddress should NOT be populated", true, DummyMatchApproval.AddressToBeMatched.P3_OH_MatchOrg.IsEmpty);
			AssertEquals("The date of match should not be set", true, DummyMatchApproval.P2_RelatedDateForPatternMatch.IsEmpty);
		}

		public void TestOnMatchApproved_BySupervisor()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();

			AssertEquals("Match is not approved yet", false, DummyMatchApproval.OnMatchApprovedCalled);
			DummyMatchApproval.ApproveMatchBySupervisor(organisation);
			AssertEquals("Match is approved by the supervisor which overrules all, the method should be called immediately", true, DummyMatchApproval.OnMatchApprovedCalled);
		}

		public void TestTypeDecider()
		{
			DummyBusinessObject parent = Factory.New<DummyBusinessObject>();
			DummyOrgMatchApproval matchApproval = (DummyOrgMatchApproval)Loader.LoadOrCreate(parent.PK, OrgMatchApprovalType.DummyType);

			AssertNotNull("TypeDecider should return the right type of business object", matchApproval);
		}

		public void TestUniqueIndexFailureHandler()
		{
			DummyBusinessObject parent = Factory.New<DummyBusinessObject>();

			DummyOrgMatchApproval matchApproval1 = (DummyOrgMatchApproval)Loader.LoadOrCreate(parent.PK, OrgMatchApprovalType.DummyType);
			matchApproval1.P2_MatchType = "XXX"; // force the create of the second OrgMatchApproval
			matchApproval1.AddressToBeMatched.P3_AddressType = "XXX";
			DummyOrgMatchApproval matchApproval2 = (DummyOrgMatchApproval)Loader.LoadOrCreate(parent.PK, OrgMatchApprovalType.DummyType);
			matchApproval1.P2_ParentID = matchApproval2.P2_ParentID;
			matchApproval1.P2_MatchType = matchApproval2.P2_MatchType;

			AssertEquals("There should be 2 separate objects", true, matchApproval1.PK != matchApproval2.PK);
			AssertEquals("P2_ParentID should be equal so we get a unique index violation", matchApproval1.P2_ParentID, matchApproval2.P2_ParentID);
			AssertEquals("P2_MatchType should be equal so we get a unique index violation", matchApproval1.P2_MatchType, matchApproval2.P2_MatchType);

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				string uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("1 bizo should be involved with the unique constraint violation", 1, ex.BusinessObjects.Length);
				DummyOrgMatchApproval matchApprovalInError = (DummyOrgMatchApproval)ex.BusinessObjects[0];

				AssertEquals("Correct unique constraint should be violated", matchApprovalInError.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single(), uniqueIndexName);
				MockNotificationHandler notifier = new MockNotificationHandler();
				matchApprovalInError.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifier, matchApprovalInError.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single());
				AssertEquals("User should be notified of the situation", "Another user has changed this record", notifier.LastErrorCaption);
				AssertEquals("User should be notified of the situation", "Another user has already made changes to this record. You must re-open this form and re-apply your changes to continue.", notifier.LastErrorMessage);
			}
		}

		class MockNotificationHandler : INotificationHandler
		{
			public string LastErrorMessage;
			public string LastErrorCaption;

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				LastErrorMessage = message;
				LastErrorCaption = caption;
			}

			public void ReportInformation(string message, string caption)
			{
				throw new NotSupportedException();
			}
		}

		public void TestCopyDetailsFromParent()
		{
			DummyOrgMatchApprovalWithLongStringAddressDetails approvalWithParentMappings =
				(DummyOrgMatchApprovalWithLongStringAddressDetails)Factory.Load(typeof(DummyOrgMatchApprovalWithLongStringAddressDetails), DummyMatchApproval.PK);
			approvalWithParentMappings.CopyDetailsFromParent();

			AssertEquals("OwnerCode", true, approvalWithParentMappings.AddressToBeMatched.P3_Code.StartsWith("ParentOwnerCode"));
			AssertEquals("CompanyName", true, approvalWithParentMappings.AddressToBeMatched.P3_CompanyName.StartsWith("ParentCompanyName"));
			AssertEquals("Street", true, approvalWithParentMappings.AddressToBeMatched.P3_Address1.StartsWith("ParentStreet"));
			AssertEquals("Street2", true, approvalWithParentMappings.AddressToBeMatched.P3_Address2.StartsWith("ParentStreet2"));
			AssertEquals("City", true, approvalWithParentMappings.AddressToBeMatched.P3_City.StartsWith("ParentCity"));
			AssertEquals("State", true, approvalWithParentMappings.AddressToBeMatched.P3_State.StartsWith("ParentState"));
			AssertEquals("PostCode", true, approvalWithParentMappings.AddressToBeMatched.P3_PostCode.StartsWith("ParentPost"));
			AssertEquals("Phone", true, approvalWithParentMappings.AddressToBeMatched.P3_Phone.StartsWith("ParentPhone"));
			AssertEquals("Fax", true, approvalWithParentMappings.AddressToBeMatched.P3_Fax.StartsWith("ParentFax"));
		}

		public void TestLinkedOrgPatternMatchAddressNotCreatedTooEarly()
		{
			ZQuery filter = new ZQuery(OrgPatternMatchAddressSchema.P3_ParentID, DummyMatchApproval.PK);
			OrgPatternMatchAddress addressToMatch = (OrgPatternMatchAddress)Factory.LoadTop1(typeof(OrgPatternMatchAddress), filter);
			AssertNull(
				"Linked address should not be created up front, for performance and also so that we can attach our own early on",
				addressToMatch);
		}

		public void TestOrganisationsInSimilarMatchesAreDistinct()
		{
			OrgHeader similarOrgRank2 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank2.OH_FullName = "Organisation CompanyName ltd";
			similarOrgRank2.MainAddress.OA_Address1 = "Street";
			similarOrgRank2.MainAddress.OA_Address2 = "Street2";
			similarOrgRank2.MainAddress.OA_City = "City";

			OrgHeader similarOrgRank1 = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			similarOrgRank1.OH_FullName = "Organisation CompanyName";
			similarOrgRank1.MainAddress.OA_Address1 = "Street";
			similarOrgRank1.MainAddress.OA_Address2 = "Street2";
			similarOrgRank1.MainAddress.OA_City = "City";

			OrgAddress address = similarOrgRank1.Addresses.AddNew();
			address.OA_Address1 = "Street";
			address.OA_Address2 = "Street2";
			address.OA_City = "City";
			Factory.Save();

			AssertEquals("Similar Orgs Should be distinct", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank.Count);
			AssertEquals("Similar organisations should be correctly sorted by rank", similarOrgRank1.PK, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].OrgPatternMatch.OS_OH);
			AssertEquals("Similar organisations should be correctly sorted by rank", 1, DummyMatchApproval.SimilarOrgMatchesSortedByRank[0].OrgPatternMatch.OS_Rank);
			AssertEquals("Similar organisations should be correctly sorted by rank", similarOrgRank2.PK, DummyMatchApproval.SimilarOrgMatchesSortedByRank[1].OrgPatternMatch.OS_OH);
			AssertEquals("Similar organisations should be correctly sorted by rank", 2, DummyMatchApproval.SimilarOrgMatchesSortedByRank[1].OrgPatternMatch.OS_Rank);
		}

		public void TestOrgMatchApprovalLookups()
		{
			AssertEquals(typeof(OrgMatchApprovalLookups), DummyMatchApproval.Lookups.GetType());
		}

		public void TestUNLOCO()
		{
			TestSettableBoundOrganisationProperty("UNLoco", "AUSYD");
		}

		public void TestManuallySelectedOrganisation()
		{
			OrgHeader org = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
			Factory.Save();
			DummyMatchApproval.P2_OH_ManuallySelectedOrganisation = org.PK;
			AssertEquals(true, DummyMatchApproval.IsInDatabase);
			AssertEquals(org.PK.ToString(), DummyMatchApproval.P2_OH_MatchOrg1.ToString());
		}

		#region Test Classes

		class ThrowOnSaveBusinessObjectFactory : BusinessObjectFactory
		{
			protected override IChangedTableNames SaveInTransactionCore()
			{
				throw new InvalidOperationException("Fuse blew");
			}
		}

		class DummyOrgMatchApprovalSimulateConcurrencyProblem : DummyOrgMatchApproval
		{
			public DummyOrgMatchApprovalSimulateConcurrencyProblem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnAfterReloadBeforeMatchApproved(OrgMatchApproval matchApprovalInOtherFactory)
			{
				base.OnAfterReloadBeforeMatchApproved(matchApprovalInOtherFactory);

				BusinessObject parent = matchApprovalInOtherFactory.Parent; // access the parent so that row original values are loaded for the test

				ExecuteNonQuery("UPDATE dbo.DummyBizo SET Z0_Description='concurrent_update'");
				ExecuteNonQuery("UPDATE dbo.OrgMatchApproval SET P2_MatchUser1='XXX', P2_OH_MatchOrg1=(SELECT TOP 1 OH_PK FROM dbo.OrgHeader)");
			}

			protected override void OnConcurrencyException()
			{
				base.OnConcurrencyException();
				// reverse the UPDATE commands executed above for the test - RollbackTransaction is ineffective in a TransactionedTestCase
				ExecuteNonQuery("UPDATE dbo.OrgMatchApproval SET P2_MatchUser1='', P2_OH_MatchOrg1=null");
			}

			void ExecuteNonQuery(string sQL)
			{
				DbCommand command = Db.Connection.Command(sQL);
				command.ExecuteNonQuery();
			}
		}

		class DummyOrgMatchApprovalWithLongStringAddressDetails : DummyOrgMatchApproval
		{
			public DummyOrgMatchApprovalWithLongStringAddressDetails(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString ParentOwnerCode
			{
				get { return ConstructVeryLongString("ParentOwnerCode"); }
			}

			protected override ZString ParentCompanyName
			{
				get { return ConstructVeryLongString("ParentCompanyName"); }
			}

			protected override ZString ParentStreet
			{
				get { return ConstructVeryLongString("ParentStreet"); }
			}

			protected override ZString ParentStreet2
			{
				get { return ConstructVeryLongString("ParentStreet2"); }
			}

			protected override ZString ParentCity
			{
				get { return ConstructVeryLongString("ParentCity"); }
			}

			protected override ZString ParentState
			{
				get { return ConstructVeryLongString("ParentState"); }
			}

			protected override ZString ParentPostCode
			{
				get { return ConstructVeryLongString("ParentPostCode"); }
			}

			protected override ZString ParentPhone
			{
				get { return ConstructVeryLongString("ParentPhone"); }
			}

			protected override ZString ParentFax
			{
				get { return ConstructVeryLongString("ParentFax"); }
			}

			string ConstructVeryLongString(string repeatingContent)
			{
				string result = "";
				for (int i = 0; i < 20; i++)
				{
					result += repeatingContent;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			DummyOrgMatchApproval dummyMatchApproval = (DummyOrgMatchApproval)Loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);
			return dummyMatchApproval;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyParent = Factory.New<DummyBusinessObject>();
			Loader = new OrgMatchApproval.Loader(Factory);
			DummyMatchApproval = (DummyOrgMatchApproval)Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			TestOrganisation = (OrgHeader)Factory.NewWithValidTestData(typeof(OrgHeader));
		}

		DummyBusinessObject DummyParent;
		DummyOrgMatchApproval DummyMatchApproval;
		OrgMatchApproval.Loader Loader;
		OrgHeader TestOrganisation;

		#endregion
	}
}
