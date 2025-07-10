using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ApplicationStatusRegistryItem))]
	sealed class ApplicationStatusRegistryItemTest : StronglyTypedRegistryItemTestCase<ApplicationStatusCollection>
	{
		public void TestConstructor()
		{
			ApplicationStatusRegistryItem item = (ApplicationStatusRegistryItem)GetNewRegistryItem();
			AssertEquals(typeof(ApplicationStatusRegistryDataType), item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<ApplicationStatusCollection, ApplicationStatusCollection> GetNewRegistryItem()
		{
			return new ApplicationStatusRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override ApplicationStatusCollection ValidValue
		{
			get
			{
				ApplicationStatusCollection collection = new ApplicationStatusCollection();
				collection.AddPair("MEH", "Description");
				return collection;
			}
		}
	}
}
