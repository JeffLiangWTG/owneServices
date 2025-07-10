namespace Enterprise.Customs.US.GUI
{
	partial class AMSDisclaimControl
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
			this.ProgramDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AMSDisclaimGroupBox.SuspendLayout();
			this.ProgramDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// AMSDisclaimGroupBox
			// 
			this.AMSDisclaimGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("16d33282-17d8-4bb8-bd67-09654b658f0d", "AMS Disclaim");
			this.AMSDisclaimGroupBox.Controls.Add(this.ProgramDropEdit);
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
			this.ProgramDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProgramDropEdit, "US_AMSDisclaimProgram");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_AMSDisclaimProgram)));
			this.ProgramDropEdit.BindToList = null;
			this.ProgramDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("715a3934-b4f4-47e9-b707-d0caa1ab9b09", "Program Message Code");
			this.ProgramDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 19, true);
			this.ProgramDropEdit.Name = "ProgramDropEdit";
			this.ProgramDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.ProgramDropEdit.TabIndex = 0;
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
			this.ProgramDropEdit.ResumeLayout(true);
			this.ProgramDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AMSDisclaimGroupBox;
		private ZArchitecture.GUI.ZDropEdit ProgramDropEdit;
	}
}
