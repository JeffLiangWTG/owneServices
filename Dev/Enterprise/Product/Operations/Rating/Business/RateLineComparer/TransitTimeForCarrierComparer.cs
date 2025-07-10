using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class TransitTimeForCarrierComparer : TransitTimeComparer
	{
		public TransitTimeForCarrierComparer(RatingCriteria criteria) : base(criteria)
		{
			transitTimes = criteria.JobDatesProvider is IJobDatesProviderForCarriers jobDatesProviderForCarriers
				? jobDatesProviderForCarriers.CarrierTransitTimes
				: new Dictionary<ZGuid, string>();
		}

		readonly Dictionary<ZGuid, string> transitTimes;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var serviceProvider1 = line1.ParentRateEntry.ServiceProviderPK();
			var serviceProvider2 = line2.ParentRateEntry.ServiceProviderPK();

			if (serviceProvider1 == serviceProvider2)
			{
				if (transitTimes.ContainsKey(serviceProvider1))
				{
					var transitTime = transitTimes[serviceProvider1];
					return Compare(line1, line2, transitTime);
				}
			}

			return 0;
		}

		protected override string GetName() => (NoResString)"Transit Times for Carrier"; // log message, subject to change, more for support people as of nows
	}
}
