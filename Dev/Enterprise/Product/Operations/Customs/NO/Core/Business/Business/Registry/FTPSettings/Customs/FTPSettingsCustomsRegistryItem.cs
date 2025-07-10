using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry;

public sealed class FTPSettingsCustomsRegistryItem(
	string name,
	MultilingualString category,
	MultilingualString caption,
	MultilingualString hint,
	RegistryStorageFlags storage,
	RegistryOptions options,
	FTPSettingsCustomsRegistry defaultValue)
	: StronglyTypedRegistryItem<FTPSettingsCustomsRegistry>(new RegistryItemImpl(name, category, caption, hint, new FTPSettingsCustomsRegistryDataType(), storage, options, defaultValue));

[RegistryEditor("Enterprise.Customs.NO.Registry.GUI.FTPSettingsCustomsRegistryItemEditor, Enterprise.Customs.NO.GUI")]
public sealed class FTPSettingsCustomsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FTPSettingsCustomsRegistry>;
