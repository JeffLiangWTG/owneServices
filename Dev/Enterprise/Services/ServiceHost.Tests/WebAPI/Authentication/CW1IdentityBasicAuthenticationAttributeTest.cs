using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Test
{
	[UseSnapshotProtection]
	public class CW1IdentityBasicAuthenticationAttributeTest : TestCase
	{
		public void TestAuthenticateAsyncInThread()
		{
			var authentication = GetAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"someuser:somepassword")));
			request.Headers.Authorization = authorization;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				}
			});

			thread.Start();
			thread.Join();

			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncCorruptedBase64()
		{
			// Arrange
			var authentication = GetAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", "pretendToBeBase64String");
			request.Headers.Authorization = authorization;

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Corrupted authentication data.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncInvalidAmountOfStrings()
		{
			// Arrange
			var authentication = GetAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var context = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue(
				"Basic",
				Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test2:test3")));
			request.Headers.Authorization = authorization;

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Invalid authentication data.", response.ReasonPhrase);
		}

		public void TestAuthenticateAsyncTimeout()
		{
			// Arrange
			var authentication = GetAuthenticationAttribute();
			var request = new HttpRequestMessage();
			var actionContext = new HttpActionContext
			{
				ControllerContext = new HttpControllerContext
				{
					Request = request
				}
			};
			var authenticationContext = new HttpAuthenticationContext(actionContext, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test:test")));
			request.Headers.Authorization = authorization;

			using var resetHook = IdentityBasicAuthenticationAttribute.SetOnAuthenticateHookForTest((context) => { throw new InvalidOperationException("Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool."); });

			// Act
			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			// Assert
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("Failed to connect to database. Please try again later.", response.ReasonPhrase);
		}

		protected virtual IAuthenticationFilter GetAuthenticationAttribute()
		{
			return new CW1IdentityBasicAuthenticationAttribute();
		}
	}

	class CW1IdentityBasicAuthenticationAttributeTransactionedTest : TransactionedTestCase
	{
		BusinessObjectFactory Factory;
		BasicAuthTestHelper TestHelper;

		protected override void SetUp()
		{
			RegisterDbConnectionToRollback(Db.Connection);
			Factory = new BusinessObjectFactory(Db.Connection);
			TestHelper = new BasicAuthTestHelper(Factory);
		}

		[TestSemaphoreProvider]
		public void TestAuthenticateAsync()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());

			AssertNull(authenticationContext.Principal);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes("test1:test1")));
			request.Headers.Authorization = authorization;

			authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
			var errorResult = authenticationContext.ErrorResult;
			var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
			AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response.ReasonPhrase);

			authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				Assert("Temporary log out for test.", !EnvProxy.Instance.IsLoggedIn);

				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var principal = authenticationContext.Principal;
				AssertNotNull(principal);
				Assert(principal.Identity.IsAuthenticated);
				AssertEquals(username, EnvProxy.Instance.CurrentUser.LoginName);
				AssertNotNull("Expecting user context switching after authentication.", EnvProxy.Instance.CurrentUserContext);
				AssertEquals("Expecting user context switching after authentication.", staff.PK, EnvProxy.Instance.CurrentUserContext.User.PK);
			}
		}

		public void TestChallengeAsync()
		{
			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var challengeContext = new HttpAuthenticationChallengeContext(context, new AuthenticationFailureResult("Invalid username or password", request));

			authentication.ChallengeAsync(challengeContext, new CancellationToken());
			var challengeAction = challengeContext.Result;
			var result = challengeAction.ExecuteAsync(new CancellationToken()).Result;
			Assert(result.Headers.WwwAuthenticate.Any(x => x.Scheme == IdentityBasicAuthenticationAttribute.BasicAuthenticationType));
		}

		public void TestAuthenticateAsyncWithEmptyHomeBranch()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);
			staff.GS_GB_HomeBranch = ZGuid.Empty;
			Factory.Save();

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			try
			{
				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var errorResult = authenticationContext.ErrorResult;
				var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
				AssertEquals("Staff branch is empty.", response.ReasonPhrase);
			}
			finally
			{
				EnvProxy.Instance.LoginController.Logout();
			}
		}

		public void TestAuthenticateAsyncWithInvalidHomeBranch()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);
			TestHelper.CreateANewBranchWithInvalidPK(staff.HomeBranch.GB_GC.ToGuid());
			staff.GS_GB_HomeBranch = ZGuid.Invalid;
			Factory.Save();

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			try
			{
				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var errorResult = authenticationContext.ErrorResult;
				var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
				AssertEquals("Staff branch is invalid.", response.ReasonPhrase);
			}
			finally
			{
				EnvProxy.Instance.LoginController.Logout();
			}
		}

		public void TestAuthenticateAsyncWithEmptyHomeDepartment()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);
			staff.GS_GE_HomeDepartment = ZGuid.Empty;
			Factory.Save();

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			try
			{
				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var errorResult = authenticationContext.ErrorResult;
				var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
				AssertEquals("Staff department is empty.", response.ReasonPhrase);
			}
			finally
			{
				EnvProxy.Instance.LoginController.Logout();
			}
		}

		public void TestAuthenticateAsyncWithInvalidHomeDepartment()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);
			TestHelper.CreateANewDepartmentWithInvalidPK();
			staff.GS_GE_HomeDepartment = ZGuid.Invalid;
			Factory.Save();

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			try
			{
				authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
				var errorResult = authenticationContext.ErrorResult;
				var response = errorResult.ExecuteAsync(new CancellationToken()).Result;
				AssertEquals(HttpStatusCode.Unauthorized, response.StatusCode);
				AssertEquals("Staff department is invalid.", response.ReasonPhrase);
			}
			finally
			{
				EnvProxy.Instance.LoginController.Logout();
			}
		}

		public void TestAuthenticateAsyncWithUserContextSwitchingNoThrow()
		{
			var username = "testuser";
			var password = "42034793";
			var staff = TestHelper.CreateStaff(username, password, "tst", true, false);
			Factory.Save();

			var authentication = new CW1IdentityBasicAuthenticationAttribute();

			var request = new HttpRequestMessage();
			var controllerContext = new HttpControllerContext();
			controllerContext.Request = request;

			var context = new HttpActionContext();
			context.ControllerContext = controllerContext;
			var authenticationContext = new HttpAuthenticationContext(context, null);

			var authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
				Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}")));
			request.Headers.Authorization = authorization;
			authenticationContext.ErrorResult = null;

			try
			{
				var webUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.WebUserName);
				var webContext = new Environment.UserContext(webUser.PK.ToGuid(), DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment);
				using (EnvProxy.Instance.SetTemporaryUserContext(webContext))
				{
					Assert("Web environment should have web user logged in.", EnvProxy.Instance.IsLoggedIn);
					authentication.AuthenticateAsync(authenticationContext, new CancellationToken());
					var principal = authenticationContext.Principal;
					AssertNotNull(principal);
					Assert(principal.Identity.IsAuthenticated);
					AssertEquals(username, EnvProxy.Instance.CurrentUser.LoginName);
					AssertNotNull("Expecting user context switching after authentication.", EnvProxy.Instance.CurrentUserContext);
					AssertEquals("Expecting user context switching after authentication.", staff.PK, EnvProxy.Instance.CurrentUserContext.User.PK);
				}
			}
			finally
			{
				EnvProxy.Instance.LoginController.Logout();
			}
		}
	}

	class BasicAuthTestHelper
	{
		public BasicAuthTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		BusinessObjectFactory Factory { get; }
		GlbStaff CreateStaffCore(GlbStaff staff, string loginName, string password, string code, bool active, bool resource, bool isOperational, bool isController, bool canLogin, bool twoFactorEnabled)
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var branch = CreateANewBranch("xyz", "xyz");
			var department = CreateANewDepartment("fbi");
			staff.GS_PER = person.PK;
			staff.GS_IsResource = resource;
			staff.GS_CanLogin = canLogin;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.ResetPassword(password);
			staff.GS_IsActive = active;
			staff.GS_IsOperational = isOperational;
			staff.GS_IsController = isController;
			staff.GS_IsTwoFactorAuthenticationEnabled = twoFactorEnabled;
			staff.ChangePasswordAtNextLogin = false;
			staff.GS_EmailAddress = "e@mail.com";
			person.PER_City = "ABC";
			person.PER_FullName = "ABC DEF";
			person.PER_HomeAddress1 = "GHI";
			person.PER_RN_NKCountry = "AU";
			staff.GS_City = "ABC";
			staff.GS_FullName = "ABC DEF";
			staff.GS_UserAddress1 = "GHI";
			staff.GS_RN_NKCountryCode = "AU";
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;
			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();
			return staff;
		}

		public GlbStaff CreateStaff(string loginName, string password, string code, bool active, bool resource, bool isOperational = true, bool isController = false, bool canLogin = true, bool twoFactorEnabled = false)
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			return CreateStaffCore(staff, loginName, password, code, active, resource, isOperational, isController, canLogin, twoFactorEnabled);
		}

		public void CreateANewBranchWithInvalidPK(Guid companyPK)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var sql = $@"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) Values ('{ZGuid.Invalid}', 'IVD', '{companyPK}')";
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void CreateANewDepartmentWithInvalidPK()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var sql = $@"INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES ('{ZGuid.Invalid}', 'IVD')";
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		GlbBranch CreateANewBranch(string companyCode, string branchCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();

				branch.FillWithValidTestData();
				branch.Company.GC_Code = companyCode;
				branch.GB_Code = branchCode;
				Factory.Save();
				return branch;
			}
		}

		GlbDepartment CreateANewDepartment(string departmentCode)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var department = Factory.NewWithValidTestData<GlbDepartment>();

				department.FillWithValidTestData();
				department.GE_Code = departmentCode;
				Factory.Save();
				return department;
			}
		}
	}
}
