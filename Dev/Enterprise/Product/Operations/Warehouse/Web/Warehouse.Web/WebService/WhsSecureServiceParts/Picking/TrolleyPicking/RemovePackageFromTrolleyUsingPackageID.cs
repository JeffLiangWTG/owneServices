using System;
using System.Linq;
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
		#region RemovePackageFromTrolleyUsingPackageID

		[WebMethod(Description = "Remove an individual package from a Trolley.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse RemovePackageFromTrolleyUsingPackageID(Guid trolleyJobPK, string packageID)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => RemovePackageFromTrolleyUsingPackageIDCore(r, trolleyJobPK, packageID));
		}

		void RemovePackageFromTrolleyUsingPackageIDCore(WebServiceResponse response, Guid trolleyJobPK, string packageID)
		{
			var trolleyJob = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);

			if (WebServiceHelper.CheckTrolleyJobValidAndIsBuilding(response, trolleyJob))
			{
				var slot = trolleyJob.Slots.FirstOrDefault(s => s.Package.KP_PackageID == packageID);
				if (slot == null)
				{
					var toteTypeName = GetTrolleyPickingTypeNameForMessages(trolleyJob.PickingType);
					response.ErrorMessage = Res.GetString("71b3d12b-df27-4762-9750-eb363c556224", "{0} '{1}' was not on this Trolley.", toteTypeName, packageID);
				}
				else
				{
					response.ErrorMessage = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>().RemovePickDockDoorAssignment(slot.WTS_KP_Package, Factory);
					if (string.IsNullOrEmpty(response.ErrorMessage))
					{
						slot.Delete();
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyException.Message);
					}
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
