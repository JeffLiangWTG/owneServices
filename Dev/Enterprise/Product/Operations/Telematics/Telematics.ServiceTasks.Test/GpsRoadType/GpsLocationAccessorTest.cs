using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.GpsRoadType;

namespace Enterprise.Telematics.ServiceTasks.Test.GpsRoadType
{
	class GpsLocationAccessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			gpsLocationAccessor = new GpsLocationAccessor();
			base.SetUp();
		}

		public void TestRetrieveGpsLocationsWithUnknownRoadType()
		{
			RetrieveGpsLocationsWithRoadTypeTest(
				new Dictionary<string, IEnumerable<string>>
				{
					{ "01020304", new[] { "U", "U", "U" } },
				},
				3);
		}

		public void TestRetrieveGpsLocationsWithMultipleTypes()
		{
			RetrieveGpsLocationsWithRoadTypeTest(new Dictionary<string, IEnumerable<string>>
				{
					{ "04030201", new [] { "U", "P", "R" } },
				},
				1);
		}

		public void TestRetrieveGpsLocationsWithNoUnknownLocations()
		{
			RetrieveGpsLocationsWithRoadTypeTest(new Dictionary<string, IEnumerable<string>>
				{
					{ "FFFFFFFF", new [] { "P", "R", "R" } },
				},
				0);
		}

		public void TestRetrieveGpsLocationsWithNoLocations()
		{
			RetrieveGpsLocationsWithRoadTypeTest(
				new Dictionary<string, IEnumerable<string>>(),
				0);
		}

		public void TestRetreiveGpsLocationsWithMaxBatchSize()
		{
			CombineAssertions(() =>
			{
				TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				RetrieveGpsLocationsWithRoadTypeTest(
					new Dictionary<string, IEnumerable<string>>
					{
						{ "01020304", new[] { "U", "U", "U" } },
					},
					1);

				TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
				RetrieveGpsLocationsWithRoadTypeTest(
					new Dictionary<string, IEnumerable<string>>
					{
						{ "04030201", new[] { "U", "U", "U", "U" } },
					},
					3);

				TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);
				RetrieveGpsLocationsWithRoadTypeTest(
					new Dictionary<string, IEnumerable<string>>
					{
						{ "FFFFFFFF", new[] { "U", "U", "U", "U" } },
					},
					11);
			});
		}

		void RetrieveGpsLocationsWithRoadTypeTest(Dictionary<string, IEnumerable<string>> existingRecords, int expectedNumberOfLocations)
		{
			// Arrange
			CreateDeviceAndRecords(existingRecords);

			// Act
			var locations = gpsLocationAccessor.GetLocations(Factory);

			// Assert
			AssertEquals(expectedNumberOfLocations, locations.Count);
		}

		void CreateDeviceAndRecords(Dictionary<string, IEnumerable<string>> recordsToCreate)
		{
			var records = recordsToCreate.Select(dict =>
			{
				var device = Factory.New<GlbDevice>();
				device.V3_HumanReadableIdentifier = dict.Key;
				device.V3_HardwareIdentifier = dict.Key;
				device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
				device.V3_MobileServicesIdentifier = ZBlob.FromAscii(dict.Key);
				device.V3_IsActive = true;
				device.V3_Model = "model";

				var locations = dict.Value.Select(roadType =>
				{
					var location = Factory.New<GlbDeviceLocation>();
					location.V2_RoadType = roadType;
					location.V2_V3_Device = device.PK;
					location.V2_MeasurementTimeUtc = ZDateTime.Now;
					return location;
				}).ToList();

				return locations;
			}).ToList();
			Factory.Save();
		}

		GpsLocationAccessor gpsLocationAccessor;
	}
}
