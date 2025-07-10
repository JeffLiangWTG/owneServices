using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentNotesPresenceHelperTest : TestCaseWithFactory
	{
		public void TestSpecialInstructions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			Factory.Save();

			AssertEquals("Shipment should have Special Instructions", true, shipment.NotesChecker.HasSpecialInstructions);
		}

		public void TestGoodsHandlingInstructions()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			Factory.Save();

			AssertEquals("Shipment should have Goods Handling Instructions", true, shipment.NotesChecker.HasGoodsHandlingInstructions);
		}

		public void TestSpecialInstructions_None()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			Factory.Save();

			AssertEquals("Should have no Special Instructions", false, shipment.NotesChecker.HasSpecialInstructions);
		}

		public void TestGoodsHandlingInstructions_None()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			shipment.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			Factory.Save();

			AssertEquals("Should have no Goods Handling instructions", false, shipment.NotesChecker.HasGoodsHandlingInstructions);
		}

		public void TestSpecialInstructions_RelatedBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			consignor.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;

			Factory.Save();

			AssertEquals("Shipment has no notes", false, shipment.Notes.HasNotes);
			AssertEquals("Shipment should have Special Instructions", true, shipment.NotesChecker.HasSpecialInstructions);
		}

		public void TestGoodsHandlingInstructions_RelatedBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsignorPK = consignor.PK;
			consignor.Notes.AddNew().ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;

			Factory.Save();

			AssertEquals("Shipment has no notes", false, shipment.Notes.HasNotes);
			AssertEquals("Shipment should have Goods Handling Instructions", true, shipment.NotesChecker.HasGoodsHandlingInstructions);
		}

		public void TestNoInstructionNotes()
		{
			var shipment = Factory.New<ForwardingShipment>();

			AssertEquals("Shipment has no notes", false, shipment.Notes.HasNotes);
			AssertEquals("Should have no Goods Handling instructions", false, shipment.NotesChecker.HasGoodsHandlingInstructions);
			AssertEquals("Should have no special instructions", false, shipment.NotesChecker.HasSpecialInstructions);
		}
	}
}
