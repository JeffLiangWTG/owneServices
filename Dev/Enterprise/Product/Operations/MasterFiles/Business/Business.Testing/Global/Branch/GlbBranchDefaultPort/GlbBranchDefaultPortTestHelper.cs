using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class GlbBranchDefaultPortTestHelper
	{
		public static GlbBranchDefaultPort AddDefaultPort(this GlbBranch branch, ZString defaultTo, ZString transportMode, ZString containerMode, ZString portCode)
		{
			if (branch.ExtraPorts.Cast<GlbBranchExtraPorts>().All(extraPort => extraPort.GY_RL_NKAdditionalBranchRelatedPort != portCode))
			{
				var extraPort = branch.ExtraPorts.AddNew();
				extraPort.GY_RL_NKAdditionalBranchRelatedPort = portCode;
			}

			var defaultPort = branch.DefaultPorts.AddNew();
			PopulateGlbBranchDefaultPort(defaultPort, defaultTo, transportMode, containerMode, portCode);
			return defaultPort;
		}

		public static GlbBranchDefaultPort CreateNewDefaultPort(BusinessObjectFactory factory, ZString defaultTo, ZString transportMode, ZString containerMode, ZString portCode)
		{
			var defaultPort = factory.NewWithValidTestData<GlbBranchDefaultPort>();
			PopulateGlbBranchDefaultPort(defaultPort, defaultTo, transportMode, containerMode, portCode);
			return defaultPort;
		}

		static void PopulateGlbBranchDefaultPort(GlbBranchDefaultPort defaultPort, ZString defaultTo, ZString transportMode, ZString containerMode, ZString portCode)
		{
			defaultPort.GBP_DefaultTo = defaultTo;
			defaultPort.GBP_TransportMode = transportMode;
			defaultPort.GBP_ContainerMode = containerMode;
			defaultPort.GBP_RL_NKPort = portCode;
		}
	}
}
