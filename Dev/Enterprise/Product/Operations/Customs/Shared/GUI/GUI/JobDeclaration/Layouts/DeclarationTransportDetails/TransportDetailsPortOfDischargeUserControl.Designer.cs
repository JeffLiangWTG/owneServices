namespace Enterprise.Customs.GUI
{
	partial class TransportDetailsPortOfDischargeUserControl
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
			this.PortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfDischargeFindBox.SuspendLayout();
			this.DateOfArrivalDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PortOfDischargeFindBox
			// 
			this.PortOfDischargeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeFindBox, "JE_RL_NKPortOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfArrival)));
			this.PortOfDischargeFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("136F0657-0767-4792-98F1-C77B310A77CE", "Discharge", "Port Of Discharge", "");
			this.PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeFindBox.Name = "PortOfDischargeFindBox";
			this.PortOfDischargeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeFindBox.ParentType = null;
			this.PortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PortOfDischargeFindBox.TabIndex = 0;
			// 
			// DateOfArrivalDateEdit
			// 
			this.DateOfArrivalDateEdit.AllowDrop = true;
			this.DateOfArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfArrivalDateEdit, "JE_DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateOfArrival)));
			this.DateOfArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true);
			this.DateOfArrivalDateEdit.Name = "DateOfArrivalDateEdit";
			this.DateOfArrivalDateEdit.TabIndex = 1;
			// 
			// TransportDetailsPortOfDischargeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PortOfDischargeFindBox);
			this.Controls.Add(this.DateOfArrivalDateEdit);
			this.Name = "TransportDetailsPortOfDischargeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfDischargeFindBox.ResumeLayout(true);
			this.PortOfDischargeFindBox.PerformLayout();
			this.DateOfArrivalDateEdit.ResumeLayout(true);
			this.DateOfArrivalDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox PortOfDischargeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit DateOfArrivalDateEdit;
	}
}
