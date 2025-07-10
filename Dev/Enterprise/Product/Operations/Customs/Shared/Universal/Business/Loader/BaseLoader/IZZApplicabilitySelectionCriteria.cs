using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IZZApplicabilitySelectionCriteria
	{
		ZDateTime EffectiveDate { get; }
		ZString TradeGroupCountry { get; }
		ZString DataGrouping { get; }
		ZString PrimaryPreference { get; }
		ISet<ZString> AdditionalCodes { get; }
		ZString ConcessionOrder { get; }
		ISet<ZString> SecondTradeGroups { get; }
	}
}
