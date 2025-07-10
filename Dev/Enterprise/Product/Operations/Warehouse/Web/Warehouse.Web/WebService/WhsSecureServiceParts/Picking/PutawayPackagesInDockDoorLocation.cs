using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Putaway Package In Dock door Location")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse PutawayPackagesInDockDoorLocation(Guid[] packagePKs, string dockDoorLocation)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => PutawayPackagesInDockDoorLocation(response, packagePKs, dockDoorLocation));
		}

		void PutawayPackagesInDockDoorLocation(WebServiceResponse response, Guid[] packagePKs, string dockDoorLocationString)
		{
			var dockDoorLocation = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, dockDoorLocationString);
			ValidatePickPutawayLocation(dockDoorLocation, isPackingStationAllowed: false, dockDoorLocationString, response);

			if (response.NoError())
			{
				var packages = Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, packagePKs));
				if (packages.Length != packagePKs.Length)
				{
					response.LogBusinessValidationError(GetPackagesNotFoundError(packagePKs.Length, packages.Length));
				}
				else if (packages.Any(package => !package.IsClosed))
				{
					response.LogBusinessValidationError(Res.GetString("c6ef9619-31dc-4c2e-be8f-16bf34a09538", "Not all packages that are to be put away to the dock door are closed."));
				}
				else
				{
					var pickQuery = GetPicksQuery(packagePKs);
					var picks = Factory.Load<WhsPick>(pickQuery);
					var dockDoorAssignmentService = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>();
					if (IsDockDoorDifferentThanExpected(picks, dockDoorLocation))
					{
						AttemptOverrideDockDoorLocation(dockDoorAssignmentService, picks, response, dockDoorLocation);
					}
					CreateAndPutawayDockDoorAssignmentForPicks(dockDoorAssignmentService, picks, response);

					if (response.NoError())
					{
						var transferLines = GetTransferLinesToPutaway(response, Factory, packagePKs);
						ValidatePutawayLocationAgainstSiblingPackages(response, Factory, transferLines, packages.First(), dockDoorLocation);
						if (response.NoError())
						{
							PutawayTransferLinesAndSave(response, transferLines, dockDoorLocation);
						}
					}
				}
			}
		}

		void CreateAndPutawayDockDoorAssignmentForPicks(IWhsPickDockDoorAssignmentService dockDoorAssignmentService, WhsPick[] picks, WebServiceResponse response)
		{
			if (response.NoError())
			{
				var errorMessage = dockDoorAssignmentService.TryCreateAndPutawayDockDoorAssignmentForPicks(picks, Factory);
				if (!string.IsNullOrEmpty(errorMessage))
				{
					response.LogBusinessValidationError(errorMessage);
				}
			}
		}

		bool IsDockDoorDifferentThanExpected(WhsPick[] picks, WhsLocation dockDoorLocation)
			=> picks[0].DockDoorPK != dockDoorLocation.PK;

		void AttemptOverrideDockDoorLocation(IWhsPickDockDoorAssignmentService dockDoorAssignmentService, WhsPick[] picks, WebServiceResponse response, WhsLocation dockDoorLocation)
		{
			var errorMessage = dockDoorAssignmentService.GetIsDockDoorOverrideAllowedForPicks(picks, Factory);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				response.LogBusinessValidationError(errorMessage);
			}
			else
			{
				dockDoorAssignmentService.OverrideDockDoorLocationsOfPicks(picks, dockDoorLocation.PK, Factory);
			}
		}

		static ZDBOnlyQuery GetPicksQuery(Guid[] packagePKs)
		{
			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageSubQuery.AddToFilter(PkgPackageSchema.PK, packagePKs);

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageJobSubQuery.AddSubQuery(PkgPackageJobSchema.PK, packageSubQuery, JoinCondition.And);

			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
			orderSubQuery.AddSubQuery(WhsDocketSchema.PK, packageJobSubQuery, JoinCondition.And);

			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddSubQuery(orderSubQuery, JoinCondition.And);
			return pickQuery;
		}

		static void ValidatePutawayLocationAgainstSiblingPackages(WebServiceResponse response, BusinessObjectFactory factory, IEnumerable<WhsTransferLine> transferLinesToPutaway, PkgPackage referencePackage, WhsLocation putawayLocation)
		{
			var referenceTransferLine = transferLinesToPutaway.FirstOrDefault();
			if (referenceTransferLine?.TransferFromLocation.WLV_LocationClass.Equals(LocationClasses.Codes.PST) ?? false)
			{
				ValidatePutawayFromPackingStationAgainstSiblingPackages(response, factory, referencePackage, putawayLocation);
			}
		}

		static void ValidatePutawayFromPackingStationAgainstSiblingPackages(WebServiceResponse response, BusinessObjectFactory factory, PkgPackage referencePackage, WhsLocation putawayLocation)
		{
			if (response.NoError())
			{
				var assignedPutawayLocationFromPackingStation = GetAssignedPutawayLocationFromPackingStation(factory, referencePackage.PackageJob.KJ_ParentID);
				if (assignedPutawayLocationFromPackingStation.LocationPK != Guid.Empty && assignedPutawayLocationFromPackingStation.LocationPK != putawayLocation.PK)
				{
					response.LogBusinessValidationError(Res.GetString("3fd83d84-d74a-426d-be53-5d22ab90161b", "Package cannot be putaway to '{0}' as some packages on the order is in '{1}'.", putawayLocation.WLV_LocationString_UserFriendly, assignedPutawayLocationFromPackingStation.LocationString_UserFriendly));
				}
			}
		}

		static WhsLocationInfo GetAssignedPutawayLocationFromPackingStation(BusinessObjectFactory factory, ZGuid orderPK)
		{
			var rawQuery = $@"
SELECT TOP 1
	PutawayLocation.WLV_PK AS LocationPK,
	PutawayLocation.WLV_LocationString AS LocationString,
	PutawayLocation.WLV_LocationString_UserFriendly AS LocationString_UserFriendly
FROM
	dbo.PkgPackage
	JOIN dbo.PkgPackageItemDivot on KI_KP_Package = KP_PK
	JOIN dbo.WhsPickLine on WZ_PK = KI_ParentID
	JOIN dbo.WhsDocketLine InventoryLine on InventoryLine.WE_PK = WZ_WE_InventoryLine
	JOIN dbo.WhsLocationView TransferFromLocation on InventoryLine.WE_WL_TransferFrom = TransferFromLocation.WLV_PK
	JOIN dbo.WhsLocationView PutawayLocation on InventoryLine.WE_WL = PutawayLocation.WLV_PK
	JOIN dbo.WhsDocketLine OrderLine on OrderLine.WE_PK = WZ_WE_TransactionLine
WHERE
	WZ_WE_OriginalPickedInventoryLine IS NOT NULL
	AND InventoryLine.WE_DocketLineType = '{DocketType.Codes.Transfer}'
	AND InventoryLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
	AND TransferFromLocation.WLV_LocationClass = '{LocationClasses.Codes.PST}'
	AND OrderLine.WE_WD = @OrderPK";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@OrderPK", orderPK, WhsDocketLineSchema.WE_WD)
			};

			var assignedPutawayLocation = new DynamicBusinessObjectCollection(factory);
			assignedPutawayLocation.Load(rawQuery, sqlParams);

			var assignedPutawayLocationInfo = new WhsLocationInfo();
			if (assignedPutawayLocation.Count > 0)
			{
				var putawayLocation = assignedPutawayLocation.Single();
				assignedPutawayLocationInfo.LocationPK = ((ZGuid)putawayLocation[nameof(WhsLocationInfo.LocationPK)]).ToGuid();
				assignedPutawayLocationInfo.LocationString = (ZString)putawayLocation[nameof(WhsLocationInfo.LocationString)];
				assignedPutawayLocationInfo.LocationString_UserFriendly = (ZString)putawayLocation[nameof(WhsLocationInfo.LocationString_UserFriendly)];
			}

			return assignedPutawayLocationInfo;
		}

		void PutawayTransferLinesAndSave(WebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, WhsLocation dockDoorLocation)
		{
			var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName).GS_Code;
			PutawayOutboundTransferLines(dockDoorLocation, transferLines, rfUser, response);
			if (response.NoError())
			{
				WebServiceHelper.SaveFactoryWithExceptionHandling(
					Factory,
					response,
					(concurrencyException) => PutawayPackagesConcurrencyErrorMessage);
			}
		}

		static List<WhsTransferLine> GetTransferLinesToPutaway(WebServiceResponse response, BusinessObjectFactory factory, Guid[] packagePKs)
		{
			var transferLines = new List<WhsTransferLine>();
			var pickLines = GetPickLines(factory, packagePKs);
			if (pickLines.Length > 0)
			{
				if (pickLines.Any(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty || pl.WZ_PickedDateTime.IsValid))
				{
					response.LogBusinessValidationError(InvalidPickLinesForPackagePutawayToOutboundPutawayLocation);
				}
				else
				{
					transferLines = GetOutboundTransferLines(factory, pickLines);
				}
			}

			return transferLines;
		}

		static WhsPickLine[] GetPickLines(BusinessObjectFactory factory, Guid[] packagePKs)
		{
			var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			divotQuery.AddToFilter(PkgPackageItemDivotSchema.KI_KP_Package, packagePKs);

			var pickLinesQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickLinesQuery.AddSubQuery(WhsPickLineSchema.PK, divotQuery, JoinCondition.And);

			return factory.Load<WhsPickLine>(pickLinesQuery);
		}

		static List<WhsTransferLine> GetOutboundTransferLines(BusinessObjectFactory factory, WhsPickLine[] pickLines)
		{
			AddFetchHintsForOutboundTransferCreation(factory, pickLines);

			var now = ZDateTimeOffset.Now;
			var transferLines = new List<WhsTransferLine>();
			var outboundDockDoorTransferCreator = new OutboundDockDoorTransferCreator();
			foreach (var pickLine in pickLines)
			{
				var transferLine = GetOrCreateOutboundTransferLine(pickLine, now, outboundDockDoorTransferCreator);
				if (transferLine != null)
				{
					transferLines.Add(transferLine);
				}
			}

			return transferLines;
		}

		static WhsTransferLine GetOrCreateOutboundTransferLine(WhsPickLine pickLine, ZDateTimeOffset pickedTime, OutboundDockDoorTransferCreator outboundDockDoorTransferCreator)
		{
			var transferLine = (WhsTransferLine)pickLine.InventoryLine;
			if (transferLine == null || transferLine.WE_CurrentInventoryStatus != InventoryStatus.Codes.InTransit)
			{
				pickLine.WZ_PickedDateTime = pickedTime;
				transferLine = outboundDockDoorTransferCreator.CreateOutboundDockDoorTransfer(pickLine);
			}

			return transferLine;
		}

		static void AddFetchHintsForOutboundTransferCreation(BusinessObjectFactory factory, WhsPickLine[] pickLines)
		{
			var transferLinePKs = new HashSet<ZGuid>();
			var orderLinePKs = new HashSet<ZGuid>();
			var originalPickedInventoryLinePKs = new HashSet<ZGuid>();
			foreach (var pickLine in pickLines)
			{
				transferLinePKs.Add(pickLine.WZ_WE_InventoryLine);
				orderLinePKs.Add(pickLine.WZ_WE_TransactionLine);
				originalPickedInventoryLinePKs.Add(pickLine.WZ_WE_OriginalPickedInventoryLine);
			}

			foreach (var transferLinePK in transferLinePKs)
			{
				var matchLineQuery = new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, transferLinePK);
				matchLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				factory.AddFetchHint(WhsDocketLineSchema.Instance, matchLineQuery);
				factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, transferLinePK));
			}

			foreach (var orderLinePK in orderLinePKs)
			{
				factory.AddFetchHint(WhsDocketLineSchema.PK, orderLinePK);
				factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLinePK));
			}
			originalPickedInventoryLinePKs.ForEach(pk => factory.AddFetchHint(WhsDocketLineSchema.PK, pk));

			var dockets = GetDockets(factory, orderLinePKs.AsEnumerable().Union(transferLinePKs).Union(originalPickedInventoryLinePKs).ToArray());

			foreach (var docket in dockets)
			{
				if (docket is WhsOrder)
				{
					factory.AddFetchHint(WhsDocketLineSchema.WE_WD, docket.PK);

					var query = new ZQuery();
					query.AddToFilter(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
					query.AddToFilter(WhsDocketSchema.WD_WP, docket.WD_WP);
					factory.AddFetchHint(WhsDocketSchema.Instance, query);
				}
				else if (docket is WhsTransfer)
				{
					var transferLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK);
					var query = new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, true);
					query.AddToFilter(WhsDocketLineSchema.WE_WE_MatchingLine, null);
					transferLineQuery.AddToFilter(query);
					factory.AddFetchHint(WhsDocketLineSchema.Instance, transferLineQuery);

					factory.AddFetchHint(WhsDocketSchema.PK, docket.PK);
				}
				else
				{
					factory.AddFetchHint(WhsDocketSchema.PK, docket.PK);
				}
			}

			WhsDocket[] GetDockets(BusinessObjectFactory factory, ZGuid[] docketLinePKs)
			{
				var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, docketLinePKs);

				var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

				return factory.Load<WhsDocket>(docketQuery);
			}
		}

		static string InvalidPickLinesForPackagePutawayToOutboundPutawayLocation => Res.GetString("209819ec-fc62-4f70-b3df-6e267b898477", "Some Pick Lines on the package have been picked already or is not picking from a packing station location.");
		static string PutawayPackagesConcurrencyErrorMessage => Res.GetString("63dd27d3-e091-4d0d-8da3-7299c8532a51", "Another user has made changes while you have been working on the packages. Please restart the operation and try again.");
	}
}
