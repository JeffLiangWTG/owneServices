using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(GlbAccreditationForm))]
	public class GlbAccreditationGroupTreeControlTest : ZFormBasherTest
	{
		public void TestDeleteGroup()
		{
			var accred = Factory.New<GlbAccreditation>();
			var group1 = accred.Groups.AddNew();
			var group2 = accred.Groups.AddNew();
			group1.SkillPivots.Reload(true);
			group2.SkillPivots.Reload(true);
			var model = new GlbAccreditationTreeModel(accred);
			using (var form = new GlbAccreditationFormForTest(model))
			{
				form.Show();
				foreach (var node in form.GroupTreeControl.Tree.AllNodes)
				{
					var treeNode = node.Tag as GlbAccreditationTreeNode;
					var group = treeNode.BizObjForBinding.Parent as GlbAccreditationJobSkillGroup;
					if (group != null && group.PK == group2.PK)
					{
						form.GroupTreeControl.Tree.SelectedNode = node;
						break;
					}
				}

				AssertNotNull(form.GroupTreeControl.Tree.SelectedNode);
				AssertEquals(2, form.GroupTreeControl.Tree.AllNodes.Count());
				form.GroupTreeControl.DeleteExposed();
				AssertEquals(1, form.GroupTreeControl.Tree.AllNodes.Count());
				AssertEquals(false, group1.IsDeleted);
				AssertEquals(true, group2.IsDeleted);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			Factory.Save();
			return new GlbAccreditationForm(accred);
		}

		public class GlbAccreditationGroupTreeControlForTest : GlbAccreditationGroupTreeControl
		{
			public void DeleteExposed()
			{
				DeleteButton_Click(null, EventArgs.Empty);
			}
		}

		public class GlbAccreditationFormForTest : ZForm
		{
			public GlbAccreditationFormForTest(GlbAccreditationTreeModel model) : base(model)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = new Size(1024, 768);
				GroupTreeControl.Dock = DockStyle.Fill;
				Controls.Add(GroupTreeControl);
				BindingSource.SetBindingMember(GroupTreeControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly GlbAccreditationGroupTreeControlForTest GroupTreeControl = new GlbAccreditationGroupTreeControlForTest();
		}
	}
}
