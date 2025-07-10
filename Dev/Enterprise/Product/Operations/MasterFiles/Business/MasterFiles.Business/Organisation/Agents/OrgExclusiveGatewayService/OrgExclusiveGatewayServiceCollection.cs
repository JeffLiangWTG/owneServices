using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgExclusiveGatewayServiceCollection : DependentBusinessObjectCollection<OrgExclusiveGatewayService, OrgAppointedAgentPorts>
	{
		public OrgExclusiveGatewayServiceCollection(OrgAppointedAgentPorts master)
			: base(master)
		{
			Load();
		}

		protected override string FkColumnName
			=> OrgExclusiveGatewayServiceSchema.Constants.O7_O5_AgentPort;
	}
}
