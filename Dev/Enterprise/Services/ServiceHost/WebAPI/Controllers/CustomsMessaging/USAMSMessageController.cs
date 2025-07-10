using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/USAMSMessaging")]
	public class USAMSMessageController : ApiController
	{
		[Route("SendManifest")]
		[HttpPost]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public HttpResponseMessage SendManifest([FromUri] Guid headerPK, [FromBody] IEnumerable<USAMSManifestBillAmendment> bills)
		{
			HttpResponseMessage result = null;
			try
			{
				using (Db.DisposableActionForDbConnection())
				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
				{
					var messageSender = ObjectFactory.Get<Integration.Customs.US.USAMS.IUSAMSMessageSender>();
					var sendingResult = messageSender.SendManifest(headerPK, bills, false);

					result = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(JsonConvert.SerializeObject(new CreatingMessagesResult() { NoOfMessagesCreated = sendingResult.Item1, ErrorMessage = sendingResult.Item2 }), Encoding.UTF8, "application/json")
					};

					return result;
				}
			}
			catch
			{
				if (result != null)
				{
					result.Dispose();
				}
				throw;
			}
		}
	}
}
