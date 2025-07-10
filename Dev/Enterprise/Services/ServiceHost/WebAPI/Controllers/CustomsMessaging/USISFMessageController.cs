using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/USISFMessaging")]
	[GlowTicketAuthentication]
	public class USISFMessageController : ApiController
	{
		public USISFMessageController(IGlowContactSecurityService securityService)
		{
			this.securityService = securityService;
		}
		readonly IGlowContactSecurityService securityService;

		public USISFMessageController() : this(new GlowContactSecurityService())
		{
		}

		[Route("SendMessage/{headerPK}/{delete}")]
		[HttpPost]
		public HttpResponseMessage SendMessage([FromUri] Guid headerPK, [FromUri] bool delete)
		{
			CreatingMessagesResult result;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (Env.Security.ImporterSecurityFilingMessaging.IsAllowed)
				{
					var securityErrorMessage = securityService.AreRightsGranted(User?.Identity as IGlowAuthenticationTicketIdentity, WebSecurityRightsList.WebISFSend);
					var success = string.IsNullOrEmpty(securityErrorMessage);
					if (success)
					{
						var messageSender = ObjectFactory.Get<Integration.Customs.US.ISF.IUSISFWebMessageSender>();
						var errorMessage = delete ? messageSender.SendDeleteMessage(headerPK) : messageSender.SendUpsertMessage(headerPK);
						var noOfMessagesCreated = string.IsNullOrEmpty(errorMessage) ? 1 : 0;

						result = new CreatingMessagesResult { NoOfMessagesCreated = noOfMessagesCreated, ErrorMessage = errorMessage };
					}
					else
					{
						result = new CreatingMessagesResult { NoOfMessagesCreated = 0, ErrorMessage = securityErrorMessage };
					}
				}
				else
				{
					result = new CreatingMessagesResult { NoOfMessagesCreated = 0, ErrorMessage = Res.GetString("117DEE4F-2610-4088-9459-E763B175AE01", "You do not have the relevant security rights.") };
				}

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json")
				};
			}
		}
	}
}
