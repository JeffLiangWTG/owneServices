using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class USISFMessageControllerTest : TestCase
	{
		public void TestSendMessageSupportsHttpPost()
		{
			var sendManifestInfo = typeof(USISFMessageController).GetMethods().Single(x => x.Name == "SendMessage");
			AssertEquals(1, sendManifestInfo.GetCustomAttributes(typeof(HttpPostAttribute), false).Length);
		}

		public void TestSendMessage()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);
			webMessageSenderMock.Setup(x => x.SendUpsertMessage(headerPK)).Returns<string>(null);

			var response = controller.SendMessage(headerPK, false);
			var result = GetResult(response);

			AssertEquals(1, result.NoOfMessagesCreated);
			AssertNull(result.ErrorMessage);
		}

		public void TestSendMessage_Failed()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);
			webMessageSenderMock.Setup(x => x.SendUpsertMessage(headerPK)).Returns("Send Message Failed");

			var response = controller.SendMessage(headerPK, false);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("Send Message Failed", result.ErrorMessage);
		}

		public void TestSendMessage_NoStaffSecurity()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = false;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);

			var response = controller.SendMessage(headerPK, false);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("You do not have the relevant security rights.", result.ErrorMessage);
		}

		public void TestSendMessage_NoContactSecurity()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns("Contact Security Check Failed");

			var response = controller.SendMessage(headerPK, false);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("Contact Security Check Failed", result.ErrorMessage);
		}

		public void TestSendMessage_Delete()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);
			webMessageSenderMock.Setup(x => x.SendDeleteMessage(headerPK)).Returns<string>(null);

			var response = controller.SendMessage(headerPK, true);
			var result = GetResult(response);

			AssertEquals(1, result.NoOfMessagesCreated);
			AssertNull(result.ErrorMessage);
		}

		public void TestSendMessage_Delete_Failed()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);
			webMessageSenderMock.Setup(x => x.SendDeleteMessage(headerPK)).Returns("Send Message Failed");

			var response = controller.SendMessage(headerPK, true);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("Send Message Failed", result.ErrorMessage);
		}

		public void TestSendMessage_Delete_NoStaffSecurity()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = false;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns<string>(null);

			var response = controller.SendMessage(headerPK, true);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("You do not have the relevant security rights.", result.ErrorMessage);
		}

		public void TestSendMessage_Delete_NoContactSecurity()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			securityServiceMock.Setup(x => x.AreRightsGranted(identity, WebSecurityRightsList.WebISFSend)).Returns("Contact Security Check Failed");

			var response = controller.SendMessage(headerPK, true);
			var result = GetResult(response);

			AssertEquals(0, result.NoOfMessagesCreated);
			AssertEquals("Contact Security Check Failed", result.ErrorMessage);
		}

		protected static CreatingMessagesResult GetResult(HttpResponseMessage response)
		{
			var stringResult = response.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

			return JsonConvert.DeserializeObject<CreatingMessagesResult>(stringResult);
		}

		protected override void SetUp()
		{
			headerPK = Guid.NewGuid();
			securityServiceMock = new Mock<IGlowContactSecurityService>();
			controller = new USISFMessageController(securityServiceMock.Object);

			identity = new Mock<IGlowAuthenticationTicketIdentity>().Object;
			controller.User = new GenericPrincipal(identity, null);

			webMessageSenderMock = new Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender>(MockBehavior.Strict);
			ObjectFactory.Substitute("ISF.IUSISFWebMessageSender", webMessageSenderMock.Object);

			base.SetUp();
		}

		protected override void TearDown()
		{
			Env.Security.ImporterSecurityFilingMessaging.ClearOverriddenSecurityValue();
		}

		Guid headerPK;
		IGlowAuthenticationTicketIdentity identity;
		Mock<IGlowContactSecurityService> securityServiceMock;
		Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender> webMessageSenderMock;
		USISFMessageController controller;
	}
}