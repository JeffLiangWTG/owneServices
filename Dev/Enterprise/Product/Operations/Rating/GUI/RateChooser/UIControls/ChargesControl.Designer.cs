namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class ChargesControl
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
            this.pnlMain = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblGroupName = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotalPriceCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblTotalPriceString = new Enterprise.ZArchitecture.ZLabel();
            this.pbErrorIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.cbIsAnyOptionalActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSize = true;
            this.pnlMain.Controls.Add(this.pnlItemsContainer);
            this.pnlMain.Controls.Add(this.pnlTop);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            this.pnlMain.TabIndex = 0;
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.AutoSize = true;
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
            this.pnlItemsContainer.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.pnlItemsContainer.TabIndex = 1;
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.DarkGray;
            this.pnlTop.Controls.Add(this.lblGroupName);
            this.pnlTop.Controls.Add(this.lblTotalPriceCurrency);
            this.pnlTop.Controls.Add(this.lblTotalPriceString);
            this.pnlTop.Controls.Add(this.pbErrorIcon);
            this.pnlTop.Controls.Add(this.cbIsAnyOptionalActive);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            this.pnlTop.TabIndex = 0;
            // 
            // lblGroupName
            // 
            this.BindingSource.SetBindingMember(this.lblGroupName, "GroupName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).GroupName)));
            this.lblGroupName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGroupName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblGroupName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 0, true);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
            this.lblGroupName.TabIndex = 15;
            this.lblGroupName.Text = "GroupName";
            // 
            // lblTotalPriceCurrency
            // 
            this.BindingSource.SetBindingMember(this.lblTotalPriceCurrency, "TotalPriceCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).TotalPriceCurrency)));
            this.lblTotalPriceCurrency.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalPriceCurrency.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblTotalPriceCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 0, true);
            this.lblTotalPriceCurrency.Name = "lblTotalPriceCurrency";
            this.lblTotalPriceCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.lblTotalPriceCurrency.TabIndex = 14;
            this.lblTotalPriceCurrency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTotalPriceString
            // 
            this.BindingSource.SetBindingMember(this.lblTotalPriceString, "TotalPriceString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).TotalPriceString)));
            this.lblTotalPriceString.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTotalPriceString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblTotalPriceString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 0, true);
            this.lblTotalPriceString.Name = "lblTotalPriceString";
            this.lblTotalPriceString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
            this.lblTotalPriceString.TabIndex = 13;
            this.lblTotalPriceString.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbErrorIcon
            // 
            this.pbErrorIcon.Dock = System.Windows.Forms.DockStyle.Right;
            this.pbErrorIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 0, true);
            this.pbErrorIcon.Name = "pbErrorIcon";
            this.pbErrorIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.pbErrorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbErrorIcon.TabIndex = 10;
            this.pbErrorIcon.TabStop = false;
            this.pbErrorIcon.MouseHover += new System.EventHandler(this.pbErrorIcon_MouseHover);
            // 
            // cbIsAnyOptionalActive
            // 
            this.BindingSource.SetBindingMember(this.cbIsAnyOptionalActive, "IsAnyOptionalActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargesViewModel)(null)).IsAnyOptionalActive)));
            this.cbIsAnyOptionalActive.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbIsAnyOptionalActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.cbIsAnyOptionalActive.Name = "cbIsAnyOptionalActive";
            this.cbIsAnyOptionalActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 20, true);
            this.cbIsAnyOptionalActive.TabIndex = 0;
            this.cbIsAnyOptionalActive.UseVisualStyleBackColor = true;
            // 
            // ChargesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlMain);
            this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 0, true);
            this.Name = "ChargesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlMain;
		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.GUI.ZCheckBox cbIsAnyOptionalActive;
		private ZArchitecture.GUI.ZPictureBox pbErrorIcon;
		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
		private ZArchitecture.ZLabel lblTotalPriceCurrency;
		private ZArchitecture.ZLabel lblTotalPriceString;
		private ZArchitecture.ZLabel lblGroupName;
	}
}
