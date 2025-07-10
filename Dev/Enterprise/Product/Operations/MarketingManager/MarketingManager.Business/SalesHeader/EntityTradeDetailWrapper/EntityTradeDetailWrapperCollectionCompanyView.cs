using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradeDetailWrapperCollectionCompanyView : BusinessObjectCollectionView<EntityTradeDetailWrapper>
	{
		public EntityTradeDetailWrapperCollectionCompanyView(EntityTradeDetailWrapperCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		public EntitySalesWrapper EntitySales => ((EntityTradeDetailWrapperCollection)collectionToFilter).EntitySales;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var detail = element as EntityTradeDetailWrapper;

			if (EntitySales.CompanyFilter.IsEmpty)
			{
				return true;
			}
			else
			{
				var answer = detail.ProspectCompanyPks != null && detail.ProspectCompanyPks.Any(x => x == EntitySales.CompanyFilter);
				return answer;
			}
		}
	}
}
