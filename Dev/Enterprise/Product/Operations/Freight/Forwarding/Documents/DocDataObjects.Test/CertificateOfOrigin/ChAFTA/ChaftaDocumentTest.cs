using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class ChaftaDocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "AU";
		protected override ZGuid TemplatePivotPK => new ZGuid("A67F8DAD-B7EE-4647-893E-3DF90BE19673");

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(LineItemSource.Invoice);

		protected override string CreateContent() => CreateContent();

		string CreateContent(LineItemSource lineItemSource = LineItemSource.PackLine) => $@"[1,2] CERTIFICATE OF ORIGIN
[2,3] 1. Exporter's name, address and country:
[2,27] Certificate Number:
[3,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[6,27] CERTIFICATE OF ORIGIN
[7,3] 2. Producer's name and address (if known):
[8,3] MANUFACTURER
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[8,27] Form for China-Australia Free Trade Agreement
[9,27] Issued in: AUSTRALIA
[12,3] 3. Importer's name and address (if known):
[12,27] For official use only:
[13,3] CONSIGNEE
UNIT 100
11 WHY STREET
BEIJING
999
CHINA
[17,3] 4. Means of transport and route (if known)
[17,27] 5. Remarks:
[18,3] Departure Date:
[18,14]  01-Jan-20 00:00:00
[19,3] Vessel/Flight/Train/Vehicle No.:
[19,14] VesselData/KH6754
[20,3] Port of loading
[20,14] Sydney,Australia
[21,3] Port of discharge
[21,14]   Shenzhen Baoan International Apt,China
[22,3] 6. Item Number (max. 20)
[22,7] 7. Marks and numbers on packages (optional)
[22,13] 8. Number and kind of packages; description of goods
[22,27] 9. HS Code (6-digit code)
[22,31] 10. Origin Criterion
[22,37] 11. Gross or net weight or other quantity (e.g. Quantity Unit, litres, m3)
[22,43] 12. Invoice number and date
[25,3] 1
{(lineItemSource is LineItemSource.PackLine ? "[25,7] Marks 1" : string.Empty)}
[25,13] 3 Pallet DDD
[25,27] 123456
[25,31] WO
[25,37] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : "[25,43] 12 \n11-Nov-2021						\n						")}
[39,3] 13. Declaration by the exporter or producer
[39,27] 14. Cerification
[40,3] The undersigned hereby declares that the above-stated information is correct and that the goods exported to
[40,27] On the basis of control carried out, it is hereby certified that the information herein is correct and that the described goods comply with the origin requirements of the China-Australia Free Trade Agreement.
[42,3] China
[43,9] (Importing Party)
[44,3] comply with the origin requirements in the China-Australia Free Trade Agreement.
[47,3] -CargoWise Support-
01-Dec-23 01:02:03
[48,27] Place, date, and signature and stamp of the Authorised Body
[49,5] Place, date and signature of authorised person
[50,3] Overleaf Instruction
[52,3] Box 1:
[52,7] State the full legal name and address of the exporter in Australia or China.
[53,3] Box 2:
[53,7] State the full legal name and address (including country) of the producer, if known. If more than one producer’s good is included in the certificate, list the additional producers, including names and addresses (including country). If the exporter or the producer wish the information to be confidential, it is acceptable to state “Available to the competent authority or authorised body upon request”. If the producer and the exporter are the same, please complete the box with “SAME”. If the producer is unknown, it is acceptable to state “UNKNOWN”.
[54,3] Box 3:
[54,7] State the full legal name and address of the importer in Australia or China, if known. 
[55,3] Box 4:
[55,7] Complete the means of transport and route and specify the departure date, transport vehicle number, and port of loading and discharge, if known.
[56,3] Box 5:
[56,7] The Customer’s Order Number, Letter of Credit Number, among others, may be included. If the invoice is issued by a non-Party operator, information such as the name, address and country of the operator issuing the invoice shall be indicated herein.
[57,3] Box 6:
[57,7] State the item number; item number shall not exceed 20.
[58,3] Box 7:
[58,7] State the shipping marks and numbers on packages, when such marks and numbers exist. 
[59,3] Box 8:
[59,7] The number and kind of packages shall be specified. Provide a full description of each good. The description should be sufficiently detailed to enable the products to be identified by the Customs Officers examining them and relate it to the invoice description and to the HS description of the good. If the goods are not packed, state “in bulk”. When the description of the goods is finished, add “***” (three stars) or “ \ ” (finishing slash).
[60,3] Box 9:
[60,7] For each good described in Box 8, identify the HS tariff classification (a six-digit code). 
[61,3] Box 10:
[61,7] For each good described in Box 8, state which criterion is applicable, according to the following instructions. The rules of origin are contained in Chapter 3 (Rules of Origin and Implementation Procedures) and Annex II (Product Specific Rules of Origin) of the China-Australia Free Trade Agreement
[62,7] Origin Criterion
[62,35] Insert in Box 10
[63,7] The good is “wholly obtained” in the territory of a Party in accordance with Article 3.3 (Wholly Obtained Goods).
[63,35] WO
[64,7] The good is produced entirely in the territory of one or both Parties, exclusively from materials whose origin conforms to the provisions of Chapter 3 (Rules of Origin and Implementation Procedures).
[64,35] WP
[65,7] The good is produced in the territory of one or both Parties, using non-originating materials that comply with the applicable product specific rule; and meets the other applicable provisions of Chapter 3 (Rules of Origin and Implementation Procedures).
[65,35] PSR
[67,3] Box 11:
[67,7] State gross or net weight in kilograms or other units of measurement for each good described in Box 8. Other units of measurement (e.g. volume or number of items) which would indicate exact quantities may be used where customary.
[68,3] Box 12:
[68,7] The invoice number and date should be shown here.
[69,3] Box 13:
[69,7] The box must be completed by the exporter or producer. Insert the place, date and the signature of a person authorised by the exporter or producer. 
[70,3] Box 14:
[70,7] The box must be completed, signed, dated and stamped by the authorised person of the authorised body. The telephone number, fax and address of the authorised body should be given.";

#region Implement

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKDestination = "CNSZX";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "CNSZX";
			shipment.JS_E_DEP = ZDateTime.Empty;
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKDischargePort = "CNSZX";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 01, 01, 00, 00, 00);
			transport.JW_RL_NKDiscPort = "CNSZX";
		}

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment)
		{
			var consigneeDocumentaryAddress = Factory.New<OrgHeader>();
			consigneeDocumentaryAddress.OH_FullName = "Consignee";
			consigneeDocumentaryAddress.OH_RL_NKClosestPort = "CNSZX";
			consigneeDocumentaryAddress.MainAddress.Address1 = "Unit 100";
			consigneeDocumentaryAddress.MainAddress.Address2 = "11 Why Street";
			consigneeDocumentaryAddress.MainAddress.City = "Beijing";
			consigneeDocumentaryAddress.MainAddress.Postcode = "999";
			consigneeDocumentaryAddress.MainAddress.OA_RN_NKCountryCode = "CN";
			return consigneeDocumentaryAddress;
		}

		#endregion
	}
}
