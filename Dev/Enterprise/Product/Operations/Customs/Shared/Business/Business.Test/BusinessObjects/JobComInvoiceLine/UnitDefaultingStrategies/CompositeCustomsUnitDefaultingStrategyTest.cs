using System;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	class CompositeCustomsUnitDefaultingStrategyTest : CustomsUnitDefaultingStrategyTest
	{
		public void TestConstructor()
		{
#if NET
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null strategies", "Value cannot be null. (Parameter 'strategies')",
					() => new CompositeCustomsUnitDefaultingStrategy(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("Empty strategies",
					"Value '0' cannot be less than 2. (Parameter 'strategies.Length')", () => new CompositeCustomsUnitDefaultingStrategy());
				AssertExceptionThrown<ArgumentOutOfRangeException>("One strategy",
					"Value '1' cannot be less than 2. (Parameter 'strategies.Length')",
					() => new CompositeCustomsUnitDefaultingStrategy(Mock.Of<ICustomsUnitDefaultingStrategy>()));
			});
#else
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null strategies", "Value cannot be null.\r\nParameter name: strategies",
					() => new CompositeCustomsUnitDefaultingStrategy(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("Empty strategies",
					"Value '0' cannot be less than 2.\r\nParameter name: strategies.Length", () => new CompositeCustomsUnitDefaultingStrategy());
				AssertExceptionThrown<ArgumentOutOfRangeException>("One strategy",
					"Value '1' cannot be less than 2.\r\nParameter name: strategies.Length",
					() => new CompositeCustomsUnitDefaultingStrategy(Mock.Of<ICustomsUnitDefaultingStrategy>()));
			});
#endif
		}

		public override void TestDefaultUOMs()
		{
			var strategy1 = new Mock<ICustomsUnitDefaultingStrategy>();
			var strategy2 = new Mock<ICustomsUnitDefaultingStrategy>();

			var compositeStrategy = new CompositeCustomsUnitDefaultingStrategy(strategy1.Object, strategy2.Object);
			compositeStrategy.DefaultUOMs(Factory.New<BaseJobComInvoiceLine>());
			strategy1.Verify(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy1 has not been called");
			strategy2.Verify(x => x.DefaultUOMs(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy2 has not been called");
			Assert(true);
		}

		public void TestInitialise()
		{
			var strategy1 = new Mock<ICustomsUnitDefaultingStrategy>();
			var strategy2 = new Mock<ICustomsUnitDefaultingStrategy>();

			var compositeStrategy = new CompositeCustomsUnitDefaultingStrategy(strategy1.Object, strategy2.Object);
			compositeStrategy.Initialise(Factory.New<BaseJobComInvoiceLine>());
			strategy1.Verify(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy1 has not been called");
			strategy2.Verify(x => x.Initialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy2 has not been called");
			Assert(true);
		}

		public void TestDeinitialise()
		{
			var strategy1 = new Mock<ICustomsUnitDefaultingStrategy>();
			var strategy2 = new Mock<ICustomsUnitDefaultingStrategy>();

			var compositeStrategy = new CompositeCustomsUnitDefaultingStrategy(strategy1.Object, strategy2.Object);
			compositeStrategy.Deinitialise(Factory.New<BaseJobComInvoiceLine>());
			strategy1.Verify(x => x.Deinitialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy1 has not been called");
			strategy2.Verify(x => x.Deinitialise(It.IsAny<BaseJobComInvoiceLine>()), Times.Once, "Strategy2 has not been called");
			Assert(true);
		}
	}
}
