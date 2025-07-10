using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.CPTPP
{
	sealed class CPTPPDocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";

		const int MaxLineItems_Page1 = 5;
		const int MaxLineItems_ContinuationPage = 13;

		protected override ZGuid TemplatePivotPK => new ZGuid("DBCFF44C-AFC1-44BB-A512-462E210380E0");

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
				AssertContents(shipment, ShipmentDocumentNames.CPTPPCertificateOfOrigin, CreateContent(numberOfLineItems));
			}
		}

		protected override string CreateContent() => CreateContent(1);

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(1, LineItemSource.Invoice);

		string CreateContent(int numberOfLineItems, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
				var packlines1 = CreatePacklineContent(
				startRow: 27,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfLineItems, MaxLineItems_Page1),
				lineItemSource: lineItemSource);

			var pages = new List<string>();
			pages.Add(@$"[1,3] 1. Goods Consigned from (Exporter name, address, country, 
telephone and e-mail)
[1,27] Certificate No.
[2,43] Form CPTPP
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[4,26] COMPREHENSIVE AND PROGRESSIVE AGREEMENT 
FOR TRANS-PACIFIC PARTNERSHIP
[7,3] 2. If different from Exporter, Producer’s name, address,
country, telephone and e-mail
[8,26] CERTIFICATE OF ORIGIN
[9,3] MANUFACTURER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[12,29] Issued in
[13,3] 3. Goods Consigned to (Importer’s/Consignee’s name, 
address, country, telephone and e-mail)
[13,36] Country
[15,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE
[15,27] 5. For Official Use
[17,29] ☐   Preferential Treatment Given Under CPTPP
[19,29] ☐   Preferential Treatment Not Given 
       (Please state reason/s)
[20,3] 4. Means of transport and route (if known)
[21,3] Shipment Date:
[21,13] 11-Nov-2021
[22,3] Vessel’s name/Aircraft etc:
[22,13] VesselData/KH6754
[23,3] Port of Loading: 
[23,13] Auckland,New Zealand
[24,3] Port of Destination:
[24,13] Shenzhen Baoan International Apt,China
[24,26] Signature of Authorised Signatory of the Importing Country
[25,3] 6. Item 
number
[25,7] 7. Marks and
numbers on 
packages
[25,13] 8. Number and kind of packages; description of 
goods including HS Code (6 digits) for each
good
[25,31] 9. Origin 
Criterion (WO, 
PE, PSR or 
Other) for each good
[25,37] 10. Quantity (Gross
weight or Net),
quantity (quantity
unit) or other
measurements
[25,47] 11. Invoice
number(s) and
date(s)
{packlines1}
[37,3] 12. Declaration by the exporter
[37,27] 13. Certification
[38,3] The undersigned hereby declares that the above details
and statements are correct; that all the goods were 
produced in 
[38,27] I, the undersigned, being duly authorised by WiseTech Global to sign 
documentary evidence of origin of goods, hereby certify that, to the best 
of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary 
evidence of origin of goods on behalf of the company. 
[40,9] Australia
[41,9] (country)
[42,3] and that they comply with the rules, as provided in 
Chapter 3 of the Comprehensive and Progressive Agreement for Trans-Pacific Partnership for the goods exported to
[45,9] Singapore
[46,2] (importing country) 
[47,3] CargoWise Support, 01-Dec-2023
[51,2] Date, name and signature of authorised
 representative or exporter/producer
[51,26] Date, Name, Signature and Stamp 
of Authorised Officer
[53,3] 14
[53,5] ☐ Subject of non-party invoice
[53,18] ☐ Issued Retroactively
[53,35] ☐ De Minimis");

			if (numberOfLineItems > MaxLineItems_Page1)
			{
				var additionalLineItems = numberOfLineItems - MaxLineItems_Page1;
				var additionalPageCount = (additionalLineItems / MaxLineItems_ContinuationPage) + (additionalLineItems % MaxLineItems_ContinuationPage == 0 ? 0 : 1);
				for (var extraPageNumber = 0; extraPageNumber < additionalPageCount; extraPageNumber++)
				{
					var firstLineNumber = 54 + (extraPageNumber * 50);
					var continuationPacklines = CreatePacklineContent(
						startRow: 61 + (extraPageNumber * 50),
						firstRowLineNumber: (MaxLineItems_Page1 + 1) + (extraPageNumber * MaxLineItems_ContinuationPage),
						length: Math.Min(13, additionalLineItems - (extraPageNumber * MaxLineItems_ContinuationPage)),
						lineItemSource: lineItemSource);

					pages.Add(@$"[{firstLineNumber},27] Certificate No.
[{firstLineNumber + 1},43] Form CPTPP
[{firstLineNumber + 2},19] CPTPP Continuation Sheet
[{firstLineNumber + 5},3] 6. Item 
number
[{firstLineNumber + 5},7] 7. Marks and
numbers on 
packages
[{firstLineNumber + 5},13] 8. Number and kind of packages; description of 
goods including HS Code (6 digits) for each
good
[{firstLineNumber + 5},31] 9. Origin 
Criterion (WO, 
PE, PSR or 
Other) for each good
[{firstLineNumber + 5},37] 10. Quantity (Gross
weight or Net),
quantity (quantity
unit) or other
measurements
[{firstLineNumber + 5},47] 11. Invoice
number(s) and
date(s)
{continuationPacklines}
[{firstLineNumber + 33},3] 12. Declaration by the exporter
[{firstLineNumber + 33},27] 13. Certification
[{firstLineNumber + 34},3] The undersigned hereby declares that the above details
and statements are correct; that all the goods were 
produced in 
[{firstLineNumber + 34},27] I, the undersigned, being duly authorised by WiseTech Global to sign 
documentary evidence of origin of goods, hereby certify that, to the best 
of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary 
evidence of origin of goods on behalf of the company. 
[{firstLineNumber + 36},9] Australia
[{firstLineNumber + 37},9] (country)
[{firstLineNumber + 38},3] and that they comply with the rules, as provided in 
Chapter 3 of the Comprehensive and Progressive Agreement for Trans-Pacific Partnership for the goods exported to
[{firstLineNumber + 41},9] Singapore
[{firstLineNumber + 42},2] (importing country) 
[{firstLineNumber + 43},3] CargoWise Support, 01-Dec-2023
[{firstLineNumber + 47},2] Date, name and signature of authorised
 representative or exporter/producer
[{firstLineNumber + 47},26] Date, Name, Signature and Stamp 
of Authorised Officer
[{firstLineNumber + 49},3] 14
[{firstLineNumber + 49},5] ☐ Subject of non-party invoice
[{firstLineNumber + 49},18] ☐ Issued Retroactively
[{firstLineNumber + 49},35] ☐ De Minimis");
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
[{row},13] 3 Pallet DDD 123456
[{row},31] WO
[{row},37] 15  KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : $"[{row},47] 12 \n11-Nov-2021")}";
				}));
	}
}
