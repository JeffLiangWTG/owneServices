namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCollectionNotesControl
	{

		#region Component Designer generated code

		public Enterprise.ZArchitecture.ZGrid OrgCollectionCallBoundGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CallFilterGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
		private Enterprise.ZArchitecture.ZLabel DateOfCallLabel;
		private Enterprise.ZArchitecture.ZLabel DateFollowUpLabel;
		private Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateNextCallToDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateNextCallFromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateOfCallToDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DateOfCallFromDateEdit;
		//private IContainer components;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CallFilterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateNextCallToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateNextCallFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOfCallToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOfCallFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DateFollowUpLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DateOfCallLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrgCollectionCallBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CallDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CallStatusCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SendEmailButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CallDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FollowUpDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DispositionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.CallDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CallFilterGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgCollectionCallBoundGrid)).BeginInit();
			this.CallDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CallFilterGroupBox
			// 
			this.CallFilterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CallFilterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|8953f6de-00ae-42a8-8fe5-fa7dc9ff91de", "Call Filter");
			this.CallFilterGroupBox.Controls.Add(this.DateNextCallToDateEdit);
			this.CallFilterGroupBox.Controls.Add(this.DateNextCallFromDateEdit);
			this.CallFilterGroupBox.Controls.Add(this.DateOfCallToDateEdit);
			this.CallFilterGroupBox.Controls.Add(this.DateOfCallFromDateEdit);
			this.CallFilterGroupBox.Controls.Add(this.ClearButton);
			this.CallFilterGroupBox.Controls.Add(this.DateFollowUpLabel);
			this.CallFilterGroupBox.Controls.Add(this.DateOfCallLabel);
			this.CallFilterGroupBox.Controls.Add(this.ContactGuidFindBox);
			this.CallFilterGroupBox.Controls.Add(this.FindButton);
			this.CallFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.CallFilterGroupBox.Name = "CallFilterGroupBox";
			this.CallFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 129, true);
			this.CallFilterGroupBox.TabIndex = 0;
			this.CallFilterGroupBox.TabStop = false;
			// 
			// DateNextCallToDateEdit
			// 
			this.DateNextCallToDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateNextCallToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateNextCallToDateEdit, "DateFollowUpTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).DateFollowUpTo)));
			this.DateNextCallToDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|9409e711-7162-429f-abe9-fd24119566ff", "To", "To", "");
			this.DateNextCallToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 42, true);
			this.DateNextCallToDateEdit.Name = "DateNextCallToDateEdit";
			this.DateNextCallToDateEdit.TabIndex = 3;
			// 
			// DateNextCallFromDateEdit
			// 
			this.DateNextCallFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateNextCallFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateNextCallFromDateEdit, "DateFollowUpFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).DateFollowUpFrom)));
			this.DateNextCallFromDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|0e4dfab8-693f-4224-9e40-c465820bf646", "From", "From", "");
			this.DateNextCallFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 42, true);
			this.DateNextCallFromDateEdit.Name = "DateNextCallFromDateEdit";
			this.DateNextCallFromDateEdit.TabIndex = 2;
			// 
			// DateOfCallToDateEdit
			// 
			this.DateOfCallToDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfCallToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfCallToDateEdit, "DateOfCallNoteTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).DateOfCallNoteTo)));
			this.DateOfCallToDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|6bf0441d-fa25-4074-a1bd-2dc1e4bcc062", "To");
			this.DateOfCallToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 18, true);
			this.DateOfCallToDateEdit.Name = "DateOfCallToDateEdit";
			this.DateOfCallToDateEdit.TabIndex = 1;
			// 
			// DateOfCallFromDateEdit
			// 
			this.DateOfCallFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfCallFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfCallFromDateEdit, "DateOfCallNoteFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).DateOfCallNoteFrom)));
			this.DateOfCallFromDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|58ad20ed-7d4d-4598-b0be-5dbe635c7b37", "From");
			this.DateOfCallFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 18, true);
			this.DateOfCallFromDateEdit.Name = "DateOfCallFromDateEdit";
			this.DateOfCallFromDateEdit.TabIndex = 0;
			// 
			// ClearButton
			// 
			this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|505d3b5a-7e00-4b15-9d52-5d073ac89429", "&Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 102, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// DateFollowUpLabel
			// 
			this.DateFollowUpLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|ffc1191c-c816-48be-8374-d0872492737d", "Date Of Follow Up:");
			this.DateFollowUpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.DateFollowUpLabel.Name = "DateFollowUpLabel";
			this.DateFollowUpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DateFollowUpLabel.TabIndex = 5;
			// 
			// DateOfCallLabel
			// 
			this.DateOfCallLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|8c37ec76-290e-46db-8f9e-46465bf00705", "Date Of Call:");
			this.DateOfCallLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18, true);
			this.DateOfCallLabel.Name = "DateOfCallLabel";
			this.DateOfCallLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.DateOfCallLabel.TabIndex = 0;
			// 
			// ContactGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ContactGuidFindBox, "CallNoteContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CallNoteContact)));
			this.ContactGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|473e33d2-677f-4135-a1ce-cb24d7c23313", "Contact");
			this.ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 72, true);
			this.ContactGuidFindBox.Name = "ContactGuidFindBox";
			this.ContactGuidFindBox.PreBoundMaxLength = 15;
			this.ContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.ContactGuidFindBox.TabIndex = 4;
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|022373f4-ad3d-49ac-a621-d218ab2facb0", "&Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 72, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.FindButton.TabIndex = 5;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// OrgCollectionCallBoundGrid
			// 
			this.OrgCollectionCallBoundGrid.AllowNavigation = false;
			this.OrgCollectionCallBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgCollectionCallBoundGrid, "CollectionNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_CallDisposition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_OC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_CallBackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_AmountOverdueAtCallTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_TotalOutstandingValueAtCallTime)));
			this.OrgCollectionCallBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PN_SystemCreateUser";
			zDropEditColumnStyleInfo1.ColumnName = "PN_Status";
			zDropEditColumnStyleInfo1.ToolTip = "Status";
			zDropEditColumnStyleInfo2.ColumnName = "PN_CallDisposition";
			zDropEditColumnStyleInfo2.ToolTip = "Disposition";
			zGuidDropEditColumnStyleInfo1.ColumnName = "PN_OC";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|e47b442e-1208-4c0e-814d-40e0ae3e28e4", "Call Date");
			zDateEditColumnStyleInfo1.ColumnName = "PN_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|f0dc1ad1-c2a8-48cc-a2ce-abc4fd917f2a", "Follow Up");
			zDateEditColumnStyleInfo2.ColumnName = "PN_CallBackDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|d4ec8293-c666-43da-8120-d8ec25f78ade", "Amount Overdue");
			zCalcEditColumnStyleInfo1.ColumnName = "PN_AmountOverdueAtCallTime";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|f5a3dd22-33c6-4e83-a5c2-a1eaede0ac3d", "Outstanding Value");
			zCalcEditColumnStyleInfo2.ColumnName = "PN_TotalOutstandingValueAtCallTime";
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrgCollectionCallBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrgCollectionCallBoundGrid.GridId = "9e162d3b-550f-4c1a-8cd8-a47b75e04754";
			this.OrgCollectionCallBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgCollectionCallBoundGrid.LayoutKey = "OrgCollectionCallBoundGrid";
			this.OrgCollectionCallBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 135, true);
			this.OrgCollectionCallBoundGrid.Name = "OrgCollectionCallBoundGrid";
			this.OrgCollectionCallBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 156, true);
			this.OrgCollectionCallBoundGrid.TabIndex = 0;
			this.OrgCollectionCallBoundGrid.AfterBind += new System.EventHandler(this.OrgCollectionCallBoundGrid_AfterBind);
			// 
			// CallDetailsGroupBox
			// 
			this.CallDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CallDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|9a475000-a000-42d2-9cc4-ee73311e622a", "Call Details");
			this.CallDetailsGroupBox.Controls.Add(this.CallStatusCodeFindBox);
			this.CallDetailsGroupBox.Controls.Add(this.SendEmailButton);
			this.CallDetailsGroupBox.Controls.Add(this.CallDateDateEdit);
			this.CallDetailsGroupBox.Controls.Add(this.PhoneNumberTextBox);
			this.CallDetailsGroupBox.Controls.Add(this.FollowUpDateEdit);
			this.CallDetailsGroupBox.Controls.Add(this.DispositionDropEdit);
			this.CallDetailsGroupBox.Controls.Add(this.StatusDropEdit);
			this.CallDetailsGroupBox.Controls.Add(this.ContactDropEdit);
			this.CallDetailsGroupBox.Controls.Add(this.CallDetailTextBox);
			this.CallDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 295, true);
			this.CallDetailsGroupBox.Name = "CallDetailsGroupBox";
			this.CallDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 227, true);
			this.CallDetailsGroupBox.TabIndex = 1;
			this.CallDetailsGroupBox.TabStop = false;
			// 
			// CallStatusCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CallStatusCodeFindBox, "CollectionNotes.PN_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_SystemCreateUser)));
			this.CallStatusCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 44, true);
			this.CallStatusCodeFindBox.Name = "CallStatusCodeFindBox";
			this.CallStatusCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.CallStatusCodeFindBox.TabIndex = 2;
			// 
			// SendEmailButton
			// 
			this.SendEmailButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|20f0123d-854a-4ef4-9925-6365d4689fae", "Send Email");
			this.SendEmailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 148, true);
			this.SendEmailButton.Name = "SendEmailButton";
			this.SendEmailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 22, true);
			this.SendEmailButton.TabIndex = 7;
			this.SendEmailButton.UseVisualStyleBackColor = true;
			this.SendEmailButton.Click += new System.EventHandler(this.SendEmailButton_Click);
			// 
			// CallDateDateEdit
			// 
			this.CallDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CallDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CallDateDateEdit, "CollectionNotes.PN_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_SystemCreateTimeUtc)));
			this.CallDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CallDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 18, true);
			this.CallDateDateEdit.Name = "CallDateDateEdit";
			this.CallDateDateEdit.TabIndex = 0;
			// 
			// PhoneNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneNumberTextBox, "CollectionNotes.TelephoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).TelephoneNumber)));
			this.PhoneNumberTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCollectionNotesControl|8d4dae60-2813-4dd0-9a5c-1bb49bafad96", "Phone");
			this.PhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 148, true);
			this.PhoneNumberTextBox.Name = "PhoneNumberTextBox";
			this.PhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 20, true);
			this.PhoneNumberTextBox.TabIndex = 6;
			// 
			// FollowUpDateEdit
			// 
			this.FollowUpDateEdit.AutoCompleteMonthThreshold = 1;
			this.FollowUpDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FollowUpDateEdit, "CollectionNotes.PN_CallBackDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_CallBackDate)));
			this.FollowUpDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FollowUpDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 18, true);
			this.FollowUpDateEdit.Name = "FollowUpDateEdit";
			this.FollowUpDateEdit.TabIndex = 1;
			// 
			// DispositionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DispositionDropEdit, "CollectionNotes.PN_CallDisposition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_CallDisposition)));
			this.DispositionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 96, true);
			this.DispositionDropEdit.Name = "DispositionDropEdit";
			this.DispositionDropEdit.PreBoundMaxLength = 3;
			this.DispositionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.DispositionDropEdit.TabIndex = 4;
			// 
			// StatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "CollectionNotes.PN_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 70, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.StatusDropEdit.TabIndex = 3;
			// 
			// ContactDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ContactDropEdit, "CollectionNotes.PN_OC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_OC)));
			this.ContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 122, true);
			this.ContactDropEdit.Name = "ContactDropEdit";
			this.ContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.ContactDropEdit.TabIndex = 5;
			// 
			// CallDetailTextBox
			// 
			this.CallDetailTextBox.AcceptsReturn = true;
			this.CallDetailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CallDetailTextBox, "CollectionNotes.PN_CallDetailNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCollectionNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CollectionNotes)).SyncRoot)).PN_CallDetailNote)));
			this.CallDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 19, true);
			this.CallDetailTextBox.Multiline = true;
			this.CallDetailTextBox.Name = "CallDetailTextBox";
			this.CallDetailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CallDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 202, true);
			this.CallDetailTextBox.TabIndex = 8;
			// 
			// OrgCollectionNotesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CallFilterGroupBox);
			this.Controls.Add(this.CallDetailsGroupBox);
			this.Controls.Add(this.OrgCollectionCallBoundGrid);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 460, true);
			this.Name = "OrgCollectionNotesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CallFilterGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrgCollectionCallBoundGrid)).EndInit();
			this.CallDetailsGroupBox.ResumeLayout(false);
			this.CallDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
