namespace Enterprise.Customs.NO.GUI
{
	partial class ShipmentDetailsWeightAndVolumeUserControl
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
            this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WeightCalcDropEdit.SuspendLayout();
            this.VolumeCalcDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
            // 
            // WeightCalcDropEdit
            // 
            this.WeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_TotalWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_TotalWeightUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).Lookups.WeightUnitList)));
            this.WeightCalcDropEdit.BindToAmount = "JE_TotalWeight";
            this.WeightCalcDropEdit.BindToList = "Lookups.WeightUnitList";
            this.WeightCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
            this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
            this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.WeightCalcDropEdit.TabIndex = 5;
            this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // VolumeCalcDropEdit
            // 
            this.VolumeCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_TotalVolume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).JE_TotalVolumeUnit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).Lookups.VolumeUnitList)));
            this.VolumeCalcDropEdit.BindToAmount = "JE_TotalVolume";
            this.VolumeCalcDropEdit.BindToList = "Lookups.VolumeUnitList";
            this.VolumeCalcDropEdit.BindToUnit = "JE_TotalVolumeUnit";
            this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 0, true);
            this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
            this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.VolumeCalcDropEdit.TabIndex = 6;
            this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // ShipmentDetailsWeightAndVolumeUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.WeightCalcDropEdit);
            this.Controls.Add(this.VolumeCalcDropEdit);
            this.Name = "ShipmentDetailsWeightAndVolumeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 33, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WeightCalcDropEdit.ResumeLayout(true);
            this.WeightCalcDropEdit.PerformLayout();
            this.VolumeCalcDropEdit.ResumeLayout(true);
            this.VolumeCalcDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
	}
}
