namespace Enterprise.Customs.SG.V4.GUI
{
    partial class CMDShipmentCusEntryNumberCollectionForm
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
        protected override void InitializeComponent()
        {
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.CusEntryNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CusEntryNumbersGrid)).BeginInit();
            this.CusEntryNumbersGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 22, true);
            this.MainStatusBar.TabIndex = 3;
            // 
            // MessageStatusBarPanel
            // 
            this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(213);
            // 
            // ErrorStatusBarPanel
            // 
            this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(214);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper);
            // 
            // CusEntryNumbersGrid
            // 
            this.CusEntryNumbersGrid.AllowNavigation = false;
            this.CusEntryNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CusEntryNumbersGrid, "CMDDataValuesForBinding");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper)(null)).CMDDataValuesForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDDataValueWrapper)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper)(null)).CMDDataValuesForBinding)).SyncRoot)).PermitOrExemptionType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDDataValueWrapper)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper)(null)).CMDDataValuesForBinding)).SyncRoot)).Lookups.EntryTypeList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDDataValueWrapper)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper)(null)).CMDDataValuesForBinding)).SyncRoot)).PermitNumberOrExemptionRemarks)));
            this.CusEntryNumbersGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.BindToList = "Lookups+EntryTypeList";
            zDropEditColumnStyleInfo1.ColumnName = "PermitOrExemptionType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo1.ColumnName = "PermitNumberOrExemptionRemarks";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
            this.CusEntryNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.CusEntryNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CusEntryNumbersGrid.GridId = "a394a13f-2089-4f8f-a8a5-c0fa22806aaf";
            this.CusEntryNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CusEntryNumbersGrid.LayoutKey = "CusEntryNumbersGrid";
            this.CusEntryNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CusEntryNumbersGrid.Name = "CusEntryNumbersGrid";
            this.CusEntryNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 243, true);
            this.CusEntryNumbersGrid.TabIndex = 0;
			// 
			// OKButton
			//
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("E0F8D56C-B56D-4C82-B489-829C20CDB524", "&OK");
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 250, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
            this.OKButton.TabIndex = 2;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4B9C639F-CF25-4873-8AF4-D7DF82E58E85", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 250, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
            this.CloseButton.TabIndex = 1;
			this.CloseButton.ToolTipCaption = null;
			// 
			// CMDShipmentCusEntryNumberCollectionForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 307, true);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.CusEntryNumbersGrid);
            this.DataSourceAssemblyName = "Enterprise.Customs.SG.V4.Business";
            this.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper);
            this.DataSourceTypeName = "Enterprise.Customs.SG.V4.Business.CMDMessaging.CMDShipmentWrapper";
            this.MinimizeBox = false;
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 317, true);
            this.Name = "CMDShipmentCusEntryNumberCollectionForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.CusEntryNumbersGrid, 0);
            this.Controls.SetChildIndex(this.CloseButton, 0);
            this.Controls.SetChildIndex(this.OKButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CusEntryNumbersGrid)).EndInit();
            this.CusEntryNumbersGrid.ResumeLayout(false);
            this.CusEntryNumbersGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

		private Enterprise.ZArchitecture.ZGrid CusEntryNumbersGrid;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton CloseButton;

	}
}
