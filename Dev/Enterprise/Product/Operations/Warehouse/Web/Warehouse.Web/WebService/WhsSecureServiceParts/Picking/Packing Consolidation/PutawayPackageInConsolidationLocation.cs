using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region PutawayPackageInConsolidationLocation

		[WebMethod(Description = "Putaway Package In Consolidation Locations")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PutawayPackageInConsolidationLocation(Guid packagePK, Guid locationPK)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => PutawayPackageInConsolidationLocation(response, packagePK, locationPK));
		}

		void PutawayPackageInConsolidationLocation(WebServiceResponse response, Guid packagePK, Guid locationPK)
		{
			var location = Factory.Load<WhsLocation>(new ZGuid(locationPK));
			if (location == null || !location.IsPackingConsolidationLocation)
			{
				response.LogBusinessValidationError(Res.GetString("ad60e5dd-d9c4-44ea-bafc-59c653246b65", "Packing Consolidation could not be found."));
			}
			else
			{
				var package = Factory.Load<PkgPackage>(packagePK);
				if (package == null)
				{
					response.LogBusinessValidationError(Res.GetString("7078c1c0-12d0-4531-8640-f4a4ab02c70c", "In-Transit Package was not found in Warehouse."));
				}
				else
				{
					var pickLines = Factory.Load<WhsPickLine>(GetPickLinesQuery(new[] { package.PK }));
					var noValidPackagesError = Res.GetString("875b405e-c41c-4406-98d0-a0ff1316445a", "Cannot putaway partially processed packages.");
					var transferLines = GetTransferLinesForPackageConsolidationPutaway(response, pickLines, noValidPackagesError);

					var referenceTransferLine = transferLines?.FirstOrDefault();
					var isPuttingAwayFromPackingStation = referenceTransferLine?.TransferFromLocation.WLV_LocationClass.Equals(LocationClasses.Codes.PST) ?? false;
					if (!isPuttingAwayFromPackingStation)
					{
						ValidateConsolidationLocationIsAllowedForJob(response, pickLines, location);
					}

					ValidatePutawayConsolidationLocationConsistentWithPutawayTransferLine(response, transferLines, location);
					ValidatePutawayLocationAgainstSiblingPackages(response, package, location, isPuttingAwayFromPackingStation);
					if (response.NoError())
					{
						PutawayPackageInConsolidationLocationCore(response, transferLines, location, package);
					}
				}
			}
		}

		IEnumerable<WhsTransferLine> GetTransferLinesForPackageConsolidationPutaway(WebServiceResponse response, IEnumerable<WhsPickLine> pickLines, string errorMessage)
		{
			IEnumerable<WhsTransferLine> transferLines = null;

			if (pickLines.Any(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty))
			{
				response.LogBusinessValidationError(errorMessage);
			}
			else
			{
				transferLines = Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.PK, pickLines.Select(pl => pl.WZ_WE_InventoryLine)));

				if (transferLines.Any(il => il.IsFinalised))
				{
					response.LogBusinessValidationError(errorMessage);
				}
				else
				{
					AddFetchHintsForSettingLocation(Factory, transferLines);
				}
			}

			return transferLines;
		}

		void ValidateConsolidationLocationIsAllowedForJob(WebServiceResponse response, IEnumerable<WhsPickLine> pickLines, WhsLocation location)
		{
			if (response.NoError())
			{
				var picksForPackage = Factory.Load<WhsPick>(GetPickQuery(pickLines.Select(pl => pl.PK)));
				if (picksForPackage.Length < 1)
				{
					response.LogBusinessValidationError(Res.GetString("113587f7-ee08-477e-9716-c7c0776b4e08", "Unable to find associated pick for package."));
				}
				else
				{
					var pick = picksForPackage[0];
					var wrapper = new PickJobWrapperForPick(Factory, pick.PK.ToGuid(), pick);
					if (!wrapper.CheckIfConsolidationLocationIsAllowedForJob())
					{
						response.LogBusinessValidationError(Res.GetString("cf558c93-657a-48f3-8332-a596185bed93", "Cannot put away the package to consolidation location '{0}' as the associated pick does not support consolidation putaway.",
							location.WLV_LocationString_UserFriendly));
					}
				}
			}
		}

		void ValidatePutawayConsolidationLocationConsistentWithPutawayTransferLine(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, WhsLocation location)
		{
			if (response.NoError())
			{
				var expectedLocationPK = transferLines.Select(line => line.WE_WL).Distinct().Single();
				if (expectedLocationPK != location.PK)
				{
					var expectedConsolidationLocation = Factory.Load<WhsLocation>(expectedLocationPK);
					response.LogBusinessValidationError(Res.GetString("5f9fb5a5-5317-46b0-bad3-a3bc977a5101", "Cannot put away the package to consolidation location '{0}'. The package needs to go to '{1}' consolidation location.",
						location.WLV_LocationString_UserFriendly,
						expectedConsolidationLocation.WLV_LocationString_UserFriendly));
				}
			}
		}

		void ValidatePutawayLocationAgainstSiblingPackages(WebServiceResponse response, PkgPackage package, WhsLocation location, bool isPuttingAwayFromPackingStation)
		{
			if (isPuttingAwayFromPackingStation)
			{
				ValidatePutawayFromPackingStationAgainstSiblingPackages(response, Factory, package, location);
			}
			else
			{
				ValidateConsolidationLocationAgainstSiblingPackages(
					response,
					new[] { package },
					location,
					Res.GetString("3fca99ff-f387-4b89-b8a3-1b519331cb65", "Cannot put away the package to consolidation location '{0}' as some packages on the same order are in a different consolidation location.", location.WLV_LocationString_UserFriendly));
			}
		}

		void ValidateConsolidationLocationAgainstSiblingPackages(WebServiceResponse response, IEnumerable<PkgPackage> packages, WhsLocation location, string invalidLocationErrorMessage)
		{
			if (response.NoError())
			{
				var parentOrderPackageJobPKs = packages
					.DistinctBy(pkg => pkg.KP_KJ_ParentPackageJob)
					.Select(pkg => pkg.PackageJob.KJ_ParentID);
				parentOrderPackageJobPKs.ForEach(orderPK => Factory.AddFetchHint(WhsDocketSchema.PK, orderPK));

				var existingOrderLocationAllocations = PackingConsolidationAllocationHelper.GetConsolidationLocationsWithCurrentPackages(Factory, location.WLV_WW_Whs, parentOrderPackageJobPKs);
				if (existingOrderLocationAllocations.Any(allocationLocationPK => allocationLocationPK != location.PK))
				{
					response.LogBusinessValidationError(invalidLocationErrorMessage);
				}
			}
		}

		void PutawayPackageInConsolidationLocationCore(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, WhsLocation location, PkgPackage package)
		{
			var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName).GS_Code;
			PutawayOutboundTransferLines(location, transferLines, rfUser, response);
			if (response.NoError())
			{
				UpdatePutawayLocationOnSiblingPackages(location, package);
				var concurrencyErrorMessage = Res.GetString("112731f4-7e14-45d9-8f44-a8e621685334", "Another user has made a change while you have been working on it. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		void UpdatePutawayLocationOnSiblingPackages(WhsLocation location, PkgPackage package)
		{
			var (siblingTransferLines, siblingPackagesPicklines) = GetSiblingTransferAndPickLinesWithDifferentPutawayConsolidationLocation(location, package);
			if (siblingTransferLines.Length > 0)
			{
				AddFetchHintsForSettingLocation(Factory, siblingTransferLines);

				var oldLocationPKs = siblingTransferLines.Select(line => line.WE_WL).ToArray();
				siblingTransferLines.ForEach(tl => tl.WE_WL = location.PK);

				AddFetchHintsForSaving(Factory, siblingTransferLines, siblingPackagesPicklines, oldLocationPKs);
			}
		}

		(WhsTransferLine[] siblingTransferLines, WhsPickLine[] siblingPackagesPicklines) GetSiblingTransferAndPickLinesWithDifferentPutawayConsolidationLocation(WhsLocation location, PkgPackage package)
		{
			WhsTransferLine[] siblingTransferLines = null;
			WhsPickLine[] pickedSiblingPackagesPicklines = null;

			var packageJobPK = package.KP_KJ_ParentPackageJob;
			var allPackages = PutawayStockInDockDoorOrPackingStationHelper.LoadOuterPackagesFromPackageJobs(Factory, new[] { packageJobPK });
			if (allPackages.Length > 1)
			{
				var siblingPackagePKs = allPackages.Select(pkg => pkg.PK).Where(pk => pk != package.PK).ToArray();
				pickedSiblingPackagesPicklines = Factory.Load<WhsPickLine>(GetPickLinesQuery(siblingPackagePKs, getPickedOnly: true));
				if (pickedSiblingPackagesPicklines.Length > 0)
				{
					var query = new ZQuery(WhsDocketLineSchema.PK, pickedSiblingPackagesPicklines.Select(pl => pl.WZ_WE_InventoryLine));
					query.AddToFilter(WhsDocketLineSchema.WE_WL, SQLComparisonOperator.NotEqual, location.PK);

					siblingTransferLines = Factory.Load<WhsTransferLine>(query)
						.Where(transferLine => !transferLine.IsFinalised)
						.ToArray();
				}
			}

			return (siblingTransferLines ?? Array.Empty<WhsTransferLine>(), pickedSiblingPackagesPicklines ?? Array.Empty<WhsPickLine>());
		}

		static ZDBOnlyQuery GetPickLinesQuery(IEnumerable<ZGuid> packagePKs, bool getPickedOnly = false)
		{
			var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			divotQuery.AddToFilter(PkgPackageItemDivotSchema.KI_KP_Package, packagePKs);

			var pickLinesQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickLinesQuery.AddSubQuery(WhsPickLineSchema.PK, divotQuery, JoinCondition.And);
			if (getPickedOnly)
			{
				pickLinesQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, SQLComparisonOperator.NotEqual, null);
			}

			return pickLinesQuery;
		}

		static ZDBOnlyQuery GetPickQuery(IEnumerable<ZGuid> pickLinePKs)
		{
			var pickLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineQuery.AddToFilter(WhsPickLineSchema.PK, pickLinePKs);

			var orderLinesQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLinesQuery.AddSubQuery(WhsDocketLineSchema.PK, pickLineQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			orderQuery.AddSubQuery(WhsDocketSchema.PK, orderLinesQuery, JoinCondition.And);

			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddSubQuery(WhsPickSchema.PK, orderQuery, JoinCondition.And);

			return pickQuery;
		}

		static void AddFetchHintsForSettingLocation(BusinessObjectFactory factory, IEnumerable<WhsTransferLine> transferLines)
		{
			var transferLinePKs = transferLines.Select(tl => tl.PK).ToArray();
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, transferLinePKs));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, transferLinePKs));
		}

		static void AddFetchHintsForSaving(BusinessObjectFactory factory, WhsTransferLine[] siblingTransferLines, WhsPickLine[] siblingPackagesPicklines, ZGuid[] locationPKs)
		{
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, siblingTransferLines.Select(tl => tl.PK)));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, siblingPackagesPicklines.Select(pl => pl.WZ_WE_TransactionLine)));

			var locations = factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.PK, locationPKs));
			factory.AddFetchHint(WhsRowSchema.Instance, new ZQuery(WhsRowSchema.PK, locations.Select(location => location.WLV_WR)));
		}

		#endregion
	}
}
