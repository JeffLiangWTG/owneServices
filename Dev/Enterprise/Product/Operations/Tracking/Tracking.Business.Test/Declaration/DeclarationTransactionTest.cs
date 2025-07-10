using System.Linq;
using CargoWise.Definitions;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class DeclarationTransactionTest : TrackingInvoiceLoaderTest
	{
		[HttpContextEnabledTest]
		public void TestNotesContainAllClientVisibleNotes()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var note = importer.Notes.AddNew();
			note.ST_Description = "test note";
			note.ST_IsCustomDescription = true;
			note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			note.ST_NoteContext = "AAA";
			note.ST_NoteDataAsText = "This is a note that should be visible to a client";

			var trackingDec = (TrackingDeclaration)GetNewBusinessObject();
			trackingDec.Declaration.JE_OH_Importer = importer.PK;
			AssertEquals(true, trackingDec.NotesHelper.VisibleNotes.Contains(note));
		}

		protected override ITransactionSupport GetNewBusinessObject()
		{
			TrackingDeclaration result = new TrackingDeclaration(TestJobDeclaration, TestSiteUser);
			result.Declaration.JE_DeclarationReference = "B00001000";
			result.Declaration.JE_OH_Supplier = TestOrg.PK;
			return result;
		}

		BaseJobDeclaration TestJobDeclaration
		{
			get
			{
				if (fTestJobDeclaration == null)
				{
					fTestJobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				}
				return fTestJobDeclaration;
			}
		}
		BaseJobDeclaration fTestJobDeclaration;
	}
}
