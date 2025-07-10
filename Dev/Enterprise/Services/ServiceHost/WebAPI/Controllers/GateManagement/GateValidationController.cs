
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/gate")]
	public class GateValidationController : ApiController
	{
		[HttpGet]
		[Route("")]
		public IHttpActionResult Get()
		{
			return Ok((NoResString)"Welcome to Gate Management Validation");
		}

		[HttpPost]
		[Route("validate/gatein/{vehicleMovementPk}")]
		public IHttpActionResult ValidateGateInMovement(Guid vehicleMovementPk)
		{
			if (!WarehouseDataRegistry.Instance.EnableGateInValidation.Value)
			{
				return Json(new GateInValidationResponse(true, Res.GetString("be0ee4d4-f81a-461b-992d-14f6ab11c321", "Gate-in validation is not enabled")));
			}

			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var gvm = factory.Load<IGteVehicleMovement>(vehicleMovementPk);
				if (gvm == null)
				{
					return Json(new GateInValidationResponse(false, Res.GetString("2a524826-4f62-4964-8c03-0422a728125d", "This vehicle movement does not exist")));
				}

				var movementFacilityTypes = gvm.FacilityTypes;
				var gateFacilityValidationServiceProviders = (Hashtable)ObjectFactory.Get("GateFacilityValidationServiceList");
				var relevantValidationProviders = gateFacilityValidationServiceProviders
					.Cast<DictionaryEntry>()
					.Where(entry => movementFacilityTypes.Contains((string)entry.Key))
					.Select(entry => ((ObjectHandle)entry.Value).GetObject() as IGateFacilityValidationService);

				if (relevantValidationProviders.Any())
				{
					var movementShipment = CreateShipment((BusinessObject)gvm);
					var validationErrors = relevantValidationProviders
						.Select(p => p.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn))
						.SelectMany(errors => errors)
						.Where(error => !string.IsNullOrEmpty(error));

					if (validationErrors.Any())
					{
						return Json(new GateInValidationResponse(false, string.Join(System.Environment.NewLine, validationErrors.Select(s => " - " + s))));
					}
					else
					{
						return Json(new GateInValidationResponse(true, string.Empty));
					}
				}

				return Json(new GateInValidationResponse(true, Res.GetString("320ca0fa-47d4-4e0d-8fba-0d982047b333", "There is no validation for this facility type")));
			}
		}

		UniversalShipment CreateShipment(BusinessObject vehicleMovement)
		{
			var manager = new DataWritingManager(new ActionInfo(null, vehicleMovement));
			var writer = ObjectFactory.Get<IGteVehicleMovementDataObjectWriter>("IGteVehicleMovementDataObjectWriter", manager);
			var shipment = writer.GetDataObject(vehicleMovement);

			return (UniversalShipment)shipment;
		}
	}

	class GateInValidationResponse
	{
		public GateInValidationResponse(bool passed, string message)
		{
			GateInValidationPassed = passed;
			ValidationErrorMessage = message;
		}

		[JsonProperty(nameof(GateInValidationPassed))]
		public bool GateInValidationPassed { get; set; }

		[JsonProperty(nameof(ValidationErrorMessage))]
		public string ValidationErrorMessage { get; set; }
	}
}
