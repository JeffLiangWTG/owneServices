namespace Enterprise.Customs.ZA.GUI
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		///

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
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
			zTextBoxColumnStyleInfo1.Caption = "VOC Reason / Reference";
			zTextBoxColumnStyleInfo1.ColumnName = "VOCReason";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.Caption = "Entry Status";
			zTextBoxColumnStyleInfo2.ColumnName = "EntryStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Entry Status Desc";
			zTextBoxColumnStyleInfo3.ColumnName = "EntryStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("F088F193-7575-4133-A8C4-CC4133EA5DC3", "Parent Msg. No.", "Parent Message No.", "Parent Message Number");
			zTextBoxColumnStyleInfo4.ColumnName = "ParentMessageNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Held Until Date";
			zDateEditColumnStyleInfo1.ColumnName = "EM_HeldUntilDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			// 
			// BottomVerticalSplitContainer
			// 
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextBox, 0);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.HtmlInterpretationBox, 0);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Visible = false;
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.HtmlInterpretationBox.TabIndex = 1;
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

		private Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlInterpretationBox;
	}
}
