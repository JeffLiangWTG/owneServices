using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigByChargeGroup))]
	public class RatingDateConfigByChargeGroupTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRatingDateConfigByChargeGroup()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);

			var ratingDateConfig1 = ratingDateConfigCollection.AddNew();
			ratingDateConfig1.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig1.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;

			var ratingDateConfig2 = ratingDateConfigCollection.AddNew();
			ratingDateConfig2.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ratingDateConfig2.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;

			var ratingDateConfigByChargeGroup = new RatingDateConfigByChargeGroup(ratingDateConfigCollection);
			ratingDateConfigByChargeGroup.ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			AssertEquals("ChargeGroup", ChargeCodeGroupList.Codes.Freight, ratingDateConfigByChargeGroup.ChargeGroup);

			AssertContainsExactElementsInAnyOrder
			(
				expected: new[]
				{
					"FCN",
				},
				actual: ratingDateConfigByChargeGroup.ChargeGroupSettings.Select(y => $"{y.JobType}")
			);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var ratingDateConfigCollection = new RatingDateConfigCollection(orgHeader);
			return new RatingDateConfigByChargeGroup(ratingDateConfigCollection);
		}

		#endregion
	}
}
