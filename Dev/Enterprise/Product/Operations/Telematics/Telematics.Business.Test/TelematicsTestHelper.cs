using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business.Test
{
	public class TelematicsTestHelper : ITelematicsTestHelper
	{
		public TelematicsTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public IDeviceLocation CreateDeviceLocation(ZGuid device, ZGuid truck, double longitude, double latitude, ZDateTime eventTime, ZDecimal speedKmh, ZDecimal headingDegrees)
		{
			var location = Factory.New<GlbDeviceLocation>();
			location.V2_V3_Device = device;
			location.V2_Location = ZGeography.CreatePoint(longitude, latitude, 0);
			location.V2_MeasurementTimeUtc = eventTime;

			location.V2_AccuracyInMetres = 0;
			location.V2_CompassHeadingDegrees = headingDegrees;
			location.V2_Speedkmh = speedKmh;

			return location;
		}

		public ZGuid CreateDevice(ZString model, ZString humanReadableIdentifier, byte[] mobileServiceIdentifier)
		{
			var device = Factory.New<GlbDevice>();
			device.V3_MobileServicesIdentifier = mobileServiceIdentifier;
			device.V3_HumanReadableIdentifier = humanReadableIdentifier;
			device.V3_Model = model;

			return device.PK;
		}

		public ZGuid CreateDeviceAndDivot(ZGuid truckPK, ZString model, ZString humanReadableIdentifier, byte[] mobileServiceIdentifier)
		{
			var device = Factory.New<GlbDevice>();
			device.V3_Model = model;
			device.V3_MobileServicesIdentifier = mobileServiceIdentifier;
			device.V3_HumanReadableIdentifier = humanReadableIdentifier;

			var divot = Factory.New<GlbDeviceAssignmentDivot>();
			divot.V7_V3_Device = device.PK;
			divot.V7_StartTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			divot.V7_EndTimeUtc = ZDateTime.UtcNow.AddDays(1);
			divot.V7_ParentTableCode = RefEquipmentSchema.Constants.Prefix;
			divot.V7_ParentID = truckPK;

			return device.PK;
		}

		public ZGuid CreateDeviceAssignment(ZGuid devicePK, ZGuid parentID, ZDateTime startTimeUtc, ZDateTime endTimeUtc, ZString parentTableCode)
		{
			var assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = startTimeUtc;
			assignment.V7_EndTimeUtc = endTimeUtc;
			assignment.V7_V3_Device = devicePK;
			assignment.V7_ParentID = parentID;
			assignment.V7_ParentTableCode = parentTableCode;

			return assignment.PK;
		}

		public void CreateTelEdgeAssociation(ZGuid fromId, string fromTableCode, ZGuid toId, string toTableCode, ZByte templateMapping, ZDateTimeOffset startTime, ZDateTimeOffset endTime, string relationshipType)
		{
			var telEdge = Factory.New<TelEdge>();
			telEdge.TE_EntityIdFrom = fromId;
			telEdge.TE_EntityTableCodeFrom = fromTableCode;
			telEdge.TE_EntityIdTo = toId;
			telEdge.TE_EntityTableCodeTo = toTableCode;
			telEdge.TE_TemplateMapping = templateMapping;
			telEdge.TE_StartTime = startTime;
			if (!endTime.IsEmpty)
			{
				telEdge.TE_EndTime = endTime;
			}
			telEdge.TE_RelationshipType = relationshipType;
		}
	}
}
