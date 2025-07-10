using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Tests
{
	[TestFixture]
	class XmlWriterConfigurationTests
	{
		[Test]
		public void TestConfigurationTypes()
		{
			var config = new RefCusTariffXmlWriterConfiguration(true);
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusTariff)), "RefCusTariff");
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusTariffRelationship)), "RefCusTariffRelationship");
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusTariffAttribute)), "RefCusTariffAttribute");
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusRate)), "RefCusRate");
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusApplicability)), "RefCusApplicability");
			Assert.IsNotNull(config.GetConfiguration(typeof(RefCusExcludedTradeGroup)), "RefCusExcludedTradeGroup");
		}

		[Test]
		public void TestRefCusTariffConfiguration_Description()
		{
			var config = new RefCusTariffXmlWriterConfiguration(true);
			var tariffConfig = config.GetConfiguration(typeof(RefCusTariff));

			var property = typeof(RefCusTariff).GetProperty(nameof(RefCusTariff.ZZ1_Description));
			Assert.True(tariffConfig.IsIncluded(property));
			config = new RefCusTariffXmlWriterConfiguration(false);
			tariffConfig = config.GetConfiguration(typeof(RefCusTariff));
			Assert.False(tariffConfig.IsIncluded(property));
		}
	}
}
