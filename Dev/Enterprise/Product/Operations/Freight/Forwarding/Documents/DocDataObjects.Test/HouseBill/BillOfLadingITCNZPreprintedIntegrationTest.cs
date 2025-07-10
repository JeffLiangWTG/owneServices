using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingITCNZPreprintedIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading ITC NZ Preprinted";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.ITClubNewZealandPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[4,13] ORIGINAL
[5,20] S00001000
[6,13] Consol Reference:
[19,13] Delivery Agent
[27,4] SYDNEY, AUSTRALIA
[27,13] SYDNEY, AUSTRALIA
[29,4]  / 
[29,19] 2 (TWO)
[31,8] 0 (s)
[31,19] 0.000 KG
[31,25] 0.000 M3
[55,4] SHIPPED ON BOARD 
[56,4] Total number of packages: 0 (s) (Outer)
[63,4] The Merchant's attention is called to the fact that according to Clauses 10, 11 and 12 of this Bill of Lading, the liability of the Carrier is, in most cases, limited in respect of loss of or damage to the goods and delay.
[66,13] BRISBANE, AUSTRALIA
[69,4] The Contract evidence by or contained in this Bill of Lading shall be governed by New Zealand law and any claim or dispute arising hereunder or in connection herewith shall (without prejudice to the Carrier's right to commence proceedings in any other jurisdiction) be subject to the jurisdiction of the Courts of New Zealand.
[72,4] HBoL-INZ";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("9217fe6a-a9b3-486d-ab7e-a745e28dfabe");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
