namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class DepartureDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.StampDutyStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StampDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoodsShippingLocationAndGIKUserControl = new Enterprise.Customs.TR.NCTS.GUI.GoodsShippingLocationAndGIKUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StampDutyStatusDropEdit.SuspendLayout();
			this.StampDutyCalcEdit.SuspendLayout();
			this.RegistrationDateEdit.SuspendLayout();
			this.GoodsShippingLocationAndGIKUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsHeader);
			// 
			// StampDutyStatusDropEdit
			// 
			this.StampDutyStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyStatusDropEdit, "StampDutyStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).StampDutyStatus)));
			this.StampDutyStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 180, true);
			this.StampDutyStatusDropEdit.Name = "StampDutyStatusDropEdit";
			this.StampDutyStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.StampDutyStatusDropEdit.TabIndex = 0;
			// 
			// StampDutyCalcEdit
			// 
			this.StampDutyCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyCalcEdit, "StampDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).StampDuty)));
			this.StampDutyCalcEdit.CaptionResourceString = null;
			this.StampDutyCalcEdit.DecimalPlaces = 2;
			this.StampDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 39, true);
			this.StampDutyCalcEdit.Name = "StampDutyCalcEdit";
			this.StampDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 15, true);
			this.StampDutyCalcEdit.TabIndex = 0;
			this.StampDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).RegistrationDate)));
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 63, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 0;
			// 
			// GoodsShippingLocationAndGIKUserControl
			// 
			this.GoodsShippingLocationAndGIKUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsShippingLocationAndGIKUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)))));
			this.GoodsShippingLocationAndGIKUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 376, true);
			this.GoodsShippingLocationAndGIKUserControl.Name = "GoodsShippingLocationAndGIKUserControl";
			this.GoodsShippingLocationAndGIKUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 26, true);
			this.GoodsShippingLocationAndGIKUserControl.TabIndex = 0;
			// 
			// DepartureDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StampDutyStatusDropEdit);
			this.Controls.Add(this.StampDutyCalcEdit);
			this.Controls.Add(this.RegistrationDateEdit);
			this.Controls.Add(this.GoodsShippingLocationAndGIKUserControl);
			this.Name = "DepartureDetailsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StampDutyStatusDropEdit.ResumeLayout(true);
			this.StampDutyStatusDropEdit.PerformLayout();
			this.StampDutyCalcEdit.ResumeLayout(true);
			this.StampDutyCalcEdit.PerformLayout();
			this.RegistrationDateEdit.ResumeLayout(true);
			this.RegistrationDateEdit.PerformLayout();
			this.GoodsShippingLocationAndGIKUserControl.ResumeLayout(true);
			this.GoodsShippingLocationAndGIKUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZDropEdit StampDutyStatusDropEdit;
		public Enterprise.ZArchitecture.ZCalcEdit StampDutyCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZDateEdit RegistrationDateEdit;
		public GoodsShippingLocationAndGIKUserControl GoodsShippingLocationAndGIKUserControl;
	}
}
