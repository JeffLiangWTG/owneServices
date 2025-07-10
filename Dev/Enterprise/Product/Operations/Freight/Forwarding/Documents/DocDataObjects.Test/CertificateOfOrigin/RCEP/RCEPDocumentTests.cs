using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	sealed class RCEPDocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";
		protected override ZGuid TemplatePivotPK => new ZGuid("cf4fb963-45e3-4cd8-aa7b-c3cc0db5eb43");
		protected override string CreateContent() => string.Empty;
		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(
		addressState: new AddressState() { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = false },
		lineItemSource: LineItemSource.Invoice);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("AU", numberOfLineItems: 1);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentTwoPages() => AssertDocumentContent("AU", numberOfLineItems: 7);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentThreePages() => AssertDocumentContent("AU", numberOfLineItems: 23);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfLineItems = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfLineItems);
				var addressState = new AddressState() { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = false };

				AssertContents(shipment, ShipmentDocumentNames.RCEPCertificateOfOrigin, CreateContent(addressState, numberOfLineItems));

				addressState.IsSameAsExporter = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems);

				addressState.IsUnknown = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems);

				addressState.ExcludeFromPDF = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems);
			}
		}

		void Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(ForwardingShipment shipment, AddressState addressState, int numberOfPacklines)
		{
			RCEP PostProcessFunc(RCEP rcep)
			{
				rcep.ProducerAddressStateString = addressState.GetAddressStateString();
				return rcep;
			}

			using (DocDataObjectPostProcessor<RCEP>.InjectInToObjectFactory(PostProcessFunc))
			{
				AssertContents(
					shipment,
					ShipmentDocumentNames.RCEPCertificateOfOrigin,
					CreateContent(addressState, numberOfPacklines));
			}
		}

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestPackLineFOBValuesForOriginCriterionRVC()
		{
			var shipment = CreateShipment();
			CreatePackingLineItems(shipment, 1);

			RCEP PostProcessFunc(RCEP rcep)
			{
				var firstPackline = rcep.LineItems.First();
				firstPackline.OriginCriterion.Code = OriginCriterionListRCEP.Codes.RVC;
				firstPackline.FOB.Currency.Code = Constants.CurrencyCodes.UnitedKingdom;
				firstPackline.FOB.Amount = 42.42;
				return rcep;
			}

			using (DocDataObjectPostProcessor<RCEP>.InjectInToObjectFactory(PostProcessFunc))
			{
				AssertContents(
					shipment,
					ShipmentDocumentNames.RCEPCertificateOfOrigin,
					CreateContent(
						new AddressState() { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = false },
						1,
						OriginCriterionListRCEP.Codes.RVC,
						$"{Constants.CurrencyCodes.UnitedKingdom} 42.42"),
					null);
			}
		}

		#region Implement

		string CreateContent(
			AddressState addressState,
			int numberOfPacklines = 1,
			string packlineOriginCriterion = OriginCriterionListRCEP.Codes.WO,
			string packlineFob = null,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var manufacturer = CreateManufacturerAddress(addressState, "UNIT52\r\nDORCUS YAMADAI\r\nSYDNEY NSW 2017\r\nAUSTRALIA");

			var packlines1 = CreatePacklineContent(
				startRow: 24,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfPacklines, 6),
				packlineOriginCriterion,
				packlineFob,
				lineItemSource);

			var pages = new List<string>();
			pages.Add(@$"[1,3] 1. Goods Consigned from (Exporter’s name, address and country)
[1,29] Certificate Number: 
[1,49] Form RCEP
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[2,29] REGIONAL COMPREHENSIVE ECONOMIC PARTNERSHIP AGREEMENT
[6,3] 2. Goods Consigned to (Importer’s/ Consignee’s name, address, country)
[7,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE
[7,35] CERTIFICATE OF ORIGIN
[9,31] Issued in: 
[10,37] (Country)
[11,3] 3. Producer’s name, address and country (if known)
[12,3] {manufacturer}
[14,29] 5. For Official Use 
[15,31] Preferential Treatment:
[16,3] 4. Means of transport and route (if known)
[16,29] ☐
[16,31] Given
[16,39] ☐
[16,41] Not Given (Please state reason/s) 
[17,3] Departure Date:
[17,17]  11-Nov-2021
[18,3] Vessel’s name/Aircraft flight number, etc.:
[18,17] VesselData/KH6754
[19,3] Port of Discharge
[19,17] Singapore,Singapore
[19,29] Signature of Authorised 
[19,41] Signatory of the Customs Authority of the Importing Country
[21,3] 6. Item number
[21,7] 7. Marks and numbers on packages
[21,13] 8. Number and kind of packages; and description of goods.  
[21,24] 9. HS Code of the goods (6 digit-level)
[21,29] 10. Origin Conferring Criterion 
[21,37] 11. RCEP Country of Origin 
[21,42] 12. Quantity (Gross weight or other measurement), and value (FOB) where RVC is applied
[21,49] 13. Invoice number(s) 
and date of invoice(s)
{packlines1}
[36,3] 14. Remarks 
[39,3] 15. Declaration by the exporter or producer
[39,29] 16. Certification
[40,3] The undersigned hereby declares that the above details and statements are correct and that the goods covered in this Certificate comply with the requirements specified for these goods in the Regional Comprehensive Economic Partnership Agreement. These goods are exported to:
[40,29] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Regional Comprehensive Economic Partnership Agreement.
[44,3] Singapore
[45,9] (Importing country)
[46,3]  - 
01-Dec-2023
[47,5] Place and date, and signature of authorised signatory
[47,29] Place, date, and signature and stamp of the Authorised Body
[48,3] 17.
[48,5] ☐
[48,7] Back-to-back Certificate of Origin
[48,25] ☐
[48,27] Third-party invoicing
[48,41] ☐
[48,43] ISSUED RETROACTIVELY
[49,3] OVERLEAF NOTES
[51,2] 1.
[51,4] CONDITIONS: To be eligible for the preferential tariff treatment under the Regional Comprehensive Economic Partnership Agreement (the Agreement), goods should:
[52,5] a.
[52,7] fall within a description of goods eligible for concessions in the importing Party; and
[53,5] b.
[53,7] comply with all relevant provisions of Chapter 3 (Rules of Origin) and if applicable, Article 2.6 (Tariff Differentials) of Chapter 2 of the Agreement.
[55,2] 2.
[55,4] EXPORTER AND CONSIGNEE/IMPORTER: Provide details of the exporter of the goods (including name, address and country) and consignee/importer (including name, address, 
and country) in Box 1 and Box 2, respectively.
[57,2] 3.
[57,4] PRODUCER: Provide the details of the producer of the goods (including name, address and country) in Box 3, if known. In case of multiple producers, indicate “SEE BOX 8” in Box 
3 and provide the details in Box 8 for each item. If the producer wishes the information to be confidential, it is acceptable to state “CONFIDENTIAL”, however, the producer may be information available to the competent authority or authorised body upon request. In case the details of the producer are unknown, it is acceptable to state “NOT AVAILABLE”.
[59,2] 4.
[59,4] DESCRIPTION OF GOODS: The description of each good in Box 8 should be sufficiently detailed to enable the products to be identified by the customs officer examining them.
[61,2] 5.
[61,4] HARMONIZED COMMODITY DESCRIPTION AND CODING SYSTEM (HS): The HS should be at the 6-digit level of the exported product and based on Annex 3A of the Agreement.
[63,2] 6.
[63,4] ORIGIN CONFERRING CRITERIA: For the goods that meet the origin conferring criteria, the exporter should indicate in Box 10 of this Form, the origin conferring criteria met, in the
manner shown in the following table:
[65,4] Origin conferring criteria
[65,41] Insert in Box 10
[66,5] (a)
[66,7] Goods wholly obtained or produced satisfying Article 3.2(a) of Chapter 3 of the Agreement
[66,41] WO
[67,5] (b)
[67,7] Goods produced exclusively from originating materials satisfying Article 3.2(b) of Chapter 3 of the Agreement
[67,41] PE
[69,5] (c)
[69,7] Goods produced using non-originating materials provided that the goods satisfy the product specific 
requirements set out in Annex 3A of the Agreement:
[70,7] -
[70,9] Change in Tariff Classification
[70,41] CTC
[71,7] -
[71,9] Regional Value Content
[71,41] RVC
[72,7] -
[72,9] Chemical Reaction
[72,41] CR
[73,5] (d)
[73,7] Goods comply with Article 3.4 of Chapter 3 of the Agreement
[73,41] ACU
[74,5] (e)
[74,7] Goods comply with Article 3.7 of Chapter 3 of the Agreement
[74,41] DMI
[76,2] 7.
[76,4] EACH GOOD CLAIMING PREFERENTIAL TARIFF TREATMENT QUALIFIES IN ITS OWN RIGHT: It should be noted that all the goods in a consignment qualifies separately in
their own right.
[78,2] 8.
[78,4] RCEP COUNTRY OF ORIGIN: The RCEP country of origin should be indicated separately for each good in the manner shown in the following table:
[80,4] Circumstances
[80,37] Insert in Box 11 – RCEP country of origin
[82,5] (a)
[82,7] Goods are in Appendix to Annex I of the importing Party but do not meet the additional requirement 
specified in the Appendix to Annex I i.e. a Domestic Value Addition of 20% (DV20).
[82,37] Indicate the name of the Party that contributed the 
highest value of originating materials used in the 
production of that good in the exporting Party in 
accordance with Article 2.6.4.
[83,5] (b)
[83,7] Goods that are not in the Appendix to Annex I of the importing Party, are produced exclusively from 
originating materials in accordance with Article 3.2(b) of Chapter 3 of the Agreement but are not 
processed beyond minimal operations set out in Article 2.6.5 of Chapter 2 of the Agreement in the 
exporting Party.
[85,5] IN ALL OTHER CIRCUMSTANCES, including
[85,37] Indicate the name of the exporting Party
[86,5] (c)
[86,7] Goods are in Appendix to Annex I of the importing Party and meet the additional requirement specified 
in Appendix to Annex I i.e. a Domestic Value Addition of 20% (DV20).
[87,5] (d)
[87,7] Goods are wholly obtained or produced in accordance with Article 3.2(a) of Chapter 3 of the Agreement
[88,5] (e)
[88,7] Goods that are not in the Appendix to Annex I of the Importing Party and satisfy the applicable 
requirements set out in Annex 3A (Product-Specific Rules) in accordance with Article 3.2(c) of Chapter 
3 of the Agreement.
[89,5] (f)
[89,7] Goods that are not in the Appendix to Annex I of the importing Party, are produced exclusively from 
originating materials in accordance with Article 3.2(b) and are processed beyond minimal operations set 
out in Article 2.6.5 of Chapter 2 of the Agreement in the exporting Party.
[91,4] Notes: Notwithstanding the above, under paragraph 6 of Article 2.6 of Chapter 2 of the Agreement the importer is allowed to make a claim for preferential tariff treatment at either:
[92,5] -
[92,7] the highest rate of customs duty the importing Party applies to the same originating good from any of the Parties contributing originating materials used in the production
of such good, (Article 2.6.6(a)), or
[93,5] -
[93,7] the highest rate of customs duty that the importing Party applies to the same originating good from any of the Parties (Article 2.6.6(b)).
[95,4] When the RCEP country of origin cannot be ascertained, based on the information provided by the exporter/producer and importer, indicate the name of the Party with the highest 
rate of customs duty followed by “ * ” if the Article 2.6.6(a) of Chapter 2 of the Agreement is being used or “ ** ” if the Article 2.6.6(b) of Chapter 2 of the Agreement is being used. 
For example: Australia * or Indonesia **.
[97,2] 9.
[97,4] FOB VALUE: The FOB value in Box 12 only needs to be provided when the Regional Value Content criterion is applied in determining the originating status of goods.
[99,2] 10.
[99,4] INVOICES: Indicate the invoice number and date in Box 13. If multiple invoices are used, indicate the invoice number and date for each item. The invoice is the one issued for the 
importation of the good into the importing Party. In cases where invoices used for the importation are not issued by the exporter or producer, in accordance with Article 3.20 of 
Chapter 3 of the Agreement, the “Third-party invoicing” box in Box 17 should be ticked (✓), and the name and country of the company issuing the invoice should be provided in Box 
14.
[101,2] 11.
[101,4] BACK-TO-BACK CERTIFICATE OF ORIGIN: In the case of a back-to back Certificate of Origin issued in accordance with Article 3.19 of Chapter 3 of the Agreement, the “Backto-back Certificate of Origin” box in Box 17 should be ticked (✓), and the original Proof of Origin reference number, date of issuance, issuing country, RCEP country of origin of the
first exporting Party, and, if applicable, approved exporter authorisation code of the first exporting Party should be indicated in Box 14.
[103,2] 12.
[103,4] ISSUED RETROACTIVELY: Where a Certificate of Origin is issued retrospectively in accordance with paragraph 8 of Article 3.17 of Chapter 3 of the Agreement, the “ISSUED
RETROACTIVELY” box in Box 17 should be ticked (✓).
[105,2] 13.
[105,4] CERTIFIED TRUE COPY: Where a certified true copy of the original Certificate of Origin is issued in accordance with paragraph 9 of Article 3.17 of Chapter 3 of the Agreement, the
words “CERTIFIED TRUE COPY” and the date of issuance of the certified true copy should be indicated in Box 14.
[107,2] 14.
[107,4] FOR OFFICIAL USE: The customs authority of the importing Party may indicate (✓) in the relevant box in Box 5 in accordance with their domestic laws and regulations.
[109,2] 15.
[109,4] REMARKS: Box 14 should only be filled out when necessary and contain information including as specified in Paragraphs 10, 11, and 13 of the Overleaf Notes.");

			if (numberOfPacklines > 6)
			{
				var additionalPageCount = (numberOfPacklines / 16) + (numberOfPacklines % 16 == 0 ? 0 : 1);
				for (var extraPageNumber = 0; extraPageNumber < additionalPageCount; extraPageNumber++)
				{
					var firstLineNumber = 110 + (extraPageNumber * 51);
					var continuationPacklines = CreatePacklineContent(
						startRow: 117 + (extraPageNumber * 51),
						firstRowLineNumber: 7 + (extraPageNumber * 16),
						length: Math.Min(16, (numberOfPacklines - 6) - (extraPageNumber * 16)),
						packlineOriginCriterion,
						packlineFob,
						lineItemSource);

					pages.Add($@"[{firstLineNumber},2] Continuation Sheet
[{firstLineNumber + 2},11] Certificate No.:
[{firstLineNumber + 2},39] Form RCEP
[{firstLineNumber + 4},3] 6. Item number
[{firstLineNumber + 4},7] 7. Marks and numbers on packages
[{firstLineNumber + 4},13] 8. Number and kind of packages; and description of goods.  
[{firstLineNumber + 4},24] 9. HS Code of the goods (6 digit-level)
[{firstLineNumber + 4},29] 10. Origin Conferring Criterion 
[{firstLineNumber + 4},37] 11. RCEP Country of Origin 
[{firstLineNumber + 4},42] 12. Quantity (Gross weight or other measurement), and value (FOB) where RVC is applied
[{firstLineNumber + 4},49] 13. Invoice number(s) and date of invoice(s)
{continuationPacklines}
[{firstLineNumber + 39},3] 14. Remarks 
[{firstLineNumber + 42},3] 15. Declaration by the exporter or producer
[{firstLineNumber + 42},29] 16. Certification
[{firstLineNumber + 43},3] The undersigned hereby declares that the above details and statements are correct and that the goods covered in this Certificate comply with the requirements specified for these goods in the Regional Comprehensive Economic Partnership Agreement. These goods are exported to:
[{firstLineNumber + 43},29] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Regional Comprehensive Economic Partnership Agreement.
[{firstLineNumber + 47},3] Singapore
[{firstLineNumber + 48},9] (Importing country)
[{firstLineNumber + 49},3]  - 
01-Dec-2023
[{firstLineNumber + 50},5] Place and date, and signature of authorised signatory
[{firstLineNumber + 50},29] Place, date, and signature and stamp of the Authorised Body");
				}
			}

			return string.Join("\r\n", pages);
		}

		string CreatePacklineContent(
			int startRow,
			int firstRowLineNumber,
			int length,
			string originCriterion = OriginCriterionListRCEP.Codes.WO,
			string fob = null,
			LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			var packlineContent = string.Join(
				"\r\n",
				Enumerable
					.Range(0, Math.Max(1, length))
					.Select(x =>
					{
						var row = startRow + (x * 2);
						return @$"[{row},3] {x + firstRowLineNumber}
{(lineItemSource is LineItemSource.PackLine ? $"[{row},7] Marks 1" : string.Empty)}
[{row},13] 3 Pallet DDD
[{row},25] 123456
[{row},28] {originCriterion}
[{row},37] New Zealand
[{row},42] 15.00 KG"
							+ (fob != null ? $"\r\n[{row + 1},42] {fob}" : string.Empty)
							+ (lineItemSource is LineItemSource.PackLine ? string.Empty : $"\n[{row},49] 12 \n11-Nov-2021");
					}));

			return packlineContent;
		}

		string CreateManufacturerAddress(AddressState addressState, string defaultAddress)
		{
			if (addressState.ExcludeFromPDF)
			{
				return "CONFIDENTIAL";
			}
			else if (addressState.IsSameAsExporter)
			{
				return "SAME";
			}
			else if (addressState.IsUnknown)
			{
				return "NOT AVAILABLE";
			}

			return $"MANUFACTURER\r\n{defaultAddress}";
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			using (shipment.RetainShipmentDatesDuringDestinationUpdate())
			{
				shipment.JS_RL_NKOrigin = Constants.CountryCodes.Australia.GetDefaultPort();
				shipment.JS_RL_NKDestination = Constants.CountryCodes.Japan.GetDefaultPort();
				shipment.JS_RL_NKLoadPort = Constants.CountryCodes.Australia.GetDefaultPort();
				shipment.JS_RL_NKDischargePort = Constants.CountryCodes.Singapore.GetDefaultPort();
			}
		}

		protected override void PopulatePackingLineItem(ForwardingPackLine lineItem) => lineItem.JL_RN_NKOrigin = Constants.CountryCodes.NewZealand;

		protected override void PopulateInvoiceLine(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_CountryOfOrigin = Constants.CountryCodes.NewZealand;

		protected override void PopulateConsol(ForwardingConsol consol) => consol.JK_RL_NKDischargePort = Constants.CountryCodes.Singapore.GetDefaultPort();

		#endregion
	}
}
