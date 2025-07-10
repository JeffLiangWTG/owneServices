using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveOrAllAddressesCollection : BusinessObjectCollectionView<OrgAddress>
	{
		public ActiveOrAllAddressesCollection(OrgAddressDependentCollection collectionToFilter)
			: base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var address = (OrgAddress)element;
			return IncludeInactiveAddresses
				|| address.OA_IsActive
				|| (!address.OA_IsActive && (address.HasErrors || address.HasChanges));
		}

		#region IncludeInactiveAddresses

		public bool IncludeInactiveAddresses
		{
			get { return includeInactiveAddresses; }
			set
			{
				if (value != includeInactiveAddresses)
				{
					includeInactiveAddresses = value;
					Rebuild();
				}
			}
		}

		bool includeInactiveAddresses;

		#endregion
	}
}
