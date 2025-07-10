using System;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ChangePackageCartonGroupAndSizeInfo

		[WebMethod(Description = "Change the Package's CartonGroup & Size and update Dimensions info and Volume info.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ChangePackageCartonGroupAndSizeInfo(Guid trolleyJobPK, string packageID, string cartonGroupCode, string cartonSizeCode)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ChangePackageCartonGroupAndSizeInfoCore(r, trolleyJobPK, packageID, cartonGroupCode, cartonSizeCode));
		}

		void ChangePackageCartonGroupAndSizeInfoCore(WebServiceResponse response, Guid trolleyJobPK, string packageID, string cartonGroupCode, string cartonSizeCode)
		{
			var trolleyJob = WebServiceHelper.GetTrolleyJobUsingTrolleyJobPK(Factory, response, trolleyJobPK);
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var package = WebServiceHelper.GetPackageOnTrolley(response, trolleyJob, packageID);
				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					var cartonGroup = WebServiceHelper.GetCartonGroupUsingCartonGroupCode(Factory, response, cartonGroupCode);
					if (string.IsNullOrEmpty(response.ErrorMessage))
					{
						var cartonSize = cartonGroup.CartonSizes.FirstOrDefault(s => s.WCS_Code == cartonSizeCode);
						if (cartonSize == null)
						{
							response.ErrorMessage = Res.GetString("89da37bb-b3c8-47b5-9847-3dc1cc8647fb", "Specified Carton Size was not in Carton Group '{0}'.", cartonGroup.WCG_Code);
						}
						else
						{
							package.SetValuesFromTemplate(cartonSize);
							package.CartonGroupAndSize = string.Format(CultureInfo.InvariantCulture, "{0} - {1}", cartonGroup.WCG_Code, cartonSize.WCS_Code);

							Factory.Save();
						}
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
