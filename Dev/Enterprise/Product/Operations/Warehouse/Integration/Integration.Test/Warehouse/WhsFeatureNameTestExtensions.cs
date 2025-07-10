using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration.Warehouse;
using Moq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Integration.Testing
{
	[CodeAlive("No non-exposed features in production code currently.")]
	public static class WhsFeatureNameTestExtensions
	{
		public static IDisposable StubFeature(this WhsNonExposedFeatureName featureName, bool enabled)
		{
			var mock = new Mock<IWhsNonExposedFeature>();
			mock.Setup(s => s.Enabled(featureName)).Returns(enabled);
			return ObjectFactory.Substitute(mock.Object);
		}
	}
}
