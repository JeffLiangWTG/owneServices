using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class EntryNumberBizObjCollection : NonPersistentBusinessObjectCollection<EntryNumberBizObj>
	{
		public EntryNumberBizObjCollection(EntrySummaryQueryBizObj queryData)
		{
			this.queryData = queryData;
		}

		readonly EntrySummaryQueryBizObj queryData;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntryNumberBizObj(queryData);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((EntryNumberBizObj)child).EntryFilerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).EntryFilerCode;
		}
	}
}
