using Enterprise.Services.OperationalActions.Business;
using Enterprise.TransportCommon.Registry;

namespace Enterprise.TransportBookings.Module
{
	public sealed class FilterMasterBookingEnabledRegistryConstraint : IFilterConstraint
	{
		public string Name => "MasterBookingsEnabled";

		public string SingularValueName => Res.GetString("0edbfa4d-cae3-483b-a355-8f61165ac135", "value");

		public string PluralValueName => Res.GetString("5611e025-4330-4ef2-88b3-4da330145874", "values");

		public string Description => Res.GetString("bc16fe22-9d3e-4937-9ea4-6f6834339324", "Whether Master Bookings are enabled on this system");

		public string GetDefaultStringValue()
		{
			return TransportRegistry.Instance.MasterBookingsEnabled.Value ? "Y" : "N";
		}

		public object GetValue() => GetDefaultStringValue();
	}
}
