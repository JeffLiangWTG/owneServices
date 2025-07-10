namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class Phase5ArrivalNotificationDetailsUserControl
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
			this.RepresentativeTraderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RepresentativeTraderGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.NCTS.Business.NctsHeader);
			// 
			// RepresentativeTraderGuidFindBox
			// 
			this.RepresentativeTraderGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeTraderGuidFindBox, "ArrivalMovementHeader.RepresentativeTrader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.RepresentativeTrader)));
			this.RepresentativeTraderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 36, true);
			this.RepresentativeTraderGuidFindBox.Name = "RepresentativeTraderGuidFindBox";
			this.RepresentativeTraderGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RepresentativeTraderGuidFindBox.ParentType = null;
			this.RepresentativeTraderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.RepresentativeTraderGuidFindBox.TabIndex = 0;
			// 
			// Phase5ArrivalNotificationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RepresentativeTraderGuidFindBox);
			this.Name = "Phase5ArrivalNotificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 227, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RepresentativeTraderGuidFindBox.ResumeLayout(true);
			this.RepresentativeTraderGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidFindBox RepresentativeTraderGuidFindBox;
	}
}
