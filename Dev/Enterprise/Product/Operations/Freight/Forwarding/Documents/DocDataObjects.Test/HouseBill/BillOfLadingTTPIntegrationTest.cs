using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingTTPIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading TTP";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.TTClubAustraliaNZPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[4,24] S00001000
[6,15] ORIGINAL
[18,4]  /
[19,24] 0
[21,17] SYDNEY, AUSTRALIA
[21,24] 2 (TWO)
[23,9] 0 (s)
[23,22] 0.000 KG
[23,27] 0.000 M3
[26,4] Consol Ref: 
[27,4] CAN: 
[28,4] INCOTERM: 
[29,4] SHIPPED ON BOARD  
[29,24] Australia
[35,4] BRISBANE, AUSTRALIA
[36,4] Signed on behalf of  - the carrier
[39,4] SYDNEY, AUSTRALIA
[42,19] ZERO (S)
[44,4] Hbol-TTC";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("492035a8-7f06-4c4f-a17d-8d4a751bb404");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
