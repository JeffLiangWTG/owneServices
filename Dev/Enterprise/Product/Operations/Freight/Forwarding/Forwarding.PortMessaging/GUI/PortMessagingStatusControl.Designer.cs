namespace Enterprise.Forwarding.PortMessaging.GUI
{
	partial class PortMessagingStatusControl
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
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SZBIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SZBInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SZBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SentByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SentStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager);
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.Controls.Add(this.SZBIssueDateEdit);
			this.StatusGroupBox.Controls.Add(this.SZBInformationTextBox);
			this.StatusGroupBox.Controls.Add(this.SZBNumberTextBox);
			this.StatusGroupBox.Controls.Add(this.MessageStatusDateEdit);
			this.StatusGroupBox.Controls.Add(this.MessageStatusDescriptionTextBox);
			this.StatusGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.SentDateEdit);
			this.StatusGroupBox.Controls.Add(this.SentByTextBox);
			this.StatusGroupBox.Controls.Add(this.SentStatusTextBox);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 85, true);
			this.StatusGroupBox.TabIndex = 0;
			this.StatusGroupBox.TabStop = false;
			this.StatusGroupBox.Text = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetString("ed46883c-4405-4006-93eb-746650b69026", "Port Message Status");
			// 
			// SZBIssueDateEdit
			// 
			this.SZBIssueDateEdit.AllowDrop = true;
			this.SZBIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.SZBIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SZBIssueDateEdit, "SZBIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).SZBIssueDate)));
			this.SZBIssueDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SZBIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 59, true);
			this.SZBIssueDateEdit.Name = "SZBIssueDateEdit";
			this.SZBIssueDateEdit.TabIndex = 8;
			// 
			// SZBInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SZBInformationTextBox, "SZBInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).SZBInformation)));
			this.SZBInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 59, true);
			this.SZBInformationTextBox.Name = "SZBInformationTextBox";
			this.SZBInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.SZBInformationTextBox.TabIndex = 7;
			// 
			// SZBNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SZBNumberTextBox, "SZBNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).SZBNumber)));
			this.SZBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 59, true);
			this.SZBNumberTextBox.Name = "SZBNumberTextBox";
			this.SZBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.SZBNumberTextBox.TabIndex = 6;
			// 
			// MessageStatusDateEdit
			// 
			this.MessageStatusDateEdit.AllowDrop = true;
			this.MessageStatusDateEdit.AutoCompleteMonthThreshold = 1;
			this.MessageStatusDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDateEdit, "StatusRetriever.MessageStatusDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.MessageStatusDate)));
			this.MessageStatusDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.MessageStatusDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 37, true);
			this.MessageStatusDateEdit.Name = "MessageStatusDateEdit";
			this.MessageStatusDateEdit.TabIndex = 5;
			// 
			// MessageStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusDescriptionTextBox, "StatusRetriever.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.MessageStatusDescription)));
			this.MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 37, true);
			this.MessageStatusDescriptionTextBox.Name = "MessageStatusDescriptionTextBox";
			this.MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.MessageStatusDescriptionTextBox.TabIndex = 4;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "StatusRetriever.MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.MessageStatus)));
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 37, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.MessageStatusTextBox.TabIndex = 3;
			// 
			// SentDateEdit
			// 
			this.SentDateEdit.AllowDrop = true;
			this.SentDateEdit.AutoCompleteMonthThreshold = 1;
			this.SentDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SentDateEdit, "StatusRetriever.SentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.SentDate)));
			this.SentDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(698, 15, true);
			this.SentDateEdit.Name = "SentDateEdit";
			this.SentDateEdit.TabIndex = 2;
			// 
			// SentByTextBox
			// 
			this.BindingSource.SetBindingMember(this.SentByTextBox, "StatusRetriever.SentBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.SentBy)));
			this.SentByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 15, true);
			this.SentByTextBox.Name = "SentByTextBox";
			this.SentByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.SentByTextBox.TabIndex = 1;
			// 
			// SentStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.SentStatusTextBox, "StatusRetriever.SentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).StatusRetriever.SentStatus)));
			this.SentStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.SentStatusTextBox.Name = "SentStatusTextBox";
			this.SentStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.SentStatusTextBox.TabIndex = 0;
			// 
			// PortMessagingStatusControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.StatusGroupBox);
			this.Name = "PortMessagingStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 85, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private ZArchitecture.ZTextBox SentByTextBox;
		private ZArchitecture.ZTextBox SentStatusTextBox;
		private ZArchitecture.GUI.ZDateEdit SentDateEdit;
		private ZArchitecture.GUI.ZDateEdit MessageStatusDateEdit;
		private ZArchitecture.ZTextBox MessageStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.GUI.ZDateEdit SZBIssueDateEdit;
		private ZArchitecture.ZTextBox SZBInformationTextBox;
		private ZArchitecture.ZTextBox SZBNumberTextBox;
	}
}
