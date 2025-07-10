using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingCargoWiseIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading CargoWise";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CargowiseBill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,4] Consignor
[2,19] HOUSE BILL OF LADING
[2,34] House Bill of Lading
[5,19] ORIGINAL
[5,34] S00001000
[9,4] Consignee
[12,4] Notify Party
[13,19] Received by the Carrier, the Goods as specified below in apparent good order and condition unless otherwise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting this Bill of Lading, any local privileges and customs notwithstanding. 
The particulars given below as stated by the shipper and the weight, measure, quantity, condition, contents and value of the Goods are unknown to the Carrier. In WITNESS, whereof one (1) original Bill of Lading has been signed if not otherwise stated below, the same being accomplished the other(s), if any to be void. If required by the Carrier one (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order. 

[16,4] Vessel
[16,17] Voyage No.
[21,4] Port of Discharge
[21,10] Destination (if on carr)
[21,18] Port of Loading
[21,26] Release
[23,4] Shipped On Board
[23,10] Print Date
[23,18] Freight Payable At
[23,26] No. of Original B/L
[24,10] 23-Sep-22
[24,19] SYDNEY, AUSTRALIA
[24,27] 2 (TWO)
[25,3] Details of cargo as declared by Shipper
[26,4] Marks and Numbers
[26,12] Description of Goods
[26,28] Gross Mass
[26,35] Cubic(M3)
[28,11] 0 (s)
[28,29] 0.000 KG
[28,35] 0.000 M3
[37,4] CAN: 
[37,12] Consol Ref: 
[38,3] Delivery Agent
[38,20] Freight and Charges
[45,3] In witness of the contract herein contained, the above stated number of original Bills of Lading have been issued, one of which to the accomplished, the other(s) being void.
[49,4] AS CARRIER
[55,4] Place Of Issue:
[55,14] Date Of Issue:
[59,3] Place of Acceptance
[59,12] Place of Delivery
[59,20] Total No. of Packages
[61,4] SYDNEY, AUSTRALIA
[61,21] ZERO (S)
[63,3] HBoL-EAG";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("23c5643e-5df0-44bc-bca5-c49a3acefcd4");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
