namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DpsStatusUpdateSettingRegistryControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DpsStatusUpdateOptionEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PhasesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DpsStatusUpdateOptionEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PhasesGrid)).BeginInit();
			this.PhasesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting);
			// 
			// DpsStatusUpdateOptionEdit
			// 
			this.DpsStatusUpdateOptionEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DpsStatusUpdateOptionEdit, "Option");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting)(null)).Option)));
			this.DpsStatusUpdateOptionEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d2bfe3d5-6321-4223-bbe4-55dba6b23a86", "Option");
			this.DpsStatusUpdateOptionEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.DpsStatusUpdateOptionEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DpsStatusUpdateOptionEdit.Name = "DpsStatusUpdateOptionEdit";
			this.DpsStatusUpdateOptionEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 20, true);
			this.DpsStatusUpdateOptionEdit.TabIndex = 0;
			// 
			// PhasesGrid
			// 
			this.PhasesGrid.AllowNavigation = false;
			this.PhasesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PhasesGrid, "JobUpdateSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting)(null)).JobUpdateSettings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.JobPhaseSetting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting)(null)).JobUpdateSettings)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.JobPhaseSetting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting)(null)).JobUpdateSettings)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Registry.JobPhaseSetting)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.DpsStatusUpdateSetting)(null)).JobUpdateSettings)).SyncRoot)).ShouldUpdate)));
			this.PhasesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d11cadd1-13ec-42ef-b667-6fa5c8e8fb43", "Phase Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("31f4a598-5b04-48b7-8527-836626c5466f", "Phase Name");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo1.Caption = "";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f878df39-15a2-42ea-a8cf-98b65fd6261c", "Status Updates");
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldUpdate";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PhasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PhasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PhasesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PhasesGrid.GridId = "c8621683-b3d8-4679-8287-092c79a5658e";
			this.PhasesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PhasesGrid.LayoutKey = "PhasesGrid";
			this.PhasesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.PhasesGrid.Name = "PhasesGrid";
			this.PhasesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 231, true);
			this.PhasesGrid.TabIndex = 1;
			this.PhasesGrid.Visible = false;
			// 
			// DpsStatusUpdateSettingRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DpsStatusUpdateOptionEdit);
			this.Controls.Add(this.PhasesGrid);
			this.Name = "DpsStatusUpdateSettingRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DpsStatusUpdateOptionEdit.ResumeLayout(true);
			this.DpsStatusUpdateOptionEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PhasesGrid)).EndInit();
			this.PhasesGrid.ResumeLayout(false);
			this.PhasesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit DpsStatusUpdateOptionEdit;
		internal ZArchitecture.ZGrid PhasesGrid;
	}
}
