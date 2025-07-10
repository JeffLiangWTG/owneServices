using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RelatedActivityLinkCollection))]
	sealed class RelatedActivityLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RelatedActivityLinkCollection>
	{
		public void TestConstructor()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			ParentOpportunity.RelatedChildActivityPivotCollection.AddActivity(inquiry);

			var collection = new RelatedActivityLinkCollection(ParentOpportunity);
			AssertEquals(1, collection.Count);
			AssertEquals(inquiry, collection[0].ToActivityForBinding);
		}

		public void TestLinksAreAutomaticallyAddedWhenAPivotIsAdded()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var linkCollection = new RelatedActivityLinkCollection(inquiry);
			AssertEquals("Precondition", 0, linkCollection.Count);

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.RelatedParentActivityPivotCollection.AddActivity(inquiry);

			AssertEquals("Precondition", 1, linkCollection.Count);
			AssertEquals(inquiry, linkCollection[0].FromActivity);
			AssertEquals(communication, linkCollection[0].ToActivityForBinding);
		}

		public void TestLinksAreAutomaticallyAddedWhenAPivotIsAdded_WithAnotherLinkBeingRemovedDuringRefresh()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communication = org.SalesCalls.AddNew();
			Factory.Save();

			var linkCollection = new RelatedActivityLinkCollection(communication);
			var uncommittedLink = linkCollection.AddNew();
			((IBindingList)linkCollection).ListChanged += (sender, e) =>
				{
					if (linkCollection.Contains(uncommittedLink))
					{
						linkCollection.RemoveAndDelete(uncommittedLink);
					}
				};

			var otherFactory = new BusinessObjectFactory();
			var opportunity = otherFactory.NewWithValidTestData<OrgOpportunity>();
			opportunity.RelatedParentActivityPivotCollection.AddNewPivot(communication);
			otherFactory.Save();

			AssertEquals(1, linkCollection.Count);
			var link = linkCollection[0];
			AssertEquals(communication.PK, link.FromActivity.PK);
			AssertEquals(opportunity.PK, link.ToActivityForBinding.PK);
		}

		public void TestOnlyAddOneLinkWhenThereIsARelatedActivityPivotToItself()
		{
			var parentPivot = (ViewRelatedActivityPivot)ParentOpportunity.RelatedParentActivityPivotCollection.AddNewPivot(ParentOpportunity);

			var collection = new RelatedActivityLinkCollection(ParentOpportunity);
			AssertContainsExactElementsInAnyOrder("Should only have one link when there is a related activity pivot to itself",
				BusinessObjectEqualityComparer<ViewRelatedActivityPivot>.PKOnlyComparer,
				new[] { parentPivot },
				collection.Cast<RelatedActivityLink>().Select(x => (ViewRelatedActivityPivot)x.Pivot));

			var link = collection[0];
			link.ToActivityID = ZGuid.Empty;
			link.ToActivityID = ParentOpportunity.PK;
			AssertContainsExactElementsInAnyOrder("Should still only have one link when there is a related activity pivot to itself",
				BusinessObjectEqualityComparer<ViewRelatedActivityPivot>.PKOnlyComparer,
				new[] { parentPivot },
				collection.Cast<RelatedActivityLink>().Select(x => (ViewRelatedActivityPivot)x.Pivot));
		}

		public override void TestAdd()
		{
			Assert("Can not manually add items as they should be automatically added when WrappedCollection is changed", true);
		}

		public override void TestDelete()
		{
			Assert("Can not manually add items as they should be automatically added when WrappedCollection is changed", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Can not manually add items as they should be automatically added when WrappedCollection is changed", true);
		}

		#region Implementation

		protected override RelatedActivityLinkCollection GetCollectionToTest()
		{
			return new RelatedActivityLinkCollection(ParentOpportunity);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var inquiry = (IRelatableActivity)Factory.NewWithValidTestData<SalesEnquiry>();
			var pivot = Factory.New<ViewRelatedActivityPivot>();
			pivot.ParentActivity = ParentOpportunity;
			pivot.ChildActivity = inquiry;

			return RelatedActivityLink.Get(pivot, inquiry);
		}

		IRelatableActivity ParentOpportunity
		{
			get
			{
				return parentOpportunity ?? (parentOpportunity = Factory.NewWithValidTestData<OrgOpportunity>());
			}
		}
		IRelatableActivity parentOpportunity;

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directionRules);
		}

		#endregion
	}
}
