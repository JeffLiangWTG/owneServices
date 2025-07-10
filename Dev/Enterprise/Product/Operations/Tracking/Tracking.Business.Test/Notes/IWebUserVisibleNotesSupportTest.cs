using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	public abstract class IWebUserVisibleNotesSupportTest : TestCaseWithFactory
	{
		#region Setup

		protected Notes BizObjNotes;
		protected IWebUserVisibleNotesSupport BizObj;

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBusinessObject();
			BizObjNotes = BizObj.NotesParentBO.Notes;
		}

		protected abstract IWebUserVisibleNotesSupport GetNewBusinessObject();

		protected abstract void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility);

		protected abstract BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent);

		#endregion

		public void TestShowsNotesForAllCompanies()
		{
			var otherCompanyNoteText = "Note for different company";
			var otherCompanyNote = BizObjNotes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, otherCompanyNoteText);
			otherCompanyNote.ST_GC_RelatedCompany = ZGuid.NewZGuid();

			var visibleNotes = BizObj.NotesHelper.VisibleNotes;
			AssertEquals("Shows notes for all companies", true, BizObj.NotesParentBO.Notes.ShowNotesForAllCompanies);
			AssertEquals("One Note", 1, BizObjNotes.GetAllNotes().Count);
			AssertEquals("One VisibleNote", 1, visibleNotes.Count);
			AssertEquals("VisibleNote Text", otherCompanyNoteText, visibleNotes[0].ST_NoteDataAsText);
		}

		public void TestNoVisibleNotesWhenThereIsNoNotesAtAll()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			AssertNotNull("VisibleNotes returns not null", BizObj.NotesHelper.VisibleNotes);

			AssertEquals("No Notes", 0, BizObjNotes.GetAllNotes().Count);
			AssertEquals("No VisibleNotes", 0, BizObj.NotesHelper.VisibleNotes.Count);
		}

		public void TestVisibleNotesDependOnLoggedInUserOrg()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			TestNoVisibleNotesWhenThereIsNoNotesAtAll();

			ZString publicNoteText = "First Public Note";
			StmNote note1 = BizObjNotes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, publicNoteText);

			AssertEquals("One Note", 1, BizObjNotes.GetAllNotes().Count);
			AssertEquals("One VisibleNote", 1, BizObj.NotesHelper.VisibleNotes.Count);
			AssertEquals("VisibleNote Description", PredefinedNoteTypes.Instance.SpecialInstructions.Description, BizObj.NotesHelper.VisibleNotes[0].ST_Description);
			AssertEquals("VisibleNote Text", publicNoteText, BizObj.NotesHelper.VisibleNotes[0].ST_NoteDataAsText);

			if (BizObj.ShowAgentNotes)
			{
				SetAgentNotesVisibility(BizObj, false);
			}

			AssertEquals("Should not show Agent Notes", false, BizObj.ShowAgentNotes);
			StmNote note2 = BizObjNotes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "First Agent Note");

			AssertEquals("Two Notes", 2, BizObjNotes.GetAllNotes().Count);
			AssertEquals("Still one VisibleNote", 1, BizObj.NotesHelper.VisibleNotes.Count);
			AssertEquals("VisibleNote Description", PredefinedNoteTypes.Instance.SpecialInstructions.Description, BizObj.NotesHelper.VisibleNotes[0].ST_Description);
			AssertEquals("VisibleNote Text", publicNoteText, BizObj.NotesHelper.VisibleNotes[0].ST_NoteDataAsText);

			SetAgentNotesVisibility(BizObj, true);
			if (BizObj.ShowAgentNotes)
			{
				AssertEquals("Two VisibleNotes", 2, BizObj.NotesHelper.VisibleNotes.Count);
				helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				AssertEquals("Shipment quick view user shouldn't see agent notes", 1, BizObj.NotesHelper.VisibleNotes.Count);
			}
		}

		public void TestVisibleNotesForRelatedBusinessObject()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			TestNoVisibleNotesWhenThereIsNoNotesAtAll();

			BusinessObject relatedBizObj = GetRelatedBusinessObject(BizObj);
			if (relatedBizObj != null)
			{
				ZString publicNoteText = "First Public Note";
				StmNote note1 = relatedBizObj.GetNotes().AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, publicNoteText);

				AssertEquals("One Note", 1, relatedBizObj.GetNotes().GetAllNotes().Count);
				AssertEquals("One VisibleNote", 1, BizObj.NotesHelper.VisibleNotes.Count);
				AssertEquals("VisibleNote Description", PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, BizObj.NotesHelper.VisibleNotes[0].ST_Description);
				AssertEquals("VisibleNote Text", publicNoteText, BizObj.NotesHelper.VisibleNotes[0].ST_NoteDataAsText);

				if (BizObj.ShowAgentNotes)
				{
					SetAgentNotesVisibility(BizObj, false);
				}

				AssertEquals("Should not show Agent Notes", false, BizObj.ShowAgentNotes);
				StmNote note2 = relatedBizObj.GetNotes().AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "First Agent Note");

				AssertEquals("Two Note", 2, relatedBizObj.GetNotes().GetAllNotes().Count);
				AssertEquals("One VisibleNote", 1, BizObj.NotesHelper.VisibleNotes.Count);
				AssertEquals("VisibleNote Description", PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, BizObj.NotesHelper.VisibleNotes[0].ST_Description);
				AssertEquals("VisibleNote Text", publicNoteText, BizObj.NotesHelper.VisibleNotes[0].ST_NoteDataAsText);

				SetAgentNotesVisibility(BizObj, true);
				if (BizObj.ShowAgentNotes)
				{
					AssertEquals("Two VisibleNotes", 2, BizObj.NotesHelper.VisibleNotes.Count);
					helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
					AssertEquals("Shipment quick view user shouldn't see agent notes", 1, BizObj.NotesHelper.VisibleNotes.Count);
				}
			}
		}
	}
}
