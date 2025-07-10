using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityRatingCodeMapCollection : ActiveBusinessObjectCollection<RefCommodityRatingCodeMap>
	{
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		protected RefCommodityCode Parent;
		public RefCommodityRatingCodeMapCollection(BusinessObject parent) : base(parent.Factory)
		{
			this.Parent = parent as RefCommodityCode;
		}
		public RefCommodityRatingCodeMapCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCommodityRatingCodeMapCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefCommodityRatingCodeMapCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override void SetDefaultsForNewElementCore(RefCommodityRatingCodeMap newCommodityCodeMap)
		{
			base.SetDefaultsForNewElementCore(newCommodityCodeMap);
			if (newCommodityCodeMap != null && Parent != null)
			{
				newCommodityCodeMap.RI_RH_NKCommodityParent = Parent.RH_Code;
			}
		}
	}
}
