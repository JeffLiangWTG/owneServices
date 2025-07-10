using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WiseTech.Business.Core;

namespace Enterprise.Services.ServiceHost
{
	public abstract class BasePaveController<S> : ApiController
	{
		protected S Service { get; private set; }

		protected BasePaveController(Func<S> getService)
		{
			serializerSettings.Converters.Add(new StringEnumConverter());
			connectionDisposable = Db.DisposableActionForDbConnection();
			Service = getService();
		}

		readonly IDisposable connectionDisposable;

		readonly JsonSerializerSettings serializerSettings = new()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Only the return is nested")]
		protected JsonResult<PaveResponse<object>> ToPaveResponseJson()
		{
			return Json(new PaveResponse<object>(), serializerSettings);
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Only the return is nested")]
		protected JsonResult<PaveResponse<T>> ToPaveResponseJson<T>(T content, PaveError error = null)
			where T : class
		{
			return Json(new PaveResponse<T>(content, error), serializerSettings);
		}

		protected override void Dispose(bool disposing)
		{
			connectionDisposable?.Dispose();
			base.Dispose(disposing);
		}

		#region Test
#if DEBUG
		public void SetServiceForTest(S service)
		{
			Service = service;
		}
#endif
		#endregion

	}

	[PaveFormatter]
	[BusinessResponseExceptionFilter]
	public abstract class BasePaveController : ApiController
	{
		internal T RunWithUserContext<T>(Func<T> action)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return action();
			}
		}

		internal IHttpActionResult UnprocessableEntity(BusinessResponse businessResponse)
		{
			return Content((HttpStatusCode)422, businessResponse);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Localized strings")]
		public static class BusinessMessages
		{
			static string CanonicalPrefix => $"{WiseTech.Business.Taxonomy.TaxonomyCommon.WiseTechCanonicalPrefix}.web";

			public static BusinessMessage InternalServerError =>
				BusinessMessage.BuildError(					
					englishText: "An internal server error occured.",
					localizationKey: Guid.Parse("2C723178-8B27-46CC-91B4-2C08824DCEC0"),
					canonicalPrefix: CanonicalPrefix + "InternalServerError",
					canonicalSuffix: "InternalServerError"
				);
		}
	}

	public sealed class BusinessResponseExceptionFilter : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			if (actionExecutedContext.Exception is ZSaveConcurrencyException or ZDataConcurrencyException)
			{
				actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.Conflict, BusinessResponse.Build(BusinessMessages.ConcurrencyError));
				return;
			}

			actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, BusinessResponse.Build(BasePaveController.BusinessMessages.InternalServerError));
		}
	}
}

[AttributeUsage(AttributeTargets.Class)]
sealed class PaveFormatterAttribute : Attribute, IControllerConfiguration
{
	public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
	{
		var formatter = controllerSettings.Formatters.JsonFormatter;

		controllerSettings.Formatters.Remove(formatter);

		formatter = new JsonMediaTypeFormatter
		{
			SerializerSettings =
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					Converters = [new StringEnumConverter()]
				}
		};

		controllerSettings.Formatters.Insert(0, formatter);
	}
}
