using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedChildActivityPivotCollection))]
	sealed class RelatedChildActivityPivotCollectionTest : RelatedActivityPivotCollectionTestCase<RelatedChildActivityPivotCollection>
	{
		public void TestCheckIsValidActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			var collection = inquiry.RelatedChildActivityPivotCollection;

			var childValidationResult = collection.CheckIsValidActivity(inquiry, true);
			AssertValidationResult(childValidationResult, false, "Can not make Inquiry a child of itself.");

			childValidationResult = collection.CheckIsValidActivity(opportunity, false);
			AssertValidationResult(childValidationResult, true);

			childValidationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(childValidationResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) must be saved before it can be a child of another activity.");

			Factory.Save();
			childValidationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(childValidationResult, true);

			inquiry.RelatedParentActivityPivotCollection.AddActivity(opportunity);
			opportunity.RelatedParentActivityPivotCollection.AddActivity(call);
			childValidationResult = collection.CheckIsValidActivity(call, true);
			AssertValidationResult(childValidationResult, false,
@"Communication (CM00001000) is already a related ancestor of Inquiry (I00001000).
Making Communication (CM00001000) the child of Inquiry (I00001000) would create an illegal cycle.

Conflicting relationship sequence: Communication (CM00001000) > Opportunity (AB02411OFUMXTD06DZ8H) > Inquiry (I00001000)");

			var campaign = Factory.New<Integration.IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var activity = ((IRelatableActivity)campaign);
			activity.RelatedParentActivityPivotCollection.AddNewPivot(call);
			Factory.Save();

			childValidationResult = collection.CheckIsValidActivity(activity, true);
			AssertValidationResult(childValidationResult, true, ZString.Empty);
		}

		public void TestCheckIsValidActivity_WhenInvalid()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			var collection = inquiry.RelatedChildActivityPivotCollection;

			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Value.RemoveAll();
			var childValidationResult = collection.CheckIsValidActivity(opportunity, false);
			string expectedReason = @"Inquiry cannot have children of activity type 'Opportunity Manager'.
This rule is defined in the registry item [Sales & Marketing/Sales Relations/Sales Relation Direction Rules].

No additional children are allowed.";
			AssertValidationResult(childValidationResult, false, expectedReason);
		}

		public void TestAddActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var collection1 = inquiry.RelatedChildActivityPivotCollection;
			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection2 = opportunity.RelatedChildActivityPivotCollection;

			var addChildResult = collection1.AddActivity(opportunity);
			AssertUpdateResult("Should not be able to add relationship to unsaved child, if child ends up not being saved, the relationship will be invalid!", addChildResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) must be saved before it can be a child of another activity.");

			addChildResult = collection2.AddActivity(inquiry);
			AssertUpdateResult("It is ok for an unsaved activity to add a relationship to a saved child - the relationship will only remain if the activity is saved.", addChildResult, true);
			AssertContainsExactElementsInAnyOrder(new[] { inquiry.PK }, ViewRelatedActivityPivot.LoadPivotsWithParent(Factory, opportunity).Select(pivot => pivot.RAP_ChildActivityID));

			addChildResult = collection2.AddActivity(inquiry);
			AssertUpdateResult(addChildResult, false, "Inquiry (I00001000) is already a child of Opportunity (AB02411OFUMXTD06DZ8H).");
		}

		public void TestRemoveActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var inquiryChildActivityCollection = new RelatedChildActivityPivotCollection(inquiry);
			var removeChildResult = inquiryChildActivityCollection.RemoveActivity(opportunity);
			AssertUpdateResult(removeChildResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) is not a child of Inquiry (I00001000).");

			inquiryChildActivityCollection.AddNewPivot(opportunity);
			removeChildResult = inquiryChildActivityCollection.RemoveActivity(opportunity);
			AssertUpdateResult(removeChildResult, true);
		}

		public void TestDefaultsForNewElement()
		{
			var parentOpportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var pivot = new RelatedChildActivityPivotCollection(parentOpportunity).AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("RAP_ParentActivityTableCode", parentOpportunity.TablePrefix, pivot.RAP_ParentActivityTableCode);
				AssertEquals("RAP_ParentActivityID", parentOpportunity.PK, pivot.RAP_ParentActivityID);
				AssertEquals("RAP_ChildActivityTableCode", "", pivot.RAP_ChildActivityTableCode);
				AssertEquals("RAP_ChildActivityID", ZGuid.Empty, pivot.RAP_ChildActivityID);
			});
		}

		public void TestHasDescendant()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			inquiry.RelatedChildActivityPivotCollection.AddNewPivot(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddNewPivot(call);

			AssertEquals(false, inquiry.RelatedChildActivityPivotCollection.HasDescendant(inquiry));
			AssertEquals(true, inquiry.RelatedChildActivityPivotCollection.HasDescendant(opportunity));
			AssertEquals(true, inquiry.RelatedChildActivityPivotCollection.HasDescendant(call));

			AssertEquals(false, opportunity.RelatedChildActivityPivotCollection.HasDescendant(inquiry));
			AssertEquals(false, opportunity.RelatedChildActivityPivotCollection.HasDescendant(opportunity));
			AssertEquals(true, opportunity.RelatedChildActivityPivotCollection.HasDescendant(call));

			AssertEquals(false, call.RelatedChildActivityPivotCollection.HasDescendant(inquiry));
			AssertEquals(false, call.RelatedChildActivityPivotCollection.HasDescendant(opportunity));
			AssertEquals(false, call.RelatedChildActivityPivotCollection.HasDescendant(call));
		}

		#region RelinkRelatedSuperAndSubActivities

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSuperActivityRelationshipWithSubActivityWhenMatchExists()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			Factory.Save();

			var parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedChildActivityPivotCollectionForTest(parentOpportunity);
			collection.AddNewPivot(dummySuperActivity);
			dummySuperActivity.StubMatchingSubActivityForTesting(parentOpportunity, dummySubActivity1);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Contact has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySuperActivity relationship with dummySubActivity1", new[] { dummySubActivity1 }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSubActivityRelationshipWithSuperActivityWhenNoMatchExists()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity1.SuperActivity = dummySuperActivity;
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity2.SuperActivity = dummySuperActivity;
			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedChildActivityPivotCollectionForTest(parentOpportunity);
			collection.AddNewPivot(dummySubActivity1);
			dummySuperActivity.StubMatchingSubActivityForTesting(parentOpportunity, null);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Contact has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySubActivity1 relationship with dummySuperActivity", new[] { dummySuperActivity }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSubActivityRelationshipWithNewSubActivityWhenNewMatch()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity1.SuperActivity = dummySuperActivity;
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity2.SuperActivity = dummySuperActivity;
			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedChildActivityPivotCollectionForTest(parentOpportunity);
			collection.AddNewPivot(dummySubActivity1);
			dummySuperActivity.StubMatchingSubActivityForTesting(parentOpportunity, dummySubActivity2);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Contact has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySubActivity1 relationship with dummySubActivity2", new[] { dummySubActivity2 }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_RelinkOnlyWhenNotInDatabaseOrContactOrHeaderHasChanged()
		{
			var parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedChildActivityPivotCollectionForTest(parentOpportunity);

			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity not in database", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			Factory.Save();

			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should not try to relink super and subactivities when opportunity in database and Client and Contact has not changed", 0, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			parentOpportunity.P8_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity Contact has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			Factory.Save();

			parentOpportunity.P8_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity Header has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		protected override RelatedChildActivityPivotCollection GetCollectionToTest()
		{
			return new RelatedChildActivityPivotCollection(ParentOpportunity);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var inquiry = (IRelatableActivity)Factory.NewWithValidTestData<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = ParentOpportunity;
			pivot.ChildActivity = inquiry;

			return pivot;
		}

		IRelatableActivity parentOpportunity;
		IRelatableActivity ParentOpportunity
		{
			get
			{
				return parentOpportunity ?? (parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>());
			}
		}

		#endregion
	}
}
