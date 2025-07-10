using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(NoteBusinessObjectFinder))]
	public class NoteBusinessObjectFinderTest : MatchingBusinessObjectFinderTest
	{
		public override void TestFind()
		{
			var dataObject = new Note();
			dataObject.Description = "ZUMBAYA";

			var finder = new NoteBusinessObjectFinder(dataObject);
			AssertExceptionThrown(typeof(ArgumentNullException), () => finder.Find((IStmNoteParent)null));

			var noteParent = Factory.New<DummyEnterpriseBusinessObject>();

			var note1 = noteParent.Notes.AddNew();
			note1.ST_Description = "CAT EATER";
			note1.ST_NoteContext = "XXX";

			var note2 = noteParent.Notes.AddNew();
			note2.ST_Description = "DR.MOOSE";
			note2.ST_NoteContext = "XXX";

			var note3 = noteParent.Notes.AddNew();
			note3.ST_Description = "BANANARAMA";
			note3.ST_NoteContext = "XXX";

			finder = new NoteBusinessObjectFinder(dataObject);
			AssertEquals("should not find note with matching context and description", null, finder.Find(noteParent));

			dataObject.Description = "DR.MOOSE";

			finder = new NoteBusinessObjectFinder(dataObject);
			AssertEquals("should not find note with matching context and description", null, finder.Find(noteParent));

			dataObject.NoteContext = new NoteContext { Code = ZString.Empty };

			finder = new NoteBusinessObjectFinder(dataObject);
			AssertEquals("should find note that matches description and the context is unspecified in data object", null, finder.Find(noteParent));

			dataObject.NoteContext = new NoteContext { Code = "AAA" };

			finder = new NoteBusinessObjectFinder(dataObject);
			AssertEquals("should find note that matches description and the context is set to AAA (A-All)in data", null, finder.Find(noteParent));

			dataObject.NoteContext = new NoteContext { Code = "XXX" };

			finder = new NoteBusinessObjectFinder(dataObject);
			AssertEquals("should find note with matching context and description", note2, finder.Find(noteParent));
		}

		public void TestFind_NoteContextIsNotSpecified_MatchNoteWithAAAContextOrEmptyContext()
		{
			var dataObject = new Note();
			dataObject.Description = "HOLVORAZA";

			var noteParent = Factory.New<DummyEnterpriseBusinessObject>();

			var note1 = noteParent.Notes.AddNew();
			note1.ST_Description = "KEZHCIVOKUNAY";
			note1.ST_NoteContext = "AAA";

			var note2 = noteParent.Notes.AddNew();
			note2.ST_Description = "HOLVORAZA";
			note2.ST_NoteContext = "AAA";

			var note3 = noteParent.Notes.AddNew();
			note3.ST_Description = "HOLVORAZA";
			note3.ST_NoteContext = "XXX";

			var finder = new NoteBusinessObjectFinder(dataObject);

			var matchedNote = finder.Find(noteParent);
			AssertEquals("Should match note with empty context", note2, matchedNote);

			var note4 = noteParent.Notes.AddNew();
			note4.ST_Description = "HOLVORAZA";
			note4.ST_NoteContext = ZString.Empty;

			matchedNote = finder.Find(noteParent);
			AssertEquals("Should match note with AAA context", note4, matchedNote);
			ErrorReporter.Clear();
		}
	}
}
