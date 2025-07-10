using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NumberGeneratorProviderCollectionTest : TestCase
	{
		public void TestAdd()
		{
			var provider1 = new Mock<INumberGeneratorValueProvider>();
			var provider2 = new Mock<INumberGeneratorValueProvider>();

			provider1.Setup(m => m.Key).Returns("key1");
			provider2.Setup(m => m.Key).Returns("key2");
			var collection = new NumberGeneratorValueProviderCollection { provider1.Object };

			AssertEquals(1, collection.Count);
			AssertSame(provider1.Object, collection["key1"]);
			AssertNull(collection["key2"]);
		}

		public void TestAddRange()
		{
			var provider1 = new Mock<INumberGeneratorValueProvider>();
			var provider2 = new Mock<INumberGeneratorValueProvider>();

			provider1.Setup(m => m.Key).Returns("key1");
			provider2.Setup(m => m.Key).Returns("key2");
			var collection = new NumberGeneratorValueProviderCollection();
			collection.AddRange(new[] { provider1.Object, provider2.Object });

			AssertEquals(2, collection.Count);
			AssertSame(provider1.Object, collection["key1"]);
			AssertSame(provider2.Object, collection["key2"]);
			AssertNull(collection["key3"]);
		}
	}
}
