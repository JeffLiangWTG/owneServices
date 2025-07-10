using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.eTail.Business
{
	public abstract class HVLVFetchHintAllocatorService : IService
	{
		public HVLVFetchHintAllocatorService(BusinessObjectFactory factory, SchemaIntColumn clusterKeyColumn)
		{
			collectionCounter = new Dictionary<int, int>();
			this.factory = factory;
			this.clusterKeyColumn = clusterKeyColumn;
#if DEBUG
			var type = GetType();
			if (!type.IsGenericType)
			{
				throw new Exception("Generic type is needed to distinguish types of services in Factory");
			}
#endif
		}

		readonly Dictionary<int, int> collectionCounter;
		readonly BusinessObjectFactory factory;
		readonly SchemaIntColumn clusterKeyColumn;
		const int ThresholdToApplyFetchHint = 5;

		public void IncrementCount(int clusterKey)
		{
			if (!collectionCounter.ContainsKey(clusterKey))
			{
				collectionCounter[clusterKey] = 1;
			}
			else
			{
				var currentCount = ++collectionCounter[clusterKey];
				if (currentCount == ThresholdToApplyFetchHint)
				{
					factory.AddFetchHint(clusterKeyColumn.TableSchema, new ZQuery(clusterKeyColumn, clusterKey));
				}
			}
		}

#if DEBUG
		public int GetCollectionCountForTesting(int clusterKey) => collectionCounter.GetValueSafe(clusterKey);

		public void SetCounterForTesting(int clusterKey, int collectionCount)
		{
			collectionCounter[clusterKey] = 0;
			for (var i = 0; i < collectionCount; i++)
			{
				IncrementCount(clusterKey);
			}
		}

		public int ThresholdToApplyFetchHintExposed => ThresholdToApplyFetchHint;
#endif
	}

	public class HVLVFetchHintAllocatorService<TCollection> : HVLVFetchHintAllocatorService where TCollection : IBusinessObjectCollection
	{
		public HVLVFetchHintAllocatorService(BusinessObjectFactory factory, SchemaIntColumn clusterKeyColumn) : base(factory, clusterKeyColumn)
		{
		}
	}
}
