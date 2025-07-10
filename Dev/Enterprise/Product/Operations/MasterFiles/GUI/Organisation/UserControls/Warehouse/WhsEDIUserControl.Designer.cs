using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WhsEDIUserControl
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
			this.EDIRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnforceUniqueRefStrategyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EDIRulesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// EDIRulesGroupBox
			// 
			this.EDIRulesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsEDIUserControl|50e79e72-9fe2-4151-b462-3ce1e0b43c37", "EDI Rules");
			this.EDIRulesGroupBox.Controls.Add(this.zCheckBox1);
			this.EDIRulesGroupBox.Controls.Add(this.EnforceUniqueRefStrategyDropEdit);
			this.EDIRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.EDIRulesGroupBox.Name = "EDIRulesGroupBox";
			this.EDIRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 87, true);
			this.EDIRulesGroupBox.TabIndex = 6;
			this.EDIRulesGroupBox.TabStop = false;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "MiscServ.OM_WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage)));
			this.zCheckBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WhsEDIUserControl|e1481ea8-a0cf-4035-bf37-e9518d0f4d13", "Cancel Order Lines not included in EDI Change Messages");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 56, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.zCheckBox1.TabIndex = 8;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// EnforceUniqueRefStrategyDropEdit
			// 
			this.BindingSource.SetBindingMember(this.EnforceUniqueRefStrategyDropEdit, "MiscServ+OM_WhsOrderNumberUniquenessStrategy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_WhsOrderNumberUniquenessStrategy)));
			this.EnforceUniqueRefStrategyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 19, true);
			this.EnforceUniqueRefStrategyDropEdit.Name = "EnforceUniqueRefStrategyDropEdit";
			this.EnforceUniqueRefStrategyDropEdit.PreBoundMaxLength = 3;
			this.EnforceUniqueRefStrategyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.EnforceUniqueRefStrategyDropEdit.TabIndex = 7;
			// 
			// WhsEDIUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EDIRulesGroupBox);
			this.Name = "WhsEDIUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 108, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EDIRulesGroupBox.ResumeLayout(false);
			this.EDIRulesGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox EDIRulesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EnforceUniqueRefStrategyDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
	}
}
