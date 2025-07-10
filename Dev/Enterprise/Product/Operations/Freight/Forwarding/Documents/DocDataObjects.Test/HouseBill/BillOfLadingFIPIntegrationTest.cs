using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingFIPIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "BillOfLadingFIP";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.FIATAHBLPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,32] S00001000
[18,18] SYDNEY, AUSTRALIA
[20,6]  /
[25,22] 0 (s)
[25,32] 0.000 KG
[25,36] 0.000 M3
[32,6] CAN: 
[32,20] Consol Ref: 
[33,16] ZERO (S)
[34,6] SHIPPED ON BOARD 
[42,6] FREIGHT PREPAID
[42,22] SYDNEY, AUSTRALIA
[42,30] BRISBANE, AUSTRALIA
[44,22] 2 (TWO)
[44,30] AS CARRIER
[55,6] HBoL-FIATA";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("445ef754-b33f-4f54-90a9-a96b60f66be9");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
