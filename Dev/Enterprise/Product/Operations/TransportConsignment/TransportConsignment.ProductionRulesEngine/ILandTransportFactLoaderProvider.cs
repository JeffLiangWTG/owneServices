using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	public interface ILandTransportFactLoaderProvider
	{
		public ILandTransportFactLoader GetFactLoader(RulesContextType rulesContextType);
	}
}
