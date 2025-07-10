using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class ODSAndTSCAControl
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
			this.EPAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateEditTSCAStatusDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateEditODSStatusDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.txtTscaLineStatus = new Enterprise.ZArchitecture.ZTextBox();
			this.txtODSLineStatus = new Enterprise.ZArchitecture.ZTextBox();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.US_TSCATrackingStatusDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_ODSTrackingStatusDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContactPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PGAContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_TSCAODSCertIndividualDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ODSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TSCAIndicator = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EPAGroupBox.SuspendLayout();
			this.DateEditTSCAStatusDate.SuspendLayout();
			this.DateEditODSStatusDate.SuspendLayout();
			this.US_TSCAODSCertIndividualDropEdit.SuspendLayout();
			this.TSCAIndicator.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// EPAGroupBox
			// 
			this.EPAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("878dead0-6846-4f1d-95aa-a5e7356766c1", "Environmental Protection Agency");
			this.EPAGroupBox.Controls.Add(this.DateEditTSCAStatusDate);
			this.EPAGroupBox.Controls.Add(this.DateEditODSStatusDate);
			this.EPAGroupBox.Controls.Add(this.txtTscaLineStatus);
			this.EPAGroupBox.Controls.Add(this.txtODSLineStatus);
			this.EPAGroupBox.Controls.Add(this.DeleteButton);
			this.EPAGroupBox.Controls.Add(this.UpdateButton);
			this.EPAGroupBox.Controls.Add(this.US_TSCATrackingStatusDescTextBox);
			this.EPAGroupBox.Controls.Add(this.US_ODSTrackingStatusDescTextBox);
			this.EPAGroupBox.Controls.Add(this.PGAContactEmailTextBox);
			this.EPAGroupBox.Controls.Add(this.PGAContactPhoneTextBox);
			this.EPAGroupBox.Controls.Add(this.PGAContactNameTextBox);
			this.EPAGroupBox.Controls.Add(this.US_TSCAODSCertIndividualDropEdit);
			this.EPAGroupBox.Controls.Add(this.zCheckBox1);
			this.EPAGroupBox.Controls.Add(this.ODSCheckBox);
			this.EPAGroupBox.Controls.Add(this.TSCAIndicator);
			this.EPAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EPAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EPAGroupBox.Name = "EPAGroupBox";
			this.EPAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 302, true);
			this.EPAGroupBox.TabIndex = 0;
			this.EPAGroupBox.TabStop = false;
			this.EPAGroupBox.Text = "Environmental Protection Agency - ODS/TSCA";
			// 
			// DateEditTSCAStatusDate
			// 
			this.DateEditTSCAStatusDate.AllowDrop = true;
			this.DateEditTSCAStatusDate.AutoCompleteMonthThreshold = 1;
			this.DateEditTSCAStatusDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditTSCAStatusDate, "TSCAStatusDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).TSCAStatusDate)));
			this.DateEditTSCAStatusDate.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("985CD3C3-F01C-4A39-A66F-48270134147C", "PGA Line Status Date");
			this.DateEditTSCAStatusDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateEditTSCAStatusDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 47, true);
			this.DateEditTSCAStatusDate.Name = "DateEditTSCAStatusDate";
			this.DateEditTSCAStatusDate.TabIndex = 14;
			// 
			// DateEditODSStatusDate
			// 
			this.DateEditODSStatusDate.AllowDrop = true;
			this.DateEditODSStatusDate.AutoCompleteMonthThreshold = 1;
			this.DateEditODSStatusDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditODSStatusDate, "ODSStatusDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).ODSStatusDate)));
			this.DateEditODSStatusDate.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9E1C3DB0-1F38-4E03-B97B-FF25A97CC0BC", "PGA Line Status Date");
			this.DateEditODSStatusDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateEditODSStatusDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 21, true);
			this.DateEditODSStatusDate.Name = "DateEditODSStatusDate";
			this.DateEditODSStatusDate.TabIndex = 13;
			// 
			// txtTscaLineStatus
			// 
			this.BindingSource.SetBindingMember(this.txtTscaLineStatus, "TSCAStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).TSCAStatusDesc)));
			this.txtTscaLineStatus.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d73dd6f6-228b-4dd0-9cd1-bc52831efece", "PGA Line Status");
			this.txtTscaLineStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 49, true);
			this.txtTscaLineStatus.Name = "txtTscaLineStatus";
			this.txtTscaLineStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.txtTscaLineStatus.TabIndex = 12;
			// 
			// txtODSLineStatus
			// 
			this.BindingSource.SetBindingMember(this.txtODSLineStatus, "ODSStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).ODSStatusDesc)));
			this.txtODSLineStatus.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("01a518bf-2ff5-47dc-994f-14c11421f36b", "PGA Line Status");
			this.txtODSLineStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 21, true);
			this.txtODSLineStatus.Name = "txtODSLineStatus";
			this.txtODSLineStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.txtODSLineStatus.TabIndex = 11;
			// 
			// DeleteButton
			// 
			this.DeleteButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("61144a87-a8f1-488c-9098-cdc667908be5", "&Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 207, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteButton.TabIndex = 10;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// UpdateButton
			// 
			this.UpdateButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4ca006b4-a7f9-4c3e-80b1-d6d75a732b63", "&Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 207, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 9;
			this.UpdateButton.UseVisualStyleBackColor = true;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// US_TSCATrackingStatusDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_TSCATrackingStatusDescTextBox, "US_TSCATrackingStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_TSCATrackingStatusDesc)));
			this.US_TSCATrackingStatusDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("65d11f6b-f63b-4c22-8125-09c557bfb8ad", "Msg. Status");
			this.US_TSCATrackingStatusDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 49, true);
			this.US_TSCATrackingStatusDescTextBox.Name = "US_TSCATrackingStatusDescTextBox";
			this.US_TSCATrackingStatusDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.US_TSCATrackingStatusDescTextBox.TabIndex = 3;
			// 
			// US_ODSTrackingStatusDescTextBox
			// 
			this.BindingSource.SetBindingMember(this.US_ODSTrackingStatusDescTextBox, "US_ODSTrackingStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_ODSTrackingStatusDesc)));
			this.US_ODSTrackingStatusDescTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b2805ccd-3a53-4b63-9cbd-8f07c56ae758", "Msg. Status");
			this.US_ODSTrackingStatusDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 21, true);
			this.US_ODSTrackingStatusDescTextBox.Name = "US_ODSTrackingStatusDescTextBox";
			this.US_ODSTrackingStatusDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.US_ODSTrackingStatusDescTextBox.TabIndex = 1;
			// 
			// PGAContactEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContactEmailTextBox, "US_FDAContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_FDAContactEmail)));
			this.PGAContactEmailTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("480dd026-628b-47c1-a6b1-26956bc828bb", "PGA Contact Email");
			this.PGAContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 181, true);
			this.PGAContactEmailTextBox.Name = "PGAContactEmailTextBox";
			this.PGAContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.PGAContactEmailTextBox.TabIndex = 8;
			// 
			// PGAContactPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContactPhoneTextBox, "US_FDAContactPhoneNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_FDAContactPhoneNo)));
			this.PGAContactPhoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2875e056-7e8a-442e-9432-af5ff216f3af", "PGA Contact Phone");
			this.PGAContactPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 155, true);
			this.PGAContactPhoneTextBox.Name = "PGAContactPhoneTextBox";
			this.PGAContactPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.PGAContactPhoneTextBox.TabIndex = 7;
			// 
			// PGAContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.PGAContactNameTextBox, "US_FDAContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_FDAContactName)));
			this.PGAContactNameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1fcfd040-7093-4e07-86e2-d5e3a8e68664", "PGA Contact Name");
			this.PGAContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 129, true);
			this.PGAContactNameTextBox.Name = "PGAContactNameTextBox";
			this.PGAContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.PGAContactNameTextBox.TabIndex = 6;
			// 
			// US_TSCAODSCertIndividualDropEdit
			// 
			this.US_TSCAODSCertIndividualDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_TSCAODSCertIndividualDropEdit, "US_TSCAODSCertIndividual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_TSCAODSCertIndividual)));
			this.US_TSCAODSCertIndividualDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e12a4623-a588-4e4c-9e13-621458f3ffc8", "Certifying Individual");
			this.US_TSCAODSCertIndividualDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 103, true);
			this.US_TSCAODSCertIndividualDropEdit.Name = "US_TSCAODSCertIndividualDropEdit";
			this.US_TSCAODSCertIndividualDropEdit.PreBoundMaxLength = 2;
			this.US_TSCAODSCertIndividualDropEdit.ShowDescriptionBox = false;
			this.US_TSCAODSCertIndividualDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.US_TSCAODSCertIndividualDropEdit.TabIndex = 5;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "IsTSCAIndBeDeclared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).IsTSCAIndBeDeclared)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e314a948-2f19-4271-9f6a-53b2d74bea81", "Toxic Substances Control Act");
			this.zCheckBox1.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 24, true);
			this.zCheckBox1.TabIndex = 2;
			this.zCheckBox1.Text = "Toxic Substances Control Act";
			this.zCheckBox1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// ODSCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ODSCheckBox, "IsODSIndBeDeclared");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).IsODSIndBeDeclared)));
			this.ODSCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("96ec55a6-701e-4367-b8ab-de86110406ed", "Ozone Depleting Substances ");
			this.ODSCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ODSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ODSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ODSCheckBox.Name = "ODSCheckBox";
			this.ODSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 24, true);
			this.ODSCheckBox.TabIndex = 0;
			this.ODSCheckBox.Text = "Ozone Depleting Substances";
			this.ODSCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ODSCheckBox.UseVisualStyleBackColor = true;
			// 
			// TSCAIndicator
			// 
			this.TSCAIndicator.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TSCAIndicator, "US_TSCACertification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).US_TSCACertification)));
			this.TSCAIndicator.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a12aa07c-878c-4654-8f04-9c3562c6bad6", "TSCA Cert. Indicator");
			this.TSCAIndicator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 77, true);
			this.TSCAIndicator.Name = "TSCAIndicator";
			this.TSCAIndicator.PreBoundMaxLength = 2;
			this.TSCAIndicator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 20, true);
			this.TSCAIndicator.TabIndex = 4;
			// 
			// ODSAndTSCAControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EPAGroupBox);
			this.Name = "ODSAndTSCAControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EPAGroupBox.ResumeLayout(false);
			this.EPAGroupBox.PerformLayout();
			this.DateEditTSCAStatusDate.ResumeLayout(true);
			this.DateEditTSCAStatusDate.PerformLayout();
			this.DateEditODSStatusDate.ResumeLayout(true);
			this.DateEditODSStatusDate.PerformLayout();
			this.US_TSCAODSCertIndividualDropEdit.ResumeLayout(true);
			this.US_TSCAODSCertIndividualDropEdit.PerformLayout();
			this.TSCAIndicator.ResumeLayout(true);
			this.TSCAIndicator.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox EPAGroupBox;
		private ZDropEdit TSCAIndicator;
		private ZCheckBox ODSCheckBox;
		private ZCheckBox zCheckBox1;
		private ZDropEdit US_TSCAODSCertIndividualDropEdit;
		private ZArchitecture.ZTextBox PGAContactNameTextBox;
		private ZArchitecture.ZTextBox PGAContactPhoneTextBox;
		private ZArchitecture.ZTextBox PGAContactEmailTextBox;
		private ZArchitecture.ZTextBox US_TSCATrackingStatusDescTextBox;
		private ZArchitecture.ZTextBox US_ODSTrackingStatusDescTextBox;
		internal ZButton DeleteButton;
		internal ZButton UpdateButton;
		private ZArchitecture.ZTextBox txtODSLineStatus;
		private ZArchitecture.ZTextBox txtTscaLineStatus;
		private ZDateEdit DateEditODSStatusDate;
		private ZDateEdit DateEditTSCAStatusDate;
	}
}
