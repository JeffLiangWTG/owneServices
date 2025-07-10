using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class DuplicateJobMawbs
	{
		public DuplicateJobMawbs(BusinessObjectFactory factory, ZString numberRangeStart, ZString numberRangeEnd, ZString airlinePrefix)
		{
			this.factory = factory;
			this.numberRangeStart = numberRangeStart;
			this.numberRangeEnd = numberRangeEnd;
			this.airlinePrefix = airlinePrefix;
			count = -1;
		}

		public DuplicateJobMawbs(RangeJobMawb rangeJobMawb)
			: this(rangeJobMawb.Factory, rangeJobMawb.NumberRangeStart, rangeJobMawb.NumberRangeEnd, rangeJobMawb.JM_Airline3DigitPrefix)
		{
		}

		public DuplicateJobMawbs(JobMawb jobMawb)
			: this(jobMawb.Factory, jobMawb.JM_MAWB, jobMawb.JM_MAWB, jobMawb.JM_Airline3DigitPrefix)
		{
		}

		public int Count
		{
			get
			{
				if (count == -1)
				{
					count = factory.GetDatabaseCount(typeof(JobMawb), Filter);
				}
				return count;
			}
		}
		int count;

		public ZString FirstDuplicate
		{
			get
			{
				ZString result = "";
				if (Count > 0)
				{
					var jobMawb = factory.LoadTop1<JobMawb>(Filter);
					result = jobMawb.JM_MAWB;
				}

				return result;
			}
		}

		ZQuery Filter
		{
			get
			{
				var result = new ZQuery(JobMawbSchema.JM_MAWB, SQLComparisonOperator.GreaterThanOrEqualTo, numberRangeStart);
				result.AddToFilter(JobMawbSchema.JM_MAWB, SQLComparisonOperator.LessThanOrEqualTo, numberRangeEnd);
				result.AddToFilter(JobMawbSchema.JM_Airline3DigitPrefix, airlinePrefix);
				result.AddToFilter(JobMawbSchema.JM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMonths(-Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));

				return result;
			}
		}

		readonly BusinessObjectFactory factory;
		readonly ZString numberRangeStart;
		readonly ZString numberRangeEnd;
		readonly ZString airlinePrefix;
	}
}
