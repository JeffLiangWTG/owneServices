namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class CustomsMessagesTabUserControl
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).BeginInit();
			this.BottomVerticalSplitContainer.Panel1.SuspendLayout();
			this.BottomVerticalSplitContainer.Panel2.SuspendLayout();
			this.BottomVerticalSplitContainer.SuspendLayout();
			this.MessageTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 551, true);
			// 
			// BottomVerticalSplitContainer
			// 
			this.BottomVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 551, true);
			this.BottomVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(536);
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 551, true);
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 524, true);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsMessageUserControl|937651ac-1ab7-4edf-9332-f53194c963f4", "English Interpretation of EDIFACT Message");
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 518, true);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsMessageUserControl|976cbbe8-98f2-4532-b0c7-9b8e76be7dd6", "Message Contents");
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			// 
			// CustomsMessagesTabUserControl
			// 
			this.Name = "CustomsMessagesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 551, true);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.BottomVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.BottomVerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).EndInit();
			this.BottomVerticalSplitContainer.ResumeLayout(false);
			this.BottomVerticalSplitContainer.PerformLayout();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageTabControl.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
