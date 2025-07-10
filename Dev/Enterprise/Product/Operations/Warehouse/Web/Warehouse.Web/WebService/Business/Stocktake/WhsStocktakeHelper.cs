using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Tools;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class WhsStocktakeHelper
	{
		#region ValidateStaffAndWarehouse

		public static string ValidateStaffAndWarehouse(BusinessObjectFactory factory, string staffUserName, string warehouseCode)
		{
			var validationError = string.Empty;
			var oper = WebServiceHelper.GetStaff(factory, staffUserName);
			var warehouse = WebServiceHelper.GetWarehouse(factory, warehouseCode);
			if (oper == null || warehouse == null)
			{
				validationError = Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service.");
			}
			return validationError;
		}

		#endregion

		#region GetNextUnfinalizedStocktake

		public static WhsStocktakeSessionObject GetUnfinalizedStocktake(WebServiceResponse response, BusinessObjectFactory factory, string staffUserName, string warehouseCode, string referenceOrEmpty, string area, string pickMethod, GlbStaff oper, WhsWarehouse warehouse, bool addLog = false)
		{
			WhsStocktakeSessionObject result = null;
			var validationError = ValidateStaffAndWarehouse(factory, staffUserName, warehouseCode);
			if (!string.IsNullOrEmpty(validationError))
			{
				response.LogBusinessValidationError(validationError);
			}
			else
			{
				var stocktakeManager = new StocktakeManager(warehouse.Factory, area, pickMethod, warehouse, oper);
				if (stocktakeManager.FindAndAssignNextStocktake(referenceOrEmpty))
				{
					var stocktakeInfo = new WhsStocktakeInfo(stocktakeManager.Stocktake);
					result = new WhsStocktakeSessionObject(stocktakeInfo, new WhsStocktakeLineInfoCollection(stocktakeInfo, stocktakeManager.LinesToCount));
					if (!Env.Security.WhsRFScanningStocktakeShowSystemCounts.IsAllowed)
					{
						result.LinesToCount.ForEach(l => l.SystemUnits = null);
					}

					if (addLog)
					{
						stocktakeManager.Stocktake.Logs.AddNew(ZArchitecture.Business.Events.ServiceCommenced, "RF");
						factory.Save();
					}
				}
				else
				{
					if (string.IsNullOrEmpty(referenceOrEmpty))
					{
						response.LogBusinessValidationError(Res.GetString("F64D52A9-0629-4F99-B5B5-971065494F75", "No stocktake available."));
					}
					else
					{
						response.LogBusinessValidationError(Res.GetString("DE1CE8A9-B359-448E-B7AD-01F5A9FD9CB4", "Stocktake '{0}' does not exist or is not available.", referenceOrEmpty));
					}
				}
			}
			return result;
		}

		#endregion

		#region SetStocktakeLineCount

		public static WhsStocktakeLine SetStocktakeLineCount(BusinessObjectFactory factory, Guid stocktakeLinePK, WebServiceResponse response, Func<WhsStocktakeLine, string> setStocktakeLine)
		{
			var stocktakeLine = factory.Load<WhsStocktakeLine>(new ZGuid(stocktakeLinePK));
			if (stocktakeLine != null)
			{
				var setStocktakeLineResultMsg = setStocktakeLine(stocktakeLine);
				if (!string.IsNullOrEmpty(setStocktakeLineResultMsg))
				{
					response.LogBusinessValidationError(setStocktakeLineResultMsg);
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("8de3af73-a7f5-4a62-a7fe-247aa08e95da", "Stocktake Line could not be found."));
			}

			return stocktakeLine;
		}

		#endregion
	}
}
