using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class ACEManifestCountrySpecificUserControl
	{
		void InitializeComponent()
		{
			this.EstDateAtFirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BillStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FIRMSTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExpressCourierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EstDateAtFirstArrivalDateEdit.SuspendLayout();
			this.ExpressCourierCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader);
			// 
			// EstDateAtFirstArrivalDateEdit
			// 
			this.EstDateAtFirstArrivalDateEdit.AllowDrop = true;
			this.EstDateAtFirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstDateAtFirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstDateAtFirstArrivalDateEdit, "EstDateAtFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader)(null)).EstDateAtFirstArrival)));
			this.EstDateAtFirstArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.EstDateAtFirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 69, true);
			this.EstDateAtFirstArrivalDateEdit.Name = "EstDateAtFirstArrivalDateEdit";
			this.EstDateAtFirstArrivalDateEdit.TabIndex = 5;
			// 
			// BillStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillStatusTextBox, "BillStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader)(null)).BillStatus)));
			this.BillStatusTextBox.CaptionResourceString = null;
			this.BillStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.BillStatusTextBox.Name = "BillStatusTextBox";
			this.BillStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.BillStatusTextBox.TabIndex = 2;
			// 
			// BillStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillStatusDescriptionTextBox, "BillStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader)(null)).BillStatusDescription)));
			this.BillStatusDescriptionTextBox.CaptionResourceString = Res.GetData("8097307d-5c59-448c-88ab-67904901249f", " ");
			this.BillStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.BillStatusDescriptionTextBox.Name = "BillStatusDescriptionTextBox";
			this.BillStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 60, true);
			this.BillStatusDescriptionTextBox.Multiline = true;
			this.BillStatusDescriptionTextBox.WordWrap = true;
			this.BillStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BillStatusDescriptionTextBox.TabIndex = 3;
			// 
			// FIRMSTextBox
			// 
			this.BindingSource.SetBindingMember(this.FIRMSTextBox, "FIRMSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader)(null)).FIRMSCode)));
			this.FIRMSTextBox.CaptionResourceString = Res.GetData("56C1DD16-D7D2-4045-9764-4CDFA669F3DA", "FIRMS");
			this.FIRMSTextBox.Name = "FIRMSTextBox";
			this.FIRMSTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			//
			// ExpressCourierCheckBox
			//
			this.BindingSource.SetBindingMember(this.ExpressCourierCheckBox, "IsExpressCourier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaManifestHeader)(null)).IsExpressCourier)));
			this.ExpressCourierCheckBox.Name = "ExpressCourierCheckBox";
			this.ExpressCourierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExpressCourierCheckBox.Text = "Express Courier";
			this.ExpressCourierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 24, true);
			// 
			// ACEManifestCountrySpecificUserControl
			// 
			this.Controls.Add(this.EstDateAtFirstArrivalDateEdit);
			this.Controls.Add(this.BillStatusTextBox);
			this.Controls.Add(this.BillStatusDescriptionTextBox);
			this.Controls.Add(this.FIRMSTextBox);
			this.Controls.Add(this.ExpressCourierCheckBox);
			this.Name = "ACEManifestCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EstDateAtFirstArrivalDateEdit.ResumeLayout(true);
			this.EstDateAtFirstArrivalDateEdit.PerformLayout();
			this.ExpressCourierCheckBox.ResumeLayout(true);
			this.ExpressCourierCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZDateEdit EstDateAtFirstArrivalDateEdit;
		internal ZTextBox BillStatusTextBox;
		internal ZTextBox BillStatusDescriptionTextBox;
		internal ZTextBox FIRMSTextBox;
		internal ZCheckBox ExpressCourierCheckBox;
	}
}
