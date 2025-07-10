using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SimpleTreeModelTest : TestCaseWithFactory
	{
		#region RootNode

		public void TestRootNode()
		{
			var master = Factory.New<DummyBusinessObject>();
			var masterParent = Factory.New<DummyBusinessObject>();
			var masterGrandParent = Factory.New<DummyBusinessObject>();
			var masterChild = Factory.New<DummyBusinessObject>();

			CreatePivot(master, masterChild);
			CreatePivot(masterParent, master);
			CreatePivot(masterGrandParent, masterParent);

			var model = new DummySimpleTreeModel(master);
			AssertContainsExactElementsInAnyOrder(new[] { masterGrandParent }, model.RootNodes.Select(x => x.BizObj));
		}

		public void TestRootNode_WithCycle()
		{
			var dummyMaster = Factory.New<DummyBusinessObject>();
			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();

			CreatePivot(dummyMaster, dummyA);
			CreatePivot(dummyA, dummyB);
			CreatePivot(dummyB, dummyMaster);

			var model = new DummySimpleTreeModel(dummyMaster);
			AssertContainsExactElementsInAnyOrder(new[] { dummyA }, model.RootNodes.Select(x => x.BizObj));
		}

		#endregion

		#region Implementation

		GenPivot CreatePivot(DummyBusinessObject parent, DummyBusinessObject child)
		{
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = parent.PK;
			pivot.XX_Relation2ID = child.PK;
			return pivot;
		}

		#endregion
	}
}
