using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransportAdditionalReferenceCollectionReader<DtbBooking>))]
	sealed class DtbBookingAdditionalReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			var existingReference1 = booking.AdditionalReferenceNumbers.AddNew();
			existingReference1.CE_EntryType = "TRF";
			existingReference1.CE_EntryNum = "123";

			var existingReference2 = booking.AdditionalReferenceNumbers.AddNew();
			existingReference2.CE_EntryType = "CLR";
			existingReference2.CE_EntryNum = "456";

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "TRF" }, ReferenceNumber = "123" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "CIN" }, ReferenceNumber = "987654321" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "ABC" }, ReferenceNumber = "OTHER" };
			var additionalReferences = new DataObjectList<AdditionalReference>(new[] { additionalReference1, additionalReference2, additionalReference3 });

			var logger = new TestErrorLogger();
			var additionalReferenceReader = new TransportAdditionalReferenceCollectionReader<DtbBooking>(additionalReferences, logger, Factory, booking);

			additionalReferenceReader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", string.Format(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Warning - Unknown Additional References were found for '{0}' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
", booking.HumanReadableName).Trim(), logger.Logs);

			var referenceNumbers = booking.AdditionalReferenceNumbers;
			AssertEquals(2, referenceNumbers.Count);
			AssertEquals("TRF", referenceNumbers[0].CE_EntryType);
			AssertEquals("123", referenceNumbers[0].CE_EntryNum);
			AssertEquals("CIN", referenceNumbers[1].CE_EntryType);
			AssertEquals("987654321", referenceNumbers[1].CE_EntryNum);

			var notes = booking.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Note contents", "Type: ABC\r\nNumber: OTHER", notes[0].ST_NoteDataAsText);
		}
	}
}
