using System;
using System.Threading;
using System.Windows.Forms;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class InvoicingFormMultiClient : ZChildForm
	{
		public InvoicingFormMultiClient(WhsInvoicePeriodicMultiClientInvoice invoiceMultiClients)
			: base(invoiceMultiClients)
		{
			invoiceMultiClients.InvoiceCreated += InvoiceMultiClients_InvoiceCreated;
			invoiceMultiClients.InvoiceCreationComplete += InvoiceMultiClients_InvoiceCreationComplete;
		}

		#region Invoicing

		void AutoRateAndPostInvoicesButton_Click(object sender, EventArgs e)
		{
			InvoiceButton_ClickBase(AutoRateAndPostInvoices);
		}

		void AutoRateInvoicesButton_Click(object sender, EventArgs e)
		{
			InvoiceButton_ClickBase(AutoRateInvoices);
		}

		void InvoiceButton_ClickBase(Action handleInvoices)
		{
			MultiClientInvoiceDetails.RunPreSaveValidation();
			if (!MultiClientInvoiceDetails.HasErrors)
			{
				ProgForm = new ProgressForm();
				ProgForm.Status = Res.GetString("fe3099ef-859c-47f4-9d73-7f0b15de50cd", "Creating Periodic Invoices...");
				ProgForm.ShowCancelButton = false;
				ProgForm.Show();

				try
				{
					handleInvoices();
					Close();
				}
				catch (AutoRaterException ex)
				{
					Globals.Message.Show(ex.Message);
				}
				finally
				{
					ProgForm.Dispose();
				}
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		void AutoRateAndPostInvoices()
		{
			MultiClientInvoiceDetails.AutoRateAndPostInvoices(CancellationToken.None);
		}

		void AutoRateInvoices()
		{
			MultiClientInvoiceDetails.AutoRateInvoices(CancellationToken.None);
		}

		void InvoiceMultiClients_InvoiceCreationComplete(object sender, TextEventArgs e)
		{
			ProgForm.Close();

			Globals.Message.Show(e.Message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void InvoiceMultiClients_InvoiceCreated(object sender, WhsInvoicePeriodicMultiClientInvoice.InvoiceCreatedEventArgs e)
		{
			ProgForm.Status = Res.GetString("ebbded2d-1288-4f30-8973-d435460258df", "Creating Invoice {0} of {1}", e.CountCompleted, e.TotalCount);
			ProgForm.PercentComplete = (e.CountCompleted / e.TotalCount) * 100;
			Application.DoEvents();
		}

		ProgressForm ProgForm;

		#endregion

		#region Implementation

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			Close();
		}

		WhsInvoicePeriodicMultiClientInvoice MultiClientInvoiceDetails => (WhsInvoicePeriodicMultiClientInvoice)BusinessEntity;

		public override string FormVerb => string.Empty;

		#endregion

		#region Component Designer Generated Code

		ZDateEdit zDateEdit2;
		ZLabel zLabel3;
		ZButton CancelButtonX;
		ZGrid zGrid1;

		protected override void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			this.zGrid1 = new ZGrid();
			ZButton autoRateAndPostInvoicesButton = new ZButton();
			this.zDateEdit2 = new ZDateEdit();
			this.zLabel3 = new ZLabel();
			this.CancelButtonX = new ZButton();
			ZButton autoRateInvoicesButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 554, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 24, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsInvoicePeriodicMultiClientInvoice);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.zGrid1, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).IncludeInInvoicing);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_OH_Client);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).ClientName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_WW);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).WarehouseName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_StorageFromDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoice)(((System.Collections.IList)(((WhsInvoicePeriodicMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_StorageToDate);
			this.zGrid1.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|b89031f3-3264-4dfe-a32e-c367da53c0dd", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInInvoicing";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ET_OH_Client";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|bc95695b-4e55-4897-8354-bd90da8757e8", "Client Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ET_WW";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|b915d3c9-c846-4740-8488-9a1f618f5369", "Warehouse Name");
			zTextBoxColumnStyleInfo2.ColumnName = "WarehouseName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo1.ColumnName = "ET_StorageFromDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "ET_StorageToDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.zGrid1.GridId = "0be8a04e-3b68-4979-81fb-ff6899a26251";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 50, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 469, true);
			this.zGrid1.TabIndex = 3;
			// 
			// InvoiceButton
			// 
			autoRateAndPostInvoicesButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			autoRateAndPostInvoicesButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|177c676c-5f7d-4adf-902a-4e6fa7cbf2e9", "Autorate and Post Invoices");
			autoRateAndPostInvoicesButton.IsCaptionOverridden = false;
			autoRateAndPostInvoicesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 525, true);
			autoRateAndPostInvoicesButton.Name = "AutoRateAndPostInvoicesButton";
			autoRateAndPostInvoicesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			autoRateAndPostInvoicesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 23, true);
			autoRateAndPostInvoicesButton.TabIndex = 5;
			autoRateAndPostInvoicesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			autoRateAndPostInvoicesButton.ToolTipCaption = null;
			autoRateAndPostInvoicesButton.UseVisualStyleBackColor = true;
			autoRateAndPostInvoicesButton.Click += new EventHandler(this.AutoRateAndPostInvoicesButton_Click);
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsInvoicePeriodicMultiClientInvoice)(null)).InvoiceDate);
			this.zDateEdit2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|031faacc-3936-43a7-99a3-4aa16095fbce", "Date", "Invoice Date", "");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(747, 12, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|0319c2b1-49a3-4179-9ced-c90a8d4e6d60", "", "The following client / warehouse pairs will be auto-rated and invoiced when you click \'Finalize Invoices\'.\r\nIf no rates exist or no charges apply for this client / warehouse combination, no invoice will be created.");
			this.zLabel3.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 29, true);
			this.zLabel3.TabIndex = 0;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelButtonX.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|37603a88-4f59-4409-a631-0792221f2ff0", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.IsCaptionOverridden = false;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 525, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.CancelButtonX.TabIndex = 6;
			this.CancelButtonX.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButtonX.ToolTipCaption = null;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new EventHandler(this.CancelButtonX_Click);
			// 
			// InvoiceAutorateNoPrintButton
			// 
			autoRateInvoicesButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			autoRateInvoicesButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoiceAutorateNoPrintButton|44951b14-79c1-4522-a1ca-9e2f091b3e7a", "Autorate Invoices");
			autoRateInvoicesButton.IsCaptionOverridden = false;
			autoRateInvoicesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 525, true);
			autoRateInvoicesButton.Name = "AutoRateInvoicesButton";
			autoRateInvoicesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			autoRateInvoicesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			autoRateInvoicesButton.TabIndex = 4;
			autoRateInvoicesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			autoRateInvoicesButton.ToolTipCaption = null;
			autoRateInvoicesButton.UseVisualStyleBackColor = true;
			autoRateInvoicesButton.Click += new EventHandler(this.AutoRateInvoicesButton_Click);
			// 
			// InvoicingFormMultiClient
			// 
			this.AcceptButton = autoRateAndPostInvoicesButton;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InvoicingFormMultiClient|925f5fa4-1beb-4c81-ab24-136c369c6d62", "Periodic Invoice for Multiple Clients");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 578, true);
			this.Controls.Add(autoRateInvoicesButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zDateEdit2);
			this.Controls.Add(autoRateAndPostInvoicesButton);
			this.Controls.Add(this.zGrid1);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsInvoicePeriodicMultiClientInvoice);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Invoicing.WhsInvoicePeriodicMultiClientInvoice";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 612, true);
			this.Name = "InvoicingFormMultiClient";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(autoRateAndPostInvoicesButton, 0);
			this.Controls.SetChildIndex(this.zDateEdit2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(autoRateInvoicesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
