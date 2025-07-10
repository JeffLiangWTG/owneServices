using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.GUI
{
	public partial class GlbAccreditationGroupTreeControl : GlbAccreditationGroupTreeControlBase
	{
		protected override bool ShouldShowEditButton => true;
		protected override bool ShouldShowNewButton => true;

		public override bool ShowColumns => false;

		protected override void SetupButtons()
		{
			base.SetupButtons();
			var deleteButton = new ZToolStripButton
			{
				Alignment = NewToolStripButton.Alignment,
				CaptionResourceString = Res.GetData("47906708-E972-4B57-B786-9544244AE1BE", "Delete"),
				Image = Icons.GetImage(IconTypes.DeleteButtonActive)
			};

			deleteButton.Click += DeleteButton_Click;
			bottomToolStrip.Items.Insert(0, deleteButton);
		}

		void RefreshTree()
		{
			Model.BuildTree();
			ModelView.RefreshView();
			ExpandRequired();
		}

		protected void DeleteButton_Click(object sender, EventArgs e)
		{
			if (Tree.SelectedNodes == null)
			{
				return;
			}

			bool deleted = false;

			foreach (var treeNode in Tree.SelectedNodes)
			{
				var node = treeNode.Tag as GlbAccreditationTreeNode;

				if (node != null)
				{
					var group = node.BizObjForBinding.Parent as GlbAccreditationJobSkillGroup;
					if (group != null)
					{
						Model.Accreditation.Groups.Delete(group);
						deleted = true;
					}
				}
			}

			if (deleted)
			{
				RefreshTree();
			}
		}

		public new GlbAccreditationTreeModel Model
		{
			get { return base.Model as GlbAccreditationTreeModel; }
		}

		protected override void NewToolStripButton_Click(object sender, EventArgs e)
		{
			if (!Model.Accreditation.IsInDatabaseIncludingChildren)
			{
				return;
			}

			if (Model.Accreditation.HasChanges)
			{
				Globals.Message.Show(Res.GetString("5BEBD5C1-4C1D-4C5F-856D-EC25B536185C", "Please save the form before adding a group"));
				return;
			}

			base.NewToolStripButton_Click(sender, e);
			var factory = new BusinessObjectFactory();
			var group = factory.New<GlbAccreditationJobSkillGroup>();
			group.HJG_ParentID = Model.Accreditation.PK;
			group.HJG_ParentTableCode = GlbAccreditationSchema.Constants.Prefix;

			ZFormModaliser.ShowDialogAndDispose(new GlbAccreditationJobSkillGroupForm(group), ParentForm);

			if (group.IsInDatabase)
			{
				Model.Accreditation.Groups.Reload();
				RefreshTree();
			}
		}

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
			var wrapper = bizObjToEdit as GlbAccreditationTreeBizObjWrapperBase;
			var parent = wrapper?.Parent;
			var factory = new BusinessObjectFactory();

			if (parent != null)
			{
				var group = parent as GlbAccreditationJobSkillGroup;
				if (group != null)
				{
					if (Model.Accreditation.HasChanges)
					{
						Globals.Message.Show(Res.GetString("96867837-8ECC-4034-A88C-4AA57ED6B502", "Please save the form before editing a group"));
						return;
					}

					var groupInAnotherFactory = factory.Load<GlbAccreditationJobSkillGroup>(group.PK);
					var form = new GlbAccreditationJobSkillGroupForm(groupInAnotherFactory);
					form.Closed += (sender, args) =>
					{
						group.Reload();
						group.SkillPivots.Reload(true);
					};

					ZFormModaliser.ShowDialogAndDispose(form, ParentForm);
				}
			}
		}

		protected override void OnAfterAllEdits()
		{
			base.OnAfterAllEdits();
			RefreshTree();
		}
	}
}
