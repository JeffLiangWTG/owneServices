using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.ServiceTask.AddressValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterData.ServiceTask.Test
{
	class UntransactionedBackgroundAddressValidationTaskTests : TestCase
	{
		[UseSnapshotProtection]
		public void TestCanCatchZSaveConcurrencyException_CantBeResolved_IsLogged()
		{
			var factory1 = new BusinessObjectFactory();

			var header = factory1.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			var companyData = header.CompanyData;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			factory1.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);
				//simulate e.g. the problem being in WhsOrder WD_WP. we're using a Strict column to make concurrency not resolvable
				var ocd1 = factory.Load<OrgCompanyData>(header.CompanyData.PK);
				ocd1.OB_APCategory = "YYY";
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var ocd2 = factory2.Load<OrgCompanyData>(header.CompanyData.PK);
				ocd2.OB_ARClientNumber = "182";
				factory2.Save();
			});

			string log = null;
			AssertNoExceptionThrown(() => { log = RunServiceTask(200, ValidationResultStatusCode.PointExact); });
			var factory3 = new BusinessObjectFactory();
			var address2 = factory3.Load<OrgAddress>(address.PK);
			var ocd3 = factory3.Load<OrgCompanyData>(header.CompanyData.PK);
			Assert(log, log.Contains("Warning|"));
			Assert(log, log.Contains("<Column>OB_ARClientNumber</Column><Original /><DB>182</DB><Changed /><Conflict>DB Changed</Conflict><Concurrency>Strict - Notify</Concurrency></ConcurrencyTableRow>"));
			AssertEquals("changes to OrgCompanyData weren't saved due to concurrency failure", "182", ocd3.OB_ARClientNumber);
			AssertNotEquals("changes to OrgCompanyData weren't saved due to concurrency failure", "YYY", ocd3.OB_APCategory);
			AssertEquals("changes to OrgAddress weren't saved due to concurrency failure", AddressValidationStatus.ToBeVerified, address2.OA_ValidationStatus);
		}

		string RunServiceTask(int expectedHttpStatusCode, string expectedValidationResultStatusCode, bool isServiceAvailable = true)
		{
			var logger = new TestServiceLogger();
			var httpResponseService = GetHttpResponseService(new Uri(localHost), @"{
  ""Items"": [
    {
      ""ValidationResultItem"": {
        ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
        ""AddressSourceTable"": ""E2"",
        ""Addressee"": null,
        ""Address1"": ""72 O'RIORDAN STREET"",
        ""Address2"": null,
        ""City"": ""ALEXANDRIA"",
        ""County"": null,
        ""State"": ""NSW"",
        ""Postcode"": 2015,
        ""PostcodeAddOn"": null,
        ""Country"": ""AU"",
        ""Apartment"": null,
        ""StreetNumber"": 72,
        ""Street"": ""O'RIORDAN STREET"",
        ""Latitude"": -33.91656513912593,
        ""Longitude"": 151.19541258,
        ""MatchCode"": ""S8HPNTSCZG"",
        ""MatchCodeFlag"": 4106,
        ""ChangeCount"": 0,
        ""LocationPrecision"": 16,
        ""ResultStatusCode"": """ + expectedValidationResultStatusCode + @""",
        ""ServiceType"": 1,
        ""QueryType"": 1,
        ""AvailableData"": 3,
        ""AddressType"": 8,
        ""ErrorMessage"": null
      },
      ""ProviderServiceCalls"": [],
      ""Suggestions"": [
        {
          ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
          ""AddressSourceTable"": ""E2"",
          ""Addressee"": null,
          ""Address1"": ""702 O Riordan St"",
          ""Address2"": null,
          ""City"": ""Alexandria"",
          ""County"": null,
          ""State"": ""NSW"",
          ""Postcode"": 2015,
          ""PostcodeAddOn"": null,
          ""Country"": ""AU"",
          ""Apartment"": null,
          ""ChangeCount"": 0,
          ""StreetNumber"": null,
          ""Street"": null,
          ""MatchCode"": null,
          ""MatchCodeFlag"": 0,
          ""LocationPrecision"": 0,
          ""ResultStatusCode"": null,
          ""ServiceType"": 0,
          ""QueryType"": 0,
          ""AvailableData"": 0,
          ""AddressType"": 0,
          ""ErrorMessage"": null,
          ""Group"": ""O Riordan St,,Alexandria,NSW,AU,2015"",
          ""Latitude"": 0,
          ""Longitude"": 0
        }
      ]
    }
  ]
}", expectedHttpStatusCode, isServiceAvailable);
			try
			{
				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();
			}
			catch (Exception ex)
			{
				throw new DeveloperNotificationException("Put Factory.Save() in a try/catch. This exception was caught by the top level exception reporter.", ex);
			}
			finally
			{
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}
			}

			return logger.ToString();
		}

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int httpStatusCode, bool isServiceAvailable = true)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "GET", "POST" },
				Processor = (uri, request) =>
				{
					if (string.IsNullOrEmpty(request))
					{
						return new Tuple<int, string>(200, @"{
  ""ServiceName"": ""WiseTechGlobal.AddressCleansing.Service"",
  ""ServiceVersion"": 2,
  ""ServiceAvailable"":" + isServiceAvailable.ToString().ToLower() + @",
  ""AssemblyVersion"": ""2.0.0.0"",
  ""ActiveProviders"": [
    ""PBO""
  ],
  ""DiagnosticMessage"": null
}");
					}
					return new Tuple<int, string>(httpStatusCode, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		IDisposable backgroundEndpointUri;
		readonly string localHost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";

		protected override void SetUp()
		{
			base.SetUp();

			backgroundEndpointUri = OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = localHost, new BusinessObjectFactory());

			Db.Connection.ExecuteNonQuery(@"UPDATE dbo.JobDocAddress SET E2_ValidationStatus = 'MAN'
WHERE E2_AddressOverride = 1 AND E2_ValidationStatus = 'NYV'

UPDATE dbo.OrgAddress SET OA_ValidationStatus = 'MAN'
WHERE OA_ValidationStatus = 'NYV'");
		}

		protected override void TearDown()
		{
			base.TearDown();

			backgroundEndpointUri.Dispose();
		}
	}

	[TestedType(typeof(BackgroundAddressValidationServiceTask))]
	class BackgroundAddressValidationTaskTests : ServiceTaskTestCase<BackgroundAddressValidationServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestInitialiseSchedule()
		{
			var testTask = new BackgroundAddressValidationServiceTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals(ZBool.True, taskSchedule.SST_Active);
			AssertEquals("6hours", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestHostedServiceAttribute()
		{
			var attribute = GetHostedServiceAttributes().SingleOrDefault();

			AssertNotNull(attribute);
			AssertEquals(false, attribute.AllowsMultipleInstances);
			AssertEquals(true, attribute.IsMandatory);
			AssertEquals(true, attribute.CanRunInAnyBranch);
			AssertEquals("BAV", attribute.Code);
			AssertEquals("Background Address Validation Service", attribute.Description);
		}

		public void TestVerifyNYVAddressOnly()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.Invalid);

			address.Reload();
			AssertEquals(AddressValidationStatus.ManuallyVerified, address.E2_ValidationStatus);
			AssertEquals(0, numberOfCallValidationService);
			AssertEquals(0, log.Length);
		}

		public void TestValidateJobDocAddress_INV()
		{
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ParentID = shipment.PK;
			address.E2_ParentTableCode = "JS";
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.E2_AddressType = "CRD";
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.Invalid);

			address.Reload();
			AssertEquals(AddressValidationStatus.Invalid, address.E2_ValidationStatus);

			var shipmentLogs = (shipment as BusinessObject).GetLogs();
			AssertAddressValidationEventLog(shipmentLogs, "AVS", "Address Validation Status", "|DEP=CRD|STA=INV|TYP=BAV", "Address Validation Status: Invalid, Type: Background Address Validation, Party: Consignor Documentary Address");
			AssertEquals($@"Debug|Background Address Validation batch has started, found 1 JobDocAddress(es).
{GetLogInformation(address)}
", log);
		}

		public void TestValidateJobDocAddress_UNV()
		{
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ParentID = shipment.PK;
			address.E2_ParentTableCode = "JS";
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.E2_AddressType = "CRD";
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.Error);

			address.Reload();
			AssertEquals(AddressValidationStatus.Unverifiable, address.E2_ValidationStatus);
			AssertNull((shipment as BusinessObject).GetLogs().GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.AddressValidationStatus.Code));
			AssertEquals($@"Debug|Background Address Validation batch has started, found 1 JobDocAddress(es).
Error|This address is unverifiable.
{GetLogInformation(address)}
", log);
		}

		public void TestVerifyNYVAndActiveAddressOnly()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = header1.MainAddress;
			address1.OA_IsActive = true;
			address1.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = header2.MainAddress;

			address2.OA_IsActive = false;
			address2.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			RunServiceTask(200, ValidationResultStatusCode.PointExact);

			address1.Reload();
			address2.Reload();
			AssertEquals(AddressValidationStatus.ManuallyVerified, address1.ValidationStatus);
			AssertEquals(AddressValidationStatus.ToBeVerified, address2.ValidationStatus);
			AssertEquals(0, numberOfCallValidationService);
		}

		public void TestValidateOrgAddress_INV()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.Invalid);

			address.Reload();
			AssertEquals(AddressValidationStatus.Invalid, address.OA_ValidationStatus);

			var headerLogs = address.Header.GetLogs();
			AssertAddressValidationEventLog(headerLogs, "AVS", "Address Validation Status", "|DEP=OrgAddress|STA=INV|TYP=BAV", "Address Validation Status: Invalid, Type: Background Address Validation, Party: Organization Address");
			AssertEquals($@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
{GetLogInformation(address)}
", log);
		}

		public void TestValidateOrgAddress_VAD()
		{
			ErrorReporter.Clear();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			using (EnvProxy.Instance.TemporaryServiceTaskContext("OTH_SERVICE", true))
			{
				var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

				address.Reload();
				AssertEquals(AddressValidationStatus.Verified, address.OA_ValidationStatus);

				var headerLogs = address.Header.GetLogs();
				AssertAddressValidationEventLog(headerLogs, "AVS", "Address Validation Status", "|DEP=OrgAddress|STA=VAD|TYP=BAV", "Address Validation Status: Valid, Type: Background Address Validation, Party: Organization Address");
				AssertEquals($@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
{GetLogInformation(address)}
", log);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestValidateOrgAddress_UNV()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.Error);

			address.Reload();
			AssertEquals(AddressValidationStatus.Unverifiable, address.OA_ValidationStatus);
			AssertNull(address.Header.GetLogs().GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.AddressValidationStatus.Code));
			AssertEquals($@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
Error|This address is unverifiable.
{GetLogInformation(address)}
", log);
		}

		public void TestValidateMultipleOrgAddressesAndJobDocAddressesWithBatchSize()
		{
			var jobDocAddressList = new List<JobDocAddress>();
			for (int i = 0; i < 16; i++)
			{
				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				jobDocAddress.E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
				jobDocAddressList.Add(jobDocAddress);
			}

			var orgAddressList = new List<OrgAddress>();
			for (int i = 0; i < 23; i++)
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = header.MainAddress;
				orgAddress.OA_IsActive = true;
				orgAddress.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
				orgAddressList.Add(orgAddress);
			}

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BackgroundValidationServiceTaskBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

				jobDocAddressList.ForEach(address =>
				{
					address.Reload();
					AssertEquals(AddressValidationStatus.Verified, address.E2_ValidationStatus);
					AssertContains(GetLogInformation(address), log);
				});

				orgAddressList.ForEach(address =>
				{
					address.Reload();
					AssertEquals(AddressValidationStatus.Verified, address.OA_ValidationStatus);
					AssertContains(GetLogInformation(address), log);
				});

				AssertEquals(39, numberOfCallValidationService);
				AssertEquals(2, Regex.Matches(log, @"Background Address Validation batch has started, found 10 OrgAddress\(es\).").Count);
				AssertEquals(1, Regex.Matches(log, @"Background Address Validation batch has started, found 3 OrgAddress\(es\).").Count);
				AssertEquals(1, Regex.Matches(log, @"Background Address Validation batch has started, found 10 JobDocAddress\(es\).").Count);
				AssertEquals(1, Regex.Matches(log, @"Background Address Validation batch has started, found 6 JobDocAddress\(es\).").Count);
			}
		}

		public void TestUnhandledExceptionWhenCallValidationService()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTaskWithUnhandledException();

			address.Reload();
			AssertEquals(3, numberOfCallValidationService);
			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
			AssertContains(@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident", log);
			AssertContains("Debug|BackgroundAddressValidationServiceTask.RunTask|System.AggregateException: One or more errors occurred.", log);
			AssertContains("Invalid character after parsing property name. Expected ':' but got: }. Path '', line 2, position 0.", log);
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestCanCatchZSaveConcurrencyException_CanBeResolved_IsSuccessful()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var add = factory2.Load<OrgAddress>(address.PK);
				add.OA_PostCode = "1828";
				factory2.Save();
			});

			AssertNoExceptionThrown(() => { RunServiceTask(200, ValidationResultStatusCode.PointExact); });
			var factory3 = new BusinessObjectFactory();
			var address2 = factory3.Load<OrgAddress>(address.PK);
			AssertEquals("change to OA_PostCode was merged", "1828", address2.OA_PostCode);
			AssertNotEquals("Validation was successful", AddressValidationStatus.ToBeVerified, address2.OA_ValidationStatus);
		}

		public void TestCanCatchZSaveConcurrencyException_CanNotBeResolved_LogErrorWithoutReport()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_OH = header.PK;
			address2.OA_Code = "#2";

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);

				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var addressFromFactory2 = factory2.Load<OrgAddress>(address.PK);
				addressFromFactory2.Delete();
				factory2.Save();

				var address2FromFactory = factory.Load<OrgAddress>(address2.PK);
				address2FromFactory.Delete();
			});

			var log = default(string);
			AssertNoExceptionThrown(() => { log = RunServiceTask(200, ValidationResultStatusCode.PointExact); });
			Assert(log, log.Contains("Warning|Error occurred when processing background address validation. If this warning persists please raise a CR4 incident and our support team will assist."));
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestCanCatchZCannotSaveExceptionButWillNotReportOnce()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var zCannotSaveException = new ZCannotSaveException("ZCannotSaveException", "");
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw zCannotSaveException);

			var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

			AssertContains("Warning|Error occurred when processing background address validation. If this warning persists please raise a CR4 incident and our support team will assist.|CargoWise.EntityFramework.ZCannotSaveException: ZCannotSaveException", log);
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestCanCatchSqlExceptionButWillNotReportOnce()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw SqlExceptionBuilder.CreateSqlException(10314, "This is a sql exception."));

			var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

#if NET
			AssertContains("Warning|Error occurred when processing background address validation. If this warning persists please raise a CR4 incident and our support team will assist.|Microsoft.Data.SqlClient.SqlException (0x80131904): This is a sql exception.", log);
#else
			AssertContains("Warning|Error occurred when processing background address validation. If this warning persists please raise a CR4 incident and our support team will assist.|System.Data.SqlClient.SqlException (0x80131904): This is a sql exception.", log);
#endif
			AssertNull(ErrorReporter.LastExceptionReported);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestCanCatchNonSaveExceptionAndWillReportOnce()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw new InvalidOperationException("Unknown exception"));

			RunServiceTask(200, ValidationResultStatusCode.PointExact);

			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Unknown exception", ErrorReporter.LastExceptionReported.Message);

			ErrorReporter.Clear();
		}

		public void TestRunServiceTaskWithoutLogger()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			RunServiceTaskWithoutLogger(200, ValidationResultStatusCode.PointExact, true);

			address.Reload();
			AssertEquals(AddressValidationStatus.Verified, address.OA_ValidationStatus);
		}

		public void TestDoesNotApplyWorkflowTemplate()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Test Template";
			template.P0_ProcessType = "ORG";

			var trigger = template.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "Test Trigger";
			using (trigger.TemporarilyAllowSettingCondition(nameof(ProcessTasksSchema.P9_SE_NKMilestoneEvent)))
			{
				trigger.P9_SE_NKMilestoneEvent = "AVS";
			}

			Factory.Save();

			RunServiceTaskWithoutLogger(200, ValidationResultStatusCode.PointExact, true);

			header.Reload();
			AssertEquals(false, header.Logs.GetAllLogs().Any(log => ((StmALog)log).SL_SE_NKEvent == "WTA"));
		}

		public void TestServiceTaskWithWebExceptionAndCanBeResolvedAfterRetry()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTaskWithWebExceptionAndExceptionCanBeResolvedOrNotAfterRetry(ValidationResultStatusCode.PointExact, 500, 2);

			address.Reload();
			AssertEquals(AddressValidationStatus.Verified, address.OA_ValidationStatus);
			AssertEquals(2, numberOfCallValidationService);
			AssertEquals($@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
{GetLogInformation(address)}
", log);
		}

		public void TestServiceTaskWithWebExceptionAndCanNotBeResolvedAfterRetry()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTaskWithWebExceptionAndExceptionCanBeResolvedOrNotAfterRetry(ValidationResultStatusCode.PointExact, 500, -1);

			address.Reload();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
			AssertEquals(3, numberOfCallValidationService);
			AssertEquals(@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.
The remote server returned an error: (500)
", log);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestServiceTaskWithWebExceptionLog()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = string.Empty;
			using (RawDataRegistry.Instance.AddressValidationWebServiceTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = "https://invalid.avsbackground.wisegrid.net/v2/", Factory))
			{
				log = RunServiceTask(200, ValidationResultStatusCode.Invalid);
			}

			CombineAssertions(() =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertContains("Debug|Received error message at", log);

#if NETFRAMEWORK
				AssertContains(@"Inner Exception1(Type: System.Net.Http.HttpRequestException):
An error occurred while sending the request.
Inner Exception2(Type: System.Net.WebException):
The remote name could not be resolved: 'invalid.avsbackground.wisegrid.net'", log);
#else
				AssertContains(@"Inner Exception1(Type: System.Net.Http.HttpRequestException):
No such host is known. (invalid.avsbackground.wisegrid.net:443)
Inner Exception2(Type: System.Net.Sockets.SocketException):
No such host is known.", log);
#endif
				AssertContains("Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.", log);
			});
		}

		public void TestServiceTaskWithSocketExceptionLog()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var logger = new TestServiceLogger();
			var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
			serviceTask.RunTask();

			var log = logger.ToString();
			CombineAssertions(() =>
			{
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertContains("Debug|Received error message at", log);
#if NETFRAMEWORK
				AssertContains(@"Inner Exception1(Type: System.Net.Http.HttpRequestException):
An error occurred while sending the request.
Inner Exception2(Type: System.Net.WebException):
Unable to connect to the remote server
Inner Exception3(Type: System.Net.Sockets.SocketException):
No connection could be made because the target machine actively refused it", log);
#else
				AssertContains(@"Inner Exception1(Type: System.Net.Http.HttpRequestException):
No connection could be made because the target machine actively refused it", log);
				AssertContains(@"Inner Exception2(Type: System.Net.Sockets.SocketException):
No connection could be made because the target machine actively refused it", log);
#endif
				AssertContains("Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.", log);
			});
		}

		public void TestServiceTaskWithTaskCanceledExceptionLog()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;
			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			using (RawDataRegistry.Instance.AddressValidationWebServiceTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = localHost, Factory))
			{
				var httpResponseService = new HttpServiceForTest
				{
					Delay = 1100,
					Methods = new string[] { "GET", "POST" },
					Processor = (uri, request) =>
					{
						return new Tuple<int, string>(200, @"{
  ""ServiceName"": ""WiseTechGlobal.AddressCleansing.Service"",
  ""ServiceVersion"": 2,
  ""ServiceAvailable"": ""true"",
  ""AssemblyVersion"": ""2.0.0.0"",
  ""ActiveProviders"": [
    ""PBO""
  ],
  ""DiagnosticMessage"": null
}");
					},
					Uri = new Uri(localHost + "GetServiceStatus" + "/"),
					ContentType = "application/json"
				};

				var logger = new TestServiceLogger();

				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}

				var log = logger.ToString();
				CombineAssertions(() =>
				{
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					AssertNull(ErrorReporter.LastExceptionReported);
					AssertContains("Debug|Received error message at", log);
					AssertContains(@"(Type: System.Threading.Tasks.TaskCanceledException)", log);
					AssertContains("Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.", log);
				});
			}
		}

		public void TestValidateOrgAddress_WithZeroAddresses()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

			address.Reload();
			AssertEquals(AddressValidationStatus.ManuallyVerified, address.OA_ValidationStatus);

			AssertEquals(0, log.Length);
		}

		public void TestValidateJobDocAddress_WithZeroAddresses()
		{
			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_ParentID = shipment.PK;
			address.E2_ParentTableCode = "JS";
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			address.E2_AddressType = "CRD";
			Factory.Save();

			var log = RunServiceTask(200, ValidationResultStatusCode.PointExact);

			address.Reload();
			AssertEquals(AddressValidationStatus.ManuallyVerified, address.E2_ValidationStatus);

			AssertEquals(0, log.Length);
		}

		public void TestValidateOrgAddress_WebServiceNotAvailable()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			var log = RunServiceTask(200, string.Empty, false);

			address.Reload();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
			AssertEquals(@"Warning|Address Validation services are unavailable at the moment, please try again later. If the issue persists please raise a Customer Service Incident.
", log);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestValidateOrgAddressBadRequest()
		{
			ErrorReporter.Clear();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			using (EnvProxy.Instance.TemporaryServiceTaskContext("OTH_SERVICE", true))
			{
				var log = RunServiceTaskCore(400, "This is a bad request", true);

				address.Reload();
				AssertEquals(AddressValidationStatus.Invalid, address.OA_ValidationStatus);
				AssertEquals($@"Debug|Background Address Validation batch has started, found 1 OrgAddress(es).
Debug|The remote server returned an error: (400)
This is a bad request

{GetLogInformation(address)}
", log);
				AssertEquals("Error occurred when ProcessAddressValidationResponse", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestValidateOrgAddressNotAuthorized()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var address = header.MainAddress;

			address.OA_IsActive = true;
			address.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
			Factory.Save();

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.EnableSystemToSystemTrustAuthentication = true, Factory))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("OTH_SERVICE", true))
			{
				var log = RunServiceTaskCore(200, "This is a not authorized request", true);
				address.Reload();
				AssertEquals(AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
				AssertContains(@"Warning|Authorization failed. If the issue persists, please raise a Customer Service Incident.", log);
			}
		}

		#region Implementation

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						OrgAddressSchema.Constants.TableName,
						BackgroundAddressValidationServiceTask.FriendlyName + " - " + OrgAddressSchema.Constants.TableName,
						OrgAddressSchema.Constants.OA_IsActive + "=Y",
						OrgAddressSchema.Constants.OA_ValidationStatus + "=" + AddressValidationStatus.ToBeVerified),

					new TaskNudgeInformationForTest(
						JobDocAddressSchema.Constants.TableName,
						BackgroundAddressValidationServiceTask.FriendlyName + " - " + JobDocAddressSchema.Constants.TableName,
						JobDocAddressSchema.Constants.E2_ValidationStatus + "=" + AddressValidationStatus.ToBeVerified),
				};
			}
		}

		string GetLogInformation(ISupportWebAddressValidation address)
		{
			var tableName = address.IsJobDocAddress ? JobDocAddressSchema.Constants.TableName : OrgAddressSchema.Constants.TableName;
			return $@"Debug|Address Type:{tableName}
PK: {address.EntityPK}
Line 1: {address.Address1}
Line 2: {address.Address2}
City: {address.City}
State: {address.State}
Postcode: {address.Postcode}
Country: {address.CountryCodeISO2}
New Status: {address.ValidationStatus}
";
		}

		void AssertAddressValidationEventLog(Logs logs, string expectedType, string expectedDescription, string expectedReference, string expectedEventDetail)
		{
			var log = logs.GetAllLogs().Cast<StmALog>().Single(x => x.SL_SE_NKEvent == AutoEvents.AddressValidationStatus.Code);
			CombineAssertions(() =>
			{
				AssertEquals(expectedType, log.Event.SE_Code);
				AssertEquals(expectedDescription, log.Event.SE_Desc);
				AssertEquals(expectedReference, log.SL_Reference);
				AssertEquals(expectedEventDetail, log.DisplayEventReference);
			});
		}

		string RunServiceTask(int expectedHttpStatusCode, string expectedValidationResultStatusCode, bool isServiceAvailable = true)
		{
			var expectedResponse = @"{
  ""Items"": [
    {
      ""ValidationResultItem"": {
        ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
        ""AddressSourceTable"": ""E2"",
        ""Addressee"": null,
        ""Address1"": ""72 O'RIORDAN STREET"",
        ""Address2"": null,
        ""City"": ""ALEXANDRIA"",
        ""County"": null,
        ""State"": ""NSW"",
        ""Postcode"": 2015,
        ""PostcodeAddOn"": null,
        ""Country"": ""AU"",
        ""Apartment"": null,
        ""StreetNumber"": 72,
        ""Street"": ""O'RIORDAN STREET"",
        ""Latitude"": -33.91656513912593,
        ""Longitude"": 151.19541258,
        ""MatchCode"": ""S8HPNTSCZG"",
        ""MatchCodeFlag"": 4106,
        ""ChangeCount"": 0,
        ""LocationPrecision"": 16,
        ""ResultStatusCode"": """ + expectedValidationResultStatusCode + @""",
        ""ServiceType"": 1,
        ""QueryType"": 1,
        ""AvailableData"": 3,
        ""AddressType"": 8,
        ""ErrorMessage"": null
      },
      ""ProviderServiceCalls"": [],
      ""Suggestions"": [
        {
          ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
          ""AddressSourceTable"": ""E2"",
          ""Addressee"": null,
          ""Address1"": ""702 O Riordan St"",
          ""Address2"": null,
          ""City"": ""Alexandria"",
          ""County"": null,
          ""State"": ""NSW"",
          ""Postcode"": 2015,
          ""PostcodeAddOn"": null,
          ""Country"": ""AU"",
          ""Apartment"": null,
          ""ChangeCount"": 0,
          ""StreetNumber"": null,
          ""Street"": null,
          ""MatchCode"": null,
          ""MatchCodeFlag"": 0,
          ""LocationPrecision"": 0,
          ""ResultStatusCode"": null,
          ""ServiceType"": 0,
          ""QueryType"": 0,
          ""AvailableData"": 0,
          ""AddressType"": 0,
          ""ErrorMessage"": null,
          ""Group"": ""O Riordan St,,Alexandria,NSW,AU,2015"",
          ""Latitude"": 0,
          ""Longitude"": 0
        }
      ]
    }
  ]
}";
			return RunServiceTaskCore(expectedHttpStatusCode, expectedResponse, isServiceAvailable);
		}

		string RunServiceTaskCore(int expectedHttpStatusCode, string expectedResponse, bool isServiceAvailable)
		{
			var logger = new TestServiceLogger();
			var httpResponseService = GetHttpResponseService(new Uri(localHost), expectedResponse, expectedHttpStatusCode, isServiceAvailable);
			try
			{
				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();
			}
			catch (Exception ex)
			{
				throw new DeveloperNotificationException(
					"Put Factory.Save() in a try/catch. This exception was caught by the top level exception reporter.", ex);
			}
			finally
			{
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}
			}

			return logger.ToString();
		}

		string RunServiceTaskWithUnhandledException()
		{
			var logger = new TestServiceLogger();
			var httpResponseService = GetHttpResponseService(new Uri(localHost), @"{Error
}", 200);
			try
			{
				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();
			}
			catch
			{
				// ignored
			}
			finally
			{
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}
			}

			return logger.ToString();
		}

		string RunServiceTaskWithWebExceptionAndExceptionCanBeResolvedOrNotAfterRetry(string expectedValidationResultStatusCode, int errorHttpStatusCode, int tryCountOfSolvingException)
		{
			var logger = new TestServiceLogger();
			var httpResponseService = GetHttpResponseServiceWhenRetryCanSolveProblemOrNot(new Uri(localHost), @"{
  ""Items"": [
    {
      ""ValidationResultItem"": {
        ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
        ""AddressSourceTable"": ""E2"",
        ""Addressee"": null,
        ""Address1"": ""72 O'RIORDAN STREET"",
        ""Address2"": null,
        ""City"": ""ALEXANDRIA"",
        ""County"": null,
        ""State"": ""NSW"",
        ""Postcode"": 2015,
        ""PostcodeAddOn"": null,
        ""Country"": ""AU"",
        ""Apartment"": null,
        ""StreetNumber"": 72,
        ""Street"": ""O'RIORDAN STREET"",
        ""Latitude"": -33.91656513912593,
        ""Longitude"": 151.19541258,
        ""MatchCode"": ""S8HPNTSCZG"",
        ""MatchCodeFlag"": 4106,
        ""ChangeCount"": 0,
        ""LocationPrecision"": 16,
        ""ResultStatusCode"": """ + expectedValidationResultStatusCode + @""",
        ""ServiceType"": 1,
        ""QueryType"": 1,
        ""AvailableData"": 3,
        ""AddressType"": 8,
        ""ErrorMessage"": null
      },
      ""ProviderServiceCalls"": [],
      ""Suggestions"": [
        {
          ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
          ""AddressSourceTable"": ""E2"",
          ""Addressee"": null,
          ""Address1"": ""702 O Riordan St"",
          ""Address2"": null,
          ""City"": ""Alexandria"",
          ""County"": null,
          ""State"": ""NSW"",
          ""Postcode"": 2015,
          ""PostcodeAddOn"": null,
          ""Country"": ""AU"",
          ""Apartment"": null,
          ""ChangeCount"": 0,
          ""StreetNumber"": null,
          ""Street"": null,
          ""MatchCode"": null,
          ""MatchCodeFlag"": 0,
          ""LocationPrecision"": 0,
          ""ResultStatusCode"": null,
          ""ServiceType"": 0,
          ""QueryType"": 0,
          ""AvailableData"": 0,
          ""AddressType"": 0,
          ""ErrorMessage"": null,
          ""Group"": ""O Riordan St,,Alexandria,NSW,AU,2015"",
          ""Latitude"": 0,
          ""Longitude"": 0
        }
      ]
    }
  ]
}", errorHttpStatusCode, tryCountOfSolvingException);
			try
			{
				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();
			}
			catch
			{
				// ignored
			}
			finally
			{
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}
			}

			return logger.ToString();
		}

		void RunServiceTaskWithoutLogger(int expectedHttpStatusCode, string expectedValidationResultStatusCode, bool isServiceAvailable = true)
		{
			var httpResponseService = GetHttpResponseService(new Uri(localHost), @"{
  ""Items"": [
    {
      ""ValidationResultItem"": {
        ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
        ""AddressSourceTable"": ""E2"",
        ""Addressee"": null,
        ""Address1"": ""72 O'RIORDAN STREET"",
        ""Address2"": null,
        ""City"": ""ALEXANDRIA"",
        ""County"": null,
        ""State"": ""NSW"",
        ""Postcode"": 2015,
        ""PostcodeAddOn"": null,
        ""Country"": ""AU"",
        ""Apartment"": null,
        ""StreetNumber"": 72,
        ""Street"": ""O'RIORDAN STREET"",
        ""Latitude"": -33.91656513912593,
        ""Longitude"": 151.19541258,
        ""MatchCode"": ""S8HPNTSCZG"",
        ""MatchCodeFlag"": 4106,
        ""ChangeCount"": 0,
        ""LocationPrecision"": 16,
        ""ResultStatusCode"": """ + expectedValidationResultStatusCode + @""",
        ""ServiceType"": 1,
        ""QueryType"": 1,
        ""AvailableData"": 3,
        ""AddressType"": 8,
        ""ErrorMessage"": null
      },
      ""ProviderServiceCalls"": [],
      ""Suggestions"": [
        {
          ""AddressRecordGUID"": ""b681b902-685a-4afc-85e2-c312c0b29f4b"",
          ""AddressSourceTable"": ""E2"",
          ""Addressee"": null,
          ""Address1"": ""702 O Riordan St"",
          ""Address2"": null,
          ""City"": ""Alexandria"",
          ""County"": null,
          ""State"": ""NSW"",
          ""Postcode"": 2015,
          ""PostcodeAddOn"": null,
          ""Country"": ""AU"",
          ""Apartment"": null,
          ""ChangeCount"": 0,
          ""StreetNumber"": null,
          ""Street"": null,
          ""MatchCode"": null,
          ""MatchCodeFlag"": 0,
          ""LocationPrecision"": 0,
          ""ResultStatusCode"": null,
          ""ServiceType"": 0,
          ""QueryType"": 0,
          ""AvailableData"": 0,
          ""AddressType"": 0,
          ""ErrorMessage"": null,
          ""Group"": ""O Riordan St,,Alexandria,NSW,AU,2015"",
          ""Latitude"": 0,
          ""Longitude"": 0
        }
      ]
    }
  ]
}", expectedHttpStatusCode, isServiceAvailable);
			try
			{
				httpResponseService.Start();
				var serviceTask = new BackgroundAddressValidationServiceTask();
				serviceTask.RunTask();
			}
			catch
			{
				// ignored
			}
			finally
			{
				if (httpResponseService.IsStarted)
				{
					httpResponseService.Stop();
				}
			}
		}

		int numberOfCallValidationService;

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int httpStatusCode, bool isServiceAvailable = true)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "GET", "POST" },
				Processor = (uri, request) =>
				{
					if (string.IsNullOrEmpty(request))
					{
						return new Tuple<int, string>(200, @"{
  ""ServiceName"": ""WiseTechGlobal.AddressCleansing.Service"",
  ""ServiceVersion"": 2,
  ""ServiceAvailable"":" + isServiceAvailable.ToString().ToLower() + @",
  ""AssemblyVersion"": ""2.0.0.0"",
  ""ActiveProviders"": [
    ""PBO""
  ],
  ""DiagnosticMessage"": null
}");
					}

					numberOfCallValidationService++;
					return new Tuple<int, string>(httpStatusCode, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		HttpServiceForTest GetHttpResponseServiceWhenRetryCanSolveProblemOrNot(Uri serviceUrl, string response, int errorHttpStatusCode, int tryCountOfSolvingException)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "GET", "POST" },
				Processor = (uri, request) =>
				{
					if (string.IsNullOrEmpty(request))
					{
						return new Tuple<int, string>(200, @"{
  ""ServiceName"": ""WiseTechGlobal.AddressCleansing.Service"",
  ""ServiceVersion"": 2,
  ""ServiceAvailable"": true,
  ""AssemblyVersion"": ""2.0.0.0"",
  ""ActiveProviders"": [
    ""PBO""
  ],
  ""DiagnosticMessage"": null
}");
					}

					numberOfCallValidationService++;
					if (tryCountOfSolvingException == -1)
					{
						return new Tuple<int, string>(errorHttpStatusCode, response);
					}

					if (numberOfCallValidationService == tryCountOfSolvingException)
					{
						return new Tuple<int, string>(200, response);
					}

					return new Tuple<int, string>(errorHttpStatusCode, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		IDisposable backgroundEndpointUri;
		readonly string localHost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";

		protected override void SetUpCore()
		{
			base.SetUpCore();

			backgroundEndpointUri = OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = localHost, Factory);

			numberOfCallValidationService = 0;
			Db.Connection.ExecuteNonQuery(@"UPDATE dbo.JobDocAddress SET E2_ValidationStatus = 'MAN'
WHERE E2_AddressOverride = 1 AND E2_ValidationStatus = 'NYV'

UPDATE dbo.OrgAddress SET OA_ValidationStatus = 'MAN'
WHERE OA_ValidationStatus = 'NYV'");
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			backgroundEndpointUri.Dispose();
		}

		#endregion
	}
}
