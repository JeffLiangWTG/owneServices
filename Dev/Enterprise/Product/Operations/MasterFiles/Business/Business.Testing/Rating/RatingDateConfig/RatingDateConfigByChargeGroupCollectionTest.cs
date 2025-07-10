using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigByChargeGroupCollection))]
	public class RatingDateConfigByChargeGroupCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RatingDateConfigByChargeGroupCollection>
	{
		#region Implementation

		protected override RatingDateConfigByChargeGroupCollection GetCollectionToTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);
			return new RatingDateConfigByChargeGroupCollection(ratingDateConfigCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);
			return new RatingDateConfigByChargeGroup(ratingDateConfigCollection);
		}

		#endregion
	}
}
