using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Update Product Measurables And Unit Rates")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse UpdateProductMeasurablesAndUnitRates(WhsProductInfo productInfo)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => { UpdateProductMeasurablesAndUnitRatesCore(response, productInfo); });
		}

		void UpdateProductMeasurablesAndUnitRatesCore(WebServiceResponse response, WhsProductInfo productInfo)
		{
			var orgSupplierPart = Factory.Load<OrgSupplierPart>(productInfo.PK);
			if (orgSupplierPart != null)
			{
				orgSupplierPart.OP_Width = productInfo.Width;
				orgSupplierPart.OP_Height = productInfo.Height;
				orgSupplierPart.OP_Depth = productInfo.Depth;
				orgSupplierPart.OP_MeasureUQ = productInfo.MeasureUQ;

				orgSupplierPart.OP_Weight = productInfo.Weight;
				orgSupplierPart.OP_WeightUQ = productInfo.WeightUQ;

				var volumesToBig = new List<string>();
				var maxMeasurementDecimal = Math.Pow(10, OrgPartUnitSchema.OF_Cubic.Precision - OrgPartUnitSchema.OF_Cubic.Scale);
				var partUnits = orgSupplierPart.PartUnits.Cast<OrgPartUnit>();
				foreach (var unitRateInfo in productInfo.ProductUnits.Where(u => u.Parent != orgSupplierPart.OP_StockKeepingUnit && !string.IsNullOrEmpty(u.MeasurementUQ)).DistinctBy(u => u.Parent).ToList())
				{
					var cubicValue = VolumeCalculator.Calculate(unitRateInfo.Depth, unitRateInfo.Width, unitRateInfo.Height, unitRateInfo.MeasurementUQ, orgSupplierPart.OP_CubicUQ);
					if (cubicValue >= maxMeasurementDecimal)
					{
						volumesToBig.Add(unitRateInfo.Parent);
					}
					else if (cubicValue > 0)
					{
						foreach (var partUnit in partUnits.Where(pu => pu.ParentUnit == unitRateInfo.Parent))
						{
							partUnit.OF_Width = unitRateInfo.Width;
							partUnit.OF_Height = unitRateInfo.Height;
							partUnit.OF_Depth = unitRateInfo.Depth;
							partUnit.OF_Weight = unitRateInfo.Weight;
							partUnit.OF_Cubic = cubicValue;
						}
					}
				}

				if (volumesToBig.Any())
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("ABC7F245-F0E4-45F2-84B1-0F7A28041DB8", "Dimensions for Pack type(s) '{0}' could not be saved. As Volume calculated is larger than max allowed value. Please enter dimensions of affected pack type(s) in the desktop platform.", string.Join(", ", volumesToBig)));
				}

				try
				{
					Factory.Save();
				}
				catch (ZSaveException saveEx)
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("FBBDC31B-104D-460B-9765-A13E89F42D52", "Product could not be saved. {0}", string.Join("\r\n", saveEx.FriendlyMessage)));
				}
				catch (ZCannotSaveException ex)
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("CFFBF1CB-5202-441E-8705-0A3EB2218E61", "Product could not be saved. {0}", string.Join("\r\n", ex.Message)));
				}
			}
			else
			{
				response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("D0924B1E-CA4E-4BBF-B60E-EDC1C3D0980E", "Product could not be found. Please restart the Unload and try again."));
			}
		}
	}
}
