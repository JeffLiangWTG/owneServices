using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers
{
	public class ContactPasswordEmailControllerTest : TestCaseWithFactory
	{
		public void TestSendInstructionEmail_NoLandingPage()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			var requestParams = new SendInstructionEmailRequest { UserKey = contact.PK.ToGuid() };
			var result = controller.SendInstructionEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.BadRequest, "LandingPageUri cannot be empty.");
		}

		public void TestSendInstructionEmail_ContactNotExist()
		{
			var requestParams = new SendInstructionEmailRequest { UserKey = Guid.NewGuid(), LandingPageUri = "https://www.google.com" };
			var result = controller.SendInstructionEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.BadRequest, "Contact doesn't exist.");
		}

		public void TestSendInstructionEmail()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var requestParams = new SendInstructionEmailRequest { UserKey = contact.PK.ToGuid(), LandingPageUri = "https://www.google.com/Portals#resetPassword" };
			var result = controller.SendInstructionEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.OK);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals($"{Env.CurrentCompany.Name} Password Set", sentEmail.Subject);
			AssertContains($"https://www.google.com/Portals#resetPassword", sentEmail.Body);
		}

		public void TestSendInstructionEmail_ResetWhenThereIsPasswordChangeLog()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com/Portals");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = contact.PK;
				log.SL_Table = OrgContactSchema.Constants.TableName;
				log.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
			}

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var requestParams = new SendInstructionEmailRequest { UserKey = contact.PK.ToGuid(), LandingPageUri = "https://www.google.com/Portals#resetPassword" };
			var result = controller.SendInstructionEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.OK);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals($"{Env.CurrentCompany.Name} Password Reset", sentEmail.Subject);
			AssertContains($"https://www.google.com/Portals#resetPassword", sentEmail.Body);
		}

		public void TestSendConfirmationEmail_Set()
		{
			TestSendConfirmationEmailCore(PasswordInstructionType.Set);
		}

		public void TestSendConfirmationEmail_Reset()
		{
			TestSendConfirmationEmailCore(PasswordInstructionType.Reset);
		}

		void TestSendConfirmationEmailCore(PasswordInstructionType instructionType)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.EmailContactItems[0].OI_Address = "user@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.OC_IsActive = true;
			contact.OC_PasswordHash = ZBlob.FromUTF8("ABCDEFG");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var requestParams = new SendConfirmationEmailRequest { UserKey = contact.PK.ToGuid(), InstructionType = instructionType };
			var result = controller.SendConfirmationEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.OK);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals($"{Env.CurrentCompany.Name} Password {instructionType} Complete", sentEmail.Subject);
		}

		public void TestSendConfirmationEmail_ContactNotExist()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var requestParams = new SendConfirmationEmailRequest { UserKey = Guid.NewGuid(), InstructionType = PasswordInstructionType.Set };
			var result = controller.SendConfirmationEmail(requestParams);

			result.AssertResultContains(HttpStatusCode.BadRequest, "Contact doesn't exist.");
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new ContactPasswordEmailController();
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
		}

		protected override void TearDown()
		{
			base.TearDown();

			controller?.Dispose();
		}

		ContactPasswordEmailController controller;
	}
}
