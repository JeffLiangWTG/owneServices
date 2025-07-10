using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolNotesCheckerTest : TestCaseWithFactory
	{
		public void TestSpecialInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			Factory.Save();

			AssertEquals("Should have Special Instructions", true, consol.NotesChecker.HasSpecialInstructions);
		}

		public void TestGoodsHandlingInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			Factory.Save();

			AssertEquals("Should have Goods Handling Instructions", true, consol.NotesChecker.HasGoodsHandlingInstructions);
		}

		public void TestInstructions_None()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			consol.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;

			Factory.Save();

			AssertEquals("Should have no Special Instructions", false, consol.NotesChecker.HasSpecialInstructions);
			AssertEquals("Should have no Goods Handling instructions", false, consol.NotesChecker.HasGoodsHandlingInstructions);
		}

		public void TestSpecialInstructions_RelatedBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			sendingForwarder.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			Factory.Save();

			AssertEquals("Consol has no notes", false, consol.Notes.HasNotes);
			AssertEquals("Consol should have Special Instructions", true, consol.NotesChecker.HasSpecialInstructions);
		}

		public void TestGoodsHandlingInstructions_RelatedBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			container.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			Factory.Save();

			AssertEquals("Consol has no notes", false, consol.Notes.HasNotes);
			AssertEquals("Consol should have Goods Handling Instructions", true, consol.NotesChecker.HasGoodsHandlingInstructions);
		}
	}
}
