using System;
using System.Threading;
using System.Windows.Forms;
using Enterprise.Rating.Integration;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Warehouse.Invoicing.GUI
{
	public class PeriodicInvoicingMultiClientForm : ZChildForm
	{
		public PeriodicInvoicingMultiClientForm(PeriodicInvoicingMultiClientInvoice invoiceMultiClients)
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
				ProgForm.Status = Res.GetString("ce637bc0-df61-4197-9415-b8f447aa3c6d", "Creating Periodic Invoices...");
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
		void InvoiceMultiClients_InvoiceCreated(object sender, PeriodicInvoicingMultiClientInvoice.InvoiceCreatedEventArgs e)
		{
			ProgForm.Status = Res.GetString("042eede6-dd7e-4e21-86b3-110942479f57", "Creating Invoice {0} of {1}", e.CountCompleted, e.TotalCount);
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

		PeriodicInvoicingMultiClientInvoice MultiClientInvoiceDetails => (PeriodicInvoicingMultiClientInvoice)BusinessEntity;

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
			this.BindingSource.DataSourceType = typeof(PeriodicInvoicingMultiClientInvoice);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.zGrid1, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).IncludeInInvoicing);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_OH_Client);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).ClientName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_WW);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).WarehouseName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_StorageFromDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicing)(((System.Collections.IList)(((PeriodicInvoicingMultiClientInvoice)(null)).Invoices)).SyncRoot)).ET_StorageToDate);
			this.zGrid1.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|38c827e2-0277-4b1e-9fdf-af18291039c3", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInInvoicing";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ET_OH_Client";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|41cc8a43-028a-41ee-8c3d-89e1734b10a3", "Client Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ET_WW";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|05c841a9-9464-4bfe-90eb-9a9c1c2aedc3", "Warehouse Name");
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
			this.zGrid1.GridId = "7e06f905-480f-4fa0-8e6d-329797888706";
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
			autoRateAndPostInvoicesButton.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|b7f8b170-f4d3-4802-8aa2-a8722ead6868", "Autorate and Post Invoices");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoicingMultiClientInvoice)(null)).InvoiceDate);
			this.zDateEdit2.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|eb21f5d1-6417-4f2b-a0c2-e878f1c4f527", "Date", "Invoice Date", "");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(747, 12, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|fbe9815d-cac5-49e0-904e-a7415435c3f5", "", "The following client / warehouse pairs will be auto-rated and invoiced when you click \'Finalize Invoices\'.\r\nIf no rates exist or no charges apply for this client / warehouse combination, no invoice will be created.");
			this.zLabel3.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 29, true);
			this.zLabel3.TabIndex = 0;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelButtonX.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|8113ceb9-d11d-4fa9-986b-3cbb31274321", "Cancel");
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
			autoRateInvoicesButton.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|a3277bb5-ae21-490d-9172-7a81365e6969", "Autorate Invoices");
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
			this.CaptionResourceString = Res.GetData("PeriodicInvoicingMultiClientForm|22e8763d-5174-4379-bda8-0770604e98c5", "Periodic Invoice for Multiple Clients");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 578, true);
			this.Controls.Add(autoRateInvoicesButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zDateEdit2);
			this.Controls.Add(autoRateAndPostInvoicesButton);
			this.Controls.Add(this.zGrid1);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Invoicing.Business";
			this.DataSourceType = typeof(PeriodicInvoicingMultiClientInvoice);
			this.DataSourceTypeName = "Enterprise.Warehouse.Shared.PeriodicInvoicing.PeriodicInvoicingMultiClientInvoice";
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
