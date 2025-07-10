using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolDefaultNumberOfDecimalsSupporterTest : CommonConsolDefaultNumberOfDecimalsSupporterTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;

			consol.JK_ConsolChargeable = 156.158m;
			consol.JK_CorrectedConsolWeight = 162.264m;
			consol.JK_CorrectedConsolVolume = 5.181m;

			consol.JK_TotalShipmentActVolumeCheck = 5.126m;
			consol.JK_TotalShipmentActWeightCheck = 126.136m;
			consol.JK_TotalShipmentChargableCheck = 135.624m;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			shipment.JS_ActualWeight = 123.522m;
			shipment.JS_DocumentedWeight = 150.526m;
			shipment.JS_ManifestedWeight = 235.123m;

			shipment.JS_ActualVolume = 5.549m;
			shipment.JS_DocumentedVolume = 4.258m;
			shipment.JS_ManifestedVolume = 3.126m;

			AssertEquals(123.522m, consol.JK_TotalShipmentWeight);
			AssertEquals(150.526m, consol.JK_TotalDocumentedWeight);
			AssertEquals(235.123m, consol.JK_TotalManifestedWeight);

			AssertEquals(5.549m, consol.JK_TotalShipmentVolume);
			AssertEquals(4.258m, consol.JK_TotalDocumentedVolume);
			AssertEquals(3.126m, consol.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment weight for AIR transport mode.", 924.833m, consol.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment weight for AIR transport mode.", 863.500m, consol.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment weight for AIR transport mode.", 709.667m, consol.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment weight for AIR transport mode.", 521.000m, consol.JK_TotalManifestedChargeable);

			AssertEquals(5.181m, consol.JK_CorrectedConsolVolume);
			AssertEquals(162.264m, consol.JK_CorrectedConsolWeight);

			AssertEquals(5.126m, consol.JK_TotalShipmentActVolumeCheck);
			AssertEquals(126.136m, consol.JK_TotalShipmentActWeightCheck);
			AssertEquals(135.624m, consol.JK_TotalShipmentChargableCheck);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(123.53m, consol.JK_TotalShipmentWeight);
			AssertEquals(150.53m, consol.JK_TotalDocumentedWeight);
			AssertEquals(235.13m, consol.JK_TotalManifestedWeight);

			AssertEquals(5.54m, consol.JK_TotalShipmentVolume);
			AssertEquals(4.25m, consol.JK_TotalDocumentedVolume);
			AssertEquals(3.12m, consol.JK_TotalManifestedVolume);

			AssertEquals("JK_TotalShipmentChargeable is made up of shipment volume for SEA transport mode.", 5.54m, consol.JK_TotalShipmentChargeable);

			AssertEquals("JK_ConsolChargeable is made up of shipment volume for SEA transport mode.", 5.18m, consol.JK_ConsolChargeable);
			AssertEquals("JK_TotalDocumentedChargeable is made up of shipment volume for SEA transport mode.", 4.25m, consol.JK_TotalDocumentedChargeable);
			AssertEquals("JK_TotalManifestedChargeable is made up of shipment volume for SEA transport mode.", 3.12m, consol.JK_TotalManifestedChargeable);

			AssertEquals(5.18m, consol.JK_CorrectedConsolVolume);
			AssertEquals(162.27m, consol.JK_CorrectedConsolWeight);

			AssertEquals(5.12m, consol.JK_TotalShipmentActVolumeCheck);
			AssertEquals(126.14m, consol.JK_TotalShipmentActWeightCheck);
			AssertEquals(135.63m, consol.JK_TotalShipmentChargableCheck);
		}

		#region ForwarderAddressWithContact

		public void TestSendingForwarderWithContact_GetDefaultAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsForwarder = true;
			var addr1 = orgHeader.Addresses.AddNew("Code1", "Addr1");
			var addr2 = orgHeader.Addresses.AddNew("Code2", "Addr2");
			var addr1Contact2 = orgHeader.Contacts.AddNew();
			addr1Contact2.WorkingAddressPK = addr2.PK;
			var hanAgent = orgHeader.AppointedAgentPorts.AddNew();
			hanAgent.O5_PortOrCountry = "CN";
			hanAgent.O5_OA_AgentOfficeAddress = addr2.PK;
			hanAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			hanAgent.O5_SeaAgentStatus = "HAN";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RL_NKLoadPort = "CNSHA";
			AssertNull("default sending agent address should be null", consol.SendingForwarderAddress);
			consol.SendingForwarderWithContact.OrgPK = orgHeader.PK;
			AssertEquals("default sending agent should use right address", addr2.PK, consol.SendingForwarderAddress.PK);
		}

		public void TestReceivingForwarderWithContact_GetDefaultAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsForwarder = true;
			var addr1 = orgHeader.Addresses.AddNew("Code1", "Addr1");
			var addr2 = orgHeader.Addresses.AddNew("Code2", "Addr2");
			var addr1Contact2 = orgHeader.Contacts.AddNew();
			addr1Contact2.WorkingAddressPK = addr2.PK;
			var hanAgent = orgHeader.AppointedAgentPorts.AddNew();
			hanAgent.O5_PortOrCountry = "CN";
			hanAgent.O5_OA_AgentOfficeAddress = addr2.PK;
			hanAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			hanAgent.O5_SeaAgentStatus = "HAN";

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			consol.JK_RL_NKDischargePort = "CNSHA";
			AssertNull("default receiving agent address should be null", consol.ReceivingForwarderAddress);
			consol.ReceivingForwarderWithContact.OrgPK = orgHeader.PK;
			AssertEquals("default receiving agent should use right address", addr2.PK, consol.ReceivingForwarderAddress.PK);
		}

		#endregion

		#region Custom Fields Test

		[TestedType(typeof(ForwardingConsol))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			consol.WeightVerificationUnit = Core.Constants.Weight.Kilograms;
			consol.VolumeVerificationUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentChargeableUnit = Core.Constants.Weight.Kilograms;
		}

		public override BusinessObject BizObj
		{
			get { return consol; }
		}
		ForwardingConsol consol;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = base.MeasurePropertiesAndUnits;
					measurePropertiesAndUnits.Add(ForwardingConsol.Schema.JK_TotalShipmentActVolumeCheck, ForwardingConsol.Schema.VolumeVerificationUnit);
					measurePropertiesAndUnits.Add(ForwardingConsol.Schema.JK_TotalShipmentActWeightCheck, ForwardingConsol.Schema.WeightVerificationUnit);
					measurePropertiesAndUnits.Add(ForwardingConsol.Schema.JK_TotalShipmentChargableCheck, ForwardingConsol.Schema.JK_TotalShipmentChargeableUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		public override List<ZString> PropertiesWithExternalUnitsToExcludeFromTesting
		{
			get
			{
				if (propertiesWithExternalUnitsToExcludeFromTesting == null)
				{
					propertiesWithExternalUnitsToExcludeFromTesting = new List<ZString>();
					propertiesWithExternalUnitsToExcludeFromTesting.Add(consol.JK_Calc_ConsolidatedFreightCostChargeableInfo.Name);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(consol.JK_Calc_ShipmentFreightCostChargeableInfo.Name);
				}

				return propertiesWithExternalUnitsToExcludeFromTesting;
			}
		}
		List<ZString> propertiesWithExternalUnitsToExcludeFromTesting;

		#endregion

	}
}
