namespace Enterprise.Freight.Module
{
	partial class VoyageVesselModuleFilterControl
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
			this.VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IncludeArchivedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.operatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Module.VoyageVesselModuleFilter);
			// 
			// operatorDropEdit
			// 
			this.operatorDropEdit.AllowDrop = true;
			this.operatorDropEdit.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("6f00dbba-0643-464a-b43b-4b6c46901167", "Include Archived");
			this.operatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 1, true);
			this.operatorDropEdit.Name = "operatorDropEdit";
			this.operatorDropEdit.ShowDescriptionBox = false;
			this.operatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.operatorDropEdit.TabIndex = 0;
			// 
			// VoyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightTextBox, "VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Module.VoyageVesselModuleFilter)(null)).VoyageFlightNo)));
			this.VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 1, true);
			this.VoyageFlightTextBox.Name = "VoyageFlightTextBox";
			this.VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 20, true);
			this.VoyageFlightTextBox.TabIndex = 1;
			// 
			// VesselFindBox
			// 
			this.VesselFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselFindBox, "Vessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Module.VoyageVesselModuleFilter)(null)).Vessel)));
			this.VesselFindBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("57d29d29-b12e-4998-8b19-1a2db21823c9", "Vessel");
			this.VesselFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 1, true);
			this.VesselFindBox.Name = "VesselFindBox";
			this.VesselFindBox.ShowDescriptionBox = false;
			this.VesselFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.VesselFindBox.TabIndex = 2;
			// 
			// IncludeArchivedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeArchivedCheckBox, "IncludeArchived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Module.VoyageVesselModuleFilter)(null)).IncludeArchived)));
			this.IncludeArchivedCheckBox.CaptionResourceString = Enterprise.Freight.Module.Res.GetData("b2ba277f-e727-4c7a-8b68-ec7986d35dd1", "Include Archived");
			this.IncludeArchivedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeArchivedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 24, true);
			this.IncludeArchivedCheckBox.Name = "IncludeArchivedCheckBox";
			this.IncludeArchivedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.IncludeArchivedCheckBox.TabIndex = 3;
			this.IncludeArchivedCheckBox.UseVisualStyleBackColor = true;
			// 
			// VoyageVesselModuleFilterControl
			// 
			this.Controls.Add(this.operatorDropEdit);
			this.Controls.Add(this.IncludeArchivedCheckBox);
			this.Controls.Add(this.VesselFindBox);
			this.Controls.Add(this.VoyageFlightTextBox);
			this.Name = "VoyageVesselModuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

		}

		#endregion

		private ZArchitecture.ZTextBox VoyageFlightTextBox;
		private ZArchitecture.GUI.ZCodeFindBox VesselFindBox;
		private ZArchitecture.GUI.ZCheckBox IncludeArchivedCheckBox;
		private ZArchitecture.GUI.ZDropEdit operatorDropEdit;
	}
}
