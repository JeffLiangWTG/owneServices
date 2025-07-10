using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using NUnit.Framework;

namespace Enterprise.Telematics.GUI.Test
{
	public class GlbDeviceFormLocationInfoTest : TestCaseWithFactory
	{
		public void TestNoLocationData()
		{
			var device = Factory.New<GlbDevice>();
			var text = GetLocationInfoText(device);

			AssertEquals("This device has not reported location data.", text);
		}

		/*
		 * The actual date/time on these tests doesn't matter, but we need to freeze it for the duration of the test.
		 */

		[TestDate(2017, 08, 10)]
		public void TestImmediate()
		{
			AssertLocationInfoText("This device was last seen just now at (-31.4, -27.5).", TimeSpan.FromMilliseconds(800), -31.4, -27.5);
		}

		[TestDate(2017, 08, 10)]
		public void TestSecondsMin()
		{
			AssertLocationInfoText("This device was last seen 2 seconds ago at (-33.931771, 151.175266).", TimeSpan.FromSeconds(2), -33.931771, 151.175266);
		}

		[TestDate(2017, 08, 10)]
		public void TestSecondsMax()
		{
			AssertLocationInfoText("This device was last seen 59 seconds ago at (-33.970516, 151.19385).", TimeSpan.FromSeconds(59), -33.970516, 151.19385);
		}

		[TestDate(2017, 08, 10)]
		public void TestMinute()
		{
			AssertLocationInfoText("This device was last seen 1 minute ago at (-33.853533, 151.209998).", TimeSpan.FromMinutes(1), -33.853533, 151.209998);
		}

		[TestDate(2017, 08, 10)]
		public void TestMinutesMin()
		{
			AssertLocationInfoText("This device was last seen 2 minutes ago at (-33.861225, 151.211312).", TimeSpan.FromMinutes(2), -33.861225, 151.211312);
		}

		[TestDate(2017, 08, 10)]
		public void TestMinutesMax()
		{
			AssertLocationInfoText("This device was last seen 59 minutes ago at (-33.857273, 151.215301).", TimeSpan.FromMinutes(59), -33.857273, 151.215301);
		}

		[TestDate(2017, 08, 10)]
		public void TestHour()
		{
			AssertLocationInfoText("This device was last seen 1 hour ago at (-33.858727, 151.213659).", TimeSpan.FromHours(1), -33.858727, 151.213659);
		}

		[TestDate(2017, 08, 10)]
		public void TestHoursMin()
		{
			AssertLocationInfoText("This device was last seen 2 hours ago at (-33.886313, 151.192844).", TimeSpan.FromHours(2), -33.886313, 151.192844);
		}

		[TestDate(2017, 08, 10)]
		public void TestHoursMax()
		{
			AssertLocationInfoText("This device was last seen 23 hours ago at (0, 0).", TimeSpan.FromHours(23), 0, 0);
		}

		[TestDate(2017, 08, 10)]
		public void TestDay()
		{
			AssertLocationInfoText("This device was last seen 1 day ago at (47.588969, -122.266178).", TimeSpan.FromDays(1), 47.588969, -122.266178);
		}

		[TestDate(2017, 08, 10)]
		public void TestDays()
		{
			AssertLocationInfoText("This device was last seen 2 days ago at (38.889288, -77.035008).", TimeSpan.FromDays(2), 38.889288, -77.035008);
		}

		void AssertLocationInfoText(string expectedText, TimeSpan age, double latitude, double longitude)
		{
			// Arrange
			base.SetUp();

			Factory.RefreshEnabled = false;
			var devicePk = SetUpDeviceAndLocation(age, latitude, longitude);

			// Act
			var text = GetLocationInfoText(Factory.Load<GlbDevice>(devicePk));

			// Assert
			AssertEquals(expectedText, text);
		}

		static ZGuid SetUpDeviceAndLocation(TimeSpan age, double latitude, double longitude)
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var device = factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = nameof(device.V3_HumanReadableIdentifier);
			device.V3_Model = nameof(device.V3_Model);
			var location = factory.New<GlbDeviceLocation>();
			location.V2_V3_Device = device.PK;
			location.V2_MeasurementTimeUtc = ZDateTime.Now.Add(-age);
			location.V2_Location = ZGeography.CreatePoint(longitude, latitude);

			var older = factory.New<GlbDeviceLocation>();
			older.V2_V3_Device = device.PK;
			older.V2_MeasurementTimeUtc = ZDateTime.Now.AddDays(-7);
			older.V2_Location = ZGeography.CreatePoint(1.234, 1.234);
			factory.Save();
			return device.PK;
		}

		static string GetLocationInfoText(GlbDevice device)
		{
			string text;

			using (var form = new GlbDeviceForm(device, withUIUpdateTimer: false))
			{
				text = form.LocationInfoText;
			}

			return text;
		}
	}
}
