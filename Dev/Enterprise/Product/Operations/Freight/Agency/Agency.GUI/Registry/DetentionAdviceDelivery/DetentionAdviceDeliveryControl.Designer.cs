namespace Enterprise.Freight.Agency.GUI
{
	partial class DetentionAdviceDeliveryControl
	{
		private void InitializeComponent()
		{
			printerDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			groupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			modeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			sendToNotificationGroupCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.DetentionAdviceDelivery);
			// 
			// printerDropEdit
			// 
			this.BindingSource.SetBindingMember(printerDropEdit, "Printer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.DetentionAdviceDelivery)(null)).Printer)));
			printerDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("DetentionAdviceDeliveryControl|86343205-4ea4-4d84-bd9e-d98f3b554701", "Printer", "The printer to use when printing Detention Advices.");
			printerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 56, true);
			printerDropEdit.Name = "printerDropEdit";
			printerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			printerDropEdit.TabIndex = 2;
			// 
			// groupFindBox
			// 
			this.BindingSource.SetBindingMember(groupFindBox, "NotificationGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.DetentionAdviceDelivery)(null)).NotificationGroup)));
			groupFindBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("DetentionAdviceDeliveryControl|6f4ac40d-16c2-42ec-b8d6-8f1f3cdeaa05", "Group", "Notification Group", "The notification group to send to instead of printing a Detention Advice.");
			groupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 80, true);
			groupFindBox.Name = "groupFindBox";
			groupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			groupFindBox.TabIndex = 3;
			// 
			// modeDropEdit
			// 
			this.BindingSource.SetBindingMember(modeDropEdit, "Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.DetentionAdviceDelivery)(null)).Mode)));
			modeDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("DetentionAdviceDeliveryControl|3ed8d4d4-0391-46c5-8172-54c05fa8a691", "Mode");
			modeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 8, true);
			modeDropEdit.Name = "modeDropEdit";
			modeDropEdit.PreBoundMaxLength = 3;
			modeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			modeDropEdit.TabIndex = 0;
			// 
			// sendToNotificationGroupCheckBox
			// 
			sendToNotificationGroupCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(sendToNotificationGroupCheckBox, "SendToNotificationGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.DetentionAdviceDelivery)(null)).SendToNotificationGroup)));
			sendToNotificationGroupCheckBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("DetentionAdviceDeliveryControl|c2ede647-9bd9-401a-9370-4c50789892f7", "Send Copy to Notification Group", "Detention Advices should also be sent to members of the selected notification group.");
			sendToNotificationGroupCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			sendToNotificationGroupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 32, true);
			sendToNotificationGroupCheckBox.Name = "sendToNotificationGroupCheckBox";
			sendToNotificationGroupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			sendToNotificationGroupCheckBox.TabIndex = 1;
			sendToNotificationGroupCheckBox.UseVisualStyleBackColor = true;
			// 
			// DetentionAdviceDeliveryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(groupFindBox);
			this.Controls.Add(modeDropEdit);
			this.Controls.Add(printerDropEdit);
			this.Controls.Add(sendToNotificationGroupCheckBox);
			this.Name = "DetentionAdviceDeliveryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZGuidDropEdit printerDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox groupFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit modeDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox sendToNotificationGroupCheckBox;
	}
}
