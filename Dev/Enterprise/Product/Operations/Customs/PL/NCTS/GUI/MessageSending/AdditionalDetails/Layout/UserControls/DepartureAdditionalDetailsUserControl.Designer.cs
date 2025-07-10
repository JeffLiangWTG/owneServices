namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class DepartureAdditionalDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.JustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmendmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PresentationDateAndTimeDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.TC11DeliveryDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.AdditionalTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ActualConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DepartureOfficeOfEnquiryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ActualOfficeOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AmendmentTypeDropEdit.SuspendLayout();
			this.PresentationDateAndTimeDateTimeOffsetEdit.SuspendLayout();
			this.TC11DeliveryDateTimeOffsetEdit.SuspendLayout();
			this.ActualConsigneeDocAddressControl.SuspendLayout();
			this.DepartureOfficeOfEnquiryCodeFindBox.SuspendLayout();
			this.ActualOfficeOfDestinationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.NCTS.Business.MessageSendingObject);
			// 
			// JustificationTextBox
			// 
			this.JustificationTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JustificationTextBox, "Justification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).Justification)));
			this.JustificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.JustificationTextBox, 0);
			this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 58, true);
			this.JustificationTextBox.Multiline = true;
			this.JustificationTextBox.Name = "JustificationTextBox";
			this.JustificationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 77, true);
			this.JustificationTextBox.TabIndex = 0;
			// 
			// AmendmentTypeDropEdit
			// 
			this.AmendmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentTypeDropEdit, "AmendmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).AmendmentType)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.AmendmentTypeDropEdit, 0);
			this.AmendmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 13, true);
			this.AmendmentTypeDropEdit.Name = "AmendmentTypeDropEdit";
			this.AmendmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.AmendmentTypeDropEdit.TabIndex = 1;
			// 
			// PresentationDateAndTimeDateTimeOffsetEdit
			// 
			this.PresentationDateAndTimeDateTimeOffsetEdit.AllowDrop = true;
			this.PresentationDateAndTimeDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationDateAndTimeDateTimeOffsetEdit, "PresentationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).PresentationDateTime)));
			this.PresentationDateAndTimeDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationDateAndTimeDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 36, true);
			this.PresentationDateAndTimeDateTimeOffsetEdit.Name = "PresentationDateAndTimeDateTimeOffsetEdit";
			this.PresentationDateAndTimeDateTimeOffsetEdit.TabIndex = 2;
			// 
			// TC11DeliveryDateTimeOffsetEdit
			// 
			this.TC11DeliveryDateTimeOffsetEdit.AllowDrop = true;
			this.TC11DeliveryDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TC11DeliveryDateTimeOffsetEdit, "TC11DeliveryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).TC11DeliveryDate)));
			this.TC11DeliveryDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.TC11DeliveryDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 154, true);
			this.TC11DeliveryDateTimeOffsetEdit.Name = "TC11DeliveryDateTimeOffsetEdit";
			this.TC11DeliveryDateTimeOffsetEdit.TabIndex = 3;
			// 
			// AdditionalTextBox
			// 
			this.AdditionalTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalTextBox, "AdditionalText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).AdditionalText)));
			this.AdditionalTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AdditionalTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AdditionalTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 208, true);
			this.AdditionalTextBox.Multiline = true;
			this.AdditionalTextBox.Name = "AdditionalTextBox";
			this.AdditionalTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AdditionalTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 77, true);
			this.AdditionalTextBox.TabIndex = 4;
			// 
			// ActualConsigneeDocAddressControl
			// 
			this.ActualConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ActualConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualConsigneeDocAddressControl, "ActualConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).ActualConsignee)));
			this.ActualConsigneeDocAddressControl.BindToOrganisations = "SendingObjectsCollection.Lookups.Consignees";
			this.ActualConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.PL.NCTS.GUI.Res.GetData("51bc337f-65eb-4f71-a7c0-7cacb02127f0", "Actual Consignee");
			this.ActualConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverride;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ActualConsigneeDocAddressControl, false);
			this.ActualConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 58, true);
			this.ActualConsigneeDocAddressControl.Name = "ActualConsigneeDocAddressControl";
			this.ActualConsigneeDocAddressControl.ReadOnly = false;
			this.ActualConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ActualConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 45, true);
			this.ActualConsigneeDocAddressControl.TabIndex = 5;
			this.ActualConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// DepartureOfficeOfEnquiryCodeFindBox
			// 
			this.DepartureOfficeOfEnquiryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureOfficeOfEnquiryCodeFindBox, "DepartureOfficeOfEnquiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).DepartureOfficeOfEnquiry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DepartureOfficeOfEnquiryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DepartureOfficeOfEnquiryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 302, true);
			this.DepartureOfficeOfEnquiryCodeFindBox.Name = "DepartureOfficeOfEnquiryCodeFindBox";
			this.DepartureOfficeOfEnquiryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DepartureOfficeOfEnquiryCodeFindBox.ParentType = null;
			this.DepartureOfficeOfEnquiryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.DepartureOfficeOfEnquiryCodeFindBox.TabIndex = 6;
			// 
			// ActualOfficeOfDestinationCodeFindBox
			// 
			this.ActualOfficeOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualOfficeOfDestinationCodeFindBox, "ActualOfficeOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.NCTS.Business.MessageSendingObject)(null)).ActualOfficeOfDestination)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ActualOfficeOfDestinationCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ActualOfficeOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 218, true);
			this.ActualOfficeOfDestinationCodeFindBox.Name = "ActualOfficeOfDestinationCodeFindBox";
			this.ActualOfficeOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ActualOfficeOfDestinationCodeFindBox.ParentType = null;
			this.ActualOfficeOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.ActualOfficeOfDestinationCodeFindBox.TabIndex = 7;
			// 
			// DepartureAdditionalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActualOfficeOfDestinationCodeFindBox);
			this.Controls.Add(this.DepartureOfficeOfEnquiryCodeFindBox);
			this.Controls.Add(this.ActualConsigneeDocAddressControl);
			this.Controls.Add(this.AdditionalTextBox);
			this.Controls.Add(this.TC11DeliveryDateTimeOffsetEdit);
			this.Controls.Add(this.JustificationTextBox);
			this.Controls.Add(this.AmendmentTypeDropEdit);
			this.Controls.Add(this.PresentationDateAndTimeDateTimeOffsetEdit);
			this.Name = "DepartureAdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 334, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AmendmentTypeDropEdit.ResumeLayout(true);
			this.AmendmentTypeDropEdit.PerformLayout();
			this.PresentationDateAndTimeDateTimeOffsetEdit.ResumeLayout(true);
			this.PresentationDateAndTimeDateTimeOffsetEdit.PerformLayout();
			this.TC11DeliveryDateTimeOffsetEdit.ResumeLayout(true);
			this.TC11DeliveryDateTimeOffsetEdit.PerformLayout();
			this.ActualConsigneeDocAddressControl.ResumeLayout(true);
			this.ActualConsigneeDocAddressControl.PerformLayout();
			this.DepartureOfficeOfEnquiryCodeFindBox.ResumeLayout(true);
			this.DepartureOfficeOfEnquiryCodeFindBox.PerformLayout();
			this.ActualOfficeOfDestinationCodeFindBox.ResumeLayout(true);
			this.ActualOfficeOfDestinationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit AmendmentTypeDropEdit;
		internal ZArchitecture.GUI.ZDateTimeOffsetEdit PresentationDateAndTimeDateTimeOffsetEdit;
		internal ZArchitecture.ZTextBox JustificationTextBox;
		internal ZArchitecture.GUI.ZDateTimeOffsetEdit TC11DeliveryDateTimeOffsetEdit;
		internal ZArchitecture.ZTextBox AdditionalTextBox;
		internal MasterFiles.GUI.ZDocAddressControl ActualConsigneeDocAddressControl;
		internal ZArchitecture.GUI.ZCodeFindBox DepartureOfficeOfEnquiryCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox ActualOfficeOfDestinationCodeFindBox;
	}
}
