using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingTTCIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading TTC";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZ;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,15] BILL OF LADING
[3,4] Consignor
[3,22] BILL OF LADING  NUMBER
[4,22] S00001000
[6,15] ORIGINAL
[9,4] Consignee (if 'To Order' so indicate)
[11,15] Received by the Carrier, the Goods as specified below in apparent good order and condition unless otherwise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting this Bill of Lading, any local privileges and customs notwithstanding. 
The particulars given below as stated by the shipper and the weight, measure, quantity, condition, contents and value of the Goods are unknown to the Carrier. In WITNESS whereof one (1) original Bill of Lading has been signed if not otherwise stated below, the same being accomplished the other(s), if any to be void, if required by the Carrier. One (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order. 
[12,4] Notify Party (No claim shall attach for failure to notify)
[16,4] Vessel
[16,17] Port of Loading
[16,24] Excess Value Declaration:
[17,4]  /
[17,24] Refer to Clause 6(3)(B) + (C) on reverse side
[18,24] 0
[19,4] Port of Discharge
[19,9] Destination (if on-carriage)
[19,17] Freight Payable at:
[19,24] No. of Originals
[20,17] SYDNEY, AUSTRALIA
[20,24] 2 (TWO)
[21,4] Marks and Numbers
[21,8] Number and Kind of Packages / Description of Goods
[21,24] Gross Weight
[21,27] Measurement
[22,9] 0 (s)
[22,22] 0.000 KG
[22,27] 0.000 M3
[25,4] Consol Ref: 
[26,4] CAN: 
[27,4] INCOTERM: 
[27,12] Temp Control Instructions
[27,22] Country of Origin
[28,4] SHIPPED ON BOARD  
[28,24] AUSTRALIA
[30,19] Freight Details, Charges, etc.
[33,4] Place and Date of Issue
[34,4] BRISBANE, AUSTRALIA
[35,4] Signed on behalf of  - the carrier
[37,4] Place of Receipt
[37,11] Place of Delivery
[38,4] SYDNEY, AUSTRALIA
[39,4] LAW AND JURISDICTION CLAUSE
[40,4] If this Bill of Lading is issued in Australia, the contract evidenced by or contained herein shall be governed by 
[41,4] the law of the State or Territory in which it is issued and any claim or dispute arising hereunder or in connection
[42,4] herewith shall at the Carriers sole option be determined by the Courts of that State or Territory & no other Court. 
[43,4] In all other cases any such claim or dispute shall be determined at the Carriers sole option either in the place
[43,19] Total No. of Packages (in words)
[44,4] place where this Bill of Lading is issued (& subject to the laws of that place) or at the place where the Carrier has
[44,19] ZERO (S)
[45,4]  its principal place of business (and subject to the laws of that place).
[46,4] Hbol-TTC";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("6ba72208-f82c-4a34-a038-4b72c7a7f186");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
