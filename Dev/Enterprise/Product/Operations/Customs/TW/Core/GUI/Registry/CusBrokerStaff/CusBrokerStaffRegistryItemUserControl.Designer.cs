namespace Enterprise.Customs.TW.GUI
{
	partial class CusBrokerStaffRegistryItemUserControl
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
			this.BrokerStaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MailboxDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokerStaffCodeFindBox.SuspendLayout();
			this.MailboxDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusBrokerStaff);
			// 
			// BrokerStaffCodeFindBox
			// 
			this.BrokerStaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerStaffCodeFindBox, "BrokerStaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusBrokerStaff)(null)).BrokerStaffCode)));
			this.BrokerStaffCodeFindBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("c826fa6e-f4b4-49d1-b352-b0bab00ae788", "Broker Staff");
			this.BrokerStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 3, true);
			this.BrokerStaffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerStaffCodeFindBox.Name = "BrokerStaffCodeFindBox";
			this.BrokerStaffCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.BrokerStaffCodeFindBox.TabIndex = 0;
			// 
			// MailboxDropEdit
			// 
			this.MailboxDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MailboxDropEdit, "Mailbox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusBrokerStaff)(null)).Mailbox)));
			this.MailboxDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b4fcde5e-e4ae-4540-9852-3e0645f1e788", "Mail Box");
			this.MailboxDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 29, true);
			this.MailboxDropEdit.Name = "MailboxDropEdit";
			this.MailboxDropEdit.ShouldResizeByMaxLength = true;
			this.MailboxDropEdit.ShowDescriptionBox = false;
			this.MailboxDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MailboxDropEdit.TabIndex = 1;
			// 
			// CusBrokerStaffRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrokerStaffCodeFindBox);
			this.Controls.Add(this.MailboxDropEdit);
			this.Name = "CusBrokerStaffRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokerStaffCodeFindBox.ResumeLayout(true);
			this.BrokerStaffCodeFindBox.PerformLayout();
			this.MailboxDropEdit.ResumeLayout(true);
			this.MailboxDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox BrokerStaffCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit MailboxDropEdit;
	}
}
