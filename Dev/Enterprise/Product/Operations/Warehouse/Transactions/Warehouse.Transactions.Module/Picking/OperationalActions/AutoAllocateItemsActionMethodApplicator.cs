using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AutoAllocateItemsActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public AutoAllocateItemsActionMethodApplicator()
			: base(Res.GetString("25ee09c3-e7d1-4f56-a542-7ff21630699f", "Auto Allocate Remaining Items")) // text used for logging)
		{
		}

		public AutoAllocateItemsActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("25ee09c3-e7d1-4f56-a542-7ff21630699f", "Auto Allocate Remaining Items"), factory)
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Pick 123 [pick hyperlink] - Message Text.
		const string OutputTextFormat_WarningReturned = "{0} {1} - {2}\r\n{3}\r\n";

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] businessObjectArray)
		{
			var pickFactory = businessObjectArray[0].Factory;
			log.SetSectionProgressMax(businessObjectArray.Length);

			var pickPKs = businessObjectArray.Select(pick => pick.PK);
			pickFactory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.WD_WP, pickPKs)); // Fetch hints for OrderBy operation.

			var sortedPicks = businessObjectArray.Cast<WhsPick>().OrderBy(p => p, new PickPriorityDateComparer()).ToArray();
			AddFetchHints(sortedPicks, pickFactory);
			foreach (var pick in sortedPicks)
			{
				TryToAutoAllocate(pick, log);
				log.BumpSectionProgress();
			}
		}

		static void TryToAutoAllocate(WhsPick pick, IOperationalActionSectionLog log)
		{
			if (pick.WP_PickStatus == PickStatus.Codes.Building)
			{
				var notifications = new NotificationBuffer();
				var result = pick.AutoAllocateItems(notifications, shouldAddFetchHints: false);

				if (string.IsNullOrEmpty(result))
				{
					LogInfo(log, pick, SuccessMessage);
				}
				else
				{
					LogWarningReturned(log, pick, WarningsReturnedMessage, result);
				}

				if (notifications.Events.Length > 0)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("bdefb489-3195-498b-8c86-b971d1a0ebdf", "{0}: Issue occurred during allocation: {1}", pick.HumanReadableName, notifications.AsString));
				}
			}
			else
			{
				LogWarning(log, pick, InvalidStatusMessage);
			}
		}

		static void AddFetchHints(IEnumerable<WhsPick> picks, BusinessObjectFactory pickFactory)
		{
			var buildingPicks = picks.Where(p => p.WP_PickStatus == PickStatus.Codes.Building).ToArray();

			foreach (var pick in buildingPicks)
			{
				pickFactory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, pick.Orders.Cast<WhsPickableDocket>().SelectMany(p => p.GetLinesToPick()).Select(l => l.PK)));
				pickFactory.AddFetchHint(JobDocAddressSchema.Instance, new ZQuery(JobDocAddressSchema.E2_ParentID, pick.Orders.Select(o => o.PK)));
			}

			foreach (var pick in buildingPicks)
			{
				pick.AddFetchHintsForAutoAllocateItems(); // After all the WhsPickLine hints above are added.
			}
		}

		static void LogWarningReturned(IOperationalActionSectionLog log, WhsPick pick, string messageIntro, string messageReturned)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
				OutputTextFormat_WarningReturned,
				pick.HumanReadableName,
				GetPickNoLink(pick),
				messageIntro,
				messageReturned);
		}

		static void LogWarning(IOperationalActionSectionLog log, WhsPick pick, string message)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
				OutputTextFormat,
				pick.HumanReadableName,
				GetPickNoLink(pick),
				message);
		}

		static void LogInfo(IOperationalActionSectionLog log, WhsPick pick, string message)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
				OutputTextFormat,
				pick.HumanReadableName,
				GetPickNoLink(pick),
				message);
		}

		static string InvalidStatusMessage => Res.GetString("04a3c190-f4a2-4d20-962f-a65e8caf0ec1", "cannot be processed because it does not have status {0}.", PickStatus.Descriptions.Building);
		static string SuccessMessage => Res.GetString("1c42d660-5352-4a2c-a627-39d017ebcd34", "successfully Auto Allocated Items.");
		static string WarningsReturnedMessage => Res.GetString("057c6d09-b4c5-42dd-81d7-194c0ee1ee6e", "had the following message while trying to Auto Allocate Items:");

		#region Comparer

		class PickPriorityDateComparer : Comparer<WhsPick>
		{
			// Compares by Priority, DateRequired, and SystemDate..
			public override int Compare(WhsPick x, WhsPick y)
			{
				var priorityResult = PriorityComparer.Compare(x.PickPriority, y.PickPriority);
				if (priorityResult != 0)
				{
					return priorityResult;
				}
				else if (x.EarliestRequiredDate.CompareTo(y.EarliestRequiredDate) != 0)
				{
					return x.EarliestRequiredDate.CompareTo(y.EarliestRequiredDate);
				}
				else if (x.WP_SystemCreateTimeUtc.CompareTo(y.WP_SystemCreateTimeUtc) != 0)
				{
					return x.WP_SystemCreateTimeUtc.CompareTo(y.WP_SystemCreateTimeUtc);
				}
				else
				{
					return x.WP_PickNo.CompareTo(y.WP_PickNo);
				}
			}

			PickPriorityComparer PriorityComparer => priorityComparer ?? (priorityComparer = new PickPriorityComparer());
			PickPriorityComparer priorityComparer;
		}

		#endregion
	}
}
