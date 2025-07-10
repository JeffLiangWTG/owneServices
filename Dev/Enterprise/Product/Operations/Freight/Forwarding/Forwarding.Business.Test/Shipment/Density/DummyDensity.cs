using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DummyDensity : Density
	{
		public bool isRefreshAllowed { get; set; }

		public bool isChargeableByWeight { get; set; }

		public ZDecimal totalWeight { get; set; }

		public ZDecimal totalVolume { get; set; }

		public ZDecimal calculatedVolumeWeight { get; set; }

		#region Density Property Overrides

		protected override bool IsRefreshAllowed => isRefreshAllowed;

		protected override bool IsChargeableByWeight => isChargeableByWeight;

		protected override ZDecimal TotalWeight => totalWeight;

		protected override ZDecimal TotalVolume => totalVolume;

		protected override ZDecimal CalculatedVolumeWeight => calculatedVolumeWeight;

		#endregion
	}
}
