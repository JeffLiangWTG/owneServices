namespace Enterprise.Customs.US.AMS.GUI
{
	partial class StowPlanUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DepartureCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IsDepartureTimeEstimatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IsArrivalTimeEstimatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IssueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IssueSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GoToErrorButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IssueFilterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.issuesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BillsAndIssuesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.stowPlanSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IssueGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IssueSplitContainer)).BeginInit();
			this.IssueSplitContainer.Panel1.SuspendLayout();
			this.IssueSplitContainer.Panel2.SuspendLayout();
			this.IssueSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).BeginInit();
			this.BillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BillsAndIssuesSplitContainer)).BeginInit();
			this.BillsAndIssuesSplitContainer.Panel1.SuspendLayout();
			this.BillsAndIssuesSplitContainer.Panel2.SuspendLayout();
			this.BillsAndIssuesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.stowPlanSplitContainer)).BeginInit();
			this.stowPlanSplitContainer.Panel1.SuspendLayout();
			this.stowPlanSplitContainer.Panel2.SuspendLayout();
			this.stowPlanSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.StowPlanSailingData);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("17fd9735-3b48-4df1-8e12-20942f761e8a", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 27, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.CancelButton.TabIndex = 7;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("0ac83217-2c07-40bb-8bc9-a2c76f05a613", "Submit to US Customs");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 2, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.SendButton.TabIndex = 6;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// PortDropEdit
			// 
			this.PortDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortDropEdit, "Arrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Arrival)));
			this.PortDropEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("9cec3677-f50a-4bf7-9fdf-5a67bb63f756", "US Arrival Port");
			this.PortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 3, true);
			this.PortDropEdit.Name = "PortDropEdit";
			this.PortDropEdit.ShowDescriptionBox = false;
			this.PortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PortDropEdit.TabIndex = 0;
			// 
			// DepartureCodeFindBox
			// 
			this.DepartureCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureCodeFindBox, "Departure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Departure)));
			this.DepartureCodeFindBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("4cb17e02-9371-4732-88ac-e5522b3c063f", "Last Foreign Port");
			this.DepartureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 29, true);
			this.DepartureCodeFindBox.Name = "DepartureCodeFindBox";
			this.DepartureCodeFindBox.ShowDescriptionBox = false;
			this.DepartureCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.DepartureCodeFindBox.TabIndex = 3;
			// 
			// IsDepartureTimeEstimatedCheckBox
			// 
			this.IsDepartureTimeEstimatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDepartureTimeEstimatedCheckBox, "IsDepartureTimeEstimated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IsDepartureTimeEstimated)));
			this.IsDepartureTimeEstimatedCheckBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("5de9971d-dbc6-4d39-8230-601d407f1d58", "Estimated?");
			this.IsDepartureTimeEstimatedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsDepartureTimeEstimatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDepartureTimeEstimatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 31, true);
			this.IsDepartureTimeEstimatedCheckBox.Name = "IsDepartureTimeEstimatedCheckBox";
			this.IsDepartureTimeEstimatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.IsDepartureTimeEstimatedCheckBox.TabIndex = 5;
			this.IsDepartureTimeEstimatedCheckBox.Text = "Estimated?";
			this.IsDepartureTimeEstimatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// DepartureDateEdit
			// 
			this.DepartureDateEdit.AllowDrop = true;
			this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateEdit, "DepartureTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).DepartureTime)));
			this.DepartureDateEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("d3c3e376-0b51-4d50-86e5-cb4796468945", "Departure Date");
			this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 29, true);
			this.DepartureDateEdit.Name = "DepartureDateEdit";
			this.DepartureDateEdit.TabIndex = 4;
			// 
			// IsArrivalTimeEstimatedCheckBox
			// 
			this.IsArrivalTimeEstimatedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsArrivalTimeEstimatedCheckBox, "IsArrivalTimeEstimated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IsArrivalTimeEstimated)));
			this.IsArrivalTimeEstimatedCheckBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("216f095e-8307-48fa-824b-bc802bd43f92", "Estimated?");
			this.IsArrivalTimeEstimatedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsArrivalTimeEstimatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsArrivalTimeEstimatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 5, true);
			this.IsArrivalTimeEstimatedCheckBox.Name = "IsArrivalTimeEstimatedCheckBox";
			this.IsArrivalTimeEstimatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.IsArrivalTimeEstimatedCheckBox.TabIndex = 2;
			this.IsArrivalTimeEstimatedCheckBox.Text = "Estimated?";
			this.IsArrivalTimeEstimatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "ArrivalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).ArrivalTime)));
			this.ArrivalDateEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("334400c7-726a-438d-9e2c-831b10894f6a", "Arrival Date");
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 4, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 1;
			// 
			// IssueGroupBox
			// 
			this.IssueGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("33ff13ce-315e-4387-ba86-abdf8369a92f", "Issues");
			this.IssueGroupBox.Controls.Add(this.IssueSplitContainer);
			this.IssueGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IssueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IssueGroupBox.Name = "IssueGroupBox";
			this.IssueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 189, true);
			this.IssueGroupBox.TabIndex = 2;
			this.IssueGroupBox.TabStop = false;
			// 
			// IssueSplitContainer
			// 
			this.IssueSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IssueSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.IssueSplitContainer.IsSplitterFixed = true;
			this.IssueSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IssueSplitContainer.Name = "IssueSplitContainer";
			this.IssueSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// IssueSplitContainer.Panel1
			// 
			this.IssueSplitContainer.Panel1.Controls.Add(this.GoToErrorButton);
			this.IssueSplitContainer.Panel1.Controls.Add(this.IssueFilterDropEdit);
			// 
			// IssueSplitContainer.Panel2
			// 
			this.IssueSplitContainer.Panel2.Controls.Add(this.issuesGrid);
			this.IssueSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 170, true);
			this.IssueSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			this.IssueSplitContainer.TabIndex = 1;
			// 
			// GoToErrorButton
			// 
			this.GoToErrorButton.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("2372d12c-3a62-4d1f-9bdf-d473df0669e6", "Go To Selected Error");
			this.GoToErrorButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 1, true);
			this.GoToErrorButton.Name = "GoToErrorButton";
			this.GoToErrorButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.GoToErrorButton.TabIndex = 1;
			this.GoToErrorButton.UseVisualStyleBackColor = true;
			this.GoToErrorButton.Click += new System.EventHandler(this.GoToErrorButton_Click);
			// 
			// IssueFilterDropEdit
			// 
			this.IssueFilterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssueFilterDropEdit, "IssueFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IssueFilter)));
			this.IssueFilterDropEdit.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("cfb4bfcd-5496-4a45-9f76-107a82b2ac20", "Issue Filter");
			this.IssueFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 3, true);
			this.IssueFilterDropEdit.Name = "IssueFilterDropEdit";
			this.IssueFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 20, true);
			this.IssueFilterDropEdit.TabIndex = 0;
			// 
			// issuesGrid
			// 
			this.issuesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.issuesGrid, "IssueCollectionView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IssueCollectionView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanMessageIssue)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IssueCollectionView)).SyncRoot)).ErrorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanMessageIssue)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IssueCollectionView)).SyncRoot)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanMessageIssue)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).IssueCollectionView)).SyncRoot)).Detail)));
			this.issuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("e3f1ee2a-6dd9-40c5-ae3e-4fb96bcfd553", "Error Type");
			zTextBoxColumnStyleInfo1.ColumnName = "ErrorType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("631d3972-c398-411a-87db-0acd4608af66", "Text");
			zTextBoxColumnStyleInfo2.ColumnName = "Text";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("16b668ff-d6ce-4dee-9430-b065c263ae27", "Detail");
			zTextBoxColumnStyleInfo3.ColumnName = "Detail";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			this.issuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.issuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.issuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.issuesGrid.CopySelectedRowsAllowed = true;
			this.issuesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.issuesGrid.GridId = "168a9514-c7dd-483a-a6e9-9fd782dec53c";
			this.issuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.issuesGrid.IsWholeRowSelectedOnClick = true;
			this.issuesGrid.LayoutKey = "issuesGrid";
			this.issuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.issuesGrid.Name = "issuesGrid";
			this.issuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 139, true);
			this.issuesGrid.TabIndex = 0;
			this.issuesGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.issuesGrid_MouseClick);
			this.issuesGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.issuesGrid_MouseDoubleClick);
			// 
			// BillsGroupBox
			// 
			this.BillsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("854e941b-0d78-46bc-8c33-22c8c2b41de6", "Bills of Lading To Be Sent");
			this.BillsGroupBox.Controls.Add(this.BillsGrid);
			this.BillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsGroupBox.Name = "BillsGroupBox";
			this.BillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 188, true);
			this.BillsGroupBox.TabIndex = 3;
			this.BillsGroupBox.TabStop = false;
			this.BillsGroupBox.Text = "Bills of Lading To Be Sent";
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "Shipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).Checked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JS_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JS_PackingMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).PortOfLading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JX_JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).JX_JB_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).ConsignorCompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.StowPlanShipmentData)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.StowPlanSailingData)(null)).Shipments)).SyncRoot)).ConsingeeCompanyName)));
			this.BillsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("1f6353e3-810d-429d-bd8d-c40abd0275df", "Send?");
			zCheckBoxColumnStyleInfo1.ColumnName = "Checked";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("cb2d6997-db90-484b-8f01-add786cad631", "Bill of Lading");
			zTextBoxColumnStyleInfo4.ColumnName = "JS_HouseBill";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("5589632a-7167-438b-b3f7-6d1c81a68acd", "Cargo Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_PackingMode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("087a5f08-4c48-44ba-89e3-70e7b9ec36f5", "Origin");
			zTextBoxColumnStyleInfo6.ColumnName = "PortOfLading";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("97c2b574-a8b9-4e7b-8f00-ea1a251a04c3", "Destination");
			zTextBoxColumnStyleInfo7.ColumnName = "PortOfDischarge";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("c2f1b8ee-8f6e-4bb2-9974-c0024547b705", "Load");
			zTextBoxColumnStyleInfo8.ColumnName = "JX_JA_RL_NKPortOfLoading";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("c36e64eb-66a1-426f-93d7-d330b5357a9e", "Departure Date");
			zDateEditColumnStyleInfo1.ColumnName = "JX_JA_E_DEP";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("38239e1e-b4d1-445a-be36-64a2e1aed88b", "Discharge");
			zTextBoxColumnStyleInfo9.ColumnName = "JX_JB_RL_NKPortOfDischarge";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("1cae2d72-e05a-4c3a-a873-b9b0ca9a152e", "Arrival Date");
			zDateEditColumnStyleInfo2.ColumnName = "JX_JB_E_ARV";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("972bc442-d712-477e-b639-d19390d06b4b", "Consignor");
			zTextBoxColumnStyleInfo10.ColumnName = "ConsignorCompanyName";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("6abd79fd-af36-4f31-bad2-31fbe139f02d", "Consignee");
			zTextBoxColumnStyleInfo11.ColumnName = "ConsingeeCompanyName";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(195);
			this.BillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.BillsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.BillsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.BillsGrid.CopySelectedRowsAllowed = true;
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "24f8e74f-e129-4070-83f5-53384d35285a";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.IsWholeRowSelectedOnClick = false;
			this.BillsGrid.LayoutKey = "BillsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1090, 169, true);
			this.BillsGrid.TabIndex = 0;
			this.BillsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.BillsGrid_MouseDoubleClick);
			// 
			// BillsAndIssuesSplitContainer
			// 
			this.BillsAndIssuesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsAndIssuesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsAndIssuesSplitContainer.Name = "BillsAndIssuesSplitContainer";
			this.BillsAndIssuesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// BillsAndIssuesSplitContainer.Panel1
			// 
			this.BillsAndIssuesSplitContainer.Panel1.Controls.Add(this.IssueGroupBox);
			// 
			// BillsAndIssuesSplitContainer.Panel2
			// 
			this.BillsAndIssuesSplitContainer.Panel2.Controls.Add(this.BillsGroupBox);
			this.BillsAndIssuesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 381, true);
			this.BillsAndIssuesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(189);
			this.BillsAndIssuesSplitContainer.TabIndex = 0;
			// 
			// stowPlanSplitContainer
			// 
			this.stowPlanSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stowPlanSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.stowPlanSplitContainer.IsSplitterFixed = true;
			this.stowPlanSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.stowPlanSplitContainer.Name = "stowPlanSplitContainer";
			this.stowPlanSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// stowPlanSplitContainer.Panel1
			// 
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.DepartureCodeFindBox);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.PortDropEdit);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.DepartureDateEdit);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.ArrivalDateEdit);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.CancelButton);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.IsDepartureTimeEstimatedCheckBox);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.SendButton);
			this.stowPlanSplitContainer.Panel1.Controls.Add(this.IsArrivalTimeEstimatedCheckBox);
			// 
			// stowPlanSplitContainer.Panel2
			// 
			this.stowPlanSplitContainer.Panel2.Controls.Add(this.BillsAndIssuesSplitContainer);
			this.stowPlanSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 441, true);
			this.stowPlanSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			this.stowPlanSplitContainer.TabIndex = 8;
			// 
			// StowPlanUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.stowPlanSplitContainer);
			this.Name = "StowPlanUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 441, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IssueGroupBox.ResumeLayout(false);
			this.IssueSplitContainer.Panel1.ResumeLayout(false);
			this.IssueSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.IssueSplitContainer)).EndInit();
			this.IssueSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.issuesGrid)).EndInit();
			this.BillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsAndIssuesSplitContainer.Panel1.ResumeLayout(false);
			this.BillsAndIssuesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BillsAndIssuesSplitContainer)).EndInit();
			this.BillsAndIssuesSplitContainer.ResumeLayout(false);
			this.stowPlanSplitContainer.Panel1.ResumeLayout(false);
			this.stowPlanSplitContainer.Panel1.PerformLayout();
			this.stowPlanSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.stowPlanSplitContainer)).EndInit();
			this.stowPlanSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit PortDropEdit;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZCheckBox IsArrivalTimeEstimatedCheckBox;
		private ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		private ZArchitecture.GUI.ZCheckBox IsDepartureTimeEstimatedCheckBox;
		private ZArchitecture.GUI.ZDateEdit DepartureDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox DepartureCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox IssueGroupBox;
		private CargoWise.Windows.UI.KSplitContainer IssueSplitContainer;
		private ZArchitecture.GUI.ZButton GoToErrorButton;
		private ZArchitecture.GUI.ZDropEdit IssueFilterDropEdit;
		internal ZArchitecture.ZGrid issuesGrid;
		private ZArchitecture.GUI.ZGroupBox BillsGroupBox;
		private ZArchitecture.ZGrid BillsGrid;
		private CargoWise.Windows.UI.KSplitContainer BillsAndIssuesSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer stowPlanSplitContainer;
	}
}
