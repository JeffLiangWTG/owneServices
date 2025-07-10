using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region UpdatePutawayConsolidationLocationForPackage

		[WebMethod(Description = "Update Putaway Consolidation Location For Package")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse UpdatePutawayConsolidationLocationForPackages(Guid[] packagePKs, Guid locationPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => UpdatePutawayConsolidationLocationForPackages(response, packagePKs, locationPK));
		}

		void UpdatePutawayConsolidationLocationForPackages(WebServiceResponse response, Guid[] packagePKs, Guid locationPK)
		{
			var location = Factory.Load<WhsLocation>(new ZGuid(locationPK));
			if (location == null || !location.IsPackingConsolidationLocation)
			{
				response.LogBusinessValidationError(Res.GetString("92ff2a11-71c8-4b8a-998d-978771020af0", "Packing Consolidation could not be found."));
			}
			else
			{
				PutawayStockInDockDoorOrPackingStationHelper.AddPackageJobFetchHints(Factory, new ZQuery(PkgPackageSchema.PK, packagePKs));
				var packages = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, packagePKs));
				if (packages.Length != packagePKs.Length)
				{
					response.LogBusinessValidationError(GetPackagesNotFoundError(packagePKs.Length, packages.Length));
				}
				else
				{
					ValidateConsolidationLocationAgainstSiblingPackages(response, packages, location, Res.GetString("2d7e1b2f-1410-4d88-9a2c-8a9a51d4f4eb", "Cannot update to consolidation location '{0}' as some packages on the same order are in a different consolidation location.", location.WLV_LocationString_UserFriendly));
					if (response.NoError())
					{
						UpdatePutawayConsolidationLocationForPackagesCore(response, packagePKs, locationPK);
					}
				}
			}
		}

		void UpdatePutawayConsolidationLocationForPackagesCore(WebServiceResponse response, Guid[] packagePKs, Guid locationPK)
		{
			var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			divotQuery.AddToFilter(PkgPackageItemDivotSchema.KI_KP_Package, packagePKs);

			var pickLinesQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickLinesQuery.AddSubQuery(WhsPickLineSchema.PK, divotQuery, JoinCondition.And);

			var pickLines = Factory.Load<WhsPickLine>(pickLinesQuery);

			var noValidPackagesError = Res.GetString("77ea48e8-d9df-4161-985e-112530746f63", "Cannot update partially processed packages.");
			var transferLines = GetTransferLinesForPackageConsolidationPutaway(response, pickLines, noValidPackagesError);

			if (response.NoError())
			{
				transferLines.ForEach(tl => tl.WE_WL = locationPK);

				// Fetch Hints for Saving
				Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transferLines.Select(tl => tl.PK)));
				Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, pickLines.Select(pl => pl.WZ_WE_TransactionLine)));

				var concurrencyErrorMessage = Res.GetString("112731f4-7e14-45d9-8f44-a8e621685334", "Another user has made a change while you have been working on it. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		static string GetPackagesNotFoundError(int expectedPackagesCount, int actualPackagesCount)
			=> Res.GetString("b95a408f-c54f-4532-9ce0-a462509e4cfd", "Some Package(s) were not found in Warehouse. {0} were expected, but {1} packages were found.", expectedPackagesCount, actualPackagesCount);

		#endregion
	}
}
