using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ConsolDashboardCalculationWrapper))]
	sealed class ConsolDashboardCalculationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdateCalculationForRefreshData()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_TotalShipmentCountCheck = 3;
			consol.JK_TotalShipmentActWeightCheck = 6;
			consol.JK_TotalShipmentActVolumeCheck = 300;
			consol.JK_TotalShipmentChargableCheck = 350;
			consol.WeightVerificationUnit = "KG";
			consol.VolumeVerificationUnit = "M3";
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "CTN";
			packLine.JL_ActualWeight = 3;
			packLine.JL_ActualWeightUQ = "KG";
			packLine.JL_Width = 4;
			packLine.JL_Length = 5;
			packLine.JL_Height = 6;
			packLine.JL_UnitOfDimension = "M";
			shipment.UpdateShipmentFromOuterPackLines();
			shipment.JS_ActualChargeable = 360;
			Factory.Save();
			consol.CalculationWrapper.MarkAsBound();
			AssertEquals(240m, consol.CalculationWrapper.JK_CorrectedConsolVolume);

			var otherFactory = new BusinessObjectFactory();
			var otherShipment = otherFactory.Load<ForwardingShipment>(shipment.PK);
			otherShipment.OuterPackLines[0].JL_Width = 5;
			otherShipment.OuterPackLines[0].JL_Length = 6;
			otherShipment.OuterPackLines[0].JL_Height = 7;
			otherShipment.OuterPackLines[0].JL_UnitOfDimension = "M";
			otherShipment.UpdateShipmentFromOuterPackLines();
			otherFactory.Save();
			AssertEquals(420m, consol.CalculationWrapper.JK_CorrectedConsolVolume);
		}

		public void TestUpdateCalculation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_TotalShipmentCountCheck = 3;
			consol.JK_TotalShipmentActWeightCheck = 6;
			consol.JK_TotalShipmentActVolumeCheck = 300;
			consol.JK_TotalShipmentChargableCheck = 350;
			consol.WeightVerificationUnit = "KG";
			consol.VolumeVerificationUnit = "M3";
			consol.CalculationWrapper.MarkAsBound();

			AssertEmpty(consol);

			using (consol.CalculationWrapper.SuspendRefreshAllValues())
			using (consol.Density.SuspendRefreshAllValues())
			{
				var shipment = consol.Shipments.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 2;
				packLine.JL_F3_NKPackType = "CTN";
				packLine.JL_ActualWeight = 3;
				packLine.JL_ActualWeightUQ = "KG";
				packLine.JL_Width = 4;
				packLine.JL_Length = 5;
				packLine.JL_Height = 6;
				packLine.JL_UnitOfDimension = "M";
				shipment.UpdateShipmentFromOuterPackLines();
				shipment.JS_ActualChargeable = 360;

				AssertEmpty(consol);
			}

			AssertEquals(3m, consol.CalculationWrapper.JK_CorrectedConsolWeight);
			AssertEquals("KG", consol.CalculationWrapper.JK_CorrectedConsolWeightUnit);
			AssertEquals(240m, consol.CalculationWrapper.JK_CorrectedConsolVolume);
			AssertEquals("M3", consol.CalculationWrapper.JK_CorrectedConsolVolumeUnit);
			AssertEquals(240m, consol.CalculationWrapper.JK_ConsolChargeable);
			AssertEquals("M3", consol.CalculationWrapper.JK_ConsolChargeableUnit);
			AssertEquals(false, consol.CalculationWrapper.JK_OverrideConsolChargeable);
			AssertEquals(0.003m, consol.CalculationWrapper.JK_Calc_ActualVolumeWeight);
			AssertEquals("M3", consol.CalculationWrapper.JK_Calc_ActualVolumeWeightUnit);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeable);
			AssertEquals("AUD PER M3", consol.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeableDesc);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeable);
			AssertEquals("AUD PER M3", consol.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeableDesc);
			AssertEquals(120m, consol.CalculationWrapper.JK_Calc_FreeSpace);
			AssertEquals(true, consol.CalculationWrapper.HasChanges);

			AssertEquals(239.997m, consol.Density.ExcessVolumeWeight);
			AssertEquals("M3", consol.Density.ExcessVolumeWeightUnit);
			AssertEquals(50m, consol.Density.WeightUtilisationPercentage);
			AssertEquals(80m, consol.Density.VolumeUtilisationPercentage);
		}

		static void AssertEmpty(ForwardingConsol consol)
		{
			AssertEquals(0m, consol.CalculationWrapper.JK_CorrectedConsolWeight);
			AssertEquals("KG", consol.CalculationWrapper.JK_CorrectedConsolWeightUnit);
			AssertEquals(0m, consol.CalculationWrapper.JK_CorrectedConsolVolume);
			AssertEquals("M3", consol.CalculationWrapper.JK_CorrectedConsolVolumeUnit);
			AssertEquals(0m, consol.CalculationWrapper.JK_ConsolChargeable);
			AssertEquals("M3", consol.CalculationWrapper.JK_ConsolChargeableUnit);
			AssertEquals(false, consol.CalculationWrapper.JK_OverrideConsolChargeable);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_ActualVolumeWeight);
			AssertEquals("M3", consol.CalculationWrapper.JK_Calc_ActualVolumeWeightUnit);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeable);
			AssertEquals("AUD PER M3", consol.CalculationWrapper.JK_Calc_ConsolidatedFreightCostChargeableDesc);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeable);
			AssertEquals("AUD PER M3", consol.CalculationWrapper.JK_Calc_ShipmentFreightCostChargeableDesc);
			AssertEquals(0m, consol.CalculationWrapper.JK_Calc_FreeSpace);

			AssertEquals(0m, consol.Density.ExcessVolumeWeight);
			AssertEquals("M3", consol.Density.ExcessVolumeWeightUnit);
			AssertEquals(0m, consol.Density.WeightUtilisationPercentage);
			AssertEquals(0m, consol.Density.VolumeUtilisationPercentage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new ConsolDashboardCalculationWrapper(consol);
		}
	}
}
