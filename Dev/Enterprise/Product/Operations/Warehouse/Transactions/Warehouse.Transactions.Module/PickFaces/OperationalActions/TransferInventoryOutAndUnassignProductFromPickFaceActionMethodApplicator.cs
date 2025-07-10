using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("EF24615B-6626-42E7-ADE5-1210A7836B18", "Transfer Inventory Out And Un-assign Product(s) From Pick Face"), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var pickFaces = targets.Cast<WhsPickFaceView>();
			var unpickedIncomingTransferLines = new List<WhsTransferLine>();

			AddDBHints(pickFaces);

			var validPickFaces = ValidatePickFaces(log, pickFaces, unpickedIncomingTransferLines);
			if (validPickFaces.Any())
			{
				if (TransferOut(log, validPickFaces))
				{
					Unassign(log, validPickFaces);
					DeleteUnpickedIncomingTransferLines(log, unpickedIncomingTransferLines);
					Factory.Save();
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("C069F7AC-F02B-4E09-A8BB-471D146C5EB3", "No transfer has been generated and no products have been un-assigned."));
			}
		}

		void AddDBHints(IEnumerable<WhsPickFaceView> pickFaces)
		{
			foreach (var pickFace in pickFaces)
			{
				Factory.AddFetchHint(WhsLocationViewSchema.PK, pickFace.WPV_WL);    // Get Location to display in the log message
				if (pickFace.WPV_OP.IsValid)
				{
					Factory.AddFetchHint(OrgSupplierPartSchema.PK, pickFace.WPV_OP);    // Get SupplierPart to display in the log message
				}

				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, pickFace.WPV_WF);   // Unassign log message

				var queryStmUniversalCopy = new ZQuery(StmUniversalCopySchema.SUC_CopyObjectTableCode, "WF");
				queryStmUniversalCopy.AddToFilter(StmUniversalCopySchema.SUC_CopyObjectId, pickFace.WPV_WF);
				Factory.AddFetchHint(StmUniversalCopySchema.Instance, queryStmUniversalCopy);   // Unassign log message				
			}

			var queryStmDocDataOverride = new ZQuery(StmDocDataOverrideSchema.DD_ParentID, pickFaces.Select(p => p.WPV_WF));
			queryStmDocDataOverride.AddToFilter(JoinCondition.Or, StmDocDataOverrideSchema.DD_ParentRelatedID, pickFaces.Select(p => p.WPV_WF));
			Factory.AddFetchHint(StmDocDataOverrideSchema.Instance, queryStmDocDataOverride);   // Unassign log message
		}

		#region Validation

		IEnumerable<WhsPickFaceView> ValidatePickFaces(IOperationalActionSectionLog log, IEnumerable<WhsPickFaceView> pickFaces, List<WhsTransferLine> unpickedIncomingTransferLines)
		{
			var isClientAndWarehouseSame = true;
			var validPickFaces = new List<WhsPickFaceView>();
			var unFinalisedDocketLines = GetUnFinalisedDocketLinesForPickFaces(pickFaces);
			foreach (var pickFace in pickFaces)
			{
				var isValid = true;
				if (!pickFace.IsPickFaceAssigned)
				{
					isValid = false;
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("2C118990-8240-4F14-946B-B1608956D4C9", "Pick face location: {0} has no product assigned, transfer out and un-assign has been ignored."), pickFace.Location.ToLocationString());
				}
				else
				{
					var errorMessage = new ZStringBuilder();
					if (pickFace.WPV_Committed > 0)
					{
						isValid = false;
						errorMessage.Append(Res.GetString("8CA0D42A-DFD6-48B8-BDC4-026066B7F066", "Pick face has committed inventories."));
					}
					if (pickFace.WPV_Incoming > 0 && HasPickedUnfinalisedTransferlines(pickFace, unFinalisedDocketLines, unpickedIncomingTransferLines))
					{
						isValid = false;
						errorMessage.Append(Res.GetString("73AF4D4C-45AF-4B6E-9C92-E27E7255D34F", "Pick face has picked but un-finalized incoming transfers."));
					}
					if (unFinalisedDocketLines.Any(l => l.WE_DocketLineType == DocketType.Codes.Receive && l.WE_OP == pickFace.WPV_OP && l.WE_WL == pickFace.WPV_WL))
					{
						isValid = false;
						errorMessage.Append(Res.GetString("929D5FEA-938B-4934-959A-079D2EE4F3C1", "Pick face has un-finalized receives."));
					}
					if (unFinalisedDocketLines.Any(l => l.WE_DocketLineType == DocketType.Codes.Adjustment && l.WE_OP == pickFace.WPV_OP && l.WE_WL == pickFace.WPV_WL))
					{
						isValid = false;
						errorMessage.Append(Res.GetString("427EFC16-6F2F-4646-925F-228350550FD1", "Pick face has un-finalized adjustments."));
					}

					if (!isValid)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("D8727EF7-DEDC-4AB0-883D-554A327F59FA", "Unable to transfer out and un-assign the product: {0} from pick face location: {1}. Reason:\r\n{2}"), pickFace.SupplierPart.OP_PartNum, pickFace.Location.ToLocationString(), errorMessage.ToStringWithNewLineBetweenAppends());
					}
				}

				if (isValid)
				{
					validPickFaces.Add(pickFace);
				}
			}

			if (validPickFaces.Count > 1)
			{
				isClientAndWarehouseSame = CheckClientAndWarehouse(log, validPickFaces);
			}

			return isClientAndWarehouseSame ? validPickFaces : Enumerable.Empty<WhsPickFaceView>();
		}

		bool HasPickedUnfinalisedTransferlines(WhsPickFaceView pickFace, IEnumerable<WhsDocketLine> unFinalisedDocketLines, List<WhsTransferLine> unpickedIncomingTransferLines)
		{
			var transferLines = unFinalisedDocketLines.Where(l => l.WE_DocketLineType == DocketType.Codes.Transfer && l.WE_OP == pickFace.WPV_OP && l.WE_WL == pickFace.WPV_WL).Cast<WhsTransferLine>().ToArray();
			if (!transferLines.Any(l => l.PickLines.Any(p => p.WZ_GS_NKAssignedTo != ZString.Empty)))
			{
				unpickedIncomingTransferLines.AddRange(transferLines);
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		IEnumerable<WhsDocketLine> GetUnFinalisedDocketLinesForPickFaces(IEnumerable<WhsPickFaceView> pickFaces)
		{
			var docketlineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
			docketlineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, new[] { DocketType.Codes.Adjustment, DocketType.Codes.Receive, DocketType.Codes.Transfer });
			docketlineQuery.AddToFilter(WhsDocketLineSchema.WE_TransactionQuantity, SQLComparisonOperator.GreaterThanOrEqualTo, 0m);
			docketlineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);
			var pickFacesQuery = new ZQuery();
			foreach (var pickface in pickFaces)
			{
				var pickFaceOrQuery = new ZQuery();
				pickFaceOrQuery.AddToFilter(WhsDocketLineSchema.WE_OP, pickface.WPV_OP);
				pickFaceOrQuery.AddToFilter(WhsDocketLineSchema.WE_WL, pickface.WPV_WL);
				pickFacesQuery.AddToFilter(pickFaceOrQuery, JoinCondition.Or);
			}
			docketlineQuery.AddToFilter(pickFacesQuery);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, pickFaces.First().WPV_OH);
			docketlineQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			return Factory.Load<WhsDocketLine>(docketlineQuery);
		}

		#endregion

		#region CheckClientAndWarehouse

		bool CheckClientAndWarehouse(IOperationalActionSectionLog log, IEnumerable<WhsPickFaceView> pickFaces)
		{
			var isValid = true;
			var clientPK = pickFaces.SameOrDefault(p => p.WPV_OH);
			if (clientPK.IsDefault)
			{
				isValid = false;
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("AF4844A0-E558-4B17-A7C0-CB964627668F", "Unable to transfer out and un-assign the products from pick faces when the selection contains multiple clients."));
			}

			var warehousePK = pickFaces.SameOrDefault(p => p.WPV_WW_Whs);
			if (warehousePK.IsDefault)
			{
				isValid = false;
				log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("55E780A3-C382-43AE-BAF3-3D634BF55998", "Unable to transfer out and un-assign the products from pick faces when the selection contains multiple warehouses."));
			}

			return isValid;
		}

		#endregion

		#region Unassign

		void Unassign(IOperationalActionSectionLog log, IEnumerable<WhsPickFaceView> pickFaces)
		{
			var pickFacesToDelete = Factory.Load<WhsPickFace>(new ZQuery(WhsPickFaceSchema.PK, pickFaces.Select(p => p.WPV_WF)));
			foreach (var pickFace in pickFaces)
			{
				var pickFaceToDelete = pickFacesToDelete.Single(p => p.PK == pickFace.WPV_WF);
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("25BFC745-2491-472A-8CBB-22ED93587831", "Product: {0} has been un-assigned from location: {1}.", pickFaceToDelete.SupplierPart.OP_PartNum, pickFaceToDelete.LocationString));
				pickFaceToDelete.Delete();
			}
			log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("3432CDEE-F0B0-45B5-B6B1-250FD4A6E5ED", "Un-assign all products from pick faces successfully."));
		}

		#endregion

		#region TransferOut

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		bool TransferOut(IOperationalActionSectionLog log, IEnumerable<WhsPickFaceView> pickFaces)
		{
			var success = true;
			var clientPK = pickFaces.First().WPV_OH;
			var warehousePK = pickFaces.First().WPV_WW_Whs;
			var inventoriesForTransferOut = Array.Empty<WhsInventoryView>();
			var invalidPickFaces = pickFaces.Where(p => p.WPV_TotalQuantity > 0);
			if (invalidPickFaces.Any())
			{
				var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, clientPK);
				query.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, new string[] { InventoryStatus.Codes.Available, InventoryStatus.Codes.Held });
				query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
				var pickFacesQuery = new ZQuery();
				foreach (var pickface in invalidPickFaces)
				{
					var pickFaceOrQuery = new ZQuery();
					pickFaceOrQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, pickface.WPV_OP);
					pickFaceOrQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, pickface.WPV_WL);
					pickFacesQuery.AddToFilter(pickFaceOrQuery, JoinCondition.Or);
				}
				query.AddToFilter(pickFacesQuery);
				var subQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
				subQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPK);
				subQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehousePK);
				query.AddSubQuery(subQuery, JoinCondition.And);
				inventoriesForTransferOut = Factory.Load<WhsInventoryView>(query);
			}

			Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventoriesForTransferOut.Select(inv => inv.WI_WE_InDocketLine)));
			var availableInventories = inventoriesForTransferOut.Where(i => i.WI_AvailableToTransferQuantity > 0);
			if (availableInventories.Any())
			{
				var transfer = WhsTransfer.New(Factory);
				transfer.WD_OH_Client = clientPK;
				transfer.WD_WW_Whs = warehousePK;
				foreach (var inventory in availableInventories)
				{
					transfer.CreateDocketLineFromInventory(inventory);
				}
				transfer.RunPreSaveValidation();
				SetupTransferForTest(transfer);
				if (!transfer.HasErrors)
				{
					Factory.Save();
					var transferLink = GetDocketIdLink(transfer);
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("7A7860DC-259D-49C4-A636-90714A280A3F", "Transfer {0} has been generated for selected pick faces.", "{0}"), transferLink);
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("6754B440-8565-4BFD-801C-97BBA77E9AA1", "Could not generate transfer for selected pick faces:\r\n{0}", transfer.GetErrors().ToUniqueMessageListString()));
					success = false;
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("C0A20A00-38B5-4CF5-9388-CD68806E659C", "No transfer has been generated because there is no inventory on selected pick faces."));
			}
			return success;
		}

		partial void SetupTransferForTest(WhsTransfer transfer);

		#endregion

		#region DeleteUnpickedIncomingTransferLines

		void DeleteUnpickedIncomingTransferLines(IOperationalActionSectionLog log, List<WhsTransferLine> unpickedIncomingTransferLines)
		{
			foreach (var line in unpickedIncomingTransferLines)
			{
				var transferLink = GetDocketIdLink(line.Docket);
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("71DF8EC5-434A-497D-B347-540AD2D2B14B", "Delete un-picked incoming transfer line [Product:{0}, Quantity:{1}, From location: {2} to location: {3}] on transfer {4}.", line.ProductCode, line.WE_TransactionQuantity, line.TransferFromLocationString, line.LocationString, "{0}"), transferLink);
				line.Delete();
			}
			if (unpickedIncomingTransferLines.Count > 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("788A2BF7-A8FA-468A-BC7F-AA0B659DA273", "Delete all un-picked incoming transfer lines successfully."));
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator
	{
		partial void SetupTransferForTest(WhsTransfer transfer)
		{
			AddErrorsToTransferForTest?.Invoke(transfer);
		}

		public Action<WhsTransfer> AddErrorsToTransferForTest;
	}
}

#endif
#endregion
