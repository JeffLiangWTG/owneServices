using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public class RelatedActivityButtonGrid : ZModuleButtonGrid
	{
		public RelatedActivityButtonGrid()
		{
			Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0);

			NewSalesActivityButtonText = DefaultNewSalesActivityButtonText;
			NewCommunicationButtonText = DefaultNewCommunicationButtonText;
			NameOfAGridElement = NameOfAGridActivity;
			this.GridId = "GridLayout+nyZMGAgtGaC5tMaNrbcTg==";
			InnerGrid.AllowReadOnlyRowsToBeDeleted = true;
			InnerGrid.DeleteMenuItem.Text = Res.GetString("E3BA41A1-A3C0-4148-B8C1-383D8FBE37EB", "Detach Activity");
		}

		#region Buttons

		protected override IList<ToolStripItem> CreateButtons()
		{
			return new List<ToolStripItem>()
			{
				CreateButton(ZModuleButtonGrid.Buttons.Edit, Icons.GetImage(IconTypes.EditButtonRest))
			};
		}

		protected override void InitializeButtons()
		{
			base.InitializeButtons();

			toolStrip.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 0, 0);

			AddNewCommunicationButton();
			AddNewSalesActivityButton();
			SetupEditButton();
		}

		protected override void UpdateButtonsReadOnly()
		{
			base.UpdateButtonsReadOnly();

			if (NewSalesActivityButton != null)
			{
				NewSalesActivityButton.Enabled = !ButtonsReadOnly && !IsListReadOnly;
			}

			if (NewCommunicationButton != null)
			{
				NewCommunicationButton.Enabled = !ButtonsReadOnly && !IsListReadOnly;
			}
		}

		#region Text

		#region NewSalesActivityButton

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData NewSalesActivityButtonText
		{
			get { return NewSalesActivityButton != null ? NewSalesActivityButton.CaptionResourceString : null; }
			set
			{
				if (NewSalesActivityButton != null)
				{
					NewSalesActivityButton.CaptionResourceString = value;
				}
			}
		}

		protected bool ShouldSerializeNewSalesActivityButtonText()
		{
			return !NewSalesActivityButtonText.Equals(DefaultNewSalesActivityButtonText);
		}

		ResourceStringData DefaultNewSalesActivityButtonText { get { return Res.GetData("9d8c5d58-7178-4d4d-8bdb-2f2229518660", "New"); } }

		#endregion

		#region NewCommunicationButton

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public ResourceStringData NewCommunicationButtonText
		{
			get { return NewCommunicationButton != null ? NewCommunicationButton.CaptionResourceString : null; }
			set
			{
				if (NewCommunicationButton != null)
				{
					NewCommunicationButton.CaptionResourceString = value;
				}
			}
		}

		protected bool ShouldSerializeNewCommunicationButtonText()
		{
			return !NewCommunicationButtonText.Equals(DefaultNewCommunicationButtonText);
		}

		ResourceStringData DefaultNewCommunicationButtonText { get { return Res.GetData("344432C9-8275-4F21-B89E-CE3306A26630", "New Communication"); } }

		#endregion

		#region NameOfAGrid

		ResourceStringData NameOfAGridActivity
		{
			get { return Res.GetData("67627324-21bc-4c29-8cca-08231a950017", "Activity"); }
		}

		protected override bool ShouldSerializeNameOfAGridElement()
		{
			return !NameOfAGridElement.Equals(NameOfAGridActivity);
		}

		#endregion

		#endregion

		public new static class Buttons
		{
			public static string NewSalesActivity = "NewSalesActivityButton";
			public static string NewCommunication = "NewCommunicationButton";
			public static string Edit = ZModuleButtonGrid.Buttons.Edit;
		}

		#endregion

		#region DataSource

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (InnerGrid != null && InnerGrid.ListManager != null)
			{
				InnerGrid.ListManager.ListChanged -= InnerGridListManager_ListChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (InnerGrid != null && InnerGrid.ListManager != null)
			{
				RefreshNewSalesActivityButtonDropdownItems();
				InnerGrid.ListManager.ListChanged += InnerGridListManager_ListChanged;
			}
		}

		void InnerGridListManager_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshNewSalesActivityButtonDropdownItems();
		}

		IRelatableActivity ParentActivity
		{
			get
			{
				if (InnerGrid != null && InnerGrid.ListManager != null)
				{
					var linkCollection = InnerGrid.ListManager.List as RelatedActivityLinkCollection;
					if (linkCollection != null)
					{
						return linkCollection.FromActivity;
					}
				}

				return null;
			}
		}

		public IEnumerable<IRelatableActivity> CurrentlySelectedGridActivitiesWithFallback
		{
			get
			{
				var selectedElements = InnerGrid.SelectedElements.OfType<RelatedActivityLink>().Select(link => link.ToActivityForBinding).Where(activity => activity != null);
				if (selectedElements.Any())
				{
					return selectedElements;
				}

				if (InnerGrid.ListManager.Count > 0)
				{
					var gridCurrent = InnerGrid.ListManager.GetCurrent() as RelatedActivityLink;
					if (gridCurrent != null)
					{
						var gridCurrentActivity = gridCurrent.ToActivityForBinding;
						if (gridCurrentActivity != null)
						{
							return new[] { gridCurrentActivity };
						}
					}
				}

				var parentActivity = ParentActivity;
				if (parentActivity != null)
				{
					return new[] { parentActivity };
				}

				return Enumerable.Empty<IRelatableActivity>();
			}
		}

		#endregion

		#region New Sales Activity

		ZToolStripDropDownButton NewSalesActivityButton
		{
			get { return toolStrip.Items.Find(Buttons.NewSalesActivity, true).FirstOrDefault() as ZToolStripDropDownButton; }
		}

		void AddNewSalesActivityButton()
		{
			var newActivityButton = new ZToolStripDropDownButton();
			newActivityButton.Name = Buttons.NewSalesActivity;
			newActivityButton.Alignment = ToolStripItemAlignment.Right;
			newActivityButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			newActivityButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			newActivityButton.Click += newSalesActivityButton_Click;

			toolStrip.Items.Add(newActivityButton);
		}

		void RefreshNewSalesActivityButtonDropdownItems()
		{
			var newSalesActivityButton = NewSalesActivityButton;
			if (newSalesActivityButton != null)
			{
				newSalesActivityButton.DropDownItems.Clear();

				var parentRelatableActivity = ParentActivity;
				if (parentRelatableActivity != null)
				{
					if (parentRelatableActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(RelatableActivityTypeList.Codes.InquiryManager).IsValid)
					{
						newSalesActivityButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("91AC2785-A91F-49E9-AA8C-E81C009631FE", "Inquiry"), GetDefaultCreateNewRelatedActivityHandler(ControllerIDs.SalesEnquiry)));
					}

					if (parentRelatableActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(RelatableActivityTypeList.Codes.OpportunityManager).IsValid)
					{
						newSalesActivityButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("84504ae7-85da-498f-85d6-72ba47020eb2", "Opportunity"), GetDefaultCreateNewRelatedActivityHandler(ControllerIDs.Opportunity)));
					}

					if (parentRelatableActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(RelatableActivityTypeList.Codes.Quotations).IsValid)
					{
						newSalesActivityButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("A4F08E6C-972B-426E-86FD-03792F17061B", "Quotation"), GetDefaultCreateNewRelatedActivityHandler(ControllerIDs.Quotations)));
					}

					if (parentRelatableActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(RelatableActivityTypeList.Codes.OneOffQuotes).IsValid)
					{
						newSalesActivityButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("7E505AED-9BBD-41D4-96C3-FEF9F9D5E2B5", "One Off Quote"), CreateNewRelatedSpotQuoteHandler));
					}
					if (parentRelatableActivity.RelatedChildActivityPivotCollection.CheckValidActivityType(RelatableActivityTypeList.Codes.Projects).IsValid)
					{
						newSalesActivityButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("B4A65C48-1739-4259-A449-709D1C4BCC29", "Project"), GetDefaultCreateNewRelatedActivityHandler(ControllerIDs.Project)));
					}
				}
			}
		}

		void newSalesActivityButton_Click(object sender, EventArgs e)
		{
			var newSalesActivityButton = NewSalesActivityButton;
			if (newSalesActivityButton != null && newSalesActivityButton.DropDownItems.Count == 0)
			{
				var parentActivity = ParentActivity;
				var parentName = parentActivity != null ? parentActivity.HumanReadableName : ZString.Empty;

				Globals.Message.ShowInformation(
						ResString.GetMultilingualString("069e7e1f-8f0e-4ab7-ad4b-2844f7026bc0", @"{0} can not have any more children.

This rule is defined in the registry item [{1}]/{2}]",
							parentName,
							OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Category,
							OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.Caption),
						ResString.GetMultilingualString("060f6097-770a-45f8-8087-b43355919fa4", @"New Related Activity"));
			}
		}

		EventHandler GetDefaultCreateNewRelatedActivityHandler(ControllerID controllerId)
		{
			return (object sender, EventArgs e) =>
				{
					var controller = ZControllerFactory.Create(controllerId);
					ShowNewRelatedActivityForm(controller, new[] { ParentActivity });
				};
		}

		void CreateNewRelatedSpotQuoteHandler(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.OneOffQuotes);
			((IQuotedBookingController)controller).SetQuotedBookingState(QuotedBookingState.QuoteOnly);
			ShowNewRelatedActivityForm(controller, new[] { ParentActivity });
		}

		protected void ShowNewRelatedActivityForm(ZController controller, IEnumerable<IRelatableActivity> parentRelatableActivities)
		{
			var newEntity = GetNewBusinessEntity(controller);
			var formShowingArgs = new FormShowingForElementArgs(newEntity);
			OnFormShowingForNewElement(formShowingArgs, parentRelatableActivities);
			if (!formShowingArgs.Cancelled)
			{
				controller.SetFormsModalTo(FindForm());
				controller.ShowFormForNewEntity(newEntity);
			}
			else
			{
				newEntity.Delete();
			}
		}

		void OnFormShowingForNewElement(FormShowingForElementArgs args, IEnumerable<IRelatableActivity> parentRelatableActivities)
		{
			var newElement = args.Element;

			var newRelatableActivity = newElement as IRelatableActivity;
			if (newRelatableActivity != null)
			{
				foreach (var parentRelatableActivity in parentRelatableActivities)
				{
					if (!newRelatableActivity.RelatedParentActivityPivotCollection.HasParent(parentRelatableActivity))
					{
						var addParentResult = newRelatableActivity.RelatedParentActivityPivotCollection.AddActivity(parentRelatableActivity);
						if (!addParentResult.Success)
						{
							args.Cancelled = true;
							Globals.Message.ShowError(ResString.GetMultilingualString("f055f229-b1c1-4602-9625-4ffb8049a465", "Can not create related activity - {0}", addParentResult.Reason), ResString.GetMultilingualString("979af1a0-0936-452d-9e30-4a4049ac2ca2", "Can not create related activity"));
							break;
						}
					}
				}

				if (!args.Cancelled)
				{
					foreach (var parentRelatableActivity in parentRelatableActivities)
					{
						var caption = SalesRelationControl.GetNewChildCaption(newRelatableActivity, parentRelatableActivity);
						var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((ZForm)ParentForm, caption);
						if (!SalesRelationTree.DoImportParentRelatedActivityInfoOnNewActions(parentRelatableActivity, newRelatableActivity, deciderFactory))
						{
							args.Cancelled = true;
							break;
						}
					}
				}
			}

			if (FormShowingForNewElement != null)
			{
				FormShowingForNewElement(this, args);
			}
		}

		public event EventHandler<FormShowingForElementArgs> FormShowingForNewElement;

		#endregion

		#region New Communication

		ZToolStripDropDownButton NewCommunicationButton
		{
			get { return toolStrip.Items.Find(Buttons.NewCommunication, true).FirstOrDefault() as ZToolStripDropDownButton; }
		}

		void AddNewCommunicationButton()
		{
			var newCommunicationButton = new ZToolStripDropDownButton();
			newCommunicationButton.Name = Buttons.NewCommunication;
			newCommunicationButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			newCommunicationButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;

			newCommunicationButton.DropDownItems.Add(new ZToolStripMenuItem(ResString.GetMultilingualString("89c0c6fd-10d9-43c4-817b-6484d4165c58", "New"), CreateNewCommunicationHandler));
			newCommunicationButton.DropDownOpening += newCommunicationButton_Opening;

			toolStrip.Items.Add(newCommunicationButton);
		}

		void CreateNewCommunicationHandler(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Communication);
			ShowNewRelatedActivityForm(controller, CurrentlySelectedGridActivitiesWithFallback);
		}

		void newCommunicationButton_Opening(object sender, EventArgs e)
		{
			AddUniversalCopyButton();
		}

		#region UniversalCopyButton

		protected void AddUniversalCopyButton()
		{
			var newCommunicationButton = NewCommunicationButton;
			if (newCommunicationButton != null)
			{
				var form = FindForm() as ZForm;
				if (form != null)
				{
					universalCopyManager = ObjectFactory.New<IFormUniversalCopyManager>(form);
					if (universalCopyManager.AllowsUniversalCopy)
					{
						var universalCopyMenuItem = new ZMenuItem(ResString.GetMultilingualString("d00a3cb0-a03f-430c-8af6-3b1480d948d9", "Universal Copy"));
						universalCopyManager.AddMenuItems(universalCopyMenuItem, true, false, false);
						universalCopyManager.FormShowingForNewElement += universalCopyManager_FormShowingForNewElement;
						newCommunicationButton.DropDownItems.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(universalCopyMenuItem, null, false));
					}
					else
					{
						universalCopyManager.Dispose();
						universalCopyManager = null;
					}

					newCommunicationButton.DropDownOpening -= newCommunicationButton_Opening;
				}
			}
		}

		void universalCopyManager_FormShowingForNewElement(object sender, FormShowingForElementArgs e)
		{
			OnFormShowingForNewElement(e, CurrentlySelectedGridActivitiesWithFallback);
		}

		IFormUniversalCopyManager universalCopyManager;

		#endregion

		#endregion

		#region Edit

		void SetupEditButton()
		{
			var editButton = toolStrip.Items.Find(Buttons.Edit, true).FirstOrDefault() as ZToolStripButton;
			editButton.Alignment = ToolStripItemAlignment.Right;
		}

		protected override void Edit(BusinessObject selected, object sender)
		{
			var pivot = (RelatedActivityLink)selected;
			if (pivot.ToActivityForBindingControllerId == null || pivot.ToActivityForBinding == null)
			{
				Globals.Message.ShowError(
					Res.GetString("B0A78432-A239-471C-8C3F-4437430D4FF4", "Related Activity could not be opened because it is invalid."),
					Res.GetString("5DE61FA6-1FCF-47FF-86D8-189D4E080C94", "Invalid Related Activity"));
			}
			else
			{
				base.Edit(selected, sender);
			}
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			var pivot = selected as RelatedActivityLink;
			if (pivot != null)
			{
				return ZControllerFactory.Create(pivot.ToActivityForBindingControllerId);
			}
			return null;
		}

		protected override BusinessObject ConvertSelectedObjectToEditableObjectForModule(BusinessObject originalBizO)
		{
			var pivot = (RelatedActivityLink)originalBizO;
			return pivot.ToActivityForBinding as BusinessObject;
		}

		#endregion

		#region Detach

		protected override void InnerGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			if (e.Objects.OfType<RelatedActivityLink>().Any(x => x.Pivot.IsInDatabaseIncludingChildren))
			{
				var result = Globals.Message.Show(
					Res.GetString("b0d2599a-d26e-44c9-94d7-2cac7fb0f391", "Detach the selected related activities?"),
					Res.GetString("8a784190-df33-4f99-893a-122dfd90d6f3", "Detach Related Activity?"),
					MessageBoxButtons.OKCancel,
					MessageBoxIcon.Question);

				if (result != DialogResult.OK)
				{
					e.Cancel = true;
				}
			}

			if (!e.Cancel)
			{
				base.InnerGrid_RowDeleting(sender, e);
			}
		}

		#endregion

		#region Implementation

		protected override bool NeedsSaveToShowEditForm(BusinessObject selected)
		{
			return false;
		}

		protected override bool ElementsBelongToDifferentModules
		{
			get { return true; }
		}

		protected override bool AllowOpenInEditFormEvenIfListIsReadOnly
		{
			get { return true; }
		}

		protected override ZGridWithoutColumnStylesSerialisation CreateNewGrid()
		{
			return new CommunicationRelatedActivityGrid();
		}

		class CommunicationRelatedActivityGrid : ZGridWithoutColumnStylesSerialisation
		{
			protected override bool CanDeleteCurrentRow(int currentRowIndex)
			{
				var currentBusinessObject = List[currentRowIndex] as RelatedActivityLink;
				if (currentBusinessObject != null && !currentBusinessObject.CanDelete)
				{
					return false;
				}
				return base.CanDeleteCurrentRow(currentRowIndex);
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (universalCopyManager != null)
				{
					universalCopyManager.Dispose();
					universalCopyManager = null;
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
