namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	partial class CPCAcquitByDateUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.QuantityEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
            this.UnitEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.UnitEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.DataRegistry.Business.CPCAcquitByDate);
            // 
            // QuantityEdit
            // 
            this.BindingSource.SetBindingMember(this.QuantityEdit, "Quantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.ZA.DataRegistry.Business.CPCAcquitByDate)(null)).Quantity)));
            this.QuantityEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
            this.QuantityEdit.Name = "QuantityEdit";
            this.QuantityEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
            this.QuantityEdit.TabIndex = 1;
            // 
            // UnitEdit
            // 
            this.UnitEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnitEdit, "Unit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.DataRegistry.Business.CPCAcquitByDate)(null)).Unit)));
            this.UnitEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 0, true);
            this.UnitEdit.Name = "UnitEdit";
            this.UnitEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.UnitEdit.TabIndex = 3;
            this.UnitEdit.UseFullWidthForCodeBox = true;
            // 
            // CPCAcquitByDateUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.QuantityEdit);
            this.Controls.Add(this.UnitEdit);
            this.Name = "CPCAcquitByDateUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 241, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.UnitEdit.ResumeLayout(true);
            this.UnitEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZIntEdit QuantityEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit UnitEdit;
	}
}
