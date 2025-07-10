using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.ZArchitecture.Business.Testing
{
	class HasChangesHunterTest : TestCaseWithDummy
	{
		public void TestLastEntityFoundWithChanges()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);
			HasChangesHunter hunter = new HasChangesHunter(Dummy);
			AssertNull("No search done yet", hunter.LastEntityFoundWithChanges);
			AssertEquals("no changes", false, hunter.HasChanges);
			AssertEquals("no changes", false, hunter.HasChangesSinceLastMark);
			AssertNull("No changes", hunter.LastEntityFoundWithChanges);

			DummyChildBusinessObject child = Dummy.Collection.AddNew();
			child.Z0_Description = "my change";
			AssertEquals("has changes", true, hunter.HasChanges);
			AssertEquals("has changes", true, hunter.HasChangesSinceLastMark);
			AssertEquals("found child with changed", child, hunter.LastEntityFoundWithChanges);

			hunter.Mark();
			AssertEquals("no changes", false, hunter.HasChangesSinceLastMark);
			AssertNull("No changes", hunter.LastEntityFoundWithChanges);

			AssertEquals("has changes", true, hunter.HasChanges);
			AssertEquals("found child with changed", child, hunter.LastEntityFoundWithChanges);
		}

		public void TestHasChangesHunterWorksWithMocks()
		{
			var dummyMock = Factory.NewMoq<DummyBusinessObject>();
			DummyBusinessObject dummy = dummyMock.Object;
			HasChangesHunter hunter = new HasChangesHunter(dummy);
			hunter.AddIncludedNamespacePrefix("CargoWise.EntityFramework");
			hunter.Mark();
			dummy.HasChanges = true;
			AssertEquals("HasChangesSinceLastMark", true, hunter.HasChangesSinceLastMark);
		}

		public void TestHasChangesSinceLastMark()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);

			HasChangesHunter hunter = new HasChangesHunter(Dummy);
			HasChangesHunter hunterWithExcludes = new HasChangesHunter(Dummy, new HasChangesHunterExclusionDetails(Dummy.Collection.TypeOfElements));

			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
			AssertEquals("HasChangesSinceLastMark with exclude", false, hunterWithExcludes.HasChangesSinceLastMark);

			Dummy.Collection.AddNew();
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
			AssertEquals("HasChangesSinceLastMark with exclude", false, hunterWithExcludes.HasChangesSinceLastMark);

			Dummy.Collection[0].Z0_AnotherDecimal = 34.34m;
			AssertEquals("HasChangesSinceLastMark", true, hunter.HasChangesSinceLastMark);
			AssertEquals("HasChangesSinceLastMark with exclude", false, hunterWithExcludes.HasChangesSinceLastMark);

			hunter.Mark();
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);

			Dummy.Z0_Number = 234234;
			AssertEquals("HasChangesSinceLastMark", true, hunter.HasChangesSinceLastMark);
			AssertEquals("HasChangesSinceLastMark with exclude", true, hunterWithExcludes.HasChangesSinceLastMark);

			Dummy.HasChanges = false;
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
			AssertEquals("HasChangesSinceLastMark with exclude", false, hunterWithExcludes.HasChangesSinceLastMark);
		}

		public void TestBusinessObjectHasChangesSinceLastMark()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);

			HasChangesHunter hunter = new HasChangesHunter(Dummy);

			AssertEquals("DummyHasChanges", false, Dummy.HasChanges);
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
			AssertEquals("BusinessObjectHasChangesSinceLastMark", false, hunter.BusinessObjectHasChangesSinceLastMark(Dummy));

			Dummy.Z0_Number = 23;

			AssertEquals("DummyHasChanges", true, Dummy.HasChanges);
			AssertEquals("HasChangesSinceLastMark", true, hunter.HasChangesSinceLastMark);
			AssertEquals("BusinessObjectHasChangesSinceLastMark", true, hunter.BusinessObjectHasChangesSinceLastMark(Dummy));

			hunter.Mark();

			AssertEquals("DummyHasChanges", true, Dummy.HasChanges);
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
			AssertEquals("BusinessObjectHasChangesSinceLastMark", false, hunter.BusinessObjectHasChangesSinceLastMark(Dummy));
		}

		public void TestIncludedNamespacePrefixPropertyManagement()
		{
			HasChangesHunter hunter = new HasChangesHunter(Dummy);
			AssertEquals(0, hunter.IncludedPrefixes.Length);
			hunter.AddIncludedNamespacePrefix("Enterprise.Customs");
			AssertEquals("Enterprise.Customs", hunter.IncludedPrefixes[0]);
		}

		public void TestIncludedNamespaceBehaviour()
		{
			HasChangesHunter hunter = new HasChangesHunter(Dummy);
			hunter.AddIncludedNamespacePrefix("Enterprise.Customs");
			Dummy.Z0_AnotherNumber++;
			AssertEquals("HasChangesSinceLastMark", false, hunter.HasChangesSinceLastMark);
		}

		public void TestHasChangesWithNoExcludedTypes()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);

			Assert("Should be no changes", !Dummy.HasChanges);
			Assert("Should be no changes", !new HasChangesHunter(Dummy).HasChanges);

			Dummy.Z0_Number = 34;

			Assert("Should be changes", Dummy.HasChanges);
			Assert("Should be changes", new HasChangesHunter(Dummy).HasChanges);

			Dummy.HasChanges = false;

			Assert("Should be no changes", !Dummy.HasChanges);
			Assert("Should be no changes", !new HasChangesHunter(Dummy).HasChanges);

			Dummy.Collection.AddNew().Z0_Number = 422;

			Assert("Should be changes", Dummy.HasChanges);
			Assert("Should be changes", new HasChangesHunter(Dummy).HasChanges);
		}

		HasChangesHunter GetHunterWithExclusions(IBusiness bO, bool shouldIgnoreChildren)
		{
			return new HasChangesHunter(bO, new HasChangesHunterExclusionDetails(typeof(DummyBusinessObject), shouldIgnoreChildren));
		}

		DummyChildOfChild GetNewChildOfChildDummyWithChanges()
		{
			DummyChildOfChild child = Factory.New<DummyChildOfChild>();
			child.HasChanges = true;
			return child;
		}

		protected class DummyChildOfChild : DummyChildBusinessObject
		{
			public DummyChildOfChild(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		public void TestHasChangesWithExcludedTypes()
		{
			Dummy.RegisterEditableChildObject(Dummy.Collection);
			Assert("Should be no changes", !GetHunterWithExclusions(Dummy, true).HasChanges);
			Assert("Should be no changes", !GetHunterWithExclusions(Dummy, false).HasChanges);

			Dummy.Z0_Number = 34;

			Assert("Should be no changes: Dummy excluded", !GetHunterWithExclusions(Dummy, false).HasChanges);
			Assert("Should be no changes: Dummy excluded", !GetHunterWithExclusions(Dummy, true).HasChanges);

			Dummy.Collection.AddNew().Z0_Number = 422;
			Assert("Should be no changes as dummy child ignored", !GetHunterWithExclusions(Dummy, true).HasChanges);
			Assert("Should be changes as dummy child considered", GetHunterWithExclusions(Dummy, false).HasChanges);

			Dummy.Collection.RemoveAndDeleteAll();
			Assert("Should be changes as the collection will have changes from delete", GetHunterWithExclusions(Dummy, false).HasChanges);
			Assert("Should be no changes as collection is excluded", !GetHunterWithExclusions(Dummy, true).HasChanges);

			Dummy.Collection.HasChanges = false;
			Assert("Should be no changes", !GetHunterWithExclusions(Dummy, false).HasChanges);
			Assert("Should be no changes", !GetHunterWithExclusions(Dummy, true).HasChanges);

			Dummy.RegisterEditableChildObject(GetNewChildOfChildDummyWithChanges());
			Assert("Should be no changes as subclasses are excluded too", !GetHunterWithExclusions(Dummy, true).HasChanges);
			Assert("Should be changes as subclasses are considered", GetHunterWithExclusions(Dummy, false).HasChanges);
		}

		public void TestGenAddOnColumnChildOfNotExcludedBizObjAffectsHasChanges()
		{
			var genAddOnColumnParent = Factory.New<DummyBusinessObjectWithGenAddOnColumn>();
			var hunter = new HasChangesHunter(genAddOnColumnParent);
			Assert("PRE-CONDITION: should not have changes as newly created", !hunter.HasChanges);

			genAddOnColumnParent.GenAddOnColumnForTest = "X";
			Assert("POST-CONDITION: should have changes as parent BizObj is not excluded", hunter.HasChanges);
		}

		public void TestGenAddOnColumnChildOfExcludedBizObjDoesNotAffectHasChanges()
		{
			var genAddOnColumnParent = Factory.New<DummyBusinessObjectWithGenAddOnColumn>();
			var hunter = new HasChangesHunter(genAddOnColumnParent, new HasChangesHunterExclusionDetails[] { new HasChangesHunterExclusionDetails(typeof(DummyBusinessObjectWithGenAddOnColumn), false) });
			Assert("PRE-CONDITION: should not have changes as newly created", !hunter.HasChanges);

			genAddOnColumnParent.GenAddOnColumnForTest = "X";
			Assert("POST-CONDITION: should not have changes as parent BizObj is excluded", !hunter.HasChanges);
		}

		public void TestHasChangesEvaluatesGenAddOnColumnItselfWhenOrphan()
		{
			var genAddOnColumn = Factory.New<GenAddOnColumn>();
			var hunter = new HasChangesHunter(genAddOnColumn);
			Assert("PRE-CONDITION: should not have changes as newly created", !hunter.HasChanges);

			genAddOnColumn.XA_Data = "X";
			Assert("POST-CONDITION: should have changes as evaluating GenAddOnColumn itself", hunter.HasChanges);
		}

		public void TestHasChangesWhenTargetIsDeletedGenAddOnColumn()
		{
			var genAddOnColumn = Factory.New<GenAddOnColumn>();
			var hunter = new HasChangesHunter(genAddOnColumn);
			Assert("PRE-CONDITION: should not have changes as newly created", !hunter.HasChanges);

			genAddOnColumn.XA_Data = "X";
			genAddOnColumn.Delete();
			Assert("POST-CONDITION: should not have changes as GenAddOnColumn itself is deleted", !hunter.HasChanges);
		}

		#region DummyBusinessObjectWithGenAddOnColumn

		[SystemDefinedValues]
		class DummyBusinessObjectWithGenAddOnColumn : DummyBusinessObject
		{
			public DummyBusinessObjectWithGenAddOnColumn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString GenAddOnColumnForTest
			{
				get => this.GetSystemDefinedValue<ZString>(nameof(GenAddOnColumnForTest));
				set => this.SetSystemDefinedValue(nameof(GenAddOnColumnForTest), value);
			}
		}

		#endregion
	}
}
