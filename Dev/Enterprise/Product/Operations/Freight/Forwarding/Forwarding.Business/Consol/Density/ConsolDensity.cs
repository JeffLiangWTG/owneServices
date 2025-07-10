using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDensity : Density
	{
		bool isSuspendRefreshAllValues;

		public ConsolDensity(ForwardingConsol parentConsol)
		{
			consol = parentConsol;

			HookConsolForDensity();
			RefreshAllValues();
		}

		readonly ForwardingConsol consol;

		#region Density Property Overrides

		protected override bool IsRefreshAllowed => !consol.IsDeleted && !isSuspendRefreshAllValues;

		protected override bool IsChargeableByWeight => consol.IsConsolChargeableByWeight;

		protected override ZDecimal TotalWeight => consol.JK_CorrectedConsolWeight;

		protected override ZDecimal TotalVolume => consol.JK_CorrectedConsolVolume;

		protected override ZDecimal CalculatedVolumeWeight => consol.JK_Calc_ActualVolumeWeight;

		#endregion

		#region Utilisation Percentage

		[DecimalPlaces(2)]
		public ZDecimal WeightUtilisationPercentage { get; private set; }

		public ZPropertyInfo WeightUtilisationPercentageInfo => GetZPropertyInfo(nameof(WeightUtilisationPercentage));

		[DecimalPlaces(2)]
		public ZDecimal VolumeUtilisationPercentage { get; private set; }

		public ZPropertyInfo VolumeUtilisationPercentageInfo => GetZPropertyInfo(nameof(VolumeUtilisationPercentage));

		#endregion

		#region ExcessVolumeWeight

		public ZDecimal ExcessVolumeWeight { get; private set; }

		public ZPropertyInfo ExcessVolumeWeightInfo => GetZPropertyInfo(nameof(ExcessVolumeWeight));

		[List("VolumeWeightUnit")]
		public ZString ExcessVolumeWeightUnit { get; private set; }

		public ZPropertyInfo ExcessVolumeWeightUnitInfo => GetZPropertyInfo(nameof(ExcessVolumeWeightUnit));

		public ZBool IsActualExcess { get; private set; }

		public ZPropertyInfo IsActualExcessInfo => GetZPropertyInfo(nameof(IsActualExcess));

		#endregion

		#region Implementation

		protected override void RefreshAllValuesCore()
		{
			RefreshUtilisationPercentages();
			RefreshExcessVolumeWeight();
		}

		#region Refresh Utilisation Percentages

		void RefreshUtilisationPercentages()
		{
			RefreshWeightUtilisationPercentage();
			RefreshVolumeUtilisationPercentage();
		}

		void RefreshExcessVolumeWeight()
		{
			if (consol.IsConsolChargeableByWeight)
			{
				SetExcessVolumeWeight(TotalWeight);
				ExcessVolumeWeightUnit = consol.JK_CorrectedConsolWeightUnit;
			}
			else
			{
				SetExcessVolumeWeight(TotalVolume);
				ExcessVolumeWeightUnit = consol.JK_CorrectedConsolVolumeUnit;
			}
		}

		void SetExcessVolumeWeight(ZDecimal totalValue)
		{
			if (totalValue > CalculatedVolumeWeight)
			{
				ExcessVolumeWeight = totalValue - CalculatedVolumeWeight;
				IsActualExcess = true;
			}
			else
			{
				ExcessVolumeWeight = CalculatedVolumeWeight - totalValue;
				IsActualExcess = false;
			}
		}

		void RefreshWeightUtilisationPercentage()
		{
			var preAllocatedWeight = consol.JK_TotalShipmentActWeightCheck;
			if (preAllocatedWeight > ZDecimal.Zero)
			{
				if (consol.WeightVerificationUnit == consol.JK_CorrectedConsolWeightUnit)
				{
					WeightUtilisationPercentage = consol.JK_CorrectedConsolWeight / preAllocatedWeight * 100;
					return;
				}

				var convertedPreAllocatedWeight = Constants.Weight.ConvertSafe(preAllocatedWeight, consol.WeightVerificationUnit, consol.JK_CorrectedConsolWeightUnit, false);
				if (convertedPreAllocatedWeight > ZDecimal.Zero)
				{
					WeightUtilisationPercentage = consol.JK_CorrectedConsolWeight / convertedPreAllocatedWeight * 100;
					return;
				}
			}

			WeightUtilisationPercentage = ZDecimal.Zero;
		}

		void RefreshVolumeUtilisationPercentage()
		{
			var preAllocatedVolume = consol.JK_TotalShipmentActVolumeCheck;
			if (preAllocatedVolume > ZDecimal.Zero)
			{
				if (consol.VolumeVerificationUnit == consol.JK_CorrectedConsolVolumeUnit)
				{
					VolumeUtilisationPercentage = consol.JK_CorrectedConsolVolume / preAllocatedVolume * 100;
					return;
				}

				var convertedPreAllocatedVolume = Constants.Volume.ConvertSafe(preAllocatedVolume, consol.VolumeVerificationUnit, consol.JK_CorrectedConsolVolumeUnit, false);
				if (convertedPreAllocatedVolume > ZDecimal.Zero)
				{
					VolumeUtilisationPercentage = consol.JK_CorrectedConsolVolume / convertedPreAllocatedVolume * 100;
					return;
				}
			}

			VolumeUtilisationPercentage = ZDecimal.Zero;
		}

		#endregion

		protected override void RefreshAllBindingsCore()
		{
			ExcessVolumeWeightInfo.RefreshBinding();
			ExcessVolumeWeightUnitInfo.RefreshBinding();
			IsActualExcessInfo.RefreshBinding();
			WeightUtilisationPercentageInfo.RefreshBinding();
			VolumeUtilisationPercentageInfo.RefreshBinding();
		}

		void HookConsolForDensity()
		{
			foreach (var propertyInfo in SignificantConsolPropertyInfos)
			{
				propertyInfo.ValueChanged += SignificantPropertyValueChanged;
			}

			consol.ShipmentsForTotalling.CountChanged += SignificantPropertyValueChanged;
		}

		void SignificantPropertyValueChanged(object sender, EventArgs e)
		{
			RefreshAllValues();
		}

		IEnumerable<ZPropertyInfo> SignificantConsolPropertyInfos
		{
			get
			{
				yield return consol.JK_TotalShipmentActWeightCheckInfo;
				yield return consol.WeightVerificationUnitInfo;
				yield return consol.JK_TotalShipmentActVolumeCheckInfo;
				yield return consol.VolumeVerificationUnitInfo;
				yield return consol.JK_TransportModeInfo;
				yield return consol.JK_CorrectedConsolWeightInfo;
				yield return consol.JK_CorrectedConsolVolumeInfo;
				yield return consol.JK_OverrideConsolChargeableInfo;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test ConsolDashboardFormBasherTest BashingForm")]
		CodeDescriptionPairList VolumeWeightUnit => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight) + Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		public DisposableAction SuspendRefreshAllValues()
		{
			return new DisposableAction(() => isSuspendRefreshAllValues = true, () => { isSuspendRefreshAllValues = false; RefreshAllValues(); });
		}

		#endregion
	}
}
