using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Tests.CodeMappingControllerTests;
using CargoWise.eHub.Portal.Tests.Model;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ClientAuthorisationControllerTest  : BaseControllerTest<ClientAuthorisationController>
	{
		[TestMethod]
		public void TestClientAuthorisationsGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CA_CC_Sender";
			request.Container["sord"] = "asc";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			var result = controller.GetClientAuthorisation();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> {
				new{ CA_CC_Sender = "CLIENT001", CA_CC_Recipient = "CLIENT002", CA_CreatedUTC = new DateTime(2020, 10 ,08),id = "CLIENT001|CLIENT002" }
			}, result, "eHubClientAuthorisations");
		}

		[TestMethod]
		public void TestRegistrationsEdit_Add()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);
			request.Clear();
			request.Container["CA_CC_Sender"] = "CLIENT011";
			request.Container["CA_CC_Recipient"] = "CLIENT022";
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			JsonResult responseAdd = ClientAuthorisationEditTest(logger) as JsonResult;

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var result = context.eHubClientAuthorisations.Select(ca => new Tuple<Guid, Guid>(ca.CA_CC_Sender, ca.CA_CC_Recipient)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid>(new Guid("{00000000-cccc-1111-1111-000000000000}"), new Guid("{00000000-cccc-1111-2222-000000000000}")),
					new Tuple<Guid, Guid>(new Guid("{00000000-ffff-1111-1111-000000000000}"), new Guid("{00000000-ffff-1111-2222-000000000000}"))
				}, result);
			Assert.IsTrue(logger.Log.Contains("Info - [add] ClientAuthorisation:"));
			Assert.IsTrue(logger.Log.Contains("CA_CC_Sender=CLIENT011, CA_CC_Recipient=CLIENT022"));
		}

		[TestMethod]
		public void TestRegistrationsEdit_Delete()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);
			request.Clear();
			request.Container["id"] = "CLIENT001|CLIENT002";
			request.Container["oper"] = "del";

			JsonResult responseDel = ClientAuthorisationEditTest(logger) as JsonResult;
			var resultState = (bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null);
			Assert.IsTrue(resultState);
			var result = context.eHubClientAuthorisations.Select(ca => new Tuple<Guid, Guid>(ca.CA_CC_Sender, ca.CA_CC_Recipient)).ToArray();
			Assert.IsTrue(logger.Log.Contains("Info - [del] ClientAuthorisation:"));
			Assert.IsTrue(logger.Log.Contains("CA_CC_Sender=CLIENT001, CA_CC_Recipient=CLIENT002"));
			Assert.AreEqual(0 , result.Length);
		}

		protected JsonResult ClientAuthorisationEditTest(ILog logger)
		{
			controller.logger = logger;
			return controller.ClientAuthorisationEdit();
		}

		void GenerateDefaultValues(Fakes.TestContext context)
		{
			var ccsender1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var ccsender2 = new eHubClient { CC_PK = new Guid("{00000000-ffff-1111-1111-000000000000}"), CC_ID = "CLIENT011" };
			var ccrecepient1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var ccrecepient2 = new eHubClient { CC_PK = new Guid("{00000000-ffff-1111-2222-000000000000}"), CC_ID = "CLIENT022" };

			var ac1 = new eHubClientAuthorisation
			{
				eHubClientSender = ccsender1,
				eHubClientRecipient = ccrecepient1,
				CA_CreatedUTC = new DateTime(2020, 10, 08)
			};

			context.eHubClients.AddObject(ccsender1);
			context.eHubClients.AddObject(ccsender2);
			context.eHubClients.AddObject(ccrecepient1);
			context.eHubClients.AddObject(ccrecepient2);
			context.eHubClientAuthorisations.AddObject(ac1);
		}
	}
}
