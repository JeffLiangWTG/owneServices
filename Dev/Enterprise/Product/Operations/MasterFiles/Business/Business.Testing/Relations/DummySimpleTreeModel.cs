using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummySimpleTreeModel : SimpleTreeModel<DummyBusinessObject>
	{
		public DummySimpleTreeModel(DummyBusinessObject master)
			: base(master)
		{
		}

		protected override ZNode<DummyBusinessObject> CreateMasterNode()
		{
			return CreateNewNode(Master);
		}

		protected override ZNode<DummyBusinessObject> CreateNewNodeCore(ZTreeModel<DummyBusinessObject> treeModel, DummyBusinessObject bizObj)
		{
			return new DummyGenPivotNode(treeModel, bizObj);
		}
	}
}
