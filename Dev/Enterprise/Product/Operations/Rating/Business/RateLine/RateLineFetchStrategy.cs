using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public RateLineFetchStrategy(RateLine line)
			: base(line)
		{
		}

		protected override void FetchForDeleteCore()
		{
			// Related objects are deleted through a direct query. We gain no benefit from fetching.
		}

		protected override void FetchForViewCore(CargoWise.EntityFramework.TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(AccChargeCodeSchema.Constants.TableName, ((RateLine)BusinessObject).TL_AC);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(RateLineItemsSchema.TM_TL, BusinessObject.PK);
			Factory.AddFetchHint(AccChargeCodeSchema.Constants.TableName, ((RateLine)BusinessObject).TL_AC);
		}
	}
}
