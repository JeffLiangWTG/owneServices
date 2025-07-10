
namespace Enterprise.Customs.GUI
{
	partial class QuickPackForm
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.QuickPackItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuickPackItemsGrid)).BeginInit();
			this.QuickPackItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 576, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.QuickPack);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CancelButton1);
			this.zPanel1.Controls.Add(this.OKButton);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 539, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 37, true);
			this.zPanel1.TabIndex = 2;
			// 
			// CancelButton1
			// 
			this.CancelButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("78cc34ea-bb33-4ce9-8b18-93804feb9339", "Cancel");
			this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton1.IsCaptionOverridden = false;
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(692, 7, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.CancelButton1.TabIndex = 9;
			this.CancelButton1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton1.ToolTipCaption = null;
			this.CancelButton1.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C6598406-52B8-49C0-998F-5590D346E76E", "Confirm");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 7, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.OKButton.TabIndex = 8;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.QuickPackItemsGrid);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 539, true);
			this.zPanel2.TabIndex = 3;
			// 
			// QuickPackItemsGrid
			// 
			this.QuickPackItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuickPackItemsGrid, "QuickPackItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).PackSeq)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).Pack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).InvoiceLineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).GoodsDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).NotPackedQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).PackedQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.QuickPackItem)(((System.Collections.IList)(((Enterprise.Customs.Business.QuickPack)(null)).QuickPackItems)).SyncRoot)).PackableUQ)));
			this.QuickPackItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo1.ColumnName = "PackSeq";
			zDropEditColumnStyleInfo1.MaxDropDownItems = 1000;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Pack";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "InvoiceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "InvoiceLineNumber";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiLineTextBoxColumnInfo1.ColumnName = "GoodsDesc";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NotPackedQty";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "PackedQty";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "PackableUQ";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.QuickPackItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuickPackItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuickPackItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuickPackItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuickPackItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.QuickPackItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.QuickPackItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.QuickPackItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.QuickPackItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.QuickPackItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuickPackItemsGrid.GridId = "d727026e-fd80-408c-bda6-0ecbaa99bc17";
			this.QuickPackItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuickPackItemsGrid.LayoutKey = "QuickPackGrid";
			this.QuickPackItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuickPackItemsGrid.Name = "QuickPackItemsGrid";
			this.QuickPackItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 539, true);
			this.QuickPackItemsGrid.TabIndex = 0;
			// 
			// QuickPackForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DA26EB71-7AB6-4FEE-8E36-29BD9AEADF76", "Quick Pack Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 600, true);
			this.Controls.Add(this.zPanel2);
			this.Controls.Add(this.zPanel1);
			this.DataSourceType = typeof(Enterprise.Customs.Business.QuickPack);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "QuickPackForm";
			this.Text = "QuickPackForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.zPanel2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuickPackItemsGrid)).EndInit();
			this.QuickPackItemsGrid.ResumeLayout(false);
			this.QuickPackItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private ZArchitecture.ZGrid QuickPackItemsGrid;
		private ZArchitecture.GUI.ZButton CancelButton1;
		private ZArchitecture.GUI.ZButton OKButton;
	}
}
