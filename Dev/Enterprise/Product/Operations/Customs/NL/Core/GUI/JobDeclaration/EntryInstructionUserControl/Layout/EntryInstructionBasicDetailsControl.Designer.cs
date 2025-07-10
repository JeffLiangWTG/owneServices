using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	partial class EntryInstructionBasicDetailsControl
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
			this.TransNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsHighValueOvrdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransNatureDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction);
			// 
			// TransNatureDropEdit
			// 
			this.TransNatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransNatureDropEdit, "ZG_TransNature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction)(null)).ZG_TransNature)));
			this.TransNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 28, true);
			this.TransNatureDropEdit.Name = "TransNatureDropEdit";
			this.TransNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 15, true);
			this.TransNatureDropEdit.TabIndex = 23;
			this.TransNatureDropEdit.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("63BAEF61-652E-4EEC-BD64-7468EC109288", "Tran. Nature", "[UCC 8/5] Transaction Nature");
			// 
			// IsHighValueOvrdCheckBox
			// 
			this.IsHighValueOvrdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHighValueOvrdCheckBox, "ZG_IsHighValueOvrd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction)(null)).ZG_IsHighValueOvrd)));
			this.IsHighValueOvrdCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHighValueOvrdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 120, true);
			this.IsHighValueOvrdCheckBox.Name = "IsHighValueOvrdCheckBox";
			this.IsHighValueOvrdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.IsHighValueOvrdCheckBox.TabIndex = 1;
			this.IsHighValueOvrdCheckBox.UseVisualStyleBackColor = true;
			this.IsHighValueOvrdCheckBox.CheckAlign = ZContentAlignment.Right;
            // 
            // EntryInstructionBasicDetailsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransNatureDropEdit);
			this.Controls.Add(this.IsHighValueOvrdCheckBox);
			this.Name = "EntryInstructionBasicDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 95, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransNatureDropEdit.ResumeLayout(true);
			this.TransNatureDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit TransNatureDropEdit;
		internal ZArchitecture.GUI.ZCheckBox IsHighValueOvrdCheckBox;
	}
}
