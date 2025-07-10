using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedParentActivityPivotCollection))]
	sealed class RelatedParentActivityPivotCollectionTest : RelatedActivityPivotCollectionTestCase<RelatedParentActivityPivotCollection>
	{
		public void TestCheckIsValidActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			var collection = inquiry.RelatedParentActivityPivotCollection;

			var parentValidationResult = collection.CheckIsValidActivity(inquiry, true);
			AssertValidationResult(parentValidationResult, false, "Can not make Inquiry the parent of itself.");

			parentValidationResult = collection.CheckIsValidActivity(opportunity, false);
			AssertValidationResult(parentValidationResult, true);

			parentValidationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(parentValidationResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) must be saved before it can be a parent of another activity.");

			Factory.Save();
			parentValidationResult = collection.CheckIsValidActivity(opportunity, true);
			AssertValidationResult(parentValidationResult, true);

			parentValidationResult = collection.CheckIsValidActivity(call, true);
			AssertValidationResult(parentValidationResult, true);

			parentValidationResult = collection.CheckIsValidActivity(null, true);
			AssertValidationResult(parentValidationResult, true);

			inquiry.RelatedChildActivityPivotCollection.AddActivity(opportunity);
			opportunity.RelatedChildActivityPivotCollection.AddActivity(call);
			parentValidationResult = collection.CheckIsValidActivity(call, true);
			AssertValidationResult(parentValidationResult, false,
@"Communication (CM00001000) is already a related descendant of Inquiry (I00001000).
Making Communication (CM00001000) the parent of Inquiry (I00001000) would create an illegal cycle.

Conflicting relationship sequence: Inquiry (I00001000) > Opportunity (AB02411OFUMXTD06DZ8H) > Communication (CM00001000)");

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.RelatedChildActivityPivotCollection.AddNewPivot(call);
			Factory.Save();

			parentValidationResult = collection.CheckIsValidActivity(opportunity2, true);
			AssertValidationResult(parentValidationResult, true, ZString.Empty);
		}

		public void TestAddActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var collection1 = inquiry.RelatedParentActivityPivotCollection;
			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection2 = opportunity.RelatedParentActivityPivotCollection;

			var addParentResult = collection1.AddActivity(opportunity);
			AssertUpdateResult("Should not be able to add relationship to unsaved parent, if parent ends up not being saved, the relationship will be invalid!", addParentResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) must be saved before it can be a parent of another activity.");

			addParentResult = collection2.AddActivity(inquiry);
			AssertUpdateResult("It is ok for an unsaved activity to add a relationship to a saved parent - the relationship will only remain if the activity is saved.", addParentResult, true);
			AssertContainsExactElementsInAnyOrder(new[] { inquiry.PK }, ViewRelatedActivityPivot.LoadPivotsWithChild(Factory, opportunity).Select(pivot => pivot.RAP_ParentActivityID));
		}

		public void TestRemoveActivity()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var inquiryParentActivityCollection = new RelatedParentActivityPivotCollection(inquiry);
			var removeParentResult = inquiryParentActivityCollection.RemoveActivity(opportunity);
			AssertUpdateResult(removeParentResult, false, "Opportunity (AB02411OFUMXTD06DZ8H) is not a parent of Inquiry (I00001000).");

			inquiryParentActivityCollection.AddNewPivot(opportunity);
			removeParentResult = inquiryParentActivityCollection.RemoveActivity(opportunity);
			AssertUpdateResult(removeParentResult, true);
		}

		public void TestDefaultsForNewElement()
		{
			var parentOpportunity = (IRelatableActivity)Factory.New<OrgOpportunity>();
			var pivot = new RelatedParentActivityPivotCollection(parentOpportunity).AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("RAP_ParentActivityTableCode", "", pivot.RAP_ParentActivityTableCode);
				AssertEquals("RAP_ParentActivityID", ZGuid.Empty, pivot.RAP_ParentActivityID);
				AssertEquals("RAP_ChildActivityTableCode", parentOpportunity.TablePrefix, pivot.RAP_ChildActivityTableCode);
				AssertEquals("RAP_ChildActivityID", parentOpportunity.PK, pivot.RAP_ChildActivityID);
			});
		}

		#region RelinkRelatedSuperAndSubActivities

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSuperActivityRelationshipWithSubActivityWhenMatchExists()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			Factory.Save();

			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedParentActivityPivotCollectionForTest(childOpportunity);
			collection.AddNewPivot(dummySuperActivity);
			dummySuperActivity.StubMatchingSubActivityForTesting(childOpportunity, dummySubActivity2);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Client has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySuperActivity relationship with dummySubActivity2", new[] { dummySubActivity2 }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSubActivityRelationshipWithSuperActivityWhenNoMatchExists()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity1.SuperActivity = dummySuperActivity;
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity2.SuperActivity = dummySuperActivity;
			Factory.Save();

			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedParentActivityPivotCollectionForTest(childOpportunity);
			collection.AddNewPivot(dummySubActivity2);
			dummySuperActivity.StubMatchingSubActivityForTesting(childOpportunity, null);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Client has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySubActivity2 relationship with dummySuperActivity", new[] { dummySuperActivity }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_ReplaceSubActivityRelationshipWithNewSubActivityWhenNewMatch()
		{
			var dummySuperActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var dummySubActivity1 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity1.SuperActivity = dummySuperActivity;
			var dummySubActivity2 = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			dummySubActivity2.SuperActivity = dummySuperActivity;
			Factory.Save();

			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedParentActivityPivotCollectionForTest(childOpportunity);
			collection.AddNewPivot(dummySubActivity2);
			dummySuperActivity.StubMatchingSubActivityForTesting(childOpportunity, dummySubActivity1);

			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should have tried to relink super and subactivities because Client has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
			AssertContainsExactElementsInAnyOrder("Should have replaced dummySubActivity2 relationship with dummySubActivity1", new[] { dummySubActivity1 }, collection.Activities);
		}

		public void TestRelinkRelatedSuperAndSubActivities_RelinkOnlyWhenNotInDatabaseOrContactOrHeaderHasChanged()
		{
			var childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var collection = new RelatedParentActivityPivotCollectionForTest(childOpportunity);

			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity not in database", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			Factory.Save();

			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should not try to relink super and subactivities when opportunity in database and Client and Contact has not changed", 0, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			childOpportunity.P8_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity Contact has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);

			Factory.Save();

			childOpportunity.P8_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection.TotalRelinkRelatedSuperAndSubActivitiesCalls = 0;
			collection.RelinkRelatedSuperAndSubActivities();
			AssertEquals("Should try to relink super and subactivities when opportunity Header has changed", 1, collection.TotalRelinkRelatedSuperAndSubActivitiesCalls);
		}

		#endregion

		#region Implementation

		protected override RelatedParentActivityPivotCollection GetCollectionToTest()
		{
			return new RelatedParentActivityPivotCollection(ChildOpportunity);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var inquiry = (IRelatableActivity)Factory.NewWithValidTestData<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = inquiry;
			pivot.ChildActivity = ChildOpportunity;

			return pivot;
		}

		IRelatableActivity childOpportunity;
		IRelatableActivity ChildOpportunity
		{
			get
			{
				return childOpportunity ?? (childOpportunity = Factory.NewWithValidTestData<OrgOpportunity>());
			}
		}

		#endregion
	}
}
