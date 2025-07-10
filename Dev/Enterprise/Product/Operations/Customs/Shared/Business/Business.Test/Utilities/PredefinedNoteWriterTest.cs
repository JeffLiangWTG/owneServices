using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PredefinedNoteWriterTest : TestCaseWithFactory
	{
		public void TestCreateUpdateDeleteNote()
		{
			var declaration = Factory.New<TestDeclaration>();
			declaration.PredefinedNoteTestProperty = "Some extra instruction";
			AssertEquals("Some extra instruction", declaration.PredefinedNoteTestProperty);
			Assert(declaration.Updated);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var declarationCopy = otherFactory.Load<TestDeclaration>(declaration.PK);
			AssertEquals("Some extra instruction", declarationCopy.PredefinedNoteTestProperty);

			declarationCopy.PredefinedNoteTestProperty = "Changed again";
			Assert(declarationCopy.Updated);
			otherFactory.Save();

			var thirdFactory = new BusinessObjectFactory();
			var declaration3 = thirdFactory.Load<TestDeclaration>(declaration.PK);
			AssertEquals("Changed again", declaration3.PredefinedNoteTestProperty);

			declaration3.PredefinedNoteTestProperty = "Changed again";
			Assert("Not updated", !declaration3.Updated);

			Assert("Note Exists", declaration3.Notes.FindByDescription(declaration3.TestingNoteType.Description).Any());
			declaration3.PredefinedNoteTestProperty = "";
			Assert(declaration3.Updated);
			thirdFactory.Save();

			Assert("Note Deleted", !declaration3.Notes.FindByDescription(declaration3.TestingNoteType.Description).Any());
		}

		public void TestCreateUpdateDeleteCustomNote()
		{
			ZString customNoteTypeDescription = "Custom Description";

			var declaration = Factory.New<TestDeclaration>();
			var customNoteWriter = new PredefinedNoteWriter(declaration, customNoteTypeDescription, StmNoteVisibility.DOC);

			customNoteWriter.UpdateValue("A sample Note");
			Factory.Save();

			ZQuery filter = new ZQuery(StmNoteSchema.ST_ParentID, declaration.PK);
			filter.AddToFilter(StmNoteSchema.ST_Description, customNoteTypeDescription);
			filter.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			filter.AddToFilter(StmNoteSchema.ST_Table, declaration.TableName);
			filter.IncludeBlob(StmNoteSchema.ST_NoteData);
			filter.IncludeBlob(StmNoteSchema.ST_NoteText);
			var note = Factory.LoadTop1<StmNote>(filter);

			AssertNotNull("Note created", note);
			AssertEquals("Text is set", "A sample Note", note.ST_NoteDataAsText);

			var customNoteReader = new PredefinedNoteWriter(declaration, customNoteTypeDescription, StmNoteVisibility.DOC);
			AssertEquals("Text can be retrieved", "A sample Note", customNoteReader.Value);

			customNoteWriter.UpdateValue("");
			Factory.Save();

			note = Factory.LoadTop1<StmNote>(filter);
			AssertNull("Note deleted", note);
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public new class Schema : BaseJobDeclaration.Schema
			{
				public const string PredefinedNoteTestProperty = "PredefinedNoteTestProperty";
			}

			public ZString PredefinedNoteTestProperty
			{
				get { return NoteWriter.Value; }
				set { Updated = NoteWriter.UpdateValue(value); }
			}

			public bool Updated { get; set; }

			public ZPropertyInfo PredefinedNoteTestPropertyInfo
			{
				get { return GetZPropertyInfo(Schema.PredefinedNoteTestProperty); }
			}

			PredefinedNoteWriter NoteWriter
			{
				get { return noteWriter ?? (noteWriter = new PredefinedNoteWriter(this, TestingNoteType)); }
			}
			PredefinedNoteWriter noteWriter;

			public PredefinedNoteType TestingNoteType = PredefinedNoteTypes.Instance.SpecialInstructions;
		}
	}
}
