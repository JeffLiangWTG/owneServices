using Microsoft.Extensions.Logging;
using Moq;
using WTG.ErrorReporting;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	public class DummyBillingTransactionsPlugin : AbstractPlugin
	{
		public DummyBillingTransactionsPlugin()
		{
			InstanceCount++;
			LastInstance = this;
		}

		public DateTime LastStart { get; private set; }

		public DateTime LastEnd { get; private set; }
		public IEnumerable<TimeStampedTransaction> FoundTransactions { get; private set; } = Enumerable.Empty<TimeStampedTransaction>();

		public override void UpdateSettings(PluginSettings settings)
		{
			UpdateSettingsCallsCount++;
		}

		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{
			LastStart = start;
			LastEnd = end;
			var transactions = Transactions != null ? Transactions.Where(t => t.TimeStamp > start && t.TimeStamp <= end) : Enumerable.Empty<TimeStampedTransaction>();
			FoundTransactions = FoundTransactions.Concat(transactions);
			GetTransactionsCallsCount++;
			exceptionsCounter++;
			if (exceptionsCounter <= MaxExceptionsCount) throw new NotImplementedException();
			transactionsMock.Setup(x => x.GetEnumerator()).Returns(transactions.GetEnumerator).Callback(() => iteratedTransactionsCount++);

			return transactionsMock.Object;
		}

		protected override void Dispose(bool disposing)
		{
			Logger.LogDebug($"Iterated transactions {iteratedTransactionsCount} times");
			IsDisposed = true;
		}

		public bool IsDisposed { get; private set; } = false;

		public int GetTransactionsCallsCount { get; private set; } = 0;

		public int UpdateSettingsCallsCount { get; private set; } = 0;

		public static int InstanceCount { get; private set; } = 0;

		public static DummyBillingTransactionsPlugin LastInstance { get; private set; } = null;

		public static void Reset()
		{
			MaxExceptionsCount = 0;
			InstanceCount = 0;
			LastInstance = null;
			Transactions = null;
		}

		public static int MaxExceptionsCount { get; set; } = 0;
		public static TimeStampedTransaction[] Transactions { get; set; }

		Mock<IEnumerable<TimeStampedTransaction>> transactionsMock = new Mock<IEnumerable<TimeStampedTransaction>>();

		int exceptionsCounter = 0;
		int iteratedTransactionsCount = 0;
	}
}
