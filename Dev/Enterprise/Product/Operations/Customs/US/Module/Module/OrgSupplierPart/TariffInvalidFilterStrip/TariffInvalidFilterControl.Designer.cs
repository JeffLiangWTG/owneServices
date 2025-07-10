namespace Enterprise.Customs.US.Module
{
	partial class TariffInvalidFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExpiredDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExpiredDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.TariffInvalidModuleFilter);
			// 
			// ExpiredDateEdit
			// 
			this.ExpiredDateEdit.AllowDrop = true;
			this.ExpiredDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExpiredDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExpiredDateEdit, "ExpiredDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Module.TariffInvalidModuleFilter)(null)).ExpiredDate)));
			this.ExpiredDateEdit.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("2286efbd-b5a7-473f-920b-c55aee22fe9b", "Tariff Expired Date");
			this.ExpiredDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 3, true);
			this.ExpiredDateEdit.Name = "ExpiredDateEdit";
			this.ExpiredDateEdit.TabIndex = 2;
			// 
			// TariffInvalidFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExpiredDateEdit);
			this.Name = "TariffInvalidFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExpiredDateEdit.ResumeLayout(true);
			this.ExpiredDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit ExpiredDateEdit;
	}
}
