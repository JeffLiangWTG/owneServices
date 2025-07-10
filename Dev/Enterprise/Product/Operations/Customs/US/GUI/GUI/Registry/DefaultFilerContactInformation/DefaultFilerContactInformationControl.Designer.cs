namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class DefaultFilerContactInformationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.DefaultFilerContactInformation);
			// 
			// ContactNameTextBox
			// 
			this.ContactNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.DefaultFilerContactInformation)(null)).ContactName)));
			this.ContactNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d4b85151-c449-4db3-85de-54705319e413", "Contact Name ");
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 6, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.ContactNameTextBox.TabIndex = 1;
			// 
			// ContactPhoneTextBox
			// 
			this.ContactPhoneTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactPhoneTextBox, "ContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.DefaultFilerContactInformation)(null)).ContactPhone)));
			this.ContactPhoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4afbbdf0-9dbb-454a-ab80-ade20f7747ed", "Contact Phone");
			this.ContactPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 32, true);
			this.ContactPhoneTextBox.Name = "ContactPhoneTextBox";
			this.ContactPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.ContactPhoneTextBox.TabIndex = 1;
			// 
			// DefaultFilerContactInformationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContactNameTextBox);
			this.Controls.Add(this.ContactPhoneTextBox);
			this.Name = "DefaultFilerContactInformationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.ZTextBox ContactNameTextBox;
		Enterprise.ZArchitecture.ZTextBox ContactPhoneTextBox;

	}
}
