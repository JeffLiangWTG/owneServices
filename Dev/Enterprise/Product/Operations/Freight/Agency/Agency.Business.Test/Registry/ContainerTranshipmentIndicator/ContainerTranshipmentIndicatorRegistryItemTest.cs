using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerTranshipmentIndicatorRegistryItem))]
	internal class ContainerTranshipmentIndicatorRegistryItemTest : StronglyTypedRegistryItemTestCase<ContainerTranshipmentIndicatorCollection>
	{
		#region Implementation
		protected override StronglyTypedRegistryItem<ContainerTranshipmentIndicatorCollection, ContainerTranshipmentIndicatorCollection> GetNewRegistryItem()
		{
			return new ContainerTranshipmentIndicatorRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
		#endregion
	}
}
