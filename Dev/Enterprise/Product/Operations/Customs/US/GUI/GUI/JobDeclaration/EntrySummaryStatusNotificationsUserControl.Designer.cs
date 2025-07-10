namespace Enterprise.Customs.US.GUI
{
	partial class EntrySummaryStatusNotificationsUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StatusNotificationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuotaInformationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StatusSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusNotificationsGrid)).BeginInit();
			this.StatusNotificationsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuotaInformationsGrid)).BeginInit();
			this.QuotaInformationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ErrorsRecordCollection);
			// 
			// StatusNotificationsGrid
			// 
			this.StatusNotificationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StatusNotificationsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).StatusDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).DispositionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).DispositionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).SourceOfActionRequest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).SourceOfActionRequestDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).ActionIDNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).CBPStaffContactDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).ImportSpecialistTeam)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).BlockText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).ActionLogNKUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).ActionLogEventTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).ReferenceOnAction)));
			this.StatusNotificationsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.Caption = "Notification Date";
			zDateEditColumnStyleInfo1.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.Caption = "Disposition Code";
			zTextBoxColumnStyleInfo1.ColumnName = "DispositionCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.Caption = "Disposition Desc";
			zMultiLineTextBoxColumnInfo1.ColumnName = "DispositionDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.Caption = "Action Type";
			zTextBoxColumnStyleInfo2.ColumnName = "SourceOfActionRequest";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Action Type Desc";
			zTextBoxColumnStyleInfo3.ColumnName = "SourceOfActionRequestDesc";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Line No";
			zTextBoxColumnStyleInfo4.ColumnName = "LineNumber";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.Caption = "Action ID #";
			zTextBoxColumnStyleInfo5.ColumnName = "ActionIDNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo6.Caption = "CBP Staff Contact Details";
			zTextBoxColumnStyleInfo6.ColumnName = "CBPStaffContactDetails";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.Caption = "Import Specialist Team";
			zTextBoxColumnStyleInfo7.ColumnName = "ImportSpecialistTeam";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMultiLineTextBoxColumnInfo2.Caption = "Remarks";
			zMultiLineTextBoxColumnInfo2.ColumnName = "BlockText";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo8.Caption = "User";
			zTextBoxColumnStyleInfo8.ColumnName = "ActionLogNKUser";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "Event Time";
			zDateEditColumnStyleInfo2.ColumnName = "ActionLogEventTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo3.Caption = "Reference on Action";
			zMultiLineTextBoxColumnInfo3.ColumnName = "ReferenceOnAction";
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.StatusNotificationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StatusNotificationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.StatusNotificationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.StatusNotificationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.StatusNotificationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.StatusNotificationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.StatusNotificationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusNotificationsGrid.GridId = "44c9e47f-7c25-41f0-ad05-f1252664ee66";
			this.StatusNotificationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StatusNotificationsGrid.LayoutKey = "StatusNotificationsGrid";
			this.StatusNotificationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatusNotificationsGrid.Name = "StatusNotificationsGrid";
			this.StatusNotificationsGrid.ReadOnly = true;
			this.StatusNotificationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 363, true);
			this.StatusNotificationsGrid.TabIndex = 1;
			this.StatusNotificationsGrid.AfterBind += new System.EventHandler(this.StatusNotificationsGrid_AfterBind);
			this.StatusNotificationsGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.StatusNotificationsGrid_ColourDeciding);
			// 
			// QuotaInformationsGrid
			// 
			this.QuotaInformationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuotaInformationsGrid, "QuotaInformations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).LineItemIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).QuotaLineStatusCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).QuotaLineStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).RequestedQuotaQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).RequestedQuotaQuantityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).ReservedQuotaQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.QuotaInformation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ErrorsRecord)(null)).QuotaInformations)).SyncRoot)).ReservedQuotaQuantityUQ)));
			this.QuotaInformationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.Caption = "Line #";
			zTextBoxColumnStyleInfo9.ColumnName = "LineItemIdentifier";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "Quota Status code";
			zTextBoxColumnStyleInfo10.ColumnName = "QuotaLineStatusCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo11.Caption = "Description";
			zTextBoxColumnStyleInfo11.ColumnName = "QuotaLineStatusDescription";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Requested Quota Qty";
			zCalcEditColumnStyleInfo1.ColumnName = "RequestedQuotaQuantity";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("c22f9388-75b8-42ab-8b92-79710efd1ffd", "Requested Quantity");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo12.Caption = "UQ";
			zTextBoxColumnStyleInfo12.ColumnName = "RequestedQuotaQuantityUQ";
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Customs.US.GUI.Res.GetData("c22f9388-75b8-42ab-8b92-79710efd1ffd", "Requested Quantity");
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Reserved Quota Qty";
			zCalcEditColumnStyleInfo2.ColumnName = "ReservedQuotaQuantity";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6d7c6ce6-734e-415b-b0ba-4bcfe6987647", "Reserved Quantity");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo13.Caption = "UQ";
			zTextBoxColumnStyleInfo13.ColumnName = "ReservedQuotaQuantityUQ";
			zTextBoxColumnStyleInfo13.GroupName = Enterprise.Customs.US.GUI.Res.GetData("6d7c6ce6-734e-415b-b0ba-4bcfe6987647", "Reserved Quantity");
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.QuotaInformationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.QuotaInformationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.QuotaInformationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.QuotaInformationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuotaInformationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.QuotaInformationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.QuotaInformationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.QuotaInformationsGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.QuotaInformationsGrid.GridId = "40d51153-9b8b-4b31-b145-902ecc9bd179";
			this.QuotaInformationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuotaInformationsGrid.LayoutKey = "StatusNotificationsGrid";
			this.QuotaInformationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 368, true);
			this.QuotaInformationsGrid.Name = "QuotaInformationsGrid";
			this.QuotaInformationsGrid.ReadOnly = true;
			this.QuotaInformationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 202, true);
			this.QuotaInformationsGrid.TabIndex = 2;
			// 
			// StatusSplitter
			// 
			this.StatusSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.StatusSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 366, true);
			this.StatusSplitter.Name = "StatusSplitter";
			this.StatusSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 2, true);
			this.StatusSplitter.TabIndex = 3;
			this.StatusSplitter.TabStop = false;
			// 
			// EntrySummaryStatusNotificationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.StatusNotificationsGrid);
			this.Controls.Add(this.StatusSplitter);
			this.Controls.Add(this.QuotaInformationsGrid);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Name = "EntrySummaryStatusNotificationsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 573, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StatusNotificationsGrid)).EndInit();
			this.StatusNotificationsGrid.ResumeLayout(false);
			this.StatusNotificationsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuotaInformationsGrid)).EndInit();
			this.QuotaInformationsGrid.ResumeLayout(false);
			this.QuotaInformationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZGrid StatusNotificationsGrid;
		internal ZArchitecture.ZGrid QuotaInformationsGrid;
		private CargoWise.Windows.UI.KSplitter StatusSplitter;

		#endregion
	}
}
