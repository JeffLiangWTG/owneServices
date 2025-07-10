using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentStmNoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEmptyST_NoteTextValidation()
		{
			var detailedGoodsDescriptionNote = CreateStmNoteWithEmptyText(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description);
			AssertNoErrors(detailedGoodsDescriptionNote.ST_NoteTextInfo);

			var marksAndNumbersNote = CreateStmNoteWithEmptyText(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			AssertNoErrors(marksAndNumbersNote.ST_NoteTextInfo);

			var invoiceDetailsNote = CreateStmNoteWithEmptyText(PredefinedNoteTypes.Instance.InvoiceDetails.Description);
			AssertHasError(invoiceDetailsNote.ST_NoteTextInfo, "Enter some note details for Invoice Details.");
		}

		public void TestValidateOriginalBillNotesIsSystemOnly()
		{
			var note = Factory.New<ForwardingShipmentStmNote>();

			note.ST_Description = PredefinedNoteTypes.Instance.OriginalBillNotes.Description;
			AssertHasError(note.ST_DescriptionInfo, "Original Bill Notes is reserved for system use and cannot be manually entered.");
		}

		#region Implementation

		ForwardingShipmentStmNote CreateStmNoteWithEmptyText(string description)
		{
			var note = Factory.NewWithValidTestData<ForwardingShipmentStmNote>();
			note.ST_Table = Shipment.TableName;
			note.ST_ParentID = Shipment.PK;
			note.ST_Description = description;
			note.ST_NoteText = ZString.Empty;

			return note;
		}

		ForwardingShipment Shipment => shipment ?? (shipment = Factory.New<ForwardingShipment>());
		ForwardingShipment shipment;

		#endregion
	}
}
