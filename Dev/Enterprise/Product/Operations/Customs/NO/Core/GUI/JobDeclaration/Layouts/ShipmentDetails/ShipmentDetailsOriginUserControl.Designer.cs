namespace Enterprise.Customs.NO.GUI
{
	partial class ShipmentDetailsOriginUserControl
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
			this.EstimatedDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EstimatedDepartureDateEdit.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginFindBox, "JE_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_RL_NKOrigin)));
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OriginFindBox.ParentType = null;
			this.OriginFindBox.PreBoundMaxLength = 5;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.OriginFindBox.TabIndex = 0;
			// 
			// EstimatedDepartureDateEdit
			// 
			this.EstimatedDepartureDateEdit.AllowDrop = true;
			this.EstimatedDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedDepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedDepartureDateEdit, "JE_DateAtOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_DateAtOrigin)));
			this.EstimatedDepartureDateEdit.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("FADD1C40-64D7-43E2-918F-0A58D9FB86FE", "ETD");
			this.EstimatedDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
			this.EstimatedDepartureDateEdit.Name = "EstimatedDepartureDateEdit";
			this.EstimatedDepartureDateEdit.TabIndex = 2;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "JE_GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_GoodsDestination)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsOriginCodeFindBox, false);
			this.GoodsOriginCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 0, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginCodeFindBox.ShowDescriptionBox = false;
			this.GoodsOriginCodeFindBox.TabIndex = 1;
			// 
			// ShipmentDetailsOriginUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EstimatedDepartureDateEdit);
			this.Controls.Add(this.OriginFindBox);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Name = "ShipmentDetailsOriginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EstimatedDepartureDateEdit.ResumeLayout(true);
			this.EstimatedDepartureDateEdit.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstimatedDepartureDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
	}
}
