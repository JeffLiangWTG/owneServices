using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(NZCClassForm))]
	public class NZCClassFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new NZCClassForm(new FamilyMemberCollectionForBinding());
			MissingResourceStringChecker.ExcludeFromTest(form.statUnitTextBox);
			MissingResourceStringChecker.ExcludeFromTest(form.suppUnitTextBox);
			return form;
		}

		public void TestDoNotSetUpPostingButtons()
		{
			using (var form = new TestNZCClassForm())
			{
				form.Show();
				Application.DoEvents();
				var buttons = form.Controls.OfType<ZButton>();
				AssertEquals(2, buttons.Count());
				var okButton = buttons.FirstOrDefault(x => !x.Enabled);
				AssertNotNull(okButton);
				var treeViews = form.Controls.OfType<ZTreeView>();
				AssertEquals(1, treeViews.Count());
				var treeView = treeViews.First();
				TreeNode node = GetChildNodeWithoutItsOwnChildren(treeView.Nodes[0]);
				treeView.SelectedNode = node;
				Assert(okButton.Enabled);
				AssertNoExceptionThrown(delegate
				{
					okButton.PerformClick();
				}

				);
				AssertEquals("&OK", okButton.Text);
			}
		}

		class TestNZCClassForm : NZCClassForm
		{
			public TestNZCClassForm() : base(new FamilyMemberCollectionForBinding())
			{
				this.findBox = new NZCClassGridFindBox();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				((NZCClassGridFindBox)findBox).Dispose();
			}
		}

		TreeNode GetChildNodeWithoutItsOwnChildren(TreeNode node)
		{
			TreeNode result = null;
			if (node.Nodes.Count == 0)
			{
				result = node;
			}
			else
			{
				node.Expand();
				foreach (TreeNode childNode in node.Nodes)
				{
					result = GetChildNodeWithoutItsOwnChildren(childNode);
					break;
				}
			}

			return result;
		}
	}
}
