using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupCountryCollection : ActiveBusinessObjectCollection<CusRefTradeGroupCountry>
	{
		public CusRefTradeGroupCountryCollection(CusRefTradeGroup parentTradeGroup)
			: base(parentTradeGroup.Factory, parentTradeGroup, new ZQuery(), CusRefTradeGroupCountrySchema.CRA_CR9_TradeGroup)
		{
		}

		protected override void SetDefaultsForNewElementCore(CusRefTradeGroupCountry newElement)
		{
			var tradeGroup = newElement.TradeGroup;
			if (tradeGroup != null)
			{
				newElement.CRA_StartDate = tradeGroup.CR9_StartDate;
				newElement.CRA_EndDate = tradeGroup.CR9_EndDate;
			}
		}
	}
}
