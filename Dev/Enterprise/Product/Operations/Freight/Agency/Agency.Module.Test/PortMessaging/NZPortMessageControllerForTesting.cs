using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class NZPortMessageControllerForTesting : AgencyPortMessagesController
	{
		public new ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return base.GetPlugIn(businessEntity);
		}
	}
}
