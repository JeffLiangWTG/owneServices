namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class TransportDepartureUserControl
	{
		private void InitializeComponent()
		{
			this.TankerStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TankerStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// TankerStatusDropEdit
			// 
			this.TankerStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TankerStatusDropEdit, "TankerStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).TankerStatus)));
			this.TankerStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 80, true);
			this.TankerStatusDropEdit.Name = "TankerStatusDropEdit";
			this.TankerStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.TankerStatusDropEdit.TabIndex = 11;
			// 
			// TransportDepartureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TankerStatusDropEdit);
			this.Name = "TransportDepartureUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TankerStatusDropEdit.ResumeLayout(true);
			this.TankerStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.ZArchitecture.GUI.ZDropEdit TankerStatusDropEdit;
	}
}

