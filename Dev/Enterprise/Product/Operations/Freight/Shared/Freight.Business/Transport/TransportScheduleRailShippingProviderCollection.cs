using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class TransportScheduleRailShippingProviderCollection : RailShippingProviderCollection
	{
		public TransportScheduleRailShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TransportScheduleRailShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public TransportScheduleRailShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public TransportScheduleRailShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery GetShippingProviderTransportTypeQuery()
		{
			var result = base.GetShippingProviderTransportTypeQuery();
			result.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsAirLine, true);
			result.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingLine, true);

			return result;
		}

		protected override ZString AdditionalFilterNotMetNotification => Res.GetString("43757e56-0adc-460f-abd4-cbce763e3ad7", "An Organization selected from here must have Rail/Sea/Air Provider selected.");
	}
}
