
namespace Enterprise.TransportBookings.Module
{
	partial class CarrierBookingAgentChangeControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare CarrierBookingAgentFindBox;

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
			this.CarrierBookingAgentFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox.Bare();
			this.CarrierBookingAgentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverridingValuesLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierBookingAgentFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// CarrierBookingAgentFindBox
			// 
			this.CarrierBookingAgentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierBookingAgentFindBox, "CarrierBookingAgentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Module.CarrierBookingAgentChangeActionMethodApplicator)(null)).CarrierBookingAgentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportBookings.Module.CarrierBookingAgentChangeActionMethodApplicator)(null)).CarrierBookingAgents)));
			this.CarrierBookingAgentFindBox.BindToList = "CarrierBookingAgents";
			this.CarrierBookingAgentFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CarrierBookingAgentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 36, true);
			this.CarrierBookingAgentFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.CarrierBookingAgentFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CarrierBookingAgentFindBox.Name = "CarrierBookingAgentFindBox";
			this.CarrierBookingAgentFindBox.ShouldResize = true;
			this.CarrierBookingAgentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.CarrierBookingAgentFindBox.TabIndex = 2;
			// 
			// CarrierBookingAgentLabel
			// 
			this.CarrierBookingAgentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CarrierBookingAgentLabel.IsFontBold = true;
			this.CarrierBookingAgentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.CarrierBookingAgentLabel.Name = "CarrierBookingAgentLabel";
			this.CarrierBookingAgentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.CarrierBookingAgentLabel.TabIndex = 1;
			this.CarrierBookingAgentLabel.Text = "Carrier Booking Agent:";
			// 
			// OverridingValuesLabel
			// 
			this.OverridingValuesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OverridingValuesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
			this.OverridingValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OverridingValuesLabel.Name = "OverridingValuesLabel";
			this.OverridingValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 23, true);
			this.OverridingValuesLabel.TabIndex = 0;
			this.OverridingValuesLabel.Text = "Overriding Values";
			// 
			// CarrierBookingAgentChangeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OverridingValuesLabel);
			this.Controls.Add(this.CarrierBookingAgentLabel);
			this.Controls.Add(this.CarrierBookingAgentFindBox);
			this.Name = "CarrierBookingAgentChangeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierBookingAgentFindBox.ResumeLayout(true);
			this.CarrierBookingAgentFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel CarrierBookingAgentLabel;
		private ZArchitecture.ZHeaderLabel OverridingValuesLabel;
	}
}
