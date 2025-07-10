using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business
{
	public interface IDtbConsignmentActionPickedUpDeliveredUpdater
	{
		void ProcessLog(BusinessObjectFactory businessObjectFactory, ZGuid dtbConsignmentActionPK);
	}
}
