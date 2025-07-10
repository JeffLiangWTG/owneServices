using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	class GpsLocationAccessor : IGpsLocationAccessor
	{
		public GpsLocationAccessor()
		{
		}

		public ICollection<GlbDeviceLocation> GetLocations(BusinessObjectFactory factory)
		{
			var collection = new DynamicBusinessObjectCollection(factory);

			collection.Load(
				$@"
SELECT TOP({TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.Value})
	{GlbDeviceLocationSchema.Constants.PK}
FROM
	{GlbDeviceLocationSchema.Constants.SqlSchemaName}.{GlbDeviceLocationSchema.Constants.TableName} WITH (INDEX = NR_RX__V2_RoadType)
WHERE
	{GlbDeviceLocationSchema.Constants.V2_RoadType} = 'U'");

			return factory.Load<GlbDeviceLocation>(
				new ZQuery(GlbDeviceLocationSchema.PK, collection.Select(item => (ZGuid)item[GlbDeviceLocationSchema.Constants.PK]))
				{
					MaximumRows = TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.Value,
				});
		}
	}
}
