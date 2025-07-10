using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(HRGlbCompanyCampaignCollection))]
	sealed class HRGlbCompanyCampaignCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HRGlbCompanyCampaignCollection(Factory);
		}
	}
}
