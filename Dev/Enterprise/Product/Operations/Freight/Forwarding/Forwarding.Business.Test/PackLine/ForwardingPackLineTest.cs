using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class ForwardingPackLineTest : BaseFreightTest
	{
		public void TestInnerPackCount()
		{
			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			var innerPackLine1 = line.InnerPackLines.AddNew();
			innerPackLine1.JL_PackageCount = 3;
			var innerPackLine2 = line.InnerPackLines.AddNew();
			innerPackLine2.JL_PackageCount = 1;
			var innerPackLine3 = line.InnerPackLines.AddNew();
			innerPackLine3.JL_PackageCount = 2;

			AssertEquals("Inner Pack Count Property is properly set", 6, line.InnerPackCount);
		}

		public void TestInnerPackType()
		{
			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			var innerPackLine1 = line.InnerPackLines.AddNew();
			innerPackLine1.JL_F3_NKPackType = "BAG";
			var innerPackLine2 = line.InnerPackLines.AddNew();
			innerPackLine2.JL_F3_NKPackType = "BAG";
			var innerPackLine3 = line.InnerPackLines.AddNew();
			innerPackLine3.JL_F3_NKPackType = "BAG";

			AssertEquals("Pack type is set", "BAG", line.InnerPackType);

			innerPackLine3.JL_F3_NKPackType = "BOX";

			AssertEquals("When multiple pack types, should be PKG", "PKG", line.InnerPackType);
		}

		public void TestInnerPackTotalWeight()
		{
			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var innerPackLine1 = line.InnerPackLines.AddNew();
			innerPackLine1.JL_ActualWeight = 10;
			innerPackLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var innerPackLine2 = line.InnerPackLines.AddNew();
			innerPackLine2.JL_ActualWeight = 20;
			innerPackLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var innerPackLine3 = line.InnerPackLines.AddNew();
			innerPackLine3.JL_ActualWeight = 15;
			innerPackLine3.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var innerPackLine4 = line.InnerPackLines.AddNew();
			innerPackLine4.JL_ActualWeight = 15;
			innerPackLine4.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			AssertEquals("Total weight of inners should add up", new ZDecimal(60), line.InnerPackTotalWeight);
		}

		public void TestInnerPackTotalVolume()
		{
			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_ActualVolumeUQ = Constants.Volume.Litre;

			var innerPackLine1 = line.InnerPackLines.AddNew();
			innerPackLine1.JL_ActualVolume = 10;
			innerPackLine1.JL_ActualVolumeUQ = Constants.Volume.Litre;

			var innerPackLine2 = line.InnerPackLines.AddNew();
			innerPackLine2.JL_ActualVolume = 20;
			innerPackLine2.JL_ActualVolumeUQ = Constants.Volume.Litre;

			var innerPackLine3 = line.InnerPackLines.AddNew();
			innerPackLine3.JL_ActualVolume = 15;
			innerPackLine3.JL_ActualVolumeUQ = Constants.Volume.Litre;

			var innerPackLine4 = line.InnerPackLines.AddNew();
			innerPackLine4.JL_ActualVolume = 15;
			innerPackLine4.JL_ActualVolumeUQ = Constants.Volume.Litre;

			AssertEquals("Total volume of inners should add up", new ZDecimal(60), line.InnerPackTotalVolume);
		}

		public void TestLastKnownTransitWarehouseStatusForBinding()
		{
			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals("DSP", line.JL_LastKnownTransitWarehouseStatus);

			line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals("DSP", line.JL_LastKnownTransitWarehouseStatus);

			line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals("DSP", line.JL_LastKnownTransitWarehouseStatus);

			line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals("DSP", line.JL_LastKnownTransitWarehouseStatus);

			line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.LastKnownTransitWarehouseStatusChanging += (sender, e) => e.Cancel = true;
			line.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals("DSP", line.JL_LastKnownTransitWarehouseStatus);

			line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.LastKnownTransitWarehouseStatusChanging += (sender, e) => e.Cancel = true;
			line.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
			Factory.Save();
			line.LastKnownTransitWarehouseStatusForBinding = "DSP";
			AssertEquals(ZString.Empty, line.JL_LastKnownTransitWarehouseStatus);
		}

		#region JL_IsHighRisk

		public void TestJL_IsHighRisk_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USCHI";
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(false, packLine.JL_IsHighRiskInfo.ReadOnly);

				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;
				AssertEquals("Is High Risk should be read-only because Allow to Override Inspection is not granted", true, packLine.JL_IsHighRiskInfo.ReadOnly);
				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
				AssertEquals(false, packLine.JL_IsHighRiskInfo.ReadOnly);

				shipment.JS_RL_NKOrigin = "AUSYD";
				AssertEquals("Is High Risk should be read-only because shipment is not an export", true, packLine.JL_IsHighRiskInfo.ReadOnly);
				shipment.JS_RL_NKOrigin = "FRPAR";
				AssertEquals(false, packLine.JL_IsHighRiskInfo.ReadOnly);

				shipment.JS_TransportMode = "SEA";
				AssertEquals("Is High Risk should be read-only because transport mode is not 'AIR'", true, packLine.JL_IsHighRiskInfo.ReadOnly);
				shipment.JS_TransportMode = "AIR";
				AssertEquals(false, packLine.JL_IsHighRiskInfo.ReadOnly);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "CNSHA";
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(true, packLine.JL_IsHighRiskInfo.ReadOnly);
			}
		}

		public void TestJL_IsHighRisk_ChangeShipmentIsHighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_IsHighRisk = true;
				var packLine = shipment.OuterPackLines.AddNew();
				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
				packLine.JL_IsHighRisk = false;
				AssertEquals("If Packline IsHighRisk is not checked, shipment Is Higk Risk still maintains true", true, shipment.JS_IsHighRisk);

				shipment.JS_IsHighRisk = false;
				AssertEquals(false, shipment.JS_IsHighRisk);
				packLine.JL_IsHighRisk = true;
				AssertEquals("If Packline IsHighRisk is checked, shipment Is High Risk will change from false to true", true, shipment.JS_IsHighRisk);
			}
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		public void TestJL_AdditionalInspectionTypeCode_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USCHI";
				var packLine = shipment.OuterPackLines.AddNew();
				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
				packLine.JL_IsHighRisk = true;
				AssertEquals(false, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);

				packLine.JL_IsHighRisk = false;
				AssertEquals("Packline Additional Inspection should be read-only because pack's Is High Risk is not checked", true, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				packLine.JL_IsHighRisk = true;
				AssertEquals(false, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);

				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;
				AssertEquals("Additional Inspection should be read-only because Allow to Override Inspection is not granted", true, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
				AssertEquals(false, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);

				shipment.JS_RL_NKOrigin = "AUSYD";
				AssertEquals("Additional Inspection should be read-only because shipment is not an export", true, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				shipment.JS_RL_NKOrigin = "FRPAR";
				AssertEquals(false, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);

				shipment.JS_TransportMode = "SEA";
				AssertEquals("Additional Inspection should be read-only because transport mode is not 'AIR'", true, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
				shipment.JS_TransportMode = "AIR";
				AssertEquals(false, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "USCHI";
				shipment.JS_RL_NKDestination = "CNSHA";
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(true, packLine.JL_IsHighRiskInfo.ReadOnly);
				packLine.JL_IsHighRisk = true;
				AssertEquals(true, packLine.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_AdditionalInspectionTypeCode_ChangeShipmentAdditionalInspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_IsHighRisk = true;
				shipment.JS_AdditionalInspectionTypeCode = FreightDataRegistry.AviationSecurity_Unknown_Code;
				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_IsHighRisk = true;
				packLine1.JL_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch;
				Factory.Save();
				AssertEquals("Shipment Additional Inspection Type is updated by PackLine", AdditionalScreeningMethods.Codes.PhysicalInspectionAndHandSearch, shipment.JS_AdditionalInspectionTypeCode);
				AssertEquals(1, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
				var secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);
				AssertNotNull(secEvent);
				AssertEquals("|NEW=PHS|OLD=UNK|RES=Additional Inspection Packlines level updated|TYP=High Risk", secEvent.SL_Reference);

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_IsHighRisk = true;
				packLine2.JL_AdditionalInspectionTypeCode = AdditionalScreeningMethods.Codes.XRayEquipment;
				Factory.Save();
				AssertEquals("Shipemnt Additional Inspection Type is updated by PackLine", BaseJobShipmentLookups.InspectionType_Screened, shipment.JS_AdditionalInspectionTypeCode);
				AssertEquals(2, shipment.Logs.Find(x => x.SL_SE_NKEvent == Events.SecurityModified.Code).Count());
				secEvent = shipment.Logs.MostRecentLogByEventTime(Events.SecurityModified);
				AssertNotNull(secEvent);
				AssertEquals("|NEW=SCR|OLD=PHS|RES=Additional Inspection Packlines level updated|TYP=High Risk", secEvent.SL_Reference);
			}
		}

		#endregion

		#region JL_InspectionTypeCode

		public void TestJL_InspectionTypeCode_Sea()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = "SEA";

			var packLine = shipment.OuterPackLines.AddNew();

			AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
			Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
		}

		public void TestJL_InspectionTypeCode_Import()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_Transhipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				var packLine = shipment.OuterPackLines.AddNew();

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_TransportMode = "AIR";

				var transport1 = consol1.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "AUBNE";
				transport1.JW_RL_NKDiscPort = "HKHKG";
				transport1.JW_TransportMode = "AIR";

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_TransportMode = "AIR";

				var transport2 = consol1.Transports.AddNew();
				transport2.JW_RL_NKLoadPort = "HKHKG";
				transport2.JW_RL_NKDiscPort = "USCHI";
				transport2.JW_TransportMode = "AIR";

				shipment.SetApprovedShipperStatus("");
				AssertEquals("Shipment is TRN", "TRN", shipment.JS_InspectionTypeCode);
				AssertEquals("Packline is TRN", "TRN", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertEquals("Packline is XRY", "XRY", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertEquals("Packline is PHS", "PHS", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "ETD";
				AssertEquals("Packline is ETD", "ETD", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		#region JL_InspectionTypeCode For Australia Transhipment

		public void TestJL_InspectionTypeCode_ForAustraliaTranshipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				SetupTranshipment(shipment);

				var packLine = shipment.OuterPackLines.AddNew();
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_ForAustraliaTranshipment_DefaultInspectionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				SetupTranshipment(shipment);

				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { e.Cancel = false; };
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertEquals("JL_InspectionTypeCode should not be defaulted.", "XRY", packLine.JL_InspectionTypeCode);
			}
		}

		public void TestJL_InspectionTypeCode_ForAustraliaTranshipment_DoNotDefaultInspectionType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				SetupTranshipment(shipment);

				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { e.Cancel = true; };
				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertEquals("JL_InspectionTypeCode should not be changed.", ZString.Empty, packLine.JL_InspectionTypeCode);
			}
		}

		void SetupTranshipment(ForwardingShipment shipment)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "PGPOM";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "PGPOM";
		}

		#endregion

		public void TestJL_InspectionTypeCode_Domestic()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				var packLine = shipment.OuterPackLines.AddNew();

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_TransportMode = "AIR";

				var transport1 = consol1.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "USLAX";
				transport1.JW_RL_NKDiscPort = "USCHI";
				transport1.JW_TransportMode = "AIR";

				shipment.SetApprovedShipperStatus("");
				AssertEquals("Shipment is UNK", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Packline is UNK", "UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertEquals("Packline is PHS", "PHS", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_USDomesticLoggedInAsOtherCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				var packLine = shipment.OuterPackLines.AddNew();

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_TransportMode = "AIR";

				var transport1 = consol1.Transports.AddNew();
				transport1.JW_RL_NKLoadPort = "USLAX";
				transport1.JW_RL_NKDiscPort = "USCHI";
				transport1.JW_TransportMode = "AIR";

				shipment.SetApprovedShipperStatus("");
				AssertEquals("Shipment is UNK", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Packline inspection is not applicable", "", packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertEquals("Packline inspection hasn't changed", "", packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_ReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_TransportMode = "AIR";
			var packLine = shipment.OuterPackLines.AddNew();

			Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;
			Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

			Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;
			Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
		}

		public void TestJL_InspectionTypeCode_ReadOnly_APP()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();
				shipment.JS_InspectionTypeCode = "UNK";
				Factory.Save();

				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				packLine.JL_InspectionTypeCode = "APP";
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var packLineInNewFac = newFactory.Load<ForwardingPackLine>(packLine.PK);
				Assert(packLineInNewFac.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_Disabled()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_MRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_InspectionTypeCode = "XRY";

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_NonMRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_RL_NKDestination = "AUBNE";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_USTranshipment_MRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_TransportMode = "AIR";

				var transport1 = shipment.Transports.AddNew();
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "DEFRA";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = shipment.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "BRSAO";

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_InspectionTypeCode = "XRY";

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_USTranshipment_NonMRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "CNSHA";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "BRSAO";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "BRSAO";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				transport1.JW_RL_NKDiscPort = "SGSIN";
				transport2.JW_RL_NKLoadPort = "SGSIN";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_DestinationNonUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CN"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "CNSHA";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_NoSCSModule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				Assert("Precondition", SupplyChainSecurityConfiguration.New().UsesGenericScheme);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "JMKIN";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_AUExport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();

				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "UNK";
				AssertEquals("UNK", packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_RL_NKOrigin = "NZAKL";
				Assert("Read only if origin is not AU", packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packline1 = shipment.OuterPackLines.AddNew();
				var packline2 = shipment.OuterPackLines.AddNew();

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);
				AssertEquals("Precondition: packline 1", "", packline1.JL_InspectionTypeCode);
				AssertEquals("Precondition: packline 2", "", packline2.JL_InspectionTypeCode);

				Assert("Pack1: not read-only when shipment is UNK", !packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: not read-only when shipment is UNK", !packline2.JL_InspectionTypeCodeInfo.ReadOnly);

				packline1.JL_InspectionTypeCode = "PHS";

				AssertEquals("If one packline has a value entered then blanks are replaced with UNK", "UNK", packline2.JL_InspectionTypeCode);
				AssertEquals("Shipment remains UNK", "UNK", shipment.JS_InspectionTypeCode);

				Assert("Pack1: not read-only when shipment is UNK", !packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: not read-only when shipment is UNK", !packline2.JL_InspectionTypeCodeInfo.ReadOnly);

				packline2.JL_InspectionTypeCode = "PHS";

				AssertEquals("All packlines have same inspection, so default this inspection to shipment", "PHS", shipment.JS_InspectionTypeCode);

				Assert("Pack1: not read-only when shipment is PHS", !packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: not read-only when shipment is PHS", !packline2.JL_InspectionTypeCodeInfo.ReadOnly);

				packline2.JL_InspectionTypeCode = "MAI";

				AssertEquals("Packlines are inspected by different methods. Default SCR to shipment", "SCR", shipment.JS_InspectionTypeCode);

				Assert("Pack1: not read-only when shipment is SCR", !packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: not read-only when shipment is SCR", !packline2.JL_InspectionTypeCodeInfo.ReadOnly);

				packline2.JL_InspectionTypeCode = "UNK";

				AssertEquals("At least one packline is UNK, so shipment defaults UNK", "UNK", shipment.JS_InspectionTypeCode);

				Assert("Pack1: not read-only when shipment is UNK", !packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: not read-only when shipment is UNK", !packline2.JL_InspectionTypeCodeInfo.ReadOnly);

				shipment.JS_InspectionTypeCode = "APP";

				AssertEquals("Pack1: inspection is blank as shipment is approved", "", packline1.JL_InspectionTypeCode);
				AssertEquals("Pack2: inspection is blank as shipment is approved", "", packline2.JL_InspectionTypeCode);

				Assert("Pack1: read-only when shipment is APP", packline1.JL_InspectionTypeCodeInfo.ReadOnly);
				Assert("Pack2: read-only when shipment is APP", packline2.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestDoNotDeleteCusEntryNumberWhenSetPackLineInspectionTypeToDefault()
		{
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UNK"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_InspectionTypeCode = "XYZ";

				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, packline.PK);
				var cusEntryNums = Factory.Load<CusEntryNumber>(query);
				AssertEquals(1, cusEntryNums.Length);

				packline.JL_InspectionTypeCode = "UNK";

				cusEntryNums = Factory.Load<CusEntryNumber>(query);
				AssertEquals(1, cusEntryNums.Length);
			}
		}

		public void TestJL_InspectionTypeCode_DefaultValue()
		{
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PHS"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.HongKong))
			{
				var shipper = Factory.New<OrgHeader>();
				var shipperAddress = shipper.Addresses.AddNew();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				var packLine = shipment.OuterPackLines.AddNew();

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "AUBNE";
				transport.JW_RL_NKDiscPort = "HKHKG";
				transport.JW_TransportMode = Constants.TransportModes.Air;

				AssertEquals("PHS", shipment.JS_InspectionTypeCode);
				AssertEquals("PHS", packLine.JL_InspectionTypeCode);
				AssertEquals(shipment.AviationSecurity.SupplyChainSecurityConfiguration.InspectionTypeDefault, packLine.JL_InspectionTypeCode);
				Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PHS"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var shipper = Factory.New<OrgHeader>();
				var shipperAddress = shipper.Addresses.AddNew();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				AssertEquals("PHS", shipment.JS_InspectionTypeCode);

				var packLine1 = shipment.OuterPackLines.AddNew();
				AssertEquals("PHS",packLine1.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "SCR";
				var packLine2 = shipment.OuterPackLines.AddNew();
				AssertEquals("Shipment Inspection Type is SCR", "PHS", packLine2.JL_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "PHS";
				var packLine3 = shipment.OuterPackLines.AddNew();
				AssertEquals("There's a packline with PHS Inspection Type", "PHS", packLine3.JL_InspectionTypeCode);
			}

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PHS"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Singapore))
			{
				var shipper = Factory.New<OrgHeader>();
				var shipperAddress = shipper.Addresses.AddNew();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "USCHI";
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.ReDefaultPackLineInspectionTypeCodesEventHandler += (s, e) => { };

				var packLine = shipment.OuterPackLines.AddNew();

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports.AddNew();
				transport.JW_RL_NKLoadPort = "SGSIN";
				transport.JW_RL_NKDiscPort = "USCHI";
				transport.JW_TransportMode = "AIR";

				AssertEquals("When login company = SG, we should not default the generic registry value to the Shipment > Inspection field or Packing > Inspection field for SG", FreightDataRegistry.AviationSecurity_Unknown_Code, shipment.JS_InspectionTypeCode);
				AssertEquals("When login company = SG, we should not default the generic registry value to the Shipment > Inspection field or Packing > Inspection field for SG", FreightDataRegistry.AviationSecurity_Unknown_Code, packLine.JL_InspectionTypeCode);
				Assert(packLine.JL_InspectionTypeCodeInfo.ReadOnly);
			}
		}

		public void TestJL_InspectionTypeCode_DefaultValue_ForOriginCountriesOtherThan_AU_HK_JP_DestinationOtherThan_US()
		{
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
				{
					var shipper = Factory.New<OrgHeader>();
					var shipperAddress = shipper.Addresses.AddNew();

					var shipment = Factory.New<ForwardingShipment>();
					shipment.ConsignorDocumentaryAddress.OrganisationPK = shipper.PK;
					shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperAddress.PK;
					shipment.JS_RL_NKOrigin = "CNSHA";
					shipment.JS_RL_NKDestination = "FRPAR";
					shipment.JS_TransportMode = Constants.TransportModes.Air;

					var packLine = shipment.OuterPackLines.AddNew();

					AssertEquals("XRY", shipment.JS_InspectionTypeCode);
					AssertEquals("XRY", packLine.JL_InspectionTypeCode);
					AssertEquals(shipment.AviationSecurity.SupplyChainSecurityConfiguration.InspectionTypeDefault,
						packLine.JL_InspectionTypeCode);
					Assert(!packLine.JL_InspectionTypeCodeInfo.ReadOnly);
				}
			}
		}

		public void TestJL_InspectionTypeCode_ManuallySelectAPP_DoNotUpdateShipmentInspectionTypeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "GBLON";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packline = shipment.OuterPackLines.AddNew();
				shipment.JS_InspectionTypeCode = "UNK";
				Factory.Save();

				packline.JL_InspectionTypeCode = "APP";
				AssertEquals("Shipment inspection type code should remain UNK", "UNK", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestInspectionTypesLookupContainsAPP_EU() => AssertInspectionTypesLookupContainsAPP("DEHAM", true);

		public void TestInspectionTypesLookupContainsAPP_UK() => AssertInspectionTypesLookupContainsAPP("GBLON", true);

		public void TestInspectionTypesLookupContainsAPP_AU() => AssertInspectionTypesLookupContainsAPP("AUSYD", false);

		void AssertInspectionTypesLookupContainsAPP(ZString origin, bool expectedContain)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(origin.Left(2)))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packline = shipment.OuterPackLines.AddNew();
				AssertEquals(expectedContain, packline.InspectionTypes.ContainsCode(FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code));
			}
		}

		[ExpectNoExceptions]
		public void TestInspectionTypesLookup_DoesNotThrowException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packline = shipment.OuterPackLines.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				Assert(packline.IsDeleted);
				AssertEquals(true, packline.InspectionTypes.ContainsCode(FreightDataRegistry.AviationSecurity_Unknown_Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "DEHAM";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_JS = new ZGuid();
				AssertNull(packline.Shipment);
				AssertEquals(true, packline.InspectionTypes.ContainsCode(FreightDataRegistry.AviationSecurity_Unknown_Code));
			}
		}

		#endregion

		#region CanDelete

		public void TestCanDelete()
		{
			var errorMessage = "You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment.";
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "123";

			var substance_Class7 = Factory.New<UNDGSubstance>();
			substance_Class7.DG_UNNO = "234";

			var dataItem = packline.UNDGs.AddNew();
			dataItem.DI_DG = substance.PK;
			dataItem.LinkDefault(substance);

			var dataItem_Class7 = packline.UNDGs.AddNew();
			dataItem_Class7.DI_DG = substance_Class7.PK;
			dataItem_Class7.DI_IMOClass = "7";
			dataItem_Class7.LinkDefault(substance_Class7);

			Factory.Save();

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;

			CombineAssertions("Should NOT be able to delete packline because class 7 substance exists", () =>
			{
				Assert(!packline.CanDelete);
				AssertEquals(errorMessage, packline.ReasonForNotAbleToDelete);
			});

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = true;

			CombineAssertions("Should all be deletable because security right is given", () =>
			{
				Assert(packline.CanDelete);
				AssertNullOrEmpty(packline.ReasonForNotAbleToDelete);
			});
		}

		public void TestCanDelete_NonDatabaseItems()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();

			var substance_Class7 = Factory.New<UNDGSubstance>();
			substance_Class7.DG_UNNO = "234";

			var dataItem_Class7 = packline.UNDGs.AddNew();
			dataItem_Class7.DI_DG = substance_Class7.PK;
			dataItem_Class7.DI_IMOClass = "7";
			dataItem_Class7.LinkDefault(substance_Class7);

			Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed = false;

			Assert("Pre-Condition - Not in database", !packline.IsInDatabase);
			Assert("Should be able to delete even if no security right as it's not saved yet", packline.CanDelete);
		}

		#endregion

		public void TestCloneCopiesUNDGsFromProduct()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "ABC";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "PRODUCT";
			var undg = orgSupplierPart.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);

			var line = Factory.NewWithValidTestData<ForwardingPackLine>();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			var product = line.Products.AddNew();
			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			line.Shipment.ConsigneePK = orgPartRelationOwner.OU_OH;

			product.D2_ProductCode = "PRODUCT";

			AssertEquals(1, line.UNDGs.Count);
			AssertEquals("ABC", line.UNDGs[0].Substance.DG_Code);

			var clone = (ForwardingPackLine)line.Clone();

			AssertEquals(1, clone.UNDGs.Count);
			AssertEquals("ABC", clone.UNDGs[0].Substance.DG_Code);
		}

		public void TestCloneIncludesProducts()
		{
			ForwardingPackLine line = Factory.New<ForwardingPackLine>();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			line.Products.AddNew();
			line.Products.AddNew();

			ForwardingPackLine clonedLine = (ForwardingPackLine)line.Clone();
			AssertEquals(2, clonedLine.Products.Count);
		}

		public void TestCloneCanExcludeProducts()
		{
			ForwardingPackLine line = Factory.New<ForwardingPackLine>();
			line.Products.AddNew();
			line.Products.AddNew();

			BusinessObjectCloneArgs cloneArgs = new BusinessObjectCloneArgs(new[] { ForwardingPackLine.Schema.ProductsCollection });

			ForwardingPackLine clonedLine = (ForwardingPackLine)line.Clone(cloneArgs);
			AssertEquals(0, clonedLine.Products.Count);
		}

		public void TestSynchronisePackLine()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			var container = consol.Containers.AddNew();
			var sync = new TestSynchroniser();
			shipment.PackLineSynchroniser = sync;

			sync.MarkedDirty = false;
			packLine.JL_ActualVolume = 10m;
			AssertEquals("Not related", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_JC = container.PK;
			AssertEquals("Unchanged value", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_JC = ZGuid.NewZGuid();
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_LinePrice = 1000m;
			AssertEquals("Not related", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_PackageCount = 10;
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_PackageCount = 10;
			AssertEquals("Unchanged value", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_Pillaged = 1;
			AssertEquals("Not related", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_F3_NKPackType = "BB";
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_F3_NKPackType = "BB";
			AssertEquals("Unchanged value", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_MarksAndNumbers = "MarksAndNumbers";
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_ActualWeight = 10m;
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_ActualWeight = 10m;
			AssertEquals("Unchanged value", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Related", true, sync.MarkedDirty);

			sync.MarkedDirty = false;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Unchanged value", false, sync.MarkedDirty);

			sync.MarkedDirty = false;
			shipment.OuterPackLines.RemoveAndDelete(packLine);
			AssertEquals("Related", true, sync.MarkedDirty);
		}

		public void TestSuspendReadOnly()
		{
			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_FreightMode = FreightConstants.OuterPackType;

			AssertEquals(false, packLine.ReadOnly);

			var collection1 = new ForwardingPackLineCollection(Factory.New<ForwardingShipment>());
			AssertEquals(false, collection1.ReadOnly);

			collection1.Add(packLine);
			Assert(((IBusinessObjectInternals)packLine).ParentCollections.Contains(collection1));
			AssertEquals(false, packLine.ReadOnly);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment.CoLoadShipments.AddNew();

			var collection2 = new ForwardingPackLineCollection(shipment);
			collection2.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, collection2.ReadOnly);

			collection2.Add(packLine);
			Assert(((IBusinessObjectInternals)packLine).ParentCollections.Contains(collection1));
			Assert(((IBusinessObjectInternals)packLine).ParentCollections.Contains(collection2));
			AssertEquals(true, packLine.ReadOnly);

			collection2.SuspendReadOnly = true;
			AssertEquals(false, packLine.ReadOnly);

			collection2.SuspendReadOnly = false;
			AssertEquals(true, packLine.ReadOnly);
		}

		public void TestDelete_IPortMessagingHelper()
		{
			var portMessagingHelper = new Mock<Integration.Forwarding.IPortMessagingForwardingHelper>(MockBehavior.Strict);

			using (ObjectFactory.Substitute(portMessagingHelper.Object))
			{
				var packLine = Factory.New<ForwardingPackLine>();

				portMessagingHelper.Setup(m => m.OnPacklineDelete(Factory, packLine.PK));
				packLine.Delete();

				Assert("Asserting via mocks", true);
			}
		}

		public void TestRefCommodityCodeTemperatureRangeIsDefaultedToTemperatureRange()
		{
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			var refCommodityCodeBO = Factory.NewWithValidTestData<RefCommodityCode>();
			refCommodityCodeBO.RH_ReeferMinTemperature = -6.0;
			refCommodityCodeBO.RH_ReeferMaxTemperature = 20.0;
			refCommodityCodeBO.RH_Code = "TEST";

			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = -5.0;
			packLine.JL_RequiredTemperatureMaximum = 5.0;
			packLine.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Fahrenheit;

			packLine.JL_RH_NKCommodityCode = refCommodityCodeBO.RH_Code;

			CombineAssertions("the packline should be changed to match the RefCommodityCode", () =>
			{
				AssertEquals(true, packLine.JL_RequiresTemperatureControl);
				AssertEquals((ZDecimal)(-6.0), packLine.JL_RequiredTemperatureMinimum);
				AssertEquals((ZDecimal)20.0, packLine.JL_RequiredTemperatureMaximum);
				AssertEquals(Core.Constants.Temperature.Centigrade, packLine.JL_RequiredTemperatureUnit);
			});
		}

		public void TestJL_RequiresTemperatureControl_DefaultsWhenValuesSet()
		{
			var packline = Factory.New<ForwardingPackLine>();
			AssertEquals("Precondition: Not requires temp control", false, packline.JL_RequiresTemperatureControl);

			packline.JL_RequiredTemperatureMinimum = -10;

			AssertEquals("Value defaulted", true, packline.JL_RequiresTemperatureControl);
		}

		public void TestGeneratePackageWithIDs()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "SSYD0000";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = "SEA";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = "PLT";
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_UnitOfDimension = "M";
			packLine.JL_RefNumber = "attachedShipment";
			packLine.JL_F3_NKPackType = "ROL";
			packLine.JL_Description = "UM";
			packLine.JL_PackageCount = 4;
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 4m;
			packLine.JL_RequiredTemperatureMaximum = 5m;
			packLine.JL_RequiredTemperatureUnit = "C";
			packLine.JL_MarksAndNumbers = "MNS";
			var undg = packLine.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("0001", "RFL").PK;

			var pkg = packLine.GeneratePackageWithIDs();
			AssertNullOrEmpty(pkg.KP_PackageID);

			Factory.Save();
			AssertEquals(shipment.JS_UniqueConsignRef + "-001", pkg.KP_PackageID);
			AssertEquals(packLine.JL_F3_NKPackType, pkg.KP_F3_NKPackType);
			AssertEquals(packLine.JL_UnitOfDimension, pkg.KP_DimensionUQ);
			AssertEquals(packLine.JL_ActualWeightUQ, pkg.KP_WeightUQ);
			AssertEquals(packLine.JL_ActualVolumeUQ, pkg.KP_VolumeUQ);
			AssertEquals(packLine.JL_RequiresTemperatureControl, pkg.KP_RequiresTemperatureControl);
			AssertEquals(packLine.JL_RequiredTemperatureMaximum, pkg.KP_RequiredTemperatureMaximum);
			AssertEquals(packLine.JL_RequiredTemperatureMinimum, pkg.KP_RequiredTemperatureMinimum);
			AssertEquals(packLine.JL_RequiredTemperatureUnit, pkg.KP_RequiredTemperatureUnit);
			AssertEquals(packLine.JL_MarksAndNumbers, pkg.KP_MarksAndNumbers);
			AssertEquals(packLine.JL_Description, pkg.KP_GoodsDescription);
			AssertEquals(packLine.JL_HarmonisedCode, pkg.KP_HSCode);
			AssertEquals(packLine.JL_RH_NKCommodityCode, pkg.KP_RH_NKCommodityCode);
			AssertEquals(packLine.UNDGs.Count, pkg.UNDGs.Count);
			AssertEquals(packLine.UNDGs[0].DI_DG, pkg.UNDGs[0].DI_DG);
		}

		UNDGSubstance CreateUNDGSubstance(string code, string specialHandlingCodes)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = code.Substring(0, 4);
			substance.DG_Code = code;
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			substance.DG_SpecialHandlingCodes = specialHandlingCodes;
			return substance;
		}

		public void TestLooseCargoContainerType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();

				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "HKHKG";
				shipment.JS_TransportMode = "AIR";

				var packLine = shipment.OuterPackLines.AddNew();
				AssertEquals("Loose Cargo Container Type will be empty when JL_RC_ContainerType has not been set.", ZString.Empty, packLine.LooseCargoContainerType);

				packLine.JL_RC_ContainerType = ZGuid.NewZGuid(); // Some Invalid Contianer Type
				AssertEquals("Loose Cargo Container Type will be empty when JL_RC_ContainerType is not valid.", ZString.Empty, packLine.LooseCargoContainerType);

				packLine.JL_RC_ContainerType = RC_20GP_PK;
				AssertEquals("Loose Cargo Container Type will be RefContainer Code of the RefContainer that it's PK is assigned to JL_RC_ContainerType.", "20GP", packLine.LooseCargoContainerType);

				packLine.LooseCargoContainerType = "40GP";
				AssertEquals("JL_RC_ContainerType will be RefContainer Code of the RefContainer that it's Code is assigned to LooseCargoContainerType.", RC_40GP_PK, packLine.JL_RC_ContainerType);

				packLine.LooseCargoContainerType = "XXXY";
				AssertEquals("JL_RC_ContainerType will be empty when setting LooseCargoContainerType to an invalid container type.", ZGuid.Empty, packLine.JL_RC_ContainerType);
			}
		}

		#region QValue

		public void TestQvalue()
		{
			var shipment = CreateShipment();
			var packline = shipment.OuterPackLines.Cast<ForwardingPackLine>().Single();
			packline.UNDGs.Single().DI_IsLimitedQuantity = true;

			var substance1 = CreateUNDGSubstance("A01", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);
			var substance2 = CreateUNDGSubstance("A02", LimitedQuantityTypes.NLMCode, 40m, Constants.Weight.Kilograms);
			var substance3 = CreateUNDGSubstance("A03", LimitedQuantityTypes.NLMCode, 50m, Constants.Volume.Litre);
			var substance4 = CreateUNDGSubstance("A04", LimitedQuantityTypes.FOBCode, 50m, Constants.Volume.Litre);
			var substance5 = CreateUNDGSubstance("A05", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms, UNDGSubstanceStandardTypes.CFR);
			var un1845 = CreateUNDGSubstance("1845", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);
			var un2807 = CreateUNDGSubstance("2807", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);
			var un3164 = CreateUNDGSubstance("3164", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);
			var un3245 = CreateUNDGSubstance("3245", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);
			var un1003 = CreateUNDGSubstanceOfLI("1003", LimitedQuantityTypes.GLMCode, 30m, Constants.Weight.Kilograms);

			CreateUNDGDataItem(packline, substance1, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, substance2, weight: 10m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, substance3, volume: 5m, unitOfVolume: Constants.Volume.Litre);
			CreateUNDGDataItem(packline, substance4, volume: 10m, unitOfVolume: Constants.Volume.Litre);
			CreateUNDGDataItem(packline, substance5, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, un1845, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, un2807, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, un3164, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, un3245, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);
			CreateUNDGDataItem(packline, un1003, weight: 5m, unitOfWeight: Constants.Weight.Kilograms);

			AssertEquals("Q-Value has been correctly calculated", 0.7m, packline.QValue);
		}

		UNDGSubstance CreateUNDGSubstance(string unno, string maxAmtType, decimal maxAmt, string maxAmtUQ, string standard = UNDGSubstanceStandardTypes.IATA)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = standard;
			substance.DG_UNNO = unno;
			substance.DG_LQMaxAmtType = maxAmtType;
			substance.DG_LQMaxAmt = maxAmt;
			substance.DG_LQMaxAmtUQ = maxAmtUQ;
			return substance;
		}

		UNDGSubstance CreateUNDGSubstanceOfLI(string unno, string maxAmtType, decimal maxAmt, string maxAmtUQ, string standard = UNDGSubstanceStandardTypes.IATA)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = standard;
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_UNNO = unno;
			substance.DG_Code = unno;
			substance.DG_CargoMaxAmt = 500;
			substance.DG_CargoMaxAmtUQ = "T";
			substance.DG_LQMaxAmtType = maxAmtType;
			substance.DG_LQMaxAmt = maxAmt;
			substance.DG_LQMaxAmtUQ = maxAmtUQ;

			return substance;
		}

		UNDGDataItem CreateUNDGDataItem(ForwardingPackLine packLine, UNDGSubstance substance, decimal weight = 0m, string unitOfWeight = "", decimal volume = 0m, string unitOfVolume = "")
		{
			var undgDataItem = packLine.UNDGs.AddNew();
			undgDataItem.DI_PackageCount = 5;
			undgDataItem.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.DI_DGWeight = weight;
			undgDataItem.DI_UnitOfWeight = unitOfWeight;
			undgDataItem.DI_DGVolume = volume;
			undgDataItem.DI_UnitOfVolume = unitOfVolume;

			return undgDataItem;
		}

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "HOUSEBILL001";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "MR Consignee";
			consignee.OH_RL_NKClosestPort = "SGSIN";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "MYKUL";
			departureConsol.JK_BookingReference = "BKG001";
			departureConsol.JK_MasterBillNum = "081-0000001";

			var arrivalConsol = shipment.Consols.AddNew();
			arrivalConsol.JK_UniqueConsignRef = "CONSOL0002";
			arrivalConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			arrivalConsol.JK_RL_NKLoadPort = "MYKUL";
			arrivalConsol.JK_RL_NKDischargePort = "SGSIN";
			arrivalConsol.JK_BookingReference = "BKG002";
			arrivalConsol.JK_MasterBillNum = "001-0000001";

			var container = departureConsol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 1;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packline1.JL_ActualWeight = 2000;
			packline1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packline1.JL_ActualVolume = 1.3;
			packline1.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			container.PackLines.Add(packline1);

			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Pumpernickel";
			contact.OC_Phone = "8000 1234";
			contact.OC_OH = shipper.PK;

			var undg = packline1.UNDGs.AddNew();
			undg.DI_DG = CreateUNDGSubstance("0143", UNDGSubstanceStandardTypes.IATA).PK;
			undg.DI_OC_DGContact = contact.PK;
			undg.DI_DGVolume = 1;
			undg.DI_UnitOfVolume = Core.Constants.Volume.CubicDecimetres;
			undg.DI_PackageCount = 1;

			Factory.Save();

			return shipment;
		}

		#endregion

		#region SupplierBookingLine_List

		public void TestSupplierBookingLine_List()
		{
			JobSupplierBookingLine CreateBookingLine(string bookingLineId, string loadMode, string bookingStatus)
			{
				var order = Factory.NewWithValidTestData<Order>();
				var orderLine = order.OrderLines.AddNew();

				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				booking.JSB_Status = bookingStatus;
				booking.JSB_LoadMode = loadMode;

				var bookingLine = booking.SupplierBookingLines.AddNew();
				bookingLine.JSL_BookingLineId = bookingLineId;
				bookingLine.JSL_JO_OrderLine = orderLine.PK;
				return bookingLine;
			}

			foreach (var loadMode in new string[] { Core.Constants.SupplierBookingLoadMode.ContainerYard, Core.Constants.SupplierBookingLoadMode.ContainerFreightStation, Core.Constants.SupplierBookingLoadMode.LooseCargo })
			{
				CreateBookingLine(loadMode + "JSL001", loadMode, SupplierBookingStatus.Approved);
				CreateBookingLine(loadMode + "JSL002", loadMode, SupplierBookingStatus.Cancelled);
				CreateBookingLine(loadMode + "JSL003", loadMode, SupplierBookingStatus.Converted);
				CreateBookingLine(loadMode + "JSL004", loadMode, SupplierBookingStatus.Incomplete);
				CreateBookingLine(loadMode + "JSL005", loadMode, SupplierBookingStatus.Placed);
				CreateBookingLine(loadMode + "JSL006", loadMode, SupplierBookingStatus.Planned);
				CreateBookingLine(loadMode + "JSL007", loadMode, SupplierBookingStatus.Received);
				CreateBookingLine(loadMode + "JSL008", loadMode, SupplierBookingStatus.Rejected);
				CreateBookingLine(loadMode + "JSL009", loadMode, SupplierBookingStatus.Shipped);
			}

			Factory.Save();

			var packLine = Factory.New<ForwardingShipment>().OuterPackLines.AddNew();
			packLine.JL_JSL_BookingLine = Factory.LoadTop1<JobSupplierBookingLine>(new ZQuery(JobSupplierBookingLineSchema.JSL_BookingLineId, "CYJSL009")).PK;
			var loadPlanLine = Factory.NewWithValidTestData<CFSContainerLoadList>().LoadListLines.AddNew();
			loadPlanLine.CLL_JL_PackLine = packLine.PK;
			var bookingLines = packLine.SupplierBookingLine_List.OfType<JobSupplierBookingLine>().OrderBy(line => line.JSL_BookingLineId).ToArray();

			CombineAssertions("CAN,INC,REJ,PLC should be ignored", () =>
			{
				AssertEquals(5, bookingLines.Length);
				AssertEquals("CYJSL001", bookingLines[0].JSL_BookingLineId);
				AssertEquals("CYJSL003", bookingLines[1].JSL_BookingLineId);
				AssertEquals("CYJSL006", bookingLines[2].JSL_BookingLineId);
				AssertEquals("CYJSL007", bookingLines[3].JSL_BookingLineId);
				AssertEquals("CYJSL009", bookingLines[4].JSL_BookingLineId);
			});
		}

		#endregion

		#region JobSupplierBookingLine

		public void TestJL_JSL_BookingLine_ReadOnly()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBooking>().SupplierBookingLines.AddNew();
			var packLine = Factory.NewWithValidTestData<ForwardingShipment>().OuterPackLines.AddNew();

			AssertEquals("should be readonly if pack line is not from SPT", true, packLine.JL_JSL_BookingLine_ReadOnly);

			packLine.JL_JSL_BookingLine = bookingLine.PK;

			AssertEquals("should be readonly if pack line is from SPT but not converted", true, packLine.JL_JSL_BookingLine_ReadOnly);
			AssertEquals(bookingLine.PK, packLine.JobSupplierBookingLine.PK);

			var loadPlanLine = Factory.NewWithValidTestData<CFSContainerLoadList>().LoadListLines.AddNew();
			loadPlanLine.CLL_JL_PackLine = packLine.PK;
			AssertEquals("should not be readonly if pack line is from SPT but converted", false, packLine.JL_JSL_BookingLine_ReadOnly);
			AssertEquals(bookingLine.PK, packLine.JobSupplierBookingLine.PK);
		}

		#endregion

		#region Container Load List Line

		public void TestLoadListLine()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderline = order.OrderLines.AddNew();
			orderline.FillWithValidTestData();

			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var bookingLine = supplierBooking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JO_OrderLine = orderline.PK;

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();

			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine.CLL_JC_Container = container.PK;
			loadListLine.CLL_JL_PackLine = packline.PK;
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;

			Factory.Save();

			AssertEquals(packline.LoadListLine, loadListLine);
		}

		public void TestIsPackedForOrderPlanning()
		{
			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			loadListLine.CLL_JSL_BookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>().PK;
			Factory.Save();

			Assert(!packLine.IsPackedForOrderPlanning);

			loadListLine.CLL_JL_PackLine = packLine.PK;
			Factory.Save();

			Assert(packLine.IsPackedForOrderPlanning);
		}

		#endregion

		public void TestCloneExcludesProperties()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine = order.OrderLines.AddNew();
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var supplierBookingLine = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine.JSL_JO_OrderLine = orderLine.PK;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var innerpackline = Factory.New<ForwardingPackLine>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			innerpackline.JL_JS = shipment.PK;
			innerpackline.JL_OriginTransitWarehouseStatus = "DIS";
			innerpackline.JL_LastKnownTransitWarehouseStatus = "RCV";
			innerpackline.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(-2);
			innerpackline.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = "DIS";
			packLine.JL_JL_OuterPackLine = innerpackline.PK;
			packLine.JL_LastKnownTransitWarehouseStatus = "RCV";
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now;
			packLine.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;
			packLine.JL_JSL_BookingLine = supplierBookingLine.PK;

			Factory.Save();

			var clonedLine = packLine.Clone() as PackLine;
			CombineAssertions("lines were excluded", () =>
			{
				AssertEquals("JL_OriginTransitWarehouseStatus", "UNK", clonedLine.JL_OriginTransitWarehouseStatus);
				AssertEquals("JL_LastKnownTransitWarehouseStatus", string.Empty, clonedLine.JL_LastKnownTransitWarehouseStatus);
				AssertEquals("JL_LastKnownTransitWarehouseStatusDateTime", ZDateTime.Empty, clonedLine.JL_LastKnownTransitWarehouseStatusDateTime);
				AssertEquals("JL_OA_LastKnownTransitWarehouseAddress", ZGuid.Empty, clonedLine.JL_OA_LastKnownTransitWarehouseAddress);
				AssertEquals("JL_JL_OuterPackLine", ZGuid.Empty, clonedLine.JL_JL_OuterPackLine);
				AssertEquals("JL_JSL_BookingLine", ZGuid.Empty, clonedLine.JL_JSL_BookingLine);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = HomePort.SubstringSafe(0, 2);
			}
		}

		#endregion
	}
}
