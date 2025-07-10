using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region CheckStockOnHandForLocationExcludingReceive

		[WebMethod(Description = "Check location has any stock not from specified Docket")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CheckStockOnHandForLocationExcludingReceive(Guid docketPK, string location)
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<WebServiceResponse>(r => CheckStockOnHandForLocationExcludingReceive(r, docketPK, location));
		}

		void CheckStockOnHandForLocationExcludingReceive(WebServiceResponse response, Guid docketPK, string location)
		{
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var inventoryLocation = whs.FindLocation(location);
			if (inventoryLocation != null && CheckStockOnHandExcludingReceipt(docketPK, inventoryLocation))
			{
				response.ErrorMessage = Res.GetString("5b4dc50f-a9ff-4fd4-b6be-f61d1c35fcbc", "Location {0} has stock on hand.", location);
			}
		}

		bool CheckStockOnHandExcludingReceipt(Guid docketPK, WhsLocation inventoryLocation)
		{
			var finalisedQuery = new ZQuery(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Finalised);
			finalisedQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);

			var pendingQuery = new ZQuery(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);
			pendingQuery.AddToFilter(WhsDocketLineSchema.WE_TransactionQuantity, SQLComparisonOperator.GreaterThan, 0m);

			var finalisedAndPendingQuery = new ZQuery();
			finalisedAndPendingQuery.AddToFilter(finalisedQuery, JoinCondition.Or);
			finalisedAndPendingQuery.AddToFilter(pendingQuery, JoinCondition.Or);

			var inventoryIncludingPendingQuery = new ZQuery(WhsDocketLineSchema.WE_WL, inventoryLocation.PK);
			inventoryIncludingPendingQuery.AddToFilter(WhsDocketLineSchema.WE_WD, SQLComparisonOperator.NotEqual, docketPK);
			inventoryIncludingPendingQuery.AddToFilter(finalisedAndPendingQuery, JoinCondition.And);

			return Factory.LoadTop1<WhsDocketLine>(inventoryIncludingPendingQuery) != null;
		}

		#endregion
	}
}
