namespace Enterprise.Customs.US.GUI
{
	partial class AMSDisclaimPivotControl
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
			this.AMSDisclaimGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AMSProgramDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AMSDisclaimGroupBox.SuspendLayout();
			this.AMSProgramDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CusClassPartPivot);
			// 
			// AMSDisclaimGroupBox
			// 
			this.AMSDisclaimGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("75541429-D2B4-4FCC-A485-47C8DA3DA4CA", "AMS Disclaim");
			this.AMSDisclaimGroupBox.Controls.Add(this.AMSProgramDropEdit);
			this.AMSDisclaimGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AMSDisclaimGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AMSDisclaimGroupBox.Name = "AMSDisclaimGroupBox";
			this.AMSDisclaimGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.AMSDisclaimGroupBox.TabIndex = 0;
			this.AMSDisclaimGroupBox.TabStop = false;
			this.AMSDisclaimGroupBox.Text = "AMS Disclaim";
			// 
			// ProgramDropEdit
			// 
			this.AMSProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AMSProgramDropEdit, "CD_AMSDisclaimProgram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CusClassPartPivot)(null)).CD_AMSDisclaimProgram)));
			this.AMSProgramDropEdit.BindToList = null;
			this.AMSProgramDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("BDC68EEA-0FDA-4EB6-9E7C-288720057CE3", "Program Message Code");
			this.AMSProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 19, true);
			this.AMSProgramDropEdit.Name = "AMSProgramDropEdit";
			this.AMSProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.AMSProgramDropEdit.TabIndex = 0;
			// 
			// AMSDisclaimControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AMSDisclaimGroupBox);
			this.Name = "AMSDisclaimControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AMSDisclaimGroupBox.ResumeLayout(false);
			this.AMSDisclaimGroupBox.PerformLayout();
			this.AMSProgramDropEdit.ResumeLayout(true);
			this.AMSProgramDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AMSDisclaimGroupBox;
		private ZArchitecture.GUI.ZDropEdit AMSProgramDropEdit;
	}
}
