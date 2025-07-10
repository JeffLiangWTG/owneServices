using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PersonAssociationsControl : ZTreeViewControl
	{
		public PersonAssociationsControl()
		{
			InitializeComponent();

			groupColumn.Header = PersonAssociationsTree.GroupColumnHeader;
			descriptionColumn.Header = PersonAssociationsTree.DescriptionColumnHeader;
			activeColumn.Header = PersonAssociationsTree.ActiveColumnHeader;
			primaryColumn.Header = PersonAssociationsTree.PrimaryColumnHeader;
			workingAddressUNLOCOColumn.Header = PersonAssociationsTree.WorkingAddressUNLOCOColumnHeader;
			cityColumn.Header = PersonAssociationsTree.CityColumnHeader;
			stateColumn.Header = PersonAssociationsTree.StateColumnHeader;
			countryColumn.Header = PersonAssociationsTree.CountryColumnHeader;
			showInactiveCheckBox.Text = PersonAssociationsTree.ShowInactiveCheckBoxCaption;
			createdTimeColumn.Header = PersonAssociationsTree.CreatedTimeColumnHeader;
			emailColumn.Header = PersonAssociationsTree.EmailColumnHeader;

			Tree.SetSortColumn(createdTimeColumn, System.Windows.Forms.SortOrder.Descending);
			Tree.SelectionMode = TreeSelectionMode.Multi;

			createdTimeColumn.IsVisible = false;
		}

		bool IsShowInactiveCheckBoxVisible;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				IsShowInactiveCheckBoxVisible = showInactiveCheckBox.Visible;
			}
		}

		protected override void OnHandleFullyCreated(EventArgs e)
		{
			base.OnHandleFullyCreated(e);

			if (!DesignModeFinder.IsDesigning)
			{
				LoadShowInactiveSettings();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning && IsShowInactiveCheckBoxVisible)
			{
				SaveShowInactiveSettings();
			}

			base.OnHandleDestroyed(e);
		}

		#region View/Model

		protected override ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new PersonAssociationsTree();
		}

		public new PersonAssociationsTreeModelView ModelView
		{
			get { return (PersonAssociationsTreeModelView)base.ModelView; }
		}

		protected override IZTreeModelView GetNewTreeModelView()
		{
			return new PersonAssociationsTreeModelView((PersonAssociationsTreeModel)CurrentDataItem);
		}

		protected PersonAssociationsTreeModel Model
		{
			get { return CurrentDataItem as PersonAssociationsTreeModel; }
		}

		#endregion

		#region Tree

		protected override ResourceStringData DefaultNameOfATreeElement
		{
			get { return Res.GetData("B1BDBFE1-E73E-4724-93C5-65487B26262A", "Person Association"); }
		}

		protected override ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return Res.GetData("780F39FB-D252-4A76-990A-DE7356B00B0F", "Person Associations"); }
		}

		protected override void SetupTree()
		{
			base.SetupTree();

			groupTextBox.DrawText += GroupTextBox_DrawText;
			descriptionTextBox.DrawText += Control_DrawText;
			cityTextBox.DrawText += Control_DrawText;
			stateTextBox.DrawText += Control_DrawText;
			activeCheckBox.IsVisibleValueNeeded += ControlOnIsVisibleValueNeeded;
			primaryCheckBox.IsVisibleValueNeeded += PrimaryCheckBox_IsVisibleValueNeeded;
			primaryCheckBox.IsEditEnabledValueNeeded += PrimaryCheckBox_IsEditEnabledValueNeeded;
			createdTimeTextBox.DrawText += Control_DrawText;
			countryTextBox.DrawText += Control_DrawText;
		}

		void ControlOnIsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (PersonAssociationsTreeNode)e.Node.Tag;
			var bizO = node?.BizObj;
			if (bizO is PersonAssociationsOrgGroupWrapper && !(bizO is PersonAssociationsOrganizationWrapper))
			{
				e.Value = false;
			}
		}

		void PrimaryCheckBox_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (PersonAssociationsTreeNode)e.Node.Tag;
			if (node == null || !node.IsPrimary_Visible)
			{
				e.Value = false;
			}
		}

		void PrimaryCheckBox_IsEditEnabledValueNeeded(object sender, NodeControlValueEventArgs e)
		{
			var node = (PersonAssociationsTreeNode)e.Node.Tag;
			if (node == null || !node.IsPrimary_Editable)
			{
				e.Value = false;
			}
		}

		protected override Brush GetRowBackgroundBrush(TreeNodeAdv node)
		{
			var brush = base.GetRowBackgroundBrush(node);
			var bizO = ((PersonAssociationsTreeNode)node.Tag).BizObj;
			if (brush == null && bizO is PersonAssociationsOrgGroupWrapper && !(bizO is PersonAssociationsOrganizationWrapper))
			{
				brush = SystemBrushes.Menu;
			}

			return brush;
		}

		static void GroupTextBox_DrawText(object sender, DrawEventArgs e)
		{
			var node = (PersonAssociationsTreeNode)e.Node.Tag;
			if (node != null &&
				(node.BizObjForBinding.GetType() == typeof(PersonAssociationsOrgGroupWrapper) ||
				 node.BizObjForBinding.GetType() == typeof(PersonAssociationsStaffWrapper) ||
				 node.BizObjForBinding.GetType() == typeof(PersonAssociationsJobApplicantWrapper)))
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
			}
		}

		static void Control_DrawText(object sender, DrawEventArgs e)
		{
			var node = (PersonAssociationsTreeNode)e.Node.Tag;
			if (node != null)
			{
				e.Font = new Font(e.Font, FontStyle.Regular);
			}
		}

		#endregion

		#region Buttons

		protected override void SetupButtons()
		{
			base.SetupButtons();

			SetupShowInactiveCheckBox();

			NewToolStripDropDownButton = new ZToolStripDropDownButton();
			CopyToolStripButtonAppearance(NewToolStripButton, NewToolStripDropDownButton);
			ReplaceToolStripButton(bottomToolStrip.Items, NewToolStripButton, NewToolStripDropDownButton);

			AttachToolStripDropDownButton = new ZToolStripDropDownButton();
			CopyToolStripButtonAppearance(AttachToolStripButton, AttachToolStripDropDownButton);
			ReplaceToolStripButton(bottomToolStrip.Items, AttachToolStripButton, AttachToolStripDropDownButton);

			DetachToolStripButton.Text = Res.GetString("00084321-BBF4-42EF-9BF7-B7F336148B20", "Move to New Person");

			RefreshNewButtons();
			RefreshAttachButtons();
		}

		static void CopyToolStripButtonAppearance(ZToolStripButton source, ZToolStripDropDownButton target)
		{
			target.Alignment = source.Alignment;
			target.CaptionResourceString = source.CaptionResourceString;
			target.Image = source.Image;
			target.ImageScaling = source.ImageScaling;
		}

		static void ReplaceToolStripButton(ToolStripItemCollection items, ZToolStripButton previousButton, ZToolStripDropDownButton newButton)
		{
			var index = items.IndexOf(previousButton);
			items.RemoveAt(index);
			items.Insert(index, newButton);
		}

		protected override void AttachToolStripButton_Click(object sender, EventArgs e)
		{
			// TODO: Implement AttachToolStripButton_Click
		}

		protected override void DetachSelectedElements()
		{
			if (Env.Security.PersonIntelligenceNew.IsAllowed)
			{
				if (Model.Person.HasChanges)
				{
					Globals.Message.Show(Res.GetString("84D42C7B-5BAD-4D4D-92F4-53421AE3C8C7", "Please save the form to proceed with this operation."));
					return;
				}

				if (Tree.SelectedNodes.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("d615796d-1d82-4585-baa1-b34de7157463", "Please select an association to remove from the tree."));
				}
				else
				{
					if (Globals.Message.Show(Res.GetString("928a72cd-e6b9-47fc-8381-9c316e6b898a",
						"You are about to detach the selected association(s) and attach them to a new Person. Would you like to continue?"),
						Res.GetString("942332cc-f759-443a-a75a-a32f6d6ed262", "Detach Association"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					{
						return;
					}

					var affectedStaff = new List<ZGuid>();
					var affectedContacts = new List<ZGuid>();
					var affectedApplicants = new List<ZGuid>();

					foreach (var node in Tree.SelectedNodes)
					{
						var staffWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsStaffWrapper;
						var applicantWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsJobApplicantWrapper;
						var contactWrapper = ((PersonAssociationsTreeNode)node.Tag).BizObj as PersonAssociationsOrganizationWrapper;

						if (staffWrapper != null)
						{
							affectedStaff.Add(staffWrapper.Staff.PK);
						}

						if (applicantWrapper != null)
						{
							affectedApplicants.Add(applicantWrapper.JobApplicant.PK);
						}

						if (contactWrapper != null)
						{
							affectedContacts.Add(contactWrapper.Contact.PK);
						}
					}

					if (affectedStaff.Count == 0 && affectedContacts.Count == 0 && affectedApplicants.Count == 0)
					{
						return;
					}

					SetButtonsEnable(false);

					var collectionForDefaults = new GlbPersonCollection(Model.Person, affectedStaff.ToArray(), affectedContacts.ToArray(), affectedApplicants.ToArray());
					var controller = ZControllerFactory.Create(ControllerIDs.GlbPersonNew);
					controller.SetCollectionForDefaultsAndValidation(collectionForDefaults);
					controller.SetFormsModalTo(this.ParentForm);
					var form = controller.ShowNewForm() as ZForm;

					var saved = false;
					form.Saved += (o, args) =>
					{
						saved = true;
						if (Model.Person.PrimaryRelationship != null)
						{
							if (affectedStaff.Union(affectedContacts).Contains(Model.Person.PrimaryRelationship.PPR_PrimaryId))
							{
								Model.Person.RemovePrimaryRelationship();
								Model.Person.Validation.ValidatePrimaryRelationship();
							}
						}

						Model.Person.StaffCollection.RefreshFromDb();
						Model.Person.ContactCollection.Reload(true);

						foreach (var applicant in affectedApplicants)
						{
							Model.Person.ApplicantCollection.Remove(applicant);
						}
					};

					form.FormClosed += delegate
					{
						SetButtonsEnable(true);

						if (saved)
						{
							Model.Person.HasChanges = true;
							if (Model.Person.HasChanges)
							{
								ModelView.BuildTree(false);
							}
						}
					};
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("f3bddfe9-c6bd-4cc0-9f0f-bfdec8e966bb", "You don't have enough security rights to create a Person Intelligence record."));
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= Factory_Saved;
				SetButtonsEnable(true);
			}
		}

		void SetButtonsEnable(bool enable)
		{
			NewToolStripDropDownButton.Enabled = enable;
			showInactiveCheckBox.Enabled = enable;
			AttachToolStripDropDownButton.Enabled = enable;
		}

		protected override void NewToolStripButton_Click(object sender, EventArgs e)
		{
			// TODO: Implement NewToolStripButton_Click
		}

		#endregion

		#region New

		public ZToolStripDropDownButton NewToolStripDropDownButton { get; private set; }

		void RefreshNewButtons()
		{
			if (NewToolStripDropDownButton != null)
			{
				NewToolStripDropDownButton.DropDownItems.Clear();

				if (Model != null)
				{
					var buttonStaff = new ZToolStripMenuItem();
					buttonStaff.CaptionResourceString = Res.GetData("3054198A-7652-4707-B8AE-2EF3FD5692F8", "New Staff");
					buttonStaff.Click += NewStaffButtonClick;
					NewToolStripDropDownButton.DropDownItems.Add(buttonStaff);

					var buttonApplicant = new ZToolStripMenuItem();
					buttonApplicant.CaptionResourceString = Res.GetData("208FBF52-76D0-4B96-B7C4-3FC65DC06751", "New Applicant");
					buttonApplicant.Click += NewApplicantButtonClick;
					NewToolStripDropDownButton.DropDownItems.Add(buttonApplicant);
				}
			}
		}

		GlbStaffCollection staffCollectionForDefaults;
		void NewStaffButtonClick(object sender, EventArgs eventArgs)
		{
			if (Env.Security.Staff.IsAllowed)
			{
				if (Model.Person.StaffCollection.Count == 0)
				{
					staffCollectionForDefaults = new GlbStaffCollection(Model.Person, new ZQuery());

					var controller = GetStaffController();
					controller.SetCollectionForDefaultsAndValidation(staffCollectionForDefaults);

					if (controller.ShowNewForm() is ZForm form)
					{
						form.Saved += OnStaffFormSaved;

						form.BusinessEntity.HasChanges = true;
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("3117BA9B-391A-4434-AFC8-AA0B0A03336E", "There is already a Staff for this Person."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("97E89531-F2A1-459A-BDA2-AE79234D18B7", "You don't have enough security rights to create a Staff record."));
			}
		}

		void OnStaffFormSaved(object sender, EventArgs e)
		{
			var newStaff = Model.Person.StaffCollection.FirstOrDefault();
			if (newStaff != null && Env.Security.PersonIntelligencePrimaryWorkplace.IsAllowed)
			{
				if (Model.Person.PrimaryRelationship?.Primary == null ||
					Model.Person.PrimaryRelationship.Primary.PK != newStaff.PK
					&& Globals.Message.Show(
						Res.GetString("a3a6e08c-4fa4-495a-98ea-d757d5df7ca8", "This Person's current Primary Workplace is {0}. Would you like to use the new staff's company as the Primary Workplace?",
							Model.Person.PrimaryRelationship.Primary.CompanyName),
						Res.GetString("8dc620c0-bca3-497d-9377-b1086a05c606", "Primary Workplace"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					Model.Person.SetPrimaryRelationship(newStaff);
				}
			}

			ModelView.BuildTree();
		}

		protected virtual ZController GetStaffController()
		{
			return ZControllerFactory.Create(ControllerIDs.GlbStaff);
		}

		IBusinessObjectCollection applicantCollectionForDefaults;
		void NewApplicantButtonClick(object sender, EventArgs eventArgs)
		{
			if (Env.Security.HRJobApplicantNew.IsAllowed)
			{
				if (Model.Person.ApplicantCollection.Count == 0)
				{
					applicantCollectionForDefaults =
						(IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IHRJobApplicantCollection>(),
							Model.Person, new ZQuery());

					var controller = GetApplicantController();
					controller.SetCollectionForDefaultsAndValidation(applicantCollectionForDefaults);

					if (controller.ShowNewForm() is ZForm form)
					{
						form.Saved += (o, args) =>
						{
							ModelView.BuildTree();
						};

						form.BusinessEntity.HasChanges = true;
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("2866215B-E916-4491-B767-28CFE2A2589C", "There is already an Applicant for this Person."));
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("DCD8247B-AA3D-4275-8F91-A3E275E0E797", "You don't have enough security rights to create an Applicant record."));
			}
		}

		protected virtual ZController GetApplicantController()
		{
			return ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
		}

		#endregion

		#region Edit

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
			switch (bizObjToEdit)
			{
				case PersonAssociationsOrganizationWrapper organizationWrapper:
					{
						var controller = ZControllerFactory.Create(ControllerIDs.OrgContacts);
						controller.ShowEditForm(organizationWrapper.Contact);
						break;
					}

				case PersonAssociationsStaffWrapper staffWrapper:
					{
						var controller = ZControllerFactory.Create(ControllerIDs.GlbStaff);
						controller.ShowEditForm(staffWrapper.Staff);
						break;
					}

				case PersonAssociationsJobApplicantWrapper jobApplicant:
					{
						var controller = ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
						controller.ShowEditForm((BusinessObject)jobApplicant.JobApplicant);
						break;
					}
			}
		}

		#endregion

		#region Attach

		public ZToolStripDropDownButton AttachToolStripDropDownButton { get; private set; }

		void RefreshAttachButtons()
		{
			//throw new NotImplementedException();
		}

		#endregion

		#region Show Inactive

		void LoadShowInactiveSettings()
		{
			showInactiveCheckBox.Checked = OrganisationGuiState.LoadInteger(showInactiveCheckBox, "IsChecked") == 1;
		}

		void SaveShowInactiveSettings()
		{
			OrganisationGuiState.SaveInteger(showInactiveCheckBox, "IsChecked", showInactiveCheckBox.Checked ? 1 : 0);
		}

		void SetupShowInactiveCheckBox()
		{
			showInactiveCheckBox.Checked = ModelView.ShowInactive;
		}

		void ShowInactiveCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ModelView != null)
			{
				ModelView.ShowInactive = showInactiveCheckBox.Checked;
			}
		}

		#endregion

		#region ReadOnly

		protected override void SetControlsReadOnly()
		{
			base.SetControlsReadOnly();

			NewToolStripDropDownButton.Enabled = false;
			AttachToolStripDropDownButton.Enabled = false;
		}

		#endregion
	}
}
