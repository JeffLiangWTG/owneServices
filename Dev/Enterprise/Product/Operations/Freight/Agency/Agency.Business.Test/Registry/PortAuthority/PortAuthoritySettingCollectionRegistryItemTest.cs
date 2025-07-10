using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthoritySettingsRegistryItem))]
	internal sealed class PortAuthoritySettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<PortAuthoritySettings>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<PortAuthoritySettings, PortAuthoritySettings> GetNewRegistryItem()
		{
			return new PortAuthoritySettingsRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System | RegistryStorageFlags.Branch);
		}
		#endregion
	}
}
