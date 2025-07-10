namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class DeparturePortUserControl
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
			this.DeparturePortScheduleDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeparturePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeparturePortScheduleDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeparturePortCodeFindBox.SuspendLayout();
			this.DeparturePortScheduleDDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// DeparturePortScheduleDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeparturePortScheduleDTextBox, "Header+AMA_CustomsFinalDeparturePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_CustomsFinalDeparturePort)));
			this.DeparturePortScheduleDTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D003", "Schedule D");
			this.DeparturePortScheduleDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.DeparturePortScheduleDTextBox.Name = "DeparturePortScheduleDTextBox";
			this.DeparturePortScheduleDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.DeparturePortScheduleDTextBox.TabIndex = 1;
			// 
			// DeparturePortScheduleDDropEdit
			// 
			this.DeparturePortScheduleDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeparturePortScheduleDDropEdit, "Header+AMA_CustomsFinalDeparturePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_CustomsFinalDeparturePort)));
			this.DeparturePortScheduleDDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D003", "Schedule D");
			this.DeparturePortScheduleDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.DeparturePortScheduleDDropEdit.Name = "DeparturePortScheduleDDropEdit";
			this.DeparturePortScheduleDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.DeparturePortScheduleDDropEdit.Visible = false;
			this.DeparturePortScheduleDDropEdit.TabIndex = 1;
			// 
			// DeparturePortCodeFindBox
			// 
			this.DeparturePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeparturePortCodeFindBox, "Header+AMA_RL_NKPortOfFinalDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_RL_NKPortOfFinalDeparture)));
			this.DeparturePortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D004", "Port of Departure UNLOCO");
			this.DeparturePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeparturePortCodeFindBox.Name = "DeparturePortCodeFindBox";
			this.DeparturePortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeparturePortCodeFindBox.ParentType = null;
			this.DeparturePortCodeFindBox.PreBoundMaxLength = 5;
			this.DeparturePortCodeFindBox.ShowDescriptionBox = false;
			this.DeparturePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.DeparturePortCodeFindBox.TabIndex = 0;
			// 
			// DeparturePortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeparturePortCodeFindBox);
			this.Controls.Add(this.DeparturePortScheduleDTextBox);
			this.Controls.Add(this.DeparturePortScheduleDDropEdit);
			this.Name = "DeparturePortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeparturePortCodeFindBox.ResumeLayout(true);
			this.DeparturePortCodeFindBox.PerformLayout();
			this.DeparturePortScheduleDDropEdit.ResumeLayout(true);
			this.DeparturePortScheduleDDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DeparturePortCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox DeparturePortScheduleDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DeparturePortScheduleDDropEdit;
	}
}
