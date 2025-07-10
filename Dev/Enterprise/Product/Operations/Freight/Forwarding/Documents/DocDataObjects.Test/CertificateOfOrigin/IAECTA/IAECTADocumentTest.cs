using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class IAECTADocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";

		protected override ZGuid TemplatePivotPK => new ZGuid("479FD694-C0A3-4150-8513-1DDE7D44E0A4");

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU", numberOfPacklines: 1);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentTwoPages() => AssertDocumentContent("AU", numberOfPacklines: 7);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentThreePages() => AssertDocumentContent("AU", numberOfPacklines: 24);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent("AU", numberOfPacklines: 100);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfPacklines = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfPacklines);
				AssertContents(shipment, ShipmentDocumentNames.IAECTACertificateOfOrigin, CreateContent(numberOfPacklines));
			}
		}

		protected override string CreateContent() => CreateContent();

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		string CreateContent(int numberOfPacklines = 1, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var packlines1 = CreatePacklineContent(
				startRow: 22,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfPacklines, 6),
				lineItemSource);
			var additionalPacklines = numberOfPacklines - 6;
			var totalPageCount = additionalPacklines > 0
								? 1 + (int)Math.Ceiling((additionalPacklines) / 14.0)
								: 1;
			var pages = new List<string>();
			pages.Add(@$"[2,2]  1. Exporter’s name, address, country, telephone and email
[2,26]  Certificate No.
[2,43]         Form IA-ECTA
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA


[3,26] INDIA-AUSTRALIA ECONOMIC COOPERATION
AND TRADE AGREEMENT
[6,26] CERTIFICATE OF ORIGIN
[8,3] 2. If different from Exporter, Producer’s name, address,
country, telephone, and email
[9,3] MANUFACTURER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA


[11,26] Page 1 of {totalPageCount}
[12,3] 3. Importer’s/Consignor's name, address, country, telephone and email (if known)
[12,26] 4. Means of transport and route (if known)
[13,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE


[15,26]  Shipment Date:
[15,39]  11-Nov-2021
[16,26]  Vessel’s name/Aircraft etc.:
[16,39] VesselData/KH6754
[17,26]  Port of Loading:
[17,39] Auckland
[18,26]  Port of Destination:
[18,39] Singapore
[19,3] 5. Item
number
[19,7] 6. Marks and
numbers on
packages
[19,12] 7. Number and kind of packages; description of goods including HS    Code (6 digits) for each good
[19,33] 8. Origin Criterion (WO, PE, RVC) for each good
[19,38] 9. Weight (Gross weight     or Net)
[19,45] 10. Invoice number(s) and date(s)
{packlines1}
[34,3] 11. Non-Party Invoice details (if applicable)
[36,3] 12. Declaration by the exporter
[36,27] 13. Certification
[37,3] I, the undersigned, declare that, the above details and statement
are true and accurate; the good(s) described above meet the
condition(s) required for the issuance of this certificate; and the
country of origin of the good(s) described above is:
[37,27] It is hereby certified, on the basis of control carried out, that the
declaration of the exporter is correct
[39,5] Australia
[40,5] (country)
[43,3] CargoWise Support
Eagle Datamation International
, 01-Dec-2023
[45,4] Place and date, name, signature and
Company of authorised signatory
[45,27] Place and date, signature and stamp of Authorised
Issuing Authority/Body
[46,3] 14
[46,4] ☐
[46,5]  Subject of non-party invoice 
[46,17] ☐
[46,18] Issued Retrospectively 
[46,35] ☐
[46,36] Cumulation
[48,5] Export Document Number (if applicable) ______________________________");

			if (totalPageCount > 1)
			{
				for (var extraPageNumber = 0; extraPageNumber < totalPageCount - 1; extraPageNumber++)
				{
					var firstLineNumber = 50 + (extraPageNumber * 48);
					var continuationPacklines = CreatePacklineContent(
						startRow: 54 + (extraPageNumber * 48),
						firstRowLineNumber: 7 + (extraPageNumber * 14),
						length: Math.Min(14, additionalPacklines - (extraPageNumber * 14)),
						lineItemSource);

					pages.Add(@$"[{firstLineNumber},2] Continuation page
[{firstLineNumber},13] Page {extraPageNumber + 2} of {totalPageCount}
[{firstLineNumber},26] Certificate No.
[{firstLineNumber},44] Form IA-ECTA  
[{firstLineNumber + 1},3] 5. Item
number
[{firstLineNumber + 1},7] 6. Marks and
numbers on
packages
[{firstLineNumber + 1},12] 7. Number and kind of packages; description of goods including HS    Code (6 digits) for each good
[{firstLineNumber + 1},33] 8. Origin Criterion (WO, PE, RVC) for each good
[{firstLineNumber + 1},38] 9. Weight (Gross weight     or Net)
[{firstLineNumber + 1},45] 10. Invoice number(s) and date(s)
{continuationPacklines}
[{firstLineNumber + 32},3] 11. Non-Party Invoice details (if applicable)
[{firstLineNumber + 34},3] 12. Declaration by the exporter
[{firstLineNumber + 34},27] 13. Certification
[{firstLineNumber + 35},3] I, the undersigned, declare that, the above details and statement
are true and accurate; the good(s) described above meet the
condition(s) required for the issuance of this certificate; and the
country of origin of the good(s) described above is:
[{firstLineNumber + 35},27] It is hereby certified, on the basis of control carried out, that the
declaration of the exporter is correct
[{firstLineNumber + 37},3] Australia
[{firstLineNumber + 38},4] (country)
[{firstLineNumber + 40},3] CargoWise Support
Eagle Datamation International
, 01-Dec-2023
[{firstLineNumber + 42},4] Place and date, name, signature and
Company of authorised signatory
[{firstLineNumber + 42},27] Place and date, signature and stamp of Authorised
Issuing Authority/Body
[{firstLineNumber + 44},3] 14
[{firstLineNumber + 44},4] ☐
[{firstLineNumber + 44},5]  Subject of non-party invoice 
[{firstLineNumber + 44},17] ☐
[{firstLineNumber + 44},19] Issued Retrospectively 
[{firstLineNumber + 44},35] ☐
[{firstLineNumber + 44},37] Cumulation
[{firstLineNumber + 46},5] Export Document Number (if applicable) ______________________________");
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(int startRow, int firstRowLineNumber, int length, LineItemSource lineItemSource) => string.Join(
			"\r\n",
			Enumerable
				.Range(0, Math.Max(1, length))
				.Select(x =>
				{
					var row = startRow + (x * 2);
					return @$"[{row},3] {x + firstRowLineNumber}
{(lineItemSource is LineItemSource.PackLine ? $"[{row},7] Marks 1" : string.Empty)}
[{row},12] 3 Pallet DDD 123456
[{row},33] WO
[{row},38] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : $"[{row},45] 12 \n11-Nov-2021")}";
				}));
	}
}
