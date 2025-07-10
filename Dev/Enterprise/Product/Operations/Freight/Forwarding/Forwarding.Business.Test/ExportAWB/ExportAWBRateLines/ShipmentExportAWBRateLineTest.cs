using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentExportAWBRateLine))]
	sealed class ShipmentExportAWBRateLineTest : ExportAWBRateLineTest
	{
		public override void TestRateClassList()
		{
			ShipmentExportAWBRateLine rateLine = (ShipmentExportAWBRateLine)AWBHeader.AWBRateLines.AddNew();

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals(OLookUpEditType.AWBRateClass, rateLine.RateClassList.LookupEditType);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals(OLookUpEditType.AWBRateClass, rateLine.RateClassList.LookupEditType);

			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertEquals(OLookUpEditType.AWBPrepayCollect, rateLine.RateClassList.LookupEditType);
		}

		public void TestIsHSCodeLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEHAM";

			AssertIsHSCodeLine0();
			AssertIsHSCodeLine1();
			AssertIsHSCodeLine2();
			AssertIsHSCodeLine3();
			AssertIsHSCodeLine4();
			AssertIsHSCodeLine5();

			void AssertIsHSCodeLine0()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				Assert(!rateLine1.IsHSCodeLine);
				Assert(!rateLine2.IsHSCodeLine);
			}

			void AssertIsHSCodeLine1()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				var rateLine3 = AddRateLine(header);
				var rateLine4 = AddRateLine(header);
				rateLine1.NatureAndQtyOfGoods.Text = "HS Codes: 490191, 490210, 490590";
				Assert(rateLine1.IsHSCodeLine);
				Assert(!rateLine2.IsHSCodeLine);
				Assert(!rateLine3.IsHSCodeLine);
				Assert(!rateLine4.IsHSCodeLine);
			}

			void AssertIsHSCodeLine2()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				var rateLine3 = AddRateLine(header);
				var rateLine4 = AddRateLine(header);
				rateLine1.NatureAndQtyOfGoods.Text = "HS Codes: 490191, 490210, 490590";
				rateLine2.NatureAndQtyOfGoods.Text = "491191, 490290, 490110, 490199";
				Assert(rateLine1.IsHSCodeLine);
				Assert(rateLine2.IsHSCodeLine);
				Assert(!rateLine3.IsHSCodeLine);
				Assert(!rateLine4.IsHSCodeLine);
			}

			void AssertIsHSCodeLine3()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				var rateLine3 = AddRateLine(header);
				var rateLine4 = AddRateLine(header);
				rateLine1.NatureAndQtyOfGoods.Text = "HS Codes: 490191, 490210, 490590";
				rateLine2.NatureAndQtyOfGoods.Text = "491191, 490290, 490110, 490199";
				rateLine3.NatureAndQtyOfGoods.Text = "491291, 490292, 490293, 490294";
				Assert(rateLine1.IsHSCodeLine);
				Assert(rateLine2.IsHSCodeLine);
				Assert(rateLine3.IsHSCodeLine);
				Assert(!rateLine4.IsHSCodeLine);
			}

			void AssertIsHSCodeLine4()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				var rateLine3 = AddRateLine(header);
				var rateLine4 = AddRateLine(header);
				rateLine1.NatureAndQtyOfGoods.Text = "HS Codes: 490191, 490210, 490590";
				rateLine2.NatureAndQtyOfGoods.Text = "This has nothing to do with HS Codes";
				rateLine3.NatureAndQtyOfGoods.Text = "Neither does this, but there is a number which overflows to line 4";
				rateLine4.NatureAndQtyOfGoods.Text = "491291";
				Assert(rateLine1.IsHSCodeLine);
				Assert(!rateLine2.IsHSCodeLine);
				Assert(!rateLine3.IsHSCodeLine);
				Assert(!rateLine4.IsHSCodeLine);
			}

			void AssertIsHSCodeLine5()
			{
				var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var rateLine1 = AddRateLine(header);
				var rateLine2 = AddRateLine(header);
				var rateLine3 = AddRateLine(header);
				rateLine1.NatureAndQtyOfGoods.Text = "HS Codes: 490191, 490210, 490590";
				rateLine2.NatureAndQtyOfGoods.Text = "491191, 490290, 490110, 490199";
				rateLine3.NatureAndQtyOfGoods.Text = "491291, 490292, 490293, 490294";
				Assert(rateLine1.IsHSCodeLine);
				Assert(rateLine2.IsHSCodeLine);
				Assert(rateLine3.IsHSCodeLine);
			}

			ShipmentExportAWBRateLine AddRateLine(ShipmentExportAWBHeader header)
			{
				var result = (ShipmentExportAWBRateLine)header.AWBRateLines.AddNew();
				result.ER_LineCount = (ZByte)(header.AWBRateLines.Count + 1);
				return result;
			}
		}

		public void TestRequireHSCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEHAM";

			var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var rateLine = (ShipmentExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(rateLine.RequireHSCode);

			shipment.JS_RL_NKDestination = "NZAKL";
			transport.JW_RL_NKDiscPort = "NZAKL";
			Assert(!rateLine.RequireHSCode);

			shipment.JS_RL_NKOrigin = "ITSPE";
			shipment.JS_RL_NKDestination = "DEHAM";

			transport.JW_RL_NKLoadPort = "ITSPE";
			transport.JW_RL_NKDiscPort = "DEHAM";
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenAirLegsAreDomestic()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "DEFRA";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USJFK";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USJFK";
			transport2.JW_RL_NKDiscPort = "USHAM";

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USHAM";
			transport3.JW_RL_NKDiscPort = "USFRA";

			var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var rateLine = (ShipmentExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenSomeEuAirLegsAreDomesticDuringTransit()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "UAKBP";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USJFK";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USJFK";
			transport2.JW_RL_NKDiscPort = "USHAM";

			var transport3 = shipment.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "USHAM";
			transport3.JW_RL_NKDiscPort = "USFRA";

			var transport4 = shipment.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport4.JW_RL_NKLoadPort = "USFRA";
			transport4.JW_RL_NKDiscPort = "UAKBP";

			var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var rateLine = (ShipmentExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(!rateLine.RequireHSCode);
		}

		public void TestRequireHSCode_WhenTransittingThroughEU()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "USLAX";

			var header = Factory.NewWithValidTestData<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			var rateLine = (ShipmentExportAWBRateLine)header.AWBRateLines.AddNew();
			Assert(rateLine.RequireHSCode);

			transport1.JW_RL_NKDiscPort = "NZAKL";
			Assert(!rateLine.RequireHSCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<ShipmentExportAWBRateLine>();
		}

		#endregion
	}
}
