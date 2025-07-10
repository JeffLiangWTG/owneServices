using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingStmNoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePredefinedDescriptionIsUnique()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			StmNote note1 = quotedBooking.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			StmNote note2 = quotedBooking.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);
			note1.ST_Description = PredefinedNoteTypes.Instance.InternalWorkNotes.Description;
			AssertHasError(note1.ST_DescriptionInfo, "There is already another note with the description 'Internal Work Notes' for Context Module ' All', Direction ' All', Freight Mode ' All'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown, it could belong to another company or recently added by another user - please reload the form. ");
			AssertNoErrors(note2.ST_DescriptionInfo);
			note1.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			AssertNoErrors(note1.ST_DescriptionInfo);
			AssertNoErrors(note2.ST_DescriptionInfo);
		}

		public void TestValidatePredefinedDescriptionIsUnique_SavingDuplicateNoteWhenEdittingSimultaneously()
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(Factory);
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			QuotedBooking quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			Factory.Save();
			Factory.RefreshEnabled = false;
			StmNote note1 = quotedBooking.Notes.AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note1.ST_NoteText = "Note Text";
			AssertNoErrors(note1.ST_DescriptionInfo);
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			QuotedBooking quotedBooking2 = factory2.Load<QuotedBooking>(quote.PK);
			StmNote note2 = quotedBooking2.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note2.ST_NoteText = "Note Text";
			AssertNoErrors(note2.ST_DescriptionInfo);
			Factory.Save();
			note2.RunPreSaveValidation();
			string expectedErrorMessage = string.Format("There is already another note with the description '{0}' for Context Module '{1}', Direction '{2}', Freight Mode '{3}'. Please select a different description, context module, direction or freight mode. If the duplicate note is not shown - please reload the form.", note2.ST_Description, note2.ST_NoteContextModuleCaption.Substring(3), note2.ST_NoteContextDirectionCaption.Substring(3), note2.ST_NoteContextFreightModeCaption.Substring(3));
			AssertHasErrors(expectedErrorMessage, note2.ST_DescriptionInfo);
		}
	}
}
