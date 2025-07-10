using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AccreditationTree : ZTreeViewAdv
	{
		public AccreditationTree()
		{
			ElementType = typeof(GlbAccreditationTreeBizObjWrapperBase);
		}

		protected override void OnNodeDragDrop_Before<T>(ZNode<T> node, ZNode<T> dropNode)
		{
			var nodeParent = node.ParentNode;
			var dropNodeParent = dropNode.ParentNode;

			if (
				!UnsetParentNode(node) ||
				!UnsetParentNode(dropNode) ||
				!SetParentNodeIfValid(node, dropNodeParent))
			{
				node.SetParentNode(nodeParent, false);
			}
		}
	}
}
