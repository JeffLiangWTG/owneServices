using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ExternalValidationServiceClientTest : TestCaseWithFactory
	{
		public void TestCausesThreadErrors()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var testClient = new ExternalValidationServiceClient(testOrganization.PK);

			var hasException = false;
			var resetEvent = new AutoResetEvent(false);

			var thread = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						var result = testClient.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}

					hasException = false;
				}
				catch
				{
					hasException = true;
				}
				finally
				{
					resetEvent.Set();
				}
			});

			thread.Start();

			resetEvent.WaitOne();

			AssertEquals("Expect no exception thrown from background thread", false, hasException);
		}

		[UseSnapshotProtection]
		public void TestNoCrossThreadAccessException()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var serviceUri = new Uri($"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/");
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 10000,
				Methods = new[] { "POST" },
				Uri = serviceUri
			};

			var validationResult = default(ExternalValidationResult);
			var resetEvent = new AutoResetEvent(false);

			try
			{
				testExternalValidationService.Start();
				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK)
				{
					ServiceUri = serviceUri,
					ServiceTimeout = 1000
				};

				AssertNoExceptionThrown(() => client.AddExternalValidationEvent(AutoEvents.ExternalValidationNotCompleted, "test"));

				Task.Factory.StartNew(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							validationResult = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
						}
					}
					finally
					{
						resetEvent.Set();
					}
				});

				resetEvent.WaitOne();
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}

			AssertNotNull(validationResult);
			AssertEquals(ErrorSource.Timeout, validationResult.ErrorSource);
		}

		public void TestResultIsInvalidWithWarningsAndErrors()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var serviceUri = new Uri(String.Format("http://localhost:{0}/", HttpServiceForTest.GetFreeTcpPort()));
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, __) => new Tuple<int, string>(200, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<OrgStatus xmlns=""http://www.wisetechglobal.com/EnterpriseService/"">
	<Result>Invalid</Result>
	<ErrorLog>
		<ErrorItem>String</ErrorItem>
		<ErrorItem>String</ErrorItem>
		<ErrorItem>String</ErrorItem>
	</ErrorLog>
	<WarningLog>
		<WarningItem>String</WarningItem>
		<WarningItem>String</WarningItem>
		<WarningItem>String</WarningItem>
	</WarningLog>
</OrgStatus>
"),
				Uri = serviceUri
			};

			try
			{
				testExternalValidationService.Start();
				Assert(string.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);

				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK) { ServiceUri = serviceUri };

				var result = default(ExternalValidationResult);
				Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}
				}).Wait();

				AssertNotNull(result);
				Assert(!result.IsValid);
				AssertEquals(3, result.Warnings.Length);
				AssertEquals(3, result.Errors.Length);
				AssertEquals(ErrorSource.External, result.ErrorSource);
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}
		}

		public void TestResultIsValid()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var serviceUri = new Uri(String.Format("http://localhost:{0}/", HttpServiceForTest.GetFreeTcpPort()));
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<OrgStatus xmlns=""http://www.wisetechglobal.com/EnterpriseService/"">
	<Result>Valid</Result>
	<ErrorLog />
	<WarningLog />
</OrgStatus>
"),
				Uri = serviceUri
			};

			try
			{
				testExternalValidationService.Start();
				Assert(string.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);

				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK) { ServiceUri = serviceUri };

				var result = default(ExternalValidationResult);
				Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}
				}).Wait();

				AssertNotNull(result);
				Assert(result.IsValid);
				AssertEquals(0, result.Warnings.Length);
				AssertEquals(0, result.Errors.Length);
				AssertEquals(ErrorSource.None, result.ErrorSource);
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}
		}

		public void TestTimeout()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var serviceUri = new Uri(String.Format("http://localhost:{0}/", HttpServiceForTest.GetFreeTcpPort()));
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 10000,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<OrgStatus xmlns=""http://www.wisetechglobal.com/EnterpriseService/"">
	<Result>Valid</Result>
	<ErrorLog />
	<WarningLog />
</OrgStatus>
"),
				Uri = serviceUri
			};

			try
			{
				testExternalValidationService.Start();
				Assert(string.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);

				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK) { ServiceUri = serviceUri, ServiceTimeout = 1000 };

				var result = default(ExternalValidationResult);
				Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}
				}).Wait();

				AssertNotNull(result);
				AssertEquals(0, result.Warnings.Length);
				AssertEquals(1, result.Errors.Length);
				AssertEquals(ErrorSource.Timeout, result.ErrorSource);
				AssertEquals("A timeout occurred during the validation with external web service.", result.Errors.Single());
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}
		}

		public void TestValidate_WhenGettingMalformedXml_ShouldAddErrorMessage()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var serviceUri = new Uri($"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/");
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"<?xml version=""1.0"" encoding=""UTF-8""?>
						<Potato>
							<Skin />
							<Potate />
						</Potato>"),
				Uri = serviceUri
			};

			try
			{
				testExternalValidationService.Start();

				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK) { ServiceUri = serviceUri };

				var result = default(ExternalValidationResult);
				Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						result = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}
				}).Wait();

				AssertNotNull(result);
				Assert(result.HasErrors);
				AssertEquals(ErrorSource.Other, result.ErrorSource);
				AssertEquals("The returned data from the external validation service is corrupted.", result.Errors.Single());
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestUriFormatExceptionCatch()
		{
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"<?xml version=""1.0"" encoding=""UTF-8""?>
					<OrgStatus xmlns=""http://www.wisetechglobal.com/EnterpriseService/"">
						<Result>Valid</Result>
						<ErrorLog />
						<WarningLog />
					</OrgStatus>"),
				Uri = null
			};

			try
			{
				testExternalValidationService.Start();
				Assert("The test external validation service cannot be started.", testExternalValidationService.IsStarted);

				var testOrganization = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				var client = new ExternalValidationServiceClient(testOrganization.PK) { ServiceUri = null };

				var result = default(ExternalValidationResult);
				Task.Factory.StartNew(() =>
				{
					using (RawDataRegistry.Instance.ExternalValidationServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
					using (Db.DisposableActionForDbConnection())
					{
						result = client.ValidateAsync(new BusinessObjectFactory(), CancellationToken.None).Result;
					}
				}).Wait();

				Assert(result.HasErrors);
				AssertEquals(ErrorSource.Other, result.ErrorSource);
				AssertEquals(
					"Invalid external validation web service URL. Please see Maintain -> System -> Registry -> Organizations -> External Validation Service -> External Validation Service URL, should you wish to edit the URL.",
					result.Errors.Single());
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
			}
		}
	}
}
