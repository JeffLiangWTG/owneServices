using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AllocationAdjustment))]
	sealed class AllocationAdjustmentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AllocationAdjustment(null));
			AssertNoExceptionThrown(() => new AllocationAdjustment(Factory.New<ForwardingConsol>()));
		}

		public void TestProperties()
		{
			SetupDefaultConsolPreAllocationCheckRestrictions();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Hello";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_TotalShipmentActWeightCheck = 10m;
			consol.JK_TotalShipmentActVolumeCheck = 20m;
			consol.JK_TotalShipmentChargableCheck = 30m;
			consol.JK_TotalShipmentCountCheck = 4;
			consol.WeightVerificationUnit = Constants.Weight.Tonnes;
			consol.VolumeVerificationUnit = Constants.Volume.CubicFeet;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 100m;
			shipment.JS_ActualVolume = 200m;
			shipment.JS_ActualChargeable = 300m;

			AllocationAdjustment adjustment = new AllocationAdjustment(consol);
			AssertEquals("Hello", adjustment.UniqueConsignRef);
			AssertEquals(100m, adjustment.Weight);
			AssertEquals("KG", adjustment.WeightUnit);
			AssertEquals(200m, adjustment.Volume);
			AssertEquals("M3", adjustment.VolumeUnit);
			AssertEquals(300m, adjustment.Chargeable);
			AssertEquals("KG", adjustment.ChargeableUnit);
			AssertEquals(1, adjustment.ShipmentCount);

			AssertEquals(10m, adjustment.AllocatedWeight);
			AssertEquals(Constants.Weight.Tonnes, adjustment.AllocatedWeightUnit);
			AssertEquals(20m, adjustment.AllocatedVolume);
			AssertEquals(Constants.Volume.CubicFeet, adjustment.AllocatedVolumeUnit);
			AssertEquals(30m, adjustment.AllocatedChargeable);
			AssertEquals(Constants.Weight.Tonnes, adjustment.AllocatedChargeableUnit);
			AssertEquals((ZShort)4, adjustment.AllocatedShipmentCount);

			AssertEquals(10m, adjustment.NewAllocatedWeight);
			AssertEquals(20m, adjustment.NewAllocatedVolume);
			AssertEquals(30m, adjustment.NewAllocatedChargeable);
			AssertEquals((ZShort)4, adjustment.NewAllocatedShipmentCount);
		}

		public void TestShipmentCount_AWBMaster()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_AgentType = Constants.AgentType.AWBMaster;

			var coloadConsol1 = masterConsol.ColoadConsols.AddNew();
			coloadConsol1.JK_AgentType = Constants.AgentType.AWBCoload;
			coloadConsol1.Shipments.AddNew();

			var coloadConsol2 = masterConsol.ColoadConsols.AddNew();
			coloadConsol2.JK_AgentType = Constants.AgentType.AWBCoload;
			coloadConsol2.Shipments.AddNew();

			var adjustment = new AllocationAdjustment(masterConsol);

			AssertEquals("Master consol should not contain any shipment directly.", 0, masterConsol.Shipments.Count);
			AssertEquals("ShipmentCount should be total shipment number of all co-load consols in master consol.", 2, adjustment.ShipmentCount);
		}

		public void TestAdjust()
		{
			SetupConsolPreAllocationCheckRestrictions(10m, 20m, 40m, 45m);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100m;
			consol.JK_TotalShipmentActVolumeCheck = 20m;
			consol.JK_TotalShipmentChargableCheck = 300m;
			consol.JK_TotalShipmentCountCheck = 1;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 100m;
			shipment.JS_ActualVolume = 20m;
			shipment.JS_ActualChargeable = 300m;

			AllocationAdjustment adjustment = new AllocationAdjustment(consol);
			AssertEquals("Current allocation", 100m, adjustment.AllocatedWeight);
			AssertEquals("Current allocation", 20m, adjustment.AllocatedVolume);
			AssertEquals("Current allocation", 300m, adjustment.AllocatedChargeable);
			AssertEquals("Current allocation", (ZShort)1, adjustment.AllocatedShipmentCount);

			AssertEquals("New allocation", 1000m, adjustment.NewAllocatedWeight);
			AssertEquals("New allocation", 100m, adjustment.NewAllocatedVolume);
			AssertEquals("New allocation", 750m, adjustment.NewAllocatedChargeable);
			AssertEquals("New allocation", (ZShort)3, adjustment.NewAllocatedShipmentCount);

			ZQuery logsQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "Pre-Allocated");
			StmALog[] adjustmentLogs = consol.Logs.Find(logsQuery);
			AssertEquals("Precondition: no pre-allocation changed logs", 0, adjustmentLogs.Length);

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Frodo Baggins";
			staff.GS_Code = "FB";

			adjustment.Adjust(staff);
			AssertEquals("Allocation adjusted", 1000m, adjustment.AllocatedWeight);
			AssertEquals("Allocation adjusted", 100m, adjustment.AllocatedVolume);
			AssertEquals("Allocation adjusted", 750m, adjustment.AllocatedChargeable);
			AssertEquals("Allocation adjusted", (ZShort)3, adjustment.AllocatedShipmentCount);

			adjustmentLogs = consol.Logs.Find(logsQuery);
			AssertEquals("Adjustment logs created", 4, adjustmentLogs.Length);
			AssertContainsExactElementsInAnyOrder("Correct staff on logs", new ZString[] { "FB" }, adjustmentLogs.Select(log => log.SL_GS_NKUser).Distinct());
			AssertEquals(true, adjustmentLogs.Any(log => log.SL_Reference == "Pre-Allocated weight changed: 100.000 => 1000.000"));
			AssertEquals(true, adjustmentLogs.Any(log => log.SL_Reference == "Pre-Allocated volume changed: 20.000 => 100.000"));
			AssertEquals(true, adjustmentLogs.Any(log => log.SL_Reference == "Pre-Allocated chargeable changed: 300.000 => 750.000"));
			AssertEquals(true, adjustmentLogs.Any(log => log.SL_Reference == "Pre-Allocated no. of shipments changed: 1 => 3"));
		}

		public void TestPropertiesValidation()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "Hello";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_TotalShipmentActWeightCheck = 1234567890m;
			consol.JK_TotalShipmentActVolumeCheck = 1234567890m;
			consol.JK_TotalShipmentChargableCheck = 1234567890m;
			consol.JK_TotalShipmentCountCheck = 9999;
			consol.WeightVerificationUnit = Constants.Weight.Tonnes;
			consol.VolumeVerificationUnit = Constants.Volume.CubicFeet;

			ForwardingShipment shipment = consol.Shipments.AddNew();

			AllocationAdjustment adjustment = new AllocationAdjustment(consol);
			Assert(adjustment.NewAllocatedWeightInfo.HasErrors());
			Assert(adjustment.NewAllocatedVolumeInfo.HasErrors());
			Assert(adjustment.NewAllocatedChargeableInfo.HasErrors());
			Assert(adjustment.NewAllocatedVolumeInfo.HasErrors());
		}

		public void TestUnitsCorrectlyConvertedForNewAllocatedValues()
		{
			SetupConsolPreAllocationCheckRestrictions(100m, 100m, 100m, 100m);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			consol.JK_TotalShipmentActWeightCheck = 69m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;

			consol.JK_TotalShipmentActVolumeCheck = 69m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			consol.JK_TotalShipmentChargableCheck = 69m;
			consol.JK_TotalShipmentChargeableUnit = Constants.Weight.Kilograms;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			shipment.JS_ActualWeight = 420m;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;

			shipment.JS_ActualVolume = 420m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicYards;

			shipment.JS_ActualChargeable = 420m;
			AssertEquals("Precondition", Constants.Weight.Pounds, shipment.JS_ChargeableUnit);

			var adjustment = new AllocationAdjustment(consol);

			var expectedNewAllocatedWeight = Constants.Weight.Convert(adjustment.Weight, adjustment.WeightUnit, consol.WeightVerificationUnit, false);
			var expectedNewAllocatedVolume = Constants.Volume.Convert(adjustment.Volume, adjustment.VolumeUnit, consol.VolumeVerificationUnit, false);
			var expectedNewAllocatedChargeable = Constants.Weight.Convert(adjustment.Chargeable, adjustment.ChargeableUnit, consol.JK_TotalShipmentChargeableUnit, false);

			AssertEquals("Registry restriction set to 100%, so New Allocated Weight should now equal total weight of consol in correct units.", expectedNewAllocatedWeight, adjustment.NewAllocatedWeight);
			AssertEquals("Registry restriction set to 100%, so New Allocated Volume should now equal total volume of consol in correct units.", expectedNewAllocatedVolume, adjustment.NewAllocatedVolume);
			AssertEquals("Registry restriction set to 100%, so New Allocated Chargeable should now equal total chargeable of consol in correct units.", expectedNewAllocatedChargeable, adjustment.NewAllocatedChargeable);
		}

		// this one may or may not be necessary, can probably remove it
		public void TestCorrectUnitsForWeightVolumeAndChargeable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 69m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;
			consol.JK_TotalShipmentActVolumeCheck = 69m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 420m;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualVolume = 420m;
			shipment.JS_UnitOfVolume = Constants.Volume.ImperialGallons;

			var adjustment = new AllocationAdjustment(consol);
			AssertEquals(consol.WeightVerificationUnit, adjustment.AllocatedWeightUnit);
			//AssertEquals(consol.WeightVerificationUnit, adjustment.NewAllocatedWeightUnit);
			AssertEquals(consol.VolumeVerificationUnit, adjustment.AllocatedVolumeUnit);
			//AssertEquals(consol.VolumeVerificationUnit, adjustment.NewAllocatedVolumeUnit);

			// if end up keeping this test then add assertion here for chargeable
		}

		public void TestCorrectDecimalPointsForWeightVolumeAndChargeable()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_TotalShipmentActWeightCheck = 69.420m;
			consol.WeightVerificationUnit = Constants.Weight.Kilograms;
			consol.JK_TotalShipmentActVolumeCheck = 69.420m;
			consol.VolumeVerificationUnit = Constants.Volume.CubicMetres;
			consol.JK_TotalShipmentChargableCheck = 69.420m;
			consol.JK_TotalShipmentChargeableUnit = Constants.Weight.Kilograms;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 420.69m;
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualVolume = 420.69m;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicYards;
			shipment.JS_ActualChargeable = 420m;
			AssertEquals("Precondition", Constants.Weight.Pounds, shipment.JS_ChargeableUnit);

			var adjustment = new AllocationAdjustment(consol);

			var consolWeightCheckDecimalPlaces = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(consol, consol.JK_TotalShipmentActWeightCheckInfo);
			var consolVolumeCheckDecimalPlaces = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(consol, consol.JK_TotalShipmentActVolumeCheckInfo);
			var consolChargeableCheckDecimalPlaces = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(consol, consol.JK_TotalShipmentChargableCheckInfo);

			AssertEquals(consolWeightCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.AllocatedWeightInfo));
			AssertEquals(consolWeightCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.NewAllocatedWeightInfo));

			AssertEquals(consolVolumeCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.AllocatedVolumeInfo));
			AssertEquals(consolVolumeCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.NewAllocatedVolumeInfo));

			AssertEquals(consolChargeableCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.AllocatedChargeableInfo));
			AssertEquals(consolChargeableCheckDecimalPlaces, DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(adjustment, adjustment.NewAllocatedChargeableInfo));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocationAdjustment(Factory.New<ForwardingConsol>());
		}

		void SetupDefaultConsolPreAllocationCheckRestrictions()
		{
			var defaultValue = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.DefaultValue;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultValue);
		}

		void SetupConsolPreAllocationCheckRestrictions(ZDecimal weightPercentage, ZDecimal volumePercentage, ZDecimal chargeablePercentage, ZDecimal shipmentCountPercentage)
		{
			PreAllocationCheckCollection allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			allocationChecks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Weight.Percentage = weightPercentage;

			allocationChecks.Volume.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Volume.Percentage = volumePercentage;

			allocationChecks.Chargeable.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Chargeable.Percentage = chargeablePercentage;

			allocationChecks.ShipmentCount.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.ShipmentCount.Percentage = shipmentCountPercentage;

			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocationChecks);
		}

		#endregion
	}
}
