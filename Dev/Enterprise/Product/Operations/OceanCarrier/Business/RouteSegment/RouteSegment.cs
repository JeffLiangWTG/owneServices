using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class RouteSegment : AutoRouteSegment
	{
		public RouteSegment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region RSG_TransitTime

		[ZDateTimeDurationValue]
		public override ZDateTime RSG_TransitTime
		{
			get => base.RSG_TransitTime;
			set => base.RSG_TransitTime = value.ConvertToDurationBasedDate(RSG_TransitTimeInfo);
		}

		#endregion
	}
}
