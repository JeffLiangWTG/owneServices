using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class EIDOMessageBuilderFactoryTest : TestCase
	{
		internal void TestGetBuilder()
		{
			IEIDOMessageBuilder builder = EIDOMessageBuilderFactory.GetNewBuilder();
			AssertNotNull("should return a builder", builder);
			AssertEquals("should return the correct builder", typeof(EIDOEdifactMessageBuilder), builder.GetType());
		}
	}
}
