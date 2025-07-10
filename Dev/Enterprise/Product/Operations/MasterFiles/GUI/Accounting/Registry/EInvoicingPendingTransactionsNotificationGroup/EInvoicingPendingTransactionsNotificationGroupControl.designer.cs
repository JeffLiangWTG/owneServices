namespace Enterprise.MasterFiles.GUI
{
	partial class EInvoicingPendingTransactionsNotificationGroupControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GroupPKGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DaysIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupPKGuidFindBox.SuspendLayout();
			this.DateTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EInvoicingPendingTransactionsNotificationGroup);
			// 
			// zGuidFindBox1
			// 
			this.GroupPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupPKGuidFindBox, "GroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EInvoicingPendingTransactionsNotificationGroup)(null)).GroupPK)));
			this.GroupPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 19, true);
			this.GroupPKGuidFindBox.Name = "zGuidFindBox1";
			this.GroupPKGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GroupPKGuidFindBox.ParentType = null;
			this.GroupPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GroupPKGuidFindBox.TabIndex = 0;
			// 
			// zDropEdit1
			// 
			this.DateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateTypeDropEdit, "DateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EInvoicingPendingTransactionsNotificationGroup)(null)).DateType)));
			this.DateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 54, true);
			this.DateTypeDropEdit.Name = "zDropEdit1";
			this.DateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DateTypeDropEdit.TabIndex = 1;
			// 
			// zIntEdit1
			// 
			this.BindingSource.SetBindingMember(this.DaysIntEdit, "Days");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.EInvoicingPendingTransactionsNotificationGroup)(null)).Days)));
			this.DaysIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 89, true);
			this.DaysIntEdit.Name = "zIntEdit1";
			this.DaysIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DaysIntEdit.TabIndex = 2;
			// 
			// EInvoicingPendingTransactionsNotificationGroupControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DaysIntEdit);
			this.Controls.Add(this.DateTypeDropEdit);
			this.Controls.Add(this.GroupPKGuidFindBox);
			this.Name = "EInvoicingPendingTransactionsNotificationGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 126, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupPKGuidFindBox.ResumeLayout(true);
			this.GroupPKGuidFindBox.PerformLayout();
			this.DateTypeDropEdit.ResumeLayout(true);
			this.DateTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GroupPKGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DateTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZIntEdit DaysIntEdit;
	}
}
