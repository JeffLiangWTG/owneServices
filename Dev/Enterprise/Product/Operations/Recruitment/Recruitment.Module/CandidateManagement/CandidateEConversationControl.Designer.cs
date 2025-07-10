namespace Enterprise.EConversation.GUI
{
	partial class CandidateEConversationControl
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
			this.components = new System.ComponentModel.Container();
			this.MessagePanelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddInternalCommentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.econversationMessageTextBox = new Enterprise.ZArchitecture.GUI.ZAutoCompleteTextBox();
			this.chatboxControl = new Enterprise.Recruitment.Module.CandidateEConversationMessageListUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagePanelSplitContainer)).BeginInit();
			this.MessagePanelSplitContainer.Panel1.SuspendLayout();
			this.MessagePanelSplitContainer.Panel2.SuspendLayout();
			this.MessagePanelSplitContainer.SuspendLayout();
			this.chatboxControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.GroupedEConversation);
			// 
			// MessagePanelSplitContainer
			// 
			this.MessagePanelSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagePanelSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MessagePanelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagePanelSplitContainer.Name = "MessagePanelSplitContainer";
			this.MessagePanelSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagePanelSplitContainer.Panel1
			// 
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.sendButton);
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.AddInternalCommentButton);
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.econversationMessageTextBox);
			this.MessagePanelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 340, true);
			this.MessagePanelSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// MessagePanelSplitContainer.Panel2
			// 
			this.MessagePanelSplitContainer.Panel2.Controls.Add(this.chatboxControl);
			this.MessagePanelSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.MessagePanelSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.MessagePanelSplitContainer.TabIndex = 5;

			// 
			// sendButton
			// 
			this.sendButton.Enabled = false;
			this.sendButton.IsCaptionOverridden = false;
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.sendButton.TabIndex = 0;
			this.sendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.sendButton.ToolTipCaption = null;
			this.sendButton.Visible = false;
			// 
			// AddInternalCommentButton
			// 
			this.AddInternalCommentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddInternalCommentButton.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("bb61e6a3-2a6b-4aaf-b392-9c58977acaa3", "Add Note");
			this.AddInternalCommentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddInternalCommentButton.IsCaptionOverridden = false;
			this.AddInternalCommentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 3, true);
			this.AddInternalCommentButton.Name = "AddInternalCommentButton";
			this.AddInternalCommentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddInternalCommentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 42, true);
			this.AddInternalCommentButton.TabIndex = 3;
			this.AddInternalCommentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AddInternalCommentButton.ToolTipCaption = null;
			this.AddInternalCommentButton.UseVisualStyleBackColor = true;
			// 
			// econversationMessageTextBox
			// 
			this.econversationMessageTextBox.AcceptsTab = true;
			this.econversationMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.econversationMessageTextBox.AutocompleteManager = null;
			this.econversationMessageTextBox.EnableValidStateColor = false;
			this.econversationMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.econversationMessageTextBox.Name = "econversationMessageTextBox";
			this.econversationMessageTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.econversationMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 95, true);
			this.econversationMessageTextBox.TabIndex = 1;
			this.econversationMessageTextBox.Text = "";
			// 
			// chatboxControl
			// 
			this.chatboxControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chatboxControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.EConversation.Business.IConversation)(((Enterprise.Recruiter.Business.GroupedEConversation)(null)))));
			this.chatboxControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chatboxControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.chatboxControl.Name = "chatboxControl";
			this.chatboxControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 237, true);
			this.chatboxControl.TabIndex = 4;
			// 
			// CandidateEConversationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagePanelSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 340, true);
			this.Name = "CandidateEConversationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 340, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagePanelSplitContainer.Panel1.ResumeLayout(false);
			this.MessagePanelSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagePanelSplitContainer)).EndInit();
			this.MessagePanelSplitContainer.ResumeLayout(false);
			this.MessagePanelSplitContainer.PerformLayout();
			this.chatboxControl.ResumeLayout(true);
			this.chatboxControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public Enterprise.Recruitment.Module.CandidateEConversationMessageListUserControl chatboxControl;
		private ZArchitecture.GUI.ZButton AddInternalCommentButton;
		private ZArchitecture.GUI.ZButton sendButton;
		private ZArchitecture.GUI.ZAutoCompleteTextBox econversationMessageTextBox;
		private CargoWise.Windows.UI.KSplitContainer MessagePanelSplitContainer;
	}
}
