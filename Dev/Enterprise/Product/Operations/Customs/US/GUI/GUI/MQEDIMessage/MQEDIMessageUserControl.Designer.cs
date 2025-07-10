namespace Enterprise.Customs.US.GUI
{
	partial class MQEDIMessageUserControl
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
		void InitializeComponent()
		{
			this.MessageDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageInterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FormattedMessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActionStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messageDetailsPanel.SuspendLayout();
			this.processingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageDetailsTabControl.SuspendLayout();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// messageDetailsPanel
			// 
			this.messageDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 401, true);
			this.messageDetailsPanel.Visible = false;
			// 
			// messageContentsPanel
			// 
			this.messageContentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 401, true);
			this.messageContentsPanel.Visible = false;
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 325, true);
			// 
			// zTextBoxMessageNumber
			// 
			this.zTextBoxMessageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 30, true);
			this.zTextBoxMessageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.MQEDIMessage);
			// 
			// MessageDetailsTabControl
			// 
			this.MessageDetailsTabControl.Controls.Add(this.MessageInterpretationTabPage);
			this.MessageDetailsTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 3, true);
			this.MessageDetailsTabControl.Name = "MessageDetailsTabControl";
			this.MessageDetailsTabControl.SelectedIndex = 0;
			this.MessageDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 395, true);
			this.MessageDetailsTabControl.TabIndex = 24;
			// 
			// MessageInterpretationTabPage
			// 
			this.MessageInterpretationTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageUserControl|6479d13b-902b-4384-8016-a76f33d51491", "Message Details");
			this.MessageInterpretationTabPage.Controls.Add(this.MessageInterpretationTextBox);
			this.MessageInterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageInterpretationTabPage.Name = "MessageInterpretationTabPage";
			this.MessageInterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageInterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 368, true);
			this.MessageInterpretationTabPage.TabIndex = 0;
			// 
			// MessageInterpretationTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationTextBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_MessageInterpretation)));
			this.MessageInterpretationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageInterpretationTextBox, false);
			this.MessageInterpretationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationTextBox.Multiline = true;
			this.MessageInterpretationTextBox.Name = "MessageInterpretationTextBox";
			this.MessageInterpretationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageInterpretationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 362, true);
			this.MessageInterpretationTextBox.TabIndex = 1;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageUserControl|1ea6fa6d-a52c-43d7-856b-c6f4607e8dc0", "Message Text");
			this.MessageTextTabPage.Controls.Add(this.FormattedMessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 368, true);
			this.MessageTextTabPage.TabIndex = 1;
			// 
			// FormattedMessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedMessageTextTextBox, "EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_FormattedMessageText)));
			this.FormattedMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FormattedMessageTextTextBox, false);
			this.FormattedMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FormattedMessageTextTextBox.Multiline = true;
			this.FormattedMessageTextTextBox.Name = "FormattedMessageTextTextBox";
			this.FormattedMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FormattedMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 362, true);
			this.FormattedMessageTextTextBox.TabIndex = 0;
			// 
			// MessageTypeDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeDescTextBox, "EM_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_MessageType)));
			this.MessageTypeDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 47, true);
			this.MessageTypeDescTextBox.Name = "MessageTypeDescTextBox";
			this.MessageTypeDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.MessageTypeDescTextBox.TabIndex = 1;
			// 
			// MessageTimeDateEdit
			// 
			this.MessageTimeDateEdit.AllowDrop = true;
			this.MessageTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.MessageTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MessageTimeDateEdit, "EM_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_SystemCreateTimeUtc)));
			this.MessageTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.MessageTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 78, true);
			this.MessageTimeDateEdit.Name = "MessageTimeDateEdit";
			this.MessageTimeDateEdit.TabIndex = 2;
			// 
			// ActionStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.ActionStatusTextBox, "EM_ActionStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_ActionStatus)));
			this.ActionStatusTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageUserControl|a4687c84-d2a8-434d-bd0f-2bfa95431210", "Action Status");
			this.ActionStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 109, true);
			this.ActionStatusTextBox.Name = "ActionStatusTextBox";
			this.ActionStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.ActionStatusTextBox.TabIndex = 3;
			// 
			// messageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageNumberTextBox, "EM_MessageNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_MessageNum)));
			this.messageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 14, true);
			this.messageNumberTextBox.Name = "messageNumberTextBox";
			this.messageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.messageNumberTextBox.TabIndex = 0;
			// 
			// MQEDIMessageUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.messageNumberTextBox);
			this.Controls.Add(this.ActionStatusTextBox);
			this.Controls.Add(this.MessageTimeDateEdit);
			this.Controls.Add(this.MessageTypeDescTextBox);
			this.Controls.Add(this.MessageDetailsTabControl);
			this.Name = "MQEDIMessageUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 401, true);
			this.Controls.SetChildIndex(this.messageDetailsPanel, 0);
			this.Controls.SetChildIndex(this.messageContentsPanel, 0);
			this.Controls.SetChildIndex(this.MessageDetailsTabControl, 0);
			this.Controls.SetChildIndex(this.MessageTypeDescTextBox, 0);
			this.Controls.SetChildIndex(this.MessageTimeDateEdit, 0);
			this.Controls.SetChildIndex(this.ActionStatusTextBox, 0);
			this.Controls.SetChildIndex(this.messageNumberTextBox, 0);
			this.messageDetailsPanel.ResumeLayout(false);
			this.processingDetailsGroupBox.ResumeLayout(false);
			this.processingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageDetailsTabControl.ResumeLayout(false);
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageInterpretationTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal Enterprise.ZArchitecture.GUI.ZTabControl MessageDetailsTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageInterpretationTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		protected Enterprise.ZArchitecture.ZTextBox FormattedMessageTextTextBox;
		protected Enterprise.ZArchitecture.ZTextBox MessageInterpretationTextBox;
		protected Enterprise.ZArchitecture.ZTextBox MessageTypeDescTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit MessageTimeDateEdit;
		protected Enterprise.ZArchitecture.ZTextBox ActionStatusTextBox;
		protected ZArchitecture.ZTextBox messageNumberTextBox;
	}
}
