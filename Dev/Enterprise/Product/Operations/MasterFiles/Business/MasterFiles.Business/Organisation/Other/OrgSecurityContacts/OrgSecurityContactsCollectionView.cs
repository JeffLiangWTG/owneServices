using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityContactsCollectionView : BusinessObjectCollectionView<OrgSecurityContacts>
	{
		public OrgSecurityContactsCollectionView(OrgSecurityContactsCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var security = (element as OrgSecurityContacts)?.Security;
			return security != null && security.WebSecurityRight != null && security.Organisation != null && (security.Organisation.OH_IsWarehouseClient || !security.WebSecurityRight.IsWarehouse);
		}
	}
}
