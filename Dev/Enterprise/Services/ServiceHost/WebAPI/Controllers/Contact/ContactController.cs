using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using WTG.Foundation.FrameworkExtensions.Collections;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/contact")]
	public class ContactController : ApiController
	{
		[Route("sendPasswordInstructionsEmail/{contactPk}")]
		[HttpPost]
		public HttpResponseMessage SendPasswordInstructionsEmail([FromUri] Guid contactPk, [FromUri] bool useCurrentUserInfo = false)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var contact = factory.Load<OrgContact>(contactPk);

				if (contact == null)
				{
					var problemDetails = new ProblemDetails()
					{
						Title = (NoResString)"Contact doesn't exist",
						Type = ProblemType.MissingUser,
						Status = (int)HttpStatusCode.NotFound
					};

					return new HttpResponseMessage(HttpStatusCode.NotFound)
					{
						Content = new StringContent(problemDetails.Serialize(), Encoding.UTF8, ProblemDetails.MediaType),
					};
				}

				var passwordResetInfo = new PasswordResetInfo()
				{
					ContactEmail = contact.Email,
					OrgCode = contact.OrgCode,
					EmailTemplateCompanyPk = GlbCompany.CurrentCompany.PK.ToString()
				};

				PasswordInstructionEmailSender.SendPasswordInstructionEmail(
						contact,
						contact.GetPasswordInstructionType(),
						passwordResetInfo,
						PasswordInstructionUrlType.Glow,
						useCurrentUserInfo);
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[Route("syncPersons")]
		[HttpPost]
		public HttpResponseMessage SyncPersons([FromBody] Guid[] contactPKs)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var errorList = new List<Guid>();

				foreach (var contactPK in contactPKs)
				{
					var contact = factory.Load<OrgContact>(contactPK);

					if (contact == null)
					{
						errorList.Add(contactPK);
						continue;
					}

					//It's too difficult to find which columns were updated outside of CW1, so we just want to force an update of all relevant person columns.
					contact.UpdatePerson(forceUpdate: true);
					//CreateFromContact calls CreateTasksAndMilestonesFromTemplateIfRequired which we're missing here, but that should trigger whenever we update the person.
				}

				factory.Save();

				if (errorList.Count > 0)
				{
					ErrorReporter.ReportOnce("Missing contact(s) for person sync", FormattableString.Invariant($"The following contact(s) are missing: {string.Join(",", errorList)}"));
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}
}
