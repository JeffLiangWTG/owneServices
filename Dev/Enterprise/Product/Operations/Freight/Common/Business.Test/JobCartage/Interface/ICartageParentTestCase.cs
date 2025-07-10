using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestsSubclassesOf(typeof(ICartageParent), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class ICartageParentTestCase : TestCaseWithFactory
	{
		#region TestRelatedLocalTransportObjectsWithEvents

		public void TestRelatedLocalTransportObjectsWithEvents()
		{
			var parent = GetNewParent();
			var relatedParent = parent.BusinessObjectForRelatedEvents;

			var outOfScopeCartage = Factory.New<ICommonCartage>();
			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.Constants.JJ_ParentTableCode] = parent.CartageParentTableCode;
			cartage[JobCartageSchema.Constants.JJ_ParentID] = parent.CartageParentID;

			AssertCollectionContains("Should contain the related Local Transport Jobs.", cartage, relatedParent.BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains("Should NOT contain the non related Local Transport Jobs.", outOfScopeCartage, relatedParent.BusinessObjectsWithRelatedEvents);

			AssertCollectionContains("Cartage1 should contain the related Parent.", relatedParent, ((EnterpriseBusinessObject)cartage).BusinessObjectsWithRelatedEvents);
			AssertCollectionNotContains("OutOfScope Cartage should NOT contain the related Parent.", relatedParent, ((EnterpriseBusinessObject)outOfScopeCartage).BusinessObjectsWithRelatedEvents);

			if (SupportsMultipleCartages)
			{
				var cartage2 = (BusinessObject)Factory.New<ICommonCartage>();
				cartage2[JobCartageSchema.Constants.JJ_ParentTableCode] = parent.CartageParentTableCode;
				cartage2[JobCartageSchema.Constants.JJ_ParentID] = parent.CartageParentID;

				AssertCollectionContains("Parent should contain the related Local Transport Job.", cartage2, relatedParent.BusinessObjectsWithRelatedEvents);
				AssertCollectionContains("Cartage2 should contain the related Parent.", relatedParent, ((EnterpriseBusinessObject)cartage2).BusinessObjectsWithRelatedEvents);
			}
		}

		#endregion

		#region TestRelatedLocalTransportObjectsWithEDocs

		public void TestRelatedLocalTransportObjectsWithEDocs()
		{
			var parent = GetNewParent();
			var relatedParent = parent.BusinessObjectForRelatedEDocs;

			var outOfScopeCartage = Factory.New<ICommonCartage>();
			var cartage = (BusinessObject)Factory.New<ICommonCartage>();
			cartage[JobCartageSchema.Constants.JJ_ParentTableCode] = parent.CartageParentTableCode;
			cartage[JobCartageSchema.Constants.JJ_ParentID] = parent.CartageParentID;

			AssertCollectionContains("Should contain the cartage.", cartage, relatedParent.DocManagerInfo.RelatedObjects);
			AssertCollectionNotContains("Should NOT contain outOfScopeCartage.", outOfScopeCartage, relatedParent.DocManagerInfo.RelatedObjects);

			AssertContainsExactElementsInAnyOrder(new[] { relatedParent }, ((IDocManagerSupport)cartage).DocManagerInfo.RelatedObjects);

			if (SupportsMultipleCartages)
			{
				var cartage2 = (BusinessObject)Factory.New<ICommonCartage>();
				cartage2[JobCartageSchema.Constants.JJ_ParentTableCode] = parent.CartageParentTableCode;
				cartage2[JobCartageSchema.Constants.JJ_ParentID] = parent.CartageParentID;

				AssertCollectionContains("Parent should contain the related Local Transport Job.", cartage2, relatedParent.DocManagerInfo.RelatedObjects);
				AssertCollectionContains("Cartage2 should contain the related Parent.", relatedParent, ((IDocManagerSupport)cartage2).DocManagerInfo.RelatedObjects);
			}
		}

		#endregion

		#region SupportsMultipleCartages

		protected virtual ZBool SupportsMultipleCartages
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		protected abstract ICartageParent GetNewParent();

		#endregion
	}
}
