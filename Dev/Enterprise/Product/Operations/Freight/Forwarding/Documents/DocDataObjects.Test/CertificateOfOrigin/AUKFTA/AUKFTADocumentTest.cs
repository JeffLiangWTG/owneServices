using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.CodeLists;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AUKFTADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.UnitedKingdom;

		protected override ZGuid TemplatePivotPK => new ZGuid("1892D27A-A8C9-4585-97AE-86741AB5635D");

		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2024, 12, 04, 11, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent(Constants.CountryCodes.UnitedKingdom);

		[TestDate(2024, 12, 04, 11, 02, 03)]
		public void TestDocumentContent_TwoPages() => AssertDocumentContent(Constants.CountryCodes.UnitedKingdom, numberOfLineItems: 10);

		[TestDate(2024, 12, 04, 11, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent(Constants.CountryCodes.UnitedKingdom, numberOfLineItems: 100);

		[TestDate(2024, 12, 04, 11, 02, 03)]
		void AssertDocumentContent(string country, int numberOfLineItems = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfLineItems);

				AssertContents(shipment, ShipmentDocumentNames.AUKFTACertificateOfOrigin, CreateContent(numberOfLineItems));
			}
		}

		#region Implement

		string CreateContent(int numberOfPacklines = 1,
			string packlineOriginCriterion = OriginCriterionListAUKFTA.Codes.WO,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var pagesCount = numberOfPacklines <= 6 ? 1 : 1 + (int)Math.Ceiling((numberOfPacklines - 6) / 13.0);

			var packlines = CreatePacklineContent(startRow: 21, length: Math.Min(numberOfPacklines, 6), packlineOriginCriterion, lineItemSource);

			var pages = new List<string>
			{
				@$"
[2,3] 1. Exporter’s name, address, country, telephone and email
[2,27] Certificate No.
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[3,27] Form AUKFTA
[5,27] AUSTRALIA – UNITED KINGDOM FREE TRADE AGREEMENT
[7,31] DECLARATION OF ORIGIN
[8,3] 2. If different from Exporter, Producer’s name, address, country, telephone, and email
[9,3] MANUFACTURER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[9,31] Issued in
[10,37] Country
[11,33] Page 1 of  {pagesCount}
[12,27] 4. Means of transport and route (if known)
[13,3] 3. Importer’s/Consignee’s name, address, country, telephone and email (if known)

[13,27] Shipment Date:
[13,39] 11-Nov-2021
[14,3] CONSIGNEE
UNIT 100
11 WHY STREET
UNITED KINGDOM CITY
1011
UNITED KINGDOM
[14,27] Vessel’s name/Aircraft etc.:
[14,39] VesselData/KH6754
[15,27] Port of Loading:
[15,39] Sydney,Australia											
[16,27] Port of Destination:
[16,39] Southampton,United Kingdom
[18,3] 5. Marks and
numbers on
packages
[18,9] 6. Number and kind of packages; Quantity, Description of goods including HS Code (6 digits) for each good
[18,29] 7. Origin Criterion (WO, PE, PSR) for each good
[18,35] 8. Weight (Gross weight or Net)
[18,43] 9. Invoice number(s) and date(s)
{packlines}
[33,3] 10. Declaration
[33,27] 11. Certification
[34,3] I (the exporter/the producer or the authorised representative of the exporter/the producer) declare that the goods described in this document qualify as originating and the information contained in this document is true and accurate. I (the exporter/the producer) assume responsibility for proving such representations and agree to maintain and present upon request or to make available during a verification visit, documentation necessary to support this declaration of origin.
[34,27] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[44,3] This declaration has been completed by:
[45,3] ☐ Exporter
[45,9] ☐ Authorised representative on behalf of exporter
[46,3] ☐ Producer
[46,9] ☐ Authorised representative on behalf of producer
[53,3] Signature, Name, Company Name, Address, telephone, email 
of Signatory and Date
[53,27] Date, Name, Signature and Stamp
of Authorised Officer"
			};

			if (numberOfPacklines > 6)
			{
				for (var extraPageNumber = 0; extraPageNumber < pagesCount - 1; extraPageNumber++)
				{
					var firstLineNumber = 55 + (extraPageNumber * 50);
					var continuationPacklines = CreatePacklineContent(
						startRow: 61 + (extraPageNumber * 50),
						length: Math.Min(13, (numberOfPacklines - 6) - (extraPageNumber * 13)),
						packlineOriginCriterion,
						lineItemSource);

					pages.Add($@"
[{firstLineNumber},27] Certificate No.
[{firstLineNumber + 1},5] A-UKFTA Continuation page
[{firstLineNumber + 1},45] Form AUKFTA
[{firstLineNumber + 3},3] 5. Marks and
numbers on
packages
[{firstLineNumber + 3},9] 6. Number and kind of packages; Quantity, Description of goods including HS Code (6 digits) for each good
[{firstLineNumber + 3},29] 7. Origin Criterion (WO, PE, PSR) for each good
[{firstLineNumber + 3},35] 8. Weight (Gross weight or Net)
[{firstLineNumber + 3},43] 9. Invoice number(s) and date(s)
{continuationPacklines}
[{firstLineNumber + 32},3] 10. Declaration
[{firstLineNumber + 32},27] 11. Certification
[{firstLineNumber + 33},3] I (the exporter/the producer or the authorised representative of the exporter/the producer) declare that the goods described in this document qualify as originating and the information contained in this document is true and accurate. I (the exporter/the producer) assume responsibility for proving such representations and agree to maintain and present upon request or to make available during a verification visit, documentation necessary to support this declaration of origin.
[{firstLineNumber + 33},27] I, the undersigned, being duly authorised by WiseTech Global to sign documentary evidence of origin of goods, hereby certify that, to the best of my knowledge, this document has been signed by an officer of the company, or their representative, who has been authorised to sign documentary evidence of origin of goods on behalf of the company.
[{firstLineNumber + 40},3] This declaration has been completed by:
[{firstLineNumber + 41},3] ☐ Exporter
[{firstLineNumber + 41},9] ☐ Authorised representative on behalf of exporter
[{firstLineNumber + 42},3] ☐ Producer
[{firstLineNumber + 42},9] ☐ Authorised representative on behalf of producer
[{firstLineNumber + 48},3] Signature, Name, Company Name, Address, telephone, email 
of Signatory and Date
[{firstLineNumber + 48},27] Date, Name, Signature and Stamp
of Authorised Officer");
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(int startRow, int length, string originCriterion, LineItemSource lineItemSource)
		{
			var packlineContent = string.Join(
				"\r\n",
				Enumerable
					.Range(0, Math.Max(1, length))
					.Select(x =>
					{
						var row = startRow + (x * 2);
						return @$"{(lineItemSource is LineItemSource.PackLine ? $"[{row},3] Marks 1" : string.Empty)}
[{row},9] 3 Pallet DDD 123456
[{row},29] {originCriterion}
[{row},35] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : $"[{row},43] 12 \n11-Nov-2021")}";
					}));

			return packlineContent;
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBSOU";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "GBSOU";
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBSOU";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "GBSOU";
		}

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.UnitedKingdom, fullName: "Consignee");

		#endregion

	}
}
