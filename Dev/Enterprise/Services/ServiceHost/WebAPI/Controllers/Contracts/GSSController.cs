using System;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts
{
	[RoutePrefix("api/CCA/GSS")]
	[GlowTicketAuthentication]
	public class GSSController : ApiController
	{
		[Route("import")]
		[HttpPost]
		public IHttpActionResult Import([FromBody] ImportGSSPayload payload)
		{
			if (payload == null)
			{
				return BadRequest((NoResString)"Unable to read payload");
			}

			if (payload.GSSNaturalKey == null)
			{
				return BadRequest((NoResString)"GSSNaturalKey is required");
			}

			var handler = ObjectFactory.Get<IOnlineSailingSchedulesWebApi>();
			return RunWithUserContextAndExceptionHandling(nameof(Import), () =>
			{
				var result = handler.ImportGSSFromNaturalKey(payload);
				if (result.Code != ImportGSSResponseCode.Success)
				{
					return Content(HttpStatusCode.BadRequest, result, GetFormatter());
				}
				return Content(HttpStatusCode.OK, result, GetFormatter());
			});
		}

		[Route("importMany")]
		[HttpPost]
		public IHttpActionResult ImportMany([FromBody] ImportManyGSSPayload payload)
		{
			var handler = ObjectFactory.Get<IOnlineSailingSchedulesWebApi>();
			return RunWithUserContextAndExceptionHandling(nameof(ImportMany), () =>
			{
				var result = handler.ImportManyGSS(payload);
				if (!result.Success)
				{
					return Content(HttpStatusCode.BadRequest, result, GetFormatter());
				}
				return Content(HttpStatusCode.OK, result, GetFormatter());
			});
		}

		IHttpActionResult RunWithUserContextAndExceptionHandling(string handlerName, Func<IHttpActionResult> action)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
				{
					return action();
				}
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce($"GSSController_{handlerName}", e);
				var message = Res.GetString("8668c435-bc04-1a9f-474c-aa0fe14a5e39", "Unexpected error while importing GSS");
#if DEBUG
				message += System.Environment.NewLine + "Exception:" + System.Environment.NewLine + e.ToString();
#endif
				return Content(HttpStatusCode.InternalServerError, message);
			}
		}

		static MediaTypeFormatter GetFormatter()
		{
			var formatter = new JsonMediaTypeFormatter();
			formatter.SerializerSettings.Converters.Add(new StringEnumConverter());
			formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			return formatter;
		}
	}
}
