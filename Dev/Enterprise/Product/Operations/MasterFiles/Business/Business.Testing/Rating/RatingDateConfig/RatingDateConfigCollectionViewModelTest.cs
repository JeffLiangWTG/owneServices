using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigCollectionViewModel))]
	public class RatingDateConfigCollectionViewModelTest : NonPersistentBusinessObjectCollectionTestCase<RatingDateConfigCollectionViewModel>
	{
		public void TestCollection()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);
			var ratingDateConfigCollectionViewModel = new RatingDateConfigCollectionViewModel(ratingDateConfigCollection, ChargeCodeGroupList.Codes.Freight);

			var ratingDateConfigViewModel = ratingDateConfigCollectionViewModel.AddNew();
			AssertEquals("ParentTableCode", OrgHeaderSchema.Constants.Prefix, ratingDateConfigViewModel.RatingDateConfigForTest.RDT_ParentTableCode);
			AssertEquals("ParentID", orgHeader.PK, ratingDateConfigViewModel.RatingDateConfigForTest.RDT_ParentID);
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, ratingDateConfigViewModel.RatingDateConfigForTest.RDT_GC_Company);
			AssertEquals("ChargeGroup", ChargeCodeGroupList.Codes.Freight, ratingDateConfigViewModel.RatingDateConfigForTest.RDT_ChargeGroup);
		}

		#region Implementation

		protected override RatingDateConfigCollectionViewModel GetCollectionToTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);

			return new RatingDateConfigCollectionViewModel(ratingDateConfigCollection, ChargeCodeGroupList.Codes.Freight);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var ratingDateConfig = Factory.NewWithValidTestData<RatingDateConfig>();
			return new RatingDateConfigViewModel(ratingDateConfig);
		}

		#endregion
	}
}
