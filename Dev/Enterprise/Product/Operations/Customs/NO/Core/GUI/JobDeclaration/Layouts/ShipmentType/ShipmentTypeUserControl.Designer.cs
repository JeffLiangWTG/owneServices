namespace Enterprise.Customs.NO.GUI
{
	partial class ShipmentTypeUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CustomsTransportModeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsTransportModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// CustomsTransportModeDropEdit
			// 
			this.CustomsTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsTransportModeDropEdit, "JE_CustomsTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_CustomsTransportMode)));
			this.CustomsTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsTransportModeDropEdit.Name = "CustomsTransportModeDropEdit";
			this.CustomsTransportModeDropEdit.PreBoundMaxLength = 2;
			this.CustomsTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.CustomsTransportModeDropEdit.TabIndex = 1;
			// 
			// ShipmentTypeLayoutUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsTransportModeDropEdit);
			this.Name = "ShipmentTypeLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsTransportModeDropEdit.ResumeLayout(true);
			this.CustomsTransportModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDropEdit CustomsTransportModeDropEdit;
	}
}
