using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingITCAUPreprintedIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "Bill Of Lading ITC AU Preprinted";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.ITClubAustraliaPreprinted;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[4,23] S00001000
[6,16] ORIGINAL
[17,4]  /
[18,25] 0
[20,18] SYDNEY, AUSTRALIA
[20,25] 2 (TWO)
[22,9] 0 (s)
[22,23] 0.000 KG
[22,28] 0.000 M3
[26,11] Consol Ref: 
[27,4] INCOTERM: 
[27,11] SHIPPED ON BOARD  
[33,4] BRISBANE, AUSTRALIA
[34,4] AS CARRIER
[37,4] SYDNEY, AUSTRALIA
[39,20] ZERO (S)
[44,4] Hbol-ITC";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("1d86e64d-004c-4c97-85dd-3dbbbda701ef");
			var templatePK = new ZGuid("478632c1-1fe0-4e3c-b945-e9c6fafc9310");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
