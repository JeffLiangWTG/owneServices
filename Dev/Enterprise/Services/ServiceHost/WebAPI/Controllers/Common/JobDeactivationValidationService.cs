using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	class JobDeactivationValidationService(ApiController apiController)
	{
		internal bool TryGetCanCancelJob(
			string controllerIDName,
			Guid[] jobPKs,
			out List<JobDeactivationValidationResult> results,
			out IHttpActionResult failedResult)
		{
			results = new List<JobDeactivationValidationResult>(jobPKs.Length);

			if (AreJobPKsEmpty(jobPKs, out failedResult))
			{
				return false;
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(apiController.ControllerContext))
			{
				if (!TryGetController(controllerIDName, out var controller, out failedResult))
				{
					return false;
				}

				if (!TryGetValidationResults(jobPKs, controller, controllerIDName, results, out failedResult))
				{
					return false;
				}
			}

			return true;
		}

		bool AreJobPKsEmpty(Guid[] jobPKs, out IHttpActionResult failedResult)
		{
			failedResult = jobPKs.Length == 0
				? GetBadRequestResult(GetNoJobPKsMessage())
				: null;

			return failedResult != null;
		}

		bool TryGetController(string controllerIDName, out ZController controller, out IHttpActionResult failedResult)
		{
			if (string.IsNullOrEmpty(controllerIDName))
			{
				failedResult = GetBadRequestResult((NoResString)"Please provide a valid controller ID.");
				controller = null;
				return false;
			}

			var controllerID = ZControllerFactory.Instance.GetRegisteredIdentifierByName(controllerIDName);
			controller = controllerID != null ? ZControllerFactory.Create(controllerID) : null;

			if (controller == null)
			{
				failedResult = GetBadRequestResult(GetControllerNotFoundMessage(controllerIDName));
				return false;
			}

			failedResult = null;
			return true;
		}

		bool TryGetValidationResults(
			IEnumerable<Guid> jobPKs,
			ZController controller,
			string controllerIDName,
			List<JobDeactivationValidationResult> results,
			out IHttpActionResult failedResult)
		{
			var factory = new BusinessObjectFactory();

			foreach (var jobPK in jobPKs.Distinct())
			{
				if (!TryGetValidationResultsForJob(jobPK, factory, controller, controllerIDName, results, out failedResult))
				{
					return false;
				}
			}

			failedResult = null;
			return true;
		}

		bool TryGetValidationResultsForJob(
			Guid jobPK,
			BusinessObjectFactory factory,
			ZController controller,
			string controllerIDName,
			List<JobDeactivationValidationResult> results,
			out IHttpActionResult failedResult)
		{
			var job = factory.Load(controller.TypeOfTopLevelBusinessObject, jobPK);

			if (job == null)
			{
				failedResult = GetBadRequestResult(GetJobNotFoundMessage(jobPK, controllerIDName));
				return false;
			}

			if (!HasEditPermissionForSpecifiedModule(controller, job))
			{
				failedResult = GetForbiddenResult(controllerIDName);
				return false;
			}

			AddValidationForJobToResults(results, job);

			failedResult = null;
			return true;
		}

		static bool HasEditPermissionForSpecifiedModule(ZController controller, BusinessObject job)
		{
			var checkpoint = controller.GetCheckPointForEdit(job);

			return checkpoint.IsAllowed;
		}

		static void AddValidationForJobToResults(List<JobDeactivationValidationResult> results, BusinessObject job)
		{
			var cancellable = job as ICancellable;

			results.Add(new JobDeactivationValidationResult
			{
				JobPK = job.PK.ToGuid(),
				ValidationError = cancellable?.CanCancel() ?? string.Empty,
			});
		}

		static string GetNoJobPKsMessage()
		{
			return (NoResString)"At least one job PK must be specified with the jobPK parameter. This parameter can be used multiple times in order to check multiple jobs at once.";
		}

		static string GetJobNotFoundMessage(Guid jobPK, string controllerIDName)
		{
			return $"No job with PK {jobPK} could be found for controller {controllerIDName}.";
		}

		static string GetForbiddenMessage(string controllerIDName)
		{
			return $"The logged in user does not have edit permission for the {controllerIDName} module.";
		}

		static string GetControllerNotFoundMessage(string controllerIDName)
		{
			return $"Could not create a controller with the ID {controllerIDName}. Please provide a valid controller ID.";
		}

		BadRequestErrorMessageResult GetBadRequestResult(string message)
		{
			return new BadRequestErrorMessageResult(message, apiController);
		}

		NegotiatedContentResult<string> GetForbiddenResult(string controllerIDName)
		{
			return new NegotiatedContentResult<string>(HttpStatusCode.Forbidden, GetForbiddenMessage(controllerIDName), apiController);
		}

		internal class JobDeactivationValidationResult
		{
			[JsonProperty("jobPK")]
			public Guid JobPK { get; set; }

			[JsonProperty("validationError")]
			public string ValidationError { get; set; }
		}
	}
}
