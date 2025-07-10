using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportAdditionalReferenceCollectionReaderTest<T> : DataObjectCollectionReaderTest
			where T : DtbTransport, IStmNoteParent, ITransportAdditionalReferenceNumbers, IAdditionalReferenceNumberTypeProvider
	{
		#region TestReadIntoCollection

		public override void TestReadIntoCollection()
		{
			var transport = GetNewTransport();
			var existingReference1 = transport.AdditionalReferenceNumbers.AddNew();
			existingReference1.CE_EntryType = "TRF";
			existingReference1.CE_EntryNum = "123";

			var existingReference2 = transport.AdditionalReferenceNumbers.AddNew();
			existingReference2.CE_EntryType = "CLR";
			existingReference2.CE_EntryNum = "456";

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "TRF" }, ReferenceNumber = "123" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "CIN" }, ReferenceNumber = "987654321" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "ABC" }, ReferenceNumber = "OTHER" };
			var additionalReferences = new DataObjectList<AdditionalReference>(new[] { additionalReference1, additionalReference2, additionalReference3 });

			var logger = new TestErrorLogger();
			var additionalReferenceReader = new TransportAdditionalReferenceCollectionReader<T>(additionalReferences, logger, Factory, transport);

			additionalReferenceReader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", string.Format(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Warning - Unknown Additional References were found for '{0}' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
", transport.HumanReadableName).Trim(), logger.Logs);

			var referenceNumbers = transport.AdditionalReferenceNumbers;
			AssertEquals(2, referenceNumbers.Count);
			AssertEquals("TRF", referenceNumbers[0].CE_EntryType);
			AssertEquals("123", referenceNumbers[0].CE_EntryNum);
			AssertEquals("CIN", referenceNumbers[1].CE_EntryType);
			AssertEquals("987654321", referenceNumbers[1].CE_EntryNum);

			var notes = transport.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Note contents", "Type: ABC\r\nNumber: OTHER", notes[0].ST_NoteDataAsText);
		}

		#endregion

		#region Implementation

		protected abstract T GetNewTransport();

		#endregion
	}
}
