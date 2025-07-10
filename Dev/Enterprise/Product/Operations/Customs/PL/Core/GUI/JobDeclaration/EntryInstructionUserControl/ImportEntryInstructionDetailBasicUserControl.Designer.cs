namespace Enterprise.Customs.PL.GUI
{
	partial class ImportEntryInstructionDetailBasicUserControl
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
			this.GuaranteesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GuaranteesUserControl = new Enterprise.Customs.PL.GUI.EntryInstructionDetailGuaranteesUserControl();
			this.DetailsLayoutControl = new Enterprise.Customs.PL.GUI.EntryInstructionDetailsLayoutControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuaranteesGroupBox.SuspendLayout();
			this.GuaranteesUserControl.SuspendLayout();
			this.DetailsLayoutControl.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.Controls.Add(this.GuaranteesGroupBox);
			// 
			// GuaranteesGroupBox
			// 
			this.GuaranteesGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.GuaranteesGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("98232f09-7be2-4398-a312-1f345b905482", "Guarantees");
			this.GuaranteesGroupBox.Controls.Add(this.GuaranteesUserControl);
			this.GuaranteesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 0, true);
			this.GuaranteesGroupBox.Name = "GuaranteesGroupBox";
			this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 148, true);
			this.GuaranteesGroupBox.TabIndex = 2;
			this.GuaranteesGroupBox.TabStop = false;
			// 
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteesUserControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((Enterprise.Customs.PL.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.GuaranteesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.GuaranteesUserControl.Name = "GuaranteesUserControl";
			this.GuaranteesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 163, true);
			this.GuaranteesUserControl.TabIndex = 0;
			//
			this.DetailsPanel.Controls.Add(this.DetailsLayoutControl);
			// 
			// EntryInstructionDetailImportBasicUserControl
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
			// ImportEntryInstructionDetailBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ImportEntryInstructionDetailBasicUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuaranteesGroupBox.ResumeLayout(false);
			this.GuaranteesGroupBox.PerformLayout();
			this.GuaranteesUserControl.ResumeLayout(true);
			this.GuaranteesUserControl.PerformLayout();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.DetailsLayoutControl.ResumeLayout(true);
			this.DetailsLayoutControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private EntryInstructionDetailsLayoutControl DetailsLayoutControl;
		internal ZArchitecture.GUI.ZGroupBox GuaranteesGroupBox;
		private EntryInstructionDetailGuaranteesUserControl GuaranteesUserControl;
	}
}
