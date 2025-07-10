using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	partial class PenaltiesItemTemplate
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
            this.lblCurrency = new Enterprise.ZArchitecture.ZLabel();
            this.lblCostPUnit = new Enterprise.ZArchitecture.ZLabel();
            this.lblUnit = new Enterprise.ZArchitecture.ZLabel();
            this.lblFreeTime = new Enterprise.ZArchitecture.ZLabel();
            this.lblName = new Enterprise.ZArchitecture.ZLabel();
            this.lblType = new Enterprise.ZArchitecture.ZLabel();
            this.lblDirection = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel);
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.lblCurrency);
            this.pnlMain.Controls.Add(this.lblCostPUnit);
            this.pnlMain.Controls.Add(this.lblUnit);
            this.pnlMain.Controls.Add(this.lblFreeTime);
            this.pnlMain.Controls.Add(this.lblName);
            this.pnlMain.Controls.Add(this.lblType);
            this.pnlMain.Controls.Add(this.lblDirection);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 19, true);
            this.pnlMain.TabIndex = 2;
            // 
            // lblCurrency
            // 
            this.BindingSource.SetBindingMember(this.lblCurrency, "Currency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).Currency)));
            this.lblCurrency.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 0, true);
            this.lblCurrency.Name = "lblCurrency";
            this.lblCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 19, true);
            this.lblCurrency.TabIndex = 7;
            this.lblCurrency.Text = "Currency";
            // 
            // lblCostPUnit
            // 
            this.BindingSource.SetBindingMember(this.lblCostPUnit, "PerUnitRate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).PerUnitRate)));
            this.lblCostPUnit.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCostPUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 0, true);
            this.lblCostPUnit.Name = "lblCostPUnit";
            this.lblCostPUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 19, true);
            this.lblCostPUnit.TabIndex = 6;
            this.lblCostPUnit.Text = "Cost P/Unit";
            // 
            // lblUnit
            // 
            this.lblUnit.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 0, true);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 19, true);
            this.lblUnit.TabIndex = 5;
            this.lblUnit.Text = "D";
            // 
            // lblFreeTime
            // 
            this.BindingSource.SetBindingMember(this.lblFreeTime, "FreeTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).FreeTime)));
            this.lblFreeTime.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblFreeTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 0, true);
            this.lblFreeTime.Name = "lblFreeTime";
            this.lblFreeTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 19, true);
            this.lblFreeTime.TabIndex = 4;
            this.lblFreeTime.Text = "Free Time";
            // 
            // lblName
            // 
            this.BindingSource.SetBindingMember(this.lblName, "Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).Name)));
            this.lblName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 0, true);
            this.lblName.Name = "lblName";
            this.lblName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 19, true);
            this.lblName.TabIndex = 3;
            this.lblName.Text = "Name";
            // 
            // lblType
            // 
            this.BindingSource.SetBindingMember(this.lblType, "Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).Type)));
            this.lblType.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 0, true);
            this.lblType.Name = "lblType";
            this.lblType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 19, true);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "Type";
            // 
            // lblDirection
            // 
            this.BindingSource.SetBindingMember(this.lblDirection, "Direction");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.PenaltyViewModel)(null)).Direction)));
            this.lblDirection.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDirection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblDirection.Name = "lblDirection";
            this.lblDirection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 19, true);
            this.lblDirection.TabIndex = 1;
            this.lblDirection.Text = "Direction";
            // 
            // PenaltiesItemTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.pnlMain);
            this.Name = "PenaltiesItemTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlMain;
		private ZArchitecture.ZLabel lblCurrency;
		private ZArchitecture.ZLabel lblCostPUnit;
		private ZArchitecture.ZLabel lblUnit;
		private ZArchitecture.ZLabel lblFreeTime;
		private ZArchitecture.ZLabel lblName;
		private ZArchitecture.ZLabel lblType;
		private ZArchitecture.ZLabel lblDirection;
	}
}
