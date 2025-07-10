namespace Enterprise.Customs.GUI
{
	partial class ShipmentDetailsFinalDestinationUserControl
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
			this.FinalDestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EstimatedArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinalDestinationFindBox.SuspendLayout();
			this.EstimatedArrivalDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// FinalDestinationFindBox
			// 
			this.FinalDestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationFindBox, "JE_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RL_NKFinalDestination)));
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.FinalDestinationFindBox.Name = "FinalDestinationFindBox";
			this.FinalDestinationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationFindBox.ParentType = null;
			this.FinalDestinationFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.FinalDestinationFindBox.TabIndex = 0;
			// 
			// EstimatedArrivalDateEdit
			// 
			this.EstimatedArrivalDateEdit.AllowDrop = true;
			this.EstimatedArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedArrivalDateEdit, "JE_DateAtFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DateAtFinalDestination)));
			this.EstimatedArrivalDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseCustomsDeclarationUserControl|fcdbadef-4517-44f7-bc0a-cc44c09f353a", "ETA");
			this.EstimatedArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
			this.EstimatedArrivalDateEdit.Name = "EstimatedArrivalDateEdit";
			this.EstimatedArrivalDateEdit.TabIndex = 1;
			// 
			// ShipmentDetailsFinalDestinationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinalDestinationFindBox);
			this.Controls.Add(this.EstimatedArrivalDateEdit);
			this.Name = "ShipmentDetailsFinalDestinationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.EstimatedArrivalDateEdit.ResumeLayout(true);
			this.EstimatedArrivalDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstimatedArrivalDateEdit;
	}
}
