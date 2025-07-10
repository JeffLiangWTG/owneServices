using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickFaceCreateTransfers : IPickFaceCreateTransfers
	{
		public PickFaceCreateTransfers()
		{
		}

		#region PreTransferValidationVirtual

		void PreTransferValidation(WhsTransfer transfer) => PreTransferValidationCore(transfer);
		public virtual void PreTransferValidationCore(WhsTransfer transfer)
		{
		}

		#endregion

		#region IPickFaceCreateTransfers.CreateAndSaveTransfers

		public void CreateAndSaveTransfers(IEnumerable<IPickFaceInfo> pickFaceInfos, IWhsDocketCreationLogger logger, bool allowMultipleTransfersToReplenish = false)
		{
			Argument.NotNull(pickFaceInfos, nameof(pickFaceInfos));
			Argument.NotNull(logger, nameof(logger));

			var groupedPickFaceInfos = pickFaceInfos.GroupBy(info => info.WarehousePK);
			var newFactory = new BusinessObjectFactory();
			var warehouses = newFactory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, groupedPickFaceInfos.Select(group => group.Key)));

			var createdTransfers = new List<WhsTransfer>();
			foreach (var group in groupedPickFaceInfos)
			{
				var referenceWarehouse = warehouses.Single(whs => whs.PK == group.Key);
				using (WarehouseUserContextHelper.SetUserContextForWarehouse(referenceWarehouse))
				{
					var transfers = PickFaceReplenishmentManager.CreateTransfersForPickFaceReplenishment(group.ToArray(), allowMultipleTransfersToReplenish);
					if (transfers.Any())
					{
						foreach (var transfer in transfers)
						{
							ResolveErrorsAndSaveTransfer(logger, transfer);
						}

						createdTransfers.AddRange(transfers);
					}
				}
			}

			if (createdTransfers.Count == 0)
			{
				logger.LogWarning(Res.GetString("6a6e16e7-a947-4e4f-af48-92e15d0b4b6a", "No transfers have been generated to replenish any Pick Faces."));
				logger.LogWarning(Res.GetString("02b35778-bef0-47a2-8aed-770e816b7d75", "Two possible reasons for this are the Pick Faces don't need replenishment or there is no stock available to transfer."));
			}
		}

		/// <summary>
		/// When Replenishing Dynamic Pick Faces, we need to consider the Ordered Attributes of the Orders in shortfall.
		/// There can be Inventory that satisfies different combinations of Ordered Attributes that have competing priority.
		/// Since it would be too complex to write an SQL Algorithmn (Requires looping/recursion) to best pick the Inventory
		/// that satisfies the Orders most optimally, we instead have a simple algorithmn that will potentially over allocate
		/// a Single Inventory if the shortfall is greater than the stock in Inventory and multiple Combinations of Ordered
		/// Attributes require said Inventory. If this occurs we gracefully handle it by reducing the transfer line quantity
		/// to match what's actually in the Location. Subsequent runs of the Service Task will transfer more stock.
		/// </summary>
		void ResolveErrorsAndSaveTransfer(IWhsDocketCreationLogger logger, WhsTransfer transfer)
		{
			PreTransferValidation(transfer);

			if (transfer.HasErrors)
			{
				var errorResolved = false;

				var linesThatCannotBeFulfilled = transfer.Lines.Cast<WhsTransferLine>().Where(l => l.QtyCommittedIncludingMatchingLines < l.QtyToMoveIncludingMatchingLines).ToArray();
				if (linesThatCannotBeFulfilled.Length > 0)
				{
					foreach (var line in linesThatCannotBeFulfilled)
					{
						DeallocateStockAndWarnUser(logger, transfer, line);
					}

					transfer.RunPreSaveValidation();
					errorResolved = !transfer.HasErrors;
				}

				if (errorResolved)
				{
					SaveTransferAndHandleErrors(transfer, logger);
				}
				else
				{
					var errorMessagePart = Res.GetString("a016f931-9220-4be9-b1ff-f8c07ba22759", "Validation errors:\r\n{0}", transfer.GetErrors().ToUniqueMessageListString());
					logger.LogFailure(ConstructErrorMessage(transfer, errorMessagePart));
				}
			}
			else
			{
				SaveTransferAndHandleErrors(transfer, logger);
			}
		}

		static void DeallocateStockAndWarnUser(IWhsDocketCreationLogger logger, WhsTransfer transfer, WhsTransferLine line)
		{
			var prefix = Res.GetString("13ed1dcd-a8ac-42c4-a845-93dd155957c9", "Attempted to replenish Pick Face: {0} with {1} Product: {2} for Client: {3} from Warehouse: {4} Location: {5},",
				line.LocationString, line.WE_TransactionQuantity.ToStringTrimZeros(), line.SupplierPart.OP_PartNum, transfer.Client.OH_Code, line.Warehouse.WW_WarehouseNameMultilingual, line.TransferFromLocationString);

			string warningMessage;

			var qtyCommitted = line.QtyCommittedIncludingMatchingLines;
			if (qtyCommitted == 0)
			{
				warningMessage = Res.GetString("523616d2-752b-4718-9cc5-c8a2772cc06d", "{0} but the location did not contain any inventory.", prefix);
				line.Delete();
			}
			else
			{
				warningMessage = Res.GetString("2ee755bf-240f-466b-b0e0-5f7b5b93cc75", "{0} but the location did not contain enough inventory. Replenishing {1} units instead.", prefix, qtyCommitted.ToStringTrimZeros());
				line.QtyToMoveIncludingMatchingLines = qtyCommitted;
			}

			logger.LogWarning(warningMessage);
		}

		#endregion

		#region SaveTransferAndHandleErrors

		void SaveTransferAndHandleErrors(WhsTransfer transfer, IWhsDocketCreationLogger logger)
		{
			try
			{
				transfer.Factory.Save();
				CreateTransferLineSuccessLogs(transfer, logger);
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException)
			{
				var errorMessage = ex.Message;
				if (ex.IsPreventOverCommitStockViaPickLineTriggerException())
				{
					errorMessage = WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForServiceTask;
				}
				else if (ex.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException())
				{
					errorMessage = WhsExceptionHandler.WhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectMsg_ForServiceTask;
				}

				logger.LogFailure(ConstructErrorMessage(transfer, Res.GetString("f3b6eb71-1af9-4a47-803b-8feb7305fedc", "Save errors: {0}", errorMessage)));
			}
		}

		#endregion

		#region CreateTransferLineSuccessLogs

		void CreateTransferLineSuccessLogs(WhsTransfer replenishmentTransfer, IWhsDocketCreationLogger logger)
		{
			var message = Res.GetString("47770B9E-A733-41E9-BF7E-A866C1B335A8", "Transfer {0}: was created successfully.", "{0}");
			logger.LogHyperLinkSuccess(replenishmentTransfer, message);

			foreach (WhsTransferLine transferLine in replenishmentTransfer.Lines)
			{
				logger.LogSuccess(
					Res.GetString("225aa4d6-dea6-4853-a62c-b2ba9527643e",
					"Pick Face: {0} is to be replenished with {1} Product: {2} for Client: {3} from Warehouse: {4} Location: {5}.",
					transferLine.LocationString,
					transferLine.QtyCommittedIncludingMatchingLines.ToStringTrimZeros(),
					transferLine.SupplierPart.OP_PartNum,
					replenishmentTransfer.Client.OH_Code,
					transferLine.Warehouse.WW_WarehouseNameMultilingual,
					transferLine.TransferFromLocationString)
				);
			}
		}

		#endregion

		#region ConstructErrorMessage

		static ZString ConstructErrorMessage(WhsTransfer transfer, string errorToAppend)
		{
			var clientName = transfer.ClientName;
			var warehouseName = transfer.Warehouse.WW_WarehouseNameMultilingual;
			var line = transfer.Lines.FirstOrDefault();

			return Res.GetString("e078afb6-967d-4c3d-930d-b98f79a27962", "Creating a transfer for Client: {0}, Warehouse: {1}, Product: {2}, Pick-face location {3} failed.\r\n{4}",
					clientName, warehouseName, line?.SupplierPart?.OP_PartNum ?? ZString.Empty, line?.LocationString ?? ZString.Empty, errorToAppend);
		}

		#endregion
	}
}
