using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusExcludedTradeGroupNew : INonPersistentBusinessObject
	{
		public RefCusExcludedTradeGroupNew(INonPersistentBusinessObjectFlatten topLevelNonPersistentObject)
		{
			S03_PK = Guid.NewGuid();
			TopLevelNonPersistentObjects.Add(topLevelNonPersistentObject);
		}

		public RefCusExcludedTradeGroupNew(RefCusExcludedTradeGroup ex, INonPersistentBusinessObjectFlatten topLevelNonPersistentObject) : this(topLevelNonPersistentObject)
		{
			S03_ZZA_TradeGroup = ex.ZZC_ZZA_TradeGroup;
			RefCusExcludedTradeGroup = ex;
			ex.RefCusExcludedTradeGroupNews.Add(this);
		}

		public void Update()
		{
			var ex = RefCusExcludedTradeGroup;
			if (ex != null)
			{
				ex.ZZC_ZZA_TradeGroup = S03_ZZA_TradeGroup;
			}
		}

		public IEnumerable<object> Unlink()
		{
			var ex = RefCusExcludedTradeGroup;
			ex?.RefCusExcludedTradeGroupNews.Remove(this);
			RefCusExcludedTradeGroup = null;
			return new [] { ex };
		}

		public void Link(RefCusExcludedTradeGroup ex)
		{
			RefCusExcludedTradeGroup = ex;
			if (!ex.RefCusExcludedTradeGroupNews.Contains(this))
			{
				ex.RefCusExcludedTradeGroupNews.Add(this);
			}
		}

		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects { get; } = new HashSet<INonPersistentBusinessObjectFlatten>();

		public Guid S03_PK { get; set; }
		public Guid? S03_S01_RateApplicability { get; set; }
		public Guid? S03_S07_ConditionApplicability { get; set; }
		public Guid? S03_ZZA_TradeGroup { get; set; }

		public RefCusExcludedTradeGroup RefCusExcludedTradeGroup { get; set; }
	}
}
