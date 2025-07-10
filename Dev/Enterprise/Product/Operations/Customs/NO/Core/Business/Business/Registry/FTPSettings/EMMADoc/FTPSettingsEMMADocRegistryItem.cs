using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry;

public sealed class FTPSettingsEMMADocRegistryItem(
	string name,
	MultilingualString category,
	MultilingualString caption,
	MultilingualString hint,
	RegistryStorageFlags storage,
	RegistryOptions options,
	FTPSettingsRegistry defaultValue)
	: StronglyTypedRegistryItem<FTPSettingsRegistry>(new RegistryItemImpl(name, category, caption, hint, new FTPSettingsEMMADocRegistryDataType(), storage, options, defaultValue));

[RegistryEditor("Enterprise.Customs.NO.Registry.GUI.FTPSettingsEMMADocRegistryItemEditor, Enterprise.Customs.NO.GUI")]
public sealed class FTPSettingsEMMADocRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FTPSettingsRegistry>;
