using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallRelatedChildActivityPivotCollection))]
	sealed class OrgSalesCallRelatedChildActivityPivotCollectionTest : RelatedActivityPivotCollectionTestCase<OrgSalesCallRelatedChildActivityPivotCollection>
	{
		public void TestCheckIsValidActivity()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			var collection = (OrgSalesCallRelatedChildActivityPivotCollection)communication.RelatedChildActivityPivotCollection;

			var validationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(validationResult, true);

			communication.LinkedInquiry = inquiry;
			validationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(validationResult, false, string.Format("{0} cannot have any additional relationships until client intelligence is set on its related inquiry ({1}).", communication.HumanReadableName, inquiry.HumanReadableName));
		}

		#region Implementation

		protected override OrgSalesCallRelatedChildActivityPivotCollection GetCollectionToTest()
		{
			return new OrgSalesCallRelatedChildActivityPivotCollection(MasterOrgSalesCall);
		}

		OrgSalesCall MasterOrgSalesCall
		{
			get { return masterOrgSalesCall ?? (masterOrgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>()); }
		}
		OrgSalesCall masterOrgSalesCall;

		#endregion
	}
}
