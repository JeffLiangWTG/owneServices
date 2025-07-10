using System;
using CargoWise.RefDbRepo.Deployment.ParameterCreation;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	class ParameterCreationStrategyFactoryFixture
	{
		[Test]
		public void GetStrategy_WhenValidEnv_ReturnsCorrectStrategy()
		{
			var expectedStrategy = new[] { new TestEnvParameterCreationStrategy() };
			var actualStrategy = ParameterCreationStrategyFactory.GetStrategies("TEST");
			Assert.AreEqual(expectedStrategy.GetType(), actualStrategy.GetType());
		}

		[Test]
		public void GetStrategy_WhenInvalidEnv_ThrowsNotImplementedException()
		{
			Assert.Throws<NotImplementedException>(() => ParameterCreationStrategyFactory.GetStrategies("INVALID_ENV"));
		}
	}
}
