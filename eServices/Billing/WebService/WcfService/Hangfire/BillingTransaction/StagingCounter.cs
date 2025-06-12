using System;
using CargoWise.eServices.Billing.DataAccess;

namespace CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction
{
	public class StagingCounter
	{
		public static StagingCounter Singleton => instance.Value;
		static readonly Lazy<StagingCounter> instance = new Lazy<StagingCounter>(() => new StagingCounter());
		readonly object lockObject = new object();
		readonly IBillingRepository repository;
		readonly IConfigurationProvider configurationProvider;
		readonly int cacheTimeInMinutes;
		DateTime lastFreshTime;
		int count;

		public StagingCounter() : this(new BillingRepository(), Global.WindsorContainer.Resolve<IConfigurationProvider>()) { }
		public StagingCounter(IBillingRepository repository, IConfigurationProvider configurationProvider)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
			cacheTimeInMinutes = configurationProvider.CacheTimeInMinutesOfStagingCounter;
			lastFreshTime = DateTime.MinValue;
		}

		public int Count
		{
			get
			{
				lock (lockObject)
				{
					var now = Now();
					if ((now - lastFreshTime).TotalMinutes > cacheTimeInMinutes)
					{
						Refresh(now);
					}
					return count;
				}
			}
		}

		internal virtual DateTime Now() => DateTime.Now;

		void Refresh(DateTime now)
		{
			count = repository.CountStaging();
			lastFreshTime = now;
		}
	}
}
