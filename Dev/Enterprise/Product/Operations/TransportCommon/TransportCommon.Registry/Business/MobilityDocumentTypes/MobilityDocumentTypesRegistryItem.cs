using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes
{
	public class MobilityDocumentTypesRegistryItem : StronglyTypedRegistryItem<MobilityDocumentTypeCollection>
	{
		public MobilityDocumentTypesRegistryItem(string name, MultilingualString category, object defaultValue)
			: base(new RegistryItemImpl(
				name,
				category,
				ResString.GetMultilingualString("75f6e403-8ef0-4f2c-9859-f47734e52bf0", "Mobility Document Types"),
				ResString.GetMultilingualString("b14d4e59-d9f5-46a5-ad23-bc834bc75eeb",
				@"This list defines the eDoc Document Types that are accessible via the Mobility App.

Only PDF files can be sent to the device. No other file types (such as Word or Excel) will be sent to the device."),
				new MobilityDocumentTypesDataType(),
				RegistryStorageFlags.System,
				defaultValue))
		{
		}
	}
}
