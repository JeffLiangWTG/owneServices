using System;

namespace Enterprise.Freight.Business
{
	internal sealed class ScheduleUpdateServices : IScheduleUpdateServices
	{
		public ScheduleUpdateServices(JobVoyage voyage)
		{
			if (voyage == null)
			{
				throw new ArgumentNullException(nameof(voyage));
			}

			this.voyage = voyage;
			this.queryProvider = ScheduleUpdateQueryProviderFactory.Get(voyage.Factory);
		}

		public Type ParentConsolType
		{
			get { return voyage.ParentConsolType; }
		}

		public IScheduleUpdateQueryProvider QueryProvider
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return queryProvider; }
		}

		readonly JobVoyage voyage;
		readonly IScheduleUpdateQueryProvider queryProvider;
	}
}
