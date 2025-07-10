using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(TelematicsConfigurationRegistry))]
	class TelematicsConfigurationRegistryTest : RegistryItemSetTestCaseWithFactory<TelematicsConfigurationRegistry>
	{
		public void TestOverspeedAlert()
		{
			AssertEquals("OverspeedAlert", ItemSet.OverspeedAlert.Name);
			AssertEquals("Telematics/Vehicle Speed Settings", ItemSet.OverspeedAlert.Category);
			AssertEquals("Overspeed Alert", ItemSet.OverspeedAlert.Caption);
			AssertMultilineASCIIEquals(@"By default, the alert will be displayed for 60 minutes. Minutes can be changed to any value greater than 0. There are two other options available, never show the alert or always show the alert.", ItemSet.OverspeedAlert.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.OverspeedAlert.Storage);
			AssertEquals(RegistryOptions.Default, ItemSet.OverspeedAlert.Options);
			AssertEquals(OverspeedAlertOptions.Default.Code, ItemSet.OverspeedAlert.DefaultValue.OverspeedAlertType);
		}

		public void TestOverspeedThresholdValue()
		{
			TestRegistryItem(
				ItemSet.OverspeedThresholdValue,
				"OverspeedThresholdValue",
				"Telematics/Vehicle Speed Settings",
				"Threshold for Overspeeding",
				"If the difference between current speed and speed limit data exceeds the threshold value set in the registry, then an overspeed condition has occurred. The value will be in km/h.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5,
				0,
				int.MaxValue);
		}

		public void TestNumberOfYearsToKeepData()
		{
			TestRegistryItem(
				ItemSet.NumberOfYearsToKeepData,
				"NumberOfYearsToKeepData",
				"Telematics/Data Retention Settings",
				"Number of Years of Device Data to Keep",
				"Obsolete device data is periodically removed from the database. This setting allows you to configure the number of years of data you wish to keep in the database. " +
				"This always excludes the current calendar year i.e. if the value chosen is 1, then all data from before January 1st of the previous calendar year will be marked for deletion.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				5,
				1,
				100);
		}

		public void TestBatchSizeForObsoleteDataCleanServiceTask()
		{
			TestRegistryItem(
				ItemSet.BatchSize,
				"BatchSize",
				"Telematics/Data Retention Settings",
				"Batch Size for Obsolete Data Removal",
				"In order to maintain efficient operation while deleting obsolete device data from the database, only a small number of items will be deleted at once. Here you can configure the number of items to be deleted each time the service task is run",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10000,
				100,
				100000
				);
		}

		public void TestNumberOfBatchesDeletedPerRun()
		{
			TestRegistryItem(
				ItemSet.NumberOfBatchesDeletedPerRun,
				"NumberOfBatchesDeletedPerRun",
				"Telematics/Data Retention Settings",
				"Number of Batches of Obsolete Data Deleted Per Service Task Run",
				"The number of batches of obsolete data to remove each time the obsolete service task runs. Setting this value too high may result in the obsolete data removal service task taking extended periods of time to run.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1000,
				1,
				10000
				);
		}

		public void TestRimMaximumDataBatchSize()
		{
			TestRegistryItem(
				ItemSet.RimMaximumDataBatchSize,
				"RimMaximumDataBatchSize",
				"Telematics/Transport Certification Authority Data Settings",
				"Number of RIM records provided to TCA Per batch",
				"The number of data records to send to the TCA (Transport Certification Authority) REST endpoint per batch",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10000,
				1,
				1000000);
		}

		public void TestTcaRimUsername()
		{
			TestRegistryItem(
				ItemSet.TcaRimUsername,
				"TcaRimUsername",
				"Telematics/Transport Certification Authority Data Settings/Login Details",
				"Telematics RIM Authentication Details - Username",
				"The username used for accessing the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"tdewtg2");
		}

		public void TestTcaRimPassword()
		{
			TestRegistryItem(
				ItemSet.TcaRimPassword,
				"TcaRimPassword",
				"Telematics/Transport Certification Authority Data Settings/Login Details",
				"Telematics RIM Authentication Details - Password",
				"The password used for accessing the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Password,
				"5K#uR7rsT%mFV^T");
		}

		public void TestTcaRimUrl()
		{
			TestRegistryItem(
				ItemSet.TcaRimUrl,
				"TcaRimUrl",
				"Telematics/Transport Certification Authority Data Settings/Login Details",
				"Telematics RIM Authentication Details - URL",
				"The URL of the Transport Certification Authorities (TCA) Remote Infrastructure Management (RIM) REST API",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.Url,
				"https://livetde.tca.gov.au/rest/");
		}

		public void TestXtRecipient()
		{
			TestRegistryItem(
				ItemSet.XtRecipient,
				"XtRecipient",
				"Telematics/Transport Certification Authority Data Settings",
				"XT Recipient for TCA Rim messages",
				"The identifier used to route TCA Rim messages through eHub to the XT service",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				"XHUB_Telematics_RIM");
		}

		public void TestTelematicsEmailNotificationGroup()
		{
			TestRegistryItem(
				ItemSet.TelematicsChecklistEmailNotificationGroup,
				"TelematicsChecklistEmailNotificationGroup",
				"Telematics/Email Notification Group",
				"Pre-Drive Checklist Email Notification Group",
				"The staff group that will be receiving the exception report after a failed Pre-Drive Checklist.",
				RegistryStorageFlags.System,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestPublicPrivateRoadEndpointRecordBatchSize()
		{
			TestRegistryItem(
				ItemSet.PublicPrivateRoadEndpointRecordBatchSize,
				"PublicPrivateRoadEndpointRecordBatchSize",
				"Telematics/Land Parcel Settings",
				"Public Private Processor Batch Size",
				"The number of records processed per batch for public/private land status.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				1000,
				1,
				1000000);
		}

		public void TestPublicPrivateRoadEndpointTimeoutInSeconds()
		{
			TestRegistryItem(
				ItemSet.PublicPrivateRoadEndpointTimeoutInSeconds,
				"PublicPrivateRoadEndpointTimeoutInSeconds",
				"Telematics/Land Parcel Settings",
				"Public Private Processor Endpoint Timeout",
				"The timeout for REST queries made to the Land Parcel endpoint.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				30,
				1,
				300);
		}

		public void TestPublicPrivateRoadEndpointTimeout()
		{
			CombineAssertions(() =>
			{
				Test(1, TimeSpan.FromSeconds(1));
				Test(10, TimeSpan.FromSeconds(10));
				Test(100, TimeSpan.FromSeconds(100));
			});

			void Test(int value, TimeSpan expected)
			{
				using (TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointTimeoutInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					AssertEquals(expected, TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointTimeout);
				}
			}
		}

		public void TestProcessGpsDataOnRoadType()
		{
			TestRegistryItem(
				ItemSet.ProcessGlobalPositionDataOnRoadType,
				"ProcessGlobalPositionDataOnRoadType",
				"Telematics/Land Parcel Settings",
				"Process Global Positioning Data On Road Type",
				"GPS locations will be marked to calculate if position is on public or private roads.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}
	}
}
