using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Update Weight And Dims For Handling Unit")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse UpdateWeightAndDimsForHandlingUnit(Guid packagePK, PackageDimensionsInfo packageDimensionsInfo)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => UpdateWeightAndDimsForHandlingUnit(response, packagePK, packageDimensionsInfo));
		}

		void UpdateWeightAndDimsForHandlingUnit(WebServiceResponse response, ZGuid packagePK, PackageDimensionsInfo packageDimensionsInfo)
		{
			var handlingUnit = Factory.Load<PkgPackage>(packagePK);
			if (handlingUnit == null)
			{
				response.LogBusinessValidationError(Res.GetString("576e70e6-5ac6-4a82-822c-71ebf22acde7", "Handling Unit was not found."));
			}
			else if (packageDimensionsInfo == null)
			{
				response.LogBusinessValidationError(Res.GetString("04eff538-c846-4a76-9554-45f443c2438b", "Package Dimensions were empty."));
			}
			else
			{
				if (!string.IsNullOrEmpty(packageDimensionsInfo.WeightUQ))
				{
					handlingUnit.KP_WeightUQ = packageDimensionsInfo.WeightUQ;
				}

				if (!string.IsNullOrEmpty(packageDimensionsInfo.DimensionUQ))
				{
					handlingUnit.KP_DimensionUQ = packageDimensionsInfo.DimensionUQ;
				}

				handlingUnit.KP_F3_NKPackType = packageDimensionsInfo.PackType;
				handlingUnit.KP_TareWeight = packageDimensionsInfo.EmptyWeight;
				handlingUnit.KP_Weight = packageDimensionsInfo.Weight;
				handlingUnit.KP_Length = packageDimensionsInfo.Length;
				handlingUnit.KP_Width = packageDimensionsInfo.Width;
				handlingUnit.KP_Height = packageDimensionsInfo.Height;

				var concurrencyErrorMessage = Res.GetString("07ff4007-74fa-4d02-8973-2ba9720dad69", "Another user has changed the package while you have been working on it. Please try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}
	}
}
