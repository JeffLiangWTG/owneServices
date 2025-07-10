using System;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers
{
	[GlowAnonymousAccessAuthentication]
	[RoutePrefix("api/contactpasswordemail")]
	public class ContactPasswordEmailController : ApiController
	{
		public ContactPasswordEmailController()
		{
			WebAppEnvironment.Setup();
		}

		[Route("sendInstructionEmail")]
		[HttpPost]
		public IHttpActionResult SendInstructionEmail([FromBody] SendInstructionEmailRequest requestParams)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (string.IsNullOrEmpty(requestParams.LandingPageUri))
				{
					return BadRequest((NoResString)"LandingPageUri cannot be empty.");
				}

				var factory = new BusinessObjectFactory();
				var contact = factory.Load<OrgContact>(requestParams.UserKey);
				if (contact == null)
				{
					return BadRequest((NoResString)"Contact doesn't exist.");
				}

				PasswordInstructionEmailSender.SendPasswordInstructionEmail(
					contact,
					contact.GetPasswordInstructionType(),
					new PasswordResetInfo { NavigateUrl = new Uri(requestParams.LandingPageUri) },
					PasswordInstructionUrlType.Glow);
			}

			return Ok();
		}

		[Route("sendConfirmationEmail")]
		[HttpPost]
		public IHttpActionResult SendConfirmationEmail([FromBody] SendConfirmationEmailRequest requestParams)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var contact = factory.Load<OrgContact>(requestParams.UserKey);
				if (contact == null)
				{
					return BadRequest((NoResString)"Contact doesn't exist.");
				}

				var sender = new WebUserConfirmationEmailSender(contact);
				sender.SendConfirmationEmail(requestParams.InstructionType);
			}

			return Ok();
		}
	}
}
