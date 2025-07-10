using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class IACEPADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.Australia;

		protected override ZGuid TemplatePivotPK => new ZGuid("3279BD3C-2D00-4C68-859A-40775DB13518");

		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU", numberOfPacklines: 1);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_TwoPages() => AssertDocumentContent("AU", numberOfPacklines: 9);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_ThreePages() => AssertDocumentContent("AU", numberOfPacklines: 21);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent("AU", numberOfPacklines: 100);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfPacklines = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfPacklines);

				AssertContents(shipment, ShipmentDocumentNames.IACEPACertificateOfOrigin, CreateContent(numberOfPacklines));
			}
		}

		#region Implement

		string CreateContent(int numberOfPacklines = 1,
			string packlineOriginCriterion = OriginCriterionListIACEPA.Codes.WO,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var packlines = CreatePacklineContent(
				startRow: 22,
				length: Math.Min(numberOfPacklines, 3),
				firstRowLineNumber: 1,
				lineItemSource,
				packlineOriginCriterion);

			var pages = new List<string>
			{
				@$"[1,3] 1. Goods Consigned from (Exporter’s name, address, country
& contact details)
[1,27] Certificate No.
[1,43] Form IA-CEPA 
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA    
[2,27] Indonesia-Australia Comprehensive Economic Partnership Agreement (IA-CEPA)
[5,27]       CERTIFICATE OF ORIGIN
[7,3] 2. Goods Consigned to (Importer’s/Consignee’s name,
address, country)
[7,29] Issued In
[7,35] Australia
[8,3] CONSIGNEE
UNIT 100
11 WHY STREET
INDONESIA CITY 666
INDONESIA
[8,35] Country
[9,27] 4. For official use
[10,29] ☐
[10,31] Preferential Treatment Given Under IA-CEPA
[11,29] ☐
[11,31] Preferential Treatment Not Given (Please state reason/s)
[13,3] 3. Port of discharge (if known):
[14,3] Banjuwangi, Java,Indonesia
[15,27] Signature of Authorised Signatory of the Importing Country
[16,3] 5. Item
number
[16,7] 6. Number and kind of packages; description of goods and brand name (if applicable)
[16,27] 7. HS Code (6 digit code) for each item
[16,33] 8. Origin Conferring Criterion. WO, PE, CTC, QVC or SP, and the adjusted value where QVC is used
[16,41] 9. Sufficient details to identify the consignment, such as importers PO number, invoice number and date, Way Bill or Bill of Lading
{packlines}
[34,3] 10. Third Party Invoice or Exhibition details
[37,3] 11. Declaration by the exporter
The undersigned hereby declares that the above details and statements are correct and that the goods exported to:
[37,27] 12. Certification

[39,27] On the basis of control carried out, it is hereby certified that
the information herein is correct and that the goods
described comply with the origin requirements specified in
the Agreement Establishing the Indonesia-Australia 
Comprehensive Economic Partnership Agreement (IA-CEPA)
[41,5]  United Kingdom
[42,5] (country)
[43,3] Comply with the origin requirements as provided in 
Chapter 4 of the Indonesia-Australia Comprehensive 

Economic Partnership Agreement (IA-CEPA)
[50,5] 01-Dec-2023
CargoWise Support   EDI CUSTOMS BROKERS
[53,3] Place and date, name, signature and
Company of authorised signatory
[53,27] Place and date, signature and stamp of Authorised 
Issuing Authority/Body
[54,3] 13
[54,5] ☐
[54,7] Subject to third-party invoice
[54,29] ☐
[54,31] Issued Retroactively
[55,5] ☐
[55,7] De Minimis
[55,29] ☐
[55,31] Exhibition"
			};

			if (numberOfPacklines > 3)
			{
				var additionalPageCount = ((numberOfPacklines - 3) / 10) + ((numberOfPacklines - 3) % 10 == 0 ? 0 : 1);
				for (var extraPageNumber = 0; extraPageNumber < additionalPageCount; extraPageNumber++)
				{
					var firstLineNumber = 57 + (extraPageNumber * 73);
					var continuationPacklines = CreatePacklineContent(
						startRow: 67 + (extraPageNumber * 73),
						length: Math.Min(10, (numberOfPacklines - 3) - (extraPageNumber * 10)),
						firstRowLineNumber: 4 + (extraPageNumber * 10),
						lineItemSource,
						packlineOriginCriterion);

					pages.Add($@"[{firstLineNumber},19] IA-CEPA CONTINUATION SHEET
[{firstLineNumber + 1},27] Certificate No.
[{firstLineNumber + 2},43] Form IA-CEPA
[{firstLineNumber + 4},3] 5. Item
number
[{firstLineNumber + 4},7] 6. Number and kind of packages; description of goods and brand name (if applicable)
[{firstLineNumber + 4},27] 7. HS Code (6 digit code) for each item
[{firstLineNumber + 4},33] 8. Origin Conferring Criterion. WO, PE, CTC, QVC or SP, and the adjusted value where QVC is used
[{firstLineNumber + 4},41] 9. Sufficient details to identify the consignment, such as importers PO number, invoice number and date, Way Bill or Bill of Lading
{continuationPacklines}
[{firstLineNumber + 50},3] 10. Third Party Invoice or Exhibition details
[{firstLineNumber + 53},3] 11. Declaration by the exporter
The undersigned hereby declares that the above details and statements are correct and that the goods exported to:
[{firstLineNumber + 53},27] 12. Certification
[{firstLineNumber + 55},27] On the basis of control carried out, it is hereby certified that
the information herein is correct and that the goods
described comply with the origin requirements specified in
the Agreement Establishing the Indonesia-Australia 
Comprehensive Economic Partnership Agreement (IA-CEPA)
[{firstLineNumber + 57},5]  United Kingdom
[{firstLineNumber + 58},5] (country)
[{firstLineNumber + 59},3] Comply with the origin requirements as provided in 
Chapter 4 of the Indonesia-Australia Comprehensive 
Economic Partnership Agreement (IA-CEPA)
[{firstLineNumber + 66},5] 01-Dec-2023
CargoWise Support   EDI CUSTOMS BROKERS
[{firstLineNumber + 69},7] Place and date, name, signature and
Company of authorised signatory
[{firstLineNumber + 69},29] Place and date, signature and stamp of Authorised 
Issuing Authority/Body
[{firstLineNumber + 70},3] 13
[{firstLineNumber + 70},5] ☐
[{firstLineNumber + 70},7] Subject to third-party invoice
[{firstLineNumber + 70},29] ☐
[{firstLineNumber + 70},31] Issued Retroactively
[{firstLineNumber + 71},5] ☐
[{firstLineNumber + 71},7] De Minimis
[{firstLineNumber + 71},29] ☐
[{firstLineNumber + 71},31] Exhibition"
					);
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(
			int startRow,
			int length,
			int firstRowLineNumber,
			LineItemSource lineItemSource,
			string originCriterion = OriginCriterionListIACEPA.Codes.WO)
		{
			var packlineContent = string.Join(
				"\r\n",
				Enumerable
					.Range(0, Math.Max(1, length))
					.Select(x =>
					{
						var row = startRow + (x * 4);
						return @$"[{row},3] {x + firstRowLineNumber}
[{row},7] 3 Pallet DDD
[{row},27] 123456
[{row},33] {originCriterion}"
+ (lineItemSource is LineItemSource.PackLine ? string.Empty : $"\n[{row},41] 12 \n11-Nov-2021");
					}));

			return packlineContent;
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IDBJU";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "IDBJU";
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "IDBJU";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2024, 01, 01, 00, 00, 00);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "IDBJU";
		}

		protected override OrgHeader CreateConsignorAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Consignor");

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Indonesia, fullName: "Consignee");

		protected override void PopulateAdditionalAddresses(ForwardingShipment shipment)
		{
			var buyerDocAddress = Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Buyer");

			shipment.BuyerDocAddress.E2_OA_Address = buyerDocAddress.MainAddress.PK;
		}

		#endregion
	}
}
