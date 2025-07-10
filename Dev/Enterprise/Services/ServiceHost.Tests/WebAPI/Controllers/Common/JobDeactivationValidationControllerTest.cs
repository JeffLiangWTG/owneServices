using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Authentication.Glow.Ticketing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost.Tests
{
	class JobDeactivationValidationControllerTest : TestCaseWithFactory
	{
		public void TestGetCanCancelJob_Success()
		{
			var job1 = (BusinessObject)Factory.New<IDtbConsignment>();
			job1.FillWithValidTestData();

			var job2 = (BusinessObject)Factory.New<IDtbConsignment>();
			job2.FillWithValidTestData();
			var jobHeader = (Job)new JobHeader.Loader((IJobHeaderParent)job2).TryCreate();
			var charge = jobHeader.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_OSCostAmt = 1;
			Factory.Save();

			var controller = CreateController("DtbConsignment", job1.PK, job2.PK);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result);
				AssertMultilineASCIIEquals($$"""
				                             [
				                               {
				                                 "jobPK": "{{job1.PK}}",
				                                 "validationError": ""
				                               },
				                               {
				                                 "jobPK": "{{job2.PK}}",
				                                 "validationError": "Land Transport Consignment {{jobHeader.JH_JobNum}} cannot be deactivated.\r\nJob Invoicing Charge(s) have been saved against this Invoicing Job Header ({{jobHeader.JH_JobNum}}) in the company EDI."
				                               }
				                             ]
				                             """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenControllerIDNotSpecified_ShouldReturnFailure()
		{
			var job = (BusinessObject)Factory.New<IDtbConsignment>();
			job.FillWithValidTestData();
			Factory.Save();

			var uri = $"http://localhost/api/Common/JobDeactivationValidation/GetCanCancelJob/?jobPK={job.PK}";
			var controller = CreateController(uri);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result);
				AssertMultilineASCIIEquals("""
				                           {
				                             "Message": "Please provide a valid controller ID."
				                           }
				                           """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenJobPKNotSpecified_ShouldReturnFailure()
		{
			const string uri = "http://localhost/api/Common/JobDeactivationValidation/GetCanCancelJob/?controllerID=DtbConsignment";
			var controller = CreateController(uri);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result);
				AssertMultilineASCIIEquals("""
				                           {
				                             "Message": "At least one job PK must be specified with the jobPK parameter. This parameter can be used multiple times in order to check multiple jobs at once."
				                           }
				                           """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenInvalidControllerIDSpecified_ShouldReturnFailure()
		{
			var job = (BusinessObject)Factory.New<IDtbConsignment>();
			job.FillWithValidTestData();
			Factory.Save();

			var controller = CreateController("NotARealController", job.PK);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result);
				AssertMultilineASCIIEquals("""
				                           {
				                             "Message": "Could not create a controller with the ID NotARealController. Please provide a valid controller ID."
				                           }
				                           """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenUserDoesNotHaveEditPermissionOnRelevantModule_ShouldReturnFailure()
		{
			var job = (BusinessObject)Factory.New<IDtbConsignment>();
			job.FillWithValidTestData();

			var userWithoutPermission = CreateUser(false);
			var userWithPermission = CreateUser(true);
			Factory.Save();

			var controller = CreateControllerWithUser(userWithoutPermission, job);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.Forbidden, result);
				AssertMultilineASCIIEquals("\"The logged in user does not have edit permission for the DtbConsignment module.\"", formattedResponse);
			});

			controller = CreateControllerWithUser(userWithPermission, job);
			result = GetResult(controller, out formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result);
				AssertEquals($$"""
				               [
				                 {
				                   "jobPK": "{{job.PK}}",
				                   "validationError": ""
				                 }
				               ]
				               """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenJobDoesNotExist_ShouldReturnFailure()
		{
			var nonexistentJobPK = Guid.NewGuid();
			var job = (BusinessObject)Factory.New<IDtbConsignment>();
			job.FillWithValidTestData();
			Factory.Save();

			var controller = CreateController("DtbConsignment", job.PK, nonexistentJobPK);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result);
				AssertMultilineASCIIEquals($$"""
				                             {
				                               "Message": "No job with PK {{nonexistentJobPK}} could be found for controller DtbConsignment."
				                             }
				                             """, formattedResponse);
			});
		}

		public void TestGetCanCancelJob_WhenJobDoesNotMatchControllerID_ShouldReturnFailure()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			job.FillWithValidTestData();
			Factory.Save();

			var controller = CreateController("DtbConsignment", job.PK);
			var result = GetResult(controller, out var formattedResponse);

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result);
				AssertMultilineASCIIEquals($$"""
				                             {
				                               "Message": "No job with PK {{job.PK}} could be found for controller DtbConsignment."
				                             }
				                             """, formattedResponse);
			});
		}

		#region Implementation

		static JobDeactivationValidationController CreateController(string controllerIDName, params ZGuid[] jobPks)
		{
			var uri = $"http://localhost/api/Common/JobDeactivationValidation/GetCanCancelJob/?controllerID={controllerIDName}&jobPK={string.Join("&jobPK=", jobPks)}";
			return CreateController(uri);
		}

		static JobDeactivationValidationController CreateController(string uri)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			var controller = new JobDeactivationValidationController();
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), request)
			{
				Controller = controller
			};

			return controller;
		}

		static HttpStatusCode GetResult(JobDeactivationValidationController controller, out string formattedResponse)
		{
			var queryString = new QueryString(controller.Request.RequestUri.Query);
			var controllerID = queryString["?controllerID"];
			var jobPKs = GetJobPKs(controller).ToArray();

			var actionResult = controller.GetCanCancelJob(controllerID, jobPKs);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			formattedResponse = FormatJson(content);

			return response.StatusCode;
		}

		static string FormatJson(string unformattedJson)
		{
			var deserialized = JsonConvert.DeserializeObject(unformattedJson);
			return JsonConvert.SerializeObject(deserialized, Formatting.Indented);
		}

		GlbStaff CreateUser(bool hasConsignmentEditPermission)
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var security = Factory.New<GlbSecurity>();
			security.GU_GS = user.PK;
			security.GU_SecurityRight = Env.Security.DtbConsignmentEdit.Code;
			security.GU_SecurityItemIsAllowed = hasConsignmentEditPermission;

			return user;
		}

		static JobDeactivationValidationController CreateControllerWithUser(GlbStaff user, BusinessObject job)
		{
			var identity = new GlowAuthenticationTicketIdentity(new AuthenticationTicket
			{
				ProviderType = GlbStaffSchema.Constants.Prefix,
				ProviderKey = user.PK.ToGuid(),
				InteropContextBranchKey = Env.CurrentBranchPK,
				InteropContextDepartmentKey = Env.CurrentDepartmentPK,
			});

			var controllerWithUser = CreateController("DtbConsignment", job.PK);
			controllerWithUser.User = new GenericPrincipal(identity, []);

			return controllerWithUser;
		}

		static IEnumerable<Guid> GetJobPKs(ApiController controller)
		{
			var queryString = new QueryString(controller.Request.RequestUri.Query);
			var jobPKStrings = queryString.GetValues("jobPK") ?? queryString.GetValues("?jobPK");
			return jobPKStrings?.Length > 0
				? jobPKStrings.Select(Guid.Parse)
				: [];
		}

		#endregion
	}
}
