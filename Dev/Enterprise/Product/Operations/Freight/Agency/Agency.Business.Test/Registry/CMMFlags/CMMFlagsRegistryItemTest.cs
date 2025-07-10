using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(CMMFlagsRegistryItem))]
	internal class CMMFlagsRegistryItemTest : StronglyTypedRegistryItemTestCase<CMMFlags>
	{
		#region Implementation
		protected override StronglyTypedRegistryItem<CMMFlags, CMMFlags> GetNewRegistryItem()
		{
			return new CMMFlagsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
		#endregion
	}
}
