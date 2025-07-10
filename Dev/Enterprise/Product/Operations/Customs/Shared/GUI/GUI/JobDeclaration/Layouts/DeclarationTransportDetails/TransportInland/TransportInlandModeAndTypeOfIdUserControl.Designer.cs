namespace Enterprise.Customs.GUI
{
	partial class TransportInlandModeAndTypeOfIdUserControl
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
			this.InlandModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeOfIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();

			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.TypeOfIDDropEdit.SuspendLayout();
			this.SuspendLayout();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);

			// 
			// InlandModeOfTransportDropEdit
			// 
			this.InlandModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandModeOfTransportDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportModeInland)));
			this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InlandModeOfTransportDropEdit.Name = "InlandModeOfTransportDropEdit";
			this.InlandModeOfTransportDropEdit.PreBoundMaxLength = 3;
			this.InlandModeOfTransportDropEdit.ShouldResizeByMaxLength = true;
			this.InlandModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.InlandModeOfTransportDropEdit.TabIndex = 0;

			// 
			// TypeOfIDDropEdit
			// 
			this.TypeOfIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfIDDropEdit, "JE_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMeans)));
			this.TypeOfIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 0, true);
			this.TypeOfIDDropEdit.Name = "TypeOfIDDropEdit";
			this.TypeOfIDDropEdit.PreBoundMaxLength = 2;
			this.TypeOfIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.TypeOfIDDropEdit.TabIndex = 1;

			// 
			// TransportInlandOwnPropulsionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InlandModeOfTransportDropEdit);
			this.Controls.Add(this.TypeOfIDDropEdit);
			this.Name = "TransportInlandModeAndTypeOfIdUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.TypeOfIDDropEdit.ResumeLayout(true);
			this.TypeOfIDDropEdit.PerformLayout();

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit InlandModeOfTransportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TypeOfIDDropEdit;
	}
}
