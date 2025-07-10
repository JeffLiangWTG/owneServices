using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesRelationControl : ZTreeViewControl
	{
		public SalesRelationControl()
		{
			InitializeComponent();

			this.createdTimeNodeDateBox.DateTimeFormat = ZDateTime.LongTimeFormat;
			this.lastEditTimeDateBox.DateTimeFormat = ZDateTime.LongTimeFormat;
			this.salesRelationColumn.Header = SalesRelationTree.RelationColumnHeader;
			this.summaryColumn.Header = SalesRelationTree.SummaryColumnHeader;
			this.lastEditTimeColumn.Header = SalesRelationTree.LastEditTimeColumn;
			this.createdTimeColumn.Header = SalesRelationTree.CreatedTimeColumn;
		}

		bool IsShowCommunicationsCheckBoxVisible;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				ReplaceWithErrorLabelIfSecurityDenied(Env.Security.SalesRelationsView, this);
				IsShowCommunicationsCheckBoxVisible = ShowCommunicationsCheckBox.Visible;
			}
		}

		protected override void OnHandleFullyCreated(EventArgs e)
		{
			base.OnHandleFullyCreated(e);

			if (!DesignModeFinder.IsDesigning)
			{
				LoadShowCommunicationSettings();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning && IsShowCommunicationsCheckBoxVisible)
			{
				SaveShowCommunicationSettings();
			}

			base.OnHandleDestroyed(e);
		}

		#region DataBinding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			var previousViewShowCommunication = ModelView != null ? ModelView.ShowCommunication : (ZBool?)null;
			var previousSalesRelationModel = CurrentDataItem as SalesRelationModel;
			if (previousSalesRelationModel != null)
			{
				previousSalesRelationModel.ChangeNodeParentFailed -= salesRelationModel_ChangeNodeParentFailed;
			}

			base.OnCurrentDataItemChanged(e);

			var newSalesRelationModel = CurrentDataItem as SalesRelationModel;
			if (newSalesRelationModel != null)
			{
				if (previousViewShowCommunication.HasValue)
				{
					ModelView.ShowCommunication = previousViewShowCommunication.Value;
				}

				SetupShowCommunicationsCheckBox();
				newSalesRelationModel.ChangeNodeParentFailed += salesRelationModel_ChangeNodeParentFailed;
			}

			RefreshAttachActivityButtons();
			RefreshNewActivityButtons();
		}

		void salesRelationModel_ChangeNodeParentFailed(object sender, ZTreeModel<IRelatableActivity>.ChangeNodeParentFailedArgs e)
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("de40db49-3a28-493f-95f6-c4b012d15ba4", "Could not update parent - {0}", e.ChangeResult.Reason));
		}

		#endregion

		#region View/Model

		protected override ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new SalesRelationTree();
		}

		public new SalesRelationModelView ModelView
		{
			get { return (SalesRelationModelView)base.ModelView; }
		}

		protected override IZTreeModelView GetNewTreeModelView()
		{
			return new SalesRelationModelView((SalesRelationModel)CurrentDataItem);
		}

		protected SalesRelationModel Model
		{
			get { return CurrentDataItem as SalesRelationModel; }
		}

		protected IRelatableActivity MasterActivity
		{
			get { return Model != null ? Model.Master : null; }
		}

		#endregion

		#region Tree

		protected override void SetupTree()
		{
			base.SetupTree();

			typeNodeComboBox.DrawText += TypeNodeComboBox_DrawText;
			uniqueIDNodeTextBox.DrawText += (s, e) => { SetMasterActivityText(e); };
			summaryNodeTextBox.DrawText += (s, e) => { SetMasterActivityText(e); };
			createdTimeNodeDateBox.DrawText += (s, e) => { SetMasterActivityText(e); };
			lastEditTimeDateBox.DrawText += (s, e) => { SetMasterActivityText(e); };
			Tree.SetSortColumn(createdTimeColumn, System.Windows.Forms.SortOrder.Ascending);
		}

		protected override Brush GetRowBackgroundBrush(TreeNodeAdv node)
		{
			var brush = base.GetRowBackgroundBrush(node);
			if (brush == null && ((SalesRelationNode)node.Tag).BizObj is OrgSalesCall)
			{
				brush = SystemBrushes.Menu;
			}

			return brush;
		}

		void TypeNodeComboBox_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			e.Font = new Font(e.Font, FontStyle.Bold);
			SetMasterActivityText(e);
		}

		void SetMasterActivityText(Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{
			var salesRelationNode = e.Node.Tag as SalesRelationNode;
			if (salesRelationNode.BizObj == MasterActivity)
			{
				e.Font = new Font(e.Font, FontStyle.Bold);
				if (!Tree.SelectedNodes.Any(node => node.Tag == salesRelationNode))
				{
					e.TextColor = Color.Blue;
#if WINZOR
					e.Node.IsMasterActivitySalesRelation = true;
#endif
				}
			}
		}

		void ReplaceWithErrorLabelIfSecurityDenied(SecurityCheckpoint checkpoint, Control control)
		{
			if (!checkpoint.IsAllowed)
			{
				while (control.Controls.Count > 0)
				{
					control.Controls[0].Dispose();
				}

				var coveringLabel = new ZLabel();
				coveringLabel.Dock = DockStyle.Fill;
				coveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				coveringLabel.Text = checkpoint.ErrorMessageForNotAllowed;
				control.Controls.Add(coveringLabel);
				coveringLabel.BringToFront();
			}
		}

		#endregion

		#region Buttons

		protected override void SetupButtons()
		{
			base.SetupButtons();

			NewToolStripDropDownButton = new ZToolStripDropDownButton();
			CopyToolStripButtonAppearance(NewToolStripButton, NewToolStripDropDownButton);
			ReplaceToolStripButton(bottomToolStrip.Items, NewToolStripButton, NewToolStripDropDownButton);

			AttachToolStripDropDownButton = new ZToolStripDropDownButton();
			CopyToolStripButtonAppearance(AttachToolStripButton, AttachToolStripDropDownButton);
			ReplaceToolStripButton(bottomToolStrip.Items, AttachToolStripButton, AttachToolStripDropDownButton);

			RefreshNewActivityButtons();
			RefreshAttachActivityButtons();
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

		#endregion

		#region Edit Activity

		protected override void ShowEditForm(IBusiness bizObjToEdit)
		{
			var campaignItem = bizObjToEdit as IGlbCompanyCampaignItem;
			if (campaignItem != null)
			{
				IGlbCompanyCampaignController controller = (IGlbCompanyCampaignController)ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign);
#if DEBUG
				SetLastControllerForTest((ZController)controller);
#endif
				controller.ShowEditFormButFocusOnCampaignItem(campaignItem);
			}
			else
			{
				ShowEditFormCore(bizObjToEdit);
			}
		}

		void ShowEditFormCore(IBusiness bizObjToEdit)
		{
			var activityToEdit = bizObjToEdit as IRelatableActivity;
			if (AllowShowEditFormForMasterActivity || activityToEdit != MasterActivity)
			{
				RelatableActivityTypeDefinition typeDef;
				if (RelatableActivityTypeDefinitions.TryGetValue(activityToEdit.ActivityType, out typeDef))
				{
					if (typeDef.ControllerId == ControllerIDs.Quotations)
					{
						var module = ZModuleFactory.Instance.Create(ModuleIDs.Quotations) as IQuotationsModule;
						module.ShowQuoteEditForm((IQuote)activityToEdit);
					}
					else
					{
						var controller = ZControllerFactory.Create(typeDef.ControllerId);
#if DEBUG
						SetLastControllerForTest(controller);
#endif
						controller.ShowEditForm((BusinessObject)activityToEdit);
					}
				}
			}
		}
#if DEBUG
		public ZController LastController_ForTestOnly;

		void SetLastControllerForTest(ZController controller)
		{
			if (Globals.IsTest)
			{
				LastController_ForTestOnly = controller;
			}
		}
#endif

		protected virtual bool AllowShowEditFormForMasterActivity => false;

		#endregion

		#region New Activity

		public ZToolStripDropDownButton NewToolStripDropDownButton { get; private set; }

		void RefreshNewActivityButtons()
		{
			if (NewToolStripDropDownButton != null)
			{
				NewToolStripDropDownButton.DropDownItems.Clear();

				if (MasterActivity != null)
				{
					foreach (var relatableTypeButtonCaptionPair in RelatableTypeButtonCaptionPairs)
					{
						if (MasterActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(relatableTypeButtonCaptionPair.Key).IsValid)
						{
							if (MasterActivity is IQuote && relatableTypeButtonCaptionPair.Key == RelatableActivityTypeList.Codes.Quotations)
							{
								var dropdownButton = new ZToolStripMenuItem();
								dropdownButton.CaptionResourceString = relatableTypeButtonCaptionPair.Value;
								dropdownButton.DropDownDirection = ToolStripDropDownDirection.Right;
								NewToolStripDropDownButton.DropDownItems.Add(dropdownButton);

								var subDropDownItemNew = new ZToolStripMenuItem();
								subDropDownItemNew.CaptionResourceString = Res.GetData("436ae043-2b9a-479a-9e1f-28d5c32faa45", "New For Same Client");
								subDropDownItemNew.Click += GetNewActivityMenuItemHandler(relatableTypeButtonCaptionPair.Key);
								dropdownButton.DropDownItems.Add(subDropDownItemNew);

								var subDropDownItemAmendment = new ZToolStripMenuItem();
								subDropDownItemAmendment.CaptionResourceString = Res.GetData("ac66a2f4-b23a-41e4-9b02-6a39f285120c", "Amendment For Same Client");
								subDropDownItemAmendment.Click += GetNewActivityMenuItemHandler(relatableTypeButtonCaptionPair.Key, SalesRelationActionTypes.AmendmentForSameClient);
								dropdownButton.DropDownItems.Add(subDropDownItemAmendment);

								var subDropDownItemCopy = new ZToolStripMenuItem();
								subDropDownItemCopy.CaptionResourceString = Res.GetData("117d7b36-8729-4a90-bae5-fa30b06c3e97", "Copy For Same Client");
								subDropDownItemCopy.Click += GetNewActivityMenuItemHandler(relatableTypeButtonCaptionPair.Key, SalesRelationActionTypes.CopyForSameClient);
								dropdownButton.DropDownItems.Add(subDropDownItemCopy);
							}
							else if (MasterActivity is OrgOpportunity && relatableTypeButtonCaptionPair.Key == RelatableActivityTypeList.Codes.Quotations)
							{
								var dropdownItem = new ZToolStripMenuItem();
								dropdownItem.CaptionResourceString = relatableTypeButtonCaptionPair.Value;
								dropdownItem.Click += OnNewQuoteFromOpportunityMenuItemClick;
								NewToolStripDropDownButton.DropDownItems.Add(dropdownItem);
							}
							else
							{
								var dropdownItem = new ZToolStripMenuItem();
								dropdownItem.CaptionResourceString = relatableTypeButtonCaptionPair.Value;
								dropdownItem.Click += GetNewActivityMenuItemHandler(relatableTypeButtonCaptionPair.Key);
								NewToolStripDropDownButton.DropDownItems.Add(dropdownItem);
							}
						}
					}
				}
			}
		}

		protected virtual IEnumerable<KeyValuePair<string, ResourceStringData>> RelatableTypeButtonCaptionPairs
		{
			get { return relatableTypeButtonCaptionPairs; }
		}

		readonly KeyValuePair<string, ResourceStringData>[] relatableTypeButtonCaptionPairs = new KeyValuePair<string, ResourceStringData>[]
		{
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.InquiryManager, Res.GetData("d5fda479-3c18-4ebc-aa87-9c6b60ccc746", "Inquiry")),
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.CampaignManagement, Res.GetData("414c86a2-3aff-4b91-b48f-40c42afb6632", "Campaign")),
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.OpportunityManager, Res.GetData("e73cae69-6860-4040-93e8-43a6e396c8d5", "Opportunity")),
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.Quotations, Res.GetData("c95603f1-7e12-453c-9f15-3416d2bf808d", "Quotation")),
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.OneOffQuotes, Res.GetData("bb4f48bd-124e-4356-8cbf-47eccab4e717", "One Off Quote")),
			new KeyValuePair<string, ResourceStringData>(RelatableActivityTypeList.Codes.Projects, Res.GetData("06704ee8-b63d-454b-8eaf-db59e91cc76d", "Project"))
		};

		public void InvokeNewActivityMenuItem(string salesRelationType)
		{
			GetNewActivityMenuItemHandler(salesRelationType)(this, EventArgs.Empty);
		}

		protected enum SalesRelationActionTypes
		{
			NewForSameClient,
			AmendmentForSameClient,
			CopyForSameClient
		}

		protected EventHandler GetNewActivityMenuItemHandler(string salesRelationType, SalesRelationActionTypes actionType = SalesRelationActionTypes.NewForSameClient)
		{
			return (object sender, EventArgs e) =>
			{
				if (!MasterActivity.IsInDatabase)
				{
					ShowCanNotCreateRelationshipError(RelatedParentActivityPivotCollection.GetParentMustBeSavedMessage(MasterActivity));
					return;
				}

				var childTypeValidationResult = MasterActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(salesRelationType);
				if (!childTypeValidationResult.IsValid)
				{
					ShowCanNotCreateRelationshipError(childTypeValidationResult.Reason);
					return;
				}

				if (!Env.Security.SalesRelationsNew.IsAllowed)
				{
					Env.Security.SalesRelationsNew.ShowError();
					return;
				}

				var controller = ZControllerFactory.Create(RelatableActivityTypeDefinitions[salesRelationType].ControllerId);
				if (controller != null)
				{
					IBusiness newEntity = GetNewEntity(salesRelationType, actionType, controller);
					CreateRelationshipAndShowFormForNewEntity(controller, newEntity);
				}
			};
		}

		IBusiness GetNewEntity(string salesRelationType, SalesRelationActionTypes actionType, ZController controller)
		{
			IBusiness newEntity = null;

			var quotedBookingController = controller as IQuotedBookingController;
			if (quotedBookingController != null)
			{
				quotedBookingController.SetQuotedBookingState(QuotedBookingState.QuoteOnly);
			}

			if (actionType == SalesRelationActionTypes.NewForSameClient)
			{
				newEntity = ((ZControllerInternals)controller).GetNewBusinessEntityInLocalFactory();
			}
			else if (salesRelationType == RelatableActivityTypeList.Codes.Quotations)
			{
				var quote = new BusinessObjectFactory().Load(ObjectFactory.Get<IRating>().QuoteType, MasterActivity.PK) as IQuote;
				if (quote != null)
				{
					var quoteController = controller as IQuotationsController;
					try
					{
						if (quoteController.CheckIsCopyAllowed((BusinessObject)quote))
						{
							quote.SameClientCopy = true;
							quote.AmendmentCopy = (actionType == SalesRelationActionTypes.AmendmentForSameClient);
							newEntity = ((ITemplateCopyable)quote).TemplateCopy();
						}
					}
					finally
					{
						quote.SameClientCopy = false;
						quote.AmendmentCopy = false;
					}
				}
			}

			return newEntity;
		}

		protected virtual void CreateRelationshipAndShowFormForNewEntity(ZController controller, IBusiness newEntity)
		{
			var newRelatableActivity = newEntity as IRelatableActivity;
			if (newRelatableActivity != null)
			{
				var shouldShow = true;

				if (!newRelatableActivity.RelatedParentActivityPivotCollection.HasParent(MasterActivity))
				{
					using (newRelatableActivity.RelatedParentActivityPivotCollection.TemporarilyIgnoreSuperAndSubActivityRelationships())
					{
						var addParentResult = newRelatableActivity.RelatedParentActivityPivotCollection.AddActivity(MasterActivity);
						if (!addParentResult.Success)
						{
							shouldShow = false;
							ShowCanNotCreateRelationshipError(addParentResult.Reason);
						}
					}
				}

				if (shouldShow)
				{
					var formShowingForActivityArgs = new FormShowingForActivityArgs(newRelatableActivity);
					OnFormShowingForNewActivity(formShowingForActivityArgs);
					if (formShowingForActivityArgs.Cancelled)
					{
						shouldShow = false;
					}
				}

				if (shouldShow)
				{
					var caption = GetNewChildCaption(newRelatableActivity, MasterActivity);
					var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)ParentForm, caption);
					if (!SalesRelationTree.DoImportParentRelatedActivityInfoOnNewActions(MasterActivity, newRelatableActivity, deciderFactory))
					{
						shouldShow = false;
					}
				}

				if (shouldShow)
				{
					controller.SetFormsModalTo(ParentForm);
					NewFormEntity = (ZForm)controller.ShowFormForNewEntity(newEntity);

					if (NewFormEntity != null)
					{
						NewFormEntity.Shown += newForm_Shown;
						NewFormEntity.Saved += newForm_Saved;
						NewFormEntity.FormClosed += newForm_FormClosed;
					}
					else
					{
						newEntity.Delete();
					}
				}
				else
				{
					newEntity.Delete();
				}
			}
		}

		ZForm NewFormEntity;

#if DEBUG
		protected IBusiness NewFormEntityBusinessEntity => NewFormEntity.BusinessEntity;

		protected void InvokeNewFormSavedForTest()
		{
			newForm_Saved(NewFormEntity, EventArgs.Empty);
		}
#endif

		void OnNewQuoteFromOpportunityMenuItemClick(object sender, EventArgs e)
		{
			var opportunity = MasterActivity as OrgOpportunity;
			if (opportunity != null)
			{
				var controller = ObjectFactory.Get<IGenerateQuoteForSalesValueAssociatedEntityController>();
				if (typeof(ZForm).IsAssignableFrom(ParentForm.GetType()))
				{
					controller.ParentModalForm = (ZForm)ParentForm;
				}
				controller.Execute(opportunity);
			}
		}

		#region FormShowingForNewActivity

		protected virtual void OnFormShowingForNewActivity(FormShowingForActivityArgs e)
		{
			if (FormShowingForNewActivity != null)
			{
				FormShowingForNewActivity(this, e);
			}
		}
		public event EventHandler<FormShowingForActivityArgs> FormShowingForNewActivity;

		public class FormShowingForActivityArgs : EventArgs
		{
			public FormShowingForActivityArgs(IRelatableActivity activity)
			{
				Cancelled = false;
				Activity = activity;
			}

			public bool Cancelled;
			public readonly IRelatableActivity Activity;
		}

		#endregion

		#region FormShownForNewActivity

		protected void OnFormShownForNewActivity(FormShownForActivityArgs e)
		{
			if (FormShownForNewActivity != null)
			{
				FormShownForNewActivity(this, e);
			}
		}
		public event EventHandler<FormShownForActivityArgs> FormShownForNewActivity;

		public class FormShownForActivityArgs : EventArgs
		{
			public FormShownForActivityArgs(ZForm form)
			{
				Form = form;
			}

			public readonly ZForm Form;
		}

		#endregion

		void newForm_Shown(object sender, EventArgs e)
		{
			var zForm = (ZForm)sender;
			zForm.Shown -= newForm_Shown;
			OnFormShownForNewActivity(new FormShownForActivityArgs(zForm));
		}

		void newForm_Saved(object sender, EventArgs e)
		{
			var zForm = (ZForm)sender;
			zForm.Saved -= newForm_Saved;
			zForm.FormClosed -= newForm_FormClosed;

			var formActivity = zForm.BusinessEntity as IRelatableActivity;
			if (formActivity != null)
			{
				var caption = GetNewChildCaption(formActivity, MasterActivity);
				var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)ParentForm, caption);
				SalesRelationTree.DoImportChildRelatedActivityInfoOnNewSavedActions(Model.Master, formActivity, deciderFactory);
			}

			var quote = MasterActivity as IQuote;
			if (quote != null && quote.TH_IsCancelled)
			{
				((BusinessObject)quote).SetReadOnlyIncludingChildren(true);
			}
		}

		void newForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			var zForm = (ZForm)sender;
			zForm.Saved -= newForm_Saved;
			zForm.FormClosed -= newForm_FormClosed;
		}

		#endregion

		#region Attach Relation

		#region SetupAttachActivityButton

		public ZToolStripDropDownButton AttachToolStripDropDownButton { get; private set; }

		void RefreshAttachActivityButtons()
		{
			if (AttachToolStripDropDownButton != null)
			{
				AttachToolStripDropDownButton.DropDownItems.Clear();

				if (MasterActivity != null)
				{
					foreach (var relatableTypeButtonCaptionPair in RelatableTypeButtonCaptionPairs)
					{
						if (MasterActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(relatableTypeButtonCaptionPair.Key).IsValid)
						{
							var dropdownItem = new ZToolStripMenuItem();
							dropdownItem.CaptionResourceString = relatableTypeButtonCaptionPair.Value;
							dropdownItem.Click += GetAttachActivityMenuItemHandler(relatableTypeButtonCaptionPair.Key);
							if (SaveDataHandler != null)
							{
								dropdownItem.Click += SaveDataHandler;
							}

							AttachToolStripDropDownButton.DropDownItems.Add(dropdownItem);
						}
					}
				}
			}
		}

		protected virtual EventHandler SaveDataHandler
		{
			get { return null; }
		}

		EventHandler GetAttachActivityMenuItemHandler(string salesRelationType)
		{
			return (object sender, EventArgs e) =>
			{
				if (!MasterActivity.IsInDatabase)
				{
					ShowCanNotCreateRelationshipError(RelatedParentActivityPivotCollection.GetParentMustBeSavedMessage(MasterActivity));
					return;
				}

				var childTypeValidationResult = MasterActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(salesRelationType);
				if (childTypeValidationResult.IsValid)
				{
					if (Env.Security.SalesRelationsNew.IsAllowed)
					{
						var popupForm = GetNewAttachPopupForm(salesRelationType);
						ZFormModaliser.ShowDialogAndDispose(popupForm);
					}
					else
					{
						Env.Security.SalesRelationsNew.ShowError();
					}
				}
				else
				{
					ShowCanNotCreateRelationshipError(childTypeValidationResult.Reason);
				}
			};
		}

		EmbeddedModulePopup GetNewAttachPopupForm(string salesRelationType)
		{
			EmbeddedModulePopup popup = null;
			ZFilterModule module = NewModuleFromModuleID(RelatableActivityTypeDefinitions[salesRelationType].ModuleId);
			if (module != null)
			{
				var moduleDecisionProvider = new AddRelatedActivityModuleDecisionProvider(Model.MasterNode);
				module.OverrideModuleDecisionProvider(moduleDecisionProvider);
				popup = new EmbeddedModulePopup(module);
				moduleDecisionProvider.Popup = popup;
			}

			return popup;
		}

		ZFilterModule NewModuleFromModuleID(ModuleIdentifier moduleID)
		{
			ZFilterModule module = null;

			if (moduleID != null && moduleID != ModuleIDs.NotAssigned)
			{
				ZModule tempModule = ZModuleFactory.Instance.Create(moduleID)
					?? throw new ZException("ZModuleFactory did not return a module for ID : " + moduleID.ToString());

				module = tempModule as ZFilterModule;
				if (module == null)
				{
					tempModule.Dispose();
					throw new ZException("Module ID : " + moduleID.ToString() + " resolves to null. FindBox Modules must be ZEmbeddedModules.");
				}
			}

			return module;
		}

		#endregion

		#endregion

		#region Detach Relation

		protected override void DetachSelectedElements()
		{
			if (Tree.SelectedNodes.Count == 0)
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("379be1c4-0291-4ed5-b9a4-d11e57709ad0", "Please select an item in the grid."), DialogCaptionForDetachNode);
			}
			else
			{
				if (ModelView == null || ModelView.SecurityCheckpointForEdit.IsAllowed)
				{
					var validNodes = new List<TreeNodeAdv>();
					var invalidActivities = new List<IRelatableActivity>();
					PopulateInfoForDetach(validNodes, invalidActivities);

					if (validNodes.Count > 0)
					{
						PromptAndDetachNodes(validNodes, invalidActivities);
					}
					else
					{
						ShowInvalidMessageForDetachIfRequired(invalidActivities);
					}
				}
				else
				{
					ModelView.SecurityCheckpointForEdit.ShowError();
				}
			}
		}

		void PopulateInfoForDetach(List<TreeNodeAdv> validNodes, List<IRelatableActivity> invalidActivities)
		{
			foreach (var node in Tree.SelectedNodes.ToArray())
			{
				var childActivity = ((SalesRelationNode)node.Tag).BizObj;
				if (node.Parent.Tag == null)
				{
					invalidActivities.Add(childActivity);
					continue;
				}

				var parentActivity = ((SalesRelationNode)(node.Parent).Tag).BizObj;
				var relatedActivityPivot = parentActivity.RelatedChildActivityPivotCollection.FindPivot(childActivity);
				if (relatedActivityPivot != null && !relatedActivityPivot.IsEditable)
				{
					invalidActivities.Add(childActivity);
				}
				else
				{
					validNodes.Add(node);
				}
			}
		}

		void PromptAndDetachNodes(List<TreeNodeAdv> validNodes, List<IRelatableActivity> invalidActivities)
		{
			var answer = Globals.Message.Show(DialogMessageForDetachNode, DialogCaptionForDetachNode, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (answer == DialogResult.OK)
			{
				foreach (var node in validNodes)
				{
					var parentActivity = ((SalesRelationNode)(node.Parent).Tag).BizObj;
					var childActivity = ((SalesRelationNode)node.Tag).BizObj;
					var caption = GetDetachingCaption(childActivity, parentActivity);
					var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)ParentForm, caption);
					if (SalesRelationTree.DoImportChildRelatedActivityInfoOnDetachActions(parentActivity, childActivity, deciderFactory))
					{
						((IZNode)node.Tag).RemoveParent();
					}
				}
				ShowInvalidMessageForDetachIfRequired(invalidActivities);
			}
		}

		void ShowInvalidMessageForDetachIfRequired(List<IRelatableActivity> invalidActivities)
		{
			if (invalidActivities.Any())
			{
				var message = new ZStringBuilder(ResString.GetMultilingualString("768658B8-1FB6-4CD8-9C80-A2619AC8F604", "Unable to detach the following {0}:", NameOfTreeElementsPlural.Caption));
				foreach (var activity in invalidActivities)
				{
					message.AppendLine();
					message.Append($"• {activity.Summary}");
				}
				Globals.Message.ShowInformation(message.ToString(), DialogCaptionForDetachNode);
			}
		}

		#endregion

		#region Show/Hide Communication

		void LoadShowCommunicationSettings()
		{
			ShowCommunicationsCheckBox.Checked = OrganisationGuiState.LoadInteger(ShowCommunicationsCheckBox, "IsChecked") == 1;
		}

		void SaveShowCommunicationSettings()
		{
			OrganisationGuiState.SaveInteger(ShowCommunicationsCheckBox, "IsChecked", ShowCommunicationsCheckBox.Checked ? 1 : 0);
		}

		void SetupShowCommunicationsCheckBox()
		{
			ShowCommunicationsCheckBox.Checked = ModelView.ShowCommunication;
		}

		void ShowCommunicationsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ModelView != null)
			{
				ModelView.ShowCommunication = ShowCommunicationsCheckBox.Checked;
			}
		}

		#endregion

		#region Popup

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(true)]
		public bool ShowPopupButton
		{
			get { return popupToolStripButton.Visible; }
			set { popupToolStripButton.Visible = value; }
		}

		void popupToolStripButton_Click(object sender, EventArgs e)
		{
			var model = Model;
			if (model != null)
			{
				var form = new SalesRelationPopupForm(model);
				form.Shown += (shownForm, formShownArgs) =>
					{
						((SalesRelationPopupForm)shownForm).SalesRelationControl.ShowCommunicationsCheckBox.Checked = this.ShowCommunicationsCheckBox.Checked;
					};
				ZFormModaliser.Show(form, ParentForm);
			}
		}

		#endregion

		#region ReadOnly

		protected override void SetControlsReadOnly()
		{
			base.SetControlsReadOnly();

			AttachToolStripDropDownButton.Enabled = false;
			NewToolStripDropDownButton.Enabled = false;
		}

		#endregion

		#region Lookups

		IDictionary<string, RelatableActivityTypeDefinition> RelatableActivityTypeDefinitions
		{
			get
			{
				if (relatableActivityTypeDefinitions == null && Model != null)
				{
					relatableActivityTypeDefinitions = RelatedActivityLinkLookups.GetRelatableActivityTypeDefinitions(Model.Factory);
				}

				return relatableActivityTypeDefinitions;
			}
		}
		IDictionary<string, RelatableActivityTypeDefinition> relatableActivityTypeDefinitions;

		#endregion

		#region Messages

		protected override ResourceStringData DefaultNameOfATreeElement
		{
			get { return SalesRelationTree.NameOfATreeElement; }
		}

		protected override ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return SalesRelationTree.NameOfTreeElementsPlural; }
		}

		static void ShowCanNotCreateRelationshipError(string message)
		{
			Globals.Message.ShowError(message, CanNotCreateRelationshipCaption);
		}

		public static ResourceString GetNewChildCaption(IRelatableActivity childActivity, IRelatableActivity parentActivity)
		{
			return ResString.GetMultilingualString("63ff78db-5d93-46ae-8906-db41e0235e0b", "New child {0} for {1}", childActivity.HumanReadableName, parentActivity.HumanReadableName);
		}

		public static ResourceString GetAttachingCaption(IRelatableActivity childActivity, IRelatableActivity parentActivity)
		{
			return ResString.GetMultilingualString("c8059117-7885-4f28-beaf-79169719e0d5", "Attaching {0} to {1}", childActivity.HumanReadableName, parentActivity.HumanReadableName);
		}

		public static ResourceString GetDetachingCaption(IRelatableActivity childActivity, IRelatableActivity parentActivity)
		{
			return ResString.GetMultilingualString("2205fc69-7f2c-4e39-ba1b-582d852d8bf9", "Detaching {0} from {1}", childActivity.HumanReadableName, parentActivity.HumanReadableName);
		}

		static ResourceString CanNotCreateRelationshipCaption
		{
			get { return ResString.GetMultilingualString("55a338d8-6221-4da3-a8f2-b39cd2982bdc", "Can not create relationship"); }
		}

		#endregion

		#region Classes

		internal class AddRelatedActivityModuleDecisionProvider : ZNodeModuleDecisionProvider<IRelatableActivity>
		{
			public AddRelatedActivityModuleDecisionProvider(ZNode<IRelatableActivity> parentActivityNode)
				: base(parentActivityNode)
			{
			}

			protected override bool TryAddChildren(IEnumerable<IRelatableActivity> children)
			{
				var added = false;
				var factory = parentNode.BizObj.Factory;

				foreach (var child in children)
				{
					var reloadedChild = factory.Load(child.TablePrefix, child.PK) as IRelatableActivity;
					if (reloadedChild != null)
					{
						var newChildNode = parentNode.AddNewChild(reloadedChild);
						if (newChildNode != null)
						{
							var parentActivity = parentNode.BizObj;
							var childActivity = newChildNode.BizObj;
							var caption = GetAttachingCaption(childActivity, parentActivity);
							var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)Popup.ParentForm, caption);
							if (SalesRelationTree.DoImportChildRelatedActivityInfoOnAttachActions(parentActivity, childActivity, deciderFactory))
							{
								added = true;
							}
							else
							{
								newChildNode.SetParentNode(null, false);
							}
						}
					}
				}

				return added;
			}
		}

		#endregion
	}
}
