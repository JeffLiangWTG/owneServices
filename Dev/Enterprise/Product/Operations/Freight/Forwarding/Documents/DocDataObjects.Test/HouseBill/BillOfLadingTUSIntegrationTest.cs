using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	internal class BillOfLadingTUSIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "BillOfLadingTUS";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.TTClubUnitedStates;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_HBLAWBChargesDisplay = "ALL";

			var localClient = Factory.New<OrgHeader>();
			localClient.OH_FullName = "Ziggy Z";
			localClient.OH_RL_NKClosestPort = "AUSYD";
			localClient.MainAddress.Address1 = "Unit 13";
			localClient.MainAddress.Address2 = "4 Lost Lane";
			localClient.MainAddress.City = "Sydney";
			localClient.MainAddress.Postcode = "2000";
			localClient.MainAddress.OA_RN_NKCountryCode = "AU";

			var agentCollect = Factory.New<OrgHeader>();
			agentCollect.OH_FullName = "Airmarine Inc.";
			agentCollect.OH_RL_NKClosestPort = "USCHI";
			agentCollect.MainAddress.Address1 = "5638 S Central Ave";
			agentCollect.MainAddress.City = "Chicago";
			agentCollect.MainAddress.Postcode = "60638";
			agentCollect.MainAddress.OA_RN_NKCountryCode = "US";

			var loader = new JobHeader.Loader(shipment);
			var header = loader.TryLoadOrCreate();

			header.LocalChargesPK = localClient.PK;
			header.AgentCollectPK = agentCollect.PK;

			var exRates = (BusinessObjectCollection)header["ExchangeRates"];

			var usdRate = exRates.AddNew();
			usdRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "USD";
			usdRate[JobExRateSchema.Constants.JF_BaseRate] = 1.2;

			var auRate = exRates.AddNew();
			auRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = "AUD";
			auRate[JobExRateSchema.Constants.JF_BaseRate] = 1.1;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[1,8] Bill of Lading
[1,18] NOT NEGOTIABLE UNLESS CONSIGNED TO ORDER
[5,3]  CONSIGNOR / EXPORTER (2) /COMPLETE NAME AND ADDRESS
[5,18]  DOCUMENT NO (5)
[6,19] S00001000
[7,18]  EXPORT REFERENCES (6)
[11,3]  CONSIGNEE (3) /COMPLETE NAME AND ADDRESS
[11,18]  FORWARDING AGENT REFERENCES (7)
[13,18]  POINT AND COUNTRY OF ORIGIN (8)
[14,20] Australia
[15,3]  NOTIFY PARTY (4) /COMPLETE NAME AND ADDRESS
[15,18]  DOCUMENT PRESENTATION (9)
[18,3]  PLACE OF RECEIPT (12)
[19,4] SYDNEY, AUSTRALIA
[20,3]  VESSEL (13)
[20,12]  PORT OF LOADING (14)
[20,18]  INTERNAL REFERENCE (10)
[21,4]  /
[21,19] S00001000
[22,3]  PORT OF DISCHARGE (15)
[22,12]  PLACE OF DELIVERY (16)
[22,19] ORIGINAL
[23,13] CHICAGO, UNITED STATES
[25,3] PARTICULARS FURNISHED BY SHIPPER
[26,3] MARKS & NOS / CONTAINER(S) NOS.
[26,8] NO OF PKGS.
[26,12] (19)          DESCRIPTION OF PACKAGES AND GOODS
[26,23] GROSS WEIGHT
[26,25] MEASUREMENT
[27,3] (17)
[27,8] (18)
[27,23] (20)
[27,25] (21)
[28,8] 0 (s)
[28,23] 0.000 KG
[28,25] 0.000 M3
[32,3] TOTAL NUMBER OF PKGS.
[32,7] ZERO (S)
[32,14] CAN: 
[33,14] INCOTERM:FOB
[33,19] SHIPPED ON BOARD 
[35,3] DECLARED VALUE ($)
[35,14] SEE CLAUSE 20 ON REVERSE SIDE
[35,21] RECEIVED FOR SHIPMENT from the MERCHANT in apparent good order and condition unless otherwise stated herein, the GOODS mentioned above to be transported as provided herein, by any mode of transport for all or any part of the Carriage, SUBJECT TO ALL THE TERMS AND CONDITIONS appearing on the face and back hereof and in the CARRIER'S applicable Tariff, to which the Merchant agrees by accepting this BILL OF LADING. 

Where applicable law requires and not otherwise, one original BILL OF LADING must be surrendered, duty endorsed, in exchange for the GOODS or CONTAINER(S) or other PACKAGE(S), the others to stand void If a 'Non-Negotiable' BILL OF LADING is issued, neither an original nor a copy need be surrendered in exchange for delivery unless applicable law so requires. 

[36,3] CHARGES, INCLUDING FREIGHT
[37,8] RATE
[37,13] PREPAID
[37,16] COLLECT
[38,3] International Freight
[38,14] 1,000.00 AUD
[39,3] Origin Labour Charges
[39,17] 500.00 AUD
[40,3] Destination Labour Charges
[40,14] 750.00 USD
[46,21] BY
[46,22] Eagle Datamation International
[48,21] AS CARRIER
[51,4] HBoL-TTC";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var header = shipment.JobHeader;
			CreateLineCharge(header, header.LocalChargesPK, 1000m, "FRT", "AUD");
			CreateLineCharge(header, header.AgentCollectPK, 500m, "OLAB", "AUD");
			CreateLineCharge(header, header.LocalChargesPK, 750m, "DLAB", "USD");

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("0bc11249-0938-46ea-9725-982f6a49bb70");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
