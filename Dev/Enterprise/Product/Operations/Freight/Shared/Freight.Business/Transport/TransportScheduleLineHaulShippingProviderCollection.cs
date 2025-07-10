using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class TransportScheduleLineHaulShippingProviderCollection : LineHaulShippingProviderCollection
	{
		public TransportScheduleLineHaulShippingProviderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public TransportScheduleLineHaulShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public TransportScheduleLineHaulShippingProviderCollection(BusinessObjectFactory factory, OrganisationDefaults defaults) : base(factory, defaults)
		{
		}

		public TransportScheduleLineHaulShippingProviderCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults) : base(factory, filter, defaults)
		{
		}

		protected override ZQuery GetShippingProviderTransportTypeQuery()
		{
			var result = base.GetShippingProviderTransportTypeQuery();
			result.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsAirLine, true);
			result.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsShippingLine, true);

			return result;
		}

		protected override ZString AdditionalFilterNotMetNotification => Res.GetString("e31fb59b-8c79-4316-9ef0-689ab451eade", "An Organization selected from here must have Line Haul or Shipping/Air Line selected.");
	}
}
