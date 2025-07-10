namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class ArrivalPortUserControl
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
			this.ArrivalPortScheduleKTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ArrivalPortScheduleKDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalPortCodeFindBox.SuspendLayout();
			this.ArrivalPortScheduleKDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill);
			// 
			// ArrivalPortScheduleKTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalPortScheduleKTextBox, "Header+AMA_CustomsFirstArrivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_CustomsFirstArrivalPort)));
			this.ArrivalPortScheduleKTextBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D000", "Schedule K");
			this.ArrivalPortScheduleKTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.ArrivalPortScheduleKTextBox.Name = "ArrivalPortScheduleKTextBox";
			this.ArrivalPortScheduleKTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ArrivalPortScheduleKTextBox.TabIndex = 1;
			// 
			// ArrivalPortScheduleKDropEdit
			// 
			this.ArrivalPortScheduleKDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalPortScheduleKDropEdit, "Header+AMA_CustomsFirstArrivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_CustomsFirstArrivalPort)));
			this.ArrivalPortScheduleKDropEdit.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D000", "Schedule K");
			this.ArrivalPortScheduleKDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 0, true);
			this.ArrivalPortScheduleKDropEdit.Name = "ArrivalPortScheduleKDropEdit";
			this.ArrivalPortScheduleKDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.ArrivalPortScheduleKDropEdit.Visible = false;
			this.ArrivalPortScheduleKDropEdit.TabIndex = 1;
			// 
			// ArrivalPortCodeFindBox
			// 
			this.ArrivalPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalPortCodeFindBox, "Header+AMA_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.USExportAsycudaBill)(null)).Header.AMA_RL_NKPortOfFirstArrival)));
			this.ArrivalPortCodeFindBox.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("427D0CFE-5E28-4C90-A196-BFE33CA8D001", "Port of Arrival UNLOCO");
			this.ArrivalPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalPortCodeFindBox.Name = "ArrivalPortCodeFindBox";
			this.ArrivalPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ArrivalPortCodeFindBox.ParentType = null;
			this.ArrivalPortCodeFindBox.PreBoundMaxLength = 5;
			this.ArrivalPortCodeFindBox.ShowDescriptionBox = false;
			this.ArrivalPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.ArrivalPortCodeFindBox.TabIndex = 0;
			// 
			// ArrivalPortUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalPortCodeFindBox);
			this.Controls.Add(this.ArrivalPortScheduleKTextBox);
			this.Controls.Add(this.ArrivalPortScheduleKDropEdit);
			this.Name = "ArrivalPortUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalPortCodeFindBox.ResumeLayout(true);
			this.ArrivalPortCodeFindBox.PerformLayout();
			this.ArrivalPortScheduleKDropEdit.ResumeLayout(true);
			this.ArrivalPortScheduleKDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ArrivalPortCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox ArrivalPortScheduleKTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ArrivalPortScheduleKDropEdit;
	}
}
