using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(NatureAndQtyOfGoodsValidation))]
	sealed class NatureAndQtyOfGoodsValidationTest : Forwarding.AWB.Business.Testing.NatureAndQtyOfGoodsValidationTest
	{
		#region Indonesia HS Code

		public void TestHSCodeRequired_ID()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols[0].JK_RL_NKDischargePort = "ID6DI";

			awbHeader.EH_ParentID = shipment.Consols[0].PK;
			awbHeader.Consol.JK_AgentType = AgentType.Agent;

			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Indonesia. Enter it on the pack line(s) or override and select H identifier to include it.");

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_HarmonisedCode = "234234";
			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Indonesia. Enter it on the pack line(s) or override and select H identifier to include it.");

			shipment.OuterPackLines.RemoveAll();
			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Indonesia. Enter it on the pack line(s) or override and select H identifier to include it.");

			awbHeader.AWBRateLine10.NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123456";
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Indonesia. Enter it on the pack line(s) or override and select H identifier to include it.");
		}

		#endregion

		#region Argentina HS Code

		public void TestHSCode_IsRequired_WhenDestinationIsArgentina_Consol()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols[0].JK_RL_NKDischargePort = "ARARR";

			awbHeader.EH_ParentID = shipment.Consols[0].PK;
			awbHeader.Consol.JK_AgentType = AgentType.Agent;

			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina. Enter it on the pack line(s) or override and select H identifier to include it.");

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_HarmonisedCode = "234234";
			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina. Enter it on the pack line(s) or override and select H identifier to include it.");

			shipment.OuterPackLines.RemoveAll();
			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina. Enter it on the pack line(s) or override and select H identifier to include it.");

			awbHeader.AWBRateLine10.NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123456";
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina. Enter it on the pack line(s) or override and select H identifier to include it.");
		}

		public void TestHSCode_Requires6Digits_WhenDestinationIsArgentina_Consol()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_UniqueConsignRef = "S00001001";

			shipment.Consols.AddNew();
			shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
			shipment.Consols[0].JK_RL_NKDischargePort = "ARARR";

			awbHeader.EH_ParentID = shipment.Consols[0].PK;
			awbHeader.Consol.JK_AgentType = AgentType.Agent;

			awbHeader.Populate();

			string errorMessage = "For Shipments destined to Argentina HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";

			awbHeader.AWBRateLine10.NatureAndQtyOfGoodsType = Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345";
			AssertHasMessageError(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);

			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345a";
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);

			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123-45";
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);

			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123456";
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);

			awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123451234512345123";
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);
		}

		public void TestHSCode_IsRequired_WhenDestinationIsArgentina_Shipment()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDestination = "ARARR";
			shipment.JS_RL_NKDischargePort = "ARARR";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			awbHeader.Populate();

			AssertHasMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina.");

			packLine.JL_HarmonisedCode = "1000";
			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, "HS code is mandatory for destination Argentina.");
		}

		public void TestHSCode_Requires6Digits_WhenDestinationIsArgentina_Shipment()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDestination = "ARARR";
			shipment.JS_RL_NKDischargePort = "ARARR";
			shipment.JS_UniqueConsignRef = "S00001001";
			var packLine = awbHeader.Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			string errorMessage = "For Shipments destined to Argentina HS Code length must be between 6 and 18 digits long.";

			packLine.JL_HarmonisedCode = "12345a";
			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

			packLine.JL_HarmonisedCode = "123-45";
			awbHeader.Populate();
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

			packLine.JL_HarmonisedCode = "123456";
			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

			packLine.JL_HarmonisedCode = "123451234512345";
			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
		}

		public void TestHSCode_OverrideStackOverflow_WhenDestinationIsArgentina_Shipment()
		{
			var localFactory = new BusinessObjectFactory();
			var awbHeader = localFactory.New<ShipmentExportAWBHeader>();
			var shipment = localFactory.New<ForwardingShipment>();
			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDestination = "ARARR";
			shipment.JS_RL_NKDischargePort = "ARARR";
			var packLine = awbHeader.Shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_ActualWeight = 1;
			packLine.JL_Length = 1;
			packLine.JL_Width = 1;
			packLine.JL_Height = 1;
			shipment.UpdateShipmentFromOuterPackLines();
			awbHeader.Populate();
			shipment.IsAWBValuesOverriddenProperty = true;
			awbHeader.EH_AreRateLinesOverridden = true;
			localFactory.Save();

			awbHeader = Factory.Load<ShipmentExportAWBHeader>(awbHeader.PK);

			AssertNoExceptionThrown(() => awbHeader.Populate());
		}

		#endregion

		#region EU HS Code - Consol

		public void TestHSCode_Requires6Digits_WhenDestinationIsOneOfIcs2Zones_Consol()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Zone in ics2Zones)
				{
					AWBHeader = Factory.New<ConsolExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;
					packLine.JL_HarmonisedCode = "123456";

					shipment.Consols.AddNew();
					shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
					shipment.Consols[0].JK_RL_NKDischargePort = ics2Zone;

					var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
					shipment.Consols[0].JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
					var misc = receivingForwarder.MiscServ;
					misc.OM_FWAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.EH_ParentID = shipment.Consols[0].PK;
					AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;

					AWBHeader.Populate();
					AssertEquals(isICS2SelfFilling, AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.";

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345";
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true);
					packLine.JL_HarmonisedCode = "12345";
					AWBHeader.Populate();
					AssertEquals("HS Code: 12345", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "1234567";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);
					packLine.JL_HarmonisedCode = "1234567";
					AWBHeader.Populate();
					AssertEquals("HS Code: 1234567", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345a";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true);
					packLine.JL_HarmonisedCode = "12345a";
					AWBHeader.Populate();
					AssertEquals("HS Code: 12345a", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123-45";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true);
					packLine.JL_HarmonisedCode = "123-45";
					AWBHeader.Populate();
					AssertEquals("HS Code: 123-45", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123456";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);
					packLine.JL_HarmonisedCode = "123456";
					AWBHeader.Populate();
					AssertEquals("HS Code: 123456", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					shipment.Consols[0].Transports[0].JW_RL_NKLoadPort = "ITSPE";
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123-45";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);
					packLine.JL_HarmonisedCode = "123-45";
					AWBHeader.Populate();
					AssertEquals("HS Code: 123-45", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
					consigneeOrg.OH_Category = "NAT";
					shipment.ConsigneePK = consigneeOrg.PK;

					var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
					shipperOrg.OH_Category = "NAT";
					shipment.ConsignorPK = shipperOrg.PK;
					AWBHeader.Populate();

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
							Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true, false, true);
					packLine.JL_HarmonisedCode = "12345";
					AWBHeader.Populate();
					AssertEquals("HS Code: 12345", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true, false, true);

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "1234567";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, false, false, true);
					packLine.JL_HarmonisedCode = "1234567";
					AWBHeader.Populate();
					AssertEquals("HS Code: 1234567", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, false, false, true);
				}
			}
		}

		public void TestHSCode_IsHSCodeMandatory_WhenDestinationIsOneOfIcs2Zones()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Zone in ics2Zones)
				{
					AWBHeader = Factory.New<ConsolExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_UniqueConsignRef = "S00001001";

					shipment.Consols.AddNew();
					shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
					shipment.Consols[0].JK_RL_NKDischargePort = ics2Zone;

					var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
					shipment.Consols[0].JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
					var misc = receivingForwarder.MiscServ;
					misc.OM_FWAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.EH_ParentID = shipment.Consols[0].PK;
					AWBHeader.Consol.JK_AgentType = AgentType.Agent;

					AWBHeader.Populate();
					AssertEquals(isICS2SelfFilling, AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";
					AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					shipment.Consols[0].JK_OverrideWaybillDefaults = ZBool.True;

					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true, true, false);

					shipment.Consols[0].JK_OverrideWaybillDefaults = ZBool.False;
					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;
					packLine.JL_HarmonisedCode = "123456";

					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					shipment.Consols[0].Transports[0].JW_RL_NKLoadPort = "ITSPE";
					packLine.JL_HarmonisedCode = ZString.Empty;
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		public void TestHSCode_Requires6Digits_WhenShippingToIcs2Members()
		{
			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Members = new List<string> { "GBAHE", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Member in ics2Members)
				{
					AWBHeader = Factory.New<ConsolExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;
					packLine.JL_HarmonisedCode = "123456";

					shipment.Consols.AddNew();
					shipment.Consols[0].JK_RL_NKLoadPort = "AUSYD";
					shipment.Consols[0].JK_RL_NKDischargePort = "FRPAR";

					var transport1 = shipment.Consols[0].Transports[0];
					transport1.JW_TransportMode = Constants.TransportModes.Air;
					transport1.JW_RL_NKLoadPort = "AUSYD";
					transport1.JW_RL_NKDiscPort = ics2Member;

					var transport2 = shipment.Consols[0].Transports.AddNew();
					transport2.JW_TransportMode = Constants.TransportModes.Air;
					transport2.JW_RL_NKLoadPort = ics2Member;
					transport2.JW_RL_NKDiscPort = "FRPAR";

					var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
					shipment.Consols[0].JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
					var misc = receivingForwarder.MiscServ;
					misc.OM_FWAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.EH_ParentID = shipment.Consols[0].PK;
					AWBHeader.Consol.JK_AgentType = Constants.AgentType.Agent;

					AWBHeader.Populate();
					AssertEquals(isICS2SelfFilling, AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.";

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345";
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true);
					packLine.JL_HarmonisedCode = "12345";
					AWBHeader.Populate();
					AssertEquals("HS Code: 12345", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					shipment.Consols[0].JK_OverrideWaybillDefaults = ZBool.True;

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType =
						Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode;
					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "12345";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true, true);
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage, true, true);

					shipment.Consols[0].JK_OverrideWaybillDefaults = ZBool.False;
					AWBHeader.EH_AreRateLinesOverridden = ZBool.False;
					AWBHeader.Populate();

					AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.Text = "123456";
					AWBHeader.RunPreSaveValidation();
					AssertHSValidation(AWBHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo, errorMessage);
					packLine.JL_HarmonisedCode = "123456";
					AWBHeader.Populate();
					AssertEquals("HS Code: 123456", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		public void TestHSCode_IsHSCodeMandatory_WhenShippingToIcs2Members()
		{
			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Members = new List<string> { "GBAHE", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Member in ics2Members)
				{
					AWBHeader = Factory.New<ConsolExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_UniqueConsignRef = "S00001001";

					var consol = shipment.Consols.AddNew();
					consol.JK_RL_NKLoadPort = "AUSYD";
					consol.JK_RL_NKDischargePort = "FRPAR";

					var transport1 = consol.Transports[0];
					transport1.JW_TransportMode = TransportModes.Air;
					transport1.JW_RL_NKLoadPort = "AUSYD";
					transport1.JW_RL_NKDiscPort = ics2Member;

					var transport2 = consol.Transports.AddNew();
					transport2.JW_TransportMode = TransportModes.Air;
					transport2.JW_RL_NKLoadPort = ics2Member;
					transport2.JW_RL_NKDiscPort = "FRPAR";

					var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
					shipment.Consols[0].JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
					var misc = receivingForwarder.MiscServ;
					misc.OM_FWAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.EH_ParentID = consol.PK;
					AWBHeader.Consol.JK_AgentType = AgentType.Agent;

					AWBHeader.Populate();
					AssertEquals(isICS2SelfFilling, this.AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage =
						"For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";
					AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription,
						AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;
					packLine.JL_HarmonisedCode = "123456";

					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		public void TestHSErrorMessage_WhenPopluateAWB_DistinguishPacklineAndHVLVItemLine()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				AWBHeader = Factory.New<ConsolExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_UniqueConsignRef = "S00001001";
				shipment.JS_OverrideWaybillDefaults = ZBool.False;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";

				var consol = shipment.Consols.AddNew();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "GBBEL";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment2.JS_UniqueConsignRef = "S00001002";

				var header = Factory.New<IHVLVConsignmentHeader>() as BusinessObject;
				header[HVLVConsignmentHeaderSchema.HCH_JS_Shipment.Name] = shipment.PK;

				var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
				consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;
				consignment[HVLVConsignmentSchema.HVC_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				shipment.Consols[0].JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				var misc = receivingForwarder.MiscServ;
				misc.OM_FWAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

				AWBHeader.EH_ParentID = consol.PK;
				AWBHeader.Consol.JK_AgentType = AgentType.Agent;

				var item = Factory.New<IHVLVItem>() as BusinessObject;
				item[HVLVItemSchema.HVI_HVC_Consignment] = consignment.PK;
				item[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipment.PK;
				item[HVLVItemSchema.HVI_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

				var itemLine = Factory.New<IHVLVItemLine>() as BusinessObject;
				itemLine[HVLVItemLineSchema.HVS_HVI_HVLVItem] = item.PK;
				itemLine[HVLVItemLineSchema.HVS_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];
				itemLine[HVLVItemLineSchema.HVS_Quantity] = (ZShort)1;
				itemLine[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode] = "AU";

				AWBHeader.Populate();
				AssertEquals(isICS2SelfFilling, AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

				var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one HVLV Item Line of S00001001, Packline of S00001002.";
				AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription,
					AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
				AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

				var packLine = shipment2.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_HarmonisedCode = "123456";
				itemLine[HVLVItemLineSchema.HVS_DestinationTariff] = "654321";

				AWBHeader.Populate();
				AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		#endregion

		#region EU HS Code - Shipment

		public void TestHSCode_Requires6Digits_WhenDestinationIsOneOfIcs2Zones_Shipment()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Zone in ics2Zones)
				{
					AWBHeader = Factory.New<ShipmentExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					AWBHeader.EH_ParentID = shipment.PK;

					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = ics2Zone;
					var transport = shipment.Transports.AddNew();
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = ics2Zone;

					var packLine = ((ShipmentExportAWBHeader)AWBHeader).Shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;

					var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
					consigneeOrg.OH_Category = "NAT";
					shipment.ConsigneePK = consigneeOrg.PK;
					var misc = consigneeOrg.MiscServ;
					misc.OM_IMAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.Populate();
					Assert("ShipmentExportAWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting should always return false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.";

					packLine.JL_HarmonisedCode = "1234567";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					packLine.JL_HarmonisedCode = "12345a";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "123-45";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "123456";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					transport.JW_RL_NKLoadPort = "ITSPE";
					packLine.JL_HarmonisedCode = "123-45";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
					shipperOrg.OH_Category = "NAT";
					shipment.ConsignorPK = shipperOrg.PK;
					AWBHeader.Populate();

					packLine.JL_HarmonisedCode = "12345";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true, false, true);

					packLine.JL_HarmonisedCode = "1234567";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, false, false, true);
				}
			}
		}

		public void TestHSCode_IsRequired_WhenDestinationIsOneOfIcs2Zones()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Zones = new List<string> { "GBBEL", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Zone in ics2Zones)
				{
					AWBHeader = Factory.New<ShipmentExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();

					AWBHeader.EH_ParentID = shipment.PK;

					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = ics2Zone;
					shipment.JS_UniqueConsignRef = "S00001001";
					var transport = shipment.Transports.AddNew();
					transport.JW_RL_NKLoadPort = "AUSYD";
					transport.JW_RL_NKDiscPort = ics2Zone;

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;

					var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
					shipment.ConsigneePK = consigneeOrg.PK;
					var misc = consigneeOrg.MiscServ;
					misc.OM_IMAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.Populate();
					Assert("ShipmentExportAWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting should always return false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";
					AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "100012";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);

					transport.JW_RL_NKLoadPort = "ITSPE";
					packLine.JL_HarmonisedCode = ZString.Empty;
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		public void TestHSErrorMessage_WhenPopluateAWBForHVLVShipment_PrintHVLVItemLine()
		{
			SetupNorthernIrelandZone();

			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Zone = "GBBEL";
				AWBHeader = Factory.New<ShipmentExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();

				AWBHeader.EH_ParentID = shipment.PK;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_OverrideWaybillDefaults = ZBool.False;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S00001001";
				var transport = shipment.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = ics2Zone;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipment.ConsigneePK = consigneeOrg.PK;
				var misc = consigneeOrg.MiscServ;
				misc.OM_IMAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

				var header = Factory.New<IHVLVConsignmentHeader>() as BusinessObject;
				header[HVLVConsignmentHeaderSchema.HCH_JS_Shipment.Name] = shipment.PK;

				var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
				consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;
				consignment[HVLVConsignmentSchema.HVC_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

				var item = Factory.New<IHVLVItem>() as BusinessObject;
				item[HVLVItemSchema.HVI_HVC_Consignment] = consignment.PK;
				item[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipment.PK;
				item[HVLVItemSchema.HVI_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

				var itemLine = Factory.New<IHVLVItemLine>() as BusinessObject;
				itemLine[HVLVItemLineSchema.HVS_HVI_HVLVItem] = item.PK;
				itemLine[HVLVItemLineSchema.HVS_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];
				itemLine[HVLVItemLineSchema.HVS_Quantity] = (ZShort)1;
				itemLine[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode] = "AU";

				AWBHeader.Populate();
				Assert("ShipmentExportAWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting should always return false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

				var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one HVLV Item Line of S00001001.";
				AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription,
					AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsType);
				AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

				itemLine[HVLVItemLineSchema.HVS_DestinationTariff] = "654321";
				AWBHeader.Populate();
				AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		public void TestHSCode_Requires6Digits_WhenTransittingThroughIcs2Members()
		{
			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Members = new List<string> { "GBAHE", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Member in ics2Members)
				{
					AWBHeader = Factory.New<ShipmentExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					AWBHeader.EH_ParentID = shipment.PK;

					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "FRPAR";

					var transport1 = shipment.Transports.AddNew();
					transport1.JW_TransportMode = Constants.TransportModes.Air;
					transport1.JW_RL_NKLoadPort = "AUSYD";
					transport1.JW_RL_NKDiscPort = ics2Member;

					var transport2 = shipment.Transports.AddNew();
					transport2.JW_TransportMode = Constants.TransportModes.Air;
					transport2.JW_RL_NKLoadPort = ics2Member;
					transport2.JW_RL_NKDiscPort = "FRPAR";

					var packLine = ((ShipmentExportAWBHeader)AWBHeader).Shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;

					var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
					shipment.ConsigneePK = consigneeOrg.PK;
					var misc = consigneeOrg.MiscServ;
					misc.OM_IMAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.Populate();
					Assert("ShipmentExportAWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting should always return false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.";

					packLine.JL_HarmonisedCode = "1234567";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

					packLine.JL_HarmonisedCode = "12345a";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "123-45";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "123456";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		public void TestHSCode_IsRequired_WhenTransittingThroughIcs2Members()
		{
			HscodeTest();
			HscodeTest(isICS2SelfFilling: true);

			void HscodeTest(bool isICS2SelfFilling = false)
			{
				var ics2Members = new List<string> { "GBAHE", "NOABE", "CHARF", "DEHAM" };
				foreach (var ics2Member in ics2Members)
				{
					AWBHeader = Factory.New<ShipmentExportAWBHeader>();
					var shipment = Factory.New<ForwardingShipment>();
					AWBHeader.EH_ParentID = shipment.PK;

					shipment.JS_OverrideWaybillDefaults = ZBool.False;
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "FRPAR";
					shipment.JS_UniqueConsignRef = "S00001001";

					var transport1 = shipment.Transports.AddNew();
					transport1.JW_TransportMode = Constants.TransportModes.Air;
					transport1.JW_RL_NKLoadPort = "AUSYD";
					transport1.JW_RL_NKDiscPort = ics2Member;

					var transport2 = shipment.Transports.AddNew();
					transport2.JW_TransportMode = Constants.TransportModes.Air;
					transport2.JW_RL_NKLoadPort = ics2Member;
					transport2.JW_RL_NKDiscPort = "FRPAR";

					var packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_PackageCount = 1;

					var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
					shipment.ConsigneePK = consigneeOrg.PK;
					var misc = consigneeOrg.MiscServ;
					misc.OM_IMAdvanceCargoReportingSelfFiler = isICS2SelfFilling;

					AWBHeader.Populate();
					Assert("ShipmentExportAWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting should always return false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

					var errorMessage = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";
					AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage, true);

					packLine.JL_HarmonisedCode = "100012";
					AWBHeader.Populate();
					AssertHSValidation(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);
				}
			}
		}

		#endregion

		#region HS Code - Switzerland

		public void TestHSCode_Switzerland_Consol()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CHBSL";
			consol.JK_RL_NKDischargePort = "USCHI";
			awbHeader.EH_ParentID = consol.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "CHBSL";
			shipment1.JS_RL_NKDestination = "USCHI";

			var packline11 = shipment1.OuterPackLines.AddNew();
			packline11.JL_HarmonisedCode = "123456";
			var packline12 = shipment1.OuterPackLines.AddNew();
			Factory.Save();

			var expectedMessage = $"For exports from Basel, Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, the HS Code is mandatory. A HS Code is missing from at least one Packline of {shipment1.JS_UniqueConsignRef}.";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError("packline12 does not have HS Code", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline11.JL_HarmonisedCode = ZString.Empty;
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError("packline11 and packline12 do not have HS Code", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline11.JL_HarmonisedCode = "123456";
			packline12.JL_HarmonisedCode = "654321";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "CHBSL";
			shipment2.JS_RL_NKDestination = "USCHI";

			var packline21 = shipment2.OuterPackLines.AddNew();
			packline21.JL_HarmonisedCode = "123456";
			var packline22 = shipment2.OuterPackLines.AddNew();
			Factory.Save();
			expectedMessage = $"For exports from Basel, Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, the HS Code is mandatory. A HS Code is missing from at least one Packline of {shipment2.JS_UniqueConsignRef}.";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError("packline22 does not have HS Code", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline22.HarmonisedCodes.HSCodeManager.Value = "123456";
			packline22.HarmonisedCodes.HSCountryManager.Value = "CH";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);
			AssertNoMessageError("All packlines have HS Code", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.TextInfo, expectedMessage);
		}

		public void TestHSCode_Switzerland_Shipment()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CHBSL";
			consol.JK_RL_NKDischargePort = "USCHI";
			var transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_RL_NKLoadPort = "CHBSL";
			transport.JW_RL_NKDiscPort = "USCHI";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "CHABL";
			shipment.JS_RL_NKDestination = "USCHI";
			awbHeader.EH_ParentID = shipment.PK;

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "CHBSL";
			consol2.JK_RL_NKDischargePort = "USCHI";
			var transport2 = consol2.Transports[0];
			transport2.JW_ETD = ZDateTime.Today.AddDays(2);
			transport2.JW_RL_NKLoadPort = "CHABL";
			transport2.JW_RL_NKDiscPort = "USCHI";

			Factory.Save();

			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageErrors("Should not display error message if Consol is direct", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo);
			AssertNoMessageErrors("Should not display error message if Consol is direct", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Factory.Save();

			var expectedMessage = $"For exports from Basel, Switzerland by airfreight, to meet RFS (Road Feeder Service) customs filing requirements, the HS Code is mandatory. A HS Code is missing from at least one Packline of {shipment.JS_UniqueConsignRef}.";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError("packline1 and packline2 do not have HS Code", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline1.JL_HarmonisedCode = "123456";
			packline2.JL_HarmonisedCode = "654321";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError("All Packlines have HS Code", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);
			AssertNoMessageError("All Packlines have HS Code", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline1.JL_HarmonisedCode = ZString.Empty;
			packline2.JL_HarmonisedCode = ZString.Empty;
			packline1.HarmonisedCodes.HSCodeManager.Value = "123456";
			packline1.HarmonisedCodes.HSCountryManager.Value = "CH";
			packline2.HarmonisedCodes.HSCodeManager.Value = "654321";
			packline2.HarmonisedCodes.HSCountryManager.Value = "CH";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError("All Packlines have HS Code", awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, expectedMessage);
			AssertNoMessageError("All Packlines have HS Code", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			packline1.HarmonisedCodes.HSCodeManager.Value = ZString.Empty;
			packline1.HarmonisedCodes.HSCountryManager.Value = ZString.Empty;
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertHasMessageError("packline1 dose not have HS Code", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);

			consol.JK_RL_NKLoadPort = "AUSYD";
			awbHeader.Populate();
			awbHeader.RunPreSaveValidation();
			AssertNoMessageError("The consol is not from Switzerland", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, expectedMessage);
		}

		#endregion

		#region UAE HS Code

		[TestDate(2022, 7, 20)]
		public void TestHSCode_Requires6Digits_UAE()
		{
			TestHSCode_Requires6Digits("AEAAN", "AUSYD", "CNSHA", "AUSYD");
			TestHSCode_Requires6Digits("AUSYD", "AEAAN", "AUSYD", "CNSHA");
			TestHSCode_Requires6Digits("AUSYD", "CNSHA", "AUSYD", "AEAAN");
			TestHSCode_Requires6Digits("AUSYD", "CNSHA", "AEAAN", "CNSHA");

			void TestHSCode_Requires6Digits(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;

				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;
				packLine.JL_HarmonisedCode = "1234567";
				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "12345a";
				awbHeader.Populate();
				AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "123-45";
				awbHeader.Populate();
				AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "123456";
				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				consigneeOrg.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				shipperOrg.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				packLine.JL_HarmonisedCode = "12345";
				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "1234567";
				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		[TestDate(2022, 7, 20)]
		public void TestHSCode_IsRequired_UAE()
		{
			TestHSCode_IsRequired("AEAAN", "AUSYD", "CNSHA", "AUSYD");
			TestHSCode_IsRequired("AUSYD", "AEAAN", "AUSYD", "CNSHA");
			TestHSCode_IsRequired("AUSYD", "CNSHA", "AUSYD", "AEAAN");
			TestHSCode_IsRequired("AUSYD", "CNSHA", "AEAAN", "CNSHA");

			void TestHSCode_IsRequired(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long. An HS code is missing from at least one Packline of S00001001.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_OverrideWaybillDefaults = ZBool.False;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;
				shipment.JS_UniqueConsignRef = "S00001001";

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;

				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;
				packLine.JL_HarmonisedCode = ZString.Empty;
				awbHeader.Populate();
				AssertHasMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "100012";
				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		[TestDate(2024, 6, 3)]
		public void TestHSCode_IsRequiredWith6Digits_UAE_And_ICS2()
		{
			var awbHeader = Factory.New<ConsolExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_UniqueConsignRef = "S00001001";

			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_Category = OrgConstants.Category.Government;
			shipment.ConsigneePK = consigneeOrg.PK;

			var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipperOrg.OH_Category = OrgConstants.Category.Government;
			shipment.ConsignorPK = shipperOrg.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AEAAN";
			consol.JK_RL_NKDischargePort = "DEHAM";

			awbHeader.EH_ParentID = consol.PK;
			awbHeader.Consol.JK_AgentType = AgentType.Agent;

			awbHeader.Populate();
			var errorMessageICS2 = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long. A HS code is missing from at least one Packline of S00001001.";
			var errorMessageUAE = "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long. An HS code is missing from at least one Packline of S00001001.";
			AssertEquals(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, awbHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageICS2);
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageUAE);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_HarmonisedCode = "123456";

			awbHeader.Populate();
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageICS2);
			AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageUAE);

			packLine.JL_HarmonisedCode = "12345";
			awbHeader.Populate();
			errorMessageICS2 = "For inbound shipments to EU, CH, NO and XI HS Code length must be between 6 and 18 digits long.";
			errorMessageUAE = "For inbound, outbound and transiting shipments to/from/via UAE HS Code length must be between 6 and 18 digits long.";
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageICS2);
			AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessageUAE);
		}

		#endregion

		#region Morocco HS Code

		[TestDate(2022, 7, 20)]
		public void TestHSCode_IsRequired_Morocco_Shipment()
		{
			TestHSCode_IsRequired("AUSYD", "MACAS", "AUSYD", "MACAS");
			TestHSCode_IsRequired("AUSYD", "MACAS", "AUSYD", "CNSHA");
			TestHSCode_IsRequired("AUSYD", "CNSHA", "AUSYD", "MACAS");

			void TestHSCode_IsRequired(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "For inbound shipments to Morocco, an HS code is mandatory. An HS code is missing from at least one Packline of S00001001.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_OverrideWaybillDefaults = ZBool.False;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;
				shipment.JS_UniqueConsignRef = "S00001001";

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_HarmonisedCode = "123456";

				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;
				packLine.JL_HarmonisedCode = ZString.Empty;
				awbHeader.Populate();
				AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);

				packLine.JL_HarmonisedCode = "100012";

				awbHeader.Populate();
				AssertNoMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		[TestDate(2022, 7, 20)]
		public void TestHSCode_Requires6Digits_Morocco_Shipment()
		{
			TestHSCode_Requires6Digits("AUSYD", "MACAS", "AUSYD", "MACAS");
			TestHSCode_Requires6Digits("AUSYD", "MACAS", "AUSYD", "CNSHA");
			TestHSCode_Requires6Digits("AUSYD", "CNSHA", "AUSYD", "MACAS");

			void TestHSCode_Requires6Digits(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "Harmonized Commodity Code must be between 6 and 18 characters long.";
				HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1234", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;

				var transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_HarmonisedCode = "1234";

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_HarmonisedCode = "123456";

				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;

				awbHeader.Populate();
				AssertEquals("HS Codes: 123400, 123456", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo.OriginalValue);

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 1;
				packLine2.JL_HarmonisedCode = "4444";

				awbHeader.Populate();
				AssertEquals("HS Codes: 123400, 123456, 4444", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo.OriginalValue);
				AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		public void TestHSCode_IsRequired_Morocco_Consol()
		{
			TestHSCode_IsRequired_Consol("AUSYD", "MACAS", "AUSYD", "MACAS");
			TestHSCode_IsRequired_Consol("AUSYD", "MACAS", "AUSYD", "CNSHA");
			TestHSCode_IsRequired_Consol("AUSYD", "CNSHA", "AUSYD", "MACAS");

			void TestHSCode_IsRequired_Consol(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "For inbound shipments to Morocco, an HS code is mandatory. An HS code is missing from at least one Packline of S00001001.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_OverrideWaybillDefaults = ZBool.False;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;
				shipment.JS_UniqueConsignRef = "S00001001";

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				shipment.Consols.AddNew();

				var transport = shipment.Consols[0].Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var transport1 = shipment.Consols[0].Transports.AddNew();
				transport1.JW_TransportMode = TransportModes.Air;
				transport1.JW_RL_NKLoadPort = origin;
				transport1.JW_RL_NKDiscPort = destination;

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.Consols[0].PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_HarmonisedCode = ZString.Empty;

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_HarmonisedCode = "123456";

				awbHeader.Populate();
				AssertHasMessageError(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		[TestDate(2022, 7, 20)]
		public void TestHSCode_Requires6Digits_Morocco_Consol()
		{
			TestHSCode_Requires6Digits_Consol("AUSYD", "MACAS", "AUSYD", "MACAS");
			TestHSCode_Requires6Digits_Consol("AUSYD", "MACAS", "AUSYD", "CNSHA");
			TestHSCode_Requires6Digits_Consol("AUSYD", "CNSHA", "AUSYD", "MACAS");

			void TestHSCode_Requires6Digits_Consol(ZString origin, ZString destination, ZString loadPort, ZString discPort)
			{
				const string errorMessage = "Harmonized Commodity Code must be between 6 and 18 characters long.";
				HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1234", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;

				var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
				consigneeOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsigneePK = consigneeOrg.PK;

				var shipperOrg = Factory.NewWithValidTestData<OrgHeader>();
				shipperOrg.OH_Category = OrgConstants.Category.Government;
				shipment.ConsignorPK = shipperOrg.PK;

				shipment.Consols.AddNew();

				var transport = shipment.Consols[0].Transports.AddNew();
				transport.JW_TransportMode = TransportModes.Air;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = discPort;

				var transport1 = shipment.Consols[0].Transports.AddNew();
				transport1.JW_TransportMode = TransportModes.Air;
				transport1.JW_RL_NKLoadPort = origin;
				transport1.JW_RL_NKDiscPort = destination;

				var awbHeader = Factory.New<ConsolExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.Consols[0].PK;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_HarmonisedCode = "1234";

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_HarmonisedCode = "123456";

				awbHeader.Populate();
				AssertEquals("HS Code: 123400", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo.OriginalValue);

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 1;
				packLine2.JL_HarmonisedCode = "4444";

				awbHeader.Populate();
				AssertEquals("HS Code: 123400", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo.OriginalValue);
				AssertEquals("HS Code: 123456", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.TextInfo.OriginalValue);
				AssertEquals("HS Code: 4444", awbHeader.AWBRateLine4.NatureAndQtyOfGoods.TextInfo.OriginalValue);
				AssertHasMessageError(awbHeader.AWBRateLine4.NatureAndQtyOfGoods.TextInfo, errorMessage);
			}
		}

		#endregion

		#region implementation

		ExportAWBHeader AWBHeader;

		void AssertHSValidation(ZPropertyInfo info, string errorMessage, bool hasError = false, bool isOverriden = false, bool isNat = false)
		{
			Action<ZPropertyInfo, string> action1;

			hasError = hasError && !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting;

			if (isOverriden && hasError)
			{
				action1 = AssertHasWarning;
			}
			else if (isNat && hasError)
			{
				action1 = AssertNoWarning;
			}
			else
			{
				if (hasError)
				{
					action1 = AssertHasMessageError;
				}
				else
				{
					action1 = AssertNoMessageError;
				}
			}

			action1(info, errorMessage);
		}

		void SetupNorthernIrelandZone()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "NORTHERN IRELAND";
			}
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoodsValidation GetNewValidation(Forwarding.AWB.Business.NatureAndQtyOfGoods parent)
		{
			return new NatureAndQtyOfGoodsValidation((NatureAndQtyOfGoods)parent);
		}

		protected override Forwarding.AWB.Business.NatureAndQtyOfGoods GetNewParent()
		{
			return new NatureAndQtyOfGoods(Factory.New<ExportAWBRateLine>());
		}

		#endregion
	}
}
