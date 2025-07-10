namespace Enterprise.Customs.US.GUI
{
	partial class CPSCDisclaimPivotControl
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
			this.CPSCDisclaimGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CPSCIntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPSCIntendedUseDescTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CPSCDisclaimGroupBox.SuspendLayout();
			this.CPSCIntendedUseCodeDropEdit.SuspendLayout();
			this.CPSCIntendedUseDescTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CPSCHeader);
			// 
			// CPSCDisclaimGroupBox
			// 
			this.CPSCDisclaimGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("AB8088FD-FD1A-4DD7-AD06-4694DDA2C223", "CPSC Disclaim");
			this.CPSCDisclaimGroupBox.Controls.Add(this.CPSCIntendedUseCodeDropEdit);
			this.CPSCDisclaimGroupBox.Controls.Add(this.CPSCIntendedUseDescTextBox);
			this.CPSCDisclaimGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CPSCDisclaimGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CPSCDisclaimGroupBox.Name = "CPSCDisclaimGroupBox";
			this.CPSCDisclaimGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.CPSCDisclaimGroupBox.TabIndex = 0;
			this.CPSCDisclaimGroupBox.TabStop = false;
			this.CPSCDisclaimGroupBox.Text = "CPSC Disclaim";
			// 
			// CPSCIntendedUseCodeDropEdit
			// 
			this.CPSCIntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPSCIntendedUseCodeDropEdit, "US_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CPSCHeader)(null)).US_IntendedUseCode)));
			this.CPSCIntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0F06CA9D-56C3-4B2E-8E9F-B4F682712ABA", "Intended Use Code");
			this.CPSCIntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
			this.CPSCIntendedUseCodeDropEdit.Name = "CPSCIntendedUseCodeDropEdit";
			this.CPSCIntendedUseCodeDropEdit.PreBoundMaxLength = 2;
			this.CPSCIntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CPSCIntendedUseCodeDropEdit.TabIndex = 4;
			// 
			// CPSCIntendedUseDescTextBox
			// 
			this.CPSCIntendedUseDescTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CPSCIntendedUseDescTextBox, "US_IntendedUseDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CPSCHeader)(null)).US_IntendedUseDescription)));
			this.CPSCIntendedUseDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5A5B4592-0E3B-4398-A20F-695A6B390095", "Intended Use Desc.");
			this.CPSCIntendedUseDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 45, true);
			this.CPSCIntendedUseDescTextBox.Name = "US_IntendedUseDescriptionTextBox";
			this.CPSCIntendedUseDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CPSCIntendedUseDescTextBox.TabIndex = 6;
			// 
			// CPSCDisclaimPivotControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CPSCDisclaimGroupBox);
			this.Name = "CPSCDisclaimPivotControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CPSCDisclaimGroupBox.ResumeLayout(false);
			this.CPSCDisclaimGroupBox.PerformLayout();
			this.CPSCIntendedUseCodeDropEdit.ResumeLayout(true);
			this.CPSCIntendedUseCodeDropEdit.PerformLayout();
			this.CPSCIntendedUseDescTextBox.ResumeLayout(true);
			this.CPSCIntendedUseDescTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CPSCDisclaimGroupBox;
		private ZArchitecture.GUI.ZDropEdit CPSCIntendedUseCodeDropEdit;
		private ZArchitecture.ZTextBox CPSCIntendedUseDescTextBox;
	}
}
