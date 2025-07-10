using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class ContainerPenaltyExclusionPopup
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
			this.mondayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.tuesdayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.wednesdayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.thursdayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.fridayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.saturdayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.sundayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.weekendCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.holidayCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)this.BindingSource).BeginInit();
			//
			// MondayCheckbox
			//
			this.BindingSource.SetBindingMember(this.mondayCheckbox, "CEX_Monday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Monday)));
			this.mondayCheckbox.Name = "MondayCheckbox";
			this.mondayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|17a60fa5-537e-628d-4516-9bd89323640e", "MON - Monday");
			this.mondayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.mondayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.mondayCheckbox.TabIndex = 0;
			//
			// TuesdayCheckbox
			//
			this.BindingSource.SetBindingMember(this.tuesdayCheckbox, "CEX_Tuesday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Tuesday)));
			this.tuesdayCheckbox.Name = "TuesdayCheckbox";
			this.tuesdayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|b5308a26-378e-cd95-4261-775b02255c46", "TUE - Tuesday");
			this.tuesdayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 32, true);
			this.tuesdayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.tuesdayCheckbox.TabIndex = 1;
			//
			// WednesdayCheckbox
			//
			this.BindingSource.SetBindingMember(this.wednesdayCheckbox, "CEX_Wednesday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Wednesday)));
			this.wednesdayCheckbox.Name = "WednesdayCheckbox";
			this.wednesdayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|f3fa72f8-b119-5dae-414e-6504021cefd3", "WED - Wednesday");
			this.wednesdayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 54, true);
			this.wednesdayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.wednesdayCheckbox.TabIndex = 2;
			//
			// ThursdayCheckbox
			//
			this.BindingSource.SetBindingMember(this.thursdayCheckbox, "CEX_Thursday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Thursday)));
			this.thursdayCheckbox.Name = "ThursdayCheckbox";
			this.thursdayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|d3d1e2c8-8fb8-aebb-4247-0bcd24dea5ef", "THU - Thursday");
			this.thursdayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 76, true);
			this.thursdayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.thursdayCheckbox.TabIndex = 3;
			//
			// FridayCheckbox
			//
			this.BindingSource.SetBindingMember(this.fridayCheckbox, "CEX_Friday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Friday)));
			this.fridayCheckbox.Name = "FridayCheckbox";
			this.fridayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|315c5cef-5aa3-06bd-49c2-a6b8901bb114", "FRI - Friday");
			this.fridayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 98, true);
			this.fridayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.fridayCheckbox.TabIndex = 4;
			//
			// SaturdayCheckbox
			//
			this.BindingSource.SetBindingMember(this.saturdayCheckbox, "CEX_Saturday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Saturday)));
			this.saturdayCheckbox.Name = "SaturdayCheckbox";
			this.saturdayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|ec172866-608c-6c95-4828-e53d929e1dce", "SAT - Saturday");
			this.saturdayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 120, true);
			this.saturdayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.saturdayCheckbox.TabIndex = 5;
			//
			// SundayCheckbox
			//
			this.BindingSource.SetBindingMember(this.sundayCheckbox, "CEX_Sunday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Sunday)));
			this.sundayCheckbox.Name = "SundayCheckbox";
			this.sundayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|d9411e89-1a77-7d86-4d2b-95351b032ee6", "SUN - Sunday");
			this.sundayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 142, true);
			this.sundayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.sundayCheckbox.TabIndex = 6;
			//
			// WeekendCheckbox
			//
			this.BindingSource.SetBindingMember(this.weekendCheckbox, "CEX_Weekend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Weekend)));
			this.weekendCheckbox.Name = "WeekendCheckbox";
			this.weekendCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|1dc0d4dc-b643-ae84-48fe-80ec2b795a52", "WKD - Weekend");
			this.weekendCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 164, true);
			this.weekendCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.weekendCheckbox.TabIndex = 7;
			//
			// HolidayCheckbox
			//
			this.BindingSource.SetBindingMember(this.holidayCheckbox, "CEX_Holiday");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion)(null)).CEX_Holiday)));
			this.holidayCheckbox.Name = "HolidayCheckbox";
			this.holidayCheckbox.CaptionResourceString = Res.GetData("ContainerPenaltyExclusionPopup|dab12861-197d-a596-46bf-add42c8618ec", "HOL - Holiday");
			this.holidayCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 186, true);
			this.holidayCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.holidayCheckbox.TabIndex = 8;
			// 
			// ContainerPenaltyExclusionPopup
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mondayCheckbox);
			this.Controls.Add(this.tuesdayCheckbox);
			this.Controls.Add(this.wednesdayCheckbox);
			this.Controls.Add(this.thursdayCheckbox);
			this.Controls.Add(this.fridayCheckbox);
			this.Controls.Add(this.saturdayCheckbox);
			this.Controls.Add(this.sundayCheckbox);
			this.Controls.Add(this.weekendCheckbox);
			this.Controls.Add(this.holidayCheckbox);
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ContainerPenaltyDayExclusion);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 218, true);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ContainerPenaltyExclusionPopup";
			this.Text = "Penalty Exclusions";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZCheckBox mondayCheckbox;
		ZCheckBox tuesdayCheckbox;
		ZCheckBox wednesdayCheckbox;
		ZCheckBox thursdayCheckbox;
		ZCheckBox fridayCheckbox;
		ZCheckBox saturdayCheckbox;
		ZCheckBox sundayCheckbox;
		ZCheckBox weekendCheckbox;
		ZCheckBox holidayCheckbox;
	}
}
