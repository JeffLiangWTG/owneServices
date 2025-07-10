using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public abstract class SimpleTreeModel<T> : ZTreeModel<T>
		where T : class, IBusiness
	{
		protected SimpleTreeModel(T master)
			: base(master.Factory)
		{
			Master = master;
		}

		public readonly T Master;

		#region MasterNode

		public ZNode<T> MasterNode
		{
			get { return masterNode ?? (masterNode = CreateMasterNode()); }
		}
		ZNode<T> masterNode;

		protected abstract ZNode<T> CreateMasterNode();

		#endregion

		#region RootNode

		public ZNode<T> RootNode
		{
			get { return RootNodes.Single(); }
		}

		protected override ZNodeCollection<T> GetRootNodes()
		{
			var collection = new ZNodeCollection<T>();

			var currentNode = MasterNode;

			var previouslyTraversedBizObjPks = new HashSet<ZGuid>();
			while (currentNode.ParentNode != null)
			{
				previouslyTraversedBizObjPks.Add(currentNode.BizObj.Identifier);
				if (previouslyTraversedBizObjPks.Contains(currentNode.ParentNode.BizObj.Identifier))
				{
					// Should never be in this state, but lets stop traversing here just in case (to prevent infinite loop)
					collection.Add(currentNode);
					return collection;
				}
				else
				{
					currentNode = currentNode.ParentNode;
				}
			}

			collection.Add(currentNode);
			return collection;
		}

		#endregion
	}
}
