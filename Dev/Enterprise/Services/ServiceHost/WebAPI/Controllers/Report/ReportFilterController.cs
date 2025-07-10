using System;
using System.Collections.Generic;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Newtonsoft.Json.Linq;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication]
	[ReportServiceErrorHandler]
	public class ReportFilterController : ReportDataBaseController
	{
		[Route("api/report/filter/getsecurityrights")]
		[HttpGet]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetSecurityRights()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetSecurityRights());
			}
		}

		[Route("api/report/filter/organisationRegistrationCodeTypes/{countryCode}")]
		[HttpGet]
		public IHttpActionResult GetOrganisationRegistrationCodeTypes(string countryCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetOrganisationRegistrationCodeTypes(countryCode));
			}
		}

		[Route("api/report/filter/lookups")]
		[HttpPost]
		public IHttpActionResult SearchLookupData(LookupFilterSearchArgs searchParam)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				List<CodeDescription> total = null;
				Func<ModuleIdentifier, IBusinessObjectCollection, LookupFilterSearchArgs, IBusinessObjectCollection> moduleDataLoader = null;
				if (Env.CurrentUser.IsWebUser)
				{
					moduleDataLoader = (m, c, p) => LookupFilterDataHelper.LoadModuleDataForContact(m, p, ContactPK ?? ZGuid.Empty);
				}
				else
				{
					moduleDataLoader = LookupFilterDataHelper.LoadModuleDataForStaff;
				}

				total = LookupFilterDataHelper.RetrieveMatchesCodeDescriptions(Service.GetLookupData(searchParam, moduleDataLoader) as IEnumerable<BusinessObject>);

				return Json(new
				{
					count = total.Count,
					results = total
				});
			}
		}

		[Route("api/report/filter/calculateDateSchedule")]
		[HttpPost]
		public IHttpActionResult CalculateDateSchedule(JObject data)
		{
			var dateSchedulePropertyName = "dateSchedule";
			var scheduleTaskPropertyName = "scheduleTask";

			if (!data.ContainsKey(dateSchedulePropertyName) || !data.ContainsKey(scheduleTaskPropertyName))
			{
				Service.RunningError.ErrorType = DocumentEngine.Exceptions.ReportServiceErrorType.ValidationError;
				Service.RunningError.Errors.Add($"{dateSchedulePropertyName} and {scheduleTaskPropertyName} are mandatory.");
				return null;
			}

			DateScheduleData dateSchedule;
			ReportScheduleTaskData scheduleTask;

			try
			{
				dateSchedule = data[dateSchedulePropertyName].ToObject<DateScheduleData>();
				scheduleTask = data[scheduleTaskPropertyName].ToObject<ReportScheduleTaskData>();
			}
			catch (Exception ex)
			{
				Service.RunningError.ErrorType = DocumentEngine.Exceptions.ReportServiceErrorType.ValidationError;
				Service.RunningError.Errors.Add(ex.Message);
				return null;
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.CalculateDateSchedule(dateSchedule, scheduleTask));
			}
		}

		[Route("api/report/filter/getDateSchedule")]
		[HttpPost]
		public IHttpActionResult GetDateSchedule(JObject data)
		{
			var storageValue = data["storageValue"].ToObject<DateTime?>();
			var scheduleTask = data["scheduleTask"].ToObject<ReportScheduleTaskData>();
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetDateSchedule(storageValue, scheduleTask));
			}
		}

		[Route("api/report/filter/calculateAccPeriodSchedule")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult CalculateAccPeriodSchedule(JObject data)
		{
			var periodSchedulePropertyName = "periodSchedule";
			if (!data.ContainsKey(periodSchedulePropertyName))
			{
				Service.RunningError.ErrorType = DocumentEngine.Exceptions.ReportServiceErrorType.ValidationError;
				Service.RunningError.Errors.Add($"{periodSchedulePropertyName} is mandatory.");
				return null;
			}

			AccPeriodScheduleData accPeriodSchedule;
			ReportScheduleTaskData scheduleTask;
			try
			{
				accPeriodSchedule = data[periodSchedulePropertyName].ToObject<AccPeriodScheduleData>();
				scheduleTask = data["scheduleTask"]?.ToObject<ReportScheduleTaskData>();
			}
			catch (Exception ex)
			{
				Service.RunningError.ErrorType = DocumentEngine.Exceptions.ReportServiceErrorType.ValidationError;
				Service.RunningError.Errors.Add(ex.Message);
				return null;
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.CalculateAccPeriodSchedule(accPeriodSchedule, scheduleTask));
			}
		}

		[Route("api/report/filter/getAccPeriodSchedule")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult GetAccPeriodSchedule(JObject data)
		{
			var storageValue = data["storageValue"].ToObject<DateTime?>();
			var scheduleTask = data["scheduleTask"].ToObject<ReportScheduleTaskData>();
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetAccPeriodSchedule(storageValue, scheduleTask));
			}
		}
	}

	#endregion
}
