namespace Enterprise.MasterFiles.Module
{
	partial class AssignWhsPutawayGroupControl
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
			this.WarehouseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.WhsPutawayGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WarehouseFindBox.SuspendLayout();
			this.WhsPutawayGroupFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.AssignWhsPutawayGroupControl);
			// 
			// WarehouseFindBox
			// 
			this.WarehouseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseFindBox, "WhsPutawayGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.WarehouseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 65, true);
			this.WarehouseFindBox.Name = "WarehouseFindBox";
			this.WarehouseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.WarehouseFindBox.TabIndex = 1;
			// 
			// WhsPutawayGroupFindBox
			// 
			this.WhsPutawayGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WhsPutawayGroupFindBox, "WarehousePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.WhsPutawayGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 27, true);
			this.WhsPutawayGroupFindBox.Name = "WhsPutawayGroupFindBox";
			this.WhsPutawayGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.WhsPutawayGroupFindBox.TabIndex = 0;
			// 
			// AssignWhsPutawayGroupControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WarehouseFindBox);
			this.Controls.Add(this.WhsPutawayGroupFindBox);
			this.Name = "AssignWhsPutawayGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 104, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WarehouseFindBox.ResumeLayout(true);
			this.WarehouseFindBox.PerformLayout();
			this.WhsPutawayGroupFindBox.ResumeLayout(true);
			this.WhsPutawayGroupFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox WarehouseFindBox;
		private ZArchitecture.GUI.ZGuidFindBox WhsPutawayGroupFindBox;
	}
}
