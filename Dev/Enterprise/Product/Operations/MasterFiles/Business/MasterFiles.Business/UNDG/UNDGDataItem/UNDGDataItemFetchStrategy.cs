using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class UNDGDataItemFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public UNDGDataItemFetchStrategy(UNDGDataItem uNDGDataItem)
			: base(uNDGDataItem)
		{
		}

		UNDGDataItem UNDGDataItem
		{
			get { return BusinessObject as UNDGDataItem; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(UNDGDataItemSchema.DI_ParentID, UNDGDataItem.DI_ParentID);
			Factory.AddFetchHint(UNDGSubstancePivotSchema.DP_ParentId, UNDGDataItem.PK);
		}
	}
}
