using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI;

partial class MessageSendingFormBottomSectionUserControl
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
			this.TCI11DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QueryInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PresentationDateAndTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.ActualOfficeOfDestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AgreeWithMinorDiscrepanciesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ActualConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TCI11DateEdit.SuspendLayout();
			this.PresentationDateAndTimeOffsetEdit.SuspendLayout();
			this.ActualOfficeOfDestinationFindBox.SuspendLayout();
			this.ActualConsigneeDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent);
			// 
			// JustificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JustificationTextBox, "SendingObjectsCollection.Justification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Justification)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.JustificationTextBox, 0);
			this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 18, true);
			this.JustificationTextBox.Multiline = true;
			this.JustificationTextBox.Name = "JustificationTextBox";
			this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 215, true);
			this.JustificationTextBox.TabIndex = 0;
			// 
			// TCI11DateEdit
			// 
			this.TCI11DateEdit.AllowDrop = true;
			this.TCI11DateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TCI11DateEdit, "SendingObjectsCollection.TCI11");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).TCI11)));
			this.TCI11DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 18, true);
			this.TCI11DateEdit.Name = "TCI11DateEdit";
			this.TCI11DateEdit.TabIndex = 1;
			// 
			// QueryInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.QueryInformationTextBox, "SendingObjectsCollection.QueryInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).QueryInformation)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.QueryInformationTextBox, 0);
			this.QueryInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 40, true);
			this.QueryInformationTextBox.Multiline = true;
			this.QueryInformationTextBox.Name = "QueryInformationTextBox";
			this.QueryInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 196, true);
			this.QueryInformationTextBox.TabIndex = 2;
			// 
			// PresentationDateAndTimeOffsetEdit
			// 
			this.PresentationDateAndTimeOffsetEdit.AllowDrop = true;
			this.PresentationDateAndTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationDateAndTimeOffsetEdit, "SendingObjectsCollection.PresentationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).PresentationDateTime)));
			this.PresentationDateAndTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationDateAndTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 1, true);
			this.PresentationDateAndTimeOffsetEdit.Name = "PresentationDateAndTimeOffsetEdit";
			this.PresentationDateAndTimeOffsetEdit.TabIndex = 5;
			// 
			// ActualOfficeOfDestinationFindBox
			// 
			this.ActualOfficeOfDestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualOfficeOfDestinationFindBox, "SendingObjectsCollection.ActualOfficeOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualOfficeOfDestination)));
			this.ActualOfficeOfDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 18, true);
			this.ActualOfficeOfDestinationFindBox.Name = "ActualOfficeOfDestinationFindBox";
			this.ActualOfficeOfDestinationFindBox.ParentType = null;
			this.ActualOfficeOfDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 15, true);
			this.ActualOfficeOfDestinationFindBox.TabIndex = 3;
			// 
			// AgreeWithMinorDiscrepanciesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AgreeWithMinorDiscrepanciesCheckBox, "SendingObjectsCollection.AgreeWithMinorDiscrepancies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AgreeWithMinorDiscrepancies)));
			this.AgreeWithMinorDiscrepanciesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AgreeWithMinorDiscrepanciesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.AgreeWithMinorDiscrepanciesCheckBox.Name = "AgreeWithMinorDiscrepanciesCheckBox";
			this.AgreeWithMinorDiscrepanciesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 24, true);
			this.AgreeWithMinorDiscrepanciesCheckBox.TabIndex = 6;
			this.AgreeWithMinorDiscrepanciesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ActualConsigneeDocAddressControl
			// 
			this.ActualConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ActualConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualConsigneeDocAddressControl, "SendingObjectsCollection.ActualConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.NL.NCTS.Business.MessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualConsignee)));
			this.ActualConsigneeDocAddressControl.BindToOrganisations = "SendingObjectsCollection.Lookups.Consignees";
			this.ActualConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.NL.NCTS.GUI.Res.GetData("BFCE43A9-19B5-4FDD-A98A-2F1DCDCF83B4", "Actual Consignee");
			this.ActualConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithOverride;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ActualConsigneeDocAddressControl, false);
			this.ActualConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 40, true);
			this.ActualConsigneeDocAddressControl.Name = "ActualConsigneeDocAddressControl";
			this.ActualConsigneeDocAddressControl.ReadOnly = false;
			this.ActualConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ActualConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 130, true);
			this.ActualConsigneeDocAddressControl.TabIndex = 7;
			this.ActualConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// MessageSendingFormBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActualConsigneeDocAddressControl);
			this.Controls.Add(this.AgreeWithMinorDiscrepanciesCheckBox);
			this.Controls.Add(this.PresentationDateAndTimeOffsetEdit);
			this.Controls.Add(this.TCI11DateEdit);
			this.Controls.Add(this.JustificationTextBox);
			this.Controls.Add(this.QueryInformationTextBox);
			this.Controls.Add(this.ActualOfficeOfDestinationFindBox);
			this.Name = "MessageSendingFormBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 294, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TCI11DateEdit.ResumeLayout(true);
			this.TCI11DateEdit.PerformLayout();
			this.PresentationDateAndTimeOffsetEdit.ResumeLayout(true);
			this.PresentationDateAndTimeOffsetEdit.PerformLayout();
			this.ActualOfficeOfDestinationFindBox.ResumeLayout(true);
			this.ActualOfficeOfDestinationFindBox.PerformLayout();
			this.ActualConsigneeDocAddressControl.ResumeLayout(true);
			this.ActualConsigneeDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}
	#endregion

	internal ZArchitecture.ZTextBox JustificationTextBox;
	internal ZArchitecture.GUI.ZDateEdit TCI11DateEdit;
	internal ZArchitecture.ZTextBox QueryInformationTextBox;
	internal ZArchitecture.GUI.ZDateTimeOffsetEdit PresentationDateAndTimeOffsetEdit;
	internal ZCodeFindBox ActualOfficeOfDestinationFindBox;
	internal ZCheckBox AgreeWithMinorDiscrepanciesCheckBox;
	internal ZDocAddressControl ActualConsigneeDocAddressControl;
}
