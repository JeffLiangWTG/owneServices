using System.Windows.Forms;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class ZSecurityPointNode : TreeNode
	{
		public ZSecurityPointNode(string text, ISecurityCheckpoint checkpoint)
			: base(text)
		{
			this.Tag = checkpoint;
		}

		public ISecurityCheckpoint Checkpoint
		{
			get { return Tag as ISecurityCheckpoint; }
		}
	}
}
