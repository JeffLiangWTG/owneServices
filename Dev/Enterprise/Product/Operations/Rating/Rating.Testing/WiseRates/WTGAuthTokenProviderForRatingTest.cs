using System;
using System.Collections.Generic;
using System.Threading;
using AuthenticationService.Client;
using AuthenticationService.Client.Models;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.Testing.WiseRates;

public class WTGAuthTokenProviderForRatingTest : TestCaseWithFactory
{
	const string AirSecurityRight = "WiseRatesCargoguideRateSearchCompanyAccess";
	const string SeaSecurityRight = "WiseRatesCargoSphereRateSearchCompanyAccess";

	public void TestGetToken_SystemCodeIsOverridenInTheRegistry_UseSystemCodeFromTheRegistry()
	{
		var aaaToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";
		var bbbToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJERERDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJEREQiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NzMzOTksImlhdCI6MTU0MzQ3MzQ1OSwiaXNzIjoiV1RHIn0.n41WFWIvQhn0vyVIQFNehmdCyo28hDHIxuxKM6OTnZc25DzftJfe5kW_cNBQF55-ElUUMARSrpl2o7a93x4GFiQaOvFhtjsGHuBKQ-FeGocUAyVds063gxeODhXk4xaWKTBTNG5NoZBCPus8TqInyJc59h5LspfPrnx9z_KA4XT04ge2wS30A-YPNk7LkHlvAftLoVp4eED8YJYLmDBNlmLdruddwOK9O38bPbEcwX9krCUgEVqkczOY41BnmCUw2UkipTyvuirCmzwCNLN7ZvwhNv_getJVLS756D130d-w9R1xIhsWyGJFK21TsaGNopjaN0kL0p57wKRX7dFzdg";

		var authServiceMock = new Mock<IWTGAuthServiceClient>();
		authServiceMock
			.Setup
			(
				wtgAuthServiceClient => wtgAuthServiceClient.GetToken(
					It.Is<LoginInfo>(loginInfo => loginInfo.EnterpriseCodeOverride == null),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>())
			).Returns(aaaToken);
		authServiceMock
			.Setup
			(
				wtgAuthServiceClient => wtgAuthServiceClient.GetToken(
					It.Is<LoginInfo>(loginInfo => loginInfo.EnterpriseCodeOverride == "BBB"),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>())
			).Returns(bbbToken);

		var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
		authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

		var environmentMock = new Mock<ICargoWiseEnvironment>();
		environmentMock.Setup(e => e.IsCargoWiseDomain(It.IsAny<string>())).Returns(true);

		var provider = new WTGAuthTokenProviderForRating(authServiceFactoryMock.Object, environmentMock.Object);

		var (token, error) = provider.GetToken("mclaren");
		Assert("Should use EnterpriseCode from the license as it is not overriden in the registry", token == aaaToken);

		using (RatingDataRegistry.Instance.EnterpriseCodeOverride.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "BBB"))
		{
			(token, error) = provider.GetToken("mclaren");
			Assert("Should use EnterpriseCode from the registry as it overrides the code in the license", token == bbbToken);

			// Simulating the CW1 system is run in the client environment. We should not allow overriding the enterprise code even theoretically
			// as it may have crucial impact on the client and WTG businesses.
			environmentMock = new Mock<ICargoWiseEnvironment>();
			environmentMock.Setup(e => e.IsCargoWiseDomain(It.IsAny<string>())).Returns(false);

			provider = new WTGAuthTokenProviderForRating(authServiceFactoryMock.Object, environmentMock.Object);
			(token, error) = provider.GetToken("mclaren");
			Assert("Should use EnterpriseCode from the license even though it is overriden in the registry as it is production CW1 instance run in the client environment", token == aaaToken);
		}
	}

	public void TestGetSecurityAccessRights()
	{
		CreateCompanyWithBranch("DUA", "UA", "UBR");
		CreateCompanyWithBranch("DNZ", "NZ", "NBR");
		CreateCompanyWithBranch("DSG", "SG", "SBR");

		AssertSecurityAccessRights(
			"Case: User All Allowed",
			groupRights: [],
			userRights: ["*|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["*"],
					Countries = ["*"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User All allowed, single company allowed",
			groupRights: [],
			userRights: ["*|*|true", "DUA|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["*"],
					Countries = ["*"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Company branch allowed",
			groupRights: [],
			userRights: ["*|UBR|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: All companies allowed, only company branch not allowed",
			groupRights: [],
			userRights: ["*|UBR|false", "*|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User Single company allowed",
			groupRights: [],
			userRights: ["*|*|false", "DUA|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User All allowed, single company not allowed",
			groupRights: [],
			userRights: ["*|*|true", "DUA|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User One company allowed, one not allowed",
			groupRights: [],
			userRights: ["DUA|*|false", "DNZ|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DNZ"],
					Countries = ["NZ"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User multiple allowed",
			groupRights: [],
			userRights: ["DUA|*|true", "DNZ|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA", "DNZ"],
					Countries = ["UA", "NZ"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: User same company allowed and not allowed, different branches",
			groupRights: [],
			userRights: ["DUA|UBR|false", "DUA|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group one allowed company",
			groupRights: ["DUA|*|true"],
			userRights: [],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group not allowed company, user allowed same company",
			groupRights: ["DUA|*|false"],
			userRights: ["DUA|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group all allowed, user one not allowed",
			groupRights: ["*|*|true"],
			userRights: ["DUA|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group allowed company, staff not allowed same company",
			groupRights: ["DUA|*|true"],
			userRights: ["DUA|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["*"],
					Countries = ["*"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group all allowed, staff all not allowed",
			groupRights: ["*|*|true"],
			userRights: ["*|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["*"],
					Countries = ["*"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group one company not allowed, staff all allowed",
			groupRights: ["DUA|*|false"],
			userRights: ["*|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["*"],
					Countries = ["*"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group all not allowed and one company allowed, staff one company not allowed",
			groupRights: ["*|*|false", "DNZ|*|true"],
			userRights: ["DUA|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DNZ"],
					Countries = ["NZ"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group and staff mixed allowed/not",
			groupRights: ["*|*|false", "DNZ|*|true", "DUA|*|true"],
			userRights: ["DUA|*|false", "DSG|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DNZ", "DSG"],
					Countries = ["NZ", "SG"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group same company allowed and not allowed, different branches",
			groupRights: ["DUA|UBR|false", "DUA|*|true"],
			userRights: [],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA"],
					Countries = ["UA"]
				}
			}
		);

		AssertSecurityAccessRights(
			"Case: Group one company allowed, user all not allowed",
			groupRights: ["DUA|*|true"],
			userRights: ["*|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				 {
					Companies = ["*"],
					Countries = ["*"]
				 }
			}
		);
	}

	public void TestGetSecurityAccessRights_MultipleCompaniesInTheSameCountry()
	{
		var dua = CreateCompanyWithBranch("DUA", "UA", "BR1");
		var dgb = CreateCompanyWithBranch("DGB", "GB", "BR2");
		var dde = CreateCompanyWithBranch("DDE", "DE", "BR3");
		var dnl = CreateCompanyWithBranch("DNL", "NL", "BR4");
		dnl.GC_IsActive = false;

		SetCommunityRegionsForCompany(dgb.PK, ["UA"]);
		SetCommunityRegionsForCompany(dnl.PK, ["DE"]);

		AssertSecurityAccessRights(
			"Case: Two companies in the same country, one allowed another one is not, the country should be included",
			groupRights: ["*|*|true"],
			userRights: ["*|*|false", "DUA|*|true", "DDE|*|true"],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				 {
					Companies = ["DUA", "DDE"],
					Countries = ["DE", "UA"]
				 }
			}
		);

		AssertSecurityAccessRights(
			"Case: Two companies in the same country, one allowed another one is not, the country should not be included",
			groupRights: ["*|*|true"],
			userRights: ["*|*|true", "DUA|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				 {
					Companies = ["DUA"],
					Countries = null
				 }
			}
		);

		AssertSecurityAccessRights(
			"Case: Two companies in the same country, both not allowed, the country should be included",
			groupRights: ["*|*|true"],
			userRights: ["*|*|true", "DUA|*|false", "DGB|*|false"],
			expectedRights: new UrsSecurityAccessRights
			{
				Denied = new UrsSecurityAccessList
				{
					Companies = ["DUA", "DGB"],
					Countries = ["UA", "GB"]
				}
			}
		);
	}

	public void TestGetSecurityAccessRights_UserIsController()
	{
		var companyUA = CreateCompanyWithBranch("DUA", "UA", "UBR");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_LoginName = "tyler";
		staff.GS_IsController = true;

		AddSecurityRight(staff, AirSecurityRight, companyUA.PK);
		AddSecurityRight(staff, SeaSecurityRight, companyUA.PK);

		Factory.Save();

		AssertSecurityAccessRights(
			"User is controller",
			groupRights: [],
			userRights: [],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList { Companies = ["*"], Countries = ["*"] }
			},
			staff
		);
	}

	public void TestGetSecurityAccessRights_MultipleStaffGroups()
	{
		var companyUA = CreateCompanyWithBranch("DUA", "UA", "UBR");
		var companyNZ = CreateCompanyWithBranch("DNZ", "NZ", "NBR");

		var groupLink = Factory.New<GlbGroupLink>();
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_LoginName = "tyler";

		var group = Factory.NewWithValidTestData<GlbGroup>();
		groupLink.GK_GG = group.PK;
		groupLink.GK_GS = staff.PK;

		var group2 = Factory.NewWithValidTestData<GlbGroup>();
		var groupLink2 = Factory.New<GlbGroupLink>();
		groupLink2.GK_GG = group2.PK;
		groupLink2.GK_GS = staff.PK;

		AddSecurityRight(group, AirSecurityRight, companyUA.PK);
		AddSecurityRight(group, SeaSecurityRight, companyUA.PK);

		AddSecurityRight(group2, AirSecurityRight, companyNZ.PK);
		AddSecurityRight(group2, SeaSecurityRight, companyNZ.PK);

		Factory.Save();

		AssertSecurityAccessRights(
			"Case: Multiple groups",
			groupRights: [],
			userRights: [],
			expectedRights: new UrsSecurityAccessRights
			{
				Allowed = new UrsSecurityAccessList
				{
					Companies = ["DUA", "DNZ"],
					Countries = ["UA", "NZ"]
				}
			},
			staff
		);
	}

	public void TestSecurityAccessRights_UrsDisabled_ShouldNotBeIncluded()
	{
		var companyUA = CreateCompanyWithBranch("DUA", "UA", "UBR");
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_LoginName = "tyler";
		staff.GS_IsController = true;

		AddSecurityRight(staff, AirSecurityRight, companyUA.PK);
		AddSecurityRight(staff, SeaSecurityRight, companyUA.PK);

		Factory.Save();

		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
		using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var expectedToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup
				(
					wtgAuthServiceClient => wtgAuthServiceClient.GetToken(
						It.Is<LoginInfo>(loginInfo => loginInfo.UrsSecurityAccessRights == null),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>())
				).Returns(expectedToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = new WTGAuthTokenProviderForRating(authServiceFactoryMock.Object, Mock.Of<ICargoWiseEnvironment>());
			var (token, _) = provider.GetToken("mclaren");
			Assert("URS Security Rights should not be included in the token since URS is disabled in the registry", token == expectedToken);
		}
	}

	void AssertSecurityAccessRights(
		string message,
		string[] groupRights,
		string[] userRights,
		UrsSecurityAccessRights expectedRights,
		GlbStaff staff = null)
	{
		if (staff == null)
		{
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = $"test_{Guid.NewGuid()}";
		}

		foreach (var userRight in userRights)
		{
			var (company, branch, allowed) = ParseRight(userRight);
			AddSecurityRight(staff, AirSecurityRight, company, branch, ZGuid.Empty, allowed);
			AddSecurityRight(staff, SeaSecurityRight, company, branch, ZGuid.Empty, allowed);
		}

		if (groupRights != null)
		{
			var groupLink = Factory.New<GlbGroupLink>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;

			foreach (var groupRight in groupRights)
			{
				var (company, branch, allowed) = ParseRight(groupRight);
				AddSecurityRight(group, AirSecurityRight, company, branch, allowed);
				AddSecurityRight(group, SeaSecurityRight, company, branch, allowed);
			}
		}

		Factory.Save();

		using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var expectedToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBQUFDQ0MiLCJEYXRhYmFzZU51bWJlciI6MTMsIlN5c3RlbUNvZGUiOiJBQUEiLCJDb21wYW55Q29kZSI6IkVESSIsIkNvbXBhbnlOYW1lIjoiQ1NDb21wYW55IiwiQnJhbmNoQ29kZSI6IkJORSIsIkJyYW5jaFBvcnQiOiJBVUJORSIsIkRlcGFydG1lbnRDb2RlIjoiQlJOIiwiQ2xpZW50TnVtYmVyIjoiNjY2LkVESSIsIlNlcnZlckNvZGUiOiJDQ0MiLCJVc2VyQ29kZSI6IkUiLCJVc2VyRnVsbE5hbWUiOiJDYXJnb1dpc2UgT25lIFN1cHBvcnQiLCJVc2VyRW1haWwiOiJzdXBwb3J0QHd0Zy5jb20iLCJleHAiOjc1NDM0NjkzODcsImlhdCI6MTU0MzQ2OTQ0NywiaXNzIjoiV1RHIn0.QYUK0bkYiu9KjSVBywl6mSKf6cDyVMeYy06UhqsALKpZAKHLeVzKn7qZKiuzD2AYlZ8ERh7LSvYmCspeDRnxI5KmK4vxCHsn0geu_MgLJ3nQE3ok5LtnusJvGMpyLZRnJFk8v-phWRUfREzqMGvq7LGeivWRvMYgMc73dKsuykv5-nPV9aqQbfjsXBUHhG1St-oq3fFHmcCoSbEik1hycrKrwbtyAOLYPmSRteGOoNYv_zXEPhRRAy-iIOs6ybpAH3Imbhi7X6awQhewuc3xT7TW0OanWV3PjctaXKjfd5HMLJ0AkMCvqk7tc5Fpnu6-4wrv2ji9f6fPTReptSZejw";

			var airExpected = new UrsSecurityAccessRights
			{
				TransportMode = TransportMode.Air,
				Allowed = expectedRights.Allowed,
				Denied = expectedRights.Denied
			};

			var seaExpected = new UrsSecurityAccessRights
			{
				TransportMode = TransportMode.Ocean,
				Allowed = expectedRights.Allowed,
				Denied = expectedRights.Denied
			};

			var authServiceMock = new Mock<IWTGAuthServiceClient>();
			authServiceMock
				.Setup
				(
					wtgAuthServiceClient => wtgAuthServiceClient.GetToken(
						It.Is<LoginInfo>(loginInfo => loginInfo.Audience == "URS" && loginInfo.UrsSecurityAccessRights.IsEquivalentTo(new List<UrsSecurityAccessRights>(new [] { airExpected, seaExpected }))),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>())
				).Returns(expectedToken);

			var authServiceFactoryMock = new Mock<IWTGAuthServciceClientFactory>();
			authServiceFactoryMock.Setup(s => s.Create(It.IsAny<string>())).Returns(authServiceMock.Object);

			var provider = new WTGAuthTokenProviderForRating(authServiceFactoryMock.Object, Mock.Of<ICargoWiseEnvironment>());
			var (token, _) = provider.GetToken("mclaren");
			Assert(message, token == expectedToken);
		}
	}

	#region Implementation

	void AddSecurityRight(GlbStaff staff, string securityRight, ZGuid company, ZGuid branch = default, ZGuid department = default, bool allowed = true)
	{
		var glbSecurity = staff.StaffSecurityPermissionsCollection.AddNew();
		glbSecurity.GU_SecurityRight = securityRight;
		glbSecurity.GU_SecurityItemIsAllowed = allowed;
		glbSecurity.GU_GC = company;
		glbSecurity.GU_GB = branch;
		glbSecurity.GU_GE = department;
	}

	void AddSecurityRight(GlbGroup group, string securityRight, ZGuid company, ZGuid branch = default, bool allowed = true)
	{
		var glbSecurity = group.SecurityPermissions.AddNew();
		glbSecurity.GU_SecurityRight = securityRight;
		glbSecurity.GU_SecurityItemIsAllowed = allowed;
		glbSecurity.GU_GC = company;
		glbSecurity.GU_GB = branch;
	}

	(ZGuid company, ZGuid branch, bool allowed) ParseRight(string right)
	{
		var parts = right.Split('|');

		var companyQuery = new ZQuery(GlbCompanySchema.GC_Code, parts[0]);
		var company = parts[0] == "*" ? ZGuid.Empty : Factory.LoadTop1<GlbCompany>(companyQuery).PK;

		var branchQuery = new ZQuery(GlbBranchSchema.GB_Code, parts[1]);
		var branch = parts[1] == "*" ? ZGuid.Empty : Factory.LoadTop1<GlbBranch>(branchQuery).PK;

		var allowed = bool.Parse(parts[2]);
		return (company, branch, allowed);
	}

	IDisposable SetCommunityRegionsForCompany(ZGuid company, IEnumerable<string> countries)
	{
		var countryPks = new List<Guid>();

		foreach (var countryCode in countries)
		{
			var query = new ZQuery(RefCountrySchema.RN_Code, countryCode);
			var country = Factory.LoadTop1<RefCountry>(query);
			countryPks.Add(country.PK.ToGuid());
		}

		return FreightDataRegistry.Instance.CommunityRegionsForDirectionCalculation.SetTemporaryValue(company.ToGuid(), Guid.Empty, Guid.Empty, countryPks.ToArray());
	}

	GlbCompany CreateCompanyWithBranch(string companyCode, string countryCode, string branchCode)
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_Code = companyCode;
		company.GC_RN_NKCountryCode = countryCode;

		var branch = company.Branches.AddNew();
		branch.GB_Code = branchCode;

		return company;
	}

	#endregion

}
