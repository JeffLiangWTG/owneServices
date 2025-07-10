using System.Linq;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsDocketReferenceCollectionReader))]
	class WhsDocketReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		#region TestReadIntoCollection

		public override void TestReadIntoCollection()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_DocketID = "W00001000";
			var existingReference1 = order.References.AddNew();
			existingReference1.WX_RefType = "TRF";
			existingReference1.WX_Reference = "123";

			var existingReference2 = order.References.AddNew();
			existingReference2.WX_RefType = "HSB";
			existingReference2.WX_Reference = "456";

			var additionalReference1 = new AdditionalReference { Type = new EntryType { Code = "TRF" }, ReferenceNumber = "123" };
			var additionalReference2 = new AdditionalReference { Type = new EntryType { Code = "OTH" }, ReferenceNumber = "FRED" };
			var additionalReference3 = new AdditionalReference { Type = new EntryType { Code = "XXX" }, ReferenceNumber = "Triple" };
			var additionalReferences = new DataObjectList<AdditionalReference>(new[] { additionalReference1, additionalReference2, additionalReference3 });

			var logger = new TestErrorLogger();
			var reader = new WhsDocketReferenceCollectionReader(additionalReferences, logger, Factory, order);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching WhsDocketReference.
Information - Populating WhsDocketReference...
Information - No matching WhsDocketReference found, creating new WhsDocketReference.
Information - Populating WhsDocketReference...
Warning - Unknown Additional References were found for 'Warehouse Order W00001000' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
".Trim(), logger.Logs);

			AssertEquals(2, order.References.Count);
			order.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == "TRF" && r.WX_Reference == "123");
			order.References.Cast<WhsDocketReference>().Single(r => r.WX_RefType == "OTH" && r.WX_Reference == "FRED");

			var notes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Note contents", "Type: XXX\r\nNumber: Triple", notes[0].ST_NoteDataAsText);
		}

		#endregion
	}
}
