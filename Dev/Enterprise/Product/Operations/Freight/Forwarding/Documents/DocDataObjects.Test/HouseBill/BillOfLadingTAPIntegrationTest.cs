using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingTAPIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading TAP";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.TANHBLPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[1,14] BILL OF LADING
[3,26] S00001000
[4,14] NOT NEGOTIABLE UNLESS CONSIGNED 'TO ORDER'
[6,14] ORIGINAL
[14,4]  /
[16,16] SYDNEY, AUSTRALIA
[16,24] 2 (TWO)
[19,11] 0 (s)
[19,26] 0.000 KG
[19,29] 0.000 M3
[28,4] CAN: 
[28,11] FREIGHT PREPAID
[29,4] Consol Ref: 
[29,11] SHIPPED ON BOARD 
[34,4] BRISBANE, AUSTRALIA
[35,4] AS CARRIER
[39,4] SYDNEY, AUSTRALIA
[40,19] ZERO (S)
[42,3] HBoL-TAN";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("91e153b1-ba5a-4d5a-8f63-4a0478d9cd92");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
