using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Module;

public class MarketIntelligenceAndAnalyticsFeatureControlHelper : IMarketIntelligenceAndAnalyticsFeatureControlHelper
{
	public bool Enabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.ILMarketIntelligenceAndAnalytics) != null;
}
