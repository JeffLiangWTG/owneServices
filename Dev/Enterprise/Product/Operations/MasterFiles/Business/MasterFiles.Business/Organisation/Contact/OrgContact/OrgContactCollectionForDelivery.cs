using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactCollectionForDelivery : BusinessObjectCollectionView<OrgContact>
	{
		public OrgContactCollectionForDelivery(OrgContactDependentCollection collectionToFilter) : base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var contact = (OrgContact)element;
			return !contact.IsSystemDefaultContact || contact.IsSystemDefaultContactForAutoDelivery;
		}
	}
}
