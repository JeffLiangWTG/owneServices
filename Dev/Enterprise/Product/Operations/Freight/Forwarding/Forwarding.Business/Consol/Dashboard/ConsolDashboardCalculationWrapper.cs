using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDashboardCalculationWrapper : NonPersistentBusinessObject
	{
		readonly ForwardingConsol consol;
		bool isBound;

		public ConsolDashboardCalculationWrapper(ForwardingConsol consol)
		{
			this.consol = consol;

			UpdateCalculation();
		}

		public DisposableAction SuspendRefreshAllValues()
		{
			return new DisposableAction(() => UpdateCalculation());
		}

		public void UpdateCalculation()
		{
			if (consol == null || !isBound)
			{
				return;
			}

			JK_CorrectedConsolWeight = consol.JK_CorrectedConsolWeight;
			JK_CorrectedConsolWeightUnit = consol.JK_CorrectedConsolWeightUnit;
			JK_CorrectedConsolVolume = consol.JK_CorrectedConsolVolume;
			JK_CorrectedConsolVolumeUnit = consol.JK_CorrectedConsolVolumeUnit;
			JK_ConsolChargeable = consol.JK_ConsolChargeable;
			JK_ConsolChargeableUnit = consol.JK_ConsolChargeableUnit;
			JK_OverrideConsolChargeable = consol.JK_OverrideConsolChargeable;
			JK_Calc_ActualVolumeWeight = consol.JK_Calc_ActualVolumeWeight;
			JK_Calc_ActualVolumeWeightUnit = consol.JK_Calc_ActualVolumeWeightUnit;
			JK_Calc_ConsolidatedFreightCostChargeable = consol.JK_Calc_ConsolidatedFreightCostChargeable;
			JK_Calc_ConsolidatedFreightCostChargeableDesc = consol.JK_Calc_ConsolidatedFreightCostChargeableDesc;
			JK_Calc_ShipmentFreightCostChargeable = consol.JK_Calc_ShipmentFreightCostChargeable;
			JK_Calc_ShipmentFreightCostChargeableDesc = consol.JK_Calc_ShipmentFreightCostChargeableDesc;
			JK_Calc_FreeSpace = consol.JK_Calc_FreeSpace;
		}

		public void MarkAsBound()
		{
			isBound = true;
			UpdateCalculation();
		}

		public void MarkAsUnbound()
		{
			isBound = false;
		}

		#region JK_CorrectedConsolWeight

		ZDecimal correctedConsolWeight;
		public ZDecimal JK_CorrectedConsolWeight
		{
			get => correctedConsolWeight;
			private set => SetNonPersistentPropertyValue(JK_CorrectedConsolWeightInfo, ref correctedConsolWeight, value);
		}

		public ZPropertyInfo JK_CorrectedConsolWeightInfo => GetZPropertyInfo(nameof(JK_CorrectedConsolWeight));

		#endregion

		#region JK_CorrectedConsolWeightUnit

		ZString correctedConsolWeightUnit;
		public ZString JK_CorrectedConsolWeightUnit
		{
			get => correctedConsolWeightUnit;
			private set => SetNonPersistentPropertyValue(JK_CorrectedConsolWeightUnitInfo, ref correctedConsolWeightUnit, value);
		}

		public ZPropertyInfo JK_CorrectedConsolWeightUnitInfo => GetZPropertyInfo(nameof(JK_CorrectedConsolWeightUnit));

		#endregion

		#region JK_CorrectedConsolVolume

		ZDecimal correctedConsolVolume;
		public ZDecimal JK_CorrectedConsolVolume
		{
			get => correctedConsolVolume;
			private set => SetNonPersistentPropertyValue(JK_CorrectedConsolVolumeInfo, ref correctedConsolVolume, value);
		}

		public ZPropertyInfo JK_CorrectedConsolVolumeInfo => GetZPropertyInfo(nameof(JK_CorrectedConsolVolume));

		#endregion

		#region JK_CorrectedConsolVolumeUnit
		ZString correctedConsolVolumeUnit;
		public ZString JK_CorrectedConsolVolumeUnit
		{
			get => correctedConsolVolumeUnit;
			private set => SetNonPersistentPropertyValue(JK_CorrectedConsolVolumeUnitInfo, ref correctedConsolVolumeUnit, value);
		}

		public ZPropertyInfo JK_CorrectedConsolVolumeUnitInfo => GetZPropertyInfo(nameof(JK_CorrectedConsolVolumeUnit));

		#endregion

		#region JK_ConsolChargeable
		ZDecimal consolChargeable;
		public ZDecimal JK_ConsolChargeable
		{
			get => consolChargeable;
			private set => SetNonPersistentPropertyValue(JK_ConsolChargeableInfo, ref consolChargeable, value);
		}

		public ZPropertyInfo JK_ConsolChargeableInfo => GetZPropertyInfo(nameof(JK_ConsolChargeable));

		#endregion

		#region JK_ConsolChargeableUnit
		ZString consolChargeableUnit;
		public ZString JK_ConsolChargeableUnit
		{
			get => consolChargeableUnit;
			private set => SetNonPersistentPropertyValue(JK_ConsolChargeableUnitInfo, ref consolChargeableUnit, value);
		}

		public ZPropertyInfo JK_ConsolChargeableUnitInfo => GetZPropertyInfo(nameof(JK_ConsolChargeableUnit));

		#endregion

		#region JK_OverrideConsolChargeable
		ZBool overrideConsolChargeable;
		public ZBool JK_OverrideConsolChargeable
		{
			get => overrideConsolChargeable;
			private set => SetNonPersistentPropertyValue(JK_OverrideConsolChargeableInfo, ref overrideConsolChargeable, value);
		}

		public ZPropertyInfo JK_OverrideConsolChargeableInfo => GetZPropertyInfo(nameof(JK_OverrideConsolChargeable));

		#endregion

		#region JK_Calc_ActualVolumeWeight
		ZDecimal calc_ActualVolumeWeight;
		public ZDecimal JK_Calc_ActualVolumeWeight
		{
			get => calc_ActualVolumeWeight;
			private set => SetNonPersistentPropertyValue(JK_Calc_ActualVolumeWeightInfo, ref calc_ActualVolumeWeight, value);
		}

		public ZPropertyInfo JK_Calc_ActualVolumeWeightInfo => GetZPropertyInfo(nameof(JK_Calc_ActualVolumeWeight));

		#endregion

		#region JK_Calc_ActualVolumeWeightUnit
		ZString calc_ActualVolumeWeightUnit;
		public ZString JK_Calc_ActualVolumeWeightUnit
		{
			get => calc_ActualVolumeWeightUnit;
			private set => SetNonPersistentPropertyValue(JK_Calc_ActualVolumeWeightUnitInfo, ref calc_ActualVolumeWeightUnit, value);
		}

		public ZPropertyInfo JK_Calc_ActualVolumeWeightUnitInfo => GetZPropertyInfo(nameof(JK_Calc_ActualVolumeWeightUnit));

		#endregion

		#region JK_Calc_ConsolidatedFreightCostChargeable
		ZDecimal calc_ConsolidatedFreightCostChargeable;
		public ZDecimal JK_Calc_ConsolidatedFreightCostChargeable
		{
			get => calc_ConsolidatedFreightCostChargeable;
			private set => SetNonPersistentPropertyValue(JK_Calc_ConsolidatedFreightCostChargeableInfo, ref calc_ConsolidatedFreightCostChargeable, value);
		}

		public ZPropertyInfo JK_Calc_ConsolidatedFreightCostChargeableInfo => GetZPropertyInfo(nameof(JK_Calc_ConsolidatedFreightCostChargeable));

		#endregion

		#region JK_Calc_ConsolidatedFreightCostChargeableDesc
		ZString calc_ConsolidatedFreightCostChargeableDesc;
		public ZString JK_Calc_ConsolidatedFreightCostChargeableDesc
		{
			get => calc_ConsolidatedFreightCostChargeableDesc;
			private set => SetNonPersistentPropertyValue(JK_Calc_ConsolidatedFreightCostChargeableDescInfo, ref calc_ConsolidatedFreightCostChargeableDesc, value);
		}

		public ZPropertyInfo JK_Calc_ConsolidatedFreightCostChargeableDescInfo => GetZPropertyInfo(nameof(JK_Calc_ConsolidatedFreightCostChargeableDesc));

		#endregion

		#region JK_Calc_ShipmentFreightCostChargeable
		ZDecimal calc_ShipmentFreightCostChargeable;
		public ZDecimal JK_Calc_ShipmentFreightCostChargeable
		{
			get => calc_ShipmentFreightCostChargeable;
			private set => SetNonPersistentPropertyValue(JK_Calc_ShipmentFreightCostChargeableInfo, ref calc_ShipmentFreightCostChargeable, value);
		}

		public ZPropertyInfo JK_Calc_ShipmentFreightCostChargeableInfo => GetZPropertyInfo(nameof(JK_Calc_ShipmentFreightCostChargeable));

		#endregion

		#region JK_Calc_ShipmentFreightCostChargeableDesc
		ZString calc_ShipmentFreightCostChargeableDesc;
		public ZString JK_Calc_ShipmentFreightCostChargeableDesc
		{
			get => calc_ShipmentFreightCostChargeableDesc;
			private set => SetNonPersistentPropertyValue(JK_Calc_ShipmentFreightCostChargeableDescInfo, ref calc_ShipmentFreightCostChargeableDesc, value);
		}

		public ZPropertyInfo JK_Calc_ShipmentFreightCostChargeableDescInfo => GetZPropertyInfo(nameof(JK_Calc_ShipmentFreightCostChargeableDesc));

		#endregion

		#region JK_Calc_FreeSpace
		ZDecimal calc_FreeSpace;
		public ZDecimal JK_Calc_FreeSpace
		{
			get => calc_FreeSpace;
			private set => SetNonPersistentPropertyValue(JK_Calc_FreeSpaceInfo, ref calc_FreeSpace, value);
		}

		public ZPropertyInfo JK_Calc_FreeSpaceInfo => GetZPropertyInfo(nameof(JK_Calc_FreeSpace));
		#endregion
	}
}
