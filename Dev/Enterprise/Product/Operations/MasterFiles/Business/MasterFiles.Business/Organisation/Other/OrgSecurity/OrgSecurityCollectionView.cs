using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityCollectionView : BusinessObjectCollectionView<OrgSecurity>
	{
		public OrgSecurityCollectionView(OrgSecurityCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		public new OrgSecurityCollection CollectionToFilter => (OrgSecurityCollection)base.CollectionToFilter;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var security = element as OrgSecurity;
			return security != null && security.WebSecurityRight != null && security.Organisation != null && (security.Organisation.OH_IsWarehouseClient || !security.WebSecurityRight.IsWarehouse);
		}
	}
}
