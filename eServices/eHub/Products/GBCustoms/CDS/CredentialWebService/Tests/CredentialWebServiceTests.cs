using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.ConfigurationHandler;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.eHub.DataAccess.Integration;
using MockRepository = Rhino.Mocks.MockRepository;
using CargoWise.eHub.Shared.IssueManager;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.Tests
{
	[TestFixture]
	public class CredentialWebServiceTests
	{
		[SetUp]
		public void SetUp()
		{
			DatabaseAccessHelpers.RetryTimeLimit = 5000;
		}

		[TearDown]
		public void TearDown()
		{
			DatabaseAccessHelpers.RetryTimeLimit = 600000;
		}

		[Test]
		public void TestPersistAuthorisationToken_AddOrUpdateAuthorisationTokenThrowsArgumentException_FailWithErrorMsg()
		{
			var mockConfigurationHelper = MockRepository.GenerateStub<IConfigurationHelper>();
			mockConfigurationHelper.Stub(_ => _.AddOrUpdateAuthorisationToken(Arg<string>.Is.Anything,
					Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything,
					Arg<DateTime>.Is.Anything))
				.Throw(new ArgumentException());
			mockConfigurationHelper
				.Stub(_ => _.DoesAuthorisationTokenAlreadyExist(Arg<string>.Is.Anything)).Return(false);

			var credentialService = new CredentialWebServiceForTesting(mockConfigurationHelper);
			var response = credentialService.PersistAuthorisationToken("HYECMT.GB123456789000.ABC", "123", "https:///cds", DateTime.UtcNow);

			Assert.AreEqual(false, response.IsSuccess);
			Assert.AreEqual("Value does not fall within the expected range.", response.ErrorMessage);
		}

		[Test]
		public void TestPersistAuthorisationToken_SuccessAddOrUpdateAuthorisationToken_Success()
		{
			var mockConfigurationHelper = MockRepository.GenerateStub<IConfigurationHelper>();
			mockConfigurationHelper
				.Stub(_ => _.DoesAuthorisationTokenAlreadyExist(Arg<string>.Is.Anything)).Return(false);

			var credentialWebService = new CredentialWebServiceForTesting(mockConfigurationHelper);
			var response = credentialWebService.PersistAuthorisationToken("HYECMT.GB123456789000.ABC", "123", "https:///cds", DateTime.UtcNow);

			Assert.AreEqual(true, response.IsSuccess);
		}

		[Test]
		public void TestPersistAuthorisationToken_AddOrUpdateAuthorisationTokenThrowsInvalidOperationException_FailWithErrorMsg()
		{
			var mockConfigurationHelper = MockRepository.GenerateStub<IConfigurationHelper>();
			mockConfigurationHelper.Stub(_ => _.AddOrUpdateAuthorisationToken(Arg<string>.Is.Anything,
					Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<DateTime>.Is.Anything,
					Arg<DateTime>.Is.Anything))
				.Throw(new InvalidOperationException());

			var credentialWebService = new CredentialWebServiceForTesting(mockConfigurationHelper);
			var response = credentialWebService.PersistAuthorisationToken("HYECMT.GB123456789000.ABC", "123", "https:///cds", DateTime.UtcNow);

			Assert.AreEqual(false, response.IsSuccess);
			Assert.AreEqual("Operation is not valid due to the current state of the object.", response.ErrorMessage);
		}

		[Test]
		public void TestPersistAuthorisationToken_WhileNotifyCW1SqlExceptionIsThrown_FailWithErrorMsg()
		{
			var mockConfigurationHelper = MockRepository.GenerateStub<IConfigurationHelper>();
			mockConfigurationHelper
				.Stub(_ => _.DoesAuthorisationTokenAlreadyExist(Arg<string>.Is.Anything)).Return(true);
			mockConfigurationHelper
				.Stub(_ => _.NotifyCW1(Arg<string>.Is.Anything)).WhenCalled(_ =>
				{
					using (var conn = new SqlConnection("User ID=Rubbish;Password=Rubbish;Initial Catalog=Rubbish;Data Source=Rubbish;Connection Timeout=1"))
					{
						conn.Open();
					}
				});

			var credentialWebService = new CredentialWebServiceForTesting(mockConfigurationHelper);
			var response = credentialWebService.PersistAuthorisationToken("HYECMT.GB123456789000.ABC", "123", "https:///cds", DateTime.UtcNow);

			Assert.AreEqual(false, response.IsSuccess);
			Assert.AreEqual("A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)",
							response.ErrorMessage);
		}

		[Test]
		public void TestPersistAuthorisationToken_WhenNoActiveClient_FailWithErrorMsg()
		{
			var mockConfigurationHelper = MockRepository.GenerateStub<IConfigurationHelper>();
			mockConfigurationHelper
				.Stub(_ => _.DoesAuthorisationTokenAlreadyExist(Arg<string>.Is.Anything)).Return(true);
			mockConfigurationHelper
				.Stub(_ => _.NotifyCW1("HYECMT.GB123456789000.ABC")).Throw(new ArgumentException("Recipient for HYECMT cannot be found."));

			var credentialWebService = new CredentialWebServiceForTesting(mockConfigurationHelper);
			var response = credentialWebService.PersistAuthorisationToken("HYECMT.GB123456789000.ABC", "123", "https:///cds", DateTime.UtcNow);

			Assert.AreEqual(false, response.IsSuccess);
			Assert.AreEqual("Recipient for HYECMT cannot be found.", response.ErrorMessage);
		}

		[Test]
		public void Test_RequestAccessToken_Failure()
		{
			int actualRetry = 5;
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = actualRetry,
				RetryInterval = 1000,
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);
			var response = MockRepository.GenerateMock<HttpWebResponse>();
			var proxiedResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("Server is not responding"));
			response.Stub(x => x.GetResponseStream()).Return(proxiedResponseStream);

			config.GBCustom.Expect
				(
					x => x.UploadValues
					(
						Arg<Uri>.Is.Anything,
						Arg<string>.Is.Anything,
						Arg<NameValueCollection>.Is.Anything
					)
				)
				.Throw(new WebException("Server is not responding", null, WebExceptionStatus.UnknownError, response))
				.Repeat.Times(actualRetry + 1);

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD").Repeat.Times(actualRetry + 1);
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock).Repeat.Times(actualRetry + 1);
			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYEAAACMT").Repeat.Times(actualRetry + 1);

			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/giveitback/to/me", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
			enterpriseAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RequestAccessToken_Failure_NoActiveClient()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,

				BaseURITest = "https://localhost/test",
				UseMockForGBCustomsTest = true,
				UseMockForConfigurationHelper = true,
			};

			var credentialService = new CredentialWebServiceForTesting(config);
			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Throw(new ArgumentException("Recipient for HYECMT cannot be found."));
			config.Logger.Expect(_ => _.Error("GBCustoms CredentialWebService - RequestCoreException"));


			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/test/giveitback/to/me", "HYECMT.GB123456789000.ABC");
			Assert.IsFalse(actualResponse.IsSuccess);

			config.Logger.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
		}

		[Test]
		public void Test_RequestAccessToken_SuccessAfterRetry_AlwaysGeneratesCorrectWebRequestParameters()
		{
			int actualRetry = 1;

			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = actualRetry,
				RetryInterval = 1000,

				BaseURITest = "https://localhost/test",
				UseMockForGBCustomsTest = true,
				UseMockForConfigurationHelper = true,
			};

			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "00000000-0000-0000-0000-000000000000" },
				{ "client_id", "____________________________" },
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", "https://localhost/test/giveitback/to/me" },
				{ "code", "TestAuthorizationToken" }
			};

			//Expect error response for the first time call web service.
			var errorResponse = MockRepository.GenerateMock<HttpWebResponse>();
			var proxiedResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("Server is not responding"));
			errorResponse.Stub(x => x.GetResponseStream()).Return(proxiedResponseStream);

			config.GBCustomTest.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Throw(new WebException("Server is not responding", null, WebExceptionStatus.UnknownError, errorResponse))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			//Expect good response for the second time call web service on retry.
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF"",
						  ""scope"": ""read:employment""
						}";

			config.GBCustomTest.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				eHubClientSystem = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f"),
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")  //GBCustomsAuthorisationToken
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("TST").Repeat.Twice();
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock).Repeat.Twice();
			config.ConfigurationHelper.Stub(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token received OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T04:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();
			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Times(2);
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/test/giveitback/to/me", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken");
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "GB123456789000.ABC");

			config.GBCustomTest.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RequestAccessToken_Success_Functional()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", "https://localhost/giveitback/to/me" },
				{ "code", "TestAuthorizationToken" }
			};
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF"",
						  ""scope"": ""read:employment""
						}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/giveitback/to/me", "HYECMT.GB123456789000.ABC");
		}

		[Test]
		public void Test_RequestAccessToken_Success()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", "https://localhost/giveitback/to/me" },
				{ "code", "TestAuthorizationToken" }
			};
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF"",
						  ""scope"": ""read:employment""
						}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD");
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);
			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token received OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T04:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();
			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Once();
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/giveitback/to/me", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken");
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "GB123456789000.ABC");

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RequestAccessToken_Success_CallURITest()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,

				BaseURITest = "https://localhost/test",
				UseMockForGBCustomsTest = true,
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "00000000-0000-0000-0000-000000000000" },
				{ "client_id", "____________________________" },
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", "https://localhost/test/giveitback/to/me" },
				{ "code", "TestAuthorizationToken" }
			};
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF"",
						  ""scope"": ""read:employment""
						}";

			config.GBCustomTest.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				eHubClientSystem = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f"),
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")  //GBCustomsAuthorisationToken
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("TST");
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);
			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC"))
				.Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token received OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T04:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();
			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Times(2);
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RequestAccessToken("TestAuthorizationToken", "https://localhost/test/giveitback/to/me", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken");
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "GB123456789000.ABC");

			config.GBCustomTest.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAccessToken_Failure()
		{
			int actualRetry = 5;
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = actualRetry,
				RetryInterval = 1000,
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);
			var response = MockRepository.GenerateMock<HttpWebResponse>();
			var proxiedResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("Server is not responding"));
			response.Stub(x => x.GetResponseStream()).Return(proxiedResponseStream);

			config.GBCustom.Expect
				(
					x => x.UploadValues
					(
						Arg<Uri>.Is.Anything,
						Arg<string>.Is.Anything,
						Arg<NameValueCollection>.Is.Anything
					)
				)
				.Throw(new WebException("Server is not responding", null, WebExceptionStatus.UnknownError, response))
				.Repeat.Times(actualRetry + 1);

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD").Repeat.Times(actualRetry + 1);
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock).Repeat.Times(actualRetry + 1);
			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYEKKKCMT").Repeat.Times(actualRetry + 1);
			config.Logger.Expect(_ => _.Error("GBCustoms CredentialWebService - RequestCoreMaxRetriesException"));

			var actualResponse = credentialService.RefreshAccessToken("TestAccessToken", "TestRefreshToken", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
			enterpriseAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAccessToken_Failure_NoActiveClient()
		{
			int actualRetry = 5;
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = actualRetry,
				RetryInterval = 1000,
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			config.ConfigurationHelper.Expect(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Throw(new ArgumentException("Recipient for HYECMT cannot be found."));
			config.Logger.Expect(_ => _.Error("GBCustoms CredentialWebService - RequestCoreException"));

			var actualResponse = credentialService.RefreshAccessToken("TestAccessToken", "TestRefreshToken", "HYECMT.GB123456789000.ABC");
			Assert.IsFalse(actualResponse.IsSuccess);

			config.Logger.VerifyAllExpectations();
			config.ConfigurationHelper.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAccessToken_Success()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,

				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "refresh_token" },
				{ "refresh_token", "TestRefreshToken" }
			};
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF""
						}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")
			});
			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "TestAccessToken",
				CD_Attr2 = "TestRefreshToken",
				CD_Code = "GB123456789000.ABC",
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "2d3d4631-daa2-42d2-aebe-32c1fdb45ef8")
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD");
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);
			config.ConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(MockRepository.GenerateMock<SqlConnection>());
			config.ConfigurationHelper.Stub(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token refreshed OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T04:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();
			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Times(2);
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RefreshAccessToken("TestAccessToken", "TestRefreshToken", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken");
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "GB123456789000.ABC");

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAccessToken_ValidateAccessTokenIsStillValid_Failure()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = false,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,
				Logger = new TestLogger(),
				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "refresh_token" },
				{ "refresh_token", "TestRefreshToken" }
			};
			var response = @"
				{
					""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
					""token_type"": ""bearer"",
					""expires_in"": 14400,
					""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF""
				}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")
			});
			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "TestAccessToken",
				CD_Attr2 = "TestRefreshToken",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_ID.ToString() == "GBCustomsAccessToken"),
				CD_IssuedUTC = new DateTime(2022, 06, 17, 00, 00, 00),
				CD_ExpiryUTC = new DateTime(2022, 06, 17, 02, 00, 00)
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD");
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);
			config.ConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(MockRepository.GenerateMock<SqlConnection>());
			config.ConfigurationHelper.Stub(_ => _.FindRecipient("HYECMT.GB123456789000.ABC"))
				.Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token refreshed OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T02:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();

			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Times(2);
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 16, 23, 55, 00);

			var actualResponse = credentialService.RefreshAccessToken("OutdatedToken", "OutdatedRefreshToken", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);
			Assert.IsTrue(((TestLogger)config.Logger).Log.Contains(
				"[RefreshAccessKey for HYECMT.GB123456789000.ABC] had the following problem: The access token has changed and is no longer the latest in our database. This is a concurrency issue resolution and no further action is required."));

			var authTokenClientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.First(x =>
				x.CD_Attr1 == "e8d10c351fa14d2fa5083a061dbeb22f" &&
				x.CD_Attr2 == "https://localhost" &&
				x.CD_Code == "GB123456789000.ABC" &&
				x.CD_EH == config.DBContext.eHubClientSystems.First(cs => cs.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK &&
				x.eHubRegistrationType == config.DBContext.eHubRegistrationTypes.First(rt => rt.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")
				);
			Assert.IsTrue(2 == authTokenClientSystemRegistration.CD_Flag1, "AuthToken status should be 2");

			config.GBCustom.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			config.MockTransaction.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAccessToken_DuplicateEORI_DifferentClient_Success()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,

				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "refresh_token" },
				{ "refresh_token", "TestRefreshToken" }
			};
			var response = @"
						{
						  ""access_token"": ""QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""refresh_token"": ""unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF""
						}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "e8d10c351fa14d2fa5083a061dbeb22f",
				CD_Attr2 = "https://localhost",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "84b70b52-1471-4ebe-a7cb-851e41f07964")
			});
			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "TestAccessToken",
				CD_Attr2 = "TestRefreshToken",
				CD_Code = "GB123456789000.ABC",
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "eb8c1ede-9808-4ce3-aeab-6fa27c126d0f").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "2d3d4631-daa2-42d2-aebe-32c1fdb45ef8")
			});
			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "OtherTestAccessToken",
				CD_Attr2 = "OtherTestRefreshToken",
				CD_Code = "GB123456789000.ABC",
				CD_Flag1 = 0,
				CD_IssuedUTC = new DateTime(2022, 06, 18),
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "342987c1-80b5-4378-9ccb-14262e002748").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "2d3d4631-daa2-42d2-aebe-32c1fdb45ef8")
			});

			var enterpriseAccessorMock = MockRepository.GenerateMock<IEnterpriseExeDetailAccessor>();
			enterpriseAccessorMock.Expect(_ => _.GetLicenceType("HYEXXXCMT")).Return("PRD");
			config.ConfigurationHelper.Expect(_ => _.NewEnterpriseExeDetailAccessor).Return(enterpriseAccessorMock);
			config.ConfigurationHelper.Stub(_ => _.eHubTransactionsSecondary).Return(MockRepository.GenerateMock<SqlConnection>());
			config.ConfigurationHelper.Stub(_ => _.FindRecipient("HYECMT.GB123456789000.ABC")).Return("HYETSTCMT");

			var expectedMessageContent = new MemoryStream(Encoding.UTF8.GetBytes(
					"﻿<Configuration Name=\"GBCustomsCDS\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\"><Group Type=\"System\" Reference=\"HYECMT\"><Group Type=\"CDS\" Status=\"VAL\"><Annotations><Item Name=\"TokenType\">Access</Item><Item Name=\"Message\">Access token refreshed OK</Item><Item Name=\"Issued\">2022-06-17T00:00:00Z</Item><Item Name=\"Expires\">2022-06-17T04:00:00Z</Item></Annotations><Item Name=\"MailBoxID\">GB123456789000</Item><Credential Name=\"Current\"><UserName>ABC</UserName></Credential></Group></Group></Configuration>"))
				.CompressAndEncode().ReadToEnd();
			var inboxAccessorMock = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessorMock.Expect(_ => _.InsertToInboxAndOutbox(
				Arg<string>.Is.Equal("eHub"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(p =>
					p.ClientID == "HYETSTCMT" &&
					p.MessageTrackingID == new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301") &&
					p.SchemaName == "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" &&
					p.MessageStream.ReadToEnd() == expectedMessageContent &&
					p.SchemaType == MessageSchemaType.Xml &&
					p.EmailSubject == string.Empty &&
					p.FileName == string.Empty
				)));
			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Return(inboxAccessorMock).Repeat.Times(2);
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RefreshAccessToken("TestAccessToken", "TestRefreshToken", "HYECMT.GB123456789000.ABC");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken" && x.CD_EH == Guid.Parse("eb8c1ede-9808-4ce3-aeab-6fa27c126d0f"));
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "QGbWG8KckncuwwD4uYXgWxF4HQvuPmrmUqKgkpQP");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "unJkSs5cvs8CS9E4DLvTkNhcRBq9BwUPm23cr3pF");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "GB123456789000.ABC");

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
			inboxAccessorMock.VerifyAllExpectations();
		}

		[Test]
		public void Test_RefreshAppwideToken_Success()
		{
			var config = new TestConfiguration
			{
				UseMockForGBCustoms = true,
				UseMockForLogger = true,
				UseMockForDbContext = true,
				BaseURI = "https://localhost",
				APIURI = "oauth/token",
				RetryCount = 0,
				RetryInterval = 1000,

				UseMockForConfigurationHelper = true,
			};
			var credentialService = new CredentialWebServiceForTesting(config);

			var parameters = new NameValueCollection()
			{
				{ "client_secret", "77096f46-f2a9-4958-8fc3-7aa1e47f6d3f" },
				{ "client_id", "_IA7f6itaVWcMmCV074yzODIfDga" },
				{ "grant_type", "client_credentials" },
				{ "scope", "read:vat" }
			};
			var response = @"
						{
						  ""access_token"": ""NewAppWideTokenValue"",
						  ""token_type"": ""bearer"",
						  ""expires_in"": 14400,
						  ""scope"": ""read:vat""
						}";

			config.GBCustom.Expect
			(
				x => x.UploadValues
				(
					Arg<Uri>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<NameValueCollection>.Is.Anything
				)
			)
			.Return(Encoding.UTF8.GetBytes(response))
			.Repeat.Once()
			.WhenCalled(y =>
			{
				var collection = y.Arguments[2] as NameValueCollection;
				Assert.IsTrue(collection.AllKeys.All(key => collection[key] == parameters[key]));
			});

			config.DBContext.eHubClientSystemRegistrations.Add(new eHubClientSystemRegistration()
			{
				CD_PK = Guid.NewGuid(),
				CD_Attr1 = "AppWideTokenValue",
				CD_Attr2 = "https://localhost",
				CD_Code = "AppWide.CDS",
				CD_Flag1 = 0,
				CD_EH = config.DBContext.eHubClientSystems.First(x => x.EH_PK.ToString() == "3f2504e0-4f89-11d3-9a0c-0305e82c3301").EH_PK,
				eHubRegistrationType = config.DBContext.eHubRegistrationTypes.First(x => x.RT_PK.ToString() == "2d3d4631-daa2-42d2-aebe-32c1fdb45ef8")
			});

			config.ConfigurationHelper.Expect(_ => _.NewInboxAccessor).Repeat.Never();
			config.ConfigurationHelper.InternalNewGuid = () => new Guid("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
			config.ConfigurationHelper.InternalUtcNow = () => new DateTime(2022, 06, 17);

			var actualResponse = credentialService.RequestAppWideAccessToken("AppWideTokenValue", "GBCPRD.AppWide.CDS", "read:vat");
			Assert.IsTrue(actualResponse.IsSuccess);

			var clientSystemRegistration = config.DBContext.eHubClientSystemRegistrations.FirstOrDefault(x => x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken");
			Assert.IsNotNull(clientSystemRegistration);
			Assert.AreEqual(clientSystemRegistration.CD_Attr1, "NewAppWideTokenValue");
			Assert.AreEqual(clientSystemRegistration.CD_Attr2, "app-wide-token");
			Assert.AreEqual(clientSystemRegistration.CD_Code, "AppWide.CDS");

			config.GBCustom.VerifyAllExpectations();
			config.Logger.VerifyAllExpectations();
			config.DBContext.VerifyAllExpectations();
		}
	}

	#region Implementation

	public class CredentialWebServiceForTesting : CredentialWebService
	{
		public CredentialWebServiceForTesting(TestConfiguration config)
		{
			GetAccessTokenMaxRetryCount = config.RetryCount;
			GetAccessTokenRetryInterval = config.RetryInterval;
			RefreshAccessTokenMaxRetryCount = config.RetryCount;
			RefreshAccessTokenRetryInterval = config.RetryInterval;
			if (config.UseMockForGBCustoms) config.GBCustom = MockGBCustoms(config.BaseURI, config.APIURI);
			if (config.UseMockForGBCustomsTest) config.GBCustomTest = MockGBCustomsTest(config.BaseURITest, config.APIURI);
			if (config.UseMockForLogger) config.Logger = MockLogger();
			else logger = config.Logger;
			if (config.UseMockForDbContext)
			{
				config.DBContext = MockDatabase();
				config.MockTransaction = MockRepository.GenerateMock<IDbTransaction>();
				config.DBContext.Stub(x => x.BeginTransaction()).Return(config.MockTransaction);
			}
			if (config.UseMockForConfigurationHelper)
			{
				config.ConfigurationHelper = MockConfigurationHelper(config.DBContext);
				config.ConfigurationHelper.ContextFactory = () => config.DBContext;
			}
		}

		WebClient MockGBCustoms(string baseUri, string apiUri)
		{
			GBCustoms = MockRepository.GenerateStub<WebClient>();
			GBCustoms.BaseAddress = baseUri;
			Uri.TryCreate(apiUri, UriKind.Relative, out TokenAPIURI);
			return GBCustoms;
		}

		WebClient MockGBCustomsTest(string baseUri, string apiUri)
		{
			GBCustomsTest = MockRepository.GenerateStub<WebClient>();
			GBCustomsTest.BaseAddress = baseUri;
			Uri.TryCreate(apiUri, UriKind.Relative, out TokenAPIURI);
			return GBCustomsTest;
		}

		ILog MockLogger()
		{
			var mockLogger = MockRepository.GenerateMock<ILog>();
			mockLogger.Stub(x => x.IsDebugEnabled).Return(true);
			mockLogger.Stub(x => x.IsErrorEnabled).Return(true);
			mockLogger.Stub(x => x.IsInfoEnabled).Return(true);
			InternalLogger = () => mockLogger;
			logger = mockLogger;
			return logger;
		}

		eHubTransactionsContext MockDatabase()
		{
			return PrepareData();
		}

		ConfigurationHelper MockConfigurationHelper(eHubTransactionsContext context)
		{
			var mockConfigurationHandler = MockRepository.GeneratePartialMock<GBCustomsConfigurationHandler>();
			mockConfigurationHandler.ContextFactory = () => context;
			var mockConfigurationHelper = MockRepository.GeneratePartialMock<ConfigurationHelper>(mockConfigurationHandler, InternalLogger());
			InternalConfigurationHelper = () => mockConfigurationHelper;
			return mockConfigurationHelper;
		}

		public CredentialWebServiceForTesting(IConfigurationHelper mockConfigurationHelper)
		{
			InternalConfigurationHelper = () => mockConfigurationHelper;
		}

		static eHubTransactionsContext PrepareData()
		{
			var eHubClients = new TestDbSet<eHubClient>()
			{
				new eHubClient { CC_PK = new Guid("151E9739-9F3F-4D6A-B0EA-11C3ECE8266C"), CC_ID = "eHub" },
				new eHubClient { CC_PK = new Guid("27638E70-C470-47CD-BF8B-F35AE2743ADA"), CC_ID = "HYETSTCMT" }
			};

			var eHubMessageTypes = new TestDbSet<eHubMessageType>()
			{
				new eHubMessageType { DT_PK = new Guid("C1B514AA-EE4F-4103-A22E-8ADCECEB55EB"), DT_Code = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration" }
			};

			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>()
			{
				new eHubRegistrationType { RT_PK = new Guid("84b70b52-1471-4ebe-a7cb-851e41f07964"), RT_ID = "GBCustomsAuthorisationToken", RT_Description = "GB Customs Cilent Authorisation Token", RT_RegistrantType = "ClientSystem" },
				new eHubRegistrationType { RT_PK = new Guid("2d3d4631-daa2-42d2-aebe-32c1fdb45ef8"), RT_ID = "GBCustomsAccessToken", RT_Description = "GB Customs Cilent Access Token", RT_RegistrantType = "ClientSystem" },
			};

			var eHubClientSystems = new TestDbSet<eHubClientSystem>()
			{
				new eHubClientSystem { EH_PK = new Guid("eb8c1ede-9808-4ce3-aeab-6fa27c126d0f"), EH_ID = "HYECMT" },
				new eHubClientSystem { EH_PK = new Guid("342987c1-80b5-4378-9ccb-14262e002748"), EH_ID = "HYETST" },
				new eHubClientSystem { EH_PK = new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), EH_ID = "GBCPRD" },
			};

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>() { };
			var eHubInboxMessages = new TestDbSet<eHubInboxMessage>() { };
			var eHubOutboxMessages = new TestDbSet<eHubOutboxMessage>() { };
			var eHubInboxXmlContents = new TestDbSet<eHubInboxXmlContent>() { };

			var dbContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			dbContext.eHubClients = eHubClients;
			dbContext.eHubMessageTypes = eHubMessageTypes;
			dbContext.eHubClientSystems = eHubClientSystems;
			dbContext.eHubRegistrationTypes = eHubRegistrationTypes;
			dbContext.eHubClientSystemRegistrations = eHubClientSystemRegistrations;
			dbContext.eHubInboxMessages = eHubInboxMessages;
			dbContext.eHubOutboxMessages = eHubOutboxMessages;
			dbContext.eHubInboxXmlContents = eHubInboxXmlContents;
			return dbContext;
		}

		protected internal override IssueManager GetIssueManager()
		{
			return new IssueManagerForTesting();
		}
	}

	public class IssueManagerForTesting : IssueManager
	{
		public override void ReportToIssueManager(string subject, Exception exception, ILog logger, NameValueCollection appSettings, bool checkExcluding = true)
		{
			logger.Error(subject);
		}
	}

	public class TestConfiguration
	{
		public int RetryCount { get; set; }
		public int RetryInterval { get; set; }
		public string BaseURI { get; set; }
		public string APIURI { get; set; }

		public bool UseMockForDbContext { get; set; } = false;
		public bool UseMockForConfigurationHelper { get; set; } = false;
		public bool UseMockForGBCustoms { get; set; } = false;
		public bool UseMockForLogger { get; set; } = false;

		public WebClient GBCustom { get; set; }
		public ConfigurationHelper ConfigurationHelper { get; set; }
		public ILog Logger { get; set; }
		public eHubTransactionsContext DBContext { get; set; }
		public IDbTransaction MockTransaction { get; set; }

		public WebClient GBCustomTest { get; set; }
		public string BaseURITest { get; set; }
		public bool UseMockForGBCustomsTest { get; set; } = false;
	}

	#endregion

}
