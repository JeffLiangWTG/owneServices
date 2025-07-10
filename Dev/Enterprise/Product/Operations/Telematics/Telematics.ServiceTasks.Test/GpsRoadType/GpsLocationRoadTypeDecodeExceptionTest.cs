using System;
using CargoWise.Types;
using Enterprise.Telematics.ServiceTasks.GpsRoadType;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.GpsRoadType
{
	public class GpsLocationRoadTypeDecodeExceptionTest : TestCase
	{
		public void TestLatitudeLongitudeException()
		{
			CombineAssertions(() =>
			{
				Test(ZGeography.CreatePoint(1, 1), "some Message", "Failed to decode json response on location [Latitude: 1, Longitude: 1]");
				Test(ZGeography.CreatePoint(20, 10), "some Message", "Failed to decode json response on location [Latitude: 10, Longitude: 20]");
			});

			void Test(ZGeography point, string innerMessage, string expectedMessage)
			{
				// Arrange
				// Act
				var exception = new GpsLocationRoadTypeDecodeException(point, new Exception(innerMessage));

				// Assert
				AssertEquals(expectedMessage, exception.Message);
				AssertEquals(innerMessage, exception.InnerException.Message);
			}
		}
	}
}
