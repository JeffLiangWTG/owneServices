using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHistoryFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public LandedCostHistoryFetchStrategy(EnterpriseBusinessObject landedCostHistory) : base(landedCostHistory)
		{
		}

		protected new LandedCostHistory BusinessObject => (LandedCostHistory)base.BusinessObject;

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			Factory.AddFetchHint(LandedLineCostItemSchema.LZ_LH, BusinessObject.PK);
		}
	}
}
