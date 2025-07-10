using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Telematics.GUI.Registry
{
	partial class OverspeedAlertConfigurationControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.overspeedAlertType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.durationInMinutes = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.overspeedAlertType.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Telematics.GUI.Registry.OverspeedAlertConfigurationControl);
			// 
			// overspeedAlertType
			// 
			this.overspeedAlertType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.overspeedAlertType, "OverspeedAlertType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Telematics.Business.Registry.OverspeedAlertConfiguration)(null)).OverspeedAlertType)));
			this.overspeedAlertType.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("75DCF0E3-7FC6-4B28-A6C1-C71940D56995", " ");
			this.overspeedAlertType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.overspeedAlertType.Name = "overspeedAlertType";
			this.overspeedAlertType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
			this.overspeedAlertType.TabIndex = 0;
			this.overspeedAlertType.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
			// 
			// durationInMinutes
			// 
			this.durationInMinutes.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.durationInMinutes, "DurationInMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Telematics.Business.Registry.OverspeedAlertConfiguration)(null)).DurationInMinutes)));
			this.durationInMinutes.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("OverspeedAlertConfiguration|8061B8FF-5E76-4D2F-923B-0765ED3E72A3", "Duration in Minutes");
			this.durationInMinutes.DecimalPlaces = 0;
			this.durationInMinutes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 63, true);
			this.durationInMinutes.Name = "durationInMinutes";
			this.durationInMinutes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.durationInMinutes.TabIndex = 0;
			this.durationInMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverspeedAlertConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.overspeedAlertType);
			this.Controls.Add(this.durationInMinutes);
			this.Name = "OverspeedAlertConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 208, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.overspeedAlertType.ResumeLayout(true);
			this.overspeedAlertType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		/// <summary> 
		/// Required designer variable.
		/// </summary>

		private ZDropEdit overspeedAlertType;
		private ZCalcEdit durationInMinutes;

	}
}
