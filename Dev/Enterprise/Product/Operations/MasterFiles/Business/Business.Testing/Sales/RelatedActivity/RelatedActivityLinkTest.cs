using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedActivityLink))]
	sealed class RelatedActivityLinkTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		#region FromActivity

		public void TestFromActivityType()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(inquiry.ActivityType, link1.FromActivityType);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(opportunity.ActivityType, link2.FromActivityType);
		}

		public void TestFromActivityID()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(inquiry.PK, link1.FromActivityID);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(opportunity.PK, link2.FromActivityID);
		}

		public void TestFromActivityCode()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_LeadUniqueReference = "I01002823";

			Factory.Save();

			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(inquiry.O1_LeadUniqueReference, link1.FromActivityCode);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(opportunity.P8_OpportunityID, link2.FromActivityCode);
		}

		#endregion

		#region ToActivity

		public void TestToActivityType()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(opportunity.ActivityType, link1.ToActivityType);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(inquiry.ActivityType, link2.ToActivityType);
		}

		public void TestToActivityTypeForBinding_ReadOnly()
		{
			var opportunity = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.NewWithValidTestData<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(false, link.ToActivityTypeForBindingInfo.ReadOnly);

			Factory.Save();
			AssertEquals(true, link.ToActivityTypeForBindingInfo.ReadOnly);
		}

		public void TestToActivityID()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(opportunity.PK, link1.ToActivityID);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(inquiry.PK, link2.ToActivityID);
		}

		public void TestToActivityCode()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_LeadUniqueReference = "I01002823";

			Factory.Save();

			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;
			pivot.ChildActivity = inquiry;

			var link1 = RelatedActivityLink.Get(pivot, inquiry);
			AssertEquals(opportunity.P8_OpportunityID, link1.ToActivityCode);

			var link2 = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(inquiry.O1_LeadUniqueReference, link2.ToActivityCode);
		}

		public void TestToActivityIDForBinding_ReadOnly()
		{
			var opportunity = (IRelatableActivity)Factory.NewWithValidTestData<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.NewWithValidTestData<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;

			var link = RelatedActivityLink.Get(pivot, opportunity);
			AssertEquals(true, link.ToActivityIDForBindingInfo.ReadOnly);

			link.ToActivityTypeForBinding = "XXX";
			AssertEquals(true, link.ToActivityIDForBindingInfo.ReadOnly);

			link.ToActivityTypeForBinding = inquiry.ActivityType;
			link.ToActivityIDForBinding = inquiry.PK;
			AssertEquals(false, link.ToActivityIDForBindingInfo.ReadOnly);

			Factory.Save();
			AssertEquals(true, link.ToActivityIDForBindingInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region Validation

		public void TestRunPreSaveValidation()
		{
			var communication = Factory.New<OrgSalesCall>();
			var pivot = communication.RelatedChildActivityPivotCollection.AddNew();

			var link = RelatedActivityLink.Get(pivot, communication);
			link.RunPreSaveValidation();

			AssertEquals("HasErrors", true, link.HasErrors);
		}

		#endregion

		#region Delete

		public void TestCanDelete()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			var inquiryNewPivot = inquiry.RelatedChildActivityPivotCollection.AddNew();
			var inquiryOppPivot = inquiry.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			communication.LinkedInquiry = inquiry;
			var inquiryComPivot = inquiry.RelatedChildActivityPivotCollection.FindPivot(communication);

			var inquiryNewLink = RelatedActivityLink.Get(inquiryNewPivot, inquiry);
			AssertEquals(true, inquiryNewLink.CanDelete);

			var inquiryOppLink = RelatedActivityLink.Get(inquiryOppPivot, inquiry);
			AssertEquals(true, inquiryOppLink.CanDelete);

			var inquiryComLink = RelatedActivityLink.Get(inquiryComPivot, inquiry);
			AssertEquals(false, inquiryComLink.CanDelete);
			AssertEquals("Cannot remove the relationship between Communication and Inquiry until the client intelligence is set on Inquiry.", inquiryComLink.ReasonForNotAbleToDelete);

			var comInquiryLink = RelatedActivityLink.Get(inquiryComPivot, communication);
			AssertEquals(false, comInquiryLink.CanDelete);
			AssertEquals("Cannot remove the relationship between Communication and Inquiry until the client intelligence is set on Inquiry.", comInquiryLink.ReasonForNotAbleToDelete);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var inquiry = Factory.New<SalesEnquiry>();
			var pivot = ViewRelatedActivityPivot.Create(Factory, opportunity, inquiry);
			return RelatedActivityLink.Get(pivot, opportunity);
		}

		#endregion
	}
}
