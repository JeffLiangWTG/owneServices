using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolStmNotes))]
	sealed class ForwardingConsolStmNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestForwardingConsolStmNotes()
		{
			var notes = new ForwardingConsolStmNotes(Consol);
			var note1 = notes.AddNew();

			AssertEquals("Parent", Consol, notes.Parent);
			AssertEquals("Note type", typeof(ForwardingConsolStmNote), note1.GetType());
			AssertEquals("Visible note type", typeof(ForwardingConsolStmNote), notes.VisibleNotes.TypeOfElements);
			AssertEquals("All notes type", typeof(ForwardingConsolStmNote), notes.GetAllNotes().TypeOfElements);
			AssertEquals("Visible notes collection type", typeof(ForwardingConsolStmNoteCollectionView), notes.VisibleNotes.GetType());
			AssertEquals("All notes collection type", typeof(ForwardingConsolStmNoteCollection), notes.GetAllNotes().GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => Consol.Notes;

		ForwardingConsol Consol => consol ?? (consol = Factory.New<ForwardingConsol>());
		ForwardingConsol consol;

		#endregion
	}
}
