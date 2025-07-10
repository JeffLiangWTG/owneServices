using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupCountryViewCollection : ActiveBusinessObjectCollection<CusRefTradeGroupCountryView>
	{
		public CusRefTradeGroupCountryViewCollection(CusRefTradeGroupView parentTradeGroup)
			: base(parentTradeGroup.Factory, parentTradeGroup, new ZQuery(), CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup)
		{
		}
	}
}
