using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchDependentCollection))]
	sealed class GlbBranchDependentCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbBranchDependentCollection>
	{
		protected override GlbBranchDependentCollection GetCollectionToTest()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			return new GlbBranchDependentCollection(company, Factory);
		}

		public void TestDeleteWithEvents()
		{
			AssertEquals("we should have a current branch set up", true, GlbCompany.CurrentCompany.Branches.Contains(GlbBranch.CurrentBranch));

			DeleteCurrentBranchFired = false;
			GlbCompany.CurrentCompany.Branches.Delete(GlbBranch.CurrentBranch);
			AssertEquals("should have fired event", true, DeleteCurrentBranchFired);

			DeleteCurrentBranchFired = false;
			GlbBranch newBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			GlbCompany.CurrentCompany.Branches.Delete(newBranch);
			AssertEquals("event should NOT have fired", false, DeleteCurrentBranchFired);
		}

		public void TestNoExceptionThrownGivenNonBranchBizo()
		{
			var company = Factory.New<GlbCompany>();
			var branchDependentCollection = new GlbBranchDependentCollectionForTest(company, Factory);

			AssertNoExceptionThrown(() =>
			{
				branchDependentCollection.SetDefaultsForNewElement(null);
				branchDependentCollection.SetDefaultsForNewElement(Factory.New<DummyBusinessObject>());
				branchDependentCollection.SetDefaultsForNewElement(Factory.New<GlbBranch>());
			});
		}

		public void TestNoExceptionThrownWithNullParent()
		{
			var branchDependentCollection = new GlbBranchDependentCollectionForTest(Factory);
			var branchDependentCollectionWithNullParent = new GlbBranchDependentCollectionForTest(null, Factory);

			AssertEquals("Parent should be current company",GlbCompany.CurrentCompany.PK, branchDependentCollection.Parent_exposed.PK);
			AssertNoExceptionThrown(() =>
			{
				branchDependentCollection.SetDefaultsForNewElement(Factory.New<GlbBranch>());
				branchDependentCollectionWithNullParent.SetDefaultsForNewElement(Factory.New<GlbBranch>());
			});
		}

		#region Implementation

		bool DeleteCurrentBranchFired;

		void OnAttemptedToDeleteCurrentBranch(object sender, EventArgs args)
		{
			DeleteCurrentBranchFired = true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.Branches.OnAttemptedToDeleteCurrentBranch +=
				new EventHandler(OnAttemptedToDeleteCurrentBranch);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.Branches.OnAttemptedToDeleteCurrentBranch -=
				new EventHandler(OnAttemptedToDeleteCurrentBranch);
		}

		#endregion

		class GlbBranchDependentCollectionForTest : GlbBranchDependentCollection
		{
			public GlbBranchDependentCollectionForTest(GlbCompany parent, BusinessObjectFactory factory) : base(parent, factory)
			{
			}

			public GlbBranchDependentCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public GlbCompany Parent_exposed => base.Parent;

			public void SetDefaultsForNewElement(BusinessObject newElement)
			{
				SetDefaultsForNewElementCore(newElement as GlbBranch);
			}

			protected override void SetDefaultsForNewElementCore(GlbBranch newElement)
			{
				base.SetDefaultsForNewElementCore(newElement);
			}
		}
	}
}
