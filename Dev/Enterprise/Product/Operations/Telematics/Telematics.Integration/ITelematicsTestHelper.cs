#if DEBUG
using CargoWise.Types;

namespace Enterprise.Telematics.Integration
{
	public interface ITelematicsTestHelper
	{
		IDeviceLocation CreateDeviceLocation(ZGuid device, ZGuid truck, double longitude, double latitude, ZDateTime eventTime, ZDecimal speedKmh, ZDecimal headingDegrees);

		ZGuid CreateDevice(ZString model, ZString humanReadableIdentifier, byte[] mobileServiceIdentifier);

		ZGuid CreateDeviceAndDivot(ZGuid truckPK, ZString model, ZString humanReadableIdentifier, byte[] mobileServiceIdentifier);

		ZGuid CreateDeviceAssignment(ZGuid devicePK, ZGuid parentID, ZDateTime startTimeUtc, ZDateTime endTimeUtc, ZString parentTableCode);

		void CreateTelEdgeAssociation(ZGuid fromId, string fromTableCode, ZGuid toId, string toTableCode, ZByte templateMapping, ZDateTimeOffset startTime, ZDateTimeOffset endTime, string relationshipType);
	}
}
#endif
