using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public interface ICommonGoodsItemsIntegrator
	{
		IEnumerable<ICommonGoodsItem> GetCommonGoodsItemsForIntegration();
		void CopyCommonGoodsItems(IEnumerable<ICommonGoodsItem> goodsItemsForIntegration, int targetIndex);
		IBusinessObjectCollection TheOtherCollectionToAttach();
	}
}
