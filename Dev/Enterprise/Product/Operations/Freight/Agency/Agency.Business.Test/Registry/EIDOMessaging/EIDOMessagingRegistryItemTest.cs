using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(EIDOMessagingRegistryItem))]
	internal class EIDOMessagingRegistryItemTest : StronglyTypedRegistryItemTestCase<EIDOMessagingHeader>
	{
		#region Implementation
		protected override StronglyTypedRegistryItem<EIDOMessagingHeader, EIDOMessagingHeader> GetNewRegistryItem()
		{
			return new EIDOMessagingRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", new EIDOMessagingRegistryDataType());
		}
		#endregion
	}
}
