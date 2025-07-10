using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingTANIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading TAN";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.TANHBL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[1,14] BILL OF LADING
[2,4] Consignor
[2,26] House Bill of Lading No.
[3,26] S00001000
[4,14] NOT NEGOTIABLE UNLESS CONSIGNED 'TO ORDER'
[6,14] ORIGINAL
[10,4] Consigned To
[14,4] Notify Party
[14,14] Received by the Carrier, the Goods as specified below in apparent good order and condition unless other-wise stated, to be transported to such place as agreed, authorised or permitted herein and subject to all the terms and conditions appearing on the front and reverse of this Bill of Lading to which the Merchant agrees by accepting this Bill of Lading, any local privileges and customs notwithstanding. The particulars given below as stated by the shipper and the weight, measure, quantity, condition. contents and value of the Goods are unknown to the Carrier. 
In WITNESS, whereof one (1) original Bill of Lading has been signed if not otherwise stated below, the same being accomplished the other(s), if any to be void. If required by the Carrier one (1) original Bill of Lading must be surrendered duly endorsed in exchange for the Goods or delivery order.
[18,3] Vessel and Voyage No.
[18,15] Port of Loading
[19,4]  /
[20,3] Port of Discharge
[20,6] Destination (if on-carriage)
[20,15] Freight Payable at
[20,23] No. Of Originals
[21,16] SYDNEY, AUSTRALIA
[21,24] 2 (TWO)
[22,4] Details of cargo as declared by shipper
[22,11] All business undertaken is Subject to our terms and conditions of trading
[23,3] Marks and Numbers
[23,11] Description of Goods
[23,26] Gross
[23,29] Cubic Measurement
[24,11] 0 (s)
[24,26] 0.000 KG
[24,29] 0.000 M3
[33,4] CAN: 
[33,11] FREIGHT PREPAID
[34,4] Consol Ref: 
[34,11] SHIPPED ON BOARD 
[35,4] Bill of Lading must be surrendered to:
[35,18] Freight and Charges
[39,4] Place of Issue:
[40,4] BRISBANE, AUSTRALIA
[41,4] AS CARRIER
[43,3] Place of Receipt
[43,8] Place of Delivery
[46,4] SYDNEY, AUSTRALIA
[47,19] Total No. of Package (in words)
ZERO (S)
[49,3] HBoL-TAN";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("ca44e64e-bc90-43df-ad7d-99571dac92f7");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
