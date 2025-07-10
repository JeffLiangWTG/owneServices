using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class CargoGuideApiSettingsControl
	{
		ZArchitecture.ZTextBox ApiURLTextBox;
		ZArchitecture.ZTextBox ApiVersionTextBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		/// 

		private void InitializeComponent()
		{
			this.ApiURLTextBox = new ZArchitecture.ZTextBox();
			this.ApiVersionTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoGuideCredentials);
			// 
			// ApiURLTextBox
			// 
			this.ApiURLTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ApiURLTextBox, "ApiURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoGuideApiSettings)(null)).ApiURL);
			this.ApiURLTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApiURLTextBox.CaptionResourceString = Res.GetData("CargoGuideApiSettingsControl|27243d7b-b4e4-4eff-8590-b101552e599b", "CargoGuide API URL");
			this.ApiURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 27, true);
			this.ApiURLTextBox.Name = "ApiURLTextBox";
			this.ApiURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.ApiURLTextBox.TabIndex = 1;
			// 
			// ApiVersionTextBox
			// 
			this.ApiVersionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ApiVersionTextBox, "ApiVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoGuideApiSettings)(null)).ApiVersion);
			this.ApiVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApiVersionTextBox.CaptionResourceString = Res.GetData("CargoGuideApiSettingsControl|72b6c08f-25cb-456b-bf1e-fc0dedf8afc9", "CargoGuide API Version");
			this.ApiVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 51, true);
			this.ApiVersionTextBox.Name = "ApiVersionTextBox";
			this.ApiVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.ApiVersionTextBox.TabIndex = 2;

			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ApiVersionTextBox);
			this.Controls.Add(this.ApiURLTextBox);
			this.Name = "CargoGuideApiSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 157, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
