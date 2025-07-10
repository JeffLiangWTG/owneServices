
namespace Enterprise.Customs.GUI
{
	partial class TransportDetailsPortOfLoadingUserControl
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
			this.PortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfLoadingFindBox.SuspendLayout();
			this.ExportDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PortOfLoadingFindBox
			// 
			this.PortOfLoadingFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingFindBox, "JE_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfLoading)));
			this.PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfLoadingFindBox.Name = "PortOfLoadingFindBox";
			this.PortOfLoadingFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingFindBox.ParentType = null;
			this.PortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PortOfLoadingFindBox.TabIndex = 0;
			// 
			// ExportDateEdit
			// 
			this.ExportDateEdit.AllowDrop = true;
			this.ExportDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExportDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExportDateEdit, "JE_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ExportDate)));
			this.ExportDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AFA67BE4-28E8-42A3-A1F1-243FA0B11326", "Dep.", "Departure", "");
			this.ExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true);
			this.ExportDateEdit.Name = "ExportDateEdit";
			this.ExportDateEdit.TabIndex = 1;
			// 
			// TransportDetailsPortOfLoadingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PortOfLoadingFindBox);
			this.Controls.Add(this.ExportDateEdit);
			this.Name = "TransportDetailsPortOfLoadingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfLoadingFindBox.ResumeLayout(true);
			this.PortOfLoadingFindBox.PerformLayout();
			this.ExportDateEdit.ResumeLayout(true);
			this.ExportDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox PortOfLoadingFindBox;
		internal ZArchitecture.GUI.ZDateEdit ExportDateEdit;
	}
}
