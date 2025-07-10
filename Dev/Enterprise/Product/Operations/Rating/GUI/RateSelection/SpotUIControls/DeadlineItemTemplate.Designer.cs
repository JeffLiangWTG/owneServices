using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class DeadlineItemTemplate
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
            this.pnlColumnTitles = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblName = new Enterprise.ZArchitecture.ZLabel();
            this.lblType = new Enterprise.ZArchitecture.ZLabel();
            this.lblDate = new Enterprise.ZArchitecture.ZLabel();
            this.lblCode = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlColumnTitles.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.TransportLegDateInfoViewModel);
            // 
            // pnlColumnTitles
            // 
            this.pnlColumnTitles.Controls.Add(this.lblName);
            this.pnlColumnTitles.Controls.Add(this.lblType);
            this.pnlColumnTitles.Controls.Add(this.lblDate);
            this.pnlColumnTitles.Controls.Add(this.lblCode);
            this.pnlColumnTitles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlColumnTitles.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlColumnTitles.Name = "pnlColumnTitles";
            this.pnlColumnTitles.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 19, true);
            this.pnlColumnTitles.TabIndex = 3;
            // 
            // lblName
            // 
            this.BindingSource.SetBindingMember(this.lblName, "Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegDateInfoViewModel)(null)).Name)));
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 0, true);
            this.lblName.Name = "lblName";
            this.lblName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 19, true);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "Name";
            // 
            // lblType
            // 
            this.BindingSource.SetBindingMember(this.lblType, "Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegDateInfoViewModel)(null)).Type)));
            this.lblType.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 0, true);
            this.lblType.Name = "lblType";
            this.lblType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 19, true);
            this.lblType.TabIndex = 7;
            this.lblType.Text = "Type";
            // 
            // lblDate
            // 
            this.lblDate.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 0, true);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 19, true);
            this.lblDate.TabIndex = 6;
            this.lblDate.Text = "Date";
            // 
            // lblCode
            // 
            this.BindingSource.SetBindingMember(this.lblCode, "Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.TransportLegDateInfoViewModel)(null)).Code)));
            this.lblCode.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 19, true);
            this.lblCode.TabIndex = 1;
            this.lblCode.Text = "Code";
            // 
            // DeadlineItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlColumnTitles);
            this.Name = "DeadlineItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlColumnTitles.ResumeLayout(false);
            this.pnlColumnTitles.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlColumnTitles;
		private ZArchitecture.ZLabel lblName;
		private ZArchitecture.ZLabel lblType;
		private ZArchitecture.ZLabel lblDate;
		private ZArchitecture.ZLabel lblCode;
	}
}
