using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class COONZDocumentTests : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.NewZealand;

		protected override ZGuid TemplatePivotPK => new ZGuid("74E8CF0C-E30C-440C-AC03-A992A78D896B");

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(LineItemSource.Invoice);

		protected override string CreateContent() => CreateContent();

		string CreateContent(LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			return $@"[1,3] 1. Consignor/Exporter
[1,28] Certificate Number: 
[2,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[3,28] Certificate Of Origin
[5,28] 3. Country Of Origin
[6,3] 2. Consignee
[7,3] CONSIGNEE
UNIT 100
11 WHY STREET
SINGAPORE CITY 999
REP. OF SINGAPORE
[10,34] WiseTech Global
PO Box 76029
Manukau Auckland 2241
Email: certification@wisetechglobal.com
[11,3] 4. Transport Details (as far as known)
[13,3] Departure Date:
[13,14] 11-Nov-21 00:00:00
[14,3] Vessel/Flight/Train/Vehicle No.:
[14,14] VesselData/KH6754
[14,28] 5. Remarks
[15,3] Port of loading
[15,14] Auckland,New Zealand
[16,3] Port of discharge
[16,14] Shenzhen Baoan International Apt,China
[19,3] 6. Marks and numbers
[19,9] 7. Number and Kind of packages; description of goods
[19,30] 8. HS Code
[19,38] 9. Country of Origin
[19,41] 10. Gross weight
{(lineItemSource is LineItemSource.PackLine ? "[22,3] Marks 1" : string.Empty)}
[22,9] 3 Pallet DDD
[22,30] 123456
[22,42] 15.00 KG
[38,3] 11. Exporter/Applicant Declaration
[38,28] 12. Certification
[39,3] I, the undersigned, being duly authorised by the above exporter, and having made the necessary enquiries, hereby certify that all the goods listed above originate in the country stated above. I further declare that I will furnish to the Customs authorities of the importing country or their nominee, for inspection at any time such evidence as may be requested for the purpose of verifying this certificate.
[39,28] The undersigned, duly authorised by WiseTech Global (NZ), certifies to the best of my knowledge and belief on the basis of information supplied to it by the consignor, the goods described in this Certificate of Origin are of the origin as stated above.
[45,18] 01-Dec-2023
[47,3] Signature of Authorised Officer
[47,18] Date
[47,28] Signature of Authorised Official
[47,43] Date";
		}
	}
}
