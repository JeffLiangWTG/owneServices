namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PhaseSecurityRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.Freight.Forwarding.GUI.ZPhaseDependantsFindBoxColumnStyleInfo zPhaseDependantsFindBoxColumnStyleInfo1 = new Enterprise.Freight.Forwarding.GUI.ZPhaseDependantsFindBoxColumnStyleInfo();
			this.IsEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PhasesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RulesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PhasesGrid)).BeginInit();
			this.PhasesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).BeginInit();
			this.RulesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Registry.PhaseSecurity);
			// 
			// IsEnabledCheckBox
			// 
			this.IsEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsEnabledCheckBox, "IsEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).IsEnabled)));
			this.IsEnabledCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("bd54c6fe-e336-44ef-8772-e3b9d2fc48d0", "Enable Phase Based Security");
			this.IsEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IsEnabledCheckBox.Name = "IsEnabledCheckBox";
			this.IsEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 17, true);
			this.IsEnabledCheckBox.TabIndex = 0;
			this.IsEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// PhasesGrid
			// 
			this.PhasesGrid.AllowNavigation = false;
			this.PhasesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PhasesGrid, "Phases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).EnglishDescription)));
			this.PhasesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d11cadd1-13ec-42ef-b667-6fa5c8e8fb43", "Phase Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("31f4a598-5b04-48b7-8527-836626c5466f", "Phase Name");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.PhasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PhasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PhasesGrid.GridId = "c8621683-b3d8-4679-8287-092c79a5658e";
			this.PhasesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PhasesGrid.LayoutKey = "PhasesGrid";
			this.PhasesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.PhasesGrid.Name = "PhasesGrid";
			this.PhasesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 166, true);
			this.PhasesGrid.TabIndex = 1;
			// 
			// RulesGrid
			// 
			this.RulesGrid.AllowNavigation = false;
			this.RulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RulesGrid, "Phases.Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Registry.PhaseRule)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).Rules)).SyncRoot)).DepartmentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.PhaseRule)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).Rules)).SyncRoot)).Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Registry.PhaseRule)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.Phase)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Registry.PhaseSecurity)(null)).Phases)).SyncRoot)).Rules)).SyncRoot)).DependantsText)));
			this.RulesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.Caption = "";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("bd53ab90-7fb8-4c77-8ad7-d8520929a351", "Department");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "DepartmentPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5f708c9b-5388-4e82-a36c-35b8d5577ef3", "Location");
			zDropEditColumnStyleInfo1.ColumnName = "Location";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zPhaseDependantsFindBoxColumnStyleInfo1.Caption = "";
			zPhaseDependantsFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f4be7bdd-d4e6-41cf-b7b7-505a16bb091a", "Field Level");
			zPhaseDependantsFindBoxColumnStyleInfo1.ColumnName = "DependantsText";
			zPhaseDependantsFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.RulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zPhaseDependantsFindBoxColumnStyleInfo1);
			this.RulesGrid.GridId = "3a031f8f-e5fd-4cfe-8607-8a7090498a31";
			this.RulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RulesGrid.LayoutKey = "RulesGrid";
			this.RulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 210, true);
			this.RulesGrid.Name = "RulesGrid";
			this.RulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 187, true);
			this.RulesGrid.TabIndex = 2;
			// 
			// PhaseSecurityRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RulesGrid);
			this.Controls.Add(this.PhasesGrid);
			this.Controls.Add(this.IsEnabledCheckBox);
			this.Name = "PhaseSecurityRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PhasesGrid)).EndInit();
			this.PhasesGrid.ResumeLayout(false);
			this.PhasesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).EndInit();
			this.RulesGrid.ResumeLayout(false);
			this.RulesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox IsEnabledCheckBox;
		internal ZArchitecture.ZGrid PhasesGrid;
		internal ZArchitecture.ZGrid RulesGrid;
	}
}
