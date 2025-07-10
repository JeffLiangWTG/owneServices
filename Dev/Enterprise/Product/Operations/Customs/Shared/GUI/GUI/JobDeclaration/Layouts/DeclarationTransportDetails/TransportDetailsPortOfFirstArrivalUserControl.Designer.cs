namespace Enterprise.Customs.GUI
{
	partial class TransportDetailsPortOfFirstArrivalUserControl
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
			this.PortOfFirstArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfFirstArrivalBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.DateOfFirstArrivalBoundDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// PortOfFirstArrivalCodeFindBox
			// 
			this.PortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfFirstArrivalCodeFindBox, "JE_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKPortOfFirstArrival)));
			this.PortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PortOfFirstArrivalCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfFirstArrivalCodeFindBox.Name = "PortOfFirstArrivalCodeFindBox";
			this.PortOfFirstArrivalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfFirstArrivalCodeFindBox.ParentType = null;
			this.PortOfFirstArrivalCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.PortOfFirstArrivalCodeFindBox.TabIndex = 0;
			// 
			// DateOfFirstArrivalBoundDateEdit
			// 
			this.DateOfFirstArrivalBoundDateEdit.AllowDrop = true;
			this.DateOfFirstArrivalBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateOfFirstArrivalBoundDateEdit, "JE_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateOfFirstArrival)));
			this.DateOfFirstArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true);
			this.DateOfFirstArrivalBoundDateEdit.Name = "DateOfFirstArrivalBoundDateEdit";
			this.DateOfFirstArrivalBoundDateEdit.TabIndex = 1;
			// 
			// TransportDetailsPortOfFirstArrivalUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PortOfFirstArrivalCodeFindBox);
			this.Controls.Add(this.DateOfFirstArrivalBoundDateEdit);
			this.Name = "TransportDetailsPortOfFirstArrivalUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.PortOfFirstArrivalCodeFindBox.PerformLayout();
			this.DateOfFirstArrivalBoundDateEdit.ResumeLayout(true);
			this.DateOfFirstArrivalBoundDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox PortOfFirstArrivalCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit DateOfFirstArrivalBoundDateEdit;
	}
}
