using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.TransportCommon.Business
{
	public static class DtbTransportTotalsHelper
	{
		#region TotalWeightUnit

		public static ZString TotalWeightUnit
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}

		#endregion

		#region TotalVolumeUnit

		public static ZString TotalVolumeUnit
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		#endregion
	}
}
