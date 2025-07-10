using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors
{
	static class TcaRegistrationHelper
	{
		public static TelEdge GetExistingRegistration(BusinessObjectFactory factory, GlbDevice device)
		{
			if (device == null)
			{
				return null;
			}

			var query = new ZQuery(
				new ZQuery(TelEdgeSchema.TE_EntityIdTo, device.PK),
				JoinCondition.And,
				new ZQuery(
					new ZQuery(TelEdgeSchema.TE_RelationshipType, TelEdgeRelationshipTypes.Codes.RIM),
					JoinCondition.And,
					new ZQuery(TelEdgeSchema.TE_EndTime, null)));
			return factory.Load<TelEdge>(query)
				.SingleOrDefault();
		}

		public static GlbDeviceAssignmentDivot GetDivot(BusinessObjectFactory factory, GlbDevice device, RimRegistrationMessage registration)
		{
			if (device == null)
			{
				return null;
			}

			var query = new ZQuery(
				new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, device.PK),
				JoinCondition.And,
				new ZQuery(
					new ZQuery(GlbDeviceAssignmentDivotSchema.V7_StartTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, registration.DeviceAssignmentTime.UtcDateTime),
					JoinCondition.And,
					new ZQuery(GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc, null)));
			return factory.Load<GlbDeviceAssignmentDivot>(query)
				.SingleOrDefault();
		}
	}
}
