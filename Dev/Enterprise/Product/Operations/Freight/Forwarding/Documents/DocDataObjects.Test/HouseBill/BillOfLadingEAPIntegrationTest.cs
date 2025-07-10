using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingEAPIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading EAP";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.CargowiseBillPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,19] HOUSE 
[6,19] ORIGINAL
[6,34] S00001000
[25,10] 23-Sep-22
[25,19] SYDNEY, AUSTRALIA
[25,27] 2 (TWO)
[29,11] 0 (s)
[29,29] 0.000 KG
[29,35] 0.000 M3
[38,4] CAN: 
[38,12] Consol Ref: 
[50,4] AS CARRIER
[56,4] Place Of Issue: 
[56,14] Date Of Issue:
[62,4] SYDNEY, AUSTRALIA
[62,21] ZERO (S)
[64,3] HBoL-EAG";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("4e2bd185-b013-4c0d-94f1-0672d9c1a122");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
