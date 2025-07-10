using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing
{
	[TestedType(typeof(CommunicationContactRowBusinessObjectCollection))]
	sealed class CommunicationContactRowBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CommunicationContactRowBusinessObjectCollection>
	{
		protected override CommunicationContactRowBusinessObjectCollection GetCollectionToTest()
			=> new CommunicationContactRowBusinessObjectCollection(Factory, RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio"));

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Manager, "dummy@email.com"));

		public void TestLoadingCollection_PrepopulatesCandidate_AsFirstRow()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";

			candidate.Application.HP_GS_NKAssignedTo = staff.GS_Code;

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection should prepopulate candidate as the first row", () =>
			{
				AssertEquals(CommunicationContactPositions.Candidate.Description, collection[0].Contact.Position);
				AssertNullOrEmpty(collection[0].Contact.Staff);
				AssertEquals("ben@jam.in", collection[0].Contact.Email);
			});
		}

		public void TestLoadingCollection_PrepopulatesAssignedRecruiter_AsSecondRow()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";

			candidate.Application.HP_GS_NKAssignedTo = staff.GS_Code;

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection should prepopulate assigned recruiter as the second row", () =>
			{
				AssertEquals(CommunicationContactPositions.Recruiter.Description, collection[1].Contact.Position);
				AssertEquals("DUK", collection[1].Contact.Staff);
				AssertEquals("donald@quack.com", collection[1].Contact.Email);
			});
		}

		public void TestLoadingCollection_DoesntPrepopulateCandidate_IfTheyHaveNoEmail()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: string.Empty);

			var collection = candidate.CommunicationContactRows;

			AssertEquals("Loading collection should not prepopulate candidate if the candidate has no email address", 0, collection.Count);
		}

		public void TestLoadingCollection_DoesntPrepopulateAssignedRecruiter_IfTheyHaveNoEmail()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";

			candidate.Application.HP_GS_NKAssignedTo = staff.GS_Code;

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection should not prepopulate assigned recruiter if the recruiter has no email address", () =>
			{
				AssertEquals("Collection should only have 1 element", 1, collection.Count);
				AssertEquals("Collection should only contain the candidate", CommunicationContactPositions.Candidate.Description, collection[0].Contact.Position);
			});
		}

		public void TestLoadingCollection_PrepopulatesBoth_WhenPossible()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DUK";
			staff.GS_FullName = "Donald Duck";
			staff.GS_EmailAddress = "donald@quack.com";

			candidate.Application.HP_GS_NKAssignedTo = staff.GS_Code;

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection should prepopulate assigned both candidate and recruiter when possible", () =>
			{
				AssertEquals(CommunicationContactPositions.Candidate.Description, collection[0].Contact.Position);
				AssertNullOrEmpty(collection[0].Contact.Staff);
				AssertEquals("ben@jam.in", collection[0].Contact.Email);

				AssertEquals(CommunicationContactPositions.Recruiter.Description, collection[1].Contact.Position);
				AssertEquals("DUK", collection[1].Contact.Staff);
				AssertEquals("donald@quack.com", collection[1].Contact.Email);

				AssertEquals(2, collection.Count);
			});
		}

		public void TestLoadingCollection_WithoutAssignedRecruiter_JustPrepopulatesCandidate()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection without assigned recruiter just prepopulates candidate", () =>
			{
				AssertEquals(CommunicationContactPositions.Candidate.Description, collection[0].Contact.Position);
				AssertNullOrEmpty(collection[0].Contact.Staff);
				AssertEquals("ben@jam.in", collection[0].Contact.Email);

				AssertEquals(1, collection.Count);
			});
		}

		public void TestLoadingCollection_WithoutCandidateEmail_JustPrepopulatesRecruiter()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio");
			candidate.Applicant.HA_EmailAddress = ZString.Empty;

			var recruiter = Factory.NewWithValidTestData<GlbStaff>();
			recruiter.GS_Code = "DUK";
			recruiter.GS_FullName = "Donald Duck";
			recruiter.GS_EmailAddress = "donald@quack.com";
			candidate.Application.HP_GS_NKAssignedTo = recruiter.GS_Code;

			var collection = candidate.CommunicationContactRows;

			CombineAssertions("Loading collection without candidate email just prepopulates recruiter", () =>
			{
				AssertEquals(CommunicationContactPositions.Recruiter.Description, collection[0].Contact.Position);
				AssertEquals("DUK", collection[0].Contact.Staff);
				AssertEquals("donald@quack.com", collection[0].Contact.Email);

				AssertEquals(1, collection.Count);
			});
		}

		public void TestLoadingCollection_WithoutCandidateOrRecruiter()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio");
			candidate.Applicant.HA_EmailAddress = ZString.Empty;
			var collection = candidate.CommunicationContactRows;

			AssertEquals("Loading collection without candidate email or assigned recruiter should return empty collection", 0, collection.Count);
		}

		public void TestReadOnlyCollection_WhenCandidateIsNullBusinessObject()
		{
			var candidate = new Candidate(Factory.GetNull<HRJobApplication>()) { IsNull = true };
			var collection = candidate.CommunicationContactRows;

			AssertEquals("Collection should be read only when candidate is null business object", true, collection.ReadOnly);
		}

		public void TestLoadingCollection_PreloadsPastConversationParticipants()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_EmailAddress = "iama@staffmemb.er";

			var conversation1 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			var participant1 = conversation1.Participants.AddNewParticipant("iama@referen.ce");
			var participant2 = conversation1.Participants.AddNewParticipant(staffMember1);

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_EmailAddress = "alsoa@staffmemb.er";

			var conversation2 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);

			//Where would the position be?
			var participant3 = conversation2.Participants.AddNewParticipant("alsoa@referen.ce");
			var participant4 = conversation2.Participants.AddNewParticipant(staffMember2);

			candidate.Factory.Save();
			var collection = candidate.CommunicationContactRows;

			var collectionEmails = new List<string>();
			foreach (CommunicationContactRow row in collection)
			{
				collectionEmails.Add(row.Contact.Email);
			}

			CombineAssertions("Loading collection should preload past conversation participants", () =>
			{
				AssertEquals("Collection should have 5 items (candidate + 4 past conversation participants)", 5, collection.Count);
				AssertEquals("Emails from collection should have 5 items (candidate + 4 past conversation participants)", 5, collectionEmails.Count);
				Assert(collectionEmails.Contains("ben@jam.in"));
				Assert(collectionEmails.Contains("alsoa@referen.ce"));
				Assert(collectionEmails.Contains("alsoa@staffmemb.er"));
				Assert(collectionEmails.Contains("iama@referen.ce"));
				Assert(collectionEmails.Contains("iama@staffmemb.er"));
			});
		}

		public void TestLoadingCollection_PreloadsPastConversationParticipants_Position()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_EmailAddress = "iama@staffmemb.er";

			var conversation1 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			var participant1 = conversation1.Participants.AddNewParticipant("iama@referen.ce");
			var participant2 = conversation1.Participants.AddNewParticipant(staffMember1);
			participant2.JCP_Relation = "Manager";

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_EmailAddress = "alsoa@staffmemb.er";

			var conversation2 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);

			var participant3 = conversation2.Participants.AddNewParticipant("alsoa@referen.ce");
			var participant4 = conversation2.Participants.AddNewParticipant(staffMember2);
			participant4.JCP_Relation = "Team Lead";

			candidate.Factory.Save();
			var collection = candidate.CommunicationContactRows;

			var positions = new List<string>();
			foreach (CommunicationContactRow row in collection)
			{
				positions.Add(row.Contact.Position);
			}

			CombineAssertions("Loading collection should preload past conversation participants", () =>
			{
				AssertEquals("Collection should have 5 items (candidate + 4 past conversation participants)", 5, collection.Count);
				AssertEquals("Position counts", 5, positions.Count);
				Assert(positions.Contains("Candidate"));
				Assert(positions.Contains("Team Lead"));
				Assert(positions.Contains("Manager"));
				AssertEquals(2, positions.FindAll(x => x.Contains("Other Position")).Count);
			});
		}

		public void TestLoadingCollection_PreloadsPastConversationParticipants_RemovingDuplicates()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staffMember = Factory.NewWithValidTestData<GlbStaff>();
			staffMember.GS_EmailAddress = "iama@staffmemb.er";

			var conversation1 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			conversation1.Participants.AddNewParticipant("iama@referen.ce");
			conversation1.Participants.AddNewParticipant(staffMember);

			var conversation2 = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);
			conversation2.Participants.AddNewParticipant("iama@referen.ce");
			conversation2.Participants.AddNewParticipant(staffMember);

			candidate.Factory.Save();
			var collection = candidate.CommunicationContactRows;

			var collectionEmails = new List<string>();
			foreach (CommunicationContactRow row in collection)
			{
				collectionEmails.Add(row.Contact.Email);
			}

			CombineAssertions("Loading collection should preload past conversation participants, removing duplicates", () =>
			{
				AssertEquals("Collection should have 3 items (candidate + 2 past conversation participants)", 3, collection.Count);
				AssertEquals("Emails from collection should have 3 items (candidate + 2 past conversation participants)", 3, collectionEmails.Count);
				Assert(collectionEmails.Contains("ben@jam.in"));
				Assert(collectionEmails.Contains("iama@referen.ce"));
				Assert(collectionEmails.Contains("iama@staffmemb.er"));
			});
		}

		public void TestLoadingCollection_PreloadsPastConversationParticipants_RemovingStaffWithoutEmails()
		{
			var candidate = RecruitmentDataHelpers.CreateCandidate(Factory, "Ben Shaprio", email: "ben@jam.in");

			var staffMember = Factory.NewWithValidTestData<GlbStaff>();
			staffMember.GS_EmailAddress = "iama@staffmemb.er";

			var conversation = JobConversation.CreateWithoutCheckingForExistingConversation(candidate.Application, candidate.Factory);

			var referenceParticipant = conversation.Participants.AddNewParticipant("iama@referen.ce");
			referenceParticipant.JCP_Relation = CommunicationContactPositions.Reference.Description;

			var staffParticipant = conversation.Participants.AddNewParticipant(staffMember);
			staffParticipant.JCP_Relation = CommunicationContactPositions.TeamLead.Description;

			candidate.Factory.Save();

			staffMember.GS_EmailAddress = string.Empty;
			candidate.Factory.Save();

			var collection = candidate.CommunicationContactRows;

			var collectionEmails = new List<string>();
			foreach (CommunicationContactRow row in collection)
			{
				collectionEmails.Add(row.Contact.Email);
			}

			CombineAssertions("Loading collection should preload past conversation participants, removing staff members who don't have emails", () =>
			{
				AssertEquals("Collection should have 2 items (candidate + 1 past conversation participants)", 2, collection.Count);
				AssertEquals("Emails from collection should have 2 items (candidate + 1 past conversation participants)", 2, collectionEmails.Count);
				Assert(collectionEmails.Contains("ben@jam.in"));
				Assert(collectionEmails.Contains("iama@referen.ce"));
			});
		}
	}
}
