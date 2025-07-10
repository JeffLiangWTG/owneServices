using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public partial class AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager : IAllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager
	{
		public AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager(IPartiallyReplenishedPickSplitter pickSplitter)
		{
			PickSplitter = Argument.NotNull(pickSplitter, nameof(pickSplitter));
		}

		IPartiallyReplenishedPickSplitter PickSplitter { get; }

		#region AllocateAwaitingReplenishmentPicks

		public void AllocateAwaitingReplenishmentPicks(ILogger logger, CancellationToken token)
		{
			Argument.NotNull(logger, nameof(logger));

			var newFactory = new BusinessObjectFactory();
			var awaitingReplenishementPicks = GetAwaitingReplenishmentPicks(newFactory);
			if (awaitingReplenishementPicks.Length > 0)
			{
				var successLogs = new ZStringBuilder();
				var failureLogs = new ZStringBuilder();
				var taskRunDate = ZDateTimeOffset.UtcNow;

				var awaitingReplenishmentPicksGroupedByWarehouse = awaitingReplenishementPicks.GroupBy(pick => pick.WP_WW_Whs);
				foreach (var groupedPicks in awaitingReplenishmentPicksGroupedByWarehouse)
				{
					using (WarehouseUserContextHelper.SetUserContextForWarehouse(groupedPicks.First().Warehouse))
					{
						AllocateAwaitingReplenishmentPicksCore(newFactory, groupedPicks.ToArray(), successLogs, failureLogs, taskRunDate, token);
					}
				}

				if (!successLogs.IsEmpty)
				{
					logger.Information(successLogs.ToStringWithNewLineBetweenAppends());
				}
				if (!failureLogs.IsEmpty)
				{
					logger.Error(failureLogs.ToStringWithNewLineBetweenAppends());
				}
			}
			else
			{
				logger.Information(Res.GetString("17a7919c-6108-4dcc-946e-f6db76cc6ef3", "Did not find any picks with Waiting Replenishment status."));
			}
		}

		void AllocateAwaitingReplenishmentPicksCore(BusinessObjectFactory newFactory, WhsPick[] awaitingReplenishementPicks, ZStringBuilder successLogs, ZStringBuilder failureLogs, ZDateTimeOffset taskRunDate, CancellationToken token)
		{
			AddFetchHintsForPickPriorityCalculation(newFactory, awaitingReplenishementPicks);
			var sortedAwaitingReplenishmentPicks = awaitingReplenishementPicks.OrderBy(p => p.PickPriority, new PickPriorityComparer());
			DeallocatePicksIfNecessary(sortedAwaitingReplenishmentPicks, newFactory, successLogs, failureLogs);

			foreach (var pick in sortedAwaitingReplenishmentPicks)
			{
				token.ThrowIfCancellationRequested();
				AutoAllocatePick(pick.PK, successLogs, failureLogs, taskRunDate);
			}
		}

		static void AddFetchHintsForPickPriorityCalculation(BusinessObjectFactory newFactory, WhsPick[] awaitingReplenishementPicks)
		{
			foreach (var pick in awaitingReplenishementPicks)
			{
				var query = new ZQuery(WhsDocketSchema.WD_DocketType, new[] { "DWO", "ORD", "WOR" });
				query.AddToFilter(WhsDocketSchema.WD_WP, pick.PK);
				newFactory.AddFetchHint(WhsDocketSchema.Instance, query);
			}
		}

		static WhsPick[] GetAwaitingReplenishmentPicks(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsPickSchema.WP_IsAwaitingReplenishment, true);
			query.OrderBy = WhsPickSchema.WP_PickNo.Name;
			var pickCollection = factory.Load<WhsPick>(query);

			return pickCollection;
		}

		void DeallocatePicksIfNecessary(IOrderedEnumerable<WhsPick> sortedAwaitingReplenishmentPicks, BusinessObjectFactory newFactory, ZStringBuilder successLogs, ZStringBuilder failureLogs)
		{
			if (WarehouseDataRegistry.Instance.DeallocateLowerPriorityPicksWhenAllocatingWaitingReplenishmentPicks.Value)
			{
				sortedAwaitingReplenishmentPicks.Skip(1).ForEach(pick => pick.ClearAllocatedItems());

				if (sortedAwaitingReplenishmentPicks.Any(pick => pick.HasChanges))
				{
					CreateDataForDBLevelErrorTesting(newFactory);
					SaveAndHandleErrors(
						newFactory,
						failureLogs,
						() => successLogs.Append(Res.GetString("7f409eae-4e35-4fe1-9551-44bd7478cd31", "Allocated stock has been successfully deallocated from lower priority picks awaiting replenishment.")),
						Res.GetString("2b86e6a6-1768-4c29-97fb-783b2314e485", "Stock could not be deallocated from lower priority picks before attempting to allocate picks. Someone else has changed the pick."),
						Res.GetString("6b5a47a1-2f75-4939-93bf-ece24a7346c3", "Stock could not be deallocated."));
				}
			}
		}

		void AutoAllocatePick(ZGuid pickPK, ZStringBuilder successLogs, ZStringBuilder failureLogs, ZDateTimeOffset taskRunDate)
		{
			var newFactory = new BusinessObjectFactory();

			var pickInNewFactory = newFactory.Load<WhsPick>(pickPK);
			var notifications = new NotificationBuffer();

			pickInNewFactory.AutoAllocateItems(taskRunDate, notifications: notifications);

			if (notifications.Events.Length > 0)
			{
				successLogs.Append(Res.GetString("e1b77522-f203-4669-a7eb-2c9f173e6003", "Issue occurred during allocation of Pick No: {0}: {1}", pickInNewFactory.WP_PickNo, notifications.AsString));
			}

			if (pickInNewFactory.HasChanges)
			{
				var pickFromSplit = PickSplitter.SplitOrdersFromPartiallyReplenishedPick(pickInNewFactory);
				AddFetchHints(pickInNewFactory);
				pickInNewFactory.RunPreSaveValidationWithFetchHints();
				if (pickInNewFactory.HasErrors())
				{
					failureLogs.Append(Res.GetString("4999f439-eb28-4bbe-b7c8-42ef64bd1c32", "Pick No: {0} could not be allocated due to following errors:", pickInNewFactory.WP_PickNo));
					failureLogs.Append(pickInNewFactory.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString());
				}
				else
				{
					CreateDataForDBLevelErrorTesting(newFactory);
					if (SaveAndHandleErrors(
							newFactory,
							failureLogs,
							() =>
							{
								successLogs.Append(Res.GetString("9c4812c0-1c8c-47a3-9359-9e08b19790cf", "Pick No: {0} has successfully allocated stock.", pickInNewFactory.WP_PickNo));
								if (pickFromSplit != null)
								{
									successLogs.Append(Res.GetString("b758e632-5a73-468f-a346-0f0894dd7cff", "Pick No: {0} has successfully been split with replenished orders being added to Pick No: {1}.", pickInNewFactory.WP_PickNo, pickFromSplit.WP_PickNo));
								}
							},
							Res.GetString("4027ddde-8bd5-4971-83c1-86f4ac873472", "Pick No: {0} could not be allocated. While service task was allocating inventory someone else has changed Pick or Order(s). This pick would be allocated when service task run next time.", pickInNewFactory.WP_PickNo),
							Res.GetString("ff3fd662-7ddd-4af4-8426-8fa648e1f49a", "Pick No: {0} could not be allocated.", pickInNewFactory.WP_PickNo))
						&& !pickInNewFactory.WP_IsAwaitingReplenishment)
					{
						AllocatePackageLabels(pickInNewFactory, failureLogs);
					}
				}
			}
			else
			{
				successLogs.Append(Res.GetString("07f91215-88e0-414b-ada9-c07be544d264", "Pick No: {0} did not find existing stock that could be allocated.", pickInNewFactory.WP_PickNo));
			}
		}

		static bool SaveAndHandleErrors(
			BusinessObjectFactory newFactory,
			ZStringBuilder failureLogs,
			Action successAction,
			string concurrencyErrorMessage,
			string genericSaveFailureMessage)
		{
			var isSaveSuccessful = true;

			try
			{
				newFactory.Save();
				successAction();
			}
			catch (Exception ex) when (ex is ZConcurrencyCheckFailureException || ex is ZSaveConcurrencyException)
			{
				isSaveSuccessful = false;
				if (ex.IsPreventOverCommitStockViaPickLineTriggerException())
				{
					var errorMessage = WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForServiceTask;
					failureLogs.Append(genericSaveFailureMessage + " " + errorMessage);
				}
				else
				{
					failureLogs.Append(concurrencyErrorMessage);
				}
			}
			catch (ZCannotSaveException ex)
			{
				isSaveSuccessful = false;
				failureLogs.Append(genericSaveFailureMessage + " " + ex.Message);
			}

			return isSaveSuccessful;
		}

		static void AddFetchHints(WhsPick pickInNewFactory)
		{
			var newFactory = pickInNewFactory.Factory;
			foreach (var order in pickInNewFactory.Orders.Cast<WhsPickableDocket>())
			{
				newFactory.AddFetchHint(typeof(WhsDocketContainer), WhsDocketContainerSchema.WC_WD, order.PK);
				newFactory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, order.PK);
				newFactory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				newFactory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, order.PK);

				var query = new ZQuery();
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, order.WD_OH_Client);
				query.AddToFilter(WhsDocketSchema.WD_DocketType, order.WD_DocketType);
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, order.WD_ExternalReference);
				query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, order.WD_ExternalReferenceSplit);
				newFactory.AddFetchHint(typeof(WhsPickableDocket), query);
			}
		}

		void AllocatePackageLabels(WhsPick pick, ZStringBuilder failureLogs)
		{
			pick.AllocatePackageLabels();
			if (!pick.WP_IsCartonised)
			{
				var notifications = (NotificationBuffer)pick.NotificationSubscriber;
				if (notifications != null && notifications.HasErrors)
				{
					var errorMessage = Res.GetString("FF307766-70D3-4BCC-9737-773AD2DD3DE0", "Pick {0}: {1}", pick.WP_PickNo, notifications.Events.GetFirstMessage());
					failureLogs.Append(errorMessage);
				}
			}
		}

		partial void CreateDataForDBLevelErrorTesting(BusinessObjectFactory factory);

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	partial class AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManager
	{
		partial void CreateDataForDBLevelErrorTesting(BusinessObjectFactory factory)
		{
			OnNewFactorySaving?.Invoke(factory);
		}

		public BusinessObjectFactory.SavingEventHandler OnNewFactorySaving;
	}
}

#endif
#endregion
