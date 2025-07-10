using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class EmailValidationControllerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			controller = new EmailValidationController();
			controller.Request = new HttpRequestMessage();
			controller.Request.SetConfiguration(new HttpConfiguration());
			controller.ControllerContext.Controller = controller;
		}

		protected override void TearDown()
		{
			base.TearDown();
			controller?.Dispose();
		}

		[TestDate(2025, 1, 1)]
		public void TestEmailVerification()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			var testToken = "abcdefghijklmnop";
			email.GSE_VerifyToken = testToken;
			email.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var response = controller.EmailVerification(staff.PK.ToGuid(), testToken).ExecuteAsync(CancellationToken.None).Result;

			var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var decodedResponse = JsonConvert.DeserializeObject<EmailVerificationResponse>(responseString);
			Assert("Response indicates success", decodedResponse.Success);
			Assert("Email is verified", email.GSE_Verified);
			AssertEquals("Token is cleared after verification", ZString.Empty, email.GSE_VerifyToken);
		}

		[TestDate(2025, 1, 1)]
		public void TestEmailVerification_WhenTokenIsInvalid()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			var testToken = "abcdefghijklmnop";
			email.GSE_VerifyToken = testToken;
			email.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var response = controller.EmailVerification(staff.PK.ToGuid(), "abc").ExecuteAsync(CancellationToken.None).Result;

			var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var decodedResponse = JsonConvert.DeserializeObject<EmailVerificationResponse>(responseString);
			Assert("Response does not indicate success", !decodedResponse.Success);
			AssertEquals("Failure message is correct", "This validation does not exist, please contact the admin user to re-initiate the validation.", decodedResponse.Message);
			Assert("Should not resend email", !decodedResponse.ResendVerificationEmail);
			Assert("Email is not verified", !email.GSE_Verified);
		}

		[TestDate(2025, 1, 1)]
		public void TestEmailVerification_WhenLoggedInUserIsNotSameAsEmailUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZZZ";
			staff2.GS_LoginName = "DifferentUser";
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff2);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			var testToken = "abcdefghijklmnop";
			email.GSE_VerifyToken = testToken;
			email.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var response = controller.EmailVerification(staff.PK.ToGuid(), testToken).ExecuteAsync(CancellationToken.None).Result;

			var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var decodedResponse = JsonConvert.DeserializeObject<EmailVerificationResponse>(responseString);
			Assert("Response does not indicate success", !decodedResponse.Success);
			AssertEquals("Failure message is correct", "The login user is not the same as the authenticated email user.", decodedResponse.Message);
			Assert("Should not resend email", !decodedResponse.ResendVerificationEmail);
			Assert("Email is not verified", !email.GSE_Verified);
		}

		[TestDate(2025, 1, 1)]
		public void TestEmailVerification_WhenTokenIsExpired()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			var testToken = "abcdefghijklmnop";
			email.GSE_VerifyToken = testToken;
			email.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			Factory.Save();

			var response = controller.EmailVerification(staff.PK.ToGuid(), testToken).ExecuteAsync(CancellationToken.None).Result;

			var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var decodedResponse = JsonConvert.DeserializeObject<EmailVerificationResponse>(responseString);
			Assert("Response does not indicate success", !decodedResponse.Success);
			AssertEquals("Failure message is correct", "The validation has expired after 24 hours.", decodedResponse.Message);
			Assert("Should resend email", decodedResponse.ResendVerificationEmail);
			AssertEquals("Should send email address in response", "email@email.com", decodedResponse.EmailAddress);
			Assert("Email is not verified", !email.GSE_Verified);
		}

		[TestDate(2025, 1, 1)]
		public void TestEmailVerification_WhenNewEmailRecordAddedConcurrently()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			var testToken = "abcdefghijklmnop";
			email.GSE_VerifyToken = testToken;
			email.GSE_VerifyTokenCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var timesCalledRaceCondition = 0;
			var currentUserContext = Env.CurrentUserContext;
			var newEmailPk = ZGuid.Empty;

			controller.raceConditionActionForTest = ConcurrentAddForTest;

			var response = controller.EmailVerification(staff.PK.ToGuid(), testToken).ExecuteAsync(CancellationToken.None).Result;

			var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			var decodedResponse = JsonConvert.DeserializeObject<EmailVerificationResponse>(responseString);
			Assert("Response indicates success", decodedResponse.Success);
			Assert("Email is verified", email.GSE_Verified);
			var newEmail = Factory.Load<GlbStaffEmailAddress>(newEmailPk);
			Assert("Concurrently added email is verified", newEmail.GSE_Verified);

			void ConcurrentAddForTest()
			{
				if (timesCalledRaceCondition > 0)
				{
					return;
				}
				using var contextSwitch = Env.SetTemporaryUserContext(currentUserContext);

				var newFactory = new BusinessObjectFactory();

				var newStaff = newFactory.NewWithValidTestData<GlbStaff>();
				newStaff.GS_LoginName = "AAA";
				newStaff.GS_Code = "AAA";

				var newCompany = newFactory.NewWithValidTestData<GlbCompany>();
				newCompany.GC_Code = "AAA";

				var newEmail = newFactory.New<GlbStaffEmailAddress>();
				newEmail.GSE_GC_Company = newCompany.PK;
				newEmail.GSE_Type = "AAA";
				newEmail.GSE_GS = newStaff.PK;
				newEmail.GSE_EmailAddress = "email@email.com";
				newEmailPk = newEmail.PK;

				newFactory.Save();
				timesCalledRaceCondition++;
			}
		}

		public void TestResendValidationEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			Factory.Save();

			var mock = new Mock<IGlbStaffEmailValidationSender>();
			using var substitution = ObjectFactory.Substitute(mock.Object);

			var response = controller.ResendValidationEmail(staff.PK.ToGuid(), "email@email.com").ExecuteAsync(CancellationToken.None).Result;

			AssertEquals("Response is Ok", HttpStatusCode.OK, response.StatusCode);
			mock.Verify(x => x.GenerateValidationEmail(It.IsAny<GlbStaff>(), It.Is<string>(e => e == "email@email.com")), Times.Once(), "Should send validation email");
		}

		public void TestResendValidationEmail_WhenEmailAlreadyVerified()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_GS = staff.PK;
			email.GSE_EmailAddress = "email@email.com";
			email.GSE_Verified = true;
			Factory.Save();

			var mock = new Mock<IGlbStaffEmailValidationSender>();
			using var substitution = ObjectFactory.Substitute(mock.Object);

			var response = controller.ResendValidationEmail(staff.PK.ToGuid(), "email@email.com").ExecuteAsync(CancellationToken.None).Result;

			AssertEquals("Response is NotFound", HttpStatusCode.NotFound, response.StatusCode);
			mock.Verify(x => x.GenerateValidationEmail(It.IsAny<GlbStaff>(), It.IsAny<string>()), Times.Never(), "Should not send validation email");
		}

		EmailValidationController controller;
	}
}
