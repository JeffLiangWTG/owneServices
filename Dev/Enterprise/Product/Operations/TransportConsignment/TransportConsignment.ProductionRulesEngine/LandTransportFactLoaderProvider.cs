using CargoWise.Application;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	class LandTransportFactLoaderProvider : ILandTransportFactLoaderProvider
	{
		public ILandTransportFactLoader GetFactLoader(RulesContextType rulesContextType)
		{
			ILandTransportFactLoader loader = null;
			switch (rulesContextType)
			{
				case RulesContextType.LandTransportControllingBranchDefaulting:
					loader = ObjectFactory.Get<IConsignmentFactLoader>(nameof(IConsignmentFactLoader));
					break;
			}
			return loader;
		}
	}
}
