using System.Collections.Generic;
using CargoWise.RefDbRepo.VNReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	[TestFixture]
	public class FindMetadataItemFixture
	{
		[Test]
		public void TestCannotFindMetadataItem()
		{
			var metadata = new CodeListMetadata
			{
				DataItems = new List<CodeListMetadataItem>
				{
					new CodeListMetadataItem { Id = 1 }, new CodeListMetadataItem { Id = 2 }
				}
			};
			Assert.Throws<MetadataItemNotFoundException>(() =>
				MetadataHelper.FindMetadataItem(metadata, ApplicationConfig.CustomsOfficeMetadataId));
		}

		[Test]
		public void TestFindMetadataItem()
		{
			var metadata = new CodeListMetadata
			{
				DataItems = new List<CodeListMetadataItem>
				{
					new CodeListMetadataItem { Id = 1 },
					new CodeListMetadataItem { Id = 2 },
					new CodeListMetadataItem { Id = ApplicationConfig.CustomsOfficeMetadataId },
				}
			};
			var metadataItem = MetadataHelper.FindMetadataItem(metadata, ApplicationConfig.CustomsOfficeMetadataId);
			Assert.IsNotNull(metadataItem);
			Assert.That(metadataItem.Id, Is.EqualTo(ApplicationConfig.CustomsOfficeMetadataId));
		}
	}
}
