using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingITCNZIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading ITC NZ";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.ITClubNewZealand;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,3] BILL OF LADING
[4,4] Consignor
[4,13] ORIGINAL
[4,20] Bill/Lading Number
[5,20] S00001000
[6,13] Consol Reference:
[6,17] Shipper's Reference
[12,4] Consignee (if 'To Order' so indicate) 
[19,4] Notify Party (No claim shall attach for failure to notify)
[19,13] Delivery Agent
[26,4] Place of Receipt
[26,9] Port of Loading
[26,13] Freight Payable at:
[27,4] SYDNEY, AUSTRALIA
[27,13] SYDNEY, AUSTRALIA
[28,4] Vessel
[28,9] Port of Discharge
[28,13] Place of Delivery
[28,19] No. of Originals
[29,4]  / 
[29,19] 2 (TWO)
[30,4] Marks and Numbers
[30,8] Number and Kind of packages/Description of Goods
[30,19] Gross Weight
[30,25] Measurement
[31,8] 0 (s)
[31,19] 0.000 KG
[31,25] 0.000 M3
[55,4] SHIPPED ON BOARD 
[56,4] Total number of packages: 0 (s) (Outer)
[56,12] RECEIVED by the Carrier, the Goods as specified above in apparent good order and condition unless otherwise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting the Bill of Lading, any local privileges and customs notwithstanding. 
The particulars given above as stated by the shipper and the weight, measure, quantity, condition, contents and value of the Goods are unknown to the Carrier. 
In WITNESS whereof one (1) original Bill of Lading has been signed if not otherwise stated above, the same being accomplished the other(s), if any to be void, if required by the Carrier. One (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order. 
[60,4] Excess Value Declaration: Refer to Clause 11(4) & (5) on reverse side
[62,4] Note:
[63,4] The Merchant's attention is called to the fact that according to Clauses 10, 11 and 12 of this Bill of Lading, the liability of the Carrier is, in most cases, limited in respect of loss of or damage to the goods and delay.
[65,13] Place and Date of Issue
[66,13] BRISBANE, AUSTRALIA
[67,4] LAW AND JURISDICTION CLAUSE
[68,13] Signed on behalf of the Carrier:
[69,4] The Contract evidence by or contained in this Bill of Lading shall be governed by New Zealand law and any claim or dispute arising hereunder or in connection herewith shall (without prejudice to the Carrier's right to commence proceedings in any other jurisdiction) be subject to the jurisdiction of the Courts of New Zealand.
[70,13] By _________________________________________________________
[72,4] HBoL-INZ";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("199317af-aa4e-43a6-adf6-d9632d53c50f");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
