namespace Enterprise.Customs.PL.GUI
{
	partial class ExportEntryInstructionDetailBasicUserControl
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
			this.DetailsLayoutControl = new Enterprise.Customs.PL.GUI.EntryInstructionDetailsLayoutControl();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.DetailsLayoutControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Controls.Add(this.DetailsLayoutControl);
			// 
			// EntryInstructionDetailExportBasicUserControl
			// 
			this.DetailsLayoutControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsLayoutControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.DetailsLayoutControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsLayoutControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsLayoutControl.Name = "DetailsLayoutControl";
			this.DetailsLayoutControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 113, true);
			this.DetailsLayoutControl.TabIndex = 0;
			// 
			// ExportEntryInstructionDetailBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExportEntryInstructionDetailBasicUserControl";
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.FromWarehouseGroupBox.ResumeLayout(false);
			this.FromWarehouseGroupBox.PerformLayout();
			this.DetailsLayoutControl.ResumeLayout(true);
			this.DetailsLayoutControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private EntryInstructionDetailsLayoutControl DetailsLayoutControl;
	}
}
