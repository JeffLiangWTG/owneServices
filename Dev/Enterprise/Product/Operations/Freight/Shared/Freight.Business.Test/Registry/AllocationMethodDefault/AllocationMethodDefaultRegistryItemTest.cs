using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocationMethodDefaultRegistryItem))]
	sealed class AllocationMethodDefaultRegistryItemTest : StronglyTypedRegistryItemTestCase<AllocationMethodDefaultHeader>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<AllocationMethodDefaultHeader, AllocationMethodDefaultHeader> GetNewRegistryItem()
		{
			AllocationMethodDefaultHeader defaultValue = new AllocationMethodDefaultHeader()
			{
				DefaultAllocationMethod = AllocationMethodList.Codes.Country,
			};

			return new AllocationMethodDefaultRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", defaultValue);
		}

		#endregion
	}
}
