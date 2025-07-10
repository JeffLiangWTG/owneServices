namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class RoutingIntegrationOptionsUserControl
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
			this.TheGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConditionalLinkRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NeverLinkRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AlwaysLinkRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TheGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.RoutingIntegrationOptions);
			// 
			// TheGroupBox
			// 
			this.TheGroupBox.Controls.Add(this.ConditionalLinkRadioButton);
			this.TheGroupBox.Controls.Add(this.NeverLinkRadioButton);
			this.TheGroupBox.Controls.Add(this.AlwaysLinkRadioButton);
			this.TheGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TheGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TheGroupBox.Name = "TheGroupBox";
			this.TheGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 117, true);
			this.TheGroupBox.TabIndex = 0;
			this.TheGroupBox.TabStop = false;
			this.TheGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C73606EA-8AED-490B-B690-1E82B59A2BA0", "Options");
			// 
			// ConditionalLinkRadioButton
			// 
			this.ConditionalLinkRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ConditionalLinkRadioButton, "ConditionalLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.RoutingIntegrationOptions)(null)).ConditionalLink)));
			this.ConditionalLinkRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConditionalLinkRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 79, true);
			this.ConditionalLinkRadioButton.Name = "ConditionalLinkRadioButton";
			this.ConditionalLinkRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 24, true);
			this.ConditionalLinkRadioButton.TabIndex = 2;
			this.ConditionalLinkRadioButton.TabStop = true;
			this.ConditionalLinkRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B4D7CD8E-0635-4F21-8D2F-5BA20327A434", " Create && Link when all mandatory fields are entered");
			this.ConditionalLinkRadioButton.UseVisualStyleBackColor = true;
			// 
			// NeverLinkRadioButton
			// 
			this.NeverLinkRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.NeverLinkRadioButton, "NeverLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.RoutingIntegrationOptions)(null)).NeverLink)));
			this.NeverLinkRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NeverLinkRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 49, true);
			this.NeverLinkRadioButton.Name = "NeverLinkRadioButton";
			this.NeverLinkRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 24, true);
			this.NeverLinkRadioButton.TabIndex = 1;
			this.NeverLinkRadioButton.TabStop = true;
			this.NeverLinkRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8A888CF0-4AD6-43C9-ABD7-AEA6F9DB1B74", "Create when key fields are entered, but never link");
			this.NeverLinkRadioButton.UseVisualStyleBackColor = true;
			// 
			// AlwaysLinkRadioButton
			// 
			this.AlwaysLinkRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.AlwaysLinkRadioButton, "AlwaysLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.RoutingIntegrationOptions)(null)).AlwaysLink)));
			this.AlwaysLinkRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AlwaysLinkRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 19, true);
			this.AlwaysLinkRadioButton.Name = "AlwaysLinkRadioButton";
			this.AlwaysLinkRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 24, true);
			this.AlwaysLinkRadioButton.TabIndex = 0;
			this.AlwaysLinkRadioButton.TabStop = true;
			this.AlwaysLinkRadioButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("830B7F4A-9193-43FC-99B5-0B3FA0D67351", "Create && Link when key fields are entered");
			this.AlwaysLinkRadioButton.UseVisualStyleBackColor = true;
			// 
			// RoutingIntegrationOptionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TheGroupBox);
			this.Name = "RoutingIntegrationOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 117, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TheGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TheGroupBox;
		private ZArchitecture.GUI.ZRadioButton ConditionalLinkRadioButton;
		private ZArchitecture.GUI.ZRadioButton NeverLinkRadioButton;
		internal ZArchitecture.GUI.ZRadioButton AlwaysLinkRadioButton;
	}
}
