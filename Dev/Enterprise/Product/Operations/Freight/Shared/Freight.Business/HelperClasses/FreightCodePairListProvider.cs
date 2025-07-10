using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class FreightCodePairListProvider : IFreightCodePairListProvider
	{
		#region IFreightCodePairListProvider Members

		public CodeDescriptionPairList GetContainerModeList(string transportMode)
		{
			return FreightCodePairLists.JS_PackingModeList(transportMode);
		}

		public CodeDescriptionPairList GetConsolTransportModeList()
		{
			return new ConsolTransportModeCodeDescriptionPairList();
		}

		public CodeDescriptionPairList GetAgentTypeList(bool isAir, BusinessObjectFactory factory = null)
		{
			return FreightCodePairLists.AgentTypeList(isAir, factory);
		}

		public CodeDescriptionPairList GetConsolModeList(ZString agentType, ZString transportMode)
		{
			return FreightCodePairLists.ConsolModeList(agentType, transportMode);
		}

		#endregion
	}
}
