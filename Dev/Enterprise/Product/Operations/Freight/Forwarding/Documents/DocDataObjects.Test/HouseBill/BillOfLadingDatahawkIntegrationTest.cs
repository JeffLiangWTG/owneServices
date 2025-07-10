using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingDatahawkIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading Datahawk";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.DataHawkBill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,3] CONSIGNOR
[2,17] BILL OF LADING
[2,29] B/L No.
[3,29] S00001000
[4,17] FOR COMBINED TRANSPORT SHIPMENT OR PORT TO PORT SHIPMENT
[5,17] ORIGINAL
[5,29] BOOKING REF. No.
[10,3] CONSIGNEE
[13,3] NOTIFY PARTY / ADDRESS
[14,17] CARRIER
[16,17] CARRIER'S AGENT
[18,17] Received by the Carrier from the Shipper in apparent good order and condition (unless otherwise noted herein) the total number or quantity of Containers or other package or units indicated * , stated by the Shipper to comprise the Goods specified below, for Carriage subject to all the terms hereof from the Place of Receipt or the Port of Loading, whichever is applicable, to the Place of Discharge or the Port of Delivery, whichever is applicable. 
In accepting this Bill of Lading the Merchant expressly accepts and agrees to all its terms, conditions and exceptions whether printed, stamped or written or otherwise incorporated notwithstanding the non-signing of this Bill of Lading by the Merchant.
[19,3] VESSEL AND VOYAGE No.
[20,3]  / 
[21,3] PORT OF LOADING
[21,8] RELEASE
[23,3] PORT OF DISCHARGE
[23,8] DESTINATION (if on-Carriage)
[23,17] *TOTAL No. OF PACKAGES
[24,17] ZERO (S)
[25,3] MARKS AND NUMBERS
[25,9] NUMBER AND KIND OF PACKAGES / DESCRIPTION OF GOODS
[25,25] GROSS WEIGHT
[25,30] MEASUREMENT
[26,9] 0 (s)
[26,25] 0.000 KG
[26,30] 0.000 M3
[32,3] SHIPPED ON BOARD 
[32,22] ABOVE PARTICULARS AS DECLARED BY SHIPPER
[33,3] Consol Ref: 
[33,19] TYPE OF SERVICE
[34,19] LCL
[35,3] FREIGHT PREPAID
[36,19] FREIGHT AND CHARGES
[37,3] CAN: 
[42,3] FREIGHT NOMINEE(S)
[44,3] DELIVERY AGENT
[46,19] IN WITNESS of the contract herein contained the number of originals stated opposite have been issued, one of which being accomplished, the other(s) to be void.
[46,29] No. OF ORIGINALS
[47,29] 2 (TWO)
[49,19] AS CARRIER
[50,3] **PLACE(S) OF ACCEPTANCE
[50,10] **PLACE OF DELIVERY
[51,3] SYDNEY, AUSTRALIA
[52,3] **Applicable only when this document is used as a Combined Transport
[52,19] AT
[52,20] BRISBANE, AUSTRALIA
[52,27] DATE
[55,3] HBoL-DHWK";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("ccc999a7-1ca4-4770-9dc1-dfb35cb19315");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
