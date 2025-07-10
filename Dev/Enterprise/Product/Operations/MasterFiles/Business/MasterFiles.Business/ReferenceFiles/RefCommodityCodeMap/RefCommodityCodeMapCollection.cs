using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeMapCollection : ActiveBusinessObjectCollection<RefCommodityCodeMap>
	{
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		protected RefCommodityCode Parent;

		public RefCommodityCodeMapCollection(BusinessObject parent) : base(parent.Factory)
		{
			this.Parent = parent as RefCommodityCode;
		}

		public RefCommodityCodeMapCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCommodityCodeMapCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefCommodityCodeMapCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override void SetDefaultsForNewElementCore(RefCommodityCodeMap newCommodityCodeMap)
		{
			base.SetDefaultsForNewElementCore(newCommodityCodeMap);
			if (newCommodityCodeMap != null && Parent != null)
			{
				newCommodityCodeMap.LC_RH_NKCommodityCode = Parent.RH_Code;
			}
		}
	}
}
