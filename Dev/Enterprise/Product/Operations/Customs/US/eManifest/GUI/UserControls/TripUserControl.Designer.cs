namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class TripUserControl
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
			this.CarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TripReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TripGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FirstExpectedPortOfArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FirstExpectedPortOfArrivalDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FirstExpectedPortOfArrivalDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.EstimatedDateOfArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransitDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierCodeFindBox.SuspendLayout();
			this.TripGroupBox.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.FirstExpectedPortOfArrivalCodeFindBox.SuspendLayout();
			this.FirstExpectedPortOfArrivalDCodeFindBox.SuspendLayout();
			this.FirstExpectedPortOfArrivalDDropEdit.SuspendLayout();
			this.CarrierFindBox.SuspendLayout();
			this.ClientControl.SuspendLayout();
			this.EstimatedDateOfArrivalDateEdit.SuspendLayout();
			this.TransitDirectionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// CarrierCodeFindBox
			// 
			this.CarrierCodeFindBox.AllowDrop = true;
			this.CarrierCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CarrierCodeFindBox, "BH_CarrierSCAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_CarrierSCAC)));
			this.CarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|28d113b4-b305-4545-aefc-0965d69aba14", "SCAC", "Carrier Code (SCAC)", "The Standard Carrier Alpha Code (SCAC).");
			this.CarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 62, true);
			this.CarrierCodeFindBox.Name = "CarrierCodeFindBox";
			this.CarrierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CarrierCodeFindBox.ParentType = null;
			this.CarrierCodeFindBox.PreBoundMaxLength = 4;
			this.CarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.CarrierCodeFindBox.TabIndex = 4;
			// 
			// TripReferenceTextBox
			// 
			this.TripReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TripReferenceTextBox, "BH_VoyageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_VoyageNumber)));
			this.TripReferenceTextBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|4f335a51-f46a-48d6-be56-388b0b11ccd6", "Trip Reference", "A unique number assigned by the carrier issuing the Manifest to each movement of a conveyance.");
			this.TripReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 39, true);
			this.TripReferenceTextBox.Name = "TripReferenceTextBox";
			this.TripReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.TripReferenceTextBox.TabIndex = 1;
			// 
			// TripGroupBox
			// 
			this.TripGroupBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|299d6a07-49a1-4c4c-8dfe-4651c37546eb", "Trip Details");
			this.TripGroupBox.Controls.Add(this.zDropEdit2);
			this.TripGroupBox.Controls.Add(this.zDropEdit1);
			this.TripGroupBox.Controls.Add(this.FirstExpectedPortOfArrivalCodeFindBox);
			this.TripGroupBox.Controls.Add(this.FirstExpectedPortOfArrivalDCodeFindBox);
			this.TripGroupBox.Controls.Add(this.FirstExpectedPortOfArrivalDDropEdit);
			this.TripGroupBox.Controls.Add(this.CarrierFindBox);
			this.TripGroupBox.Controls.Add(this.ClientControl);
			this.TripGroupBox.Controls.Add(this.EstimatedDateOfArrivalDateEdit);
			this.TripGroupBox.Controls.Add(this.TransitDirectionDropEdit);
			this.TripGroupBox.Controls.Add(this.TripReferenceTextBox);
			this.TripGroupBox.Controls.Add(this.CarrierCodeFindBox);
			this.TripGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TripGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TripGroupBox.Name = "TripGroupBox";
			this.TripGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 128, true);
			this.TripGroupBox.TabIndex = 0;
			this.TripGroupBox.TabStop = false;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "BH_ReleaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_ReleaseStatus)));
			this.zDropEdit2.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c91ee572-858e-4a4a-a24a-795b40f214f8", "Release Status");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 106, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 1;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.zDropEdit2.TabIndex = 8;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.zDropEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zDropEdit1, "BH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_MessageStatus)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("f8b4f674-4436-407e-9134-59cd969fad33", "Message Status");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 106, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 1;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.zDropEdit1.TabIndex = 9;
			// 
			// FirstExpectedPortOfArrivalCodeFindBox
			// 
			this.FirstExpectedPortOfArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstExpectedPortOfArrivalCodeFindBox, "BH_RL_NKPortUnlading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_RL_NKPortUnlading)));
			this.FirstExpectedPortOfArrivalCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|62b62f86-dd1a-441a-ab89-aa740d659dfc", "UNLOCO", "Expected Port of Arrival", "First Expected Port of Arrival", "First port where conveyance will enter the United States (UNLOCO).");
			this.FirstExpectedPortOfArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 83, true);
			this.FirstExpectedPortOfArrivalCodeFindBox.Name = "FirstExpectedPortOfArrivalCodeFindBox";
			this.FirstExpectedPortOfArrivalCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FirstExpectedPortOfArrivalCodeFindBox.ParentType = null;
			this.FirstExpectedPortOfArrivalCodeFindBox.PreBoundMaxLength = 5;
			this.FirstExpectedPortOfArrivalCodeFindBox.ShowDescriptionBox = false;
			this.FirstExpectedPortOfArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.FirstExpectedPortOfArrivalCodeFindBox.TabIndex = 6;
			// 
			// FirstExpectedPortOfArrivalDCodeFindBox
			// 
			this.FirstExpectedPortOfArrivalDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstExpectedPortOfArrivalDCodeFindBox, "BH_PortUnladingDCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_PortUnladingDCode)));
			this.FirstExpectedPortOfArrivalDCodeFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|fcce1065-2259-4699-9184-d4fccb417c3c", "Schedule D", "Arrival Port (Schedule D)", "First Expected Port of Arrival (Schedule D)", "First port where conveyance will enter the United States (Schedule D - US Domestic Ports).");
			this.FirstExpectedPortOfArrivalDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 83, true);
			this.FirstExpectedPortOfArrivalDCodeFindBox.Name = "FirstExpectedPortOfArrivalDCodeFindBox";
			this.FirstExpectedPortOfArrivalDCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FirstExpectedPortOfArrivalDCodeFindBox.ParentType = null;
			this.FirstExpectedPortOfArrivalDCodeFindBox.PreBoundMaxLength = 4;
			this.FirstExpectedPortOfArrivalDCodeFindBox.ShowDescriptionBox = false;
			this.FirstExpectedPortOfArrivalDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.FirstExpectedPortOfArrivalDCodeFindBox.TabIndex = 5;
			// 
			// FirstExpectedPortOfArrivalDDropEdit
			// 
			this.FirstExpectedPortOfArrivalDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstExpectedPortOfArrivalDDropEdit, "BH_PortUnladingDCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_PortUnladingDCode)));
			this.FirstExpectedPortOfArrivalDDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|fcce1065-2259-4699-9184-d4fccb417c3c", "Schedule D", "Arrival Port (Schedule D)", "First Expected Port of Arrival (Schedule D)", "First port where conveyance will enter the United States (Schedule D - US Domestic Ports).");
			this.FirstExpectedPortOfArrivalDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 83, true);
			this.FirstExpectedPortOfArrivalDDropEdit.Visible = false;
			this.FirstExpectedPortOfArrivalDDropEdit.Name = "FirstExpectedPortOfArrivalDDropEdit";
			this.FirstExpectedPortOfArrivalDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.FirstExpectedPortOfArrivalDDropEdit.TabIndex = 5;
			// 
			// CarrierFindBox
			// 
			this.CarrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierFindBox, "BH_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_OH_Carrier)));
			this.CarrierFindBox.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|44444444-b226-4b10-9fd0-464801c38db2", "Carrier");
			this.CarrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 62, true);
			this.CarrierFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CarrierFindBox.Name = "CarrierFindBox";
			this.CarrierFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CarrierFindBox.ParentType = null;
			this.CarrierFindBox.PreBoundMaxLength = 9;
			this.CarrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CarrierFindBox.TabIndex = 3;
			// 
			// ClientControl
			// 
			this.ClientControl.AllowDrop = true;
			this.ClientControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientControl, "BH_OA_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_OA_Importer)));
			this.ClientControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("c4b7b2f3-ec4c-4f22-a319-7a8465cb2cf2", "Client");
			this.ClientControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 17, true);
			this.ClientControl.Name = "ClientControl";
			this.ClientControl.PopupCaption = "";
			this.ClientControl.ShowAddress = false;
			this.ClientControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ClientControl.TabIndex = 0;
			// 
			// EstimatedDateOfArrivalDateEdit
			// 
			this.EstimatedDateOfArrivalDateEdit.AllowDrop = true;
			this.EstimatedDateOfArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EstimatedDateOfArrivalDateEdit, "BH_ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_ETA)));
			this.EstimatedDateOfArrivalDateEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|a38cc5cd-b226-4b10-9fd0-464801c38db2", "ETA", "Estimated Date of Arrival", "Estimated Date/Time conveyance will arrive at the first port in USA.");
			this.EstimatedDateOfArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstimatedDateOfArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 84, true);
			this.EstimatedDateOfArrivalDateEdit.Name = "EstimatedDateOfArrivalDateEdit";
			this.EstimatedDateOfArrivalDateEdit.TabIndex = 7;
			// 
			// TransitDirectionDropEdit
			// 
			this.TransitDirectionDropEdit.AllowDrop = true;
			this.TransitDirectionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransitDirectionDropEdit, "BH_TransitDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).BH_TransitDirection)));
			this.TransitDirectionDropEdit.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("TripUserControl|4f21a179-893f-4ae6-b6a1-aedb2236679a", "Transit Direction", "Indicates whether the conveyance is entering or leaving the United States.");
			this.TransitDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 39, true);
			this.TransitDirectionDropEdit.Name = "TransitDirectionDropEdit";
			this.TransitDirectionDropEdit.PreBoundMaxLength = 1;
			this.TransitDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.TransitDirectionDropEdit.TabIndex = 2;
			// 
			// TripUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TripGroupBox);
			this.Name = "TripUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 128, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierCodeFindBox.ResumeLayout(true);
			this.CarrierCodeFindBox.PerformLayout();
			this.TripGroupBox.ResumeLayout(false);
			this.TripGroupBox.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.FirstExpectedPortOfArrivalCodeFindBox.ResumeLayout(true);
			this.FirstExpectedPortOfArrivalCodeFindBox.PerformLayout();
			this.FirstExpectedPortOfArrivalDCodeFindBox.ResumeLayout(true);
			this.FirstExpectedPortOfArrivalDCodeFindBox.PerformLayout();
			this.FirstExpectedPortOfArrivalDDropEdit.ResumeLayout(true);
			this.FirstExpectedPortOfArrivalDDropEdit.PerformLayout();
			this.CarrierFindBox.ResumeLayout(true);
			this.CarrierFindBox.PerformLayout();
			this.ClientControl.ResumeLayout(true);
			this.ClientControl.PerformLayout();
			this.EstimatedDateOfArrivalDateEdit.ResumeLayout(true);
			this.EstimatedDateOfArrivalDateEdit.PerformLayout();
			this.TransitDirectionDropEdit.ResumeLayout(true);
			this.TransitDirectionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox CarrierCodeFindBox;
		private ZArchitecture.ZTextBox TripReferenceTextBox;
		private ZArchitecture.GUI.ZGroupBox TripGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox FirstExpectedPortOfArrivalCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox FirstExpectedPortOfArrivalDCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit FirstExpectedPortOfArrivalDDropEdit;
		private ZArchitecture.GUI.ZDateEdit EstimatedDateOfArrivalDateEdit;
		private ZArchitecture.GUI.ZDropEdit TransitDirectionDropEdit;
		private ZArchitecture.GUI.ZAddressControl ClientControl;
		private ZArchitecture.GUI.ZGuidFindBox CarrierFindBox;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZDropEdit zDropEdit2;
	}
}
