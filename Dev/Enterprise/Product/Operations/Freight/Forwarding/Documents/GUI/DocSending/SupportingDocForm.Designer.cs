namespace Enterprise.Freight.Forwarding.Documents.GUI.DocSending
{
	partial class SupportingDocForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.OkBtn = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
            this.gridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.supportingDocumentsLabel = new Enterprise.ZArchitecture.ZLabel();
            this.supportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.panelOkCancelButtons.SuspendLayout();
            this.gridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.supportingDocumentsGrid)).BeginInit();
            this.supportingDocumentsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 289, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObjectCollection);
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.panelOkCancelButtons);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 31, true);
            this.buttonPanel.TabIndex = 8;
            // 
            // panelOkCancelButtons
            // 
            this.panelOkCancelButtons.Controls.Add(this.OkBtn);
            this.panelOkCancelButtons.Controls.Add(this.CancelBtn);
            this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 0, true);
            this.panelOkCancelButtons.Name = "panelOkCancelButtons";
            this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 31, true);
            this.panelOkCancelButtons.TabIndex = 1;
            // 
            // OkBtn
            // 
            this.OkBtn.IsCaptionOverridden = true;
            this.OkBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 22, true);
            this.OkBtn.TabIndex = 0;
            this.OkBtn.Text = "Send for Certification";
            this.OkBtn.ToolTipCaption = null;
            this.OkBtn.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.IsCaptionOverridden = true;
            this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 7, true);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
            this.CancelBtn.TabIndex = 1;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.ToolTipCaption = null;
            this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // gridPanel
            // 
            this.gridPanel.AutoSize = true;
            this.gridPanel.Controls.Add(this.supportingDocumentsLabel);
            this.gridPanel.Controls.Add(this.supportingDocumentsGrid);
            this.gridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.gridPanel.Name = "gridPanel";
            this.gridPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
            this.gridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 258, true);
            this.gridPanel.TabIndex = 9;
            // 
            // supportingDocumentsLabel
            // 
            this.supportingDocumentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.supportingDocumentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 15, true);
            this.supportingDocumentsLabel.Name = "supportingDocumentsLabel";
            this.supportingDocumentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 23, true);
            this.supportingDocumentsLabel.TabIndex = 1;
            this.supportingDocumentsLabel.Text = "Supporting Documents";
            this.supportingDocumentsLabel.UseMnemonic = false;
            // 
            // supportingDocumentsGrid
            // 
            this.supportingDocumentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.supportingDocumentsGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)).Include)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)).Name)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)).Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)).DocumentType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObject)(null)).Certify)));
            this.supportingDocumentsGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo1.Caption = "";
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("SupportingDocForm|35717c2a-5926-4065-8337-107cef124196", "Include");
            zCheckBoxColumnStyleInfo1.ColumnName = "Include";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zTextBoxColumnStyleInfo1.Caption = "";
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("SupportingDocForm|76ae7706-3695-4e74-855d-fc16a0a0bad4", "Description");
            zTextBoxColumnStyleInfo1.ColumnName = "Name";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
            zTextBoxColumnStyleInfo2.ColumnName = "Description";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
            zTextBoxColumnStyleInfo3.Caption = "";
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("SupportingDocForm|40f80222-f5bb-4cab-b937-53a837bdf193", "Document type");
            zTextBoxColumnStyleInfo3.ColumnName = "DocumentType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo2.Caption = "";
            zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("SupportingDocForm|30081bbf-d5cf-41e5-b727-2b5f0d05db42", "Certify");
            zCheckBoxColumnStyleInfo2.ColumnName = "Certify";
            zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            this.supportingDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.supportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.supportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.supportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.supportingDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.supportingDocumentsGrid.GridId = "34ca586f-90e0-4b44-8717-1f1cde9d71c9";
            this.supportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.supportingDocumentsGrid.LayoutKey = "supportingDocumentsGrid";
            this.supportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 41, true);
            this.supportingDocumentsGrid.Name = "supportingDocumentsGrid";
            this.supportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 192, true);
            this.supportingDocumentsGrid.TabIndex = 0;
            // 
            // SupportingDocForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 313, true);
            this.Controls.Add(this.gridPanel);
            this.Controls.Add(this.buttonPanel);
            this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending.DocSendingBusinessObjectCollection);
            this.Name = "SupportingDocForm";
            this.Text = "SupportingDocForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.buttonPanel, 0);
            this.Controls.SetChildIndex(this.gridPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.panelOkCancelButtons.ResumeLayout(false);
            this.panelOkCancelButtons.PerformLayout();
            this.gridPanel.ResumeLayout(false);
            this.gridPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.supportingDocumentsGrid)).EndInit();
            this.supportingDocumentsGrid.ResumeLayout(false);
            this.supportingDocumentsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel buttonPanel;
		private ZArchitecture.GUI.ZPanel panelOkCancelButtons;
		protected ZArchitecture.GUI.ZButton OkBtn;
		protected ZArchitecture.GUI.ZButton CancelBtn;
		protected ZArchitecture.ZGrid supportingDocumentsGrid;
		private ZArchitecture.ZLabel supportingDocumentsLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel gridPanel;
	}
}
