namespace Enterprise.Customs.US.GUI
{
	partial class PSTDisclaimPivotControl
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
			this.PSTDisclaimGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PSTProgramDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PSTDisclaimGroupBox.SuspendLayout();
			this.PSTProgramDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			// 
			// PSTDisclaimGroupBox
			// 
			this.PSTDisclaimGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1B930EA5-9629-43FF-8184-CE344662514B", "PST Disclaim");
			this.PSTDisclaimGroupBox.Controls.Add(this.PSTProgramDropEdit);
			this.PSTDisclaimGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PSTDisclaimGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PSTDisclaimGroupBox.Name = "PSTDisclaimGroupBox";
			this.PSTDisclaimGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.PSTDisclaimGroupBox.TabIndex = 0;
			this.PSTDisclaimGroupBox.TabStop = false;
			this.PSTDisclaimGroupBox.Text = "PST Disclaim";
			// 
			// ProgramDropEdit
			// 
			this.PSTProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PSTProgramDropEdit, "CD_PSTDisclaimProgram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_PSTDisclaimProgram)));
			this.PSTProgramDropEdit.BindToList = null;
			this.PSTProgramDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("93929DD6-AE92-4C39-8974-201F11B8AADF", "Program Message Code");
			this.PSTProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 19, true);
			this.PSTProgramDropEdit.Name = "PSTProgramDropEdit";
			this.PSTProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.PSTProgramDropEdit.TabIndex = 0;
			// 
			// PSTDisclaimControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PSTDisclaimGroupBox);
			this.Name = "PSTDisclaimControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PSTDisclaimGroupBox.ResumeLayout(false);
			this.PSTDisclaimGroupBox.PerformLayout();
			this.PSTProgramDropEdit.ResumeLayout(true);
			this.PSTProgramDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PSTDisclaimGroupBox;
		private ZArchitecture.GUI.ZDropEdit PSTProgramDropEdit;
	}
}
