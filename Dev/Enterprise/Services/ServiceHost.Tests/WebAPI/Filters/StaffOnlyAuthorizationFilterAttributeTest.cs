using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class StaffOnlyAuthorizationFilterAttributeTest : TestCaseWithFactory
	{
		public void TestStaffOnlyAuthority()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var identity = GlowTicketTestHelper.CreateContactIdentity(contact, branch.PK.ToGuid(), department.PK.ToGuid());

			using (Env.SetTemporaryUserContext(new Environment.UserContext(User.WebUserName, Guid.Empty, Guid.Empty)))
			using (var request = new HttpRequestMessage())
			{
				var httpActionContext = new HttpActionContext()
				{
					ControllerContext = new HttpControllerContext()
					{
						Request = request,
						RequestContext = new HttpRequestContext() { Principal = new GenericPrincipal(identity, null) }
					}
				};

				var authentication = new StaffOnlyAuthorizationFilterAttribute();
				var task = ((IAuthorizationFilter)authentication).ExecuteAuthorizationFilterAsync(httpActionContext, CancellationToken.None, () => Task.FromResult(httpActionContext.Response));
				var response = task.Result;

				AssertEquals("It should return 403 Forbidden for web users.", HttpStatusCode.Forbidden, response.StatusCode);
			}
		}
	}
}
