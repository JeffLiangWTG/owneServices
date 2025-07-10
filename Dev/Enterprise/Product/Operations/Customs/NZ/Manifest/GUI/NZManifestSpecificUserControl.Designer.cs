namespace Enterprise.Customs.NZ.Manifest.GUI
{
	partial class NZManifestSpecificUserControl
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
			this.NotifyPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.NotifyPartyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NotifyPartyAddressControl.SuspendLayout();
			this.NotifyPartyPortFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Manifest.Business.AsycudaManifestHeader);
			// 
			// NotifyPartyAddressControl
			// 
			this.NotifyPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyAddressControl, "DeliveryNotificationParty.E2_OA_DeliveryNotificationParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaManifestHeader)(null)).DeliveryNotificationParty.E2_OA_DeliveryNotificationParty)));
			this.NotifyPartyAddressControl.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("8CCC4E56-CABA-47B3-A318-E2E5FC6B6AD0", "Delivery Notification Organization", "Select an organization with an identification code issued by TSW for the party registered to receive delivery notifications by TSW when the OCR is accepted. No address or communication details need be transmitted for a registered Delivery notification party.");
			this.NotifyPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 15, true);
			this.NotifyPartyAddressControl.Name = "NotifyPartyAddressControl";
			this.NotifyPartyAddressControl.PopupCaption = null;
			this.NotifyPartyAddressControl.ReadOnly = false;
			this.NotifyPartyAddressControl.ShowAddress = false;
			this.NotifyPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.NotifyPartyAddressControl.TabIndex = 0;
			// 
			// NotifyPartyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyNameTextBox, "DeliveryNotificationParty.DeliveryNotificationPartyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaManifestHeader)(null)).DeliveryNotificationParty.DeliveryNotificationPartyName)));
			this.NotifyPartyNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("4CFD553C-163F-4105-BC79-6328789D6AA6", "", "Notify Party", "Delivery Notification Party", "State the name of a Delivery notification party to be notified by TSW when the OCR is accepted.");
			this.NotifyPartyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 36, true);
			this.NotifyPartyNameTextBox.Name = "NotifyPartyNameTextBox";
			this.NotifyPartyNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.NotifyPartyNameTextBox.TabIndex = 1;
			// 
			// NotifyPartyEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyEmailTextBox, "DeliveryNotificationParty.DeliveryNotificationPartyEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaManifestHeader)(null)).DeliveryNotificationParty.DeliveryNotificationPartyEmail)));
			this.NotifyPartyEmailTextBox.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("F250A84B-99A3-4CF7-9243-087E021D0F6C", "Email", "A valid email address must be included in the associated communication details.");
			this.NotifyPartyEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NotifyPartyEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 57, true);
			this.NotifyPartyEmailTextBox.Name = "NotifyPartyEmailTextBox";
			this.NotifyPartyEmailTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.NotifyPartyEmailTextBox.TabIndex = 2;
			// 
			// NotifyPartyPortFindBox
			// 
			this.NotifyPartyPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyPortFindBox, "DeliveryNotificationParty.DeliveryNotificationPartyPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Manifest.Business.AsycudaManifestHeader)(null)).DeliveryNotificationParty.DeliveryNotificationPartyPort)));
			this.NotifyPartyPortFindBox.CaptionResourceString = Enterprise.Customs.NZ.Manifest.GUI.Res.GetData("81E8038B-C3D9-4BF3-BC3F-908AED022541", "Delivery Notification Port");
			this.NotifyPartyPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 79, true);
			this.NotifyPartyPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.NotifyPartyPortFindBox.Name = "NotifyPartyPortFindBox";
			this.NotifyPartyPortFindBox.PreBoundMaxLength = 5;
			this.NotifyPartyPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.NotifyPartyPortFindBox.TabIndex = 3;
			// 
			// NZManifestSpecificUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NotifyPartyPortFindBox);
			this.Controls.Add(this.NotifyPartyEmailTextBox);
			this.Controls.Add(this.NotifyPartyNameTextBox);
			this.Controls.Add(this.NotifyPartyAddressControl);
			this.Name = "NZManifestSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NotifyPartyAddressControl.ResumeLayout(true);
			this.NotifyPartyAddressControl.PerformLayout();
			this.NotifyPartyPortFindBox.ResumeLayout(true);
			this.NotifyPartyPortFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZAddressControl NotifyPartyAddressControl;
		private ZArchitecture.ZTextBox NotifyPartyNameTextBox;
		private ZArchitecture.ZTextBox NotifyPartyEmailTextBox;
		private ZArchitecture.GUI.ZCodeFindBox NotifyPartyPortFindBox;
	}
}
