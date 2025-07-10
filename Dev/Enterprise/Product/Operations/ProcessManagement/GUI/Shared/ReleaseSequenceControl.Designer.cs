using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class ReleaseSequenceControl
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
			this.ReleaseSequenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseSequenceRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.ReleaseSequenceDateBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseSequenceInvestmentBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseSequenceValueBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseSequenceNameBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseSequencePositionBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OpenSequenceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReleaseSequenceGroupBox.SuspendLayout();
			this.ReleaseSequenceRowPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = BindingDataSourceType;
			// 
			// ReleaseSequenceGroupBox
			// 
			this.ReleaseSequenceGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("4c124040-96ca-4fe4-ad7a-6c44badc19a3", "Release Sequence");
			this.ReleaseSequenceGroupBox.Controls.Add(this.ReleaseSequenceRowPanel);
			this.ReleaseSequenceGroupBox.Controls.Add(this.OpenSequenceButton);
			this.ReleaseSequenceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReleaseSequenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReleaseSequenceGroupBox.Name = "ReleaseSequenceGroupBox";
			this.ReleaseSequenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 199, true);
			this.ReleaseSequenceGroupBox.TabIndex = 0;
			this.ReleaseSequenceGroupBox.TabStop = false;
			this.ReleaseSequenceGroupBox.Text = "Release Sequence";
			// 
			// ReleaseSequenceRowPanel
			// 
			this.ReleaseSequenceRowPanel.Alignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.ReleaseSequenceRowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseSequenceRowPanel.Controls.Add(this.ReleaseSequenceDateBox);
			this.ReleaseSequenceRowPanel.Controls.Add(this.ReleaseSequenceInvestmentBox);
			this.ReleaseSequenceRowPanel.Controls.Add(this.ReleaseSequenceValueBox);
			this.ReleaseSequenceRowPanel.Controls.Add(this.ReleaseSequenceNameBox);
			this.ReleaseSequenceRowPanel.Controls.Add(this.ReleaseSequencePositionBox);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.ReleaseSequenceRowPanel, true);
			this.ReleaseSequenceRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 16, true);
			this.ReleaseSequenceRowPanel.Name = "ReleaseSequenceRowPanel";
			this.ReleaseSequenceRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.ReleaseSequenceRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 126, true);
			this.ReleaseSequenceRowPanel.TabIndex = 9;
			// 
			// ReleaseSequenceDateBox
			// 
			this.ReleaseSequenceDateBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseSequenceDateBox, "ReleaseSequenceDateAsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).ReleaseSequenceDateAsText)));
			this.ReleaseSequenceDateBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ReleaseSequenceDateBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("84facb31-a49b-4897-9b6c-0e59c1f526ea", "Due Date");
			this.ReleaseSequenceDateBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseSequenceDateBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReleaseSequenceDateBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseSequenceDateBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.ReleaseSequenceDateBox.Name = "ReleaseSequenceDateBox";
			this.ReleaseSequenceDateBox.ReadOnly = true;
			this.ReleaseSequenceRowPanel.SetRow(this.ReleaseSequenceDateBox, 4);
			this.ReleaseSequenceDateBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReleaseSequenceDateBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.ReleaseSequenceDateBox.TabIndex = 5;
			// 
			// ReleaseSequenceInvestmentBox
			// 
			this.ReleaseSequenceInvestmentBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseSequenceInvestmentBox, "ReleaseSequenceInvestment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).ReleaseSequenceInvestment)));
			this.ReleaseSequenceInvestmentBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ReleaseSequenceInvestmentBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("58487bd3-0cfd-42d7-afbd-239bb26fd0ad", "Investment");
			this.ReleaseSequenceInvestmentBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseSequenceInvestmentBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReleaseSequenceInvestmentBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseSequenceInvestmentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.ReleaseSequenceInvestmentBox.Name = "ReleaseSequenceInvestmentBox";
			this.ReleaseSequenceInvestmentBox.ReadOnly = true;
			this.ReleaseSequenceRowPanel.SetRow(this.ReleaseSequenceInvestmentBox, 3);
			this.ReleaseSequenceInvestmentBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReleaseSequenceInvestmentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.ReleaseSequenceInvestmentBox.TabIndex = 4;
			// 
			// ReleaseSequenceValueBox
			// 
			this.ReleaseSequenceValueBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseSequenceValueBox, "ReleaseSequenceValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).ReleaseSequenceValue)));
			this.ReleaseSequenceValueBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ReleaseSequenceValueBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("1174ff61-c0c9-4fa7-aacf-950af0bf0070", "Value");
			this.ReleaseSequenceValueBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseSequenceValueBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReleaseSequenceValueBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseSequenceValueBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 55, true);
			this.ReleaseSequenceValueBox.Name = "ReleaseSequenceValueBox";
			this.ReleaseSequenceValueBox.ReadOnly = true;
			this.ReleaseSequenceRowPanel.SetRow(this.ReleaseSequenceValueBox, 2);
			this.ReleaseSequenceValueBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReleaseSequenceValueBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.ReleaseSequenceValueBox.TabIndex = 3;
			// 
			// ReleaseSequenceNameBox
			// 
			this.ReleaseSequenceNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseSequenceNameBox, "ReleaseSequenceName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).ReleaseSequenceName)));
			this.ReleaseSequenceNameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ReleaseSequenceNameBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("d61c6d92-338c-47f1-976e-4e107a4f4349", "Sequence Name");
			this.ReleaseSequenceNameBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseSequenceNameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReleaseSequenceNameBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseSequenceNameBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.ReleaseSequenceNameBox.Name = "ReleaseSequenceNameBox";
			this.ReleaseSequenceNameBox.ReadOnly = true;
			this.ReleaseSequenceRowPanel.SetRow(this.ReleaseSequenceNameBox, 0);
			this.ReleaseSequenceNameBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReleaseSequenceNameBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.ReleaseSequenceNameBox.TabIndex = 1;
			// 
			// ReleaseSequencePositionBox
			// 
			this.ReleaseSequencePositionBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseSequencePositionBox, "ReleaseSequencePosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).ReleaseSequencePosition)));
			this.ReleaseSequencePositionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ReleaseSequencePositionBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("cbe80db6-4b34-40ac-a362-36ab1dbcbb0e", "Position");
			this.ReleaseSequencePositionBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseSequencePositionBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReleaseSequencePositionBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseSequencePositionBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.ReleaseSequencePositionBox.Name = "ReleaseSequencePositionBox";
			this.ReleaseSequencePositionBox.ReadOnly = true;
			this.ReleaseSequenceRowPanel.SetRow(this.ReleaseSequencePositionBox, 1);
			this.ReleaseSequencePositionBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReleaseSequencePositionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.ReleaseSequencePositionBox.TabIndex = 2;
			// 
			// OpenSequenceButton
			// 
			this.OpenSequenceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OpenSequenceButton.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("4d10979d-63ac-4295-9388-c78afff8246c", "Open Sequence");
			this.OpenSequenceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OpenSequenceButton.IsCaptionOverridden = false;
			this.OpenSequenceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 146, true);
			this.OpenSequenceButton.Name = "OpenSequenceButton";
			this.OpenSequenceButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OpenSequenceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 24, true);
			this.OpenSequenceButton.TabIndex = 12;
			this.OpenSequenceButton.TabStop = false;
			this.OpenSequenceButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OpenSequenceButton.ToolTipCaption = null;
			this.OpenSequenceButton.UseVisualStyleBackColor = false;
			this.OpenSequenceButton.Click += OpenSequenceButton_Click;
			// 
			// WorkItemReleaseSequenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleaseSequenceGroupBox);
			this.Name = "WorkItemReleaseSequenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 199, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReleaseSequenceGroupBox.ResumeLayout(false);
			this.ReleaseSequenceGroupBox.PerformLayout();
			this.ReleaseSequenceRowPanel.ResumeLayout(false);
			this.ReleaseSequenceRowPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZArchitecture.GUI.ZGroupBox ReleaseSequenceGroupBox;
		private ZArchitecture.ZTextBox ReleaseSequencePositionBox;
		public CargoWise.Windows.UI.Layout.RowLayoutPanel ReleaseSequenceRowPanel;
		private ZArchitecture.ZTextBox ReleaseSequenceNameBox;
		private ZArchitecture.ZTextBox ReleaseSequenceDateBox;
		private ZArchitecture.ZTextBox ReleaseSequenceInvestmentBox;
		private ZArchitecture.ZTextBox ReleaseSequenceValueBox;
		internal ZArchitecture.GUI.ZButton OpenSequenceButton;
	}
}
