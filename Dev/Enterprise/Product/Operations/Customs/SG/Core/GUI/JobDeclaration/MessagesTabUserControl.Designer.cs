using Enterprise.Messaging.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	partial class MessagesTabUserControl
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
			this.components = new System.ComponentModel.Container();
			this.InterpretedMessageHtmlBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
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
			// BottomVerticalSplitContainer
			// 
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			// 
			// InterpretedMessageHtmlBox
			// 
			this.BindingSource.SetBindingMember(this.InterpretedMessageHtmlBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.InterpretedMessageHtmlBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageHtmlBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InterpretedMessageHtmlBox.Name = "InterpretedMessageHtmlBox";
			this.InterpretedMessageHtmlBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 417, true);
			this.InterpretedMessageHtmlBox.TabIndex = 0;
			// 
			// MessagesTabUserControl
			// 
			this.Name = "MessagesTabUserControl";
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
		private HtmlInterpretationBox InterpretedMessageHtmlBox;

	}
}
