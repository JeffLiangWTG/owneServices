using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region RemoveAllPackagesFromTrolley

		[WebMethod(Description = "Remove all packages from a Trolley.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse RemoveAllPackagesFromTrolley(Guid trolleyJobPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => RemoveAllPackagesFromTrolleyCore(r, trolleyJobPK));
		}

		void RemoveAllPackagesFromTrolleyCore(WebServiceResponse response, Guid trolleyJobPK)
		{
			var trolleyJob = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);
			if (WebServiceHelper.CheckTrolleyJobValidAndIsBuilding(response, trolleyJob))
			{
				var ddaService = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>();
				foreach (var slot in trolleyJob.Slots.ToArray())
				{
					response.ErrorMessage = ddaService.RemovePickDockDoorAssignment(slot.WTS_KP_Package, Factory);
					if (!string.IsNullOrEmpty(response.ErrorMessage))
					{
						break;
					}

					slot.Delete();
				}

				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyException.Message);
				}
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		#endregion
	}
}
