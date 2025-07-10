using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NumberGeneratorValueProviderTest : TestCase
	{
		public void TestAccessor()
		{
			NumberGenerator generator = new NumberGenerator();

			var accessor = new Mock<NumberGeneratorValueProvider.Accessor>();

			NumberGeneratorValueProvider provider = new NumberGeneratorValueProvider("key", accessor.Object);
			AssertEquals("key", provider.Key);

			accessor.Setup((m => m(generator, "detail"))).Returns("result");
			AssertEquals("result", provider.GetValue(generator, "detail"));

			accessor.VerifyAll();
		}
	}
}
