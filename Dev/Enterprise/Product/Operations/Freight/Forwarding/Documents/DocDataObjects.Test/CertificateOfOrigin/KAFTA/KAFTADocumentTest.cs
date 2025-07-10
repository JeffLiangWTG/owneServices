using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class KAFTADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";

		protected override ZGuid TemplatePivotPK => new ZGuid("A3FDD515-02AF-4F30-A0BA-6775D456154E");

		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU");

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_TwoPages() => AssertDocumentContent("AU", numberOfPacklines: 10);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_ThreePages() => AssertDocumentContent("AU", numberOfPacklines: 28);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent("AU", numberOfPacklines: 100);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfPacklines = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfPacklines);

				AssertContents(shipment, ShipmentDocumentNames.KAFTACertificateOfOrigin, CreateContent(numberOfPacklines));
			}
		}

		#region Implement

		string CreateContent(int numberOfPacklines = 1,
			string packlineOriginCriterion = OriginCriterionListKAFTA.Codes.WO,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var pagesCount = numberOfPacklines <= 6 ? 1 : 1 + (int)Math.Ceiling((double)(numberOfPacklines - 6) / 10);

			var packlines = CreatePacklineContent(
				startRow: 18,
				length: Math.Min(numberOfPacklines, 6),
				lineItemSource,
				packlineOriginCriterion);

			var pages = new List<string>
			{
				@$"[1,3] 1. Exporter’s name, address, country, telephone and email
[1,27] Issuing No.
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[2,43] Form KAFTA
[4,27] AUSTRALIA – KOREA FREE TRADE AGREEMENT
[6,27] Issued in ……………………………………
[7,27] Country
[8,27] THIS CERTIFICATE COVERS ONE SHIPMENT ONLY
[9,3] 2. If different from Exporter, Producer’s name, address, country, telephone, and email (Optional)
[9,27] 3. Importer’s/Consignee’s name, address, country, telephone and email (Optional)
[10,3] 
MANUFACTURER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[10,27] CONSIGNEE
UNIT 100
11 WHY STREET
SEOUL CITY 11 01004
KOREA, REPUBLIC OF
[14,3] 4. Marks and numbers on packages
[14,9] 5. Number and kind of packages; Quantity, Description of goods 
[14,29] 6. HS Code (6 digits) for each good
[14,35] 7. Origin Criterion (WO, PE, PSR, Other) for each good
[14,41] 8. Weight (Gross weight or Net) 
[14,47] 9. Invoice number(s) and date(s)
{packlines}
[36,3] 10. Observations (optional)
[38,3] 11. Declaration
[38,29] 12. Certification
[39,3] The information in this document is true and accurate and I assume 
the responsibility for proving such representations. I understand that I 
am liable for any false statements or material omissions made on or in 
connection with this document.
[39,29] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[43,3] I agree to maintain, and present upon request, documentation 
necessary to support this Certificate, and to inform, in writing, all 
persons to whom the Certificate was given of any changes that would 
affect the accuracy or validity of this Certificate.
[46,11] Australia
[47,3] The goods originate in …………………………… (the territory of one or 
both of the Parties) and comply with the origin requirements specified 
for those goods in the Australia - Korea Free Trade Agreement. 
[50,3] This Certificate consists of
[50,13] {pagesCount}
[50,17] pages, including all attachments.
[52,3] 01-Dec-2023
[54,3] CargoWise Support, EDI CUSTOMS BROKERS, Australia
[55,3] ………………………………………………………………………
[55,29] ………………………………………………………………
[56,3] Signature, Name, Company Name, Address, telephone, email of 
Signatory and Date
[56,29] Date, Name, Signature and Stamp
of Authorised Officer"
			};

			if (numberOfPacklines > 6)
			{
				for (var extraPageNumber = 0; extraPageNumber < pagesCount - 1; extraPageNumber++)
				{
					var firstLineNumber = 58 + (extraPageNumber * 59);
					var continuationPacklines = CreatePacklineContent(
						startRow: 65 + (extraPageNumber * 59),
						length: Math.Min(10, (numberOfPacklines - 6) - (extraPageNumber * 10)),
						lineItemSource,
						packlineOriginCriterion);

					pages.Add($@"
[{firstLineNumber},27] Issuing No.
[{firstLineNumber + 1},3] KAFTA Continuation Sheet
[{firstLineNumber + 1},47] Form KAFTA
[{firstLineNumber + 3},3] 4. Marks and numbers on packages
[{firstLineNumber + 3},9] 5. Number and kind of packages; Quantity, Description of goods 
[{firstLineNumber + 3},29] 6. HS Code (6 digits) for each good
[{firstLineNumber + 3},35] 7. Origin Criterion (WO, PE, PSR, Other) for each good
[{firstLineNumber + 3},41] 8. Weight (Gross weight or Net) 
[{firstLineNumber + 3},47] 9. Invoice number(s) and date(s)
{continuationPacklines}
[{firstLineNumber + 37},3] 10. Observations (optional)
[{firstLineNumber + 39},3] 11. Declaration
[{firstLineNumber + 39},29] 12. Certification
[{firstLineNumber + 40},3] The information in this document is true and accurate and I assume 
the responsibility for proving such representations. I understand that I 
am liable for any false statements or material omissions made on or in 
connection with this document.
[{firstLineNumber + 40},29] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[{firstLineNumber + 44},3] I agree to maintain, and present upon request, documentation 
necessary to support this Certificate, and to inform, in writing, all 
persons to whom the Certificate was given of any changes that would 
affect the accuracy or validity of this Certificate.
[{firstLineNumber + 47},11] Australia
[{firstLineNumber + 48},3] The goods originate in …………………………… (the territory of one or 
both of the Parties) and comply with the origin requirements specified 
for those goods in the Australia - Korea Free Trade Agreement. 
[{firstLineNumber + 51},3] This Certificate consists of
[{firstLineNumber + 51},13] {pagesCount}
[{firstLineNumber + 51},17] pages, including all attachments.
[{firstLineNumber + 53},3] 01-Dec-2023
[{firstLineNumber + 55},3] CargoWise Support, EDI CUSTOMS BROKERS, Australia
[{firstLineNumber + 56},3] ………………………………………………………………………
[{firstLineNumber + 56},29] ………………………………………………………………
[{firstLineNumber + 57},3] Signature, Name, Company Name, Address, telephone, email of 
Signatory and Date
[{firstLineNumber + 57},29] Date, Name, Signature and Stamp
of Authorised Officer");
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(int startRow, int length, LineItemSource lineItemSource, string originCriterion = OriginCriterionListKAFTA.Codes.WO)
		{
			var packlineContent = string.Join(
				"\r\n",
				Enumerable
					.Range(0, Math.Max(1, length))
					.Select(x =>
					{
						var row = startRow + (x * 3);
						return @$"{(lineItemSource is LineItemSource.PackLine ? $"[{row},3] Marks 1" : string.Empty)}
[{row},9] 3 PLT  DDD
[{row},29] 123456
[{row},41] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : $"[{row},47] 12 \n11-Nov-2021")}
[{(row + 1)},35] {originCriterion}";
					}));

			return packlineContent;
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "KRSEL";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "KRSEL";
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "KRSEL";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "KRSEL";
		}

		protected override OrgHeader CreateConsignorAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Consignor");

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.KoreaSouth, fullName: "Consignee");

		protected override OrgHeader CreateManufacturerAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Manufacturer");

		#endregion
	}
}
