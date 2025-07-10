
namespace Enterprise.Customs.US.GUI.Protest
{
	partial class MessagesUserControl
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
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusesErrorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesStatusErrorsUserControl = new Enterprise.Customs.US.GUI.MessagesStatusErrorsUserControl();
			this.StatusesErrorsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageDetailsTabPage.SuspendLayout();
			this.StatusesErrorsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessagesTabControl.Controls.Add(this.StatusesErrorsTabPage);
			this.MessagesTabControl.Controls.SetChildIndex(this.StatusesErrorsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageDetailsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.Protest.Protest);
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.CaptionResourceString = null;
			this.MessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.Text = "Message Details";
			// 
			// MessageDetailsTextBox
			// 
			this.MessageDetailsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageDetailsTextBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.Protest.Protest)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageDetailsTextBox.CaptionResourceString = null;
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.MessageDetailsTextBox.TabIndex = 1;
			this.MessageDetailsTextBox.WordWrap = false;
			// 
			// StatusesErrorsTabPage
			// 
			this.StatusesErrorsTabPage.CaptionResourceString = null;
			this.StatusesErrorsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MessagesUserControl|ba1802fb-a599-4050-9f22-130cc3d88e14", "Status/Errors");
			this.StatusesErrorsTabPage.Controls.Add(this.messagesStatusErrorsUserControl);
			this.StatusesErrorsTabPage.Controls.Add(this.StatusesErrorsLabel);
			this.StatusesErrorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusesErrorsTabPage.Name = "StatusesErrorsTabPage";
			this.StatusesErrorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.StatusesErrorsTabPage.TabIndex = 2;
			// 
			// messagesStatusErrorsUserControl
			// 
			this.messagesStatusErrorsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesStatusErrorsUserControl, "Messages.StatusesAndErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.StatusErrorsDataViewCollection)(((Enterprise.Customs.US.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.Protest.Protest)(null)).Messages)).SyncRoot)).StatusesAndErrors)));
			this.messagesStatusErrorsUserControl.CaptionResourceString = null;
			this.messagesStatusErrorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesStatusErrorsUserControl.Name = "messagesStatusErrorsUserControl";
			this.messagesStatusErrorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.messagesStatusErrorsUserControl.TabIndex = 20;
			// 
			// StatusesErrorsLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusesErrorsLabel, "Messages.StatusesErrorsExist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.Protest.Protest)(null)).Messages)).SyncRoot)).StatusesErrorsExist)));
			this.StatusesErrorsLabel.CaptionResourceString = null;
			this.StatusesErrorsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusesErrorsLabel.ForeColor = System.Drawing.Color.Black;
			this.StatusesErrorsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusesErrorsLabel, false);
			this.StatusesErrorsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusesErrorsLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 0, true);
			this.StatusesErrorsLabel.Name = "StatusesErrorsLabel";
			this.StatusesErrorsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.StatusesErrorsLabel.TabIndex = 18;
			this.StatusesErrorsLabel.VisibleChanged += new System.EventHandler(this.StatusesErrorsLabel_VisibleChanged);
			// 
			// MessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MessagesUserControl";
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.StatusesErrorsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		private Enterprise.ZArchitecture.ZTextBox MessageDetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage StatusesErrorsTabPage;
		public Enterprise.ZArchitecture.ZLabel StatusesErrorsLabel;
		internal MessagesStatusErrorsUserControl messagesStatusErrorsUserControl;
	}
}
