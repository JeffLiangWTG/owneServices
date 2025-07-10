using System;
using CargoWise.Types;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	[Serializable]
	public class GpsLocationRoadTypeDecodeException : Exception
	{
		public GpsLocationRoadTypeDecodeException(ZGeography location, Exception innerException)
			: base($"Failed to decode json response on location [Latitude: {location.Latitude}, Longitude: {location.Longitude}]", innerException)
		{
		}

#if NETFRAMEWORK
		public GpsLocationRoadTypeDecodeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
