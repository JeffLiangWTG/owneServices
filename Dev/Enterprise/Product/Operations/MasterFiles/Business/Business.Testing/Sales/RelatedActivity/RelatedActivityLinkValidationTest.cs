using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RelatedActivityLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestToActivityTypeForBinding()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = opportunity;

			var link = RelatedActivityLink.Get(pivot, opportunity);

			link.ToActivityTypeForBinding = "";
			AssertMandatoryValidationError(link.ToActivityTypeForBindingInfo, true);
			AssertListValidationInvalidCodeError(link.ToActivityTypeForBindingInfo, false);

			link.ToActivityTypeForBinding = "XXX";
			AssertMandatoryValidationError(link.ToActivityTypeForBindingInfo, false);
			AssertListValidationInvalidCodeError(link.ToActivityTypeForBindingInfo, true);

			link.ToActivityTypeForBinding = RelatableActivityTypeList.Codes.Communication;
			AssertMandatoryValidationError(link.ToActivityTypeForBindingInfo, false);
			AssertListValidationInvalidCodeError(link.ToActivityTypeForBindingInfo, false);
		}

		public void TestToActivityIDForBinding()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var collection = opportunity.RelatedChildActivityPivotCollection;

			var pivot1 = collection.AddNew();
			pivot1.ChildActivity = opportunity;
			pivot1.ParentActivity = inquiry;
			var link1 = RelatedActivityLink.Get(pivot1, opportunity);
			AssertNoErrors(link1.ToActivityIDForBindingInfo);

			var pivot2 = collection.AddNew();
			pivot2.ChildActivity = opportunity;
			pivot2.ParentActivity = inquiry;
			var link2 = RelatedActivityLink.Get(pivot2, opportunity);

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();
			AssertHasError(link1.ToActivityIDForBindingInfo, "Duplicate related activity already exists.");
			AssertHasError(link2.ToActivityIDForBindingInfo, "Duplicate related activity already exists.");
		}

		public void TestToActivityIDForBinding_ShouldCheckDuplicateErrorBetweenID1and2()
		{
			var opportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var inquiry = (IRelatableActivity)Factory.New<SalesEnquiry>();
			var collection = opportunity.RelatedChildActivityPivotCollection;

			var pivot1 = (ViewRelatedActivityPivot)collection.AddNew();
			pivot1.ParentActivity = inquiry;
			pivot1.ChildActivity = opportunity;
			var link1 = RelatedActivityLink.Get(pivot1, opportunity);
			AssertNoErrors(pivot1.RAP_ChildActivityIDInfo);

			var pivot2 = collection.AddNew();
			pivot2.ChildActivity = inquiry;
			var link2 = RelatedActivityLink.Get(pivot2, inquiry);

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();
			AssertHasError(link1.ToActivityIDForBindingInfo, "Duplicate related activity already exists.");
			AssertHasError(link2.ToActivityIDForBindingInfo, "Duplicate related activity already exists.");
		}
	}
}
