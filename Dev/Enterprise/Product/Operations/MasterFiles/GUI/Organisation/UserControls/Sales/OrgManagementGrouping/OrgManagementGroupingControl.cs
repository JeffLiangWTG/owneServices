using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgManagementGroupingControl : ZTreeViewControl
	{
		public OrgManagementGroupingControl()
		{
			InitializeComponent();

			this.RelationColumn.Header = SalesRelationTree.RelationColumnHeader;
		}

		#region DataBinding

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			var previousModel = CurrentDataItem as OrgManagementGroupingModel;
			if (previousModel != null)
			{
				previousModel.ChangeNodeParentFailed -= Model_ChangeNodeParentFailed;
			}

			base.OnCurrentDataItemChanged(e);

			var model = CurrentDataItem as OrgManagementGroupingModel;
			if (model != null)
			{
				model.ChangeNodeParentFailed += Model_ChangeNodeParentFailed;
			}
		}

		void Model_ChangeNodeParentFailed(object sender, ZTreeModel<OrgHeader>.ChangeNodeParentFailedArgs e)
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("de40db49-3a28-493f-95f6-c4b012d15ba4", "Could not update parent - {0}", e.ChangeResult.Reason));
		}

		#endregion

		#region View/Model

		protected override IZTreeModelView GetNewTreeModelView()
		{
			return new OrgManagementGroupingModelView((OrgManagementGroupingModel)CurrentDataItem);
		}

		public OrgManagementGroupingModel Model
		{
			get { return CurrentDataItem as OrgManagementGroupingModel; }
		}

		OrgHeader MasterOrganisation
		{
			get { return Model != null ? Model.Master : null; }
		}

		#endregion

		#region Visual Appearance

		protected override void SetupTree()
		{
			base.SetupTree();

			ClientCodeTextBox.DrawText += (s, e) => { SetMasterOrganisationText(e); };
			ClientNameTextBox.DrawText += (s, e) => { SetMasterOrganisationText(e); };

			Tree.SetSortColumn(RelationColumn, System.Windows.Forms.SortOrder.Ascending);
		}

		void SetMasterOrganisationText(Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var orgManagementGroupingNode = e.Node.Tag as OrgManagementGroupingNode;
			if (orgManagementGroupingNode.BizObj == MasterOrganisation)
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
				if (!Tree.SelectedNodes.Any(node => node.Tag == orgManagementGroupingNode))
				{
					e.TextColor = Color.Blue;
				}
			}
		}

		#endregion

		#region Edit Organisation

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
			if (bizObjToEdit != MasterOrganisation)
			{
				ShowEditFormCore(bizObjToEdit);
			}
		}

		internal IZForm ShowEditFormCore(IBusiness bizObjToEdit)
		{
			IOrganisationController controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
			return controller.ShowForm((BusinessObject)bizObjToEdit, OrganisationTabPages.Sales_ClientRelationship, FormAction.Edit);
		}

		#endregion

		#region Detach Organisation Relation
		protected override void DetachSelectedElements()
		{
			var nodesCount = Tree.AllNodes.Count();
			base.DetachSelectedElements();
			if (Tree.AllNodes.Count() < nodesCount)
			{
				MasterOrganisation.FindDuplicates();
			}
		}
		#endregion

		#region Attach Organisation Relation

		protected override void AttachToolStripButton_Click(object sender, System.EventArgs e)
		{
			if (Env.Security.OrgDetailsModifyRelatedParties.IsAllowed)
			{
				if (!MasterOrganisation.IsInDatabase)
				{
					ShowCanNotCreateRelationshipError(OrgManagementRelatedParentCollection.GetParentMustBeSavedMessage(MasterOrganisation));
				}
				else
				{
					var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
					if (controller != null)
					{
						var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Organisation);
						if (module != null)
						{
							var moduleDecisionProvider = AddRelatedManagementOrganisationModuleDecisionProvider.New(Model.MasterNode);
							module.OverrideModuleDecisionProvider(moduleDecisionProvider);
							var popup = new EmbeddedModulePopup(module);
							moduleDecisionProvider.Popup = popup;
							((OrgHeaderCollection)module.GridCollection).SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("6899dc16-92b5-446f-adaf-3b3a860d0be2", "Organization Exists already"));
							if (popup != null)
							{
								var nodesCount = Tree.AllNodes.Count();
								ZFormModaliser.ShowDialogAndDispose(popup);
								if (Tree.AllNodes.Count() > nodesCount)
								{
									MasterOrganisation.FindDuplicates();
								}
							}
						}
					}
				}
			}
			else
			{
				Env.Security.OrgDetailsModifyRelatedParties.ShowError();
			}
		}

		#endregion

		#region Messages

		protected override ResourceStringData DefaultNameOfATreeElement
		{
			get { return Res.GetData("25f13800-c251-4c06-bec5-5bf0dd2f2403", "Organization"); }
		}

		protected override ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return Res.GetData("dd07a2a8-d4cb-453a-b7db-254133628d37", "Organizations"); }
		}

		static void ShowCanNotCreateRelationshipError(string message)
		{
			Globals.Message.ShowError(message, CanNotCreateRelationshipCaption);
		}

		static ResourceString CanNotCreateRelationshipCaption
		{
			get { return ResString.GetMultilingualString("1EB24711-43F7-490C-8191-8161EA7112E3", "Can not create relation"); }
		}

		#endregion
	}
}
