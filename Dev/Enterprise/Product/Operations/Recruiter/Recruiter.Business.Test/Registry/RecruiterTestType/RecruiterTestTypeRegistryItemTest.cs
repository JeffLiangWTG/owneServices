using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(RecruiterTestTypeRegistryItem))]
	sealed class RecruiterTestTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<RecruiterTestTypeCollection, RecruiterTestTypeCollection>
	{
		public void TestConstructor()
		{
			RecruiterTestTypeRegistryItem item1 = new RecruiterTestTypeRegistryItem("name1", (NoResString)"category1", (NoResString)"caption1", (NoResString)"hint1", RegistryStorageFlags.System);
			AssertEquals("name1", item1.Name);
			AssertEquals("category1", item1.Category);
			AssertEquals("caption1", item1.Caption);
			AssertEquals("hint1", item1.Hint);
			AssertEquals(RegistryStorageFlags.System, item1.Storage);
			AssertEquals(item1.DataType.Serialise(new RecruiterTestTypeCollection()), item1.DataType.Serialise(item1.DefaultValue));

			RecruiterTestTypeCollection coll = new RecruiterTestTypeCollection();
			coll.Add("cod", "description");
			RecruiterTestTypeRegistryItem item2 = new RecruiterTestTypeRegistryItem("name2", (NoResString)"category2", (NoResString)"caption2", (NoResString)"hint2", RegistryStorageFlags.Branch, coll);
			AssertEquals("name2", item2.Name);
			AssertEquals("category2", item2.Category);
			AssertEquals("caption2", item2.Caption);
			AssertEquals("hint2", item2.Hint);
			AssertEquals(RegistryStorageFlags.Branch, item2.Storage);
			AssertEquals(item2.DataType.Serialise(coll), item2.DataType.Serialise(item2.DefaultValue));
		}

		protected override StronglyTypedRegistryItem<RecruiterTestTypeCollection, RecruiterTestTypeCollection> GetNewRegistryItem()
		{
			return new RecruiterTestTypeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
