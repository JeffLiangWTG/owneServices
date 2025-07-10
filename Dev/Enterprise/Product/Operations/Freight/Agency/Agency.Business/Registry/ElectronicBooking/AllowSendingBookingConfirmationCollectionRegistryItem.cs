using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	public class AllowSendingBookingConfirmationCollectionRegistryItem : StronglyTypedRegistryItem<AllowSendingBookingConfirmationCollection>
	{
		public AllowSendingBookingConfirmationCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, AllowSendingBookingConfirmationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AllowSendingBookingConfirmationRegistryDataType(defaultValue), storage, options))
		{
		}

		#region AllowSendingBookingConfirmationRegistryDataType

		[RegistryEditor("Enterprise.Freight.Agency.GUI.AllowSendingBookingConfirmationRegistryItemEditor, Enterprise.Freight.Agency.GUI")]
		public class AllowSendingBookingConfirmationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AllowSendingBookingConfirmationCollection>
		{
			public AllowSendingBookingConfirmationRegistryDataType(AllowSendingBookingConfirmationCollection defaultValue)
			: base(defaultValue) { }
		}

		#endregion
	}
}
