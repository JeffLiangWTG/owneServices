using CargoWise.Application;
using Enterprise.Warehouse.Integration.Warehouse;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Integration
{
	[CodeAlive("No non-exposed features in production code currently.")]
	public static class WhsFeatureNameExtensions
	{
		public static bool IsEnabled(this WhsNonExposedFeatureName featureName)
		{
			return ObjectFactory.Get<IWhsNonExposedFeature>().Enabled(featureName);
		}
	}
}
