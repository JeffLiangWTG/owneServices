using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallRelatedParentActivityPivotCollection))]
	sealed class OrgSalesCallRelatedParentActivityPivotCollectionTest : RelatedActivityPivotCollectionTestCase<OrgSalesCallRelatedParentActivityPivotCollection>
	{
		public void TestCheckIsValidActivity()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			var collection = (OrgSalesCallRelatedParentActivityPivotCollection)communication.RelatedParentActivityPivotCollection;

			var validationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(validationResult, true);

			communication.LinkedInquiry = inquiry;
			validationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(validationResult, false, string.Format("{0} cannot have any additional relationships until client intelligence is set on its related inquiry ({1}).", communication.HumanReadableName, inquiry.HumanReadableName));

			validationResult = collection.CheckIsValidActivity(inquiry, true);
			AssertValidationResult(validationResult, true);
		}

		public void TestCheckCanRemoveActivity()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var collection = (OrgSalesCallRelatedParentActivityPivotCollection)communication.RelatedParentActivityPivotCollection;
			collection.AddNewPivot(opportunity);
			var validationResult = collection.CheckCanRemoveActivity(opportunity);
			AssertValidationResult(validationResult, true);

			communication.LinkedInquiry = inquiry;
			validationResult = collection.CheckCanRemoveActivity(inquiry);
			AssertValidationResult(validationResult, false, "Cannot remove the relationship between Communication (CM00001000) and Inquiry (I00001000) until the client intelligence is set on Inquiry (I00001000).");
		}

		public void TestCheckCanRemoveActivity_CrmOpportunity()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var mockCrmOpp = new Mock<IRelatableActivity>();

			Factory.Save();

			var collection = (OrgSalesCallRelatedParentActivityPivotCollection)communication.RelatedParentActivityPivotCollection;

			var validationResult = collection.CheckCanRemoveActivity(mockCrmOpp.Object);
			AssertValidationResult(validationResult, true);

			mockCrmOpp.Setup(m => m.TablePrefix).Returns(CrmOpportunitySchema.Constants.Prefix);
			validationResult = collection.CheckCanRemoveActivity(mockCrmOpp.Object);
			AssertValidationResult(validationResult, false, "Communications cannot be detached from GLOW opportunities");
		}

		#region Implementation

		protected override OrgSalesCallRelatedParentActivityPivotCollection GetCollectionToTest()
		{
			return new OrgSalesCallRelatedParentActivityPivotCollection(MasterOrgSalesCall);
		}

		OrgSalesCall MasterOrgSalesCall
		{
			get { return masterOrgSalesCall ?? (masterOrgSalesCall = Factory.NewWithValidTestData<OrgSalesCall>()); }
		}
		OrgSalesCall masterOrgSalesCall;

		#endregion
	}
}
