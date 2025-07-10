using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public class HVLVItemLineCollection : BusinessObjectCollectionWithAutoFetchHints<HVLVItemLine, HVLVItem>, IHVLVItemLineCollection
	{
		public HVLVItemLineCollection(HVLVItem parent)
			: base(parent)
		{
		}

		IHVLVItemLine IHVLVItemLineCollection.this[int i] => (HVLVItemLine)Elements[i];

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return HVLVItemLineSchema.HVS_HVI_HVLVItem; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			if (!Master.IsDeleted && !Master.HVI_ClusterKey.IsEmpty)
			{
				result.AddToFilter(new ZQuery(HVLVItemLineSchema.HVS_ClusterKey, Master.HVI_ClusterKey));
			}

			return result;
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			Master.CalculateManifestedWeight();
		}

#if DEBUG
		public static void SetCollectionCountForTest(BusinessObjectFactory factory, ZInt clusterKey, int collectionCount)
		{
			var service = factory.ServiceContainer.AddService(new HVLVFetchHintAllocatorService<HVLVItemLineCollection>(factory, HVLVItemLineSchema.HVS_ClusterKey));
			service.SetCounterForTesting(clusterKey, collectionCount);
		}
#endif
	}
}
