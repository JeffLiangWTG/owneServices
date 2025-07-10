using CargoWise.Application;
using Enterprise.Warehouse.Integration.Warehouse;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Integration.Testing
{
	class WhsFeatureNameExtensionsTest : TestCase
	{
		public void TestIsEnabled()
		{
			var enabledMock = new Mock<IWhsNonExposedFeature>();
			var someFeature = (WhsNonExposedFeatureName)(-1);
			enabledMock.Setup(s => s.Enabled(someFeature)).Returns(true);
			using (ObjectFactory.Substitute(enabledMock.Object))
			{
				AssertEquals(true, someFeature.IsEnabled());
			}

			var disabledMock = new Mock<IWhsNonExposedFeature>();
			disabledMock.Setup(s => s.Enabled(someFeature)).Returns(false);
			using (ObjectFactory.Substitute(disabledMock.Object))
			{
				AssertEquals(false, someFeature.IsEnabled());
			}
		}
	}
}
