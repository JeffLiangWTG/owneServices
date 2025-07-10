using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	class PortReferenceCollectionProvider : IPortReferenceCollectionProvider
	{
		IPortReferenceCollection IPortReferenceCollectionProvider.GetCollection(BusinessObject parent)
		{
			return new PortReferenceCollection(parent);
		}
	}
}
