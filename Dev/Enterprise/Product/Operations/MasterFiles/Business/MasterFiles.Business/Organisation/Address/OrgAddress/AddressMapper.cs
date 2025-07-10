using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AddressMapper : NonPersistentBusinessObject
	{
		public AddressMapper(OrgAddress address1, OrgTranslatedAddress address2)
		{
			Address1 = address1;
			Address2 = address2;
			//Register this to make the notification refresh
			((IBindingList)Address1).ListChanged += AddressMapper_ListChanged;
			((IBindingList)Address2).ListChanged += AddressMapper_ListChanged;
		}

		void AddressMapper_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshBinding();
		}

		public ISupportWebAddressValidation Address1 { get; set; }
		public ISupportWebAddressValidation Address2 { get; set; }
	}
}
