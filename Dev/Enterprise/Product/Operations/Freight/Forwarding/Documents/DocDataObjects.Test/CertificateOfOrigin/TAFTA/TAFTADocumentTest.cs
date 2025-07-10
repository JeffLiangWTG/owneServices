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
	sealed class TAFTADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";
		protected override ZGuid TemplatePivotPK => new ZGuid("1eba1ed5-09a0-44d3-a81a-044bb4fbb0b8");
		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2024, 09, 20, 11, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU", numberofLineItems: 1);

		[TestDate(2024, 09, 20, 11, 02, 03)]
		public void TestDocumentContent_TwoPages() => AssertDocumentContent("AU", numberofLineItems: 9);

		[TestDate(2024, 09, 20, 11, 02, 03)]
		public void TestDocumentContent_ThreePages() => AssertDocumentContent("AU", numberofLineItems: 25);

		[TestDate(2024, 09, 20, 11, 02, 03)]
		public void TestDocumentContent_100Packlines() => AssertDocumentContent("AU", numberofLineItems: 100);

		[TestDate(2024, 09, 20, 11, 02, 03)]
		void AssertDocumentContent(string country, int numberofLineItems = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberofLineItems);

				AssertContents(shipment, ShipmentDocumentNames.TAFTACertificateOfOrigin, CreateContent(numberofLineItems));
			}
		}

		#region Implement

		string CreateContent(int numberOfPacklines = 1,
			string packlineOriginCriterion = OriginCriterionListTAFTA.Codes.WO,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var packlines = CreatePacklineContent(
				startRow: 23,
				length: Math.Min(numberOfPacklines, 8),
				lineItemSource,
				packlineOriginCriterion);

			var pages = new List<string>
			{
				@$"[1,3] 1. EXPORTER (name and address)
[1,26] Certificate No: 
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[3,26] 3. BUYER (if not consignee)
[4,26] BUYER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[6,3] 2. CONSIGNEE (name and address)
[7,3] CONSIGNEE
UNIT 100
11 WHY STREET
THAILAND CITY
4242
THAILAND
[11,2] CERTIFICATE OF ORIGIN
FOR THAILAND
[16,3] 4. EXPORTERS INVOICE NO. AND DATE
[16,19] 5. SHIP/AIRLINE
[16,36] 6. SEA/AIR PORT OF LOADING
[17,19] KH6754
[17,36] Sydney,Australia
[18,19] 7. SEA/AIR PORT OF DISCHARGE
[18,36] 8. EST DEPARTURE DATE
[19,19] Phichit,Thailand
[19,36]  11-Nov-2021
[20,3] DETAILED DESCRIPTION OF REGISTERED GOODS SUBJECT OF THIS CERTIFICATE OF ORIGIN
[21,3] 9. QUANTITY
[21,9] 10. HS CODE
[21,15] 11. ORIGIN CRITERION
[21,21] 12. DESCRIPTION OF GOODS (including Brand Name and Product Number, if applicable
{packlines}
[47,3] 13. Declaration
[47,26] 14. Certification
[48,3] I, the authorised signatory of the Registered Exporter herein named, certify and declare that the goods described herein originate in
[48,26] In accordance with the above declaration of the Registered Exporter, and based on the evidence provided to support that declaration as required by the Government of the Commonwealth of Australia for the purposes of satisfying the rules of origin for Thailand, the undersigned being a competent representative of the Authorised Body CERTIFIES that the goods described herein originate in AUSTRALIA and comply with the rules of origin as provided in Chapter 4 of the Thailand-Australia Free Trade Agreement (2004).
[51,9] AUSTRALIA
[53,3] and comply with the rules of origin, as provided in Chapter 4 of the Thailand-Australia Free Trade Agreement and there has been no material change to the basis of registration of those goods.
[56,3] Authorised Exporter Signatory 
[56,26] Signature, Name & Designation of competent representative and Date"
			};

			if (numberOfPacklines > 8)
			{
				var additionalPageCount = ((numberOfPacklines - 8) / 19) + ((numberOfPacklines - 8) % 19 == 0 ? 0 : 1);
				for (var extraPageNumber = 0; extraPageNumber < additionalPageCount; extraPageNumber++)
				{
					var firstLineNumber = 59 + (extraPageNumber * 73);
					var continuationPacklines = CreatePacklineContent(
						startRow: 63 + (extraPageNumber * 73),
						length: Math.Min(19, (numberOfPacklines - 8) - (extraPageNumber * 19)),
						lineItemSource,
						packlineOriginCriterion);

					pages.Add($@"[{firstLineNumber},3] TAFTA Continuation Page
[{firstLineNumber},26] Certificate No: 
[{firstLineNumber + 2},3] 9. QUANTITY
[{firstLineNumber + 2},9] 10. HS CODE
[{firstLineNumber + 2},15] 11. ORIGIN
CRITERION
[{firstLineNumber + 2},21] 12. DESCRIPTION OF GOODS (including Brand Name and Product Number, if applicable
{continuationPacklines}
[{firstLineNumber + 61},3] 13. Declaration
[{firstLineNumber + 61},26] 14. Certification
[{firstLineNumber + 62},3] I, the authorised signatory of the Registered Exporter herein named, certify and declare that the goods described herein originate in
[{firstLineNumber + 62},26] In accordance with the above declaration of the Registered Exporter, and based on the evidence provided to support that declaration as required by the Government of the Commonwealth of Australia for the purposes of satisfying the rules of origin for Thailand, the undersigned being a competent representative of the Authorised Body CERTIFIES that the goods described herein originate in AUSTRALIA and comply with the rules of origin as provided in Chapter 4 of the Thailand-Australia Free Trade Agreement (2004).
[{firstLineNumber + 65},9] AUSTRALIA
[{firstLineNumber + 67},3] and comply with the rules of origin, as provided in Chapter 4 of the Thailand-Australia Free Trade Agreement and there has been no material change to the basis of registration of those goods.
[{firstLineNumber + 70},3] Authorised Exporter Signatory 
[{firstLineNumber + 70},26] Signature, Name & Designation of competent representative and Date");
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(int startRow, int length, LineItemSource lineItemSource, string originCriterion = OriginCriterionListTAFTA.Codes.WO)
		{
			var packlineContent = string.Join(
				"\r\n",
				Enumerable
					.Range(0, Math.Max(1, length))
					.Select(x =>
					{
						var row = startRow + (x * 3);
						return @$"[{row},3] 3 PLT
[{row},9] 123456
[{row},21]  DDD
[{(row + 1)},15] {originCriterion}";
					}));

			return packlineContent;
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "THPHI";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "THPHI";
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "THPHI";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2024, 01, 01, 00, 00, 00);
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "THPHI";
		}

		protected override OrgHeader CreateConsignorAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Consignor");

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment) => Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Thailand, fullName: "Consignee");

		protected override void PopulateAdditionalAddresses(ForwardingShipment shipment)
		{
			var buyerDocAddress = Factory.New<OrgHeader>().PopulateForCountry(Constants.CountryCodes.Australia, fullName: "Buyer");

			shipment.BuyerDocAddress.E2_OA_Address = buyerDocAddress.MainAddress.PK;
		}

		#endregion
	}
}
