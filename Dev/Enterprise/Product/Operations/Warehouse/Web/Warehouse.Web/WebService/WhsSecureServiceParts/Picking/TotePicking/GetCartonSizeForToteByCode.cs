using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Get Carton Size for Tote by Code")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsCartonSizeWebServiceResponse GetCartonSizeForToteByCode(string cartonSizeCode)
		{
			return HandleWebServiceRequest<WhsCartonSizeWebServiceResponse>(r => GetCartonSizeForToteByCodeCore(r, cartonSizeCode));
		}

		void GetCartonSizeForToteByCodeCore(WhsCartonSizeWebServiceResponse response, string cartonSizeCode)
		{
			if (string.IsNullOrEmpty(cartonSizeCode))
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("d79f815a-497e-4187-9e39-38bfc3ef27dc", "Please provide a Carton Size Code.");
			}
			else
			{
				var cartonSize = GetCartonSize(cartonSizeCode);
				if (cartonSize == null)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("9064234b-290a-44ef-8d39-5129f95d3524", "Carton Size '{0}' cannot be found.", cartonSizeCode);
				}
				else
				{
					var cartonSizeInfo = new WhsCartonSizeInfo(cartonSize);
					response.CartonSizeInfo = cartonSizeInfo;
				}
			}
		}

		WhsCartonSize GetCartonSize(string cartonSizeCode)
		{
			var query = new ZQuery(WhsCartonSizeSchema.WCS_Code, cartonSizeCode);

			return Factory.LoadTop1<WhsCartonSize>(query);
		}
	}
}
