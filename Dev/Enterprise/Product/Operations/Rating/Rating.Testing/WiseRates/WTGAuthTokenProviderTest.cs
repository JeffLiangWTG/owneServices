using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using AuthenticationService.Client;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Rating.Testing.WiseRates
{
	public class WTGAuthTokenProviderTest : TestCaseWithFactory
	{
		#region CargoGuide User Token

		public void TestCargoGuideUserToken_HasPermission()
		{
			AssertCargoGuideUserToken
			(
				wiseRatesCargoguideRateSearchIsAllowed: true,
				expectedRole: WTGRoles.CargoguideStandardUser
			);
		}

		public void TestCargoGuideUserToken_HasNoPermission()
		{
			AssertCargoGuideUserToken
			(
				wiseRatesCargoguideRateSearchIsAllowed: false,
				expectedRole: WTGRoles.CargoguideAutoCostingUser
			);
		}

		void AssertCargoGuideUserToken(bool wiseRatesCargoguideRateSearchIsAllowed, string expectedRole)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var oldWiseRatesCargoguideRateSearchIsAllowed = Env.Security.WiseRatesCargoguideRateSearch.IsAllowed;

				Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = wiseRatesCargoguideRateSearchIsAllowed;

				try
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
					registrationKey.DatabaseNumberForTest = 13;
					registrationKey.EnterpriseCodeForTest = "AAA";
					registrationKey.SystemIdForTest = "666";
					registrationKey.ServerCodeForTest = "CCC";
					registrationKey.PasswordForTest = "12345";
					registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

					var expectedLoginInfo = new LoginInfo
					{
						Type = LoginType.CW1Client,
						DatabaseNumber = 13,
						EnterpriseCode = "AAA",
						CompanyCode = GlbCompany.CurrentCompany.GC_Code,
						CompanyName = GlbCompany.CurrentCompany.GC_Name,
						CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
						BranchCode = GlbBranch.CurrentBranch.GB_Code,
						BranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort,
						DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code,
						ClientNumber = $"666.{GlbCompany.CurrentCompany.GC_Code}",
						ServerCode = "CCC",
						UserCode = GlbStaff.CurrentUser.GS_Code,
						UserFullName = GlbStaff.CurrentUser.GS_FullName,
						UserEmail = GlbStaff.CurrentUser.GS_EmailAddress,
						Password = "12345",
						Roles = new[] { expectedRole }
					};

					var authServiceClientMock = new Mock<IWTGAuthServiceClient>();
					authServiceClientMock
						.Setup
						(
							wtgAuthServiceClient => wtgAuthServiceClient.GetToken
							(
								It.Is<LoginInfo>(loginInfo => loginInfo.IsEquivalentTo(expectedLoginInfo)),
								It.IsAny<string>(),
								It.IsAny<CancellationToken>()
							)
						)
						.Returns(TestToken);

					(var token, var _) = GetToken(authServiceClientMock);
					AssertEquals("Token from auth service", TestToken, token);
				}
				finally
				{
					Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = oldWiseRatesCargoguideRateSearchIsAllowed;
				}
			}
		}

		(string, string) GetToken(Mock<IWTGAuthServiceClient> authServiceClientMock)
		{
			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceClientMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);

			return provider.GetToken(Guid.NewGuid().ToString());
		}

		#endregion

		public void TestGetToken_AuthServiceThrowsHttpRequestException_ReturnError()
		{
			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.IsAny<LoginInfo>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new HttpRequestException("It happens"));

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var (token, error) = provider.GetToken("McLaren");

			AssertNullOrEmpty(token);
			AssertEquals("Unable to connect to the authentication service at the moment", error);
			AssertNullOrEmpty(
				"HttpRequestException means network connectivity issue. There is nothing we can do to address the issue.",
				ErrorReporter.LastKeyReported
			);
		}

		public void TestGetToken_AuthServiceReturnsUnauthorized_ReturnError()
		{
			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.IsAny<LoginInfo>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new AuthServiceException(HttpStatusCode.Unauthorized, "Authentication failed"));

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var (token, error) = provider.GetToken("McLaren");

			AssertNullOrEmpty("token", token);
			AssertEquals("The CW1 system cannot be validated. Please check if it has a valid license.", "The CW1 system cannot be validated. Please check if it has a valid license.", error);
			AssertNullOrEmpty("No issue report is expected as this is client issue.", ErrorReporter.LastKeyReported);
		}

		public void TestGetToken_AuthServiceThrowsAuthServiceException_ReturnError()
		{
			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.IsAny<LoginInfo>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Throws(new AuthServiceException(HttpStatusCode.InternalServerError, "It happens"));

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var (token, error) = provider.GetToken("McLaren");

			AssertNullOrEmpty("token", token);
			AssertEquals(
				"Unexpected error string mismatch",
				"Unable to request an authentication token due to unexpected error. An Error Report has been sent to the development team.",
				error
			);
			AssertEquals("AuthService.GetToken.Failure", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestGetToken_TokenIsNotYetRequested_RequestTokenFromAuthService()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseNumberForTest = 13;
			registrationKey.EnterpriseCodeForTest = "AAA";
			registrationKey.SystemIdForTest = "666";
			registrationKey.ServerCodeForTest = "CCC";
			registrationKey.PasswordForTest = "12345";
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var expectedLoginInfo = new LoginInfo
			{
				Type = LoginType.CW1Client,
				DatabaseNumber = 13,
				EnterpriseCode = "AAA",
				CompanyCode = GlbCompany.CurrentCompany.GC_Code,
				CompanyName = GlbCompany.CurrentCompany.GC_Name,
				CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				BranchCode = GlbBranch.CurrentBranch.GB_Code,
				BranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort,
				DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code,
				ClientNumber = $"666.{GlbCompany.CurrentCompany.GC_Code}",
				ServerCode = "CCC",
				UserCode = GlbStaff.CurrentUser.GS_Code,
				UserFullName = GlbStaff.CurrentUser.GS_FullName,
				UserEmail = GlbStaff.CurrentUser.GS_EmailAddress,
				Password = "12345",
				Roles = new[] { WTGRoles.CargoSphereRateAdmin, WTGRoles.CargoSphereStandardUser, WTGRoles.CargoguideStandardUser },
			};

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup
				(
					wtgAuthServiceClient =>
						wtgAuthServiceClient.GetToken
						(
							It.Is<LoginInfo>(loginInfo => loginInfo.IsEquivalentTo(expectedLoginInfo)),
							It.IsAny<string>(),
							It.IsAny<CancellationToken>()
						)
				).Returns(TestToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);

			var token = provider.GetToken(Guid.NewGuid().ToString());
			AssertEquals("Token from auth service", TestToken, token.Token);

			token = provider.GetToken(Guid.NewGuid().ToString());
			AssertEquals("Token from cache", TestToken, token.Token);

			authServiceMock
				.Verify
				(
					wtgAuthServiceClient =>
						wtgAuthServiceClient.GetToken
						(
							It.Is<LoginInfo>(loginInfo => loginInfo.IsEquivalentTo(expectedLoginInfo)),
							It.IsAny<string>(),
							It.IsAny<CancellationToken>()
						),
						Times.Once
				);
		}

		#region Standard-User and Rate-Admin

		public void TestGetToken_RateAdmin()
		{
			AssertGetToken(isCargoSphereRateAdmin: true, isCargoSphereStandardUser: false);
		}

		public void TestGetToken_StandardUser()
		{
			AssertGetToken(isCargoSphereRateAdmin: false, isCargoSphereStandardUser: true);
		}

		public void TestGetToken_RateAdminAndStandardUser()
		{
			AssertGetToken(isCargoSphereRateAdmin: true, isCargoSphereStandardUser: true);
		}

		public void TestGetToken_NotRateAdminAndNotStandardUser()
		{
			AssertGetToken(isCargoSphereRateAdmin: false, isCargoSphereStandardUser: false);
		}

		void AssertGetToken(bool isCargoSphereRateAdmin, bool isCargoSphereStandardUser)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseNumberForTest = 13;
			registrationKey.EnterpriseCodeForTest = "AAA";
			registrationKey.SystemIdForTest = "666";
			registrationKey.ServerCodeForTest = "CCC";
			registrationKey.PasswordForTest = "12345";
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var roles = new List<string>();
			roles.Add(WTGRoles.CargoguideStandardUser);
			if (isCargoSphereRateAdmin)
			{
				roles.Add(WTGRoles.CargoSphereRateAdmin);
			}
			if (isCargoSphereStandardUser)
			{
				roles.Add(WTGRoles.CargoSphereStandardUser);
			}

			var expectedLoginInfo = new LoginInfo
			{
				Type = LoginType.CW1Client,
				DatabaseNumber = 13,
				EnterpriseCode = "AAA",
				CompanyCode = GlbCompany.CurrentCompany.GC_Code,
				CompanyName = GlbCompany.CurrentCompany.GC_Name,
				CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
				BranchCode = GlbBranch.CurrentBranch.GB_Code,
				BranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort,
				DepartmentCode = GlbDepartment.CurrentDepartment.GE_Code,
				ClientNumber = $"666.{GlbCompany.CurrentCompany.GC_Code}",
				ServerCode = "CCC",
				UserCode = GlbStaff.CurrentUser.GS_Code,
				UserFullName = GlbStaff.CurrentUser.GS_FullName,
				UserEmail = GlbStaff.CurrentUser.GS_EmailAddress,
				Password = "12345",
				Roles = roles.ToArray(),
			};

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup
				(
					wtgAuthServiceClient =>
						wtgAuthServiceClient.GetToken
						(
							It.Is<LoginInfo>(loginInfo => loginInfo.IsEquivalentTo(expectedLoginInfo)),
							It.IsAny<string>(),
							It.IsAny<CancellationToken>()
						)
				).Returns(TestToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var isRateAdmin = Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed;
			var isStandardUser = Env.Security.WiseRatesCargoguideRateSearch.IsAllowed;

			try
			{
				Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed = isCargoSphereRateAdmin;
				Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = isCargoSphereStandardUser;

				var provider = CreateTokenProvider(authServiceFactoryMock.Object);
				var token = provider.GetToken(Guid.NewGuid().ToString());

				AssertEquals("Token from auth service", TestToken, token.Token);
			}
			finally
			{
				Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed = isRateAdmin;
				Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = isStandardUser;
			}
		}

		#endregion

		public void TestGetToken_TheDatabaseIsProduction_GetTokenForProductionSystem()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "a@a.com";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.Is<LoginInfo>(i => !i.IsTestSystem), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(TestToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var token = provider.GetToken(Guid.NewGuid().ToString());

			AssertEquals("Token from auth service", TestToken, token.Token);
		}

		public void TestGetToken_TheDatabaseIsNotProduction_GetTokenForNonProductionSystem()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "a@a.com";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.Is<LoginInfo>(i => i.IsTestSystem), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(TestToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var token = provider.GetToken(Guid.NewGuid().ToString());

			AssertEquals("Token from auth service", TestToken, token.Token);
		}

		public void TestGetToken_CachedTokenIsExpired_RequestNewTokenFromAuthService()
		{
			// Expired Token Expiry Date: 22.08.2017
			const string expiredToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJDVzFTdXBwb3J0XFxzdnMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJzdXBwb3J0IiwiZXhwIjoxNTAzMzg0MzM4LCJpYXQiOjE1MDMyOTc5MzgsImlzcyI6Ildpc2VSYXRlcyJ9.Zk4htMPGeES8AVyKtpThMqGPBU2bcuwF5UzMCoHlfJT5L-Fv2UX5M62Y7ng4ETkYkuGUY6G_99jmfIfIVL585ga7wRx8Q4GMoRia6jYfwl6F3vaKUFwXTLPSnMRGRZw8nKCao2bVEgEgTg88LZjSjTUX6p5UbvwZ7QSQmtvRXkb3NS57CGeiwvLK77soKOLUPfE2ciIo1ZrSFOP-eIaQPPws-UKJUr71T9h0OBB5giiyI4_Ic9gKPIf4aQQp-xhGWSOg_O4pL8VtPMq1hRtVcqcNzq3kAQfVgsyu1osI6qn0L9d0nHNS4UtOA5pWBuELzRcprEQyt3qh8PrpoYoYyA";
			const string newToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJDVzFTdXBwb3J0XFxzdnMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJzdXBwb3J0IiwiZXhwIjoxNTAzMzg0Njg2LCJpYXQiOjE1MDMyOTgyODYsImlzcyI6Ildpc2VSYXRlcyJ9.h3dmUvLtM5QVsA0D2H6BTQxTRUP88YiljPPcagx1OAm5yR-GoigIDgkXcx9DLwHHpTg8LM213GDvTx8TQ_gWhjlwALPvlnLQXKMU9xBeiq2zerGuUYzb4K4igBr-NdnKNK0R_5NhW-Y3mu5sAaoGRCcKzAVbtnnQOubdEXRVKTvC63TeKg8dcXzg_SOc-8zlRzPxSE2MBngTzu5_pDL4uFGhVqviEk5usuZh9oIStYBHcFvyKpFpooTXV4q6p58AUgUo0jlKTPnRBsmrNo48yKWJv9fCQwWK5rKTC2Re-Zrc0C_XHaWUj8b_CTsT6kUOJazkR4eO2ABcQUWgM59xsA";

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.SetupSequence(s => s.GetToken(It.IsAny<LoginInfo>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(expiredToken)
				.Returns(newToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);

			var token = provider.GetToken(Guid.NewGuid().ToString());
			AssertEquals("Expired token", expiredToken, token.Token);

			token = provider.GetToken(Guid.NewGuid().ToString());
			AssertEquals("New token", newToken, token.Token);
		}

		void AssertNewTokenGenerated<T>(Func<T> getValue, Action<T> updateValue, Func<LoginInfo, T> getLoginInfoValue, T value1, T value2)
		{
			var token1 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";
			var token2 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJERERDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJEREQiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NzMzOTksImlhdCI6MTU0MzQ3MzQ1OSwiaXNzIjoiV1RHIn0.n41WFWIvQhn0vyVIQFNehmdCyo28hDHIxuxKM6OTnZc25DzftJfe5kW_cNBQF55-ElUUMARSrpl2o7a93x4GFiQaOvFhtjsGHuBKQ-FeGocUAyVds063gxeODhXk4xaWKTBTNG5NoZBCPus8TqInyJc59h5LspfPrnx9z_KA4XT04ge2wS30A-YPNk7LkHlvAftLoVp4eED8YJYLmDBNlmLdruddwOK9O38bPbEcwX9krCUgEVqkczOY41BnmCUw2UkipTyvuirCmzwCNLN7ZvwhNv_getJVLS756D130d-w9R1xIhsWyGJFK21TsaGNopjaN0kL0p57wKRX7dFzdg";

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => getLoginInfoValue(loginInfo).Equals(value1)),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token1);

			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => getLoginInfoValue(loginInfo).Equals(value2)),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token2);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var defaultValue = getValue();

			try
			{
				var provider = CreateTokenProvider(authServiceFactoryMock.Object);

				updateValue(value1);
				AssertEquals(token1, provider.GetToken(Guid.NewGuid().ToString()).Token);

				updateValue(value2);
				AssertEquals(token2, provider.GetToken(Guid.NewGuid().ToString()).Token);
			}
			finally
			{
				updateValue(defaultValue);
			}
		}

		public void TestGetToken_CompanyCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbCompany.CurrentCompany.GC_Code, code => GlbCompany.CurrentCompany.GC_Code = code, login => login.CompanyCode, "AAA", "BBB");
		}

		public void TestGetToken_CompanyNameChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbCompany.CurrentCompany.GC_Name, code => GlbCompany.CurrentCompany.GC_Name = code, login => login.CompanyName, "WTG", "CargoSphere");
		}

		public void TestGetToken_BranchCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbBranch.CurrentBranch.GB_Code, code => GlbBranch.CurrentBranch.GB_Code = code, login => login.BranchCode, "XXX", "ZZZ");
		}

		public void TestGetToken_BranchPortChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbBranch.CurrentBranch.GB_RL_NKHomePort, code => GlbBranch.CurrentBranch.GB_RL_NKHomePort = code, login => login.BranchPort, "AUCCC", "AUXXX");
		}

		public void TestGetToken_DepartmentCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbDepartment.CurrentDepartment.GE_Code, code => GlbDepartment.CurrentDepartment.GE_Code = code, login => login.DepartmentCode, "WWW", "ZZZ");
		}

		public void TestGetToken_UserCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbStaff.CurrentUser.GS_Code, code => GlbStaff.CurrentUser.GS_Code = code, login => login.UserCode, "AZI", "SSS");
		}

		public void TestGetToken_UserFullNameChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbStaff.CurrentUser.GS_FullName, code => GlbStaff.CurrentUser.GS_FullName = code, login => login.UserFullName, "User A", "User B");
		}

		public void TestGetToken_UserEmailAddressChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => GlbStaff.CurrentUser.GS_EmailAddress, code => GlbStaff.CurrentUser.GS_EmailAddress = code, login => login.UserEmail, "support@wtg.com", "support@wisetech.com");
		}

		public void TestGetToken_IsCargoSphereRateAdminChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated
			(
				getValue: () => Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed,
				updateValue: code => Env.Security.WiseRatesCargoSphereContractManagement.IsAllowed = code,
				getLoginInfoValue: login => login.Roles.Contains(WTGRoles.CargoSphereRateAdmin),
				value1: true,
				value2: false
			);
		}

		public void TestGetToken_IsCargoSphereStandardUserChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated
			(
				getValue: () => Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed,
				updateValue: code => Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = code,
				getLoginInfoValue: login => login.Roles.Contains(WTGRoles.CargoSphereStandardUser),
				value1: true,
				value2: false
			);
		}

		public void TestGetToken_EnterpriseCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest, code => ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = code, login => login.EnterpriseCode, "OOO", "EDD");
		}

		public void TestGetToken_DatabaseNumberChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated(() => ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest, code => ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseNumberForTest = code, login => login.DatabaseNumber, 13, 15);
		}

		public void TestGetToken_ServerCodeChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGenerated<ZString>(() => ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest, code => ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = code, login => login.ServerCode, "DDD", "CCC");
		}

		public void TestGetToken_OverridenEnterpriseCodeChanged_EnsureNewTokenGenerated()
		{
			var token1 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";
			var token2 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJERERDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJEREQiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NzMzOTksImlhdCI6MTU0MzQ3MzQ1OSwiaXNzIjoiV1RHIn0.n41WFWIvQhn0vyVIQFNehmdCyo28hDHIxuxKM6OTnZc25DzftJfe5kW_cNBQF55-ElUUMARSrpl2o7a93x4GFiQaOvFhtjsGHuBKQ-FeGocUAyVds063gxeODhXk4xaWKTBTNG5NoZBCPus8TqInyJc59h5LspfPrnx9z_KA4XT04ge2wS30A-YPNk7LkHlvAftLoVp4eED8YJYLmDBNlmLdruddwOK9O38bPbEcwX9krCUgEVqkczOY41BnmCUw2UkipTyvuirCmzwCNLN7ZvwhNv_getJVLS756D130d-w9R1xIhsWyGJFK21TsaGNopjaN0kL0p57wKRX7dFzdg";

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => loginInfo.EnterpriseCodeOverride == "XXX"),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token1);

			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => loginInfo.EnterpriseCodeOverride == "YYY"),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token2);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);

			AssertEquals(token1, provider.GetToken(Guid.NewGuid().ToString(), 5, info => info.EnterpriseCodeOverride = "XXX").Token);
			AssertEquals(token2, provider.GetToken(Guid.NewGuid().ToString(), 5, info => info.EnterpriseCodeOverride = "YYY").Token);
		}

		public void TestGetToken_OverridenSecurityAccessRightsChanged_EnsureNewTokenGenerated()
		{
			AssertNewTokenGeneratedForSecurityAccessRights(
				new[]
				{
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Air,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					},
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Ocean,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					}
				},
				new[]
				{
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Air,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					},
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Ocean,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DNZ" },
							Countries = new[] { "AU" }
						}
					}
				}
			);

			AssertNewTokenGeneratedForSecurityAccessRights(
				new[]
				{
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Air,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					},
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Ocean,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					}
				},
				new[]
				{
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Air,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					},
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Ocean,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "NZ" }
						}
					}
				}
			);

			AssertNewTokenGeneratedForSecurityAccessRights(
				Array.Empty<UrsSecurityAccessRights>(),
				new[]
				{
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Air,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DAU" },
							Countries = new[] { "AU" }
						}
					},
					new UrsSecurityAccessRights
					{
						TransportMode = TransportMode.Ocean,
						Allowed = new UrsSecurityAccessList
						{
							Companies = new[] { "DNZ" },
							Countries = new[] { "AU" }
						}
					}
				}
			);
		}

		void AssertNewTokenGeneratedForSecurityAccessRights(IEnumerable<UrsSecurityAccessRights> accessRights1, IEnumerable<UrsSecurityAccessRights> accessRights2)
		{
			var token1 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";
			var token2 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJERERDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJEREQiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NzMzOTksImlhdCI6MTU0MzQ3MzQ1OSwiaXNzIjoiV1RHIn0.n41WFWIvQhn0vyVIQFNehmdCyo28hDHIxuxKM6OTnZc25DzftJfe5kW_cNBQF55-ElUUMARSrpl2o7a93x4GFiQaOvFhtjsGHuBKQ-FeGocUAyVds063gxeODhXk4xaWKTBTNG5NoZBCPus8TqInyJc59h5LspfPrnx9z_KA4XT04ge2wS30A-YPNk7LkHlvAftLoVp4eED8YJYLmDBNlmLdruddwOK9O38bPbEcwX9krCUgEVqkczOY41BnmCUw2UkipTyvuirCmzwCNLN7ZvwhNv_getJVLS756D130d-w9R1xIhsWyGJFK21TsaGNopjaN0kL0p57wKRX7dFzdg";

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => loginInfo.UrsSecurityAccessRights == accessRights1),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token1);

			authServiceMock.Setup
			(
				authServiceClient =>
					authServiceClient.GetToken
					(
						It.Is<LoginInfo>(loginInfo => loginInfo.UrsSecurityAccessRights == accessRights2),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()
					)
			).Returns(token2);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);

			AssertEquals(token1, provider.GetToken(Guid.NewGuid().ToString(), 5, info => info.UrsSecurityAccessRights = accessRights1).Token);
			AssertEquals(token2, provider.GetToken(Guid.NewGuid().ToString(), 5, info => info.UrsSecurityAccessRights = accessRights2).Token);
		}

		public void TestGetToken_SSLCertificateIsInvalid_ShowErrorMessageBox()
		{
#if !WINZOR
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => false;
#endif
			UnitTestUserNotification.Instance.ClearShownErrorKeys();

			try
			{
				for (var i = 0; i < 5; i++)
				{
#if WINZOR
					var handler = new HttpClientHandler();
					handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => false;
					var serviceClientFactory = new Mock<IWTGAuthServciceClientFactory>();
					serviceClientFactory.Setup(factory => factory.Create(It.IsAny<string>()))
						.Returns((string serviceURL) => new WTGAuthServiceClient(serviceURL, handler));
					var provider = new WTGAuthTokenProvider(serviceClientFactory.Object);
#else
					var provider = new WTGAuthTokenProvider();
#endif
					(var token, var error) = provider.GetToken(Guid.NewGuid().ToString());

					AssertNull("Token", token);
					AssertEquals("Error", "Unable to connect to the authentication service at the moment", error);

					if (i == 0)
					{
						AssertEquals(
							"MessageBox Error after the first occurrence",
							"Failed to validate the SSL/TLS certificate of WiseTech Global service at 'https://auth.wisegrid.net'. Please contact your system administrator.",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals("MessageBox Error must not be shown for subsequent occurrences", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
			finally
			{
				ServicePointManager.ServerCertificateValidationCallback = null;
			}
		}

		public void TestGetToken_NonSuccessStatusCode_503_RetryTwiceThenGiveUp()
		{
			// The first call is not a retry. So it becomes 0
			// The second call is the first retry, so it becomes 1
			// The third call is the second retry, so it becomes 2
			var retryCount = -1;
			var codeToRespond = HttpStatusCode.ServiceUnavailable;
			var expectedMessage = "CargoWise could NOT get an authentication token due to the service being unavailable. Please check your internet connection and try again in a few minutes.";

			var testURL = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (uri, request) =>
				{
					retryCount++;
					return new Tuple<int, string>((int)codeToRespond, $"This is a {codeToRespond} response");
				},
				Uri = new Uri(testURL),
				ContentType = "text/plain"
			};
			testExternalValidationService.Start();

			try
			{
				using (RatingDataRegistry.Instance.WTGAuthServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testURL))
				{
					var tokenProvider = new WTGAuthTokenProvider();
					var (token, message) = tokenProvider.GetToken("1234");

					AssertNull("token should be null on failure", token);
					AssertEquals("expect nice user friendly message", expectedMessage, message);
				}

				AssertEquals("Should retry 2 times before giving up", 2, retryCount);
			}
			finally
			{
				testExternalValidationService.Stop();
			}

			AssertEquals("Should have no error report", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestGetToken_NonSuccessStatusCode_408_RetryTwiceThenGiveUp()
		{
			// The first call is not a retry. So it becomes 0
			// The second call is the first retry, so it becomes 1
			// The third call is the second retry, so it becomes 2
			var retryCount = -1;
			var codeToRespond = HttpStatusCode.RequestTimeout;
			var expectedMessage = "CargoWise could NOT get an authentication token due to time out. Please check your internet connection and try again in a few minutes.";

			var testURL = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (uri, request) =>
				{
					retryCount++;
					return new Tuple<int, string>((int)codeToRespond, $"This is a {codeToRespond} response");
				},
				Uri = new Uri(testURL),
				ContentType = "text/plain"
			};
			testExternalValidationService.Start();

			try
			{
				using (RatingDataRegistry.Instance.WTGAuthServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testURL))
				{
					var tokenProvider = new WTGAuthTokenProvider();
					var (token, message) = tokenProvider.GetToken("1234");

					AssertNull("token should be null on failure", token);
					AssertEquals("expect nice user friendly message", expectedMessage, message);
				}

				AssertEquals("Should retry 2 times before giving up", 2, retryCount);
			}
			finally
			{
				testExternalValidationService.Stop();
			}

			AssertEquals("Should have zero error reports", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestGetToken_OverrideLoginInfo()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "a@a.com";

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			var overrideLoginInfo = new Action<LoginInfo>((loginInfo) =>
			{
				loginInfo.ClientCompanyCode = "ORGCODE";
				loginInfo.ClientCompanyName = "Org Name";
				loginInfo.UserEmail = "contact@website.com";
			});

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup(s => s.GetToken(It.Is<LoginInfo>(i => i.ClientCompanyCode == "ORGCODE" && i.ClientCompanyName == "Org Name" && i.UserEmail == "contact@website.com"), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(TestToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = CreateTokenProvider(authServiceFactoryMock.Object);
			var token = provider.GetToken(Guid.NewGuid().ToString(), overrideLoginInfo: overrideLoginInfo);

			AssertEquals("Token from auth service", TestToken, token.Token);
		}

		WTGAuthTokenProvider CreateTokenProvider(IWTGAuthServciceClientFactory authServiceClientFactory) =>
			new(authServiceClientFactory);

		// Expiry Date: 04.08.2028
		// If the test fails, the Rates Service and CW1 are still alive, generate a new test token with extended expiry date
		const string TestToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJDVzFTdXBwb3J0XFxzdnMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJzdXBwb3J0IiwiZXhwIjoxODQ5MDQzODY1LCJpYXQiOjE1MDM0NDM4NjUsImlzcyI6Ildpc2VSYXRlcyJ9.bGHN-5LvKaIp401_uGSaA26L543RwsKv12wD4dlys9Tw2i-kXvpwcRxQxX0juNzrK3LDLPoJwzzDIhMGGCyOiDojn_lQEsD3GZgOxxsYpT-JVfU4_I9AIVAcsrjO-5unDIdZfXEkgNGI26OuausaBJwvJV2zLLRIwp0abB2kR1QnDMvtE12PNeJ5zJL3OTzxOxhSwKJNBf29mD4nlW5w-yAs754q5KK7KecnPkn7al4qquUVpIhzAJOJPhT-PhSKCRzWNDtLGYeL2rCR7L7fw9AYSSQXpcmf7dgCHP8T4gslBkSbxv5_AXgEPPF8YGw0JtYCZ-I4Jluy93slKp6XNg";
	}
}
