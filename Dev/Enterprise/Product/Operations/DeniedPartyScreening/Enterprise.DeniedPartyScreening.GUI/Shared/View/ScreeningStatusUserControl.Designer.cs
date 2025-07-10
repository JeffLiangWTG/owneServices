namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class ScreeningStatusUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DpsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.MatchDecisionDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.ClearingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearingReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DropEditAndTextBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DropDownPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinkPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MatchDecisionDropEdit.SuspendLayout();
			this.ClearingReasonDropEdit.SuspendLayout();
			this.DropEditAndTextBoxPanel.SuspendLayout();
			this.DropDownPanel.SuspendLayout();
			this.LinkPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// DpsLinkLabel
			// 
			this.DpsLinkLabel.AutoSize = true;
			this.DpsLinkLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.DpsLinkLabel.IsFontBold = false;
			this.DpsLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(112)))), ((int)(((byte)(154)))));
			this.DpsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 0, true);
			this.DpsLinkLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 8, 0, true);
			this.DpsLinkLabel.Name = "DpsLinkLabel";
			this.DpsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.DpsLinkLabel.TabIndex = 3;
			this.DpsLinkLabel.TabStop = false;
			this.DpsLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DpsLinkLabel.Click += new System.EventHandler(this.DpsLinkLabel_Click);
			// 
			// MatchDecisionDropEdit
			// 
			this.MatchDecisionDropEdit.AllowDrop = true;
			this.MatchDecisionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchDecisionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 4, true);
			this.MatchDecisionDropEdit.Name = "MatchDecisionDropEdit";
			this.MatchDecisionDropEdit.PreBoundMaxLength = 3;
			this.MatchDecisionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 18, true);
			this.MatchDecisionDropEdit.TabIndex = 4;
			// 
			// ClearingReasonDropEdit
			// 
			this.ClearingReasonDropEdit.AllowDrop = true;
			this.ClearingReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 34, true);
			this.ClearingReasonDropEdit.Name = "ClearingReasonDropEdit";
			this.ClearingReasonDropEdit.PreBoundMaxLength = 3;
			this.ClearingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 18, true);
			this.ClearingReasonDropEdit.TabIndex = 5;
			// 
			// ClearingReasonTextBox
			// 
			this.ClearingReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClearingReasonTextBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ClearingReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 0, true);
			this.ClearingReasonTextBox.Multiline = true;
			this.ClearingReasonTextBox.Name = "ClearingReasonTextBox";
			this.ClearingReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ClearingReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 62, true);
			this.ClearingReasonTextBox.TabIndex = 6;
			this.ClearingReasonTextBox.TextChanged += new System.EventHandler(this.ClearingReasonTextBox_TextChanged);
			this.ClearingReasonTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ClearingReasonTextBox_KeyPress);
			// 
			// DropEditAndTextBoxPanel
			// 
			this.DropEditAndTextBoxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DropEditAndTextBoxPanel.AutoSize = true;
			this.DropEditAndTextBoxPanel.Controls.Add(this.DropDownPanel);
			this.DropEditAndTextBoxPanel.Controls.Add(this.ClearingReasonTextBox);
			this.DropEditAndTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 26, true);
			this.DropEditAndTextBoxPanel.Name = "DropEditAndTextBoxPanel";
			this.DropEditAndTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 62, true);
			this.DropEditAndTextBoxPanel.TabIndex = 7;
			// 
			// DropDownPanel
			// 
			this.DropDownPanel.Controls.Add(this.MatchDecisionDropEdit);
			this.DropDownPanel.Controls.Add(this.ClearingReasonDropEdit);
			this.DropDownPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.DropDownPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 0, true);
			this.DropDownPanel.Name = "DropDownPanel";
			this.DropDownPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 62, true);
			this.DropDownPanel.TabIndex = 0;
			// 
			// LinkPanel
			// 
			this.LinkPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LinkPanel.Controls.Add(this.DpsLinkLabel);
			this.LinkPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 0, true);
			this.LinkPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.LinkPanel.Name = "LinkPanel";
			this.LinkPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 22, true);
			this.LinkPanel.TabIndex = 8;
			// 
			// ScreeningStatusUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LinkPanel);
			this.Controls.Add(this.DropEditAndTextBoxPanel);
			this.Name = "ScreeningStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 90, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MatchDecisionDropEdit.ResumeLayout(true);
			this.MatchDecisionDropEdit.PerformLayout();
			this.ClearingReasonDropEdit.ResumeLayout(true);
			this.ClearingReasonDropEdit.PerformLayout();
			this.DropEditAndTextBoxPanel.ResumeLayout(false);
			this.DropEditAndTextBoxPanel.PerformLayout();
			this.DropDownPanel.ResumeLayout(false);
			this.DropDownPanel.PerformLayout();
			this.LinkPanel.ResumeLayout(false);
			this.LinkPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZLinkLabel DpsLinkLabel;
		private DeniedPartyScreeningStatusDropEdit MatchDecisionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ClearingReasonDropEdit;
		private ZArchitecture.ZTextBox ClearingReasonTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel DropEditAndTextBoxPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DropDownPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel LinkPanel;
	}
}
