using System.Collections;
using CargoWise.Application;

namespace Enterprise.Customs.Business
{
	public class LandedCostOnlyConfigurationProviderBase : ILandedCostOnlyConfigurationProvider
	{
		public ILandedCostOnlyConfiguration GetLandedCostOnlyConfiguration(BaseJobDeclaration declaration)
		{
			var countryCode = declaration?.CountryCode;
			var types = (Hashtable)ObjectFactory.Get("LandedCostingOnlyConfigurations");

			var objectHandle = (ObjectHandle)types[countryCode.ToString()];
			return (ILandedCostOnlyConfiguration)objectHandle?.GetObject();
		}
	}
}
