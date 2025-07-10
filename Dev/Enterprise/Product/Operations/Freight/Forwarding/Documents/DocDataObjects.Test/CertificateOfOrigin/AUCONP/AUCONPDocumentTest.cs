using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AUCONPDocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.Australia;

		protected override ZGuid TemplatePivotPK => new ZGuid("6E968C87-5B8D-4AF5-8A49-31FBC8E6E3C9");

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent(Constants.CountryCodes.Australia, numberOfPacklines: 1);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentTwoPages() => AssertDocumentContent(Constants.CountryCodes.Australia, numberOfPacklines: 14);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentThreePages() => AssertDocumentContent(Constants.CountryCodes.Australia, numberOfPacklines: 30);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent(Constants.CountryCodes.Australia, numberOfPacklines: 100);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfPacklines = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfPacklines);
				AssertContents(shipment, ShipmentDocumentNames.AUCONPCertificateOfOrigin, CreateContent(numberOfPacklines));
			}
		}

		protected override string CreateContent() => CreateContent(1);

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(1, LineItemSource.Invoice);

		protected override void PopulatePackingLineItem(ForwardingPackLine lineItem) => lineItem.JL_RN_NKOrigin = Constants.CountryCodes.Australia;

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.Australia;

		string CreateContent(int numberOfPacklines, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var pagesCount = numberOfPacklines <= 8 ? 1 : 1 + (int)Math.Ceiling((numberOfPacklines - 8) / 16.0);
			var packlines1 = CreatePacklineContent(
				startRow: 21,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfPacklines, 8),
				lineItemSource: lineItemSource);

			var pages = new List<string>();
			pages.Add(@$"[2,3] 1. Exporter/Consignor (Name and Address)
[2,27] Certificate No.
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[3,27] CERTIFICATE OF ORIGIN
[7,3] 2. Consignee (Name and Address)
[8,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE
[8,31] ISSUED IN
[9,27] AUSTRALIA
[12,3] 3. Means of transport and route (if known)
[12,27] 4. Exporter Reference
[14,3] Departure Date:
[14,15]  11-Nov-2021
[15,3] Vessel’s name/Aircraft etc.:
[15,15] VesselData/KH6754
[15,27] 5. Remarks
[16,3] Port of Loading:
[16,15] Auckland,New Zealand
[17,3] Port of Destination:
[17,15] Shenzhen Baoan International Apt,China
[19,3] 6. Number
and Kind of
Packages
[19,7] 7. Marks and 
numbers
[19,13] 8. Description of Goods
[19,33] 9. HS Code
(6 digits)
[19,39] 10. Country
of Origin
[19,45] 11. Weight (Gross
or Net)
{packlines1}
[37,3] 12. Declaration by the exporter
[37,27] 13. Certification
[38,3] I, the undersigned, being duly authorised by the exporter above,
declare that the details shown above are true and correct. I further
declare that the Customs Authority of the importing country (or
their nominee) will be furnished with such evidence as may be
requested for the purpose of verifying this declaration.
[38,27] I, the undersigned, being duly authorised by WiseTech Global
Certification to sign documentary evidence of origin of goods,
hereby certify that to the best of my knowledge and belief, this
document has been signed by an authorised officer of the
applicant company to sign documentary evidence of origin of
goods on behalf of the company.
[49,3] Eagle Datamation International
, 01-Dec-2023
[51,5] Place and date, name, and signature 
of authorised representative
[51,27] Place and date, name and signature and stamp of Authorised 
Officer and Issuing Authority/Body
[52,23] Page  1  of  {pagesCount}");

			if (numberOfPacklines > 7)
			{
				var additionalPacklines = numberOfPacklines - 8;
				var additionalPageCount = (additionalPacklines / 16) + (additionalPacklines % 16 == 0 ? 0 : 1);
				for (var extraPageNumber = 0; extraPageNumber < additionalPageCount; extraPageNumber++)
				{
					var firstLineNumber = 54 + (extraPageNumber * 53);
					var continuationPacklines = CreatePacklineContent(
						startRow: 58 + (extraPageNumber * 53),
						firstRowLineNumber: 8 + (extraPageNumber * 16),
						length: Math.Min(16, additionalPacklines - (extraPageNumber * 16)),
						lineItemSource: lineItemSource);

					pages.Add(@$"[{firstLineNumber},3] Certificate of Origin Continuation Sheet 
[{firstLineNumber},25] Certificate No.
[{firstLineNumber + 2},3] 6. Number
and Kind of
Packages
[{firstLineNumber + 2},7] 7. Marks and 
numbers
[{firstLineNumber + 2},13] 8. Description of Goods
[{firstLineNumber + 2},33] 9. HS Code
(6 digits)
[{firstLineNumber + 2},39] 10. Country
of Origin
[{firstLineNumber + 2},45] 11. Weight (Gross
or Net)
{continuationPacklines}
[{firstLineNumber + 36},3] 12. Declaration by the exporter
[{firstLineNumber + 36},27] 13. Certification
[{firstLineNumber + 37},3] I, the undersigned, being duly authorised by the exporter above,
declare that the details shown above are true and correct. I further
declare that the Customs Authority of the importing country (or
their nominee) will be furnished with such evidence as may be
requested for the purpose of verifying this declaration.
[{firstLineNumber + 37},27] I, the undersigned, being duly authorised by WiseTech Global
Certification to sign documentary evidence of origin of goods,
hereby certify that to the best of my knowledge and belief, this
document has been signed by an authorised officer of the
applicant company to sign documentary evidence of origin of
goods on behalf of the company.
[{firstLineNumber + 48},3] Eagle Datamation International
, 01-Dec-2023
[{firstLineNumber + 50},5] Place and date, name, and signature 
of authorised representative
[{firstLineNumber + 50},27] Place and date, name and signature and stamp of Authorised 
Officer and Issuing Authority/Body
[{firstLineNumber + 51},23] Page  {extraPageNumber + 2}  of  {pagesCount}");
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
					return @$"[{row},3] 3 Pallet
{(lineItemSource is LineItemSource.PackLine ? $"[{row},7] Marks 1" : string.Empty)}
[{row},12] DDD 
[{row},33] 123456
[{row},38] AU
[{row},45] 15.00 KG";
				}));
	}
}
