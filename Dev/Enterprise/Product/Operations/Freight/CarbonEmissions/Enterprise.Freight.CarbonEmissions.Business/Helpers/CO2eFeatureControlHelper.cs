using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.CarbonEmissions.Business;

public class CO2eFeatureControlHelper : ICO2eFeatureControlHelper
{
	public bool Enabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CarbonEmissionGreenhouseGasFeature) != null;

	public bool DashboardEnabled => ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CarbonEmissionDashboardPortalFeature) != null;
}
