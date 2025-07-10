using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class DescriptionAsMainContentListFixture
	{
		[TestCase(nameof(RefCusTradeGroupCountry), true)]
		[TestCase(nameof(RefCusCodeList), false)]
		public void Contains(string entityName, bool descriptionIsMainContent)
		{
			Assert.That(DescriptionAsMainContentList.Contains(entityName), Is.EqualTo(descriptionIsMainContent));
		}
	}
}
