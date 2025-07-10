using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class NoteDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteNoteData()
		{
			var noteParentBO = Factory.New<OrgHeader>();
			var noteBO = noteParentBO.Notes.AddNew();
			noteBO.ST_IsCustomDescription = true;
			noteBO.ST_Description = "CAT EATER!!";
			noteBO.ST_NoteContext = "DEB";
			noteBO.ST_NoteType = "PUB";
			noteBO.ST_GC_RelatedCompany = GlbCompany.CurrentCompany.PK;

			var writer = new NoteDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, noteBO)));
			var noteData = writer.GetDataObject(noteBO);

			CombineAssertions("noteData Contents", delegate
			{
				AssertEquals("Description", "CAT EATER!!", noteData.Description);
				AssertEquals("Visibility.Code", nameof(StmNoteVisibility.PUB), noteData.Visibility.Code);
				AssertEquals("Visibility.Description", "CLIENT-VISIBLE", noteData.Visibility.Description);
				AssertEquals("NoteContext.Code", "DEB", noteData.NoteContext.Code);
				AssertEquals("NoteContext.Description", "Module: D - Customs/Declarations, Direction: E - Export, Freight: B - Air and Sea", noteData.NoteContext.Description);
				AssertEquals("VisibleCompany.Code", GlbCompany.CurrentCompany.GC_Code, noteData.VisibleCompany.Code);
				AssertEquals("VisibleCompany.Description", GlbCompany.CurrentCompany.GC_Name, noteData.VisibleCompany.Description);
			});
		}

		public void TestAlwaysExportDescriptionInEnglish()
		{
			using (var mockENG = Res.UseMockData())
			using (var mockFRN = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			{
				mockENG.Put("2011e3e6-0718-43c6-8d3f-ca79c193cad2", new ResourceStringData("2011e3e6-0718-43c6-8d3f-ca79c193cad2", "Goods Handling Instructions"));
				mockFRN.Put("2011e3e6-0718-43c6-8d3f-ca79c193cad2", new ResourceStringData("2011e3e6-0718-43c6-8d3f-ca79c193cad2", "Instruction manipulation marchandise"));

				var noteParentBO = Factory.New<OrgHeader>();
				var noteBO = noteParentBO.GetNotes().AddNew(false, Res.GetString("2011e3e6-0718-43c6-8d3f-ca79c193cad2", "Goods Handling Instructions"), "Ce'st une note");

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
				{
					Assert("Language should have changed", !Res.IsEnglish(Res.CurrentLanguage));

					var writer = new NoteDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, noteBO)));
					var noteData = writer.GetDataObject(noteBO);

					CombineAssertions("noteData Contents", delegate
					{
						AssertEquals("Description", "Goods Handling Instructions", noteData.Description);
						AssertEquals("Visibility.Code", nameof(StmNoteVisibility.PUB), noteData.Visibility.Code);
						AssertEquals("Visibility.Description", "CLIENT-VISIBLE", noteData.Visibility.Description);
						AssertEquals("NoteContext.Code", "AAA", noteData.NoteContext.Code);
						AssertEquals("NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", noteData.NoteContext.Description);
					});
				}
			}
		}
	}
}
