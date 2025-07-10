using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	partial class SecuritySelectionForm : ZChildForm
	{
		CheckpointLookupKey lookupKey;

		public SecuritySelectionForm()
		{
			SecurityTreeView.Populate(Env.Security);
		}

		public override string FormHeading
		{
			get { return Res.GetString("a96506ad-3a20-4605-81b2-55a4f5389eb5", "Select a Security Right"); }
		}

		public CheckpointLookupKey LookupKey
		{
			get { return lookupKey; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			lookupKey = (DialogResult == DialogResult.OK) ? ((ZSecurityPointNode)SecurityTreeView.SelectedNode).Checkpoint.LookupKey : CheckpointLookupKey.Empty;
		}

		ZSecurityPointNode FindNode(TreeNodeCollection nodes, CheckpointLookupKey lookupKey)
		{
			int index = 0;
			ZSecurityPointNode result = null;

			while ((result == null) && (index < nodes.Count))
			{
				ZSecurityPointNode node = (ZSecurityPointNode)nodes[index++];
				if (node.Checkpoint.LookupKey == lookupKey)
				{
					result = node;
				}
				else if (node.Nodes.Count > 0)
				{
					result = FindNode(node.Nodes, lookupKey);
				}
			}

			return result;
		}

		public void NavigateToNode(CheckpointLookupKey lookupKey)
		{
			if (!lookupKey.IsEmpty)
			{
				ZSecurityPointNode node = FindNode(SecurityTreeView.Nodes, lookupKey);
				if (node != null)
				{
					SecurityTreeView.SelectedNode = node;
				}
			}
		}

		#region Test
#if DEBUG
		internal ZSecurityTreeView SecurityTreeView_Exposed
		{
			get { return SecurityTreeView; }
		}

#endif
		#endregion
	}
}
