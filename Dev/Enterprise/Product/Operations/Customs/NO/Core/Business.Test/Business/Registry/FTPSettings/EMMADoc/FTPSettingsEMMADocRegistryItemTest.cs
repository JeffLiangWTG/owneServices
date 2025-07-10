using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsEMMADocRegistryItem))]
sealed class FTPSettingsEMMADocRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FTPSettingsRegistry>
{
	protected override StronglyTypedRegistryItem<FTPSettingsRegistry, FTPSettingsRegistry> GetNewRegistryItem()
	{
		return new FTPSettingsEMMADocRegistryItem(
			name: "name",
			category: (NoResString)"category",
			caption: (NoResString)"caption",
			hint: (NoResString)"hint",
			storage: RegistryStorageFlags.Company,
			options: RegistryOptions.IsOnlyForController,
			defaultValue: FTPSettingsEMMADocRegistry.DefaultValues);
	}

	protected override FTPSettingsRegistry ValidValue =>
		new FTPSettingsRegistry
		{
			Url = "ftpedoc.emma.no",
			Port = "21",
			Username = "admin",
			Password = "password"
		 };
}
