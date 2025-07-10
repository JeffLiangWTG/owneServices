using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public static class WhsTransferHelper
	{
		#region FindClientPK

		public static ZGuid FindClientPK(BusinessObjectFactory factory, ZString clientCode)
		{
			return clientCode.IsEmpty ? ZGuid.Empty : WebServiceHelper.GetOrgHeader(factory, clientCode)?.PK ?? ZGuid.Invalid;
		}

		#endregion

		#region GetOldestMatchingTransfer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of sql query")]
		public static WhsTransfer GetOldestMatchingTransfer(BusinessObjectFactory factory, GlbStaff staff, SearchFilterCriteriaInfo criteria, ZGuid clientPK, ZGuid warehousePK, IEnumerable<Guid> exceptedPKs, string pickNoOrDocketId, bool onlyReturnReplenishments)
		{
			WhsTransfer matchingTransfer = null;

			if (clientPK.IsValid || clientPK.IsEmpty)
			{
				var replenishmentQuery = onlyReturnReplenishments
					? "WhsTransfer.WD_IsPickFaceReplenishment = 1 and"
					: "";

				var clientQuery = clientPK.IsValid
					? "WhsTransfer.WD_OH_Client = @ClientPK and"
					: "";

				var isAnyPickMethod = WhsAreaAndPickMethodHelper.IsAnyCode(criteria.PickMethod);
				var pickMethodQuery = isAnyPickMethod
					? ""
					: $"WL_PickMethod IN ('', '{WhsAreaAndPickMethodHelper.AnyCode}', @PickMethod) and";

				var isAnyArea = WhsAreaAndPickMethodHelper.IsAnyCode(criteria.AreaCode);
				var areaQuery = isAnyArea
					? ""
					: "WA_Name IN ('', @AreaName) and";

				var isReferenceProvided = !pickNoOrDocketId.IsNullOrEmpty();
				var referenceQuery = isReferenceProvided
					? "((WhsOrder.WP_PickNo = @PickNoOrDocketID or WhsPick.WP_PickNo = @PickNoOrDocketID) OR (WhsTransfer.WD_DocketID = @PickNoOrDocketID)) and"
					: "";

				var pickForDynamicPickFace = isReferenceProvided
					? "left join dbo.WhsPick on WD_WP_PickBeingReplenished = WP_PK"
					: "";

				var dynamicPickFaceCondition = isReferenceProvided
					? "case when (WhsPick.WP_PK is not null) then 0 else 1 end as DynamicPickFaceCondition,"
					: "";

				var sortOrder = isReferenceProvided
					? "DynamicPickFaceCondition asc, Cond1 asc, Cond2 asc, Cond3 asc, Cond4 asc, Cond5 asc,"
					: "Cond1 asc, Cond2 asc, Cond3 asc, Cond4 asc, Cond5 asc,";

				var pickMethodAndAreaJoin = isAnyArea && isAnyPickMethod
					? ""
					: $@"
		cross apply
		(
			select
				WL_PickMethod,
				WA_Name
			from
				dbo.WhsLocation
				join dbo.WhsArea on WA_PK = WL_WA_PickingArea
			where
				WL_PK = WE_WL_TransferFrom
				and WE_DocketLineStatus = '{DocketLineStatus.Codes.Entered}'

			union all

			select
				WL_PickMethod,
				WA_Name
			from
				dbo.WhsLocation
				join dbo.WhsArea on WA_PK = WL_WA_PickingArea
			where
				WL_PK = WE_WL
				and WE_DocketLineStatus = '{DocketLineStatus.Codes.HeldForTransfer}'

			union all

			select
				'' as WL_PickMethod,
				'' as WA_Name
			where
				WE_WL is null
				and WE_DocketLineStatus = '{DocketLineStatus.Codes.HeldForTransfer}'
		) as WhsTransferLineLocation";

				var rawSQL = $@"
WD_PK in
(select WD_PK from (
	select top 1
		WD_PK,
		{dynamicPickFaceCondition}
		case when (WhsTransferLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.Entered}' and WZ_GS_NKAssignedTo != '') or (WE_DocketLineStatus = '{DocketLineStatus.Codes.HeldForTransfer}' and WE_GS_NKPutawayBy != '') then 0 else 1 end as Cond1,
		case when (WF_PK is not null) then 0 else 1 end as Cond2,
		case when (WhsOrder.WP_PK is not null) then 0 else 1 end as Cond3,
		case when (WD_IsPickFaceReplenishment = 1) then 0 else 1 end as Cond4,
		case when (isnull(Inventory.AvailableQty, 0) <= WF_ReplenishMinimum) then 0 else 1 end as Cond5
	from
		dbo.WhsDocket as WhsTransfer
		join dbo.WhsDocketLine as WhsTransferLine on WhsTransferLine.WE_WD = WhsTransfer.WD_PK
		join dbo.WhsPickLine on WZ_WE_TransactionLine = WhsTransferLine.WE_PK{pickMethodAndAreaJoin}
		{pickForDynamicPickFace}
		left join dbo.WhsPickFace on WF_OP = WhsTransferLine.WE_OP and WF_WL = WhsTransferLine.WE_WL and WF_OH_Client = WhsTransfer.WD_OH_Client
		-- Order
		left join
		(
			select
				WP_PK,
				WP_PickNo,
				WD_WW_Whs,
				WD_OH_Client,
				WE_OP
			from
				dbo.WhsDocket
				join dbo.WhsDocketLine on WE_WD = WD_PK
				join dbo.WhsPick on WP_PK = WD_WP
				left join
				(
					select
						WZ_WE_TransactionLine,
						sum(WZ_Units) as AllocatedQty
					from
						dbo.WhsPickLine
					group by
						WZ_WE_TransactionLine
				) as PickLines on PickLines.WZ_WE_TransactionLine = WE_PK
			where
				WP_IsAwaitingReplenishment = 1 and
				ISNULL(PickLines.AllocatedQty, 0) < WE_TransactionQuantity
		) as WhsOrder on WhsOrder.WD_OH_Client = WF_OH_Client and WhsOrder.WD_WW_Whs = WhsTransfer.WD_WW_Whs and WhsOrder.WE_OP = WF_OP
		-- Inventory
		left join
		(
			select
				WD_OH_Client,
				WI_OP,
				WI_WL,
				sum(WI_TotalUnits) as AvailableQty
			from
				dbo.WhsInventoryView
				join dbo.WhsDocketLine on WE_PK = WI_WE_InDocketLine
				join dbo.WhsDocket on WD_PK = WE_WD
			where
				WI_TotalUnits > 0
			group by
				WD_OH_Client,
				WI_OP,
				WI_WL
		) as Inventory on Inventory.WD_OH_Client = WF_OH_Client and Inventory.WI_OP = WF_OP and Inventory.WI_WL = WF_WL
	where
		WhsTransfer.WD_WP_ParentPickForTransfer is null and
		WhsTransfer.WD_IsPutawayTransfer = 0 and
		WhsTransfer.WD_DocketType = '{DocketType.Codes.Transfer}' and
		WhsTransfer.WD_DocketSubType = '{TransferType.Codes.Internal}' and
		(
			(WhsTransferLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.Entered}' and WZ_GS_NKAssignedTo IN ('', @User))
			or
			(WhsTransferLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.HeldForTransfer}' and WhsTransferLine.WE_GS_NKPutawayBy IN ('', @User))
		) and
		{pickMethodQuery}
		{areaQuery}
		WhsTransfer.WD_WW_Whs = @WarehousePk and
		{clientQuery}
		{replenishmentQuery}
		{referenceQuery}
		not WhsTransfer.WD_PK in (select Value from @ExceptedPKs) 

	order by
		{sortOrder}
		WD_BookingDate
) as InnerQuery)";

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@WarehousePk", warehousePK, WhsDocketSchema.WD_WW_Whs);
				sqlParams.Add("@User", staff.GS_Code, WhsPickLineSchema.WZ_GS_NKAssignedTo);
				sqlParams.Add(ZSqlParameter.New("@ExceptedPKs", exceptedPKs ?? Enumerable.Empty<Guid>(), WhsDocketSchema.PK, true));

				if (clientPK.IsValid)
				{
					sqlParams.Add("@ClientPK", clientPK, WhsDocketSchema.WD_OH_Client);
				}

				if (!isAnyPickMethod)
				{
					sqlParams.Add("@PickMethod", criteria.PickMethod, WhsLocationSchema.WL_PickMethod);
				}

				if (!isAnyArea)
				{
					sqlParams.Add("@AreaName", criteria.AreaCode, WhsAreaSchema.WA_Name);
				}

				if (isReferenceProvided)
				{
					sqlParams.Add("@PickNoOrDocketID", pickNoOrDocketId, WhsDocketSchema.WD_DocketID);
				}

				var query = new ZDBOnlyQuery(typeof(WhsDocket));
				query.AddFilterAndZSQLParameterCollection(rawSQL, sqlParams);

				matchingTransfer = factory.LoadTop1<WhsTransfer>(query);
			}

			return matchingTransfer;
		}

		#endregion

		#region SetLinesForAllocateOrPutaway

		public static void SetLinesForAllocateOrPutaway(BusinessObjectFactory factory, WhsDocketWebServiceResponse response, WhsTransfer transfer, GlbStaff staff, SearchFilterCriteriaInfo criteria, bool isForcedPutaway, ZGuid processTaskPK)
		{
			if (transfer != null && transfer.CheckTransferIsMasterTransfer(response))
			{
				response.Docket = new WhsDocketInfo(transfer, shouldCreateDocketLines: false);

				var transferLines = GetLinesForAllocateOrPutaway(response, transfer, staff, criteria, isForcedPutaway, processTaskPK);
				if (response.NoError())
				{
					if (!processTaskPK.IsEmpty)
					{
						response.Docket.TaskPK = processTaskPK.ToGuid();
					}
					response.Docket.Lines = transferLines;
					response.Docket.PalletsToTransferCompletely = transfer.WD_IsPutawayTransfer
						? new List<string> { transferLines[0].PalletID }
						: GetCompletePalletsForTransfer(factory, transfer);
					transfer.AddEvents(ZArchitecture.Business.Events.ServiceCommenced);
				}
			}
		}

		#region GetLinesForAllocateOrPutaway

		static WhsDocketLineInfoCollection GetLinesForAllocateOrPutaway(WhsDocketWebServiceResponse response, WhsTransfer transfer, GlbStaff staff, SearchFilterCriteriaInfo criteria, bool isForcedPutaway, ZGuid processTaskPK)
		{
			var linesToAllocate = new List<WhsTransferLine>();
			var linesToPutaway = new List<WhsTransferLine>();
			var linesToPutawayPreallocated = new List<WhsTransferLine>();

			foreach (WhsTransferLine line in transfer.Lines)
			{
				if (CanLineBeAllocatedOrPutaway(line, staff, criteria.AreaCode, criteria.PickMethod, processTaskPK))
				{
					if (line.WE_DocketLineStatus == DocketLineStatus.Codes.Entered)
					{
						linesToAllocate.Add(line);
					}
					else if (line.IsHeldForTransfer)
					{
						if (line.WE_WL.IsEmpty)
						{
							linesToPutaway.Add(line);
						}
						else
						{
							linesToPutawayPreallocated.Add(line);
						}
					}
				}
			}

			var result = SelectAndOrderLinesForAllocateOrPutaway(response, isForcedPutaway, linesToAllocate, linesToPutaway, linesToPutawayPreallocated);
			if (response.NoError())
			{
				AllocateLinesToAUser(result, staff);
				return new WhsDocketLineInfoCollection(result);
			}
			else
			{
				return new WhsDocketLineInfoCollection();
			}
		}

		#region CanLineBeAllocatedOrPutaway

		static bool CanLineBeAllocatedOrPutaway(WhsTransferLine line, GlbStaff staff, string area, string pickMethod, ZGuid processTaskPK)
		{
			return
				IsTransferLineOnProcessTask(line, processTaskPK)
				&& (CanTransferLineBeAllocated(line, staff, area, pickMethod) || (CanTransferLineBePutaway(line, staff, pickMethod)));
		}

		static bool IsTransferLineOnProcessTask(WhsTransferLine line, ZGuid processTaskPK) => processTaskPK.IsEmpty || line.WE_P9_Task == processTaskPK;

		static bool CanTransferLineBeAllocated(WhsTransferLine line, GlbStaff staff, string area, string pickMethod)
			=> line.WE_DocketLineStatus == DocketLineStatus.Codes.Entered
			&& (line.GS_NKPickedBy.IsEmpty || line.GS_NKPickedBy.EqualsIgnoringCase(staff.GS_Code))
			&& IsEquipmentMatch(line.TransferFromLocation, pickMethod)
			&& IsAreaMatch(line.TransferFromLocation, area);

		static bool CanTransferLineBePutaway(WhsTransferLine line, GlbStaff staff, string pickMethod)
			=> line.IsHeldForTransfer
			&& (line.WE_GS_NKPutawayBy.IsEmpty || line.WE_GS_NKPutawayBy.EqualsIgnoringCase(staff.GS_Code))
			&& IsEquipmentMatch(line.Location, pickMethod);

		static bool IsEquipmentMatch(WhsLocation location, string pickMethod)
			=> WhsAreaAndPickMethodHelper.IsAnyCode(pickMethod) || location == null || WhsAreaAndPickMethodHelper.IsAnyCode(location.WLV_PickMethod) || location.WLV_PickMethod.EqualsIgnoringCase(pickMethod);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Properties are used internally.")]
		static bool IsAreaMatch(WhsLocation location, string area)
			=> WhsAreaAndPickMethodHelper.IsAnyCode(area) || location.PickingArea.WA_Name.EqualsIgnoringCase(area);

		#endregion

		#region SelectLinesForAllocateOrPutaway

		static IEnumerable<WhsTransferLine> SelectAndOrderLinesForAllocateOrPutaway(WhsDocketWebServiceResponse response, bool isForcedPutaway, IEnumerable<WhsTransferLine> linesToAllocate, IEnumerable<WhsTransferLine> linesToPutaway, IEnumerable<WhsTransferLine> linesToPutawayPreallocated)
		{
			return (!isForcedPutaway && linesToAllocate.Any())
				? linesToAllocate.OrderBy(line => line, new SortTransferLinesForPicking()).ToArray()
				: SelectLinesToPutaway(response, isForcedPutaway, linesToPutaway, linesToPutawayPreallocated).OrderBy(line => line, new SortTransferLinesForPutaway()).ToArray();
		}

		static IEnumerable<WhsTransferLine> SelectLinesToPutaway(WhsDocketWebServiceResponse response, bool isForcedPutaway, IEnumerable<WhsTransferLine> linesToPutaway, IEnumerable<WhsTransferLine> linesToPutawayPreallocated)
		{
			var putawayLines = (linesToPutawayPreallocated.Any()) ? linesToPutawayPreallocated : linesToPutaway;
			if (isForcedPutaway && !putawayLines.Any())
			{
				response.LogBusinessValidationError(Res.GetString("b7c27d47-57be-4f16-b058-289c9db22363", "None of the lines on this transfer is ready to be putaway. Please pick something first."));
			}

			return putawayLines;
		}

		#endregion

		#region AllocateLinesToAUser

		static void AllocateLinesToAUser(IEnumerable<WhsTransferLine> result, GlbStaff staff)
		{
			foreach (var line in result)
			{
				if (line.GS_NKPickedBy.IsEmpty && line.WE_DocketLineStatus == DocketLineStatus.Codes.Entered)
				{
					line.GS_NKPickedBy = staff.GS_Code;
				}

				if (line.WE_GS_NKPutawayBy.IsEmpty)
				{
					line.WE_GS_NKPutawayBy = staff.GS_Code;
				}

				var pickLines = line.PickLines.Where(pl => !pl.IsPicked);
				var pickLinesInMatchingLines = line.MatchingLines.SelectMany(ml => ml.PickLines.Where(pl => !pl.IsPicked));
				foreach (var pickLine in pickLines.Concat(pickLinesInMatchingLines))
				{
					pickLine.WZ_IsPicking = true;
				}
			}
		}

		#endregion

		#endregion

		#region GetCompletePalletsForTransfer

		public static List<string> GetCompletePalletsForTransfer(BusinessObjectFactory factory, WhsTransfer transfer)
		{
			var isPalletIdNeutralUsed = IsPalletIdNeutralUsed();

			#region rawQuery

			var palletIdNeutralSQL = !isPalletIdNeutralUsed ? "" : PalletIdNeutralSQL;

			var rawQuery = $@"
select distinct
	WE_TransferFromPalletID
from
	(
		select
			WE_TransferFromPalletID,
			SUM(WE_TransactionQuantity) as TotalUnits
		from
			dbo.WhsDocketLine
		where
			WE_WD = @TransferPK
			and WE_TransferFromPalletID <> ''
			and WE_DocketLineStatus <> '{DocketLineStatus.Codes.Finalised}'
		group by
			WE_TransferFromPalletID{palletIdNeutralSQL}
	) as TransferLines
	join
	(
		select
			WE_PalletID,
			SUM(WE_StockOnHand) as TotalUnits
		from
			dbo.WhsDocketLine
			join dbo.WhsDocket on WD_PK = WE_WD
		where
			WE_StockOnHand > 0
			and WD_WW_Whs = @WarehousePk
			and WE_WL is not null
			and WE_PalletID <> ''
		group by
			WE_PalletID
	) as Inventory on TransferLines.WE_TransferFromPalletID = Inventory.WE_PalletID and TransferLines.TotalUnits = Inventory.TotalUnits";

			#endregion

			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@TransferPK", transfer.PK, WhsDocketLineSchema.WE_WD);
			queryParams.Add("@WarehousePk", transfer.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs);
			queryParams.Add("@ClientPK", transfer.WD_OH_Client, WhsDocketSchema.WD_OH_Client);

			dynamicCollection.Load(rawQuery, queryParams);

			var result = new List<string>();

			foreach (DynamicBusinessObject dynamicObject in dynamicCollection)
			{
				var palletId = dynamicObject[WhsDocketLineSchema.WE_TransferFromPalletId].ToString().ToUpper(Culture.Current);
				result.Add(palletId);
			}

			return result;

			bool IsPalletIdNeutralUsed()
			{
				var locationsChecked = new HashSet<ZGuid>();
				foreach (WhsTransferLine line in transfer.Lines)
				{
					if (locationsChecked.Add(line.WE_WL_TransferFrom))
					{
						if (line.TransferFromLocation.LocationType.WLT_IsPalletIDNeutral)
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		static string PalletIdNeutralSQL => PalletIdNeutralSQL_WithSerialNumberColumn;

		static string PalletIdNeutralSQL_WithSerialNumberColumn => $@"

		union all

		select
			PalletID as WE_TransferFromPalletID,
			SUM(WE_TransactionQuantity) as TotalUnits
		from
			(
				select
					WE_PalletID as PalletID,
					WE_WL as Location,
					WE_OP as Product,
					Pallet.WE_PartAttrib1 as InventoryPartAttrib1,
					Pallet.WE_PartAttrib2 as InventoryPartAttrib2,
					Pallet.WE_PartAttrib3 as InventoryPartAttrib3,
					WE_ExpiryDate,
					WE_PackingDate,
					WE_WHC_NKCurrentInventoryHeldCode,
					SUM(WE_StockOnHand) as TotalStock
				from
					dbo.WhsDocketLine as Pallet
					join dbo.WhsDocket on WE_WD = WD_PK
					join dbo.OrgPartRelation on OU_OP = WE_OP and OU_OH = @ClientPK and OU_Relationship in ('{OrgPartRelation.RelationshipTypes.Owner}', '{OrgPartRelation.RelationshipTypes.Both}')
					join dbo.OrgMiscServ on OM_OH = @ClientPK
					join dbo.WhsLocation on Pallet.WE_WL = WL_PK
					join dbo.WhsLocationType on WL_WLT_LocationType = WLT_PK
				where
					WLT_IsPalletIDNeutral = 1
					and WE_StockOnHand > 0
					and WE_PalletID <> ''
					and WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
					and WE_CurrentInventoryStatus IN ('{InventoryStatus.Codes.Available}', '{InventoryStatus.Codes.Held}')
					and WD_WW_Whs = @WarehousePk
					and WD_OH_Client = @ClientPK
					and WE_PalletID not in
					(
						select
							LinesOnTransfer.WE_TransferFromPalletID
						from
							dbo.WhsDocketLine LinesOnTransfer
						where
							LinesOnTransfer.WE_WD = @TransferPK
							and LinesOnTransfer.WE_TransferFromPalletID <> ''
					)
					and not exists
					(
						select null
						from
							dbo.WhsDocketLine AdjustmentLine
							join dbo.WhsPickLine on WZ_WE_TransactionLine = AdjustmentLine.WE_PK
						where
							AdjustmentLine.WE_PalletID = Pallet.WE_PalletID
							and AdjustmentLine.WE_PalletID <> ''
							and AdjustmentLine.WE_DocketLineType = '{DocketType.Codes.Adjustment}'
							and AdjustmentLine.WE_TransactionQuantity < 0
							and AdjustmentLine.WE_DocketLineStatus <> '{DocketLineStatus.Codes.Finalised}'
							and AdjustmentLine.WE_DocketLineStatus <> '{DocketLineStatus.Codes.Cancelled}'
							and WZ_PickedDateTime is null

						union all

						select null
						from
							dbo.WhsDocketLine OtherTransferLine
							join dbo.WhsPickLine on WZ_WE_TransactionLine = OtherTransferLine.WE_PK
						where
							OtherTransferLine.WE_WD <> @TransferPK
							and OtherTransferLine.WE_TransferFromPalletID = Pallet.WE_PalletID
							and OtherTransferLine.WE_TransferFromPalletID <> ''
							and OtherTransferLine.WE_DocketLineType = '{DocketType.Codes.Transfer}'
							and OtherTransferLine.WE_DocketLineStatus <> '{DocketLineStatus.Codes.Finalised}'
							and WZ_PickedDateTime is null

						union all

						select null
						from
							dbo.WhsPickLine
						where
							WZ_WE_InventoryLine = Pallet.WE_PK
							and WZ_PickedDateTime is null
							and WZ_IsPicking = 1
					)
				group by
					WE_PalletID,
					WE_WL,
					WE_OP,
					Pallet.WE_PartAttrib1,
					Pallet.WE_PartAttrib2,
					Pallet.WE_PartAttrib3,
					WE_ExpiryDate,
					WE_PackingDate,
					WE_WHC_NKCurrentInventoryHeldCode
			) as Pallet
			join dbo.WhsDocketLine TransferLine on Location = TransferLine.WE_WL_TransferFrom
			cross apply
			(
				select
					TransferLine.WE_PartAttrib1 as TransferPartAttrib1,
					TransferLine.WE_PartAttrib2 as TransferPartAttrib2,
					TransferLine.WE_PartAttrib3 as TransferPartAttrib3
			) as PartAttribs
		where
			TransferLine.WE_TransferFromPalletID <> ''
			and TransferLine.WE_WD = @TransferPK
			and TransferLine.WE_DocketLineStatus = '{DocketLineStatus.Codes.Entered}'
			and TransferLine.WE_CurrentInventoryStatus NOT IN ('{InventoryStatus.Codes.InTransit}', '{InventoryStatus.Codes.PuttingAway}')
			and exists
			(
				select Pallet.WE_WHC_NKCurrentInventoryHeldCode, Product, InventoryPartAttrib1, InventoryPartAttrib2, InventoryPartAttrib3, Pallet.WE_ExpiryDate, Pallet.WE_PackingDate
				intersect
				select TransferLine.WE_WHC_NKCurrentInventoryHeldCode, TransferLine.WE_OP, TransferPartAttrib1, TransferPartAttrib2, TransferPartAttrib3, TransferLine.WE_ExpiryDate, TransferLine.WE_PackingDate
			)
		group by
			TransferLine.WE_TransferFromPalletID,
			PalletID,
			TotalStock
		having
			SUM(WE_TransactionQuantity) = TotalStock";

		#endregion

		#endregion

		#region Errors

		public static ZString WhsTransferConcurrencyError => Res.GetString("f5856a6c-ef95-4309-8914-57c66306dcd9", "Another user has changed the Transfer Job while you have been working on it. Please restart the operation and try again.");

		#endregion
	}
}
