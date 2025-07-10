using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class MetadataProviderExtensionsFixture
	{
		[Test]
		public void IsKeyProperty()
		{
			var metadata = new Mock<IMetadataProvider>();
			metadata.Setup(x => x.GetKeys("AA")).Returns(new KeyProperty[] { new KeyProperty { Name = "BB" }, new KeyProperty { Name = "CC" } });
			Assert.IsTrue(metadata.Object.IsKeyProperty("AA", "BB"));
			Assert.IsFalse(metadata.Object.IsKeyProperty("AA", "DD"));
		}
	}
}
