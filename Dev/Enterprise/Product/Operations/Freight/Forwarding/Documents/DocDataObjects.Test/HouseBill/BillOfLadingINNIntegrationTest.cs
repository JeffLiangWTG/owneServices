using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingINNIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading INN";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaNoTermsNoLaw;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,16] BILL OF LADING
[3,4] Consignor
[3,23] Bill/Lading Number
[4,23] S00001000
[6,16] ORIGINAL
[9,4] Consignee (if 'To Order' so indicate)
[11,16] Received by the Carrier, the Goods as specified below in apparent good order and condition unless otherwise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting this Bill of Lading, any local privileges and customs notwithstanding. 
The particulars given below as stated by the shipper and the weight, measure, quantity, condition, contents and value of the Goods are unknown to the Carrier. In WITNESS whereof one (1) original Bill of Lading has been signed if not otherwise stated below, the same being accomplished the other(s), if any to be void, if required by the Carrier. One (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order. 
[12,4] Notify Party (No claim shall attach for failure to notify)
[16,4] Vessel
[16,18] Port of Loading
[16,25] Excess Value Declaration:
[17,4]  /
[17,25] Refer to Clause 11(4) + (5) on reverse side
[18,25] 0
[19,4] Port of Discharge
[19,9] Destination (if on-carriage)
[19,18] Freight Payable at:
[19,25] No. of Originals
[20,18] SYDNEY, AUSTRALIA
[20,25] 2 (TWO)
[21,4] Marks and Numbers
[21,8] Number and Kind of Packages / Description of Goods
[21,25] Gross Weight
Kgs.
[21,28] Measurement
M3
[22,9] 0 (s)
[22,23] 0.000 KG
[22,28] 0.000 M3
[26,11] Consol Ref: 
[27,4] CAN: 
[28,4] INCOTERM:
[28,11] SHIPPED ON BOARD  
[30,4] Bill of Lading must be surrendered to:
[30,20] Freight Details, Charges, etc.
[33,4] Place and Date of Issue
[34,4] BRISBANE, AUSTRALIA
[35,4] AS CARRIER
[37,4] Place of Receipt
[37,11] Place of Delivery
[38,4] SYDNEY, AUSTRALIA
[39,20] Total No. of Packages (in words)
[40,20] ZERO (S)
[43,20] Note:
[44,20] The Merchant's attention is called to the fact that according to Clauses 10, 11 and 12 of this Bill of Lading, the liability of the Carrier is, in most cases, limited in the respect of loss of or damage to the goods and delay.
[45,4] Hbol-ITC";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("200ebc5d-d64b-4190-8a03-f7f9e72de064");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
