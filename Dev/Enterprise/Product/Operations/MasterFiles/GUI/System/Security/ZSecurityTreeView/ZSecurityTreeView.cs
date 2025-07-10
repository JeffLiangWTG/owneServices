using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class ZSecurityTreeView : ZTreeView
	{
		public ZSecurityTreeView()
		{
			HideSelection = false;
		}

		public void Populate(SecurityCore security)
		{
			SecurityVector vector = new SecurityVector();
			vector.Initialise(security);

			Nodes.Clear();
			foreach (ISecurityInfo info in vector.Nodes)
			{
				Add(info, Nodes).Expand();
			}
			SelectedNode = Nodes[0];
			currentSecurity = security;
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand(e);

			if (!isRegistryLoaded && e.Node.Tag is SecurityCheckpointNonOperationalAllowed checkpointNonOperationalAllowed)
			{
				if (checkpointNonOperationalAllowed.Code == Env.Security.SystemRegistry.Code)
				{
					// To make SystemRegistryEditSpecificSettings can be expanded.
					e.Node.Nodes.Cast<TreeNode>().First(x => ((SecurityCheckpoint)x.Tag).Code == "SystemRegistryEditSpecificSettings").Nodes.Add(new ZSecurityPointNode(string.Empty, null));
				}
				else if (checkpointNonOperationalAllowed.Code == Env.Security.SystemRegistryEditSpecificSettings.Code)
				{
					LoadRegistryNodesCore(checkpointNonOperationalAllowed, e.Node.Nodes);
				}
			}
		}

		bool isRegistryLoaded;
		SecurityCore currentSecurity;

		ZSecurityPointNode Add(ISecurityCheckpoint checkpoint, TreeNodeCollection nodes)
		{
			ZSecurityPointNode node = null;

			foreach (var child in checkpoint.ChildCheckPoints)
			{
				node = new ZSecurityPointNode(child.DisplayText, child);
				nodes.Add(node);

				Add(child, node.Nodes);
			}

			return node;
		}

		ZSecurityPointNode Add(ISecurityInfo info, TreeNodeCollection nodes)
		{
			ZSecurityPointNode node = new ZSecurityPointNode(info.Name, info.Checkpoint);
			nodes.Add(node);
			foreach (ISecurityInfo child in info.Nodes)
			{
				Add(child, node.Nodes);
			}

			return node;
		}

		public override void LoadRegistryNodes()
		{
			if (!isRegistryLoaded)
			{
				var foundNode = FindFirstNodeByCode(Env.Security.SystemRegistryEditSpecificSettings.Code, Nodes);

				if (foundNode != null)
				{
					LoadRegistryNodesCore((ISecurityCheckpoint)foundNode.Tag, foundNode.Nodes);
				}
			}
		}

		void LoadRegistryNodesCore(ISecurityCheckpoint checkpoint, TreeNodeCollection nodes)
		{
			currentSecurity.LoadRegistrySecurityCheckPoints();
			isRegistryLoaded = true;
			nodes.Clear();
			Add(checkpoint, nodes);
		}

		internal TreeNode FindFirstNodeByCode(string code, TreeNodeCollection nodes)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag is SecurityCheckpointNonOperationalAllowed checkpointNonOperationalAllowed && checkpointNonOperationalAllowed.Code == code)
				{
					return node;
				}
			}

			for (var index = 0; index < nodes.Count; ++index)
			{
				if (nodes[index] != null && nodes[index].Nodes.Count > 0)
				{
					var foundNode = FindFirstNodeByCode(code, nodes[index].Nodes);

					if (foundNode != null)
					{
						return foundNode;
					}
				}
			}

			return null;
		}
	}
}
