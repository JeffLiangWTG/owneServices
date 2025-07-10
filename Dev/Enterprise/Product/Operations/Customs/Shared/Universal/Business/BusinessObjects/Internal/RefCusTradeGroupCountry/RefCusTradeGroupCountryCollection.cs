using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusTradeGroupCountryCollection : ActiveBusinessObjectCollection<RefCusTradeGroupCountry>
	{
		public RefCusTradeGroupCountryCollection(RefCusTradeGroup parentTariff)
			: base(parentTariff.Factory, parentTariff, new ZQuery(), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup)
		{
		}
	}
}
