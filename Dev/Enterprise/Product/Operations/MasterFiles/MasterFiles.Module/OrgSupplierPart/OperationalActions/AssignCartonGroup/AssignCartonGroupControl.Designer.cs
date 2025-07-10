namespace Enterprise.MasterFiles.Module
{
	partial class AssignCartonGroupControl
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
		void InitializeComponent()
		{
			this.CartonGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OrganisationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CartonGroupFindBox.SuspendLayout();
			this.OrganisationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.AssignCartonGroupControl);
			// 
			// CartonGroupFindBox
			// 
			this.CartonGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CartonGroupFindBox, "CartonGroupPK");
			this.CartonGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 21, true);
			this.CartonGroupFindBox.Name = "CartonGroupFindBox";
			this.CartonGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.CartonGroupFindBox.TabIndex = 0;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "OrganisationPK");
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 63, true);
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.OrganisationFindBox.TabIndex = 1;
			// 
			// AssignCartonGroupControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OrganisationFindBox);
			this.Controls.Add(this.CartonGroupFindBox);
			this.Name = "AssignCartonGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CartonGroupFindBox.ResumeLayout(true);
			this.CartonGroupFindBox.PerformLayout();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox CartonGroupFindBox;
		private ZArchitecture.GUI.ZGuidFindBox OrganisationFindBox;
	}
}
