using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public abstract class Density : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Zero Division constants

		public const decimal DensityFactorForZeroDivision = 0m;
		public const string NAValueForZeroDivision = "N/A";

		#endregion

		#region VolumeRatio

		public ZString VolumeRatio { get; private set; }

		public ZPropertyInfo VolumeRatioInfo => GetZPropertyInfo(nameof(VolumeRatio));

		#endregion

		#region DensityFactor

		public ZDecimal DensityFactor { get; private set; }

		public ZPropertyInfo DensityFactorInfo => GetZPropertyInfo(nameof(DensityFactor));

		#endregion

		#region DensityRemark

		public ZString DensityRemark { get; private set; }

		public ZPropertyInfo DensityRemarkInfo => GetZPropertyInfo(nameof(DensityRemark));

		#endregion

		#region RefreshAllValues

		public void RefreshAllValues()
		{
			if (IsRefreshAllowed)
			{
				RefreshDensityFactor();

				RefreshAllValuesCore();

				RefreshAllBindings();
			}
		}

		protected void RefreshDensityFactor()
		{
			var isValidDensityFactor = false;
			DensityFactor = DensityFactorForZeroDivision;

			if (IsChargeableByWeight && TotalWeight > ZDecimal.Zero && CalculatedVolumeWeight >= ZDecimal.Zero)
			{
				DensityFactor = CalculatedVolumeWeight / TotalWeight;
				isValidDensityFactor = true;
			}
			else if (!IsChargeableByWeight && CalculatedVolumeWeight > ZDecimal.Zero && TotalVolume >= ZDecimal.Zero)
			{
				DensityFactor = TotalVolume / CalculatedVolumeWeight;
				isValidDensityFactor = true;
			}
			else
			{
				VolumeRatio = DensityRemark = NAValueForZeroDivision;
			}

			if (isValidDensityFactor)
			{
				var densityValues = LookupDensityValuesFromDensityFactor();
				VolumeRatio = densityValues.VolumeRatio;
				DensityRemark = densityValues.DensityRemark;
			}
		}

		protected virtual void RefreshAllValuesCore()
		{
		}

		void RefreshAllBindings()
		{
			DensityFactorInfo.RefreshBinding();
			VolumeRatioInfo.RefreshBinding();
			DensityRemarkInfo.RefreshBinding();

			RefreshAllBindingsCore();
		}

		protected virtual void RefreshAllBindingsCore()
		{
		}

		protected abstract bool IsRefreshAllowed { get; }

		protected abstract bool IsChargeableByWeight { get; }

		protected abstract ZDecimal TotalWeight { get; }

		protected abstract ZDecimal TotalVolume { get; }

		protected abstract ZDecimal CalculatedVolumeWeight { get; }

		#endregion

		public static List<ZDecimal> DensityValuesList => new List<ZDecimal> { 0.0000m, 0.2505m, 0.4200m, 0.5800m, 0.7500m, 0.9200m, 1.0855m, 1.2525m, 1.4195m, 1.5865m, 1.7535m, 1.9190m };

		public DensityValues[] DensityValuesLookup = {
			new DensityValues(DensityValuesList[0], "1:1", (NoResString)"Dense ++++"),		// Visualization Indicator
			new DensityValues(DensityValuesList[1], "1:2", (NoResString)"Dense +++"),		// Visualization Indicator
			new DensityValues(DensityValuesList[2], "1:3", (NoResString)"Dense ++"),			// Visualization Indicator
			new DensityValues(DensityValuesList[3], "1:4", (NoResString)"Dense +"),			// Visualization Indicator
			new DensityValues(DensityValuesList[4], "1:5", (NoResString)"Dense"),			// Visualization Indicator
			new DensityValues(DensityValuesList[5], "1:6", (NoResString)"1 to 1 cargo"),		// Visualization Indicator
			new DensityValues(DensityValuesList[6], "1:7", (NoResString)"Volume"),			// Visualization Indicator
			new DensityValues(DensityValuesList[7], "1:8", (NoResString)"Volume +"),			// Visualization Indicator
			new DensityValues(DensityValuesList[8], "1:9", (NoResString)"Volume ++"),		// Visualization Indicator
			new DensityValues(DensityValuesList[9], "1:10", (NoResString)"Volume +++"),		// Visualization Indicator
			new DensityValues(DensityValuesList[10], "1:11", (NoResString)"Volume ++++"),	// Visualization Indicator
			new DensityValues(DensityValuesList[11], "1:12", (NoResString)"Volume +++++")	// Visualization Indicator
		};

		public class DensityValues
		{
			public DensityValues(ZDecimal densityFactor, ZString volumeRatio, ZString densityRemark)
			{
				VolumeRatio = volumeRatio;
				DensityFactor = densityFactor;
				DensityRemark = densityRemark;
			}

			public ZDecimal DensityFactor { get; }
			public ZString VolumeRatio { get; }
			public ZString DensityRemark { get; }
		}

		#region Implementation

		DensityValues LookupDensityValuesFromDensityFactor()
		{
			return DensityValuesLookup
				.OrderBy(x => -x.DensityFactor)
				.FirstOrDefault(d => DensityFactor >= d.DensityFactor);
		}

		#endregion
	}
}
