using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ServiceLevel)]
	public class GatewayServiceLevelCollection : RefServiceLevelCollection
	{
		public GatewayServiceLevelCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(RefServiceLevelSchema.RS_IsGateway, true))
		{
		}

		protected override void OnAdded(RefServiceLevel serviceLevel)
		{
			base.OnAdded(serviceLevel);
			serviceLevel.RS_IsGateway = true;
		}
	}
}
