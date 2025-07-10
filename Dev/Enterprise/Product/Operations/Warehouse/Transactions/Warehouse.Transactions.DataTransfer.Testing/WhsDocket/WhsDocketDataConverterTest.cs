using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public abstract class WhsDocketDataConverterTest : WhsTestCaseWithFactory
	{
		[TestDate(2005, 1, 1)]
		public void TestMapImport()
		{
			Xsd.WhsDockets xsdDockets = new Xsd.WhsDockets();
			Converter.MapImportForTest(xsdDockets, GetRowsCollection());
			AssertEquals("2 whs dockets should have been populated", 2, xsdDockets.WhsDocket.Count);
			Xsd.WhsDocket xsdWhsDocket = xsdDockets.WhsDocket[0];
			AssertNotNull(xsdWhsDocket.Identifier.Client);

			AssertOrgDetail(xsdWhsDocket.Identifier.Client, "DANPACWLG", "Alcan Packaging Danaflex", "NZWLG");

			AssertEquals("43924", xsdWhsDocket.Identifier.Reference);
			AssertEquals("BNE", xsdWhsDocket.DocketDetail.WarehouseCode);
			AssertEquals("2005", xsdWhsDocket.DocketDetail.CustomerReference);
			AssertEquals("TRREF", xsdWhsDocket.DocketDetail.TransportReference);
			AssertEquals("D2D", xsdWhsDocket.DocketDetail.TransportServiceLevel);
			AssertEquals("TSL", xsdWhsDocket.DocketDetail.ServiceLevel);
			AssertEquals(15m, xsdWhsDocket.DocketDetail.Units);

			AssertEquals(1, xsdWhsDocket.DocketDetail.References.Count);
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber, xsdWhsDocket.DocketDetail.References[0].Type);
			AssertEquals("20240329", xsdWhsDocket.DocketDetail.References[0].Value);
			AssertNotNull(xsdWhsDocket.DocketDetail.TransportCompany.AddressReference.Organisation);
			AssertOrgDetail(xsdWhsDocket.DocketDetail.TransportCompany.AddressReference.Organisation, "ALLTRANS", "ALL TRANSPORT Australia", "AUSYD");

			AssertOrgDetail(xsdWhsDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation, "TRANSBILL", "Transport Bill To Organisation", "AUSYD");

			AssertNotes(xsdWhsDocket.Notes, Xsd.NotesNoteNoteType.HandlingInstructions, "Goods Handling Notes");
			AssertNotes(xsdWhsDocket.Notes, Xsd.NotesNoteNoteType.DangerousGoodsAdditionalHandlingInformation, "DG Additional Notes");
			AssertNotes(xsdWhsDocket.Notes, Xsd.NotesNoteNoteType.DeliveryInstructionsNote, "DeliveryInstructions");

			AssertHeaderOtherDetail(xsdWhsDocket);

			AssertEquals("XsdWhsDocket.DocketLines should have one line", 1, xsdWhsDocket.DocketLines.Count);

			Xsd.WhsDocketLine xsdWhsDocketLine = xsdWhsDocket.DocketLines[0];
			AssertEquals((ZShort)1, xsdWhsDocketLine.LineNumber);
			AssertEquals("X8434", xsdWhsDocketLine.Product);
			AssertEquals("SK10 PLAIN MARAFLEX 380MM TUBING", xsdWhsDocketLine.Description);

			AssertEquals(6800.0m, xsdWhsDocketLine.QuantityFromClientOrder);
			AssertEquals(6500.0m, xsdWhsDocketLine.QuantityActuallyOrdered);
			AssertEquals("M", xsdWhsDocketLine.ProductUQ);

			AssertEquals("", xsdWhsDocketLine.LineAttributes.BondedEntryKey);
			AssertEquals(ZDateTime.Empty, xsdWhsDocketLine.LineAttributes.ExpiryDate);
			AssertEquals(ZDateTime.Empty, xsdWhsDocketLine.LineAttributes.PackingDate);
			AssertEquals("24799", xsdWhsDocketLine.LineAttributes.PartAttribute1);
			AssertEquals("6500", xsdWhsDocketLine.LineAttributes.PartAttribute2);
			AssertEquals("01.2335.00", xsdWhsDocketLine.LineAttributes.PartAttribute3);
			AssertEquals("", xsdWhsDocketLine.LineComments);

			AssertLineOtherDetail(xsdWhsDocketLine);
		}

		public void TestImportWithWrongVersion()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			FlatFileDataRow row = new FlatFileDataRow(5);
			row[0] = WhsDocketConstants.HeaderType;
			row[1] = "0.5";
			Xsd.WhsDockets xsdDockets = new Xsd.WhsDockets();
			Converter.MapImportForTest(xsdDockets, rows);

			Assert("Notify should have errors", Notify.HasErrors);
			Assert("ErrorType should be 'InvalidFileFormat'", Notify.ContainsNotificationType(ErrorType.InvalidFileFormat));
			Assert("Error Message:", Notify.AsString.Contains("Invalid File - The file is either empty or its version number is not supported"));
		}

		public void TestImportFileWithWrongTypeLines()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			FlatFileDataRow row = new FlatFileDataRow(5);
			row[0] = "ABC";
			row[1] = "1.0";
			rows.Add(row);

			Xsd.WhsDockets xsdDockets = new Xsd.WhsDockets();
			Converter.MapImportForTest(xsdDockets, rows);

			Assert("Notify should have errors", Notify.HasErrors);
			Assert("ErrorType should be 'UnknownRecordType'", Notify.ContainsNotificationType(ErrorType.UnknownRecordType));
			Assert("Error Message:", Notify.AsString.Contains("Unidentifiable line. Record Type unknown: ABC"));
		}

		#region Implementation

		void AssertNotes(Xsd.NotesNoteCollection notes, Xsd.NotesNoteNoteType noteType, ZString noteValue)
		{
			Xsd.NotesNote note = notes.GetNote(noteType);
			AssertEquals(noteType, note.NoteType);
			AssertEquals(noteValue, note.NoteData);
		}

		protected void AssertOrgDetail(Xsd.Organisation xsdOrg, ZString ownerCode, ZString name, ZString uNLOCO)
		{
			Assert("Organisation should be specified", xsdOrg.IsSpecified);
			AssertEquals(ownerCode, xsdOrg.OwnerCode);
			AssertEquals(name, xsdOrg.OrganisationDetails.Name);
			AssertEquals(uNLOCO, xsdOrg.OrganisationDetails.Location.Value);
		}

		protected WhsDocketDataConverter Converter
		{
			get { return GetDataConverter(); }
		}

		protected virtual FlatFileDataRowCollection GetRowsCollection()
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();

			rows.Add(new FlatFileDataRow(Header1));
			rows.Add(new FlatFileDataRow(Line1));
			rows.Add(new FlatFileDataRow(Header2));
			rows.Add(new FlatFileDataRow(Line2));

			return rows;
		}

		protected virtual string[] Header1
		{
			get { return new string[] { "WOH", "1.0", "Rob.Hunter@alcan.com", "43924", "2005", "", "ORD", "MOP", "20061004", "", "", "", "BNE", "0", "", "", "TRREF", "D2D", "TSL", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "DANPACWLG", "Rob Hunter", "Alcan Packaging Danaflex", "101 Collins Ave", "Linden", "Wellington", "WGN", "6006", "NZ", "New Zealand", "NZWLG", "+64 4 2320880", "Rob.Hunter@alcan.com", "", "", "KILABABNE", "John Bradfield", "KILLARNEY ABATTOIR Pty Ltd", "", "KILLARNEY", "", "QLD", "", "AU", "Australia", "AUBNE", "0061 7466 41244", "test@KILLARNEY.com", "", "", "DANPACWLG", "Rob Hunter", "Alcan Packaging Danaflex", "101 Collins Ave", "Linden", "Wellington", "WGN", "6006", "NZ", "New Zealand", "NZWLG", "+64 4 2320880", "Rob.Hunter@alcan.com", "", "", "ALLTRANS", "Dylan Paul", "ALL TRANSPORT Australia", "125 Victoria Ave", "Camden", "Camden", "NSW", "2145", "AU", "Australia", "AUSYD", "+61 2 98897854", "Dylan.Paul@alltr.com", "", "", "TRANSBILL", "Charles Chils", "Transport Bill To Organisation", "234 Maroubra Road", "Sydney", "Sydney", "NSW", "1230", "AU", "Australia", "AUSYD", "+61 2 00023321", "Charles.Chils@trbill.com", "", "", "Goods Handling Notes", "DG Additional Notes", "DeliveryInstructions", "20240329" }; }
		}

		protected virtual string[] Line1
		{
			get { return new string[] { "WOL", "1", "", "X8434", "SK10 PLAIN MARAFLEX 380MM TUBING", "", "", "", "", "", "", "6500.000", "M", "", "", "0", "0", "0", "0", "0", "", "", "24799", "6500", "01.2335.00", "ATTR1", "ATTR2", "ATTR3", "SERRN2", "20061005", "20061004", "Please despatch 6500.00 M (13 Cartons) from pallet 24799" }; }
		}

		readonly string[] Header2 = { "WOH", "1.0", "Rob.Hunter@alcan.com", "43925", "2006", "", "ORD", "MOP", "20061004", "", "", "", "BNE", "0", "", "", "", "", "", "15", "", "", "", "", "", "", "", "", "", "", "", "", "", "DANPACWLG", "Rob Hunter", "Alcan Packaging Danaflex", "101 Collins Ave", "Linden", "Wellington", "", "6006", "NZ", "New Zealand", "NZWLG", "+64 4 2320880", "Rob.Hunter@alcan.com", "", "", "KILABABNE", "John Bradfield", "KILLARNEY ABATTOIR Pty Ltd", "", "KILLARNEY", "", "QLD", "", "AU", "Australia", "AUBNE", "0061 7466 41244", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
		readonly string[] Line2 = { "WOL", "1", "", "X8434", "SK10 PLAIN MARAFLEX 380MM TUBING", "", "", "", "", "", "", "6500.000", "M", "", "", "0", "0", "0", "0", "0", "", "", "24799", "6500", "01.2335.00", "", "", "", "", "", "20061004", "Please despatch 6500.00 M (13 Cartons) from pallet 24799" };

		protected abstract WhsDocketDataConverter GetDataConverter();
		protected abstract void AssertHeaderOtherDetail(Xsd.WhsDocket xsdWhsDocket);
		protected abstract void AssertLineOtherDetail(Xsd.WhsDocketLine xsdWhsDocketLine);

		#endregion
	}
}
