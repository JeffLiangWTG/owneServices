using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin.Base;
using Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CertificateOfOrigin.NZ
{
	sealed class NZCFTADocumentTest : CertificateOfOriginDocumentTests
	{
		protected override string CountryCode => "NZ";
		protected override ZGuid TemplatePivotPK => new ZGuid("7fd14eae-7bbd-4e7a-960e-018e0a8f132c");
		protected override string CreateContent() => string.Empty;
		protected override string CreateContentForShipmentUsingInvoiceLines() => string.Empty;

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent() => AssertDocumentContent("NZ", numberOfLineItems: 1, newZealandToChina: true);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_TwoPagesOfPacklines_AsterisksMoveToNextPage() => AssertDocumentContent("NZ", numberOfLineItems: 7, newZealandToChina: true);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContent_TwoPagesOfPacklines() => AssertDocumentContent("NZ", numberOfLineItems: 8, newZealandToChina: true);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentFromChina_SinglePackline() => AssertDocumentContent("CN", numberOfLineItems: 1, newZealandToChina: false);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentFromChina_TwoPagesOfPacklines() => AssertDocumentContent("CN", numberOfLineItems: 8, newZealandToChina: false);

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public void TestDocumentContentFromChina_TwoPagesOfPacklines_AsterisksMoveToNextPage() => AssertDocumentContent("CN", numberOfLineItems: 7, newZealandToChina: false);

		[TestDate(2008, 09, 13, 12, 30, 00)]
		public void TestDocumentContent_TwoPagesOfPacklines_WithTwentyPacklines() => AssertDocumentContent("NZ", numberOfLineItems: 20, newZealandToChina: true);

		void AssertDocumentContent(string country, int numberOfLineItems = 1, bool newZealandToChina = true)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var shipment = CreateShipment(newZealandToChina);
				CreatePackingLineItems(shipment, numberOfLineItems);
				var addressState = new AddressState()
				{
					IsSameAsExporter = false,
					IsUnknown = false,
					ExcludeFromPDF = false
				};

				AssertContents(shipment, ShipmentDocumentNames.NZCFTACertificateOfOrigin, CreateContent(addressState, numberOfLineItems, newZealandToChina));

				addressState.IsSameAsExporter = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems, newZealandToChina);

				addressState.IsUnknown = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems, newZealandToChina);

				addressState.ExcludeFromPDF = true;
				Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(shipment, addressState, numberOfLineItems, newZealandToChina);
			}
		}

		void Assert_DocumentContent_When_AddressStateStringInjectedInObjectFactory(ForwardingShipment shipment, AddressState addressState, int numberOfPacklines, bool newZealandToChina)
		{
			NZCFTA PostProcessFunc(NZCFTA nzcfta)
			{
				nzcfta.ProducerAddressStateString = addressState.GetAddressStateString();
				return nzcfta;
			}

			using (DocDataObjectPostProcessor<NZCFTA>.InjectInToObjectFactory(PostProcessFunc))
			{
				AssertContents(
					shipment,
					ShipmentDocumentNames.NZCFTACertificateOfOrigin,
					CreateContent(addressState, numberOfPacklines, newZealandToChina));
			}
		}

		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContentWithInvoices()
		{
			TestDocumentContentWithInvoices(grossWeight: 10, netWeight: -1);
			TestDocumentContentWithInvoices(grossWeight: 10, netWeight: 0);

			TestDocumentContentWithInvoices(grossWeight: 10, netWeight: 18);

			TestDocumentContentWithInvoices(grossWeight: 0, netWeight: 18);
			TestDocumentContentWithInvoices(grossWeight: -1, netWeight: 18);

			TestDocumentContentWithInvoices(grossWeight: 0, netWeight: 0);
		}

		void TestDocumentContentWithInvoices(int grossWeight, int netWeight)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = CreateShipment<ForwardingShipmentForTest>();
				CreateInvoiceLineItems(shipment);
				SetLineItemWeights(shipment, grossWeight, netWeight);

				var defaultAddressState = new AddressState() { IsSameAsExporter = false, IsUnknown = false, ExcludeFromPDF = false };
				AssertContents(shipment, TemplatePivotPK, CreateContent(
					addressState: defaultAddressState,
					lineItemSource: LineItemSource.Invoice,
					grossWeight: grossWeight,
					netWeight: netWeight));
			}
		}

		void SetLineItemWeights(ForwardingShipment shipment, int grossWeight, int netWeight)
		{
			foreach (var declaration in shipment.Declarations.Cast<BaseJobDeclaration>())
			{
				foreach (var invoiceHeader in declaration.Invoices)
				{
					foreach (var invoiceLineBO in invoiceHeader.InvoiceLines)
					{
						var invoiceLine = (BaseJobComInvoiceLine)invoiceLineBO;
						invoiceLine.JI_Weight = grossWeight;
						invoiceLine.JI_NetWeight = netWeight;
					}
				}
			}
		}

		#region Implement

		string CreateContent(AddressState addressState,
			int numberOfPacklines = 1,
			bool newZealandToChina = true,
			LineItemSource lineItemSource = LineItemSource.PackLine,
			int grossWeight = 15,
			int netWeight = 0)
		{
			var nzAddress = "UNIT 52\r\n83 ULSTER ROAD\r\nAUCKLAND 1010\r\nNEW ZEALAND";
			var cnAddress = "UNIT 100\r\n11 WHY STREET\r\nBEIJING\r\n999\r\nCHINA";
			var consignor = newZealandToChina ? nzAddress : cnAddress;
			var consignee = newZealandToChina ? cnAddress : nzAddress;
			var manufacturer = CreateManufacturerAddress(addressState, newZealandToChina ? nzAddress : cnAddress);
			var cell20_14 = newZealandToChina ? "Auckland,New Zealand" : "Shenzhen Baoan International Apt,China";
			var cell21_14 = newZealandToChina ? "Shenzhen Baoan International Apt,China" : "Auckland,New Zealand";
			var cell42_3 = newZealandToChina ? string.Empty : "[42,3] China\r\n";
			var cell46_3 = newZealandToChina ? string.Empty : "[46,3] New Zealand\r\n";
			var cell48_3 = newZealandToChina ? string.Empty : "[48,3] 01-Dec-2023\r\n";
			var cell125_3 = newZealandToChina ? string.Empty : "[123,3] China\r\n";
			var cell129_3 = newZealandToChina ? string.Empty : "[127,3] New Zealand\r\n";
			var cell131_3 = newZealandToChina ? string.Empty : "[129,3] 01-Dec-2023\r\n";

			var packlines1 = CreatePacklineContent(
				startRow: 25,
				firstRowLineNumber: 1,
				length: Math.Min(numberOfPacklines, numberOfPacklines == 7 ? 6 : 7),
				appendAsterisks: numberOfPacklines < 7,
				isFirstPage: true,
				lineItemSource: lineItemSource,
				grossWeight: grossWeight,
				netWeight: netWeight);

			var pageCount = numberOfPacklines > 6 ? 3 : 2;

			var initialPages = @$"[1,2] CERTIFICATE OF ORIGIN
[3,3] ORIGINAL
[4,3] 1. Exporter's name, address and country:
[4,28] Certificate Number: 
[5,3] CONSIGNOR
{consignor}
[5,27] CERTIFICATE OF ORIGIN
[6,28] Form for the Free Trade Agreement between the Government of the People's Republic of China and the Government of New Zealand
[7,34] Issued in New Zealand
[8,34] (See Instruction Overleaf)
[9,3] 2. Producer's name and address, if known:
[9,28] 5. For official use only
{manufacturer}
[10,30]   Preferential Tariff Treatment Given Under
[11,30]   Preferential Treatment Not Given (Please state reasons)
[13,3] 3. Consignee’s name, address, country:
[13,28] Signature of Authorized Signatory of the Importing Country
[14,3] CONSIGNEE
{consignee}
[14,28] 6. Remarks:
[17,3] 4. Means of transport and route (as far as known)
[18,3] Departure Date:
[18,14] 01-Jan-2020
[19,3] Vessel/Flight/Train/Vehicle No.:
[19,14] VesselData/KH6754
[20,3] Port of loading
[20,14] {cell20_14}
[21,3] Port of discharge
[21,14] {cell21_14}
[22,3] 7. Item Number (max. 20)
[22,7] 8. Marks and numbers on packages
[22,13] 9. Number and Kind of packages; description of goods
[22,28] 10. HS Code (Six Digit Code)
[22,34] 11. Origin Criterion
[22,40] 12. Gross weight quantity (quantity unit) or other measures (litres, m3, etc)
[22,46] 13. Number, Date of invoice and invoiced value
{packlines1}
[39,3] 14. Declaration by the Exporter
[39,28] 15. Certification
[40,3] The undersigned hereby declares that the above details and statements are correct; that all the goods were produced in
[40,28] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Free Trade Agreement between the Government of the People’s Republic of China and the Government of New Zealand 
{cell42_3}[43,3] (Country)
[44,3] and that they comply with the origin requirements specified in the FTA for the goods exported to
{cell46_3}[47,3] (Importing Country)
{cell48_3}[50,3] Place and date, signature of authorized signatory
[50,30] Place and date, signature and stamp of Authorized body
[51,2] PAGE:  1  OF  {pageCount}
[53,5] Overleaf Instruction
[55,5] Box 1:
[55,9] State the full legal name, address (including country) of the exporter
[56,5] Box 2:
[56,9] State the full legal name, address (including country) of the producer. If more than one producer’s good is included in the certificate, list the additional producers, including name, address (including country). If the exporter or the producer wishes the information to be confidential, it is acceptable to state “Available to the authorized body upon request”. If the producer and the exporter are the same, please complete field with “SAME”. If the producer is unknown, it is acceptable to state ”UNKNOWN”.
[57,5] Box 3:
[57,9] State the full legal name, address (including country) of the consignee.
[58,5] Box 4:
[58,9] Complete the means of transport and route and specify the departure date, transport vehicle No., port of loading and discharge.
[59,5] Box 5:
[59,9] The customs administration of the importing country must indicate (✓) in the relevant boxes whether or not preferential tariff treatment is accorded.
[60,5] Box 6:
[60,9] Any additional information such as Customer’s Order Number, Letter of Credit Number, etc. may be included.
[61,5] Box 7:
[61,9] State the item number, and item number should not exceed 20.
[62,5] Box 8:
[62,9] State the shipping marks and numbers on the packages.
[63,5] Box 9:
[63,9] Number and kind of package shall be specified. Provide a full description of each good. The description should be
sufficiently detailed to enable the products to be identified by the Customs Officers examining them and relate it to the invoice description and to the HS description of the good. If goods are not packed, state “in bulk”. When the description of the goods is finished, add “***” (three stars) or “ \ ” (finishing slash).
[64,5] Box 10:
[64,9] For each good described in Box 9, identify the HS tariff classification to six digits.
[65,5] Box 11:
[65,9] If the goods qualify under the Rules of Origin, the exporter must indicate in Box 11 of this form the origin criteria on the basis of which he claims that his goods qualify for preferential tariff treatment, in the manner shown in the following table:
[66,9] The origin criteria on the basis of which the exporter claims that his goods qualify for preferential tariff treatment 
[66,34] Insert in Box 11
[67,9] The good is wholly obtained or produced in the territory of a Party as set out and defined in Article 20, including where required to be so under Annex 5
[67,34] WO
[68,9] The good is produced entirely in the territory of one or both Parties, exclusively from materials whose origin conforms to the provisions of Section 1 of Chapter 4. 
[68,34] WP
[69,9] The good is produced in the territory of one or both Parties, using 
non-originating materials that conform to a change in tariff 
classification, a regional value content, a process requirement or other requirements specified in Annex 5, and the good meets the other applicable provisions of Section  of Chapter 4
[69,34] PSR¹
[71,5] Box 12:
[71,9] Gross weight in kilograms should be shown here. Other units of measurement e.g. volume or number of items which would indicate exact quantities may be used when customary.
[72,5] Box 13:
[72,9] Invoice number, date of invoices and invoiced value should be shown here.
[73,5] Box 14:
[73,9] The field must be completed, signed and dated by the exporter for exports from China. It is not required for New Zealand exports to China. Insert the place, date of signature.
[74,5] Box 15:
[74,9] The field must be completed, signed, dated and stamped by the authorized person of the authorized body.
[81,5] ¹ When the good is subject to a regional value content (RVC) requirement stipulated in Annex 5, indicate the percentage
[83,5] PAGE:  2  OF  {pageCount}";

			if (numberOfPacklines > 6)
			{
				var packlines2 = CreatePacklineContent(
					startRow: 90,
					firstRowLineNumber: numberOfPacklines == 7 ? 7 : 8,
					length: numberOfPacklines - (numberOfPacklines == 7 ? 6 : 7),
					appendAsterisks: true,
					isFirstPage: false,
					lineItemSource,
					grossWeight,
					netWeight);

				return $@"{initialPages}
[85,2] Attachment
[86,28] Certificate No.:
[87,3] 7. Item Number (max. 20)
[87,7] 8. Marks and numbers on packages
[87,13] 9. Number and Kind of packages; description of goods
[87,28] 10. HS Code (Six Digit Code)
[87,34] 11. Origin Criterion
[87,40] 12. Gross weight quantity (quantity unit) or other measures (litres, m3, etc)
[87,46] 13. Number, Date of invoice and invoiced value
{packlines2}
[120,3] 14. Declaration by the Exporter
[120,28] 15. Certification
[121,3] The undersigned hereby declares that the above details and statements are correct; that all the goods were produced in
[121,28] On the basis of control carried out, it is hereby certified that the information herein is correct and that the goods described comply with the origin requirements specified in the Free Trade Agreement between the Government of the People’s Republic of China and the Government of New Zealand 
{cell125_3}[124,3] (Country)
[125,3] and that they comply with the origin requirements specified in the FTA for the goods exported to
{cell129_3}[128,3] (Importing Country)
{cell131_3}[131,3] Place and date, signature of authorized signatory
[131,30] Place and date, signature and stamp of Authorized body
[132,2] PAGE:  3  OF  {pageCount}";
			}
			else
			{
				return initialPages;
			}
		}

		string CreatePacklineContent(int startRow,
			int firstRowLineNumber,
			int length,
			bool appendAsterisks,
			bool isFirstPage,
			LineItemSource lineItemSource,
			decimal grossWeight,
			decimal netWeight)
		{
			var netWeightString = netWeight > 0
				? $" / {netWeight.ToString("0.00")} KG NET WEIGHT /"
				: string.Empty;

			var box12WeightString = grossWeight <= 0 && netWeight > 0
				? netWeight.ToString("0.00")
				: grossWeight.ToString("0.00");

			var packlineContent = string.Join(
			"\r\n",
			Enumerable
				.Range(0, Math.Max(1, length))
				.Select(x =>
				{
					var row = startRow + (x * 2);
					return @$"[{row},3] {x + firstRowLineNumber}
{(lineItemSource is LineItemSource.PackLine ? $"[{row},7] Marks 1" : string.Empty)}
[{row},13] 3 Pallet{netWeightString} DDD
[{row},28] 123456
[{row},34] WO
[{row},40] {box12WeightString}  KG
{(lineItemSource is LineItemSource.PackLine ? $"[{row},46]  \n\n0.00  " : $"[{row},46] 12 \n11-Nov-2021\n100.00 AUD ")}
[{row + 1},40] 3 NMP";
				}));

			if (appendAsterisks)
			{
				packlineContent += $"\r\n[{startRow + (length * 2)},13] ***";
			}

			return packlineContent;
		}

		string CreateManufacturerAddress(AddressState addressState, string defaultAddress)
		{
			if (addressState.ExcludeFromPDF)
			{
				return "[10,3] Available to the authorized body upon request";
			}
			else if (addressState.IsSameAsExporter)
			{
				return "[10,3] SAME";
			}
			else if (addressState.IsUnknown)
			{
				return "[10,3] UNKNOWN";
			}

			return $"[10,3] MANUFACTURER\r\n{defaultAddress}";
		}

		ForwardingShipment CreateShipment(bool newZealandToChina = true) =>
			CreateShipment<ForwardingShipment>(newZealandToChina);

		T CreateShipment<T>(bool newZealandToChina = true) where T : ForwardingShipment
		{
			var shipment = Factory.New<T>();

			if (newZealandToChina)
			{
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "CNSZX";
				shipment.JS_RL_NKLoadPort = "NZAKL";
				shipment.JS_RL_NKDischargePort = "CNSZX";
			}
			else
			{
				shipment.JS_RL_NKOrigin = "CNSZX";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_RL_NKLoadPort = "CNSZX";
				shipment.JS_RL_NKDischargePort = "NZAKL";
			}

			shipment.JS_UniqueConsignRef = "S00001527";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			CreateAddresses(shipment, newZealandToChina);
			var consol = CreateConsol(newZealandToChina);
			shipment.Consols.Add(consol);

			return shipment;
		}

		ForwardingConsol CreateConsol(bool newZealandToChina = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			if (newZealandToChina)
			{
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "CNSZX";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "CNSZX";
				consol.JK_RL_NKDischargePort = "NZAKL";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2020, 01, 01, 00, 00, 00);
			transport.JW_VoyageFlight = "KH6754";
			transport.JW_Vessel = "VesselData";

			if (newZealandToChina)
			{
				transport.JW_RL_NKLoadPort = "NZAKL";
				transport.JW_RL_NKDiscPort = "CNSZX";
			}
			else
			{
				transport.JW_RL_NKLoadPort = "CNSZX";
				transport.JW_RL_NKDiscPort = "NZAKL";
			}

			return consol;
		}

		void CreateAddresses(ForwardingShipment shipment, bool newZealandToChina = true)
		{
			var consignorDocumentaryAddress = Factory.New<OrgHeader>().PopulateForCountry(
				newZealandToChina ? Constants.CountryCodes.NewZealand : Constants.CountryCodes.China,
				fullName: "Consignor");
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorDocumentaryAddress.MainAddress.PK;

			var consigneeDocumentaryAddress = Factory.New<OrgHeader>().PopulateForCountry(
				newZealandToChina ? Constants.CountryCodes.China : Constants.CountryCodes.NewZealand,
				fullName: "Consignee");
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.MainAddress.PK;

			var manufacturerDocumentaryAddress = Factory.New<OrgHeader>().PopulateForCountry(
				newZealandToChina ? Constants.CountryCodes.NewZealand : Constants.CountryCodes.China,
				fullName: "Manufacturer");
			shipment.ManufacturerDocAddress.E2_OA_Address = manufacturerDocumentaryAddress.MainAddress.PK;
		}

		#endregion
	}
}
