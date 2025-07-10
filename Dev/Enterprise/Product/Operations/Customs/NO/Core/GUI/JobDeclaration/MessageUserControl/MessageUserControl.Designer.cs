namespace Enterprise.Customs.NO.GUI
{
	partial class MessageUserControl
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
			this.DutiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryDutiesUserControl = new Enterprise.Customs.NO.GUI.EntryDutiesUserControl();
			this.EntryLineDutiesUserControl = new Enterprise.Customs.NO.GUI.EntryLineDutiesUserControl();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DutiesTabPage.SuspendLayout();
			this.EntryDutiesUserControl.SuspendLayout();
			this.EntryLineDutiesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainHorizontalSplitContainer
			// 
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.DutiesTabPage);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.DutiesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Controls.Add(this.EntryLineDutiesUserControl);
			this.EntryLinesTabPage.Controls.SetChildIndex(this.ExtendedInfoGroupBox, 0);
			this.EntryLinesTabPage.Controls.SetChildIndex(this.EntryLineDutiesUserControl, 0);
			this.EntryLinesTabPage.Controls.SetChildIndex(this.EntryLineGrid, 0);
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntriesBoundGrid.ReadOnly = false;
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
			// 
			// TopVerticalSplitContainer
			// 
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
			// 
			// DutiesTabPage
			// 
			this.DutiesTabPage.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("f2cef805-dc65-4431-8b88-fef39db57764", "Sum Duties and VAT");
			this.DutiesTabPage.Controls.Add(this.EntryDutiesUserControl);
			this.DutiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DutiesTabPage.Name = "DutiesTabPage";
			this.DutiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DutiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.DutiesTabPage.TabIndex = 2;
			this.DutiesTabPage.UseVisualStyleBackColor = true;
			// 
			// EntryDutiesUserControl
			// 
			this.EntryDutiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryDutiesUserControl, ".");
			this.EntryDutiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryDutiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryDutiesUserControl.Name = "EntryDutiesUserControl";
			this.EntryDutiesUserControl.CaptionRenderingEnabled = true;
			this.EntryDutiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
			this.EntryDutiesUserControl.TabIndex = 1;
			// 
			// EntryLineDutiesUserControl
			// 
			this.EntryLineDutiesUserControl.AllowDrop = true;
			this.EntryLineDutiesUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntryLineDutiesUserControl, ".");
			this.EntryLineDutiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 336, true);
			this.EntryLineDutiesUserControl.Name = "EntryLineDutiesUserControl";
			this.EntryLineDutiesUserControl.CaptionRenderingEnabled = true;
			this.EntryLineDutiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 225, true);
			this.EntryLineDutiesUserControl.TabIndex = 2;
			// 
			// MessageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MessageUserControl";
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DutiesTabPage.ResumeLayout(false);
			this.DutiesTabPage.PerformLayout();
			this.EntryDutiesUserControl.ResumeLayout(true);
			this.EntryDutiesUserControl.PerformLayout();
			this.EntryLineDutiesUserControl.ResumeLayout(true);
			this.EntryLineDutiesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage DutiesTabPage;
		protected EntryDutiesUserControl EntryDutiesUserControl;
		protected EntryLineDutiesUserControl EntryLineDutiesUserControl;
	}
}
