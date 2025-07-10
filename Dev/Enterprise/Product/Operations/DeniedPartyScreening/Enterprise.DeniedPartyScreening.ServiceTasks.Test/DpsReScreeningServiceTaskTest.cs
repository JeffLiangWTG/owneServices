using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Business.Test;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DeniedPartyScreening.ServiceTasks.Test
{
	[TestedType(typeof(DpsRescreeningServiceTask))]
	public class DpsRescreeningServiceTaskTest : ServiceTaskTestCase<DpsRescreeningServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceCanRunInAnyBranch()
		{
			AssertEquals("CanRunInAnyBranch", true, GetHostedServiceAttributes().Single().CanRunInAnyBranch);
		}

		#region Re-Screening

		[TestDate(2021, 1, 1)]
		public void TestReScreening_Organization()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var itemPKs = new[] { Guid.NewGuid() };
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[0], ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var log = $@"Information|Received 1 Rescreening advice(s)
Information|Rescreen batch has started processing 1 record(s).
Debug|Processing OrgHeader - {header.PK}.
Information|Rescreen batch processed: Reset 1 Org and 0 Vessel record(s).
Information|Denied Party Rescreen Advice service task completed successfully: Processed 1 rescreen advice(s), reset 1 record(s).
";
			var stmEntityScreeningLog = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Single();
			var authorizationHeader = httpService.Headers["Authorization"].Split(' ');
			var authorizationtoken = new AuthenticationHeaderValue(authorizationHeader[0], authorizationHeader[1]);

			header.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, header.OH_ScreeningStatus);
				AssertEquals(log, serviceTask.ServiceLogger.ToString());
				AssertEquals(ZDateTime.UtcNow, stmEntityScreeningLog.PJ_SystemCreateTimeUtc);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate, stmEntityScreeningLog.PJ_Status);
				AssertEquals(User.ServiceUserCode, stmEntityScreeningLog.PJ_SystemCreateUser);
				AssertEquals(header.PK, stmEntityScreeningLog.PJ_SourceID);
				AssertEquals(header.TablePrefix, stmEntityScreeningLog.PJ_SourceTableCode);
				AssertContainsExactElementsInAnyOrder(itemPKs, DeleteRequestItemPks);
				AssertEquals(HMACSHA256Helper.GetComputedLicenceCode(GlbBranch.GetOneActiveBranchPerCompany().First().Company.GetLicenceKeyIdentifier("-")), httpService.Headers["LicenceCode"]);
				AssertNotNull(Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "ServiceTaskDPR_LastSuccessfulRuntime")));
				AssertEquals("Bearer", authorizationtoken.Scheme);
				AssertNotNullOrEmpty(authorizationtoken.Parameter);
			});
		}

		[TestDate(2021, 1, 1)]
		public void TestReScreening_Vessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Code1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = vessel.PK.ToGuid(), TypeOfEntity = vessel.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var log = $@"Information|Received 1 Rescreening advice(s)
Information|Rescreen batch has started processing 1 record(s).
Debug|Processing RefVessel - {vessel.PK}.
Information|Rescreen batch processed: Reset 0 Org and 1 Vessel record(s).
Information|Denied Party Rescreen Advice service task completed successfully: Processed 1 rescreen advice(s), reset 1 record(s).
";
			var stmEntityScreeningLog = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, vessel.PK)).Single();

			vessel.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, vessel.RV_ScreeningStatus);
				AssertEquals(log, serviceTask.ServiceLogger.ToString());
				AssertEquals(ZDateTime.UtcNow, stmEntityScreeningLog.PJ_SystemCreateTimeUtc);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate, stmEntityScreeningLog.PJ_Status);
				AssertEquals(User.ServiceUserCode, stmEntityScreeningLog.PJ_SystemCreateUser);
				AssertEquals(vessel.PK, stmEntityScreeningLog.PJ_SourceID);
				AssertEquals(vessel.TablePrefix, stmEntityScreeningLog.PJ_SourceTableCode);
			});
		}

		[TestDate(2021, 1, 1)]
		public void TestReScreening_JobDocAddress()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			Factory.Save();

			jobDocAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk =  Guid.NewGuid(), ClientSpecifiedIdentifier = jobDocAddress.PK.ToGuid(), TypeOfEntity = jobDocAddress.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var log = @"Information|Received 1 Rescreening advice(s)
Information|Rescreen batch has started processing 1 record(s).
Information|Rescreen batch processed: Reset 0 Org and 0 Vessel record(s).
Information|Denied Party Rescreen Advice service task completed successfully: Processed 1 rescreen advice(s), reset 0 record(s).
";
			jobDocAddress.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, jobDocAddress.E2_ScreeningStatus);
				AssertEquals(log, serviceTask.ServiceLogger.ToString());
				AssertEquals(0, Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, jobDocAddress.PK)).Length);
			});
		}

		[TestDate(2021, 1, 1)]
		public void TestReScreening_Vessel_Organization()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Code1";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);
			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var itemPKs = new[] { Guid.NewGuid(), Guid.NewGuid() };
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[0], ClientSpecifiedIdentifier = vessel.PK.ToGuid(), TypeOfEntity = vessel.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } },
				new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[1], ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } },
			};

			var response = JsonConvert.SerializeObject(rescreenAdvices);
			var httpService = GetHttpResponseService(new Uri(localHost), response, 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var expectedHeaderLog = $"Debug|Processing OrgHeader - {header.PK}.";
			var expectedVesselLog = $"Debug|Processing RefVessel - {vessel.PK}.";

			var headerOrgPartyScreeningStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Single();
			var vesselOrgPartyScreeningStatus = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, vessel.PK)).Single();

			vessel.Reload();
			header.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, vessel.RV_ScreeningStatus);
				AssertContains(expectedVesselLog, serviceTask.ServiceLogger.ToString());
				AssertEquals(ZDateTime.UtcNow, vesselOrgPartyScreeningStatus.PJ_SystemCreateTimeUtc);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate, vesselOrgPartyScreeningStatus.PJ_Status);
				AssertEquals(User.ServiceUserCode, vesselOrgPartyScreeningStatus.PJ_SystemCreateUser);
				AssertEquals(vessel.PK, vesselOrgPartyScreeningStatus.PJ_SourceID);
				AssertEquals(vessel.TablePrefix, vesselOrgPartyScreeningStatus.PJ_SourceTableCode);

				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, header.OH_ScreeningStatus);
				AssertContains(expectedHeaderLog, serviceTask.ServiceLogger.ToString());
				AssertEquals(ZDateTime.UtcNow, headerOrgPartyScreeningStatus.PJ_SystemCreateTimeUtc);
				AssertEquals(DeniedPartyConstants.LogsScreeningStatus.InvalidatedByContentUpdate, headerOrgPartyScreeningStatus.PJ_Status);
				AssertEquals(User.ServiceUserCode, headerOrgPartyScreeningStatus.PJ_SystemCreateUser);
				AssertEquals(header.PK, headerOrgPartyScreeningStatus.PJ_SourceID);
				AssertEquals(header.TablePrefix, headerOrgPartyScreeningStatus.PJ_SourceTableCode);

				AssertContainsExactElementsInAnyOrder(itemPKs, DeleteRequestItemPks);
			});
		}

		public void TestReScreening_IgnoreNotClearRecord()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var itemPKs = new[] { Guid.NewGuid() };
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[0], ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var log = $@"Information|Received 1 Rescreening advice(s)
Information|Rescreen batch has started processing 1 record(s).
Debug|The following OrgHeader record(s) will not be reset to (REQ) Requires Review: [{string.Join(",", itemPKs)}]. This is because the record is no longer clear, the record is deactivated or the record does not exist.
Information|Rescreen batch processed: Reset 0 Org and 0 Vessel record(s).
Information|Denied Party Rescreen Advice service task completed successfully: Processed 1 rescreen advice(s), reset 0 record(s).
";
			var stmEntityScreeningLog = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).FirstOrDefault();

			header.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Matched, header.OH_ScreeningStatus);
				AssertEquals(null, stmEntityScreeningLog);
				AssertEquals(log, serviceTask.ServiceLogger.ToString());
				AssertContainsExactElementsInAnyOrder(itemPKs, DeleteRequestItemPks);
			});
		}

		public void TestReScreening_NoAdvices()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var httpService = GetHttpResponseService(new Uri(localHost), "[]", 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var log = @"Debug|Received 0 Rescreening advice(s)
";
			AssertEquals(log, serviceTask.ServiceLogger.ToString());
		}

		[TestDate(2021, 1, 1)]
		public void TestReScreening_DpsEventCreated()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var itemPKs = new[] { Guid.NewGuid() };
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[0], ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			header.Reload();

			var latestLog = header.GetLogs().MostRecentLogByEventTime(AutoEvents.DeniedPartyStatusUpdated);
			AssertEquals(ScreeningStatusesList.Codes.RequiresReview, header.OH_ScreeningStatus);
			AssertContains("NEW=REQ|OLD=CLR|TYP=RSA", latestLog.SL_Reference);
		}

		public void TestShouldNotCallConfirmRescreenAdvicesWhenNoValidData()
		{
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = Guid.NewGuid(), TypeOfEntity = "OH", Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "ABC123XYZ" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();

				new DpsRescreeningServiceTask().RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			AssertEquals(1, numberOfCallConfirmRescreenAdvices);
		}

		public void TestRunsInAnyBranch()
		{
			ErrorReporter.Clear();
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var itemPKs = new[] { Guid.NewGuid() };
			var rescreenAdvices = new[]
			{
					new DeniedPartyRescreenAdvice { AdviceItemPk = itemPKs[0], ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
				};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);

			try
			{
				httpService.Start();
				using (Env.Instance.TemporaryServiceTaskContext(DpsRescreeningServiceTask.Code, canRunInAnyBranch: true))
				{
					serviceTask.RunTask(CancellationToken.None);
					AssertEquals("No Errors", string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		#endregion

		#region Throw Exception
		public void TestThrowException_ProcessEntities()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Code6";
			vessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, vessel.RV_ScreeningStatus);

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code5";
			header.OH_FullName = "TestOrg";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);

			var serviceTask = new DpsRescreenServiceTask_ForTest()
			{
				ServiceLogger = new TestServiceLogger()
			};
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = vessel.PK.ToGuid(), TypeOfEntity = vessel.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } },
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 100, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 200);
			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
			var logError = "Error|Denied Party Rescreen Advice service task terminated with an error: Reset 1 record(s).";
			var logProcessing = $@"Information|Received 2 Rescreening advice(s)
Information|Rescreen batch has started processing 2 record(s).
Debug|Processing OrgHeader - {header.PK}.";
			AssertContains(logError, serviceTask.ServiceLogger.ToString());
			AssertContains(logProcessing, serviceTask.ServiceLogger.ToString());
			ErrorReporter.Clear();
		}

		public void TestThrowWebException500_GetReScreeningAdvices()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};
			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), 500);

			InsertOrUpdateServiceTaskLastSuccessfulRuntime(ZDateTime.UtcNow.AddHours(-2.1));

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			header.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
				AssertEquals(0, Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Length);
				AssertContains("Error|Web error occurred in DPS Service Task.", serviceTask.ServiceLogger.ToString());
				AssertContains("System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", serviceTask.ServiceLogger.ToString());
				AssertEquals("Web error occurred in DPS Service Task.", ErrorReporter.LastKeyReported);
				AssertEquals(typeof(WebException), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals(0, DeleteRequestItemPks.Count);
			});

			ErrorReporter.Clear();
		}

		public void TestHandleWebException400LogAsError()
		{
			AssertWebException(400, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (400) Bad Request.", false);
		}

		public void TestHandleWebException408LogAsError()
		{
			AssertWebException(408, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (408) Request Timeout.", false);
		}

		public void TestHandleWebException500LogAsError()
		{
			AssertWebException(500, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", true);
		}
		public void TestHandleWebException500LogAsWarning()
		{
			AssertWebException(500, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", false);
		}

		public void TestHandleWebException501LogAsWarning()
		{
			AssertWebException(501, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (501) Not Implemented.", false);
		}

		public void TestHandleWebException503LogAsWarning()
		{
#if NET
			AssertWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Service Unavailable.", false);
#else
			AssertWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Server Unavailable.", false);
#endif
		}

		public void TestHandleWebException504LogAsError()
		{
			AssertWebException(504, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (504) Gateway Timeout.", false);
		}

		public void TestHandleWebException305LogAsWarning()
		{
#if NET
			AssertWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy", false);
#else
			AssertWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy Redirect.", false);
#endif
		}

		void InsertOrUpdateServiceTaskLastSuccessfulRuntime(ZDateTime lastSuccessfulRuntime)
		{
			var runningStatus = new
			{
				FailedToRunCount = 1,
				LastSuccessfulRuntime = lastSuccessfulRuntime.ToDateTime(),
			};

			var binaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(runningStatus));

			_ = TestConnection.ExecuteNonQuery($@"
IF ((SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'ServiceTaskDPR_LastSuccessfulRuntime') = 0)
BEGIN
	INSERT INTO dbo.StmData (SD_PK, SD_Name)
	VALUES (NEWID(), 'ServiceTaskDPR_LastSuccessfulRuntime')
END

UPDATE dbo.StmData SET SD_BinaryValue = @BinaryValue
WHERE SD_Name = 'ServiceTaskDPR_LastSuccessfulRuntime'", parameters => parameters.AddParameter("@BinaryValue", System.Data.SqlDbType.VarBinary, binaryValue.XmlSerializedValue));
		}

		void AssertWebException(int statusCode, string expectedMessage, bool shouldReportOnce)
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Code" + statusCode;
			header.OH_FullName = "Name" + statusCode;
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};
			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};
			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices), statusCode);

			if (statusCode == 500)
			{
				InsertOrUpdateServiceTaskLastSuccessfulRuntime(shouldReportOnce ? ZDateTime.UtcNow.AddHours(-2.1) : ZDateTime.UtcNow.AddHours(1));
			}

			try
			{
				numberOfCalls = 0;

				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);

				header.Reload();

				CombineAssertions(() =>
				{
					AssertEquals(3, numberOfCalls);
					AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
					AssertEquals(0, Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Length);

					var messages = expectedMessage.Split('|');
					var actualLog = serviceTask.ServiceLogger.ToString();

					AssertEquals(3, messages.Length);
					AssertContains(messages[0] + "|" + messages[1], actualLog);
					AssertContains(messages[2], actualLog);

					if (shouldReportOnce)
					{
						AssertEquals("Web error occurred in DPS Service Task.", ErrorReporter.LastKeyReported);
						AssertEquals(typeof(WebException), ErrorReporter.LastExceptionReported.GetType());
						ErrorReporter.Clear();
					}
					else
					{
						AssertNull(ErrorReporter.LastExceptionReported);
					}
				});
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		public void TestRunTask_When_AuthCertNotFoundException_Should_OnlyLogMessageAndNotReport()
		{
			ErrorReporter.Clear();
			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var serviceTask = new DpsRescreeningServiceTask()
				{
					ServiceLogger = new TestServiceLogger()
				};

				serviceTask.RunTask();

				AssertContains(new AuthCertNotFoundException().Message, serviceTask.ServiceLogger.ToString());
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		#endregion

		#region Batch Size

		public void TestBatchSize_LessThanFifty()
		{
			AssertBatchSize(10);
		}

		public void TestBatchSize_EqualToFifty()
		{
			AssertBatchSize(50);
		}

		public void TestBatchSize_MoreThanFifty()
		{
			AssertBatchSize(151);
		}

		public void AssertBatchSize(int size)
		{
			var headerList = new List<OrgHeader>();
			for (int i = 0; i < size; i++)
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "Code" + i;
				header.OH_FullName = "Name" + i;
				header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
				headerList.Add(header);
			}

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var rescreenAdvices = headerList.Select(x => new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = x.PK.ToGuid(), TypeOfEntity = x.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }).ToArray();
			var response = JsonConvert.SerializeObject(rescreenAdvices);
			var httpService = GetHttpResponseService(new Uri(localHost), response, 200);

			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyRescreeningServiceTaskBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
				{
					serviceTask.RunTask(CancellationToken.None);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			var logs = headerList.Select(x => $"Debug|Processing OrgHeader - {x.PK}.");
			var stmEntityScreeningLog = Factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, headerList.Select(x => x.PK)));

			foreach (var header in headerList)
			{
				header.Reload();
				AssertEquals(ScreeningStatusesList.Codes.RequiresReview, header.OH_ScreeningStatus);
			}

			foreach (var log in logs)
			{
				AssertContains(log, serviceTask.ServiceLogger.ToString());
			}

			AssertEquals(size, stmEntityScreeningLog.Length);
			AssertEquals(size, DeleteRequestItemPks.Distinct().Count());
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		readonly string localHost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";

		List<Guid> DeleteRequestItemPks { get; } = new List<Guid>();
		int numberOfCalls;
		int numberOfCallConfirmRescreenAdvices;

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int statusCode)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST", "PUT" },
				Processor = (uri, request) =>
				{
					numberOfCalls++;
					var isRetrievingAdvices = request.Contains("OrgNameMediumConfidenceThreshold");
					if (isRetrievingAdvices)
					{
						return new Tuple<int, string>(statusCode, response);
					}
					else
					{
						numberOfCallConfirmRescreenAdvices++;
						foreach (var pk in JsonConvert.DeserializeObject<List<Guid>>(request))
						{
							DeleteRequestItemPks.Add(pk);
						}
					}

					return new Tuple<int, string>(200, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		IDisposable setRegistryUrl;
		IDisposable tokenProvider;
		IDisposable mdmSupportCertificateForDPS;

		protected override void SetUpCore()
		{
			var newStaff = Factory.New<IGlbStaff>();
			newStaff.GS_Code = "APH";
			newStaff.GS_EmailAddress = "dps.rescreenadvice@wisetechglobal.com";
			Factory.Save();

			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			setRegistryUrl = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localHost));
			tokenProvider = ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest());
			mdmSupportCertificateForDPS = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo);
		
			DeleteRequestItemPks.Clear();
			numberOfCallConfirmRescreenAdvices = 0;

			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			setRegistryUrl.Dispose();
			tokenProvider?.Dispose();
			mdmSupportCertificateForDPS?.Dispose();

			DeleteRequestItemPks.Clear();
			base.TearDownCore();
		}

		#endregion
	}

	public class RescreeningServiceTaskNonTransactionedTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestThrowException_DeleteReScreeningAdvices()
		{
			var localHost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";
			var factory = new BusinessObjectFactory();
			var newStaff = factory.New<IGlbStaff>();
			newStaff.GS_Code = "APH";
			newStaff.GS_EmailAddress = "dps.rescreenadvice@wisetechglobal.com";

			var group = factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));

			var header = factory.New<OrgHeader>();
			header.OH_Code = "Code1";
			header.OH_FullName = "Name1";
			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			factory.Save();

			var serviceTask = new DpsRescreeningServiceTask()
			{
				ServiceLogger = new TestServiceLogger()
			};

			var rescreenAdvices = new[]
			{
				new DeniedPartyRescreenAdvice { AdviceItemPk = Guid.NewGuid(), ClientSpecifiedIdentifier = header.PK.ToGuid(), TypeOfEntity = header.TablePrefix, Score = 80, RescreenTime = DateTime.UtcNow, SourceListsCode = new[] { "Test" } }
			};

			var httpService = GetHttpResponseService(new Uri(localHost), JsonConvert.SerializeObject(rescreenAdvices));

			try
			{
				httpService.Start();
				var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

				using (ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest()))
				using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo))
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localHost)))
				{
					serviceTask.RunTask(CancellationToken.None);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			header.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(ScreeningStatusesList.Codes.Clear, header.OH_ScreeningStatus);
				AssertEquals(0, factory.Load<StmEntityScreeningLog>(new ZQuery(StmEntityScreeningLogSchema.PJ_ParentID, header.PK)).Length);
				AssertContains("Warning|Web error occurred in DPS Service Task.", serviceTask.ServiceLogger.ToString());
				AssertContains("System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", serviceTask.ServiceLogger.ToString());
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
				AssertNull(ErrorReporter.LastExceptionReported);
			});

			ErrorReporter.Clear();
		}

		#region Implementation

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST", "PUT" },
				Processor = (uri, request) =>
				{
					if (request.Contains("OrgNameMediumConfidenceThreshold"))
					{
						return new Tuple<int, string>(200, response);
					}

					return new Tuple<int, string>(500, response);
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		#endregion
	}

	public class DpsRescreenServiceTask_ForTest : DpsRescreeningServiceTask
	{
		protected override int ProcessEntities<TBizo>(List<DeniedPartyRescreenAdvice> batchAdvices, SchemaPKColumn pkColumn, SchemaStringColumn screeningStatusColumn, BusinessObjectFactoryProvider factoryProvider, CancellationToken token)
		{
			if (pkColumn.Equals(RefVesselSchema.PK))
			{
				throw new Exception("Make ProcessEntities throw Exception for Vessel");
			}
			return base.ProcessEntities<TBizo>(batchAdvices, pkColumn, screeningStatusColumn, factoryProvider, token);
		}
	}
}
