using Enterprise.TransportConsignment.ProductionRulesEngine;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.Business
{
	class ControllingBranchDefaultingManager : ValueDefaultingManager, IControllingBranchDefaultingManager
	{
		public ControllingBranchDefaultingManager(ILandTransportFactLoaderProvider factLoaderProvider) : base(factLoaderProvider)
		{
		}

		protected override RulesContextType ContextType => RulesContextType.LandTransportControllingBranchDefaulting;
	}
}
