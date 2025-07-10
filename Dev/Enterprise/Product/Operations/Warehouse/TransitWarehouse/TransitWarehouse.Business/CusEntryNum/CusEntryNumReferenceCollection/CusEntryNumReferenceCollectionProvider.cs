using CargoWise.EntityFramework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business
{
	class CusEntryNumReferenceCollectionProvider : ICusEntryNumReferenceCollectionProvider
	{
		ICusEntryNumReferenceCollection ICusEntryNumReferenceCollectionProvider.GetCollection(BusinessObject parent)
		{
			return new CusEntryNumReferenceCollection(parent);
		}
	}
}
