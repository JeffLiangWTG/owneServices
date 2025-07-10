using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect
{
	public interface IRateSelectorProviderFactory
	{
		internal IEnumerable<IRateSelectorProvider> CreateProviders(LoggerDecorator loggerDecorator, BusinessObjectFactory factory);
	}
}
