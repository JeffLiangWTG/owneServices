using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZSecurityTreeViewTest : TestCaseWithFactory
	{
		public void TestPopulate()
		{
			using (ZSecurityTreeView tree = new ZSecurityTreeView())
			{
				AssertEquals(0, tree.Nodes.Count);
				tree.Populate(Env.Security);
				Assert(tree.Nodes.Count > 0);
			}
		}

		public void TestOnBeforeExpand()
		{
			using (var tree = new ZSecurityTreeView())
			{
				tree.Populate(new SecurityCore(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK));
				var systemRegistryNode = tree.FindFirstNodeByCode(Env.Security.SystemRegistry.Code, tree.Nodes);

				Assert(!systemRegistryNode.IsExpanded);
				AssertNotNull(systemRegistryNode);

				var systemRegistryEditSpecificSettingsNode = tree.FindFirstNodeByCode(Env.Security.SystemRegistryEditSpecificSettings.Code, tree.Nodes);

				AssertNotNull(systemRegistryEditSpecificSettingsNode);
				Assert(!systemRegistryEditSpecificSettingsNode.IsExpanded);
				AssertEquals(0, tree.FindFirstNodeByCode(Env.Security.SystemRegistryEditSpecificSettings.Code, tree.Nodes).Nodes.Count);

				var info = tree.GetType().GetMethod("OnBeforeExpand", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				AssertNotNull(info);
				var treeViewCancelEventArgs = new TreeViewCancelEventArgs(systemRegistryNode, false, TreeViewAction.Expand);
				info.Invoke(tree, new object[] { treeViewCancelEventArgs });

				Assert(systemRegistryEditSpecificSettingsNode.Nodes.Count == 1);
				AssertEquals(string.Empty, systemRegistryEditSpecificSettingsNode.Nodes[0].Text);

				treeViewCancelEventArgs = new TreeViewCancelEventArgs(systemRegistryEditSpecificSettingsNode, false, TreeViewAction.Expand);
				info.Invoke(tree, new object[] { treeViewCancelEventArgs });

				Assert(systemRegistryEditSpecificSettingsNode.Nodes.Count > 1);
			}
		}
	}
}
