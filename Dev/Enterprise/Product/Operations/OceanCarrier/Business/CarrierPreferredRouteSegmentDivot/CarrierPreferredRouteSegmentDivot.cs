using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierPreferredRouteSegmentDivot : AutoCarrierPreferredRouteSegmentDivot
	{
		public CarrierPreferredRouteSegmentDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CarrierPreferredRoute PreferredRoute => Factory.Load<CarrierPreferredRoute>(CPG_CPU_CarrierPreferredRoute);

		[RelatedBusinessObject(nameof(PreferredRoute))]
		public override ZGuid CPG_CPU_CarrierPreferredRoute
		{
			get => base.CPG_CPU_CarrierPreferredRoute;
			set => base.CPG_CPU_CarrierPreferredRoute = value;
		}

		public RouteSegment Segment => Factory.Load<RouteSegment>(CPG_RSG_RouteSegment);

		[RelatedBusinessObject(nameof(Segment))]
		public override ZGuid CPG_RSG_RouteSegment
		{
			get => base.CPG_RSG_RouteSegment;
			set => base.CPG_RSG_RouteSegment = value;
		}
	}
}
