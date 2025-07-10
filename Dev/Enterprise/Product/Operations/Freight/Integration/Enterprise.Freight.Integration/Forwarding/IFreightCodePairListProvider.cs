using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Integration
{
	public interface IFreightCodePairListProvider
	{
		CodeDescriptionPairList GetContainerModeList(string transportMode);

		CodeDescriptionPairList GetConsolTransportModeList();

		CodeDescriptionPairList GetAgentTypeList(bool isAir, BusinessObjectFactory factory = null);

		CodeDescriptionPairList GetConsolModeList(ZString agentType, ZString transportMode);
	}
}
