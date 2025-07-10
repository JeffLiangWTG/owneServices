
namespace Enterprise.Customs.NL.GUI
{
	partial class CustomsOfficesUserControl
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
            this.CustomsOfficeDropBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.OfficesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
            this.CustomsOfficesGrid.SuspendLayout();
            this.CustomsOfficeFindBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsOfficeDropBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // OfficesGroupBox
            // 
            this.OfficesGroupBox.Controls.Add(this.CustomsOfficeDropBox);
            this.OfficesGroupBox.Controls.SetChildIndex(this.CustomsOfficesGrid, 0);
            this.OfficesGroupBox.Controls.SetChildIndex(this.CustomsOfficeFindBox, 0);
            this.OfficesGroupBox.Controls.SetChildIndex(this.CustomsOfficeDropBox, 0);
            // 
            // CustomsOfficeDropBox
            // 
            this.CustomsOfficeDropBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeDropBox, "JE_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_CustomsOffice)));
            this.CustomsOfficeDropBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("dd8748f4-3378-4162-91a1-1482fd2d5a58", "Customs Office");
            this.CustomsOfficeDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 19, true);
            this.CustomsOfficeDropBox.Name = "CustomsOfficeDropBox";
            this.CustomsOfficeDropBox.ShouldResizeByMaxLength = true;
            this.CustomsOfficeDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 20, true);
            this.CustomsOfficeDropBox.TabIndex = 1;
            // 
            // CustomsOfficesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "CustomsOfficesUserControl";
            this.OfficesGroupBox.ResumeLayout(false);
            this.OfficesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
            this.CustomsOfficesGrid.ResumeLayout(false);
            this.CustomsOfficesGrid.PerformLayout();
            this.CustomsOfficeFindBox.ResumeLayout(true);
            this.CustomsOfficeFindBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsOfficeDropBox.ResumeLayout(true);
            this.CustomsOfficeDropBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit CustomsOfficeDropBox;
	}
}
