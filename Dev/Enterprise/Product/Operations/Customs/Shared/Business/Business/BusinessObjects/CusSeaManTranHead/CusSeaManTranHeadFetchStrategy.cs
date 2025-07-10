using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	class CusSeaManTranHeadFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusSeaManTranHeadFetchStrategy(CusSeaManTranHead tranHead)
			: base(tranHead)
		{
			this.tranHead = tranHead;
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(RefVesselSchema.RV_Code, tranHead.BT_VesselName);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (column.ColumnName == CusSeaManTranHead.Schema.DischargePortToShow)
				{
					Factory.AddFetchHint(CusSeaManOBLHeaderSchema.BO_BT, BusinessObject.PK);
				}
			}
		}

		readonly CusSeaManTranHead tranHead;
	}
}
