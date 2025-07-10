using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Business.Test;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DeniedPartyScreening.ServiceTasks.Test
{
	[TestedType(typeof(DPSSendScreeningDecisionServiceTask))]
	class DPSSendScreeningDecisionServiceTaskTest : ServiceTaskTestCase<DPSSendScreeningDecisionServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceCanRunInAnyBranch()
		{
			AssertEquals("CanRunInAnyBranch", true, GetHostedServiceAttributes().Single().CanRunInAnyBranch);
		}

		public void TestSendScreeningDecisionsToServer_WithMessageData()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();
			var message3 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message3.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			message3.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message3.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);

				AssertEquals($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message3.PK} and CSI {csi} is added to the web request
Information|Sent 2 Screening Decision(s).
Information|Updated Screening Decision(s) As Processed.
", serviceTask.ServiceLogger.ToString());
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message3.EM_Status);

				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);
				AssertEquals(string.Empty, serviceTask.ServiceLogger.ToString());
				AssertEquals(HMACSHA256Helper.GetComputedLicenceCode(GlbBranch.GetOneActiveBranchPerCompany().First().Company.GetLicenceKeyIdentifier("-")), httpService.Headers["LicenceCode"]);
				AssertNotNull(Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "ServiceTaskDPM_LastSuccessfulRuntime")));

				var authorizationHeader = httpService.Headers["Authorization"].Split(' ');
				var authorizationtoken = new AuthenticationHeaderValue(authorizationHeader[0], authorizationHeader[1]);

				AssertEquals("Bearer", authorizationtoken.Scheme);
				AssertNotNullOrEmpty(authorizationtoken.Parameter);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		public void TestSendScreeningDecisionsToServer_WhenBothEDIMessageFormatsArePresent_ShouldProcessBothDecisions()
		{
			var csi = Guid.NewGuid();
			var messageDataOldFormat = new DpsEDIMessageContent
			{
				ClientLicence = "ABC-EFG-HIJ",
				DatabaseType = "UNK",
				ClientSpecifiedIdentifier = csi,
				EntityType = "OH",
				PersistentStatus = "CLR",
				ScreenTime = DateTime.Today,
				DpsRequestHeader = new DpsRequestHeader()
			};
			var messageDataNewFormat = new DpsEDIMessageCandidatesContent
			{
				ClientLicence = "ABC-EFG-HIJ",
				DatabaseType = "UNK",
				ClientSpecifiedIdentifier = csi,
				EntityType = "OH",
				PersistentStatus = "CLR",
				ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageDataOldFormat, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageDataOldFormat, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;

			var message3 = Factory.New<DpsEDIMessage>();
			var message4 = Factory.New<DpsEDIMessage>();

			message3.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageDataNewFormat, Formatting.None));
			message4.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageDataNewFormat, Formatting.None));

			message3.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message4.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.ProcessedOK, message3.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message4.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);

				AssertEquals($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message4.PK} and CSI {csi} is added to the web request
Information|Sent 2 Screening Decision(s).
Information|Updated Screening Decision(s) As Processed.
", serviceTask.ServiceLogger.ToString());
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message1.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message2.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message3.EM_Status);
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message4.EM_Status);

				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask(CancellationToken.None);
				AssertEquals(string.Empty, serviceTask.ServiceLogger.ToString());
				AssertEquals(HMACSHA256Helper.GetComputedLicenceCode(GlbBranch.GetOneActiveBranchPerCompany().First().Company.GetLicenceKeyIdentifier("-")), httpService.Headers["LicenceCode"]);
				AssertNotNull(Factory.LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "ServiceTaskDPM_LastSuccessfulRuntime")));

				var authorizationHeader = httpService.Headers["Authorization"].Split(' ');
				var authorizationtoken = new AuthenticationHeaderValue(authorizationHeader[0], authorizationHeader[1]);

				AssertEquals("Bearer", authorizationtoken.Scheme);
				AssertNotNullOrEmpty(authorizationtoken.Parameter);
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}
		}

		public void TestSendScreeningDecisionsToServer_WithEmptyDatabaseTypeInMessageData()
		{
			var messageData = @"
{
""clientLicence"": ""HYE-DAU-CMT"",
""clientSpecifiedIdentifier"": ""95e15a8a-199d-4afe-9902-ee19139c4a43"",
""entityType"": ""OH"",
""persistentStatus"": ""DPS"",
""screenTime"": ""2021-07-12T03:03:46.5674778Z"",
""dpsNameCandidates"": [
        {
            ""NameType"": ""ORG"",
            ""FullName"": ""WISETECH"",
            ""LanguageCode"": null,
            ""StandardizedValue"": null
        }
    ],
""dpsAddressCandidates"": [
        {
            ""Address1"": ""74 O'RIORDAN STREET"",
            ""Address2"": """",
            ""City"": ""ALEXANDRIA"",
            ""State"": 13,
            ""PostCode"": 2015,
            ""Country"": ""CN"",
            ""AdditionalAddressLine"": ""SADDAM"",
            ""StandardizedValue"": null
        }
    ],
""dpsRegistrationCodeCandidates"": [],
}";

			var ediMessage = Factory.New<DpsEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			ediMessage.EM_IsActive = true;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_MessageData = ZBlob.FromUTF8(messageData);
			Factory.Save();

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

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

			AssertContains($"Debug|Screening decision with Message PK {ediMessage.PK} and CSI {Guid.Parse("95e15a8a-199d-4afe-9902-ee19139c4a43")} is added to the web request", serviceTask.ServiceLogger.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
		}

		public void TestSendScreeningDecisionsToServer_WithoutMessageData_Organization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var message = Factory.New<DpsEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = "OH " + org.PK;
			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

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

			AssertEquals("Should report exception", "DPM service task got invalid DpsEDIMessageContent", ErrorReporter.LastKeyReported);
			AssertEquals("Should report exception", $"Screening decision with Message PK {message.PK} is not added to the web request as Message Data is Null", ErrorReporter.LastMessageReported);
			AssertContains("Information|Updated Screening Decision(s) As Processed.", serviceTask.ServiceLogger.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			ErrorReporter.Clear();
		}

		public void TestSendScreeningDecisionsToServer_WithoutMessageData_Vessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var message = Factory.New<DpsEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = "RV " + vessel.PK;
			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

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

			AssertEquals("Should report exception", "DPM service task got invalid DpsEDIMessageContent", ErrorReporter.LastKeyReported);
			AssertEquals("Should report exception", $"Screening decision with Message PK {message.PK} is not added to the web request as Message Data is Null", ErrorReporter.LastMessageReported);
			AssertContains("Information|Updated Screening Decision(s) As Processed.", serviceTask.ServiceLogger.ToString());
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			ErrorReporter.Clear();
		}

		public void TestSendScreeningDecisionsToServer_WithNullRequestHeader()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = null,
				DpsRegistrationCodeCandidates = null,
				DpsCountryCandidates = null,
				DpsNameCandidates = null
			};
			var message = Factory.New<DpsEDIMessage>();

			message.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

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

			AssertEquals("Should report exception", "DPM service task got invalid DpsRequestHeader", ErrorReporter.LastKeyReported);
			AssertEquals("Should report exception", $"Screening decision with Message PK {message.PK} and CSI {csi} is not added to the web request as the Request Header is Null", ErrorReporter.LastMessageReported);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			ErrorReporter.Clear();
		}

		public void TestSendDpsMessage_HttpResponse500_ThrowsWebException()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.Queued;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message2.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null, 500);

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

			AssertContains($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.
", serviceTask.ServiceLogger.ToString());
			Assert("Should report exception", ErrorReporter.LastMessageReported.Contains("Web error occurred in DPS Service Task."));
			ErrorReporter.Clear();
		}

		public void TestSendDpsMessage_RetrySuccessful()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.Queued;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message2.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseServiceWithRetry(new Uri(localHost), null, 2);

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

			AssertEquals($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Information|Sent 2 Screening Decision(s).
Information|Updated Screening Decision(s) As Processed.
", serviceTask.ServiceLogger.ToString());

			AssertEquals("Should not report exception", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestRetryHandlerThreadSleep_HasNotExceedRetries_NoWebExceptionThrown()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message1.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message1.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseServiceWithRetry(new Uri(localHost), null, 2);
			var watch = System.Diagnostics.Stopwatch.StartNew();

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

			watch.Stop();
			AssertGreaterThanOrEqualTo(watch.ElapsedMilliseconds, 1000);
		}

		public void TestRetryHandlerThreadSleep_HasExceedRetries_WebExceptionThrown()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message1.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message1.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var watch = System.Diagnostics.Stopwatch.StartNew();

			serviceTask.RunTask(CancellationToken.None);

			watch.Stop();
			AssertGreaterThanOrEqualTo(watch.ElapsedMilliseconds, 1000);
#if NET

			AssertContains($@"Information|Retrieved 1 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Warning|There was no Http response.|System.Net.WebException: No connection could be made because the target machine actively refused it.", serviceTask.ServiceLogger.ToString());
#else
			AssertContains($@"Information|Retrieved 1 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Warning|There was no Http response.|System.Net.WebException: Unable to connect to the remote server ---> System.Net.Sockets.SocketException: No connection could be made because the target machine actively refused it", serviceTask.ServiceLogger.ToString());

#endif
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestSendDpsMessage_WebServiceNotAlive_ThrowsWebException()
		{
			var csi = Guid.NewGuid();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = csi, EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.Queued;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message2.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };

			serviceTask.RunTask(CancellationToken.None);
#if NET
			AssertContains($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Warning|There was no Http response.|System.Net.WebException: No connection could be made because the target machine actively refused it", serviceTask.ServiceLogger.ToString());
#else
			AssertNull(ErrorReporter.LastExceptionReported);

			AssertContains($@"Information|Retrieved 2 Screening Decision(s).
Debug|Screening decision with Message PK {message1.PK} and CSI {csi} is added to the web request
Debug|Screening decision with Message PK {message2.PK} and CSI {csi} is added to the web request
Warning|There was no Http response.|System.Net.WebException: Unable to connect to the remote server ---> System.Net.Sockets.SocketException: No connection could be made because the target machine actively refused it", serviceTask.ServiceLogger.ToString());
#endif
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestRun_SendInBatches()
		{
			using (OrganisationsDataRegistry.Instance.DPSSendScreeningDecisionServiceTaskBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				for (int i = 0; i < 11; i++)
				{
					var message = Factory.New<DpsEDIMessage>();
					message.EM_Status = EDIMessageStatusList.Codes.Queued;
					var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = Guid.NewGuid(), EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
						DpsAddressCandidates = new List<DpsAddressCandidate>(),
						DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
						DpsCountryCandidates = new List<DpsCountryCandidate>(),
						DpsNameCandidates = new List<DpsNameCandidate>()
					};
					message.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
				}
				Factory.Save();

				var serviceTask = new DPSSendScreeningDecisionServiceTask() { ServiceLogger = new TestServiceLogger() };
				var httpService = GetHttpResponseService(new Uri(localHost), null);

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

				AssertEquals("Web service should be called twice.", 2, numberOfCalls);
			}
		}

		public void TestRun_ExceptionThrown()
		{
			var message = Factory.New<DpsEDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			Factory.Save();

			AssertEquals("Precondition", EDIMessageStatusList.Codes.Queued, message.EM_Status);

			var serviceTask = new DPSSendScreeningDecisionServiceTask() { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

			Db.Connection.Command("DROP INDEX NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum ON EDIMessage").ExecuteNonQuery();

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

			AssertContains("Error|Index 'NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum' on table 'dbo.EDIMessage' (specified in the FROM clause) does not exist.", serviceTask.ServiceLogger.ToString());
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("Should report exception", "Exception occurred while running DPM service task.", ErrorReporter.LastMessageReported);
			AssertEquals("Should report exception", "Index 'NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum' on table 'dbo.EDIMessage' (specified in the FROM clause) does not exist.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestHandleSavingException()
		{
			var message = Factory.New<DpsEDIMessage>();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = Guid.NewGuid(), EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			Factory.Save();
			new DpsServiceTaskHelper(DPSSendScreeningDecisionServiceTask.Code);

			var sql = "SELECT COUNT(1) FROM dbo.OrgHeader";
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			var dataRow = dataTable.Rows[0];

			var zSaveConcurrencyException = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("ZSaveConcurrencyException"), dataRow, Db.Connection), Factory);
			AssertHandleSavingException(zSaveConcurrencyException, "Error|" + zSaveConcurrencyException.Message);

			var zCannotSaveException = new ZCannotSaveException("ZCannotSaveException", string.Empty);
			AssertHandleSavingException(zCannotSaveException, "Error|" + zCannotSaveException.Message);

			var zSaveException = new ZSaveException(new ZDataException(new Exception(string.Empty), dataRow, Db.Connection), Factory);
			AssertHandleSavingException(zSaveException, "Error|" + zSaveException.Message);
		}

		public void TestRunTask_When_AuthCertNotFoundException_Should_OnlyLogMessageAndNotReport()
		{
			ErrorReporter.Clear();

			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = Guid.Parse("6f7969de-e14d-42e5-8e88-b1660a3d17ac"), EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};

			var message = Factory.New<DpsEDIMessage>();
			message.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var serviceTask = new DPSSendScreeningDecisionServiceTask()
				{
					ServiceLogger = new TestServiceLogger()
				};

				serviceTask.RunTask();

				AssertContains(new AuthCertNotFoundException().Message, serviceTask.ServiceLogger.ToString());
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestRunsInAnyBranch()
		{
			ErrorReporter.Clear();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = Guid.NewGuid(), EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			var message1 = Factory.New<DpsEDIMessage>();
			var message2 = Factory.New<DpsEDIMessage>();
			var message3 = Factory.New<DpsEDIMessage>();

			message1.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message2.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			message3.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));

			message1.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			message3.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

			try
			{
				httpService.Start();
				using (Env.Instance.TemporaryServiceTaskContext(DPSSendScreeningDecisionServiceTask.Code, canRunInAnyBranch: true))
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

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Denied Party Screening Send Screening Decisions Service",
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DPSRequestMessage,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.JDC,
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.ScreeningRequest,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued),
				};
			}
		}

		void AssertHandleSavingException(Exception exception, string expectedMessage)
		{
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw exception);

			var serviceTask = new DPSSendScreeningDecisionServiceTask() { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null);

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

			AssertContains(expectedMessage, serviceTask.ServiceLogger.ToString());
		}

		public void TestHandleWebException()
		{
			var message = Factory.New<DpsEDIMessage>();
			var messageData = new DpsEDIMessageCandidatesContent { ClientLicence = "ABC-EFG-HIJ", DatabaseType = "UNK", ClientSpecifiedIdentifier = Guid.NewGuid(), EntityType = "OH", PersistentStatus = "CLR", ScreenTime = DateTime.Today,
				DpsAddressCandidates = new List<DpsAddressCandidate>(),
				DpsRegistrationCodeCandidates = new List<DpsRegistrationCodeCandidate>(),
				DpsCountryCandidates = new List<DpsCountryCandidate>(),
				DpsNameCandidates = new List<DpsNameCandidate>()
			};
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageData = ZBlob.FromUTF8(JsonConvert.SerializeObject(messageData, Formatting.None));
			Factory.Save();

			AssertHandleWebException(400, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (400) Bad Request.", false);
			AssertHandleWebException(408, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (408) Request Timeout.", false);
			AssertHandleWebException(500, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", true);
			AssertHandleWebException(500, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (500) Internal Server Error.", false);
			AssertHandleWebException(501, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (501) Not Implemented.", false);
#if NET
			AssertHandleWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Service Unavailable.", false);
#else
			AssertHandleWebException(503, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (503) Server Unavailable.", false);
#endif

			AssertHandleWebException(504, "Error|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (504) Gateway Timeout.", false);
#if NET
			AssertHandleWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy", false);
#else
			AssertHandleWebException(305, "Warning|Web error occurred in DPS Service Task.|System.Net.WebException: The remote server returned an error: (305) Use Proxy Redirect.", false);
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
IF ((SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'ServiceTaskDPM_LastSuccessfulRuntime') = 0)
BEGIN
	INSERT INTO dbo.StmData (SD_PK, SD_Name)
	VALUES (NEWID(), 'ServiceTaskDPM_LastSuccessfulRuntime')
END

UPDATE dbo.StmData SET SD_BinaryValue = @BinaryValue
WHERE SD_Name = 'ServiceTaskDPM_LastSuccessfulRuntime'", parameters => parameters.AddParameter("@BinaryValue", System.Data.SqlDbType.VarBinary, binaryValue.XmlSerializedValue));
		}

		void AssertHandleWebException(int statusCode, string expectedMessage, bool shouldReportOnce)
		{
			numberOfCalls = 0;
			var serviceTask = new DPSSendScreeningDecisionServiceTask { ServiceLogger = new TestServiceLogger() };
			var httpService = GetHttpResponseService(new Uri(localHost), null, statusCode);

			if (statusCode == 500)
			{
				InsertOrUpdateServiceTaskLastSuccessfulRuntime(shouldReportOnce ? ZDateTime.UtcNow.AddHours(-2.1) : ZDateTime.UtcNow.AddHours(1));
			}

			try
			{
				httpService.Start();
				serviceTask.RunTask(CancellationToken.None);

				CombineAssertions(() =>
				{
					AssertEquals(3, numberOfCalls);
					AssertContains(expectedMessage, serviceTask.ServiceLogger.ToString());

					if (shouldReportOnce)
					{
						AssertEquals("Web error occurred in DPS Service Task.", ErrorReporter.LastMessageReported);
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

		#region Implementation

		readonly string localHost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";

		HttpServiceForTest GetHttpResponseServiceWithRetry(Uri serviceUrl, string response, int numberOfThrows = 0)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET", "DELETE", "POST" },
				Processor = (uri, request) =>
				{
					numberOfCalls++;
					if (numberOfCalls <= numberOfThrows)
					{
						return new Tuple<int, string>(500, response);
					}
					else
					{
						return new Tuple<int, string>(200, response);
					}
				},
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int statusCode = 200)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET", "DELETE", "POST" },
				Processor = (uri, request) =>
				{
					numberOfCalls++;
					return new Tuple<int, string>(statusCode, response);
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
			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();

			setRegistryUrl = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localHost));
			tokenProvider = ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest());
			mdmSupportCertificateForDPS = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo);

			numberOfCalls = 0;

			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			setRegistryUrl.Dispose();
			tokenProvider?.Dispose();
			mdmSupportCertificateForDPS?.Dispose();

			base.TearDownCore();
		}

		int numberOfCalls;
		#endregion
	}
}
