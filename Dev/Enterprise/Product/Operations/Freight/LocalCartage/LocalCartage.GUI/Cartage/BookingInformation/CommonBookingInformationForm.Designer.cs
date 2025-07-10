using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CommonBookingInformationForm
	{
		public new void InitializeComponent()
		{
			this.BookingStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BookingStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BookingAcceptedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BookingRejectedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BookingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BookingGroupBox.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 167, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CommonCartageBookingInformation);
			// 
			// BookingStatusLabel
			// 
			this.BookingStatusLabel.AutoSize = true;
			this.BookingStatusLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|9f989f44-7248-47fa-bd14-17c662e02400", "Select Booking Status");
			this.BookingStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.BookingStatusLabel.Name = "BookingStatusLabel";
			this.BookingStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.BookingStatusLabel.TabIndex = 11;
			// 
			// BookingStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.BookingStatusDropEdit, "BookingStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageBookingInformation)(null)).BookingStatus)));
			this.BookingStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 62, true);
			this.BookingStatusDropEdit.Name = "BookingStatusDropEdit";
			this.BookingStatusDropEdit.PreBoundMaxLength = 3;
			this.BookingStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.BookingStatusDropEdit.TabIndex = 2;
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|05901b56-bdee-436b-97ab-d4b790d77f3f", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 135, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 11;
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|635c41e6-b405-4c87-b9a7-1fe3d9a37a24", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 135, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 10;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// BookingAcceptedRadioButton
			// 
			this.BookingAcceptedRadioButton.AutoCheck = false;
			this.BookingAcceptedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BookingAcceptedRadioButton, "BookingAccepted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CommonCartageBookingInformation)(null)).BookingAccepted)));
			this.BookingAcceptedRadioButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|ead28c26-8034-4adc-98fc-e5a254894599", "Booking Is Accepted");
			this.BookingAcceptedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BookingAcceptedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.BookingAcceptedRadioButton.Name = "BookingAcceptedRadioButton";
			this.BookingAcceptedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.BookingAcceptedRadioButton.TabIndex = 0;
			this.BookingAcceptedRadioButton.TabStop = true;
			this.BookingAcceptedRadioButton.UseVisualStyleBackColor = true;
			// 
			// BookingRejectedRadioButton
			// 
			this.BookingRejectedRadioButton.AutoCheck = false;
			this.BookingRejectedRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BookingRejectedRadioButton, "BookingRejected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CommonCartageBookingInformation)(null)).BookingRejected)));
			this.BookingRejectedRadioButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|b233d67f-4782-4c1a-9248-180c21e3e351", "Booking Is Rejected");
			this.BookingRejectedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BookingRejectedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 27, true);
			this.BookingRejectedRadioButton.Name = "BookingRejectedRadioButton";
			this.BookingRejectedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.BookingRejectedRadioButton.TabIndex = 1;
			this.BookingRejectedRadioButton.TabStop = true;
			this.BookingRejectedRadioButton.UseVisualStyleBackColor = true;
			// 
			// BookingGroupBox
			// 
			this.BookingGroupBox.Controls.Add(this.BookingAcceptedRadioButton);
			this.BookingGroupBox.Controls.Add(this.BookingRejectedRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BookingGroupBox, false);
			this.BookingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.BookingGroupBox.Name = "BookingGroupBox";
			this.BookingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 50, true);
			this.BookingGroupBox.TabIndex = 0;
			this.BookingGroupBox.TabStop = false;
			// 
			// CommentLabel
			// 
			this.CommentLabel.AutoSize = true;
			this.CommentLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|e7144a3c-a3ec-4177-b9d0-046e802cefb4", "Comments (optional)");
			this.CommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 90, true);
			this.CommentLabel.Name = "CommentLabel";
			this.CommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.CommentLabel.TabIndex = 17;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "BookingComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageBookingInformation)(null)).BookingComment)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 106, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 20, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.BookingGroupBox);
			this.zPanel1.Controls.Add(this.CancelButton);
			this.zPanel1.Controls.Add(this.zTextBox1);
			this.zPanel1.Controls.Add(this.BookingStatusDropEdit);
			this.zPanel1.Controls.Add(this.OKButton);
			this.zPanel1.Controls.Add(this.CommentLabel);
			this.zPanel1.Controls.Add(this.BookingStatusLabel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 167, true);
			this.zPanel1.TabIndex = 18;
			// 
			// CommonBookingInformationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 191, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("CommonBookingInformationForm|d56e9e22-4a8e-453e-9658-d06a089e371f", "Port Transport Booking Information");
			this.Controls.Add(this.zPanel1);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(CommonCartageBookingInformation);
			this.DataSourceTypeName = "CommonCartageBookingInformation";
			this.Name = "CommonBookingInformationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BookingGroupBox.ResumeLayout(false);
			this.BookingGroupBox.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
