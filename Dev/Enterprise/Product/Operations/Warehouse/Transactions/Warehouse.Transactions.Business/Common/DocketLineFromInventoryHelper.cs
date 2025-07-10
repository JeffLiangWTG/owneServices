using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class DocketLineFromInventoryHelper<T>
		where T : WhsDocketLine
	{
		protected DocketLineFromInventoryHelper(INotifications notifications, WhsDocket docket)
		{
			NotificationSubscriber = Argument.NotNull(notifications, nameof(notifications));
			Docket = Argument.NotNull(docket, nameof(docket));
		}

		INotifications NotificationSubscriber { get; }
		protected WhsDocket Docket { get; }

		public void AcceptInventoryLinesFromSearchGrid(WhsDocketLineCollection docketLines, BusinessObject[] inventory)
		{
			if (inventory == null)
			{
				throw new ArgumentNullException(nameof(inventory));
			}

			if (((IBindingList)docketLines).AllowNew)
			{
				AcceptInventoryLinesFromSearchGridCore(docketLines, inventory);
			}
			else
			{
				NotificationSubscriber.Notify(new ErrorNotification(WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew));
			}
		}

		void AcceptInventoryLinesFromSearchGridCore(WhsDocketLineCollection docketLines, BusinessObject[] inventoryList)
		{
			var factory = inventoryList.Length > 0 ? inventoryList[0].Factory : null;

			using (SuspendProcessWhileAcceptingInventoryLines())
			using (factory != null ? ActiveBusinessObjectCollection.DelayListChangedEvents(factory) : null)
			{
				var createdLines = new List<WhsDocketLine>(inventoryList.Length);
				var type = ((IBusinessObjectCollection)docketLines).TypeOfElements;

				foreach (var line in inventoryList)
				{
					var inventory = line as WhsInventoryView;

					if (inventory != null)
					{
						var docketLine = (T)inventory.Factory.New(type);
						SetDocketLineFromInventory(docketLine, inventory);
						createdLines.Add(docketLine);
					}
				}

				docketLines.AddRange(createdLines);

				// Validation is suspended when creating the lines so that it doesn't error when there is no docket
				// attached yet. After the lines are attached to the Docket, we run the Validation that was missed.
				foreach (var line in createdLines)
				{
					if (!line.IsValidationSuspended)
					{
						line.Validation.ValidateWE_OP();
					}
				}
			}
		}

		protected virtual IDisposable SuspendProcessWhileAcceptingInventoryLines() => null;

		public WhsDocketLine CreateDocketLineFromInventory(WhsDocketLineCollection docketLines, WhsInventoryView inventory)
		{
			var docketLine = (T)docketLines.AddNew();
			SetDocketLineFromInventory(docketLine, inventory.InDocketLine, ExcludeFromCopy.None);

			return docketLine;
		}

		void SetDocketLineFromInventory(T transactionLine, WhsInventoryView inventory)
		{
			SetDocketLineFromInventoryWithProductValidationSuspended(transactionLine, inventory.InDocketLine, ExcludeFromCopy.None);
		}

		public void SetDocketLineFromInventory(T transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			SetDocketLineFromInventoryWithProductValidationSuspended(transactionLine, inventoryLine, exclude);

			if (!transactionLine.IsValidationSuspended)
			{
				transactionLine.Validation.ValidateWE_OP();
			}
		}

		void SetDocketLineFromInventoryWithProductValidationSuspended(T transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			Argument.NotNull(inventoryLine, nameof(inventoryLine));

			if (!inventoryLine.IsInventoryLine)
			{
				throw new ArgumentException("You should never pass in a Docket Line which is not a Inventory Line.");
			}

			SetDocketLineFromInventoryCore(transactionLine, inventoryLine, exclude);
		}

		ICopyCustomsStrategy CopyCustomsStrategy => copyCustomsStrategy ?? (copyCustomsStrategy = CreateCopyCustomsStrategyCore());
		ICopyCustomsStrategy copyCustomsStrategy;

		protected virtual ICopyCustomsStrategy CreateCopyCustomsStrategyCore() => new DefaultCopyCustomsStrategy();

		protected virtual void SetDocketLineFromInventoryCore(T transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			using (transactionLine.GetValidationSuspender())
			{
				transactionLine.WE_OP = inventoryLine.WE_OP;
			}

			if (!exclude.HasFlag(ExcludeFromCopy.PackType))
			{
				transactionLine.WE_F3_NKPackType = inventoryLine.WE_F3_NKPackType;
			}

			SetDocketLineFromInventoryBeforeSettingAdjustmentArrivalDate(transactionLine, inventoryLine, exclude);
			SetLocationDataFromInventory(transactionLine, inventoryLine);

			transactionLine.SetAttributes(inventoryLine);

			if (!exclude.HasFlag(ExcludeFromCopy.CustomAttribs))
			{
				transactionLine.SetCustomAttributes(inventoryLine);
			}

			if (Docket.IsCustomsTransaction)
			{
				CopyCustomsStrategy.CopyCustomsData(transactionLine, inventoryLine);
			}

			using (transactionLine.GetValidationSuspender())
			{
				transactionLine.WE_PackageGroupId = inventoryLine.WE_PackageGroupId;
				transactionLine.WE_PerPackageQty = inventoryLine.WE_PerPackageQty;
			}
		}

		protected virtual void SetDocketLineFromInventoryBeforeSettingAdjustmentArrivalDate(T transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			// In WE_AdjustmentArrivalDate validation we do AdjustmentArrivalDate validation if WE_TransactionQuantity is Zero or positive.
			// Therefore in adjustments we use this method to set WE_TransactionQuantity prior to AdjustmentArrivalDate
		}

		protected virtual void SetLocationDataFromInventory(T transactionLine, WhsDocketLine inventoryLine)
		{
			transactionLine.WE_WL = inventoryLine.WE_WL;
			transactionLine.WE_PalletID = inventoryLine.WE_PalletID;
		}
	}
}
