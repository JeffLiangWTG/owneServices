using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Core;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Update Package Weight From Carton Size Weight UQ")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public SinglePackageForPackingWebServiceResponse UpdatePackageWeightFromCartonSizeWeightUQ(PackageForPackingInfo packageToUpdate, WhsCartonSizeInfo cartonSize)
		{
			return HandleWebServiceRequest<SinglePackageForPackingWebServiceResponse>(r => UpdatePackageWeightFromCartonSizeWeightUQCore(r, packageToUpdate, cartonSize));
		}

		void UpdatePackageWeightFromCartonSizeWeightUQCore(SinglePackageForPackingWebServiceResponse response, PackageForPackingInfo packageToUpdate, WhsCartonSizeInfo cartonSize)
		{
			if (packageToUpdate.IsUsingCartonSizes)
			{
				if (Constants.Weight.ContainsCode(cartonSize.WeightUQ) && Constants.Weight.ContainsCode(packageToUpdate.WeightUQ))
				{
					decimal updatedWeight;

					if (packageToUpdate.ScannedProductInfos != null)
					{
						updatedWeight = 0m;

						foreach (var scannedInfo in packageToUpdate.ScannedProductInfos)
						{
							updatedWeight += Constants.Weight.Convert(scannedInfo.Quantity * scannedInfo.ProductWeight, scannedInfo.ProductWeightUQ, cartonSize.WeightUQ);
						}
					}
					else
					{
						updatedWeight = Constants.Weight.Convert(packageToUpdate.Weight, packageToUpdate.WeightUQ, cartonSize.WeightUQ);
					}

					response.PackageForPackingInfo = new PackageForPackingInfo { Weight = updatedWeight, WeightUQ = cartonSize.WeightUQ };
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("6997aa11-ef02-4d66-8b91-abc5d7f4674b", "Invalid Weight Unit(s) provided. Source '{0}', Target '{1}'.", packageToUpdate.WeightUQ, cartonSize.WeightUQ));
				}
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("000344fd-c5e7-420a-899a-99feba78975f", "Cannot convert Package Weight when not using Carton Sizes."));
			}
		}
	}
}
