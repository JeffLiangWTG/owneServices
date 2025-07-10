namespace Enterprise.TransportBookings.GUI
{
	partial class TransportBookingTemplateForm
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
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 374, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 347, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 347, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 374, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingTmpl);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_Direction)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_InstructionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_OrgType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_DropMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_IsContainerRateable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingInstructionTmpl)(((System.Collections.IList)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).Instructions)).SyncRoot)).K2_IsLooseRateable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_IsSystem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_RatingFreightMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBookingTmpl)(null)).KT_IsActive)));
			// 
			// TransportBookingTemplateForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 430, true);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingTmpl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 468, true);
			this.Name = "TransportBookingTemplateForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox BookingInstructionTemplateGroupBox;
		private ZArchitecture.ZGrid BookingInstructionTemplateGrid;
		private ZArchitecture.GUI.ZPanel BookingTemplatePanel;
		private ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		private ZArchitecture.ZTranslatableTextControl DescriptionTextbox;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private ZArchitecture.GUI.ZDropEdit RatingFreightModeDropEdit;
		private ZArchitecture.GUI.ZCheckBox IsHiddenCheckBox;
	}
}
