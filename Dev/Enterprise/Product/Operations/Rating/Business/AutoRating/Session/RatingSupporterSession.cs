using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public sealed class RatingSupporterSession : IDisposable
	{
		Action cleanUpCacheAction;

		public RatingSupporterSession(Action cleanUpCacheAction, IBusiness target)
		{
			this.cleanUpCacheAction = cleanUpCacheAction;
			this.target = target;
		}

		public void Dispose()
		{
			cleanUpCacheAction();
			cleanUpCacheAction = null;
			target = null;
		}

		public BusinessObject Target
		{
			get { return (BusinessObject)target; }
		}

		IBusiness target;

		public bool IsBeingCopiedFromQuote(JobCharge charge)
		{
			return chargeBeingCopiedFromQuotePKs.Contains(charge, BusinessObjectEqualityComparer<JobCharge>.PKOnlyComparer);
		}

		public void MarkAsBeingCopiedFromQuote(JobCharge charge)
		{
			chargeBeingCopiedFromQuotePKs.Add(charge);
		}

		readonly List<JobCharge> chargeBeingCopiedFromQuotePKs = new List<JobCharge>();

		public T GetCachedValue<T>(string cacheKey, Func<T> retriever)
		{
			return (T)cacheDictionary.GetOrAdd(cacheKey, () => retriever());
		}

		readonly Dictionary<string, object> cacheDictionary = new Dictionary<string, object>();
	}
}
