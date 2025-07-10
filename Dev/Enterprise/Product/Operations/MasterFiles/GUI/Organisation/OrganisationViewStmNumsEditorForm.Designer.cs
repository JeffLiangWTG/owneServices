namespace Enterprise.MasterFiles.GUI
{
	partial class OrganisationViewStmNumsEditorForm
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
		private new void InitializeComponent()
		{
			this.ZoneIDPrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientPrefexTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrganisationViewStmNums);
			// 
			// ZoneIDPrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZoneIDPrefixTextBox, "SN_ZoneIDPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationViewStmNums)(null)).SN_ZoneIDPrefix)));
			this.ZoneIDPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 38, true);
			this.ZoneIDPrefixTextBox.Name = "ZoneIDPrefixTextBox";
			this.ZoneIDPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 18, true);
			this.ZoneIDPrefixTextBox.TabIndex = 2;
			// 
			// ClientPrefexTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientPrefexTextBox, "SN_ClientPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrganisationViewStmNums)(null)).SN_ClientPrefix)));
			this.ClientPrefexTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 38, true);
			this.ClientPrefexTextBox.Name = "ClientPrefexTextBox";
			this.ClientPrefexTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 18, true);
			this.ClientPrefexTextBox.TabIndex = 3;
			// 
			// OrganisationViewStmNumsEditorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 223, true);
			this.Controls.Add(this.ClientPrefexTextBox);
			this.Controls.Add(this.ZoneIDPrefixTextBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrganisationViewStmNums);
			this.Name = "OrganisationViewStmNumsEditorForm";
			this.Controls.SetChildIndex(this.ZoneIDPrefixTextBox, 0);
			this.Controls.SetChildIndex(this.ClientPrefexTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.ZTextBox ZoneIDPrefixTextBox;
		public ZArchitecture.ZTextBox ClientPrefexTextBox;
	}
}
