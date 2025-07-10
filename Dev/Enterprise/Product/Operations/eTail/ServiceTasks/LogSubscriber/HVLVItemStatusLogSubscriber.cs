using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	[Serializable]
	public class HVLVItemStatusLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(HVLVItemStatusLogSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.DepartureCode, AutoEvents.ArrivalCode };

		public override string[] TableNames => new[] { JobConsolTransportSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs.Where(x => !x.SJ_IsEstimate))
			{
				var transport = log.Factory.Load<Transport>(log.SJ_ParentID);

				if (transport != null && transport.JW_ParentType == Core.Constants.TransportParentTypes.Shipment)
				{
					ProcessShipments(transport, new[] { log.Factory.Load<ForwardingShipment>(transport.JW_ParentGUID) }, log.SJ_SE_NKEvent);
				}
				else if (transport != null && transport.JW_ParentType == Core.Constants.TransportParentTypes.Consol)
				{
					ProcessShipments(transport, log.Factory.Load<ForwardingConsol>(transport.JW_ParentGUID).Shipments.Cast<ForwardingShipment>(), log.SJ_SE_NKEvent);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Avoid loading large data into memory, Service task logging")]
		void ProcessShipments(Transport transport, IEnumerable<ForwardingShipment> shipments, string eventType)
		{
			var shipmentsToUpdate = shipments.Where(x => IsEventOnHVLShipmentTransport(transport, x, eventType)).ToArray();

			if (shipmentsToUpdate.Length > 0)
			{
				var newHVIStatus = eventType == AutoEvents.DepartureCode ? HVLVItemStatus.Codes.ShipmentDeparted : HVLVItemStatus.Codes.ShipmentArrived;
				var newHVCStatus = eventType == AutoEvents.DepartureCode ? HVLVConsignmentStatus.Codes.DepartedFromOriginDepot : HVLVConsignmentStatus.Codes.ArrivedAtDestination;

				const string newHVIStatusParam = "@NewHVIStatus";
				const string newHVCStatusParam = "@NewHVCStatus";
				const string shipmentPKsParam = "@ShipmentPKs";
				const string headerPKsParam = "@HeaderPKs";
				const string statusesToIgnoreParam = "@StatusesToIgnore";
				const string currentUserCode = "@CurrentUserCode";
				const string currentBranchCode = "@CurrentBranchCode";
				const string currentDepartmentCode = "@CurrentDepartmentCode";

				const string sqlHVLVItems = @"
DECLARE @ItemsToUpdate TABLE
(
	ItemPK		uniqueidentifier,
	OldStatus	varchar(3),
	NewStatus	varchar(3),
	Reference	varchar(50),
	Type		varchar(23)
);

UPDATE dbo.HVLVItem
SET HVI_Status = " + newHVIStatusParam + @"
OUTPUT
	INSERTED.HVI_PK,
	DELETED.HVI_Status,
	INSERTED.HVI_Status,
	CASE WHEN ISNULL(INSERTED.HVI_CurrentBarcode, '') <> '' THEN INSERTED.HVI_CurrentBarcode
		WHEN ISNULL(INSERTED.HVI_ShipperReference, '') <> '' THEN INSERTED.HVI_ShipperReference
		ELSE INSERTED.HVI_ItemId
	END,
	CASE WHEN ISNULL(INSERTED.HVI_CurrentBarcode, '') <> '' THEN 'ITEM BARCODE'
		WHEN ISNULL(INSERTED.HVI_ShipperReference, '') <> '' THEN 'ITEM SHIPPER REFERENCE'
		ELSE 'ITEM ID'
	END
INTO @ItemsToUpdate
WHERE HVI_JS_LoadedOnShipment IN
(
	SELECT Value
	FROM " + shipmentPKsParam + @"
)
AND HVI_Status NOT IN
(
	SELECT Value
	FROM " + statusesToIgnoreParam + @"
)

INSERT INTO dbo.StmALog
(
	SL_Table,
	SL_Parent,
	SL_IsEstimate,
	SL_Reference,
	SL_EventTime,
	SL_SE_NKEvent,
	SL_GS_NKUser,
	SL_GB_NKBranch,
	SL_GE_NKDepartment,
	SL_FireWorkflow
)
SELECT
	'HVLVItem',
	ItemPK,
	'Y',
	CONCAT('|OLD=', OldStatus, '|NEW=', NewStatus, '|RFN=', Reference, '|TYP=', Type),
	GETDATE(),
	'STU',
	" + currentUserCode + @",
	" + currentBranchCode + @",
	" + currentDepartmentCode + @",
	1
FROM
	@ItemsToUpdate
";

				var sqlHVLVConsignments = @"
DECLARE @ConsignmentsToUpdate TABLE
(
	ConsignmentPK   uniqueidentifier,
	OldStatus       varchar(3),
	NewStatus       varchar(3),
	Reference       varchar(50),
	Type            varchar(50)
);

UPDATE dbo.HVLVConsignment
SET HVC_STATUS = " + newHVCStatusParam + @"
OUTPUT
	INSERTED.HVC_PK,
	DELETED.HVC_Status,
	INSERTED.HVC_Status,
	CASE WHEN ISNULL(INSERTED.HVC_WaybillNumber, '') <> '' THEN INSERTED.HVC_WaybillNumber
		WHEN ISNULL(INSERTED.HVC_ShipperReference, '') <> '' THEN INSERTED.HVC_ShipperReference
		ELSE INSERTED.HVC_ConsignmentId
	END,
	CASE WHEN ISNULL(INSERTED.HVC_WaybillNumber, '') <> '' THEN 'CONSIGNMENT WAYBILL NUMBER'
		WHEN ISNULL(INSERTED.HVC_ShipperReference, '') <> '' THEN 'CONSIGNMENT SHIPPER REFERENCE'
		ELSE 'CONSIGNMENT ID'
	END
INTO @ConsignmentsToUpdate
WHERE HVC_HCH_Header IN
(
	SELECT Value
	FROM " + headerPKsParam + @"
)

INSERT INTO dbo.StmALog
(
	SL_Table,
	SL_Parent,
	SL_IsEstimate,
	SL_Reference,
	SL_EventTime,
	SL_SE_NKEvent,
	SL_GS_NKUser,
	SL_GB_NKBranch,
	SL_GE_NKDepartment,
	SL_FireWorkflow
)
SELECT
	'HVLVConsignment',
	ConsignmentPK,
	'Y',
	CONCAT('|OLD=', OldStatus, '|NEW=', NewStatus, '|RFN=', Reference, '|TYP=', Type),
	GETDATE(),
	'STU',
	" + currentUserCode + @",
	" + currentBranchCode + @",
	" + currentDepartmentCode + @",
	1
FROM
	@ConsignmentsToUpdate
";

				using (var cmd = Db.Connection.Command(sqlHVLVItems))
				{
					cmd.AddParameter(newHVIStatusParam, SqlDbType.VarChar, newHVIStatus);
					cmd.AddTableValuedParameter(shipmentPKsParam, HVLVItemSchema.HVI_JS_LoadedOnShipment, shipmentsToUpdate.Select(x => x.PK));
					cmd.AddTableValuedParameter(statusesToIgnoreParam, HVLVItemSchema.HVI_Status, GetStatusesToIgnore);
					cmd.AddParameter(currentUserCode, SqlDbType.VarChar, GlbStaff.CurrentUser?.GS_Code.ToString() ?? string.Empty);
					cmd.AddParameter(currentBranchCode, SqlDbType.VarChar, GlbBranch.CurrentBranch?.GB_Code.ToString() ?? string.Empty);
					cmd.AddParameter(currentDepartmentCode, SqlDbType.VarChar, GlbDepartment.CurrentDepartment?.GE_Code.ToString() ?? string.Empty);

					cmd.ExecuteNonQuery();

					var log = string.Format(CultureInfo.InvariantCulture,
						"{0} Event processed for Shipment(s): {1}",
						eventType, string.Join(", ", shipmentsToUpdate.Select(x => x.JS_UniqueConsignRef)));
					DefaultLogger.Log(LogType.Information, log);
				}

				using (var cmd = Db.Connection.Command(sqlHVLVConsignments))
				{
					var consignmentHeaders = shipmentsToUpdate.Select(shipment => shipment.GetOrCreateHVLVConsignmentHeader());
					cmd.AddParameter(newHVCStatusParam, SqlDbType.VarChar, newHVCStatus);
					cmd.AddTableValuedParameter(shipmentPKsParam, HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipmentsToUpdate.Select(x => x.PK));
					cmd.AddTableValuedParameter(headerPKsParam, HVLVConsignmentSchema.HVC_HCH_Header, consignmentHeaders.Select(header => header.PK));
					cmd.AddParameter(currentUserCode, SqlDbType.VarChar, GlbStaff.CurrentUser?.GS_Code.ToString() ?? string.Empty);
					cmd.AddParameter(currentBranchCode, SqlDbType.VarChar, GlbBranch.CurrentBranch?.GB_Code.ToString() ?? string.Empty);
					cmd.AddParameter(currentDepartmentCode, SqlDbType.VarChar, GlbDepartment.CurrentDepartment?.GE_Code.ToString() ?? string.Empty);

					cmd.ExecuteNonQuery();

					var log = string.Format(CultureInfo.InvariantCulture,
						"{0} Event processed for Shipment(s): {1}",
						eventType, string.Join(", ", shipmentsToUpdate.Select(x => x.JS_UniqueConsignRef)));
					DefaultLogger.Log(LogType.Information, log);
				}
			}
		}

		bool IsEventOnHVLShipmentTransport(Transport transport, ForwardingShipment shipment, string eventType)
		{
			return shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue
				&& ((eventType == AutoEvents.DepartureCode && transport.PK == shipment.TransportsIncludingRelated.DepartureTransport?.PK)
					|| (eventType == AutoEvents.ArrivalCode && transport.PK == shipment.TransportsIncludingRelated.ArrivalTransport?.PK));
		}

		string[] GetStatusesToIgnore
		{
			get
			{
				if (statusesToIgnore == null)
				{
					statusesToIgnore = HVLVItemLookups.GetAllHVLVItemStatus()
						.Cast<CodeDescriptionPair>()
						.Select(x => x.Code)
						.SkipWhile(x => x != lastShipmentStatus)
						.Skip(1)
						.ToArray();
				}

				return statusesToIgnore;
			}
		}

		string[] statusesToIgnore;

		const string lastShipmentStatus = HVLVItemStatus.Codes.ShipmentArrived;
	}
}
