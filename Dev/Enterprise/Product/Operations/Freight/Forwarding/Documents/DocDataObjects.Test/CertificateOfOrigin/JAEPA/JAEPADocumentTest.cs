using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CertificateOfOrigin.Testing
{
	sealed class JAEPADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => Constants.CountryCodes.Australia;
		protected override ZGuid TemplatePivotPK => new ZGuid("9c376136-eee3-42d0-bd90-526828152a37");

		protected override string CreateContent() => string.Empty;

		protected override string CreateContentForShipmentUsingInvoiceLines() => CreateContent(lineItemSource: LineItemSource.Invoice);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent(CountryCode);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_TwoPage() => AssertDocumentContent(CountryCode, numberOfLineItems: 10);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_100LineItems() => AssertDocumentContent(CountryCode, numberOfLineItems: 100);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		void AssertDocumentContent(string country, int numberOfLineItems = 1)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				// Arrange
				var expectedDocumentContents = CreateContent(numberOfLineItems);

				// Act
				var shipment = CreateShipment();
				CreatePackingLineItems(shipment, numberOfLineItems);

				// Assert
				AssertContents(shipment, ShipmentDocumentNames.JAEPACertificateOfOrigin, expectedDocumentContents);
			}
		}

		#region Implement

		string CreateContent(int numberOfLineItems = 1, string packlineOriginCriterion = OriginCriterionListJAEPA.Codes.WO, LineItemSource lineItemSource = LineItemSource.PackLine)
		{
			int pageIndex = 0;
			var lineItems = CreateLineItems(numberOfLineItems, pageIndex, startRow: 30, lineItemSource);
			numberOfLineItems -= 6;
			var expectedContent = @$"[2,4] 1. Goods Consigned from (Exporter name, address and country)
[2,30] Certificate No.
[2,50] Form JAEPA
[3,4] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[4,32] JAPAN - AUSTRALIA ECONOMIC PARTNERSHIP AGREEMENT (JAEPA)
[7,34] CERTIFICATE OF ORIGIN
[8,4] 2. Goods Consigned to (Importer’s/ Consignee’s name,
address, country)
[8,34] (Combined Declaration and Certificate)
[9,4] CONSIGNEE
UNIT 100
11 WHY STREET
BEIJING
999
CHINA
[10,33] Issued in 
[10,38] Australia
[11,38] Country
[13,4] 3. Means of transport and route (if known)
[13,30] 4. For Official Use
[15,4] Shipment Date:
[15,18] 01-Jan-2020
[15,32] ☐
[15,34] Preferential treatment Given Under JAEPA
[17,4] Vessel’s name/Aircraft etc.:
[17,18] VesselData/KH6754
[17,32] ☐
[17,34] Preferential treatment Not Given (Please state reason/s)
[19,4] Port of Loading:
[19,18] Sydney,Australia
[21,4] Port of Destination:
[23,30] Signature of Authorised Signatory of the Importing Country
[24,4] 5. Item
number
[24,8] 6. Marks and
numbers on
packages
[24,14] 7. Number and kind of packages; description of goods including HS Code(6 digits) and brand name(if applicable)
[24,30] 8. Origin Conferring Criterion (see Overleaf Notes)
[24,38] 9. Quantity (Gross weight or other measurement), and value(FOB) (see Overleaf Notes)
[24,48] 10. Invoice number and date
{lineItems}
[42,4] 11. Declaration by the exporter
[42,30] 12. Certification
[43,4] The undersigned hereby declares that the above details
and statements are correct; that all the goods were
produced in
[43,30] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Japan – Australia Economic Partnership Agreement
[47,6]  Australia
[48,6] (country)
[50,4] and that they comply with the rules, as provided in Chapter 3 of the Japan – Australia Economic Partnership Agreement (JAEPA) for the goods exported to
[55,6] (importing country)
[57,6] 01-Dec-2023
CargoWise Support
[61,30] Place and date, name and signature and stamp of Authorised Officer and Issuing Authority/ Body
[62,4] Place and date, name, signature and
company of authorised representative or exporter
[63,4] 13
[63,6] ☐
[63,8] Subject of third-party invoice
[63,24] ☐
[63,26] Issued retroactively
[63,44] ☐
[63,46] De Minimis
[65,6] ☐
[65,8] Accumulation";

			int continuationPageStartRow = 67;
			pageIndex += 1;

			while(numberOfLineItems > 0)
			{
				string continuationPageLineItems = CreateLineItems(numberOfLineItems, pageIndex, startRow: continuationPageStartRow + 7, lineItemSource);
				numberOfLineItems -= 13;

				string newContinuationPage = $@"[{continuationPageStartRow},30] Certificate No.:
[{continuationPageStartRow + 1},4] Continuation Page
[{continuationPageStartRow + 1},48] Form JAEPA
[{continuationPageStartRow + 4},4] 5. Item
number
[{continuationPageStartRow + 4},8] 6. Marks and
numbers on
packages
[{continuationPageStartRow + 4},14] 7. Number and kind of packages; description of goods including HS Code(6 digits) and brand name(if applicable)
[{continuationPageStartRow + 4},30] 8. Origin Conferring Criterion (see Overleaf Notes)
[{continuationPageStartRow + 4},37] 9. Quantity (Gross weight or other measurement), and value(FOB) (see Overleaf Notes)
[{continuationPageStartRow + 4},48] 10. Invoice number and date
{continuationPageLineItems}
[{continuationPageStartRow + 33},4] 11. Declaration by the exporter
[{continuationPageStartRow + 33},30] 12. Certification
[{continuationPageStartRow + 34},4] The undersigned hereby declares that the above details
and statements are correct; that all the goods were
produced in
[{continuationPageStartRow + 34},30] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Japan – Australia Economic Partnership Agreement
[{continuationPageStartRow + 38},6]  Australia
[{continuationPageStartRow + 39},6] (country)
[{continuationPageStartRow + 41},4] and that they comply with the rules, as provided in Chapter 3 of the Japan – Australia Economic Partnership Agreement (JAEPA) for the goods exported to
[{continuationPageStartRow + 46},6] (importing country)
[{continuationPageStartRow + 48},6] 01-Dec-2023
CargoWise Support
[{continuationPageStartRow + 52},30] Place and date, name and signature and stamp of Authorised Officer and Issuing Authority/ Body
[{continuationPageStartRow + 53},4] Place and date, name, signature and
company of authorised representative or exporter
[{continuationPageStartRow + 54},4] 13
[{continuationPageStartRow + 54},6] ☐
[{continuationPageStartRow + 54},8] Subject of third-party invoice
[{continuationPageStartRow + 54},24] ☐
[{continuationPageStartRow + 54},26] Issued retroactively
[{continuationPageStartRow + 54},44] ☐
[{continuationPageStartRow + 54},46] De Minimis
[{continuationPageStartRow + 56},6] ☐
[{continuationPageStartRow + 56},8] Accumulation";

				expectedContent = expectedContent + "\r\n" + newContinuationPage;

				continuationPageStartRow += 58;
				pageIndex += 1;
			}

			return expectedContent;
		}

		string CreateLineItems(int numberOfLineItems, int pageIndex, int startRow, LineItemSource lineItemSource)
		{
			var firstLineItemIndex = 1 + (pageIndex > 0 ? (6 + (pageIndex > 1 ? 13 * (pageIndex - 1) : 0) ) : 0);
			List<string> lineItemsList = new List<string>();
			for(int i = 0; i < Math.Min(numberOfLineItems, pageIndex == 0 ? 6 : 13); i++)
			{
				var row = startRow + (i * 2);
				string lineItem = $@"[{row},4] {firstLineItemIndex + i}
{(lineItemSource is LineItemSource.PackLine ? $"[{row},8] Marks 1" : string.Empty)}
[{row},14] 3 Pallet DDD 123456
[{row},30] WO
[{row},38] 15.00 KG
{(lineItemSource is LineItemSource.PackLine ? string.Empty : $"[{row},48] 12 \n11-Nov-2021")}";

				lineItemsList.Add(lineItem);
			}

			string lineItems = string.Join("\r\n", lineItemsList);
			return lineItems;
		}

		protected override void PopulateShipment(ForwardingShipment shipment)
		{
			shipment.JS_RL_NKDestination = "JPSZX";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDischargePort = "JPSZX";
			shipment.JS_E_DEP = ZDateTime.Empty;
		}

		protected override void PopulateConsol(ForwardingConsol consol)
		{
			consol.JK_RL_NKDischargePort = "JPSZX";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 01, 01, 00, 00, 00);
			transport.JW_RL_NKDiscPort = "JPSZX";
		}

		protected override OrgHeader CreateConsigneeAddress(ForwardingShipment shipment)
		{
			var consigneeDocumentaryAddress = Factory.New<OrgHeader>();
			consigneeDocumentaryAddress.OH_FullName = "Consignee";
			consigneeDocumentaryAddress.OH_RL_NKClosestPort = "JPSZX";
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
