using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect
{
	public class RateSelectorProviderFactory : IRateSelectorProviderFactory
	{
		public IEnumerable<IRateSelectorProvider> CreateProviders(LoggerDecorator loggerDecorator, BusinessObjectFactory factory)
		{
			return new IRateSelectorProvider[]
			{
				new UrsRateSelectorProvider(factory, loggerDecorator),
				new CW1RateSelectorProvider(factory, loggerDecorator)
			};
		}
	}
}
