using System;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class GlowUserContextSwitcherTest : TestCaseWithFactory
	{
		public void TestSetUserContextBasedOnHttp_WithNullHttpActionContext_ShouldNotChangeCurrentUser()
		{
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(null))
			{
				AssertEquals(initialUser, Env.CurrentUser);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSetUserContextBasedOnHttp_WithGlowAuthenticationTicketIdentity_ShouldChangeCurrentUser_WhenNoUserContext()
		{
			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			newUser.GS_LoginName = "User";
			newUser.GS_GB_HomeBranch = branch.PK;
			newUser.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			Env.SetUserContext(null);

			AssertNull(Env.CurrentUser);

			var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateStaffIdentity(newUser, branch.PK.ToGuid(), department.PK.ToGuid());

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					},
				};

				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
				{
					AssertEquals(newUser.PK, Env.CurrentUser.PK);
					AssertEquals(newUser.GS_GB_HomeBranch, Env.CurrentBranchPK);
					AssertEquals(newUser.GS_GE_HomeDepartment, Env.CurrentDepartmentPK);
				}
			}

			AssertNull("Should restore the initial context", Env.CurrentUser);
		}

		public void TestSetUserContextBasedOnHttp_WithGlowAuthenticationTicketIdentity_ShouldChangeCurrentUser_WhenUserContextIsNotNull()
		{
			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			newUser.GS_LoginName = "User";
			newUser.GS_GB_HomeBranch = branch.PK;
			newUser.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			AssertEquals(initialUser, Env.CurrentUser);
			AssertEquals(initialBranch, Env.CurrentBranch);
			AssertEquals(initialDepartment, Env.CurrentDepartment);

			var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateStaffIdentity(newUser, branch.PK.ToGuid(), department.PK.ToGuid());

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					}
				};

				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
				{
					AssertEquals(newUser.PK, Env.CurrentUser.PK);
					AssertEquals(newUser.GS_GB_HomeBranch, Env.CurrentBranchPK);
					AssertEquals(newUser.GS_GE_HomeDepartment, Env.CurrentDepartmentPK);
				}
			}

			CombineAssertions("Should restore the initial context", () =>
			{
				AssertEquals(initialUser, Env.CurrentUser);
				AssertEquals(initialBranch, Env.CurrentBranch);
				AssertEquals(initialDepartment, Env.CurrentDepartment);
			});
		}

		public void TestSetUserContextBasedOnHttp_WithGlowAuthenticationTicketIdentity_ShouldBeAbleToChangeCurrentUserToCWWeb()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			AssertEquals(initialUser, Env.CurrentUser);
			AssertEquals(initialBranch, Env.CurrentBranch);
			AssertEquals(initialDepartment, Env.CurrentDepartment);

			var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateContactIdentity(contact, branch.PK.ToGuid(), department.PK.ToGuid());

			var newUserContext = new Environment.UserContext(User.WebUserName, glowAuthenticationTicketIdentity.BranchKey, glowAuthenticationTicketIdentity.DepartmentKey);

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					}
				};

				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
				{
					AssertEquals(newUserContext.User.PK, Env.CurrentUser.PK);
					AssertEquals(newUserContext.Branch.PK, Env.CurrentBranchPK);
					AssertEquals(newUserContext.Department.PK, Env.CurrentDepartmentPK);
				}
			}

			CombineAssertions("Should restore the initial context", () =>
			{
				AssertEquals(initialUser, Env.CurrentUser);
				AssertEquals(initialBranch, Env.CurrentBranch);
				AssertEquals(initialDepartment, Env.CurrentDepartment);
			});
		}

		public void TestSetUserContextBasedOnHttp_WithGlowAuthenticationTicketIdentity_ShouldNotErrorReport_WhenSwitchingContextFromCWWeb()
		{
			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			newUser.GS_LoginName = "User";
			newUser.GS_GB_HomeBranch = branch.PK;
			newUser.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new Environment.UserContext(User.WebUserName, initialBranch.PK, initialDepartment.PK)))
			{
				AssertEquals("CWWeb", Env.CurrentUser.LoginName);
				AssertEquals(initialBranch.PK, Env.CurrentBranch.PK);
				AssertEquals(initialDepartment.PK, Env.CurrentDepartment.PK);

				var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateStaffIdentity(newUser, branch.PK.ToGuid(), department.PK.ToGuid());

				using (var request = new HttpRequestMessage())
				{
					var httpActionContext = new HttpActionContext()
					{
						ControllerContext = new HttpControllerContext()
						{
							Controller = new DummyController(glowAuthenticationTicketIdentity),
							Request = request,
						}
					};

					using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
					{
						AssertEquals(newUser.PK, Env.CurrentUser.PK);
						AssertEquals(newUser.GS_GB_HomeBranch, Env.CurrentBranchPK);
						AssertEquals(newUser.GS_GE_HomeDepartment, Env.CurrentDepartmentPK);
					}
				}

				CombineAssertions("Should restore the initial context", () =>
				{
					AssertEquals("CWWeb", Env.CurrentUser.LoginName);
					AssertEquals(initialBranch.PK, Env.CurrentBranch.PK);
					AssertEquals(initialDepartment.PK, Env.CurrentDepartment.PK);
				});
			}

			AssertNullOrEmpty("Should not complain about switching from CWWeb to another context", ErrorReporter.LastMessageReported);
		}

		public void TestSetUserContextBasedOnHttp_WhenTicketProviderTypeIsStaff_ShouldUseStaffPkForUserContext()
		{
			var newUser = Factory.NewWithValidTestData<GlbStaff>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			newUser.GS_LoginName = "OldLoginName";
			newUser.GS_GB_HomeBranch = branch.PK;
			newUser.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			var glowAuthenticationTicketIdentity = new GlowAuthenticationTicketIdentity(new AuthenticationTicket()
			{
				ProviderType = GlbStaffSchema.Constants.Prefix,
				ProviderKey = newUser.PK.ToGuid(),
				Username = "NewLoginName",
				InteropContextBranchKey = branch.PK.ToGuid(),
				InteropContextDepartmentKey = department.PK.ToGuid(),
			});

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					},
				};

				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
				{
					AssertEquals(newUser.PK, Env.CurrentUser.PK);
					AssertEquals(newUser.GS_GB_HomeBranch, Env.CurrentBranchPK);
					AssertEquals(newUser.GS_GE_HomeDepartment, Env.CurrentDepartmentPK);
				}
			}

			CombineAssertions("Should restore the initial context", () =>
			{
				AssertEquals(initialUser, Env.CurrentUser);
				AssertEquals(initialBranch, Env.CurrentBranch);
				AssertEquals(initialDepartment, Env.CurrentDepartment);
			});
		}

		public void TestSetUserContextBasedOnHttp_WhenTicketProviderTypeIsPerson_ShouldThrowForbiddenResponseException()
		{
			var glowAuthenticationTicketIdentity = new GlowAuthenticationTicketIdentity(new AuthenticationTicket()
			{
				ProviderType = GlbPersonSchema.Constants.Prefix,
				ProviderKey = Guid.NewGuid(),
				Username = "fake@email.com",
				InteropContextBranchKey = Guid.Empty,
				InteropContextDepartmentKey = Guid.Empty,
			});

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					},
				};

				var responseException = AssertExceptionThrown<HttpResponseException>(() => {
					using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
					{
						Fail("Expecting exception to be thrown");
					}
				});

				AssertEquals(HttpStatusCode.Forbidden, responseException.Response.StatusCode);
			}

			CombineAssertions("Should keep the initial context", () =>
			{
				AssertEquals(initialUser, Env.CurrentUser);
				AssertEquals(initialBranch, Env.CurrentBranch);
				AssertEquals(initialDepartment, Env.CurrentDepartment);
			});
		}

		public void TestSetUserContextBasedOnHttp_WhenTicketProviderTypeIsDiagnostic_ShouldThrowForbiddenResponseException()
		{
			var glowAuthenticationTicketIdentity = new GlowAuthenticationTicketIdentity(new AuthenticationTicket()
			{
				ProviderType = "DIA",
				ProviderKey = Guid.NewGuid(),
				Username = string.Empty,
				InteropContextBranchKey = Guid.Empty,
				InteropContextDepartmentKey = Guid.Empty,
			});

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					},
				};

				var responseException = AssertExceptionThrown<HttpResponseException>(() => {
					using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
					{
						Fail("Expecting exception to be thrown");
					}
				});

				AssertEquals(HttpStatusCode.Forbidden, responseException.Response.StatusCode);
			}

			CombineAssertions("Should keep the initial context", () =>
			{
				AssertEquals(initialUser, Env.CurrentUser);
				AssertEquals(initialBranch, Env.CurrentBranch);
				AssertEquals(initialDepartment, Env.CurrentDepartment);
			});
		}

		public void TestSetUserContextBasedOnHttp_WithDisposableActionForDbConnection_WhenSetTheEnviroment()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var lastMessageReported = string.Empty;

			var glowAuthenticationTicketIdentity = GlowTicketTestHelper.CreateContactIdentity(contact, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Controller = new DummyController(glowAuthenticationTicketIdentity),
						Request = request,
					}
				};

				ExceptionDispatchInfo edi = null;

				var thread = new Thread(() =>
				{
					try
					{
						ErrorReporter.Clear();

						using (Db.DisposableActionForDbConnection())
						using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(httpActionContext.ControllerContext))
						{
						}

						lastMessageReported = ErrorReporter.LastMessageReported;
						ErrorReporter.Clear();
					}
					catch (Exception ex)
					{
						edi = ExceptionDispatchInfo.Capture(ex);
					}
				});

				thread.Start();
				thread.Join();
				edi?.Throw();
			}

			AssertEquals(string.Empty, lastMessageReported);
		}

		#region Implementation

		public class DummyController : ApiController
		{
			public DummyController(IIdentity identity)
			{
				User = new GenericPrincipal(identity, null);
			}
		}

		IUser initialUser;
		IBranch initialBranch;
		IDepartment initialDepartment;

		protected override void SetUp()
		{
			base.SetUp();
			initialUser = Env.CurrentUser;
			initialBranch = Env.CurrentBranch;
			initialDepartment = Env.CurrentDepartment;
		}

		#endregion
	}
}
