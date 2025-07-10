using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

using static Enterprise.Recruitment.Testing.RecruitmentDataHelpers;

namespace Enterprise.Recruitment.Testing.Module
{
	sealed class PeopleAndCommunicationControlTest : TestCaseWithFactory
	{
		Form GetFormToBash(out PeopleAndCommunicationControl control) => GetFormToBash<PeopleAndCommunicationControl>(out control);

		Form GetFormToBash<T>(out T control) where T : Control, new()
		{
			var form = new ZChildForm();
			form.Controls.Add(control = new T { Dock = DockStyle.Fill });
			return form;
		}

		Candidate TestCandidate => CreateCandidate(Factory, "Chris Kes", email: "chris@k.es");

		public void TestControlAndGrid_BindingAndRebinding()
		{
			var vadim = CreateCandidate(Factory, "Vadims", email: "vad@im.s");
			vadim.CommunicationContactRows.RemoveAndDeleteAll();
			vadim.CommunicationContactRows.AddNew().Contact.Position = CommunicationContactPositions.Reference.Description;

			var benjamin = CreateCandidate(Factory, "Benjamin", email: "ben@jam.in");
			benjamin.CommunicationContactRows.RemoveAndDeleteAll();
			benjamin.CommunicationContactRows.AddNew().Contact.Position = CommunicationContactPositions.Reference.Description;
			benjamin.CommunicationContactRows.AddNew().Contact.Position = CommunicationContactPositions.TeamLead.Description;

			using (var form = GetFormToBash(out var control))
			{
				form.Show();
				Application.DoEvents();
				NavigateToTabPage(control, "emailTabPage");

				var grid = GetContactsZGrid(control);

				control.SetDataBinding(vadim, "CommunicationContactRows");
				Application.DoEvents();

				AssertEquals("Contacts ZGrid should have two visible rows, one communication contact from candidate vadim & one uncommitted row", 2, grid.VisibleRowCount);

				control.SetDataBinding(benjamin, "CommunicationContactRows");
				Application.DoEvents();

				AssertEquals("Contacts ZGrid should have three visible rows, two communication contacts from candidate benjamin & one uncommitted row", 3, grid.VisibleRowCount);
			}
		}

		public void TestContactsGrid_ContainsCorrectColumns()
		{
			using (var form = GetFormToBash(out var control))
			{
				form.Show();

				control.SetDataBinding(TestCandidate, "CommunicationContactRows");
				var contactsZGrid = (ZGrid)control.Controls.Find("contactsZGrid", true).Single();

				CombineAssertions("Contacts ZGrid should contain the correct columns", () =>
				{
					var columns = new string[] { "Selected", "Contact+Position", "Contact+Position", "Contact+Position" };
					columns.ForEach(c => AssertEquals(c + " column should exist", true, contactsZGrid.Columns.Contains(c)));
				});
			}
		}

		public void TestShowErrorMessage()
		{
			PeopleAndCommunicationControl.ShowErrorMessage("I don't feel so good Mr Stark");

			AssertEquals("ShowErrorMessage method should prompt an Error User Notification", "Error I don't feel so good Mr Stark", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestGenerateEConversation_ReturnsNullWithError_WhenMailDomainRegistryNotSet()
		{
			AssertNullOrEmpty("PRE: '" + RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.GetLocation() + "' registry item is empty by default", RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value);

			var conversation = PeopleAndCommunicationControl.GenerateEConversation(TestCandidate, true);

			CombineAssertions("Generating EConversation should return null and prompt error when Mail Domain Registry item is not set", () =>
			{
				AssertEquals("Error message should display", "Error '" + RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.GetLocation() + "' must be set in the registry.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNull("EConversation should be null", conversation);
			});
		}

		public void TestGenerateEConversation_ReturnsNullWithError_WhenThereIsAnErrorInTheContactCollection()
		{
			var candidate = TestCandidate;
			var contactRow = candidate.CommunicationContactRows.AddNew();
			contactRow.Contact.Position = "Dummy position";

			AssertHasError("PRE: Inserting dummy position adds validation error to Position", contactRow.Contact.PositionInfo, "Invalid position");

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, true);

				CombineAssertions("Generating EConversation should return null and prompt error when there is an error in the communication contact collection", () =>
				{
					AssertEquals("Error message should display", "Error There are unresolved errors in the grid.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("EConversation should be null", conversation);
				});
			}
		}

		public void TestGenerateEConversation_ReturnsNullWithError_WhenReferenceSelectedWithoutPermission()
		{
			var candidate = TestCandidate;
			var contactRow = candidate.CommunicationContactRows.AddNew();

			contactRow.Selected = true;
			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = "dummy@email.com";

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, false);

				CombineAssertions("Generating EConversation should return null and prompt error when a reference is selected without being 'permitted to contact references'", () =>
				{
					AssertEquals("Error message should display", "Error You have not indicated permission to contact references.", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("EConversation should be null", conversation);
				});
			}
		}

		public void TestGenerateEConversation_ReturnsNull_WhenNoContactsAreSelected()
		{
			var candidate = TestCandidate;
			var contactRow = candidate.CommunicationContactRows.AddNew();

			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = "dummy@email.com";

			foreach (CommunicationContactRow row in candidate.CommunicationContactRows)
			{
				row.Selected = false;
			}

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, true);

				CombineAssertions("Generating EConversation should return null when no contacts are selected", () =>
				{
					AssertEquals("Error message should not display", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertNull("EConversation should be null", conversation);
				});
			}
		}

		public void TestGenerateEConversation_ReturnsConversation_WithSelectedContactParticipants()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var contactRow1 = candidate.CommunicationContactRows.AddNew();
			var contactRow2 = candidate.CommunicationContactRows.AddNew();
			var contactRow3 = candidate.CommunicationContactRows.AddNew();

			contactRow1.Selected = true;
			contactRow1.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow1.Contact.Email = "dumb@dumber.com";

			contactRow2.Selected = false;
			contactRow2.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow2.Contact.Email = "phort@night.com";

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "elliot@evilcorp.com";
			contactRow3.Selected = true;
			contactRow3.Contact.Position = CommunicationContactPositions.OtherPosition.Description;
			contactRow3.Contact.Staff = newStaff.GS_Code;

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, true);

				CombineAssertions("Generating EConversation should return conversation with selected contacts as participants", () =>
				{
					AssertNotNull("EConversation should not be null", conversation);
					AssertEquals("EConversation should contain two participants", 2, conversation.Participants.Count);
					AssertEquals("EConversation should contain 'dumb@dumber.com' participant", "dumb@dumber.com", conversation.Participants[0].EmailAddress);
					AssertEquals("EConversation should contain 'elliot@evilcorp.com' participant", "elliot@evilcorp.com", conversation.Participants[1].EmailAddress);
				});
			}
		}

		public void TestGenerateEConversation_ReturnsConversation_WithParticipantsAndTheirPositions()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var contactRow1 = candidate.CommunicationContactRows.AddNew();
			var contactRow2 = candidate.CommunicationContactRows.AddNew();

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "over@due.com";
			contactRow1.Selected = true;
			contactRow1.Contact.Position = CommunicationContactPositions.TeamLead.Description;
			contactRow1.Contact.Staff = newStaff.GS_Code;

			contactRow2.Selected = true;
			contactRow2.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow2.Contact.Email = "code@review.co";

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, true);

				CombineAssertions("Generating EConversation should store each participant and their selected position", () =>
				{
					AssertNotNull("EConversation should not be null", conversation);
					AssertEquals("EConversation should contain two participants", 2, conversation.Participants.Count);
					AssertEquals("EConversation should contain 'over@due.com' participant", "over@due.com", conversation.Participants[0].EmailAddress);
					AssertEquals("EConversation should contain 'code@review.co' participant", "code@review.co", conversation.Participants[1].EmailAddress);
					AssertEquals("'over@due.com' participant should have position 'Team Lead'", "Team Lead", conversation.Participants[0].JCP_Relation);
					AssertEquals("'code@review.co' participant should have position 'Reference'", "Reference", conversation.Participants[1].JCP_Relation);
				});
			}
		}

		public void TestGenerateEConversation_ReturnsConversation_WithNoDuplicateParticipants()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var contactRow1 = candidate.CommunicationContactRows.AddNew();
			var contactRow2 = candidate.CommunicationContactRows.AddNew();
			var contactRow3 = candidate.CommunicationContactRows.AddNew();
			var contactRow4 = candidate.CommunicationContactRows.AddNew();

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "over@due.com";

			contactRow1.Selected = true;
			contactRow1.Contact.Position = CommunicationContactPositions.TeamLead.Description;
			contactRow1.Contact.Staff = newStaff.GS_Code;

			contactRow2.Selected = true;
			contactRow2.Contact.Position = CommunicationContactPositions.Manager.Description;
			contactRow2.Contact.Staff = newStaff.GS_Code;

			contactRow3.Selected = true;
			contactRow3.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow3.Contact.Email = "code@review.co";

			contactRow4.Selected = true;
			contactRow4.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow4.Contact.Email = "code@review.co";

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				var conversation = PeopleAndCommunicationControl.GenerateEConversation(candidate, true);

				CombineAssertions("Generating EConversation should store unqiue participants (removing any selected duplicates)", () =>
				{
					AssertNotNull("EConversation should not be null", conversation);
					AssertEquals("EConversation should contain two participants", 2, conversation.Participants.Count);
					AssertEquals("EConversation should contain 'over@due.com' participant", "over@due.com", conversation.Participants[0].EmailAddress);
					AssertEquals("EConversation should contain 'code@review.co' participant", "code@review.co", conversation.Participants[1].EmailAddress);
				});
			}
		}

		public void TestOpenInMailClientButton_DoesNothingIfNoContactsSelected()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var contactRow = candidate.CommunicationContactRows.AddNew();
			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = "dummy email";
			contactRow.Selected = false;

			WebUrlLauncher.ClearLastUrlLaunched();

			using (var form = GetFormToBash(out var control))
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				form.Show();
				Application.DoEvents();
				NavigateToTabPage(control, "emailTabPage");

				control.SetDataBinding(candidate, "CommunicationContactRows");
				Application.DoEvents();

				var button = (ZButton)control.Controls.Find("openInMailClientButton", true).First();
				button.PerformClick();
				Application.DoEvents();

				AssertNullOrEmpty("Clicking 'Open in mail client' with no contacts selected should do nothing", WebUrlLauncher.LastUrlLaunched);
			}

			WebUrlLauncher.ClearLastUrlLaunched();
		}

		public void TestOpenInMailClientButton_LaunchesMailToLinkOfEConversation()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "dummy@leader.com";

			var contactRow = candidate.CommunicationContactRows.AddNew();
			contactRow.Contact.Position = CommunicationContactPositions.TeamLead.Description;
			contactRow.Contact.Staff = newStaff.GS_Code;
			contactRow.Selected = true;

			WebUrlLauncher.ClearLastUrlLaunched();

			using (var form = GetFormToBash(out var control))
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "example.com"))
			{
				form.Show();
				Application.DoEvents();
				NavigateToTabPage(control, "emailTabPage");

				control.SetDataBinding(candidate, "CommunicationContactRows");
				Application.DoEvents();

				var button = (ZButton)control.Controls.Find("openInMailClientButton", true).First();
				button.PerformClick();
				Application.DoEvents();

				AssertNotNull("LastUrlLaunched (Link) should not be null. If it is, the EConversation may have failed to be created", WebUrlLauncher.LastUrlLaunched);

				CombineAssertions("Clicking 'Open in mail client' with contacts selected should launch mailto link of conversation", () =>
				{
					Assert("Link should begin with 'mailto:'", WebUrlLauncher.LastUrlLaunched.StartsWith("mailto:"));
					Assert("Link should contain some characters in the middle", WebUrlLauncher.LastUrlLaunched.Length > 10);
				});
			}

			WebUrlLauncher.ClearLastUrlLaunched();
		}

		public void TestPermittedToContactReferencesCheckbox_IsUsedWhenCreatingConversation()
		{
			var candidate = TestCandidate;
			candidate.CommunicationContactRows.RemoveAndDeleteAll();

			var contactRow = candidate.CommunicationContactRows.AddNew();
			contactRow.Contact.Position = CommunicationContactPositions.Reference.Description;
			contactRow.Contact.Email = "dontcontactme@unsubscribe.com";
			contactRow.Selected = true;

			WebUrlLauncher.ClearLastUrlLaunched();

			using (var form = GetFormToBash(out var control))
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "dummydomain"))
			{
				form.Show();
				Application.DoEvents();
				NavigateToTabPage(control, "emailTabPage");

				control.SetDataBinding(candidate, "CommunicationContactRows");
				Application.DoEvents();

				var checkbox = (ZCheckBox)control.Controls.Find("permittedToContactReferencesCheckBox", true).First();
				checkbox.Checked = false;

				var button = (ZButton)control.Controls.Find("openInMailClientButton", true).First();
				button.PerformClick();
				Application.DoEvents();

				CombineAssertions("Attempting to contact reference without ticking the 'permitted to contact references' checkbox should fail", () =>
				{
					AssertNullOrEmpty("LastUrlLaunched (Link) should be null since the conversation should not have been created", WebUrlLauncher.LastUrlLaunched);
					AssertEquals("Error message should display", "Error You have not indicated permission to contact references.", UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}

			WebUrlLauncher.ClearLastUrlLaunched();
		}

		public void TestIsContactAReference()
		{
			var referenceContact = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.Reference, "iama@referen.ce"));
			var internalContact = new CommunicationContactRow(new CommunicationContact(Factory, CommunicationContactPositions.TeamLead, "iama@teamle.ad"));

			AssertEquals("IsContactAReference method should return true when contact is a reference", true, PeopleAndCommunicationControl.IsContactAReference(referenceContact));
			AssertEquals("IsContactAReference method should return false when contact is not a reference", false, PeopleAndCommunicationControl.IsContactAReference(internalContact));
		}

		public void TestAttachTipLabel()
		{
			using (GetFormToBash(out var control))
			{
				var attachTipLabel = (ZLabel)control.Controls.Find("attachTipLabel", true).First();

				AssertEquals("Tip: you can copy && paste eDocs to the email from the eDocs tab.", attachTipLabel.Text);
				AssertEquals(true, attachTipLabel.Font.Italic);
				AssertEquals(Color.Yellow, attachTipLabel.BackColor);
			}
		}

		static void NavigateToTabPage(PeopleAndCommunicationControl control, string tabPageName)
		{
			var tabControl = control.Controls.Find("peopleAndCommunicationTabControl", true).First() as ZTabControl;
			tabControl.SelectedTab = (ZTabPage)tabControl.TabPages[tabPageName];
			Application.DoEvents();
		}

		static ZGrid GetContactsZGrid(Control control) => (ZGrid)control.Controls.Find("contactsZGrid", true).Single();
	}
}
