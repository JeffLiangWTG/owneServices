using CargoWise.EntityFramework;
using Enterprise.Customs.SG.Access.Business.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPackedItemFetchStrategy : ASYCUDA.Business.AsycudaPackedItemFetchStrategy
	{
		public AsycudaPackedItemFetchStrategy(AsycudaPackedItem packedItem)
			: base(packedItem)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AsycudaPackedItem.Schema.GoodsType:
						Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
						break;
				}
			}
		}

		protected override void AddFetchHintsForValidateCore()
		{
			base.AddFetchHintsForValidateCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
			if (SGAccessRegistry.IsOVRLiveEffective)
			{
				Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.API_ABL_Bill);
			}
		}

		protected override void AddFetchHintsForDeleteCore()
		{
			base.AddFetchHintsForDeleteCore();
			Factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, BusinessObject.PK);
		}
	}
}
