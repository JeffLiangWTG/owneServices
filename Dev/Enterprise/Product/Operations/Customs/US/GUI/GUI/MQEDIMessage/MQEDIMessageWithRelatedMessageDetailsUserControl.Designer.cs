

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	partial class MQEDIMessageWithRelatedMessageDetailsUserControl
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
			this.ReceivedMessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReceivedMessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceivedMessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReceivedMessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResponseMsgDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageDetailsTabControl.SuspendLayout();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReceivedMessageDetailsTabPage.SuspendLayout();
			this.ReceivedMessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessageDetailsTabControl
			// 
			this.MessageDetailsTabControl.Controls.Add(this.ReceivedMessageDetailsTabPage);
			this.MessageDetailsTabControl.Controls.Add(this.ReceivedMessageTextTabPage);
			this.MessageDetailsTabControl.Controls.SetChildIndex(this.ReceivedMessageTextTabPage, 0);
			this.MessageDetailsTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			this.MessageDetailsTabControl.Controls.SetChildIndex(this.ReceivedMessageDetailsTabPage, 0);
			this.MessageDetailsTabControl.Controls.SetChildIndex(this.MessageInterpretationTabPage, 0);
			// 
			// FormattedMessageTextTextBox
			// 
			this.FormattedMessageTextTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// ActionStatusTextBox
			// 
			this.ActionStatusTextBox.Visible = false;
			// 
			// ReceivedMessageDetailsTabPage
			// 
			this.ReceivedMessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageWithRelatedMessageDetailsUserControl|a51ce4ae-d188-4895-a934-d38a7ced4edb", "Response Message Details");
			this.ReceivedMessageDetailsTabPage.Controls.Add(this.ReceivedMessageDetailsTextBox);
			this.ReceivedMessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReceivedMessageDetailsTabPage.Name = "ReceivedMessageDetailsTabPage";
			this.ReceivedMessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReceivedMessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 368, true);
			this.ReceivedMessageDetailsTabPage.TabIndex = 2;
			// 
			// ReceivedMessageDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceivedMessageDetailsTextBox, "EM_RelatedMessageInterpretationText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_RelatedMessageInterpretationText)));
			this.ReceivedMessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivedMessageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceivedMessageDetailsTextBox, false);
			this.ReceivedMessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ReceivedMessageDetailsTextBox.Multiline = true;
			this.ReceivedMessageDetailsTextBox.Name = "ReceivedMessageDetailsTextBox";
			this.ReceivedMessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ReceivedMessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 362, true);
			this.ReceivedMessageDetailsTextBox.TabIndex = 0;
			// 
			// ReceivedMessageTextTabPage
			// 
			this.ReceivedMessageTextTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageWithRelatedMessageDetailsUserControl|f6eb1243-c258-449f-8f52-9fbfb4556b0a", "Response Message Text");
			this.ReceivedMessageTextTabPage.Controls.Add(this.ReceivedMessageTextTextBox);
			this.ReceivedMessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReceivedMessageTextTabPage.Name = "ReceivedMessageTextTabPage";
			this.ReceivedMessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReceivedMessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 368, true);
			this.ReceivedMessageTextTabPage.TabIndex = 3;
			// 
			// ReceivedMessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceivedMessageTextTextBox, "EM_RelatedMessageFormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_RelatedMessageFormattedMessageText)));
			this.ReceivedMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceivedMessageTextTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceivedMessageTextTextBox, false);
			this.ReceivedMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ReceivedMessageTextTextBox.Multiline = true;
			this.ReceivedMessageTextTextBox.Name = "ReceivedMessageTextTextBox";
			this.ReceivedMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ReceivedMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 362, true);
			this.ReceivedMessageTextTextBox.TabIndex = 0;
			// 
			// ResponseMsgDateEdit
			// 
			this.ResponseMsgDateEdit.AutoCompleteMonthThreshold = 1;
			this.ResponseMsgDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ResponseMsgDateEdit, "EM_RelatedMessageCreateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.MQEDIMessage)(null)).EM_RelatedMessageCreateTime)));
			this.ResponseMsgDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageWithRelatedMessageDetailsUserControl|652b558d-6c6f-48cd-8ca5-03479c7852c2", "Response Msg. Time");
			this.ResponseMsgDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ResponseMsgDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 109, true);
			this.ResponseMsgDateEdit.Name = "ResponseMsgDateEdit";
			this.ResponseMsgDateEdit.TabIndex = 30;
			//
			// MessageInterpretationTabPage
			//
			this.MessageInterpretationTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageWithRelatedMessageDetailsUserControl|6479d13b-902b-4384-8016-a76f33d51491", "Transmit Message Details");
			//
			// MessageTextTabPage
			//
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MQEDIMessageWithRelatedMessageDetailsUserControl|1ea6fa6d-a52c-43d7-856b-c6f4607e8dc0", "Transmit Message Text");
			// 
			// MQEDIMessageWithRelatedMessageDetailsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ResponseMsgDateEdit);
			this.Name = "MQEDIMessageWithRelatedMessageDetailsUserControl";
			this.Controls.SetChildIndex(this.ActionStatusTextBox, 0);
			this.Controls.SetChildIndex(this.ResponseMsgDateEdit, 0);
			this.Controls.SetChildIndex(this.MessageDetailsTabControl, 0);
			this.Controls.SetChildIndex(this.MessageTypeDescTextBox, 0);
			this.Controls.SetChildIndex(this.MessageTimeDateEdit, 0);
			this.MessageDetailsTabControl.ResumeLayout(false);
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageInterpretationTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReceivedMessageDetailsTabPage.ResumeLayout(false);
			this.ReceivedMessageDetailsTabPage.PerformLayout();
			this.ReceivedMessageTextTabPage.ResumeLayout(false);
			this.ReceivedMessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage ReceivedMessageDetailsTabPage;
		private Enterprise.ZArchitecture.ZTextBox ReceivedMessageDetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage ReceivedMessageTextTabPage;
		private Enterprise.ZArchitecture.ZTextBox ReceivedMessageTextTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ResponseMsgDateEdit;
	}
}
