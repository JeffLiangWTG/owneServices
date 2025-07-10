namespace Enterprise.Customs.SG.Access.GUI
{
	partial class SGBillSpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CycleNumberDropEditWithFixedWidth = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CycleDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SGPayeeIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SGPartyStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SGPartyIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SGGstAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SGDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSTNReferenceNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CycleNumberDropEditWithFixedWidth.SuspendLayout();
			this.CycleDateDateEdit.SuspendLayout();
			this.SGPayeeIndicatorDropEdit.SuspendLayout();
			this.SGPartyStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.Access.Business.AsycudaBill);
			// 
			// CycleNumberDropEditWithFixedWidth
			// 
			this.CycleNumberDropEditWithFixedWidth.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CycleNumberDropEditWithFixedWidth, "CycleNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).CycleNumber)));
			this.CycleNumberDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(950, 0, true);
			this.CycleNumberDropEditWithFixedWidth.Name = "CycleNumberDropEditWithFixedWidth";
			this.CycleNumberDropEditWithFixedWidth.ShouldResizeByMaxLength = true;
			this.CycleNumberDropEditWithFixedWidth.ShowDescriptionBox = false;
			this.CycleNumberDropEditWithFixedWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CycleNumberDropEditWithFixedWidth.TabIndex = 6;
			// 
			// CycleDateDateEdit
			// 
			this.CycleDateDateEdit.AllowDrop = true;
			this.CycleDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CycleDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CycleDateDateEdit, "CycleDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).CycleDate)));
			this.CycleDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(789, 0, true);
			this.CycleDateDateEdit.Name = "CycleDateDateEdit";
			this.CycleDateDateEdit.TabIndex = 5;
			// 
			// SGPayeeIndicatorDropEdit
			// 
			this.SGPayeeIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SGPayeeIndicatorDropEdit, "SG_PayeeIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).SG_PayeeIndicator)));
			this.SGPayeeIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 0, true);
			this.SGPayeeIndicatorDropEdit.Name = "SGPayeeIndicatorDropEdit";
			this.SGPayeeIndicatorDropEdit.PreBoundMaxLength = 1;
			this.SGPayeeIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.SGPayeeIndicatorDropEdit.ShowDescriptionBox = false;
			this.SGPayeeIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.SGPayeeIndicatorDropEdit.TabIndex = 2;
			// 
			// SGPartyStatusDropEdit
			// 
			this.SGPartyStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SGPartyStatusDropEdit, "SG_PartyStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).SG_PartyStatus)));
			this.SGPartyStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 0, true);
			this.SGPartyStatusDropEdit.Name = "SGPartyStatusDropEdit";
			this.SGPartyStatusDropEdit.PreBoundMaxLength = 1;
			this.SGPartyStatusDropEdit.ShouldResizeByMaxLength = true;
			this.SGPartyStatusDropEdit.ShowDescriptionBox = false;
			this.SGPartyStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.SGPartyStatusDropEdit.TabIndex = 1;
			// 
			// MessageStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusDescriptionTextBox, "ShortStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).ShortStatusDescription)));
			this.MessageStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("fec905ca-fd1f-44a6-af82-aacf7b806aaa", "Msg. Status Desc.");
			this.MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 26, true);
			this.MessageStatusDescriptionTextBox.Name = "MessageStatusDescriptionTextBox";
			this.MessageStatusDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 20, true);
			this.MessageStatusDescriptionTextBox.TabIndex = 7;
			// 
			// SGPartyIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.SGPartyIDTextBox, "SG_PartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).SG_PartyID)));
			this.SGPartyIDTextBox.CaptionResourceString = null;
			this.SGPartyIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 0, true);
			this.SGPartyIDTextBox.Name = "SGPartyIDTextBox";
			this.SGPartyIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.SGPartyIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SGPartyIDTextBox.TabIndex = 0;
			// 
			// SGGstAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SGGstAmountCalcEdit, "TaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).TaxAmount)));
			this.SGGstAmountCalcEdit.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("A21D8AD2-65E5-42A2-BD02-3EE7DDC401C2", "GST", "GST Amount", "");
			this.SGGstAmountCalcEdit.DecimalPlaces = 2;
			this.SGGstAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 0, true);
			this.SGGstAmountCalcEdit.Name = "SGGstAmountCalcEdit";
			this.SGGstAmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.SGGstAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.SGGstAmountCalcEdit.TabIndex = 3;
			this.SGGstAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SGDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SGDutyAmountCalcEdit, "DutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).DutyAmount)));
			this.SGDutyAmountCalcEdit.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("29E062D2-7168-4F5D-B6F3-0C2A74AE98C3", "Duty", "Duty Amount", "");
			this.SGDutyAmountCalcEdit.DecimalPlaces = 2;
			this.SGDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 0, true);
			this.SGDutyAmountCalcEdit.Name = "SGDutyAmountCalcEdit";
			this.SGDutyAmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.SGDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.SGDutyAmountCalcEdit.TabIndex = 4;
			this.SGDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSTNReferenceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.GSTNReferenceNoTextBox, "GSTNReferenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.Access.Business.AsycudaBill)(null)).GSTNReferenceNo)));
			this.GSTNReferenceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(809, 26, true);
			this.GSTNReferenceNoTextBox.Name = "GSTNReferenceNoTextBox";
			this.GSTNReferenceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 38, true);
			this.GSTNReferenceNoTextBox.TabIndex = 8;
			// 
			// SGBillSpecificUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CycleNumberDropEditWithFixedWidth);
			this.Controls.Add(this.CycleDateDateEdit);
			this.Controls.Add(this.SGPayeeIndicatorDropEdit);
			this.Controls.Add(this.SGPartyStatusDropEdit);
			this.Controls.Add(this.MessageStatusDescriptionTextBox);
			this.Controls.Add(this.SGPartyIDTextBox);
			this.Controls.Add(this.SGGstAmountCalcEdit);
			this.Controls.Add(this.SGDutyAmountCalcEdit);
			this.Controls.Add(this.GSTNReferenceNoTextBox);
			this.Name = "SGBillSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1059, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CycleNumberDropEditWithFixedWidth.ResumeLayout(true);
			this.CycleNumberDropEditWithFixedWidth.PerformLayout();
			this.CycleDateDateEdit.ResumeLayout(true);
			this.CycleDateDateEdit.PerformLayout();
			this.SGPayeeIndicatorDropEdit.ResumeLayout(true);
			this.SGPayeeIndicatorDropEdit.PerformLayout();
			this.SGPartyStatusDropEdit.ResumeLayout(true);
			this.SGPartyStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth CycleNumberDropEditWithFixedWidth;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit CycleDateDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SGPayeeIndicatorDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SGPartyStatusDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox SGPartyIDTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit SGGstAmountCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit SGDutyAmountCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox MessageStatusDescriptionTextBox;
		internal Enterprise.ZArchitecture.ZTextBox GSTNReferenceNoTextBox;
	}
}
