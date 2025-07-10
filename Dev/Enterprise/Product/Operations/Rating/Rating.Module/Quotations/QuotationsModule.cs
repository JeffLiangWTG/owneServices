using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Integration.Rating;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class QuotationsModule : ZFilterGridModule, IQuotationsModule
	{
		const bool DoInterfaceConnectorLicenceCheck = false;

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.Quotations;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.QuotationWorkflowDescriptorCode;

		protected virtual ControllerID ControllerID => ControllerIDs.Quotations;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Quotation };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Quotations);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new QuotationsFilterControl(GridCollection, (QuotationsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new QuoteCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new QuotationsFilterBusinessObject();
		}

		#endregion

		#region Action Menu / Toolbar Buttons

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem bulkUpdateMenuItem = new ZMenuItem(BulkUpdateText, new EventHandler(BulkUpdate_Click));
			MenuItem bulkUpdateSubMenu = new ZMenuItem(BulkUpdateSubMenuName, new MenuItem[] { bulkUpdateMenuItem });
			menuItemCollection.Insert(Math.Max(0, menuItemCollection.Count - 1), bulkUpdateSubMenu);
			menuItemCollection.Insert(Math.Max(0, menuItemCollection.Count - 1), new ZMenuItem("-"));

			return menuItemCollection.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewStandardMenuItems());
			var copyNewClientItem = (ZMenuItem)CopyMenuItem.CloneMenu();
			var copySameClientItem = (ZMenuItem)CopyMenuItem.CloneMenu();

			CopyMenuItem.Caption = CopyForNewClientText;
			CopyMenuItem.Click += new EventHandler(CopyItem_Click);

			copyNewClientItem.Caption = CopyForAmendmentText;
			copyNewClientItem.Click += new EventHandler(CopyItem_Click);

			copySameClientItem.Caption = CopyForSameClientText;
			copySameClientItem.Click += new EventHandler(CopyItem_Click);

			MenuItem[] copyMenuSubItems = { CopyMenuItem, copyNewClientItem, copySameClientItem };
			MenuItem copySubMenu = new ZMenuItem(CopyText, copyMenuSubItems);
			result.Add(copySubMenu);
			result.Remove(CopyMenuItem);

			return result.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());

			result.Insert(result.Count - 1, new ZMenuItem("-"));

			MenuItem acceptMenuItem = new ZMenuItem(AcceptText, new EventHandler(ShowAcceptForm));
			result.Insert(result.Count - 1, acceptMenuItem);

			MenuItem cancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("c4bedf8d-2fcb-41cb-9e81-9e915c1070c2", "Cancel"), new EventHandler(ShowCancelForm));
			result.Insert(result.Count - 1, cancelMenuItem);

			result.Insert(result.Count - 1, new ZMenuItem("-"));

			return result.ToArray();
		}

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				var buttons = base.ToolBarButtons;
				foreach (ZToolBarButton button in buttons)
				{
					if (button.Text == AcceptText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Tick);
						button.ToolTipText = Res.GetString("a7f1349c-25d2-4e83-bf48-82e24bfc9053", "Accepts the selected Quotation");
					}
					else if (button.Text == CancelText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Cross);
						button.ToolTipText = Res.GetString("21489325-b47f-43a5-b862-f38cba42b52f", "Cancels the selected Quotation");
					}
					else if (button.Text == BulkUpdateSubMenuName)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Briefcase);
					}
					else if (button.Text == CopyText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.CopyButtonRest);
					}
				}

				return buttons;
			}
		}

		void BulkUpdate_Click(object sender, EventArgs e)
		{
			GetNewBulkRateUpdatesController().ShowNewForm();
		}

		ZController GetNewBulkRateUpdatesController()
		{
			BulkRateUpdatesController result = (BulkRateUpdatesController)ZControllerFactory.Create(ControllerIDs.BulkRateUpdates);
			result.DefaultRateType = RatingConstants.RatingHeaderTypes.Quote;
			return result;
		}

		void CopyItem_Click(object sender, EventArgs e)
		{
			if (SelectedBusinessObject != null)
			{
				ToolStripItem toolStripItem = sender as ToolStripItem;
				MenuItem menuItem = sender as MenuItem;
				string text = "";
				if (toolStripItem != null)
				{
					text = toolStripItem.Text;
				}
				else if (menuItem != null)
				{
					text = menuItem.Text;
				}

				if (text == CopyForNewClientText)
				{
					base.ShowTemplateCopyForm(SelectedBusinessObject);
				}
				else if (text == CopyForSameClientText)
				{
					ShowCopyFormSameClient((Quote)SelectedBusinessObject, false);
				}
				else if (text == CopyForAmendmentText)
				{
					ShowCopyFormSameClient((Quote)SelectedBusinessObject, true);
				}
			}
		}

		static MultilingualString CopyForNewClientText => ResString.GetMultilingualString("5e3e47bc-d8c8-490d-a165-d25cf6cdbc0e", "For New Client");
		static MultilingualString CopyForSameClientText => ResString.GetMultilingualString("bc195032-4194-44fd-8fde-7dd2c3611a78", "For Same Client");
		static MultilingualString CopyForAmendmentText => ResString.GetMultilingualString("df5dccee-2cf4-4975-8e66-d4ec811d3506", "Amendment for Same Client");
		static MultilingualString BulkUpdateSubMenuName => ResString.GetMultilingualString("e0522236-e148-4a5b-b407-c707b8a13780", "Updates");
		static MultilingualString BulkUpdateText => ResString.GetMultilingualString("127719d1-5fab-4a2f-ad0a-d671b3f04bab", "Bulk Rate/Cost Update");
		static MultilingualString AcceptText => ResString.GetMultilingualString("6c71ffdb-1414-46c2-914c-700d20f57ced", "Accept");
		static MultilingualString CopyText => ResString.GetMultilingualString("96458f17-6cce-4d79-931d-68bf4ed04ba0", "Copy");
		static string CancelText => Res.GetString("c4bedf8d-2fcb-41cb-9e81-9e915c1070c2", "Cancel");

		#endregion

		#region Show Form Methods

		#region Delete

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			Quote selectedQuote = (Quote)selectedBusinessObject;
			if (!selectedBusinessObject.CanDelete)
			{
				Globals.Message.ShowWarning(selectedBusinessObject.ReasonForNotAbleToDelete, Res.GetString("bf3304c6-9f13-44a8-8e3e-44538c404110", "Can't Delete"));
				return null;
			}

			return base.ShowDeleteForm(selectedBusinessObject);
		}

		#endregion

		#region Edit

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			var controller = (QuotationsController)GetNewController(selectedBusinessObject);
			return controller.ShowEditForm(selectedBusinessObject);
		}

		void IQuotationsModule.ShowQuoteEditForm(IQuote selectedQuote)
		{
			ShowEditForm((BusinessObject)selectedQuote);
		}

		#endregion

		#region Accept

		void ShowAcceptForm(object sender, EventArgs e)
		{
			if (Grid.ListManager.Position >= 0)
			{
				var selectedQuote = (Quote)Grid.ListManager.GetCurrent();

				if (CanAcceptQuote(selectedQuote.QuoteStatus))
				{
					bool minimumCostMarkupNotMetWarning = selectedQuote.HasMinimumCostMarkupNotMetWarning();
					if (minimumCostMarkupNotMetWarning)
					{
						minimumCostMarkupNotMetWarning = !Enterprise.Rating.GUI.QuotationForm.ShowMinimumCostMarkupNotMetDialog();
					}

					if (!minimumCostMarkupNotMetWarning)
					{
						var controller = (QuotationsController)GetNewController(selectedQuote);
						controller.ShowAcceptForm(selectedQuote);

#if DEBUG
						LastUsedControllerForTest = controller;
#endif
					}
				}
				else
				{
					string message = "";
					if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Accepted)
					{
						message = Res.GetString("8506b67e-b88b-4a8b-a8cf-6a651a7098b9", "The selected quote has already been accepted.  Would you like to view it instead?");
					}
					else if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Cancelled)
					{
						message = Res.GetString("75d3679d-9abd-44be-bb69-4637f4801a9a", "The selected quote has already been canceled and cannot be accepted.  Would you like to view it instead?");
					}
					else if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Expired)
					{
						message = Res.GetString("aadefbf4-1f8d-445b-8728-136ad7bc905b", "The selected quote has already expired and cannot be accepted.  Would you like to view it instead?");
					}

					string caption = Res.GetString("674a666f-ad7a-4333-b25b-3e6a8cebf875", "Can't Accept");
					DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						base.ShowViewForm(selectedQuote);
					}
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		bool CanAcceptQuote(ZString quoteStatus)
		{
			return (quoteStatus == Quote.QuoteStatusOptions.Active ||
					quoteStatus == Quote.QuoteStatusOptions.ClientAccepted ||
					quoteStatus == Quote.QuoteStatusOptions.Finalized ||
					quoteStatus == Quote.QuoteStatusOptions.Approved);
		}

		#endregion

		#region Cancel

		bool CanCancelQuote(ZString quoteStatus) => CanAcceptQuote(quoteStatus);

		void ShowCancelForm(object sender, EventArgs e)
		{
			if (Grid.ListManager.Position >= 0)
			{
				var selectedQuote = (Quote)Grid.ListManager.GetCurrent();

				if (CanCancelQuote(selectedQuote.QuoteStatus))
				{
					var controller = (QuotationsController)GetNewController(selectedQuote);
					controller.ShowCancelForm(selectedQuote);

#if DEBUG
					LastUsedControllerForTest = controller;
#endif
				}
				else
				{
					string message = "";
					if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Accepted)
					{
						message = Res.GetString("5f05557a-89e7-45b4-86c5-dcd5db64442f", "The selected quote has already been accepted and cannot be canceled.  Would you like to view it instead?");
					}
					else if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Cancelled)
					{
						message = Res.GetString("1511919b-8fdd-4237-bb4c-022794717a96", "The selected quote has already been canceled.  Would you like to view it instead?");
					}
					else if (selectedQuote.QuoteStatus == Quote.QuoteStatusOptions.Expired)
					{
						message = Res.GetString("656ab1a3-5cac-4722-90b8-dda6e9b142cf", "The selected quote has already expired and cannot be canceled.  Would you like to view it instead?");
					}
					string caption = Res.GetString("84c16422-549a-49eb-9a5e-77daf395fc11", "Can't Cancel");
					DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
					if (result == DialogResult.Yes)
					{
						base.ShowViewForm(selectedQuote);
					}
				}
			}
			else
			{
				ShowNoSelectedMessage();
			}
		}

		#endregion

		#region Copy

		void ShowCopyFormSameClient(Quote selectedQuote, bool isAmendment)
		{
			if (selectedQuote != null)
			{
				selectedQuote.SameClientCopy = true;
				selectedQuote.AmendmentCopy = isAmendment;

				try
				{
					base.ShowTemplateCopyForm(selectedQuote);
				}
				finally
				{
					selectedQuote.AmendmentCopy = false;
					selectedQuote.SameClientCopy = false;
				}
			}
		}

		BusinessObject SelectedBusinessObject;

		protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
		{
			this.SelectedBusinessObject = selectedBusinessObject;
			return null;
		}

		#endregion

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Quotation;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#region XmlImport

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new XmlDataTransferDirector(new QuotationValueObjectDataAdapter(), DoInterfaceConnectorLicenceCheck).PromptUserAndImport(BillingInterfaceName.QuotationsXmlImport); // Interface name for billing purposes
		}

		#endregion

#if DEBUG
		public IZForm ShowEditForm_ForTest(BusinessObject selectedBusinessObject)
			=> ShowEditForm(selectedBusinessObject);

		public IZForm ShowDeleteForm_ForTest(BusinessObject selectedBusinessObject)
			=> ShowDeleteForm(selectedBusinessObject);

		public MenuItem[] ContextMenu_ForTest
			=> ContextMenu;

		public ZDisplayGrid Grid_ForTest
			=> Grid;

		public QuotationsController LastUsedControllerForTest { get; set; }
#endif
	}
}
