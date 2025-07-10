using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class AanzftaDocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.NewZealand;

		protected override ZGuid TemplatePivotPK => new ZGuid("F25E7B5B-8E57-4BB0-80B2-B431F6392F39");

		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent();

		void AssertDocumentContent()
		{
			var shipment = CreateShipment();
			CreatePackingLineItems(shipment);

			Assert_DocumentContent_When_ThirdPartyInvoiceDetails_InjectedInObjectFactory(shipment, isSubjectOfThirdPartyInvoice: true);
			Assert_DocumentContent_When_ThirdPartyInvoiceDetails_InjectedInObjectFactory(shipment, isSubjectOfThirdPartyInvoice: false);
		}

		void Assert_DocumentContent_When_ThirdPartyInvoiceDetails_InjectedInObjectFactory(ForwardingShipment shipment, bool isSubjectOfThirdPartyInvoice, string thirdPartyInvoiceIssuer = "ABC Company")
		{
			Aanzfta PostProcessFunc(Aanzfta aanzfta)
			{
				aanzfta.IsSubjectOfThirdPartyInvoice = isSubjectOfThirdPartyInvoice;
				aanzfta.ThirdPartyInvoiceIssuer = thirdPartyInvoiceIssuer;
				return aanzfta;
			}

			using (DocDataObjectPostProcessor<Aanzfta>.InjectInToObjectFactory(PostProcessFunc))
			{
				AssertContents(
					shipment,
					ShipmentDocumentNames.AANZFTACertificateOfOrigin,
					CreateContent(isSubjectToThirdPartyInvoice: isSubjectOfThirdPartyInvoice));
			}
		}

		#region Implement

		string CreateContent(bool isSubjectToThirdPartyInvoice = false, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			return $@"[1,39] Original
[2,3] 1. Goods Consigned from (Exporter’s name, address and country)
[2,27] Certificate No.
[2,43] Form AANZ
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[4,27] AGREEMENT ESTABLISHING THE ASEAN –
AUSTRALIA – NEW ZEALAND FREE TRADE
AREA (AANZFTA)
[7,3] 2. Goods Consigned to (Importer’s/ Consignee’s name,
address, country)
[7,31] CERTIFICATE OF ORIGIN
[8,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE
[8,31] (Combined Declaration and Certificate)
[9,31] Issued in
[10,37] Country
[11,33] (see Overleaf Notes)
[12,3] 3. Means of transport and route (if known)
[12,27] 4. For Official Use
[13,3] Shipment Date:
[13,15]  11-Nov-2021
[13,27] ☐
[13,29] Preferential Treatment Given Under AANZFTA
[14,3] Vessel’s name/Aircraft etc.:
[14,15] VesselData/KH6754
[14,27] ☐
[14,29] Preferential Treatment Not Given (Please state reason/s)
[15,3] Port of Discharge:
[15,15] Shenzhen Baoan International Apt,China
[18,27] Signature of Authorised Signatory of the Importing Country
[19,3] 5. Item
number
[19,7] 6. Marks and
numbers on
packages
[19,13] 7. Number and kind of packages; description of goods including HS Code (6 digits) and brand name (if applicable)
[19,27] 8. Origin Conferring Criterion (see Overleaf Notes)
[19,32] 9. Quantity (Gross weight or other measurement), and value (FOB) (see Overleaf Notes)
[19,43] 10. Invoice number and date
[22,3] 1
{(lineItemSource is LineItemSource.PackLine ? "[22,7] Marks 1" : string.Empty)}
[22,13] 3 Pallet DDD 123456
[22,27] WO
[22,33] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : "[22,43] 12 \n11-Nov-2021")}
[34,2] 7a. Third Party Invoice details
{(isSubjectToThirdPartyInvoice ? "[35,2] ABC Company" : "")}
[36,3] 11. Declaration by the exporter
[36,27] 12. Certification
[37,3] The undersigned hereby declares that the above details
and statements are correct; that all the goods were
produced in
[37,27] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Agreement Establishing the ASEAN-Australia-New Zealand Free Trade Area.
[39,5]  Australia
[40,5] (country)
[41,3] and that they comply with the rules of origin, as provided in Chapter 3 of the Agreement Establishing the ASEAN-Australia-New Zealand Free Trade Area for the goods exported to
[48,5]  Singapore
[49,5] (importing country))
[52,3] Eagle Datamation International
, 01-Dec-2023
[54,3] Place and date, name, signature and
company of authorised signatory
[54,27]               Place and date, signature and stamp of Authorised
Issuing Authority/ Body
[56,3] 13
[56,5] ☐
[56,7] Back-to-back Certificate of Origin
[56,23] {(isSubjectToThirdPartyInvoice ? "☑" : "☐")}
[56,25] Subject of third-party invoice
[56,37] ☐
[56,39] Issued retroactively
[57,5] ☐
[57,7] De Minimis
[57,23] ☐
[57,25] Accumulation
[59,3] OVERLEAF NOTES
[61,3] 1.
[61,5] Countries which accept this form for the purpose of preferential treatment under the Agreement Establishing the ASEAN-AustraliaNew Zealand Free Trade Area (the Agreement):

[63,5] Australia
[63,11] Brunei Darussalam 
[63,21] Cambodia
[63,29] Indonesia
[63,37] Lao PDR 
[63,43] Malaysia
[64,5] Myanmar
[64,11] New Zealand
[64,21] Philippines
[64,29] Singapore
[64,37] Thailand
[64,43] Vietnam
[65,5] (herein after individually referred to as a Party)
[67,3] 2.
[67,5] CONDITIONS:
[67,11]  To be eligible for the preferential treatment under the AANZFTA, goods must:
[69,7] a.
[69,9]  Fall within a description of products eligible for concessions in the importing Party.
[70,7] b.
[70,9]  Comply with all relevant provisions of Chapter 3 (Rules of Origin) of the Agreement.
[72,3] 3.
[72,5] EXPORTER AND CONSIGNEE:
[72,17]   Details of the exporter of the goods (including name, address and country) and consignee
[73,5] (name and address) must be provided in Box 1 and Box 2, respectively.
[75,3] 4.
[75,5] DESCRIPTION OF GOODS:
[75,16] The description of each good in Box7 must include the Harmonized Commodity Description
[76,5] and Coding System (HS) subheading at the 6-digit level of the exported product, and if applicable, product name and brand name. This information should be sufficiently detailed to enable the products to be identified by the customs officer examining them.
[79,3] 5.
[79,5] ORIGIN CRITERIA:
[79,13] For the goods that meet the origin criteria, the exporter should indicate in Box8 of this Form, the origin
[80,5] criteria met, in the manner shown in the following table:
[82,5]   Circumstances of production or manufacture in the country named in Box11of this form:
[82,42] Insert in Box8
[83,5] (a)
[83,7]  Goods wholly produced or obtained satisfying Article 2.1(a) of Chapter 3 of the Agreement
[83,42] WO
[84,5] (b)
[84,7] Goods produced entirely satisfying Article 2.1(c) of Chapter 3 of the Agreement
[84,42] PE
[85,5] (c)
[85,7] Not wholly produced or obtained in a Party, provided that the goods satisfy Article 4 of Chapter 3 of the Agreement as amended by the First Protocol i.e., if the good is specified in Annex 2, all the product specific requirements listed have been met:
[86,42] CTC 
RVC 
“e.g. CTSH + RVC 35%” 
Other 
[87,7] -
[87,9] Change in Tariff Classification
[88,7] -
[88,9] Regional Value Content
[89,7] -
[89,9] Regional Value Content + Change in Tariff Classification
[90,7] -
[90,9] Other, including a Specific Manufacturing or Processing Operation
[93,3] 6.
[93,5] EACH GOOD CLAIMING PREFERENTIAL TARIFF TREATMENT MUST QUALIFY IN ITS OWN RIGHT: 
[93,44] It should be
[94,5] noted that all the goods in a consignment must qualify separately in their own right. This is of particular relevance when similar articles of different sizes or spare parts are exported.
[97,3] 7.
[97,5] FOB VALUE:
[97,11]  For Consignments to all Parties where the origin criteria includes a Regional Value Content requirement:
[99,5] •   
[99,6] An exporter from an ASEAN Member State must provide in Box 9 the FOB value of the goods
[100,5] •   
[100,6] An exporter from Australia or New Zealand can complete either Box 9 or provide a separate “Exporter Declaration” stating the FOB value of the goods.
[103,5] The FOB value is not required for consignments where the origin criteria does not include a Regional Value Content requirement. In the case of goods exported from and imported by Cambodia and Myanmar, the FOB value shall be included in the Certificate of Origin or the back-to-back Certificate of Origin for all goods, irrespective of the origin criteria used, for two (2) years from the date of entry into force of the First Protocol or an earlier date as endorsed by the Committee on Trade in Goods. 
[108,3] 8.
[108,5] INVOICES:
[108,10] Indicate the invoice number and date for each item. The invoice should be the one issued for the importation of
[109,5] the good into the importing Party.
[111,3] 9.
[111,5] SUBJECT OF THIRD PARTY INVOICE: 
[111,20]  In cases where invoices used for the importation are issued in a third country, in
[112,5] accordance with Rule 22 of the Operational Certification Procedures, the “SUBJECT OF THIRD-PARTY INVOICE” box in Box 13 should be ticked (✓) and the name of the company issuing the invoice should be provided in Box 7 or, if there is insufficient space, on a continuation sheet. The number of the invoices issued by the manufacturers or the exporters and the number of the invoices issued by the trader (if known) for the importation of goods into the importing Party should be indicated in Box 10.
[117,3] 10.
[117,5] BACK-TO-BACK CERTIFICATE OF ORIGIN:
[117,22] In the case of a back-to-back certificate of origin issued in accordance with
[118,5]  stamped on paragraph 3 of Rule 10 of the Operational Certification Procedures, the back-to-back certificate of origin in Box 13 should be ticked (✓).
[121,3] 11.
[121,5]  CERTIFIED TRUE COPY:
[121,15]    In case of a certified true copy, the words “CERTIFIED TRUE COPY” should be written or
[122,5] stamped on Box 12 of the Certificate with the date of issuance of the copy in accordance with Rule 11 of the Operational Certification Procedures.
[125,3] 12.
[125,5] FOR OFFICIAL USE:
[125,13]     The Customs Authority of the Importing Party must indicate (✓) in the relevant boxes in Box4 whether
[126,5] or not preferential tariff treatment is accorded.
[128,3] 13.
[128,5] BOX 13:
[128,9]   The items in Box 13 should be ticked (✓), as appropriate, in those cases where such items are relevant to the
[129,5] goods covered by the Certificate.";
		}

		#endregion
	}
}
