using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.ServiceHost.NetCore;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	[TestFixture]
	class DummyWithDependencyControllerTest : TestCaseWithFactory
	{
		[Test]
		public void DummyControllerGetNumberWithDependencies()
		{
			const int input1 = 42;
			const int input2 = 1337;

			var dependency1Mock = new Mock<IDummyDependancy1>();
			dependency1Mock.Setup(d => d.GetDummyNumber()).Returns(input1);

			var dependency2Mock = new Mock<IDummyDependancy2>();
			dependency2Mock.Setup(d => d.GetDummyNumber()).Returns(input2);

			var controllerBuilder = new ControllerBuilder<DummyWithDependencyController>();
			var controller = controllerBuilder
				.AddDependency(dependency1Mock.Object)
				.AddDependency(dependency2Mock.Object)
				.BuildController();

			var expectedOutput = (input1 + input2).ToString();

			controller.GetDummyNumber()
				.AssertResultEquals(System.Net.HttpStatusCode.OK, expectedOutput);
		}

		[Test]
		public void DummyControllerGetStringWithDependencies()
		{
			const string input1 = "Hello";
			const string input2 = "World";

			var dependency1Mock = new Mock<IDummyDependancy1>();
			dependency1Mock.Setup(d => d.GetDummyString()).Returns(input1);

			var dependency2Mock = new Mock<IDummyDependancy2>();
			dependency2Mock.Setup(d => d.GetDummyString()).Returns(input2);

			var controllerBuilder = new ControllerBuilder<DummyWithDependencyController>();
			var controller = controllerBuilder
				.AddDependency(dependency1Mock.Object)
				.AddDependency(dependency2Mock.Object)
				.BuildController();

			var expectedOutput = $"{input1} | {input2}";

			controller.GetDummyString()
				.AssertResultEquals(System.Net.HttpStatusCode.OK, expectedOutput);
		}

		[Test]
		public void DummyControllerMissingDependencies()
		{
			var dependency1Mock = new Mock<IDummyDependancy1>();
			dependency1Mock.Setup(d => d.GetDummyString());

#if NET
			var exception = AssertExceptionThrown<InvalidOperationException>("The DI container should have failed due to missing dependencies.", () =>
			{
				var controllerBuilder = new ControllerBuilder<DummyWithDependencyController>();
				var controller = controllerBuilder
					.AddDependency(dependency1Mock.Object)
					.BuildController();
			});
			CombineAssertions(() =>
			{
				AssertEquals("Microsoft.Extensions.DependencyInjection", exception.Source);
				AssertContains((typeof(IDummyDependancy2)).FullName, exception.Message);
			});
#elif NETFRAMEWORK
			//Framework isn't doing real dependency injection, so it does not report which dependency was missing.
			var exception = AssertExceptionThrown<MissingMethodException>("The DI container should have failed due to missing dependencies.", () =>
			{
				var controllerBuilder = new ControllerBuilder<DummyWithDependencyController>();
				var controller = controllerBuilder
					.AddDependency(dependency1Mock.Object)
					.BuildController();
			});
#endif

		}
	}
}
