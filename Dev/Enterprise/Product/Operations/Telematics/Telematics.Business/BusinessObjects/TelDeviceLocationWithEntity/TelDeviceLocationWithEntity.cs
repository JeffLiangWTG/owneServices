using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business
{
	public class TelDeviceLocationWithEntity : AutoTelDeviceLocationWithEntity, IDeviceLocationWithEntity
	{
		public TelDeviceLocationWithEntity(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(TLL_DeviceID);

		[RelatedBusinessObject("Device")]
		public override ZGuid TLL_DeviceID
		{
			get => base.TLL_DeviceID;
			set => base.TLL_DeviceID = value;
		}

		public ZDateTime MeasurementTimeUtc => TLL_MeasurementTimeUtc;
		public ZGeography Location => TLL_Location;
		public ZDecimal Speedkmh => TLL_Speedkmh;
		public ZDecimal CompassHeadingDegrees => TLL_CompassHeadingDegrees;
		public ZDecimal SpeedLimitKmh => TLL_SpeedLimitKmh;
		public ZString SpeedLimitState => TLL_SpeedLimitState;
		public ZString EntityTableCode => TLL_ParentTableCode;
		public ZGuid EntityId => TLL_ParentID;

		public ITelEdge GetOperator()
		{
			var measurementTimeOffset = new ZDateTimeOffset(TLL_MeasurementTimeUtc);
			var query = new ZQuery();
			query.AddToFilter(TelEdgeSchema.TE_EntityTableCodeTo, GlbStaffSchema.Constants.Prefix);
			query.AddToFilter(TelEdgeSchema.TE_EntityTableCodeFrom, TLL_ParentTableCode);
			query.AddToFilter(TelEdgeSchema.TE_EntityIdFrom, TLL_ParentID);
			query.AddToFilter(TelEdgeSchema.TE_StartTime, SQLComparisonOperator.LessThan, measurementTimeOffset);
			query.AddToFilter(TelEdgeSchema.TE_RelationshipType, TelEdgeRelationshipTypes.Codes.OPT);

			var queryEndTime = new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.GreaterThan, measurementTimeOffset);
			queryEndTime.AddToFilter(JoinCondition.Or, TelEdgeSchema.TE_EndTime, ZDateTime.Empty);

			query.AddToFilter(queryEndTime);

			var telEdge = Factory.LoadTop1<TelEdge>(query);
			return telEdge;
		}
	}
}
