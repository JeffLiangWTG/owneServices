using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(SecuritySelectionForm))]
	sealed class SecuritySelectionForm_Test : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SecuritySelectionForm();
		}

		public void TestFormHeading()
		{
			using (SecuritySelectionForm form = (SecuritySelectionForm)GetFormToBash())
			{
				AssertEquals("FormHeading", "Select a Security Right", form.FormHeading);
			}
		}

		public void TestLookupKeyIsSetOnClosing()
		{
			using (SecuritySelectionForm form = (SecuritySelectionForm)GetFormToBash())
			{
				form.Show();
				form.SecurityTreeView.SelectedNode = form.SecurityTreeView.Nodes[1];
				form.CloseButton.PerformClick();
				form.Close();
				AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
				AssertEquals("LookupKey.IsEmpty", true, form.LookupKey.IsEmpty);
			}

			using (SecuritySelectionForm form = (SecuritySelectionForm)GetFormToBash())
			{
				form.Show();
				form.SecurityTreeView.SelectedNode = form.SecurityTreeView.Nodes[1];
				form.OKButton.PerformClick();
				form.Close();
				AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
				AssertEquals("LookupKey", ((ZSecurityPointNode)form.SecurityTreeView.Nodes[1]).Checkpoint.LookupKey, form.LookupKey);
				AssertEquals("LookupKey.IsEmpty", false, form.LookupKey.IsEmpty);
			}
		}

		public void TestMissingNodesBug()
		{
			_ = Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(Env.Security.FindCheckPoint("MaintainShipment"));
			_ = Env.Security.FindOrCreateUniversalCopyCheckpoint(Env.Security.FindCheckPoint("MaintainShipment"));

			using (var form = (SecuritySelectionForm)GetFormToBash())
			{
				form.NavigateToNode(new CheckpointLookupKey("MaintainShipmentAllowRunOnAllMatchingRecords"));
				AssertEquals("MaintainShipmentAllowRunOnAllMatchingRecords", (((ZSecurityPointNode)form.SecurityTreeView.SelectedNode).Checkpoint).Code);
				form.NavigateToNode(new CheckpointLookupKey("MaintainShipmentUCopyEditPub"));
				AssertEquals("MaintainShipmentUCopyEditPub", (((ZSecurityPointNode)form.SecurityTreeView.SelectedNode).Checkpoint).Code);
			}
		}

		public void TestNavigateToNode()
		{
			using (SecuritySelectionForm form = (SecuritySelectionForm)GetFormToBash())
			{
				form.NavigateToNode(CheckpointLookupKey.Empty);
				AssertEquals("SecurityTreeView.SelectedNode", form.SecurityTreeView.Nodes[0], form.SecurityTreeView.SelectedNode);

				form.NavigateToNode(Env.Security.Forwarding.LookupKey);
				Assert("SecurityTreeView.SelectedNode should not be the first node.", form.SecurityTreeView.SelectedNode != form.SecurityTreeView.Nodes[0]);
				AssertEquals("SecurityTreeView.SelectedNode.CheckPoint", Env.Security.Forwarding, ((ZSecurityPointNode)form.SecurityTreeView.SelectedNode).Checkpoint);
			}
		}
	}
}
