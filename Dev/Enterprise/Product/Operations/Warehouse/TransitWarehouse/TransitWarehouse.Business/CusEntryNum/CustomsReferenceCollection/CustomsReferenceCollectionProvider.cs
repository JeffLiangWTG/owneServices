using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	class CustomsReferenceCollectionProvider : ICustomsReferenceCollectionProvider
	{
		ICustomsReferenceCollection ICustomsReferenceCollectionProvider.GetCollection(BusinessObject parent)
		{
			return new CustomsReferenceCollection(parent);
		}
	}
}
