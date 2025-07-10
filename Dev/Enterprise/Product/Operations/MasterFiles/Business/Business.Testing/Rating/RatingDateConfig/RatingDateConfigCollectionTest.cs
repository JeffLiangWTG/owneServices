using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigCollection))]
	public class RatingDateConfigCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParentTableCode()
		{
			var ratingDateConfigCollection = GetCollectionToTest();
			var ratingDateConfig = (RatingDateConfig)ratingDateConfigCollection.AddNew();
			AssertEquals("Parent Table Code", OrgHeaderSchema.Constants.Prefix, ratingDateConfig.RDT_ParentTableCode);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			return new RatingDateConfigCollection(orgHeader);
		}

		#endregion
	}
}
