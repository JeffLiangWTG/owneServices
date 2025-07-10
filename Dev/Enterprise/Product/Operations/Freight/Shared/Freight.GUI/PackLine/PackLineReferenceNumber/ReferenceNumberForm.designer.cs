namespace Enterprise.Freight.GUI
{
	partial class ReferenceNumberForm
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
		protected new void InitializeComponent()
		{
      Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
      this.ReferenceNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ReferenceNumbersGrid)).BeginInit();
      this.ReferenceNumbersGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainStatusBar
      // 
      this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
      this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 24, true);
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.PackLine);
      // 
      // OKButton
      // 
      this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.OKButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("5d5ba21d-b678-4caa-bfa9-1e646fc50953", "OK");
      this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(545, 188, true);
      this.OKButton.Name = "OKButton";
      this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
      this.OKButton.TabIndex = 4;
      this.OKButton.ToolTipCaption = null;
      this.OKButton.UseVisualStyleBackColor = true;
      this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // ReferenceNumbersGrid
      // 
      this.ReferenceNumbersGrid.AllowNavigation = false;
      this.ReferenceNumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.BindingSource.SetBindingMember(this.ReferenceNumbersGrid, "CusEntryNums");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).CE_RN_NKCountryCode)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).CE_EntryType)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).CE_Category)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).CE_EntryNum)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).AdditionalReferenceNumberTypeDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Freight.Business.PackLine)(null)).CusEntryNums)).SyncRoot)).CE_EntryStatus)));
      this.ReferenceNumbersGrid.CaptionVisible = false;
      zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("a9ca8c2e-81c2-486a-90cd-3693386a6618", "Country/Region Of Issue");
      zCodeFindBoxColumnStyleInfo1.ColumnName = "CE_RN_NKCountryCode";
      zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
      zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("4a0121da-e84a-4b82-9d17-25946d3f91b7", "Number Type");
      zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryType";
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("d20e73ac-94b8-43bc-bf70-37dae627e7cd", "Category");
      zTextBoxColumnStyleInfo2.ColumnName = "CE_Category";
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("42d38c8e-1f1f-45c2-abcc-f39fab65b199", "Reference Number");
      zTextBoxColumnStyleInfo3.ColumnName = "CE_EntryNum";
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
      zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("59f103b6-942c-4c6b-b42d-8fd1a662f6b1", "Type Description");
      zTextBoxColumnStyleInfo4.ColumnName = "AdditionalReferenceNumberTypeDescription";
      zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9104e0e4-195b-4bc9-b973-1bd6ba8285ab", "Status");
      zTextBoxColumnStyleInfo5.ColumnName = "CE_EntryStatus";
      zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
      this.ReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
      this.ReferenceNumbersGrid.GridId = "2e39c9e4-460f-479e-bba3-25d9f253e32b";
      this.ReferenceNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.ReferenceNumbersGrid.LayoutKey = "ReferenceNumbersGrid";
      this.ReferenceNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
      this.ReferenceNumbersGrid.Name = "ReferenceNumbersGrid";
      this.ReferenceNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 174, true);
      this.ReferenceNumbersGrid.TabIndex = 2;
      // 
      // ReferenceNumberForm
      // 
      this.AcceptButton = this.OKButton;
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("9cf80921-8d45-431d-8245-f8236c07387b", "Reference Numbers");
      this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 240, true);
      this.Controls.Add(this.ReferenceNumbersGrid);
      this.Controls.Add(this.OKButton);
      this.DataSourceType = typeof(Enterprise.Freight.Business.PackLine);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "ReferenceNumberForm";
      this.Text = "ReferenceNumberForm";
      this.Controls.SetChildIndex(this.MainStatusBar, 0);
      this.Controls.SetChildIndex(this.OKButton, 0);
      this.Controls.SetChildIndex(this.ReferenceNumbersGrid, 0);
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ReferenceNumbersGrid)).EndInit();
      this.ReferenceNumbersGrid.ResumeLayout(false);
      this.ReferenceNumbersGrid.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.ZGrid ReferenceNumbersGrid;
	}
}
