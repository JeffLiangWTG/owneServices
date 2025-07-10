using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentStmNotes))]
	sealed class ForwardingShipmentStmNotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestForwardingShipmentStmNotes()
		{
			var notes = new ForwardingShipmentStmNotes(Shipment);
			var note1 = notes.AddNew();

			AssertEquals("Parent", Shipment, notes.Parent);
			AssertEquals("Note type", typeof(ForwardingShipmentStmNote), note1.GetType());
			AssertEquals("Visible note type", typeof(ForwardingShipmentStmNote), notes.VisibleNotes.TypeOfElements);
			AssertEquals("All notes type", typeof(ForwardingShipmentStmNote), notes.GetAllNotes().TypeOfElements);
			AssertEquals("Visible notes collection type", typeof(ForwardingShipmentStmNoteCollectionView), notes.VisibleNotes.GetType());
			AssertEquals("All notes collection type", typeof(ForwardingShipmentStmNoteCollection), notes.GetAllNotes().GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Shipment.Notes;
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
