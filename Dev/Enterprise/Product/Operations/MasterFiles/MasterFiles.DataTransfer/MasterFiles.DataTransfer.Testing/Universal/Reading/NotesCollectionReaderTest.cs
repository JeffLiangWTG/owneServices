using System.Linq;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(NotesCollectionReader))]
	public class NotesCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var noteParent = Factory.New<DummyEnterpriseBusinessObject>();

			var note1 = noteParent.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note1.ST_NoteContext = "AAA";
			note1.ST_NoteText = "SOME TEXT";

			var note2 = noteParent.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note2.ST_NoteContext = "BBB";

			var note3 = noteParent.Notes.AddNew();
			note3.ST_Description = "BANANARAMA";
			note3.ST_NoteContext = "XXX";

			var noteDataObject1 = new Note
			{
				Description = "ZUMBAYA",
				NoteContext = new NoteContext { Code = "AAA" }
			};

			var noteDataObject2 = new Note
			{
				Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description,
				NoteContext = new NoteContext { Code = "AAA" },
				NoteText = "KATIUSZA"
			};

			var logger = new TestErrorLogger();
			var reader = new NotesCollectionReader(new DataObjectList<Note>(new[]
			{
				noteDataObject1, noteDataObject2
			}),
			logger, Factory, noteParent);

			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals("notes count", 4, noteParent.Notes.GetAllNotes().Count);
				AssertCollectionContains("matched note", note1, noteParent.Notes.GetAllNotes());
				AssertCollectionContains("unmatched but not removed note", note2, noteParent.Notes.GetAllNotes());

				AssertMultilineASCIIEquals("logs", @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
".Trim(), logger.Logs);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"Detailed Goods Description|AAA|KATIUSZA", "Detailed Goods Description|BBB|", "BANANARAMA|XXX|", "ZUMBAYA|AAA|"
				},
				FormatTransports(noteParent));
			});
		}

		public void TestStmNoteWithVisibleCompanySpecifiedIgnoresTheCompanyIfThereIsNoTopLevelDataContextProvided()
		{
			var noteParent = Factory.New<DummyEnterpriseBusinessObject>();

			var companyType = CargoWise.Application.ObjectFactory.GetType<Integration.IGlbCompany>();
			var company = (Integration.IGlbCompany)Factory.LoadTop1(companyType, new CargoWise.EntityFramework.ZQuery());

			var noteDataObject = new Note
			{
				Description = "ZUMBAYA",
				NoteContext = new NoteContext { Code = "AAA" },
				VisibleCompany = new CodeDescriptionPair { Code = company.GC_Code, Description = company.GC_Name } // As there is no top level data context, this *SHOULD* do nothing.
			};

			var logger = new TestErrorLogger();
			var reader = new NotesCollectionReader(new DataObjectList<Note>(new[]
			{
				noteDataObject
			}), logger, Factory, noteParent);

			reader.ReadIntoCollection();
			CombineAssertions(() =>
			{
				AssertEquals("notes count", 1, noteParent.Notes.GetAllNotes().Count);

				AssertMultilineASCIIEquals("logs", @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
".Trim(), logger.Logs);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					"ZUMBAYA|AAA|"
				},
				FormatTransports(noteParent));
			});
		}

		string[] FormatTransports(IStmNoteParent noteParent)
		{
			return noteParent.Notes.GetAllNotes()
				.Cast<StmNote>()
				.Select(n => string.Format("{0}|{1}|{2}", n.ST_Description, n.ST_NoteContext, n.ST_NoteText))
				.ToArray();
		}
	}
}
