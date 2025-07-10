using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ShipmentAdditionalReferenceCollectionReader<>))]
	sealed class ShipmentAdditionalReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var shipment = Factory.New<CommonShipment>();

			var cusEntryNumber1 = shipment.Numbers.AddNew();
			cusEntryNumber1.CE_EntryType = "COC";
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2012, 2, 1);

			var cusEntryNumber2 = shipment.Numbers.AddNew();
			cusEntryNumber2.CE_EntryType = "BBB";
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber2.CE_IssueDate = new ZDateTime(2012, 2, 2);

			Factory.SaveForTesting();

			var additionalNumberDataObject1 = CreateDataObject("COC", "222", new ZDateTime(2012, 3, 3));
			var additionalNumberDataObject2 = CreateDataObject("AMS", "123", new ZDateTime(2012, 3, 5));
			var additionalNumberDataObject3 = CreateDataObject("ZZZ", "456", new ZDateTime(2012, 3, 5));
			additionalNumberDataObject3.Type.Description = "Zod";
			var additionalNumberDataObject4 = CreateDataObject("AAA", "POLO", new ZDateTime(2012, 3, 5));

			var logger = new TestErrorLogger();
			var reader = new ShipmentAdditionalReferenceCollectionReader<CommonShipment>(new DataObjectList<AdditionalReference>(new[]
			{
				additionalNumberDataObject1, additionalNumberDataObject2, additionalNumberDataObject3, additionalNumberDataObject4
			}),
			logger, Factory, shipment);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Warning - Unknown Additional References were found for 'Shipment S00001000' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"COC|222|03-Mar-12", "AMS|123|05-Mar-12"
			},
			FormatNumbers(shipment));

			var notes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(1, notes.Length);
			AssertMultilineASCIIEquals("Note contents", "Type: ZZZ - Zod\r\nNumber: 456\r\n\r\nType: AAA\r\nNumber: POLO", notes[0].ST_NoteDataAsText);

			var additionalNumberDataObject5 = CreateDataObject("TTT", "TailorTot", new ZDateTime(2012, 3, 5));
			var newLogger = new TestErrorLogger();
			var newReader = new ShipmentAdditionalReferenceCollectionReader<CommonShipment>(new DataObjectList<AdditionalReference>(new[] { additionalNumberDataObject4, additionalNumberDataObject5 }), newLogger, Factory, shipment);

			newReader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Warning - Unknown Additional References were found for 'Shipment S00001000' and not imported. These were added to the 'Unrecognized Additional Reference Types' Note.
				".Trim(), newLogger.Logs);

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(),
			FormatNumbers(shipment));

			var updatedNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(1, updatedNotes.Length);
			AssertMultilineASCIIEquals("Note contents", "Type: ZZZ - Zod\r\nNumber: 456\r\n\r\nType: AAA\r\nNumber: POLO\r\n\r\nType: TTT\r\nNumber: TailorTot", updatedNotes[0].ST_NoteDataAsText);

			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var additionalNumberDataObject6 = CreateDataObject("SZB", "SZBNumber123", new ZDateTime(2012, 3, 5));
				var newLogger6 = new TestErrorLogger();
				var newReader6 = new ShipmentAdditionalReferenceCollectionReader<CommonShipment>(new DataObjectList<AdditionalReference>(new[] { additionalNumberDataObject6 }), newLogger6, Factory, shipment);
				newReader6.ReadIntoCollection();

				AssertMultilineASCIIEquals("logs for SZB number in country other than Germanny", @"Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), newLogger6.Logs);

				AssertCollectionContains("SZB number", "SZB|SZBNumber123|05-Mar-12", FormatNumbers(shipment));
			}
		}

		#region Implementation

		string[] FormatNumbers(CommonShipment shipment)
		{
			return shipment.Numbers
				.Cast<CusEntryNumber>()
				.Select(n => string.Format("{0}|{1}|{2}", n.CE_EntryType, n.CE_EntryNum, n.CE_IssueDate.ToShortDateString()))
				.ToArray();
		}

		AdditionalReference CreateDataObject(ZString code, ZString referenceNumber, ZDateTime issueDate)
		{
			var dataObject = new AdditionalReference();
			dataObject.Type = new EntryType { Code = code };
			dataObject.ReferenceNumber = referenceNumber;
			dataObject.IssueDate = issueDate;
			return dataObject;
		}

		#endregion
	}
}
