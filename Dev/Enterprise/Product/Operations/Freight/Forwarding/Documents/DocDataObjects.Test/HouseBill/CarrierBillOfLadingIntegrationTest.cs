using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class CarrierBillOfLadingIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Carrier Bill of Lading";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,17] BILL OF LADING
[3,5] Shipper
[3,24] Bill/Lading Number
[4,24] S00001000
[6,17] ORIGINAL
[9,5] Consignee (if 'To Order' so indicate)
[11,17] Received by the Carrier, the Goods as specified below in apparent good order and condition unless otherwise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting this Bill of Lading, any local privileges and customs notwithstanding. 
The particulars given below as stated by the shipper and the weight, measure, quantity, condition, contents and value of the Goods are unknown to the Carrier. In WITNESS whereof one (1) original Bill of Lading has been signed if not otherwise stated below, the same being accomplished the other(s), if any to be void, if required by the Carrier. One (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order. 
[12,5] Notify Party (No claim shall attach for failure to notify)
[16,5] Vessel
[16,19] Port of Loading
[16,26] Excess Value Declaration:
[17,5]  /
[17,19] , 
[17,26] Refer to Clause 11(4) + (5) on reverse side
[18,26] 0
[19,5] Port of Discharge
[19,10] Destination (if on-carriage)
[19,19] Freight Payable at:
[19,26] No. of Originals
[20,5] , 
[20,10] , 
[20,19] SYDNEY, AUSTRALIA
[20,26] 2 (TWO)
[21,5] Marks and Numbers
[21,9] Number and Kind of Packages / Description of Goods
[21,26] Gross Weight
Kgs.
[21,29] Measurement
M3
[22,10] 0 (s)
[22,24] 0.000 KG
[22,29] 0.000 M3
[25,5] CAN: 
[26,5] INCOTERM:
[26,12] Consol Ref: 
[27,12] SHIPPED ON BOARD  
[29,5] Bill of Lading must be surrendered to:
[29,21] Freight Details, Charges, etc.
[32,5] Place and Date of Issue
[33,5] BRISBANE, AUSTRALIA
[34,5] AS CARRIER
[36,5] Place of Receipt
[36,12] Place of Delivery
[36,21] Total No. of Packages (in words)
[37,5] SYDNEY, AUSTRALIA
[37,21] ZERO (S)";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("d23fd083-3b81-4539-9c26-837c67cbcaa2");
			var templatePK = new ZGuid("bf695095-380c-4241-bd71-084b68120e75");

			AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
		}
	}
}
