using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Get Consolidation Location infos for Packages To Putaway")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPackageToPutawayToConsolidationLocationsResponse GetConsolidationLocationInfosForPackagesToPutaway(Guid[] packagePKs, string orderID)
		{
			return HandleWebServiceRequest<WhsPackageToPutawayToConsolidationLocationsResponse>(response => GetConsolidationLocationInfosForPackagesToPutawayCore(response, packagePKs, orderID));
		}

		void GetConsolidationLocationInfosForPackagesToPutawayCore(WhsPackageToPutawayToConsolidationLocationsResponse response, Guid[] packagePKs, string orderID)
		{
			var order = WebServiceHelper.GetOrderByDocketID(Factory, response, SecurityHeader.WarehouseCode, orderID);
			if (response.NoError())
			{
				var packages = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, packagePKs));
				if (packages.Length != packagePKs.Length)
				{
					response.LogBusinessValidationError(GetPackagesNotFoundError(packagePKs.Length, packages.Length));
				}
				else
				{
					GetOrCreateOutboundTransferAndAllocateConsolidationLocation(response, packagePKs, order, packages);
				}
			}
		}

		void GetOrCreateOutboundTransferAndAllocateConsolidationLocation(WhsPackageToPutawayToConsolidationLocationsResponse response, Guid[] packagePKs, WhsOrder order, PkgPackage[] packages)
		{
			var transferLines = GetTransferLinesToPutaway(response, Factory, packagePKs);
			if (response.NoError())
			{
				var packagesToPutawayGroupedByOrders = new Dictionary<WhsOrder, (List<WhsTransferLine>, List<PkgPackage>)>();
				packagesToPutawayGroupedByOrders.Add(order, (transferLines, packages.ToList()));

				AllocateConsolidationLocationForPackages(response, packagesToPutawayGroupedByOrders, PutawayPackagesConcurrencyErrorMessage);
			}
		}
	}
}
