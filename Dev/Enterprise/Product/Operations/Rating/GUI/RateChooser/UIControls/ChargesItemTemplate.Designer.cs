using Enterprise.Rating.GUI.RateChooser.ViewModel;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	partial class ChargesItemTemplate
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
            this.lblCalculatedFormula = new Enterprise.ZArchitecture.ZLabel();
            this.lblHandlingOffice = new Enterprise.ZArchitecture.ZLabel();
            this.pnlError = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.pbErrorIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.lblCalculatedAmountString = new Enterprise.ZArchitecture.ZLabel();
            this.lblCode = new Enterprise.ZArchitecture.ZLabel();
            this.cbIsActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlError.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lblCalculatedFormula);
            this.pnlMain.Controls.Add(this.lblHandlingOffice);
            this.pnlMain.Controls.Add(this.pnlError);
            this.pnlMain.Controls.Add(this.lblCalculatedAmountString);
            this.pnlMain.Controls.Add(this.lblCode);
            this.pnlMain.Controls.Add(this.cbIsActive);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 19, true);
            this.pnlMain.TabIndex = 0;
            // 
            // lblCalculatedFormula
            // 
            this.BindingSource.SetBindingMember(this.lblCalculatedFormula, "CalculatedFormula");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel)(null)).CalculatedFormula)));
            this.lblCalculatedFormula.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCalculatedFormula.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCalculatedFormula.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 0, true);
            this.lblCalculatedFormula.Name = "lblCalculatedFormula";
            this.lblCalculatedFormula.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 19, true);
            this.lblCalculatedFormula.TabIndex = 17;
            this.lblCalculatedFormula.Text = "Calc";
            this.lblCalculatedFormula.MouseHover += new System.EventHandler(this.lblCalculatedFormula_MouseHover);
            // 
            // lblHandlingOffice
            // 
            this.BindingSource.SetBindingMember(this.lblHandlingOffice, "HandlingOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel)(null)).HandlingOffice)));
            this.lblHandlingOffice.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblHandlingOffice.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblHandlingOffice.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 0, true);
            this.lblHandlingOffice.Name = "lblHandlingOffice";
            this.lblHandlingOffice.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 19, true);
            this.lblHandlingOffice.TabIndex = 16;
            this.lblHandlingOffice.Text = "Office";
            this.lblHandlingOffice.MouseHover += new System.EventHandler(this.lblHandlingOffice_MouseHover);
            // 
            // pnlError
            // 
            this.pnlError.Controls.Add(this.pbErrorIcon);
            this.pnlError.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlError.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 0, true);
            this.pnlError.Name = "pnlError";
            this.pnlError.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 19, true);
            this.pnlError.TabIndex = 15;
            // 
            // pbErrorIcon
            // 
            this.pbErrorIcon.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbErrorIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
            this.pbErrorIcon.Name = "pbErrorIcon";
            this.pbErrorIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 11, true);
            this.pbErrorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbErrorIcon.TabIndex = 15;
            this.pbErrorIcon.TabStop = false;
            this.pbErrorIcon.MouseHover += new System.EventHandler(this.pbErrorIcon_MouseHover);
            // 
            // lblCalculatedAmountString
            // 
            this.BindingSource.SetBindingMember(this.lblCalculatedAmountString, "CalculatedAmountString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel)(null)).CalculatedAmountString)));
            this.lblCalculatedAmountString.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblCalculatedAmountString.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCalculatedAmountString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 0, true);
            this.lblCalculatedAmountString.Name = "lblCalculatedAmountString";
            this.lblCalculatedAmountString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 19, true);
            this.lblCalculatedAmountString.TabIndex = 13;
            this.lblCalculatedAmountString.Text = "Amount";
            this.lblCalculatedAmountString.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCode
            // 
            this.BindingSource.SetBindingMember(this.lblCode, "Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel)(null)).Code)));
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 0, true);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 19, true);
            this.lblCode.TabIndex = 2;
            this.lblCode.Text = "Code";
            this.lblCode.MouseHover += new System.EventHandler(this.lblCode_MouseHover);
            // 
            // cbIsActive
            // 
            this.BindingSource.SetBindingMember(this.cbIsActive, "IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Rating.GUI.RateChooser.ViewModel.ChargeViewModel)(null)).IsActive)));
            this.cbIsActive.Dock = System.Windows.Forms.DockStyle.Left;
            this.cbIsActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.cbIsActive.Name = "cbIsActive";
            this.cbIsActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 19, true);
            this.cbIsActive.TabIndex = 0;
            this.cbIsActive.UseVisualStyleBackColor = true;
            // 
            // ChargesItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.pnlMain);
            this.Name = "ChargesItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlError.ResumeLayout(false);
            this.pnlError.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbErrorIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlMain;
		private ZArchitecture.GUI.ZCheckBox cbIsActive;
		private ZArchitecture.ZLabel lblCode;
		private ZArchitecture.ZLabel lblCalculatedAmountString;
		private ZArchitecture.ZLabel lblCalculatedFormula;
		private ZArchitecture.ZLabel lblHandlingOffice;
		private ZArchitecture.GUI.ZPanel pnlError;
		private ZArchitecture.GUI.ZPictureBox pbErrorIcon;
	}
}
