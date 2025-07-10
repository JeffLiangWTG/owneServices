using System;
using System.Linq;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/Staff")]
	public class EmailValidationController : ApiController
	{
		[HttpPost]
		[Route("EmailVerification/{staffPk}/{token}")]
		public IHttpActionResult EmailVerification(Guid staffPk, string token)
		{
			using var dbConnection = Db.DisposableActionForDbConnection();
			using var userContextSwitch = GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext);

			var factory = CreateFactory();
			var staff = factory.Load<GlbStaff>(staffPk);

			var emailQuery = new ZQuery(GlbStaffEmailAddressSchema.GSE_VerifyToken, token);
			var email = factory.Load<GlbStaffEmailAddress>(emailQuery).FirstOrDefault();

			if (email is null)
			{
				return Json(new EmailVerificationResponse(false, Res.GetString("CB60F9C6-572C-41BC-A38B-3CD6A6B7DBE3", "This validation does not exist, please contact the admin user to re-initiate the validation.")));
			}

			if (Env.CurrentUserPK != staffPk || Env.CurrentUserPK != email.GSE_GS)
			{
				return Json(new EmailVerificationResponse(false, Res.GetString("8D314389-5770-4721-8F50-19A83C4BEE5A", "The login user is not the same as the authenticated email user.")));
			}

			if (email.GSE_VerifyTokenCreateTimeUtc < ZDateTime.UtcNow.AddDays(-1))
			{
				return Json(new EmailVerificationResponse(false, Res.GetString("F60141FF-57B5-4C76-BBA0-DB51C494D047", "The validation has expired after 24 hours."), true, email.GSE_EmailAddress));
			}

			var unverifiedEmailQuery = new ZDBOnlyQuery(typeof(GlbStaffEmailAddress));
			unverifiedEmailQuery.AddToFilter(GlbStaffEmailAddressSchema.GSE_Verified, false);
			unverifiedEmailQuery.AddToFilter(GlbStaffEmailAddressSchema.GSE_EmailAddress, email.GSE_EmailAddress);

			for (var tries = 0; tries < 2; tries++)
			{
				var unverifiedEmails = factory.Load<GlbStaffEmailAddress>(unverifiedEmailQuery);
				foreach (var unverifiedEmail in unverifiedEmails)
				{
					unverifiedEmail.GSE_Verified = true;
					unverifiedEmail.GSE_VerifyToken = ZString.Empty;
				}
				HookForTest();
				factory.Save();
			}

			return Json(new EmailVerificationResponse(true));
		}

		[HttpPost]
		[Route("ResendValidationEmail/{staffPk}/{email}")]
		public IHttpActionResult ResendValidationEmail(Guid staffPk, string email)
		{
			var factory = CreateFactory();
			var staff = factory.Load<GlbStaff>(staffPk);
			var unverifiedEmail = staff.EmailAddresses.FirstOrDefault(x => x.GSE_EmailAddress == email && !x.GSE_Verified);
			if (unverifiedEmail is null)
			{
				return NotFound();
			}
			var emailSender = ObjectFactory.Get<IGlbStaffEmailValidationSender>();
			emailSender.GenerateValidationEmail(staff, email);
			return Ok();
		}

		static BusinessObjectFactory CreateFactory()
		{
			var factory = new BusinessObjectFactory();
			factory.NameForDebugging = "EmailValidationControllerFactory";
			return factory;
		}

		internal Action raceConditionActionForTest;

		void HookForTest()
		{
#if DEBUG
			raceConditionActionForTest?.Invoke();
#endif
		}
	}

	internal class EmailVerificationResponse(bool success, string message = "", bool resendVerificationEmail = false, string email = "")
	{
		public string Message { get; set; } = message;
		public bool Success { get; set; } = success;
		public bool ResendVerificationEmail { get; set; } = resendVerificationEmail;
		public string EmailAddress { get; set; } = email;
	}
}
