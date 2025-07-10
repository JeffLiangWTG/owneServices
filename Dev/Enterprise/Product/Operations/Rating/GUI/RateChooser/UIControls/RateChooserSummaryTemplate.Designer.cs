namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class RateChooserSummaryTemplate
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.pnlSelection = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.cbSelected = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblCommodityCode = new Enterprise.ZArchitecture.ZLabel();
            this.lblCommodityLabel = new Enterprise.ZArchitecture.ZLabel();
            this.lblContainerType = new Enterprise.ZArchitecture.ZLabel();
            this.lblContainerTypeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlEmptyRow = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblNoRateSelectedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pnlCardInfoContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ucCardInfo = new Enterprise.Rating.GUI.RateChooser.UIControls.RateChooserCard();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlSelection.SuspendLayout();
            this.pnlTop.SuspendLayout();
            this.pnlEmptyRow.SuspendLayout();
            this.pnlCardInfoContainer.SuspendLayout();
            this.ucCardInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.ChooserRateRow);
            // 
            // pnlSelection
            // 
            this.pnlSelection.AutoSize = true;
            this.pnlSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSelection.Controls.Add(this.cbSelected);
            this.pnlSelection.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSelection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1731, 0, true);
            this.pnlSelection.Name = "pnlSelection";
            this.pnlSelection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 436, true);
            this.pnlSelection.TabIndex = 0;
            // 
            // cbSelected
            // 
            this.cbSelected.AutoSize = true;
            this.cbSelected.Checked = true;
            this.cbSelected.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSelected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 10, true);
            this.cbSelected.Name = "cbSelected";
            this.cbSelected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.cbSelected.TabIndex = 0;
            this.cbSelected.UseVisualStyleBackColor = true;
            this.cbSelected.CheckedChanged += new System.EventHandler(this.cbSelected_CheckedChanged);
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblCommodityCode);
            this.pnlTop.Controls.Add(this.lblCommodityLabel);
            this.pnlTop.Controls.Add(this.lblContainerType);
            this.pnlTop.Controls.Add(this.lblContainerTypeLabel);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1731, 37, true);
            this.pnlTop.TabIndex = 1;
            // 
            // lblCommodityCode
            // 
            this.lblCommodityCode.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblCommodityCode, "CommodityCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityCode)));
            this.lblCommodityCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblCommodityCode.IsFontBold = true;
            this.lblCommodityCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 18, true);
            this.lblCommodityCode.Name = "lblCommodityCode";
            this.lblCommodityCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
            this.lblCommodityCode.TabIndex = 3;
            this.lblCommodityCode.Text = "CommodityCode";
            this.lblCommodityCode.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblCommodityLabel
            // 
            this.lblCommodityLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblCommodityLabel, "CommodityLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).CommodityLabel)));
            this.lblCommodityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCommodityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 18, true);
            this.lblCommodityLabel.Name = "lblCommodityLabel";
            this.lblCommodityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
            this.lblCommodityLabel.TabIndex = 2;
            this.lblCommodityLabel.Text = "CommodityLabel";
            this.lblCommodityLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblContainerType
            // 
            this.lblContainerType.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblContainerType, "ContainerType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ContainerType)));
            this.lblContainerType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblContainerType.IsFontBold = true;
            this.lblContainerType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 18, true);
            this.lblContainerType.Name = "lblContainerType";
            this.lblContainerType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
            this.lblContainerType.TabIndex = 1;
            this.lblContainerType.Text = "ContainerType";
            this.lblContainerType.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // lblContainerTypeLabel
            // 
            this.lblContainerTypeLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblContainerTypeLabel, "ContainerTypeLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).ContainerTypeLabel)));
            this.lblContainerTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblContainerTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
            this.lblContainerTypeLabel.Name = "lblContainerTypeLabel";
            this.lblContainerTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 13, true);
            this.lblContainerTypeLabel.TabIndex = 0;
            this.lblContainerTypeLabel.Text = "ContainerTypeLabel";
            this.lblContainerTypeLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlEmptyRow
            // 
            this.pnlEmptyRow.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlEmptyRow.Controls.Add(this.lblNoRateSelectedLabel);
            this.pnlEmptyRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEmptyRow.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
            this.pnlEmptyRow.Name = "pnlEmptyRow";
            this.pnlEmptyRow.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1731, 75, true);
            this.pnlEmptyRow.TabIndex = 3;
            this.pnlEmptyRow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlEmptyRow_MouseUp);
            // 
            // lblNoRateSelectedLabel
            // 
            this.lblNoRateSelectedLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblNoRateSelectedLabel, "NoRateSelectedLabel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).NoRateSelectedLabel)));
            this.lblNoRateSelectedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblNoRateSelectedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 31, true);
            this.lblNoRateSelectedLabel.Name = "lblNoRateSelectedLabel";
            this.lblNoRateSelectedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 13, true);
            this.lblNoRateSelectedLabel.TabIndex = 1;
            this.lblNoRateSelectedLabel.Text = "NoRateSelectedLabel";
            this.lblNoRateSelectedLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // pnlCardInfoContainer
            // 
            this.pnlCardInfoContainer.BackColor = System.Drawing.SystemColors.Control;
            this.pnlCardInfoContainer.Controls.Add(this.ucCardInfo);
            this.pnlCardInfoContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardInfoContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
            this.pnlCardInfoContainer.Name = "pnlCardInfoContainer";
            this.pnlCardInfoContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1731, 323, true);
            this.pnlCardInfoContainer.TabIndex = 4;
            // 
            // ucCardInfo
            // 
            this.ucCardInfo.AllowDrop = true;
            this.ucCardInfo.AutoSize = true;
            this.ucCardInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.ucCardInfo, ".");
            this.ucCardInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.ucCardInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ucCardInfo.Name = "ucCardInfo";
            this.ucCardInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1731, 131, true);
            this.ucCardInfo.TabIndex = 3;
            // 
            // RateChooserSummaryTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlCardInfoContainer);
            this.Controls.Add(this.pnlEmptyRow);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlSelection);
            this.Name = "RateChooserSummaryTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1754, 436, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlSelection.ResumeLayout(false);
            this.pnlSelection.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlEmptyRow.ResumeLayout(false);
            this.pnlEmptyRow.PerformLayout();
            this.pnlCardInfoContainer.ResumeLayout(false);
            this.pnlCardInfoContainer.PerformLayout();
            this.ucCardInfo.ResumeLayout(true);
            this.ucCardInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlSelection;
		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.ZLabel lblContainerTypeLabel;
		private ZArchitecture.ZLabel lblContainerType;
		private ZArchitecture.ZLabel lblCommodityLabel;
		private ZArchitecture.ZLabel lblCommodityCode;
		private ZArchitecture.GUI.ZCheckBox cbSelected;
		private ZArchitecture.GUI.ZPanel pnlEmptyRow;
		private ZArchitecture.ZLabel lblNoRateSelectedLabel;
		private ZArchitecture.GUI.ZPanel pnlCardInfoContainer;
		private RateChooserCard ucCardInfo;
	}
}
