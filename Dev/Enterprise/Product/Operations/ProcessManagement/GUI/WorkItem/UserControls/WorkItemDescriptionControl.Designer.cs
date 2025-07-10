using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkItemDescriptionControl : ZUserControl
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
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailNoteRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.SummaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			DescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("5dae738d-047c-4d12-9d34-d784b97c91cf", "Description");
			this.DescriptionGroupBox.Controls.Add(this.DetailNoteRichTextBox);
			this.DescriptionGroupBox.Controls.Add(this.SummaryTextBox);
			this.DescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 411, true);
			this.DescriptionGroupBox.TabIndex = 0;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// DetailNoteRichTextBox
			// 
			this.DetailNoteRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DetailNoteRichTextBox, "WKI_Details");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_Details)));
			this.DetailNoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.DetailNoteRichTextBox.MaxLength = 10000000;
			this.DetailNoteRichTextBox.Name = "DetailNoteRichTextBox";
			this.DetailNoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 360, true);
			this.DetailNoteRichTextBox.TabIndex = 1;
			// 
			// SummaryTextBox
			// 
			this.SummaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SummaryTextBox, "WKI_Summary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_Summary)));
			this.SummaryTextBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("5265f606-13ba-4691-a398-7d4c8c81ca5c", "Summary");
			this.SummaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 19, true);
			this.SummaryTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 20, true);
			this.SummaryTextBox.Name = "SummaryTextBox";
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 17, true);
			this.SummaryTextBox.TabIndex = 0;
			// 
			// WorkItemDescriptionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionGroupBox);
			this.Name = "WorkItemDescriptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 411, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			DescriptionGroupBox.ResumeLayout(false);
			DescriptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private ZArchitecture.ZTextBox SummaryTextBox;
		protected ZRichTextBox DetailNoteRichTextBox;
	}
}
