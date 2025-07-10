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
	sealed class PAFTADocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";

		protected override ZGuid TemplatePivotPK => new ZGuid("02797600-8670-4fdd-a89a-051618b39234");

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU", numberOfLineItems: 1);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentTwoPages() => AssertDocumentContent("AU", numberOfLineItems: 8);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentThreePages() => AssertDocumentContent("AU", numberOfLineItems: 24);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfLineItems = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfLineItems);
				AssertContents(shipment, ShipmentDocumentNames.PAFTACertificateOfOrigin, CreateContent(numberOfLineItems));
			}
		}

		protected override string CreateContent() => CreateContent(1);

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(1, LineItemSource.Invoice);

		string CreateContent(int numberOfLineItems, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var packlines1 = CreatePacklineContent(
				startRow: 24,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfLineItems, 7),
				lineItemSource: lineItemSource);
			var additionalPacklines = numberOfLineItems - 7;
			var totalPageCount = additionalPacklines > 0
								? 1 + (int)Math.Ceiling((additionalPacklines) / 14.0)
								: 1;
			var pages = new List<string>();
			pages.Add(@$"[1,3] 1. Exporter’s name, address, country, telephone and email
[1,27] Certificate No.
[1,43] Form PAFTA
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA


[2,27] PERU-AUSTRALIA FREE TRADE AGREEMENT
[5,27] CERTIFICATE OF ORIGIN
[7,3] 2. Consignor's name, address, country, telephone and email
[7,30] Issued in
[8,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE


[8,36] Country
[9,27] Page 1 of {totalPageCount}
[11,27] 4. For Official Use
[12,29] ☐   Preferential Treatment Given Under PAFTA
[13,3] 3. Means of transport and route (if known)
[16,3] Shipment Date:
[16,15]  11-Nov-2021
[16,29] ☐   Preferential Treatment Not Given (Please state reason)
[17,3] Vessel’s name/Aircraft etc.:
[17,15] VesselData/KH6754
[18,3] Port of Loading:
[18,15] Auckland
[19,3] Port of Destination:
[19,15] Shenzhen Baoan International Apt
[20,26] Signature of Customs Official of the Importing Country
[21,3] 5. Item
number
[21,7] 6. Marks and
numbers on
packages
[21,12] 7. Number and kind of packages; Quantity, Description of goods including HS Code (6 digits) for each good
[21,33] 8. Origin Criterion (WO,PE, RVC) for each good
[21,38] 9. Weight (Gross weight or Net)
[21,45] 10. Invoice number(s) and date(s)
{packlines1}
[38,3] 11. Declaration
[38,27] 12. Certification
[39,3] I, the undersigned, declare that the goods described in this document and related statements are correct, and that all goods were produced in:
[39,27] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[41,8] Australia
[42,5] (Country)
[43,3] And that they comply with the rules of origin as provided in Chapter 3 of the Peru-Australia Free Trade Agreement for the goods exporter to: 
[46,10] PERU
[47,5] (importing country)
[50,3] Eagle Datamation International
, 01-Dec-2023
[52,5] Date, Name, Company Name and Signature of 
Authorised Representative of exporter
[52,27] Date, Name, Signature and Stamp
of Authorised Officer
[53,3] 13. Other Specifications
[54,13] ☐
[54,15] De Minimis
[54,27] ☐
[54,29]  Accumulation");

			if (totalPageCount > 1)
			{
				for (var extraPageNumber = 0; extraPageNumber < totalPageCount - 1; extraPageNumber++)
				{
					var firstLineNumber = 56 + (extraPageNumber * 54);
					var continuationPacklines = CreatePacklineContent(
						startRow: 60 + (extraPageNumber * 54),
						firstRowLineNumber: 8 + (extraPageNumber * 16),
						length: Math.Min(16, additionalPacklines - (extraPageNumber * 16)),
						lineItemSource: lineItemSource);

					pages.Add(@$"[{firstLineNumber},2] Continuation Page
[{firstLineNumber},13] Page {extraPageNumber + 2} of {totalPageCount}
[{firstLineNumber},27] Certificate No.
[{firstLineNumber},44] Form PAFTA
[{firstLineNumber + 1},3] 5. Item
number
[{firstLineNumber + 1},7] 6. Marks and
numbers on
packages
[{firstLineNumber + 1},12] 7. Number and kind of packages; Quantity, Description of goods including HS Code (6 digits) for each good
[{firstLineNumber + 1},33] 8. Origin Criterion (WO,PE, RVC) for each good
[{firstLineNumber + 1},38] 9. Weight (Gross weight or Net)
[{firstLineNumber + 1},45] 10. Invoice number(s) and date(s)
{continuationPacklines}
[{firstLineNumber + 36},3] 11. Declaration
[{firstLineNumber + 36},27] 12. Certification
[{firstLineNumber + 37},3] I, the undersigned, declare that the goods described in this document and related statements are correct, and that all goods were produced in:
[{firstLineNumber + 37},27] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[{firstLineNumber + 39},8] Australia
[{firstLineNumber + 40},5] (Country)
[{firstLineNumber + 41},3] And that they comply with the rules of origin as provided in Chapter 3 of the Peru-Australia Free Trade Agreement for the goods exporter to: 
[{firstLineNumber + 44},10] PERU
[{firstLineNumber + 45},5] (importing country)
[{firstLineNumber + 48},3] Eagle Datamation International
, 01-Dec-2023
[{firstLineNumber + 50},5] Date, Name, Company Name and Signature of 
Authorised Representative of exporter
[{firstLineNumber + 50},27] Date, Name, Signature and Stamp
of Authorised Officer
[{firstLineNumber + 51},3] 13. Other Specifications
[{firstLineNumber + 52},13] ☐
[{firstLineNumber + 52},15] De Minimis
[{firstLineNumber + 52},27] ☐
[{firstLineNumber + 52},29]  Accumulation");
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
