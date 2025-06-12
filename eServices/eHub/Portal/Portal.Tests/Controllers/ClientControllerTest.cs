using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Tests.Fakes;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MvcContrib.UI.Grid;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ClientControllerTest
	{
		ClientController controller;
		IeHubTransactionsContext context;

		public ClientControllerTest()
		{
			var httpContext = new Mock<HttpContextBase>();
			var routeData = new RouteData();
			controller = new ClientController();
			context = TestContextWithData.Create();
			controller.Context = context;
			controller.ControllerContext = new Mock<ControllerContext>(httpContext.Object, routeData, controller).Object;
		}

		[TestMethod]
		public void TestIndex()
		{
			var clients = context.eHubClients;

			ActionResult result = controller.Index(null, null, null, null, null, null, new GridSortOptions(), null);
			Assert.IsNotNull(result);
			ViewResult view = result as ViewResult;
			ClientListContainerViewModel clientList = (result as ViewResult).Model as ClientListContainerViewModel;
			Assert.IsNotNull(clientList);
			Assert.AreEqual(clients.Count(), clientList.PagedList.Count());
			foreach (var client in clients)
			{
				Assert.IsTrue(clientList.PagedList.Contains(client));
			}
		}

		[TestMethod]
		public void TestFilterByAS2Code()
		{
			AssertAS2CodeFilter("TestAS2Code1");
			AssertAS2CodeFilter("TestAS2Code");
		}

		[TestMethod]
		public void TestEditAS2Code()
		{
			var logger = new TestLogger();
			eHubClient client = context.eHubClients.First();
			ActionResult result = controller.Edit(client.CC_PK);
			Assert.IsNotNull(result);
			ViewResult view = assertView(result, "Edit");
			ClientView clientView = view.Model as ClientView;
			Assert.AreEqual(client, clientView.Client);

			FormCollection formValues = new FormCollection()
			{
				{ "Client.CC_ID", client.CC_ID },
				{ "Client.CC_FriendlyName", client.CC_FriendlyName },
				{ "Client.CC_Odyssey_OH", client.CC_Odyssey_OH.ToString() },
				{ "Client.CC_DistributionZone", client.CC_DistributionZone.ToString() },
				{ "Client.CC_EmailAddress", client.CC_EmailAddress },
				{ "Client.CC_Password", client.CC_Password },
				{ "Client.CC_IsAirServiceProvider", client.CC_IsAirServiceProvider.ToString() },
				{ "Client.CC_AirlineCode", client.CC_AirlineCode },
				{ "Client.CC_AS2_Code", "UPDATED" + client.CC_AS2_Code },
				{ "Client.CC_AirServiceProvider", client.CC_AirServiceProvider.ToString() },
			};

			controller.ValueProvider = formValues.ToValueProvider();
			EditTest(client.CC_PK, formValues, logger);
			Assert.IsTrue(client.CC_AS2_Code.Contains("UPDATED"));
			Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubClient: CC_PK=00000000-aaaa-1111-0000-000000000000, CC_ID=TEST0001, CC_FriendlyName=Test Client 1, CC_Odyssey_OH=00000000-0000-0000-0000-000000000000, CC_DistributionZone=, CC_EmailAddress=, CC_Password=, CC_IsAirServiceProvider=, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=UPDATEDTestAS2Code1, CC_SCAC_Code=, CC_OwnerCategory=, CC_SystemCategory=, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		[TestMethod]
		public void TestCreate()
		{
			var logger = new TestLogger();
			int initialClientCount = context.eHubClients.Count();
			eHubClient client = context.eHubClients.First();
			ActionResult result = controller.Create((string)null);
			assertView(result, "Edit");

			const string CC_ID = "NewCC_ID";
			const string CC_FN = "New Test Client";
			const string CC_AS2 = "TestAS2Code";

			FormCollection formValues = new FormCollection()
			{
				{ "Client.CC_ID", CC_ID},
				{ "Client.CC_FriendlyName", CC_FN },
				{ "Client.CC_Odyssey_OH", "" },
				{ "Client.CC_DistributionZone", client.CC_DistributionZone.ToString() },
				{ "Client.CC_EmailAddress", client.CC_EmailAddress },
				{ "Client.CC_Password", client.CC_Password },
				{ "Client.CC_IsAirServiceProvider", client.CC_IsAirServiceProvider.ToString() },
				{ "Client.CC_AirlineCode", client.CC_AirlineCode },
				{ "Client.CC_AS2_Code", CC_AS2 },
				{ "Client.CC_AirServiceProvider", client.CC_AirServiceProvider.ToString() },
			};

			controller.ValueProvider = formValues.ToValueProvider();
			CreateTest(formValues, logger);

			Assert.AreEqual(initialClientCount + 1, context.eHubClients.Count());
			var newClient = (from c in context.eHubClients
							 select c).Where(c => c.CC_ID == CC_ID).First();

			Assert.AreEqual(CC_ID, newClient.CC_ID);
			Assert.AreEqual(CC_FN, newClient.CC_FriendlyName);
			Assert.AreEqual(CC_AS2, newClient.CC_AS2_Code);
			Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubClient:"));
			Assert.IsTrue(logger.Log.Contains("CC_ID=NewCC_ID, CC_FriendlyName=New Test Client, CC_Odyssey_OH=00000000-0000-0000-0000-000000000000, CC_DistributionZone=, CC_EmailAddress=, CC_Password=, CC_IsAirServiceProvider=, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=TestAS2Code, CC_SCAC_Code=, CC_OwnerCategory=, CC_SystemCategory=, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Success()
		{
			var logger = new TestLogger();
			var OrgPK = Guid.NewGuid();
			var OrgCode = "ORGONE";
			var OrgFriendlyName = "Org One";

			context.ediProdAllOrgs.AddObject(new ediProdAllOrg() { OH_PK = OrgPK, OH_Code = OrgCode, OH_FullName = OrgFriendlyName });

			var clientForm = new ClientThirdPartyPartner()
			{
				Id = "MYNEWID",
				Password = "password",
				OrgCode = OrgCode,
				Email = "someone@wisetechglobal.com"
			};

			RunValidation(clientForm, true);

			ActionResult result = CreateThirdPartyPartnerTest(clientForm, logger);
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
			RedirectToRouteResult view = result as RedirectToRouteResult;
			Assert.IsTrue(view.RouteValues.ContainsValue("IntegrationAccountAuthorisations"));
			Assert.IsTrue(controller.ModelState.IsValid, "Unexpected validation error");

			Assert.AreEqual("eHubClient successfully created", controller.TempData["Success"]);

			var newClient = context.eHubClients.Where(x => x.CC_ID == clientForm.Id).FirstOrDefault();
			Assert.IsNotNull(newClient, string.Format("New eHubClient could not be found with ID '{0}'", clientForm.Id));

			Assert.IsNull(newClient.CC_AirlineCode);
			Assert.IsNull(newClient.CC_AirlinePrefix);
			Assert.IsNull(newClient.CC_AirServiceProvider);
			Assert.IsNull(newClient.CC_AS2_Code);
			Assert.IsNull(newClient.CC_DistributionZone);
			Assert.AreEqual(clientForm.Email, newClient.CC_EmailAddress);
			Assert.AreEqual(OrgFriendlyName, newClient.CC_FriendlyName);
			Assert.AreEqual(clientForm.Id, newClient.CC_ID);
			Assert.IsNull(newClient.CC_IsAirServiceProvider);
			Assert.AreEqual(OrgPK, newClient.CC_Odyssey_OH);
			Assert.AreEqual("Partner", newClient.CC_OwnerCategory);
			Assert.AreEqual("12-A4-BC-53-C2-80-41-04-9C-BA-BD-F0-DF-0C-38-19-20-DA-3A-47-7C-72-7A-AD-97-0D-F5-8E-2A-DF-C7-17-16-C6-53-FC-DE-EB-9D-95-C6-7E-F1-35-D5-4C-84-19-3A-B7-74-40-79-20-10-E2-12-1C-D4-DA-80-26-F6-C7", newClient.CC_Password);
			Assert.IsNotNull(newClient.CC_PK);
			Assert.IsFalse(newClient.CC_RequireStatusResponse);
			Assert.IsNull(newClient.CC_RR);
			Assert.IsNull(newClient.CC_SCAC_Code);
			Assert.AreEqual("Third Party", newClient.CC_SystemCategory);
			Assert.IsNull(newClient.CC_USCustomsRecipient);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubClient:"));
			Assert.IsTrue(logger.Log.Contains("CC_ID=MYNEWID, CC_FriendlyName=Org One"));
			Assert.IsTrue(logger.Log.Contains("CC_DistributionZone=, CC_EmailAddress=someone@wisetechglobal.com, CC_Password=12-A4-BC-53-C2-80-41-04-9C-BA-BD-F0-DF-0C-38-19-20-DA-3A-47-7C-72-7A-AD-97-0D-F5-8E-2A-DF-C7-17-16-C6-53-FC-DE-EB-9D-95-C6-7E-F1-35-D5-4C-84-19-3A-B7-74-40-79-20-10-E2-12-1C-D4-DA-80-26-F6-C7, CC_IsAirServiceProvider=, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=, CC_SCAC_Code=, CC_OwnerCategory=Partner, CC_SystemCategory=Third Party, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Validation_Duplicate()
		{
			var logger = new TestLogger();
			var OrgPK = Guid.NewGuid();
			var OrgCode = "ORGONE";
			var OrgFriendlyName = "Org One";

			context.ediProdAllOrgs.AddObject(new ediProdAllOrg() { OH_PK = OrgPK, OH_Code = OrgCode, OH_FullName = OrgFriendlyName });

			var clientForm = new ClientThirdPartyPartner()
			{
				Id = "MYNEWID",
				Password = "password",
				OrgCode = OrgCode,
				Email = "someone@wisetechglobal.com"
			};

			RunValidation(clientForm, true);

			ActionResult result1 = CreateThirdPartyPartnerTest(clientForm, logger);
			Assert.IsTrue(controller.ModelState.IsValid, "Unexpected validation error");
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubClient:"));
			Assert.IsTrue(logger.Log.Contains("CC_ID=MYNEWID, CC_FriendlyName=Org One"));
			Assert.IsTrue(logger.Log.Contains("CC_DistributionZone=, CC_EmailAddress=someone@wisetechglobal.com, CC_Password=12-A4-BC-53-C2-80-41-04-9C-BA-BD-F0-DF-0C-38-19-20-DA-3A-47-7C-72-7A-AD-97-0D-F5-8E-2A-DF-C7-17-16-C6-53-FC-DE-EB-9D-95-C6-7E-F1-35-D5-4C-84-19-3A-B7-74-40-79-20-10-E2-12-1C-D4-DA-80-26-F6-C7, CC_IsAirServiceProvider=, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=, CC_SCAC_Code=, CC_OwnerCategory=Partner, CC_SystemCategory=Third Party, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));

			var newClient = context.eHubClients.Where(x => x.CC_ID == clientForm.Id).FirstOrDefault();
			Assert.IsNotNull(newClient, string.Format("New eHubClient could not be found with ID '{0}'", clientForm.Id));

			ActionResult result2 = CreateThirdPartyPartnerTest(clientForm, logger);
			Assert.IsFalse(controller.ModelState.IsValid, "Validation error expected");
			AssertValidationError("Id", string.Format("eHubClient with code '{0}' already exists", clientForm.Id));
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubClient:"));
			Assert.IsTrue(logger.Log.Contains("CC_ID=MYNEWID, CC_FriendlyName=Org One"));
			Assert.IsTrue(logger.Log.Contains("CC_DistributionZone=, CC_EmailAddress=someone@wisetechglobal.com, CC_Password=12-A4-BC-53-C2-80-41-04-9C-BA-BD-F0-DF-0C-38-19-20-DA-3A-47-7C-72-7A-AD-97-0D-F5-8E-2A-DF-C7-17-16-C6-53-FC-DE-EB-9D-95-C6-7E-F1-35-D5-4C-84-19-3A-B7-74-40-79-20-10-E2-12-1C-D4-DA-80-26-F6-C7, CC_IsAirServiceProvider=, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=, CC_SCAC_Code=, CC_OwnerCategory=Partner, CC_SystemCategory=Third Party, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Validation_OrgNotFound()
		{
			var logger = new TestLogger();
			var clientForm = new ClientThirdPartyPartner()
			{
				Id = "MYNEWID",
				Password = "password",
				OrgCode = "FAKEORGCODE",
				Email = "someone@wisetechglobal.com"
			};

			RunValidation(clientForm, true);

			CreateThirdPartyPartnerTest(clientForm, logger);

			Assert.IsFalse(controller.ModelState.IsValid, "Validation error expected");
			Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

			AssertValidationError("OrgCode", string.Format("Organisation with code '{0}' could not be found in ediProd", clientForm.OrgCode));
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Exception()
		{
			// setup a local context which will throw an exception when SaveChanges() is called.
			var httpContext = new Mock<HttpContextBase>();
			var routeData = new RouteData();
			var localContext = TestContextWithData.Create(new Exception("this is bad mmmmk"));
			var localController = new ClientController();
			localController.Context = localContext;
			localController.ControllerContext = new Mock<ControllerContext>(httpContext.Object, routeData, localController).Object;

			var OrgPK = Guid.NewGuid();
			var OrgCode = "ORGONE";
			var OrgFriendlyName = "Org One";

			localContext.ediProdAllOrgs.AddObject(new ediProdAllOrg() { OH_PK = OrgPK, OH_Code = OrgCode, OH_FullName = OrgFriendlyName });

			var clientForm = new ClientThirdPartyPartner()
			{
				Id = "MYNEWID",
				Password = "password",
				OrgCode = OrgCode,
				Email = "someone@wisetechglobal.com"
			};

			RunValidation(localController, clientForm, true);

			localController.CreateThirdPartyPartner(clientForm);

			Assert.IsFalse(localController.ModelState.IsValid, "Validation error expected");

			Assert.AreEqual(1, localController.ModelState[string.Empty].Errors.Count);
			Assert.AreEqual("this is bad mmmmk", localController.ModelState[string.Empty].Errors[0].ErrorMessage);
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Validation_Required()
		{
			var clientForm = new ClientThirdPartyPartner();

			RunValidation(clientForm, false);

			AssertValidationRequired("Id");
			AssertValidationRequired("Password");
			AssertValidationRequired("OrgCode");
			AssertValidationRequired("Email");
		}

		[TestMethod]
		public void TestCreateThirdPartyPartner_Validation_MaxLength()
		{
			var clientForm = new ClientThirdPartyPartner()
			{
				Id = "1234567890123456789012345678901234567890",
				Password = "1234567890123456789012345678901234567890",
				OrgCode = "12345678901234567890",
				Email = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
			};

			RunValidation(clientForm, false);

			AssertValidationMaxLength("Id", 36);
			AssertValidationMaxLength("Password", 36);
			AssertValidationMaxLength("OrgCode", 12);
			AssertValidationMaxLength("Email", 128);
		}

		[TestMethod]
		public void TestDelete()
		{
			var logger = new TestLogger();
			var clientToDelete = context.eHubClients.First();
			ActionResult actionResult = DeleteTest(clientToDelete.CC_PK, logger);
			assertView(actionResult, "Delete");
			Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

			actionResult = DeleteTest(clientToDelete.CC_PK, (string)null, logger);
			assertView(actionResult, "Deleted");
			Assert.IsFalse(context.eHubClients.Contains(clientToDelete));
			Assert.IsTrue(logger.Log.Contains("Info - [del] eHubClient: CC_PK=00000000-aaaa-1111-0000-000000000000, CC_ID=TEST0001, CC_FriendlyName=Test Client 1, CC_Odyssey_OH=00000000-0000-0000-0000-000000000000, CC_DistributionZone=, CC_EmailAddress=, CC_Password=, CC_IsAirServiceProvider=False, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_USCustomsRecipient=, CC_AS2_Code=TestAS2Code1, CC_SCAC_Code=, CC_OwnerCategory=, CC_SystemCategory=, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		ViewResult assertView(ActionResult action, string viewName)
		{
			Assert.IsNotNull(action);
			Assert.IsInstanceOfType(action, typeof(ViewResult));
			ViewResult view = action as ViewResult;
			Assert.AreEqual(viewName, view.ViewName);
			return view;
		}

		void AssertAS2CodeFilter(string as2CodeFilter)
		{
			var expectedClients = context.eHubClients.Where(c => c.CC_AS2_Code.Contains(as2CodeFilter));
			ActionResult result = controller.Index(null, null, as2CodeFilter, null, null, null, new GridSortOptions(), null);
			Assert.IsNotNull(result);
			ClientListContainerViewModel clientList = (result as ViewResult).Model as ClientListContainerViewModel;
			Assert.IsNotNull(clientList);
			Assert.AreEqual(as2CodeFilter, clientList.FilterViewModel.AS2Code);
			Assert.AreEqual(expectedClients.Count(), clientList.PagedList.Count());
			foreach (var client in expectedClients)
			{
				Assert.IsTrue(clientList.PagedList.Contains(client));
			}
		}

		void RunValidation(object obj, bool expectedToPass)
		{
			RunValidation(controller, obj, expectedToPass);
		}

		void RunValidation(Controller localController, object obj, bool expectedToPass)
		{
			var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj, null, null);
			var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
			System.ComponentModel.DataAnnotations.Validator.TryValidateObject(obj, validationContext, validationResults, true);
			foreach (var validationResult in validationResults)
			{
				localController.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
			}

			if (expectedToPass)
			{
				Assert.IsTrue(localController.ModelState.IsValid, "ModelState was invalid and it was expected to be valid");
			}
			else
			{
				Assert.IsFalse(localController.ModelState.IsValid, "ModelState is valid and it was expected to be invalid");
			}
		}

		void AssertValidationError(string key, string message)
		{
			Assert.AreEqual(1, controller.ModelState[key].Errors.Count);
			Assert.AreEqual(message, controller.ModelState[key].Errors[0].ErrorMessage);
		}

		void AssertValidationRequired(string key)
		{
			Assert.AreEqual(1, controller.ModelState[key].Errors.Count);
			Assert.AreEqual(string.Format("The {0} field is required.", key), controller.ModelState[key].Errors[0].ErrorMessage);
		}

		void AssertValidationMaxLength(string key, int maxLength)
		{
			Assert.AreEqual(1, controller.ModelState[key].Errors.Count);
			Assert.AreEqual(string.Format("The field {0} must be a string with a maximum length of {1}.", key, maxLength), controller.ModelState[key].Errors[0].ErrorMessage);
		}

		protected ActionResult EditTest(Guid id, FormCollection formValues, ILog logger)
		{
			controller.logger = logger;
			return controller.Edit(id, formValues);
		}

		protected ActionResult CreateTest(FormCollection formValues, ILog logger)
		{
			controller.logger = logger;
			return controller.Create(formValues);
		}

		protected ActionResult CreateThirdPartyPartnerTest(ClientThirdPartyPartner clientForm, ILog logger)
		{
			controller.logger = logger;
			return controller.CreateThirdPartyPartner(clientForm);
		}

		protected ActionResult DeleteTest(Guid id, ILog logger)
		{
			controller.logger = logger;
			return controller.Delete(id);
		}
		protected ActionResult DeleteTest(Guid id, string confirmButton, ILog logger)
		{
			controller.logger = logger;
			return controller.Delete(id, confirmButton);
		}
	}
}
