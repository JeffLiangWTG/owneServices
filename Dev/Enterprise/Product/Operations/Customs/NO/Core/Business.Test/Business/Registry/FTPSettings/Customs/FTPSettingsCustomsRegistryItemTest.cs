using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(FTPSettingsCustomsRegistryItem))]
sealed class FTPSettingsCustomsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<FTPSettingsCustomsRegistry>
{
	protected override StronglyTypedRegistryItem<FTPSettingsCustomsRegistry, FTPSettingsCustomsRegistry> GetNewRegistryItem()
	{
		return new FTPSettingsCustomsRegistryItem(
			name: "name",
			category: (NoResString)"category",
			caption: (NoResString)"caption",
			hint: (NoResString)"hint",
			storage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
			options: RegistryOptions.IsOnlyForController,
			defaultValue: FTPSettingsCustomsRegistry.DefaultValues);
	}

	protected override FTPSettingsCustomsRegistry ValidValue =>
		new FTPSettingsCustomsRegistry
		{
			Url = "ftp.ec.evry.com/",
			Port = "21",
			SendToCustomFolder = "in/",
			ReceiveFromCustomFolder = "out/",
			Username = "admin",
			Password = "password"
		};
}
