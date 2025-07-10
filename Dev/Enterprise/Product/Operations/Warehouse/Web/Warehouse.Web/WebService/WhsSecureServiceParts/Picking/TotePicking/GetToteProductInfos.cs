using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetToteProductInfos

		[WebMethod(Description = "Get Tote Products")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PackageProductInfosWebServiceResponse GetToteProductInfos(Guid totePK)
		{
			return HandleWebServiceRequest<PackageProductInfosWebServiceResponse>(r => GetToteProductInfosCore(r, totePK));
		}

		void GetToteProductInfosCore(PackageProductInfosWebServiceResponse response, Guid totePK)
		{
			var tote = Factory.Load<PkgPackage>(totePK);
			if (tote == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("c174c9c3-cbeb-45af-8f5b-e38c7e3fa054", "Cannot find Tote with specified Tote ID.");
			}
			else
			{
				var productInfos = PackageHelper.GetPackageRelatedProductInfos(Factory, tote);
				if (productInfos.Length <= 0)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("8abc271d-bd22-4a96-930b-f752bf713495", "Cannot find any product has been allocated to the Tote.");
				}
				else
				{
					response.PackageID = tote.KP_PackageID;
					response.ProductInfos = productInfos;
				}
			}
		}

		#endregion
	}
}
