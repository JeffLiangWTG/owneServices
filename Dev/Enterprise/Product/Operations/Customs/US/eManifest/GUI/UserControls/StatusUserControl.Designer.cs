
namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class StatusUserControl
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
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MsgStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AcceptedDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompleteEManifestLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompleteEManifestDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CompleteEManifestStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnassociatedShipmentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnassociatedShipmentsDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UnassociatedShipmentsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreliminaryTripDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PreliminaryTripDetailsDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PreliminaryTripDetailsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CrewPassengersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CrewPassengersDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CrewPassengersStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompleteTripDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CompleteTripDetailsDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CompleteTripDetailsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CancelTripAndLinkedShipmentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelTripAndLinkedShipmentsDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CancelTripAndLinkedShipmentsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegisterCrewInformationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RegisterCrewInformationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RegisterCrewInformationStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.CompleteEManifestDateEdit.SuspendLayout();
			this.UnassociatedShipmentsDateEdit.SuspendLayout();
			this.PreliminaryTripDetailsDateEdit.SuspendLayout();
			this.CrewPassengersDateEdit.SuspendLayout();
			this.CompleteTripDetailsDateEdit.SuspendLayout();
			this.CancelTripAndLinkedShipmentsDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Trip);
			// 
			// StatusGroupBox
			//
			this.StatusGroupBox.Controls.Add(this.MsgStatusLabel);
			this.StatusGroupBox.Controls.Add(this.AcceptedDateLabel);
			this.StatusGroupBox.Controls.Add(this.TypeLabel);
			this.StatusGroupBox.Controls.Add(this.CompleteEManifestLabel);
			this.StatusGroupBox.Controls.Add(this.CompleteEManifestDateEdit);
			this.StatusGroupBox.Controls.Add(this.CompleteEManifestStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.UnassociatedShipmentsLabel);
			this.StatusGroupBox.Controls.Add(this.UnassociatedShipmentsDateEdit);
			this.StatusGroupBox.Controls.Add(this.UnassociatedShipmentsStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.PreliminaryTripDetailsLabel);
			this.StatusGroupBox.Controls.Add(this.PreliminaryTripDetailsDateEdit);
			this.StatusGroupBox.Controls.Add(this.PreliminaryTripDetailsStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.CrewPassengersLabel);
			this.StatusGroupBox.Controls.Add(this.CrewPassengersDateEdit);
			this.StatusGroupBox.Controls.Add(this.CrewPassengersStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.CompleteTripDetailsLabel);
			this.StatusGroupBox.Controls.Add(this.CompleteTripDetailsDateEdit);
			this.StatusGroupBox.Controls.Add(this.CompleteTripDetailsStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.CancelTripAndLinkedShipmentsLabel);
			this.StatusGroupBox.Controls.Add(this.CancelTripAndLinkedShipmentsDateEdit);
			this.StatusGroupBox.Controls.Add(this.CancelTripAndLinkedShipmentsStatusTextBox);
			this.StatusGroupBox.Controls.Add(this.RegisterCrewInformationLabel);
			this.StatusGroupBox.Controls.Add(this.RegisterCrewInformationDateEdit);
			this.StatusGroupBox.Controls.Add(this.RegisterCrewInformationStatusTextBox);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 634, true);
			this.StatusGroupBox.TabIndex = 0;
			this.StatusGroupBox.TabStop = false;
			// 
			// MsgStatusLabel
			// 
			this.MsgStatusLabel.AutoSize = true;
			this.MsgStatusLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("0CA65F81-2F02-4BA3-B812-E63FBD21450F", "Last Message Status");
			this.MsgStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MsgStatusLabel.IsFontBold = true;
			this.MsgStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 16, true);
			this.MsgStatusLabel.Name = "MsgStatusLabel";
			this.MsgStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 13, true);
			this.MsgStatusLabel.TabIndex = 0;
			// 
			// AcceptedDateLabel
			// 
			this.AcceptedDateLabel.AutoSize = true;
			this.AcceptedDateLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("FA6CD035-CD32-46A2-B30F-2B879F55D3FC", "Last Accepted Date");
			this.AcceptedDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AcceptedDateLabel.IsFontBold = true;
			this.AcceptedDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 16, true);
			this.AcceptedDateLabel.Name = "AcceptedDateLabel";
			this.AcceptedDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 13, true);
			this.AcceptedDateLabel.TabIndex = 0;
			// 
			// TypeLabel
			// 
			this.TypeLabel.AutoSize = true;
			this.TypeLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("70BF3415-5021-470B-A23E-51C38B02DEC6", "Message Type");
			this.TypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TypeLabel.IsFontBold = true;
			this.TypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.TypeLabel.Name = "TypeLabel";
			this.TypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.TypeLabel.TabIndex = 0;
			// 
			// CompleteEManifestLabel
			// 
			this.CompleteEManifestLabel.AutoSize = true;
			this.CompleteEManifestLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("951FD798-4280-414D-A47C-B1338D3A55BF", "Complete e-Manifest");
			this.CompleteEManifestLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CompleteEManifestLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 41, true);
			this.CompleteEManifestLabel.Name = "CompleteEManifestLabel";
			this.CompleteEManifestLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.CompleteEManifestLabel.TabIndex = 0;
			// 
			// CompleteEManifestDateEdit
			// 
			this.CompleteEManifestDateEdit.AllowDrop = true;
			this.CompleteEManifestDateEdit.AutoCompleteMonthThreshold = 1;
			this.CompleteEManifestDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CompleteEManifestDateEdit, "CompleteEManifestLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CompleteEManifestLastAcceptedDate)));
			this.CompleteEManifestDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompleteEManifestDateEdit, false);
			this.CompleteEManifestDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 38, true);
			this.CompleteEManifestDateEdit.Name = "CompleteEManifestDateEdit";
			this.CompleteEManifestDateEdit.TabIndex = 1;
			// 
			// CompleteEManifestStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompleteEManifestStatusTextBox, "CompleteEManifestLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CompleteEManifestLastAcceptedStatus)));
			this.CompleteEManifestStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompleteEManifestStatusTextBox, false);
			this.CompleteEManifestStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 38, true);
			this.CompleteEManifestStatusTextBox.Name = "CompleteEManifestStatusTextBox";
			this.CompleteEManifestStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.CompleteEManifestStatusTextBox.TabIndex = 2;
			// 
			// UnassociatedShipmentsLabel
			// 
			this.UnassociatedShipmentsLabel.AutoSize = true;
			this.UnassociatedShipmentsLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("1DE7DBDD-54B5-477C-985D-BDBCB8963C30", "Unassociated Shipments");
			this.UnassociatedShipmentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnassociatedShipmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 67, true);
			this.UnassociatedShipmentsLabel.Name = "UnassociatedShipmentsLabel";
			this.UnassociatedShipmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 13, true);
			this.UnassociatedShipmentsLabel.TabIndex = 0;
			// 
			// UnassociatedShipmentsDateEdit
			// 
			this.UnassociatedShipmentsDateEdit.AllowDrop = true;
			this.UnassociatedShipmentsDateEdit.AutoCompleteMonthThreshold = 1;
			this.UnassociatedShipmentsDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.UnassociatedShipmentsDateEdit, "UnassociatedShipmentsLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).UnassociatedShipmentsLastAcceptedDate)));
			this.UnassociatedShipmentsDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnassociatedShipmentsDateEdit, false);
			this.UnassociatedShipmentsDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 64, true);
			this.UnassociatedShipmentsDateEdit.Name = "UnassociatedShipmentsDateEdit";
			this.UnassociatedShipmentsDateEdit.TabIndex = 3;
			// 
			// UnassociatedShipmentsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnassociatedShipmentsStatusTextBox, "UnassociatedShipmentsLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).UnassociatedShipmentsLastAcceptedStatus)));
			this.UnassociatedShipmentsStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnassociatedShipmentsStatusTextBox, false);
			this.UnassociatedShipmentsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 64, true);
			this.UnassociatedShipmentsStatusTextBox.Name = "UnassociatedShipmentsStatusTextBox";
			this.UnassociatedShipmentsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.UnassociatedShipmentsStatusTextBox.TabIndex = 4;
			// 
			// PreliminaryTripDetailsLabel
			// 
			this.PreliminaryTripDetailsLabel.AutoSize = true;
			this.PreliminaryTripDetailsLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("D548ECB1-D475-47E9-9D06-FF7026A57CC7", "Preliminary Trip Details");
			this.PreliminaryTripDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PreliminaryTripDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 93, true);
			this.PreliminaryTripDetailsLabel.Name = "PreliminaryTripDetailsLabel";
			this.PreliminaryTripDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.PreliminaryTripDetailsLabel.TabIndex = 0;
			// 
			// PreliminaryTripDetailsDateEdit
			// 
			this.PreliminaryTripDetailsDateEdit.AllowDrop = true;
			this.PreliminaryTripDetailsDateEdit.AutoCompleteMonthThreshold = 1;
			this.PreliminaryTripDetailsDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PreliminaryTripDetailsDateEdit, "PreliminaryTripDetailsLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).PreliminaryTripDetailsLastAcceptedDate)));
			this.PreliminaryTripDetailsDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreliminaryTripDetailsDateEdit, false);
			this.PreliminaryTripDetailsDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 90, true);
			this.PreliminaryTripDetailsDateEdit.Name = "PreliminaryTripDetailsDateEdit";
			this.PreliminaryTripDetailsDateEdit.TabIndex = 5;
			// 
			// PreliminaryTripDetailsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreliminaryTripDetailsStatusTextBox, "PreliminaryTripDetailsLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).PreliminaryTripDetailsLastAcceptedStatus)));
			this.PreliminaryTripDetailsStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreliminaryTripDetailsStatusTextBox, false);
			this.PreliminaryTripDetailsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 90, true);
			this.PreliminaryTripDetailsStatusTextBox.Name = "PreliminaryTripDetailsStatusTextBox";
			this.PreliminaryTripDetailsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.PreliminaryTripDetailsStatusTextBox.TabIndex = 6;
			// 
			// CrewPassengersLabel
			// 
			this.CrewPassengersLabel.AutoSize = true;
			this.CrewPassengersLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("D395FC90-8480-4C1D-8DF6-D87CABEBD75D", "Crew/Passengers Details");
			this.CrewPassengersLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CrewPassengersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 119, true);
			this.CrewPassengersLabel.Name = "CrewPassengersLabel";
			this.CrewPassengersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 13, true);
			this.CrewPassengersLabel.TabIndex = 0;
			// 
			// CrewPassengersDateEdit
			// 
			this.CrewPassengersDateEdit.AllowDrop = true;
			this.CrewPassengersDateEdit.AutoCompleteMonthThreshold = 1;
			this.CrewPassengersDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CrewPassengersDateEdit, "CrewPassengersLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewPassengersLastAcceptedDate)));
			this.CrewPassengersDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CrewPassengersDateEdit, false);
			this.CrewPassengersDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 116, true);
			this.CrewPassengersDateEdit.Name = "CrewPassengersDateEdit";
			this.CrewPassengersDateEdit.TabIndex = 7;
			// 
			// CrewPassengersStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CrewPassengersStatusTextBox, "CrewPassengersLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CrewPassengersLastAcceptedStatus)));
			this.CrewPassengersStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CrewPassengersStatusTextBox, false);
			this.CrewPassengersStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 116, true);
			this.CrewPassengersStatusTextBox.Name = "CrewPassengersStatusTextBox";
			this.CrewPassengersStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.CrewPassengersStatusTextBox.TabIndex = 8;
			// 
			// CompleteTripDetailsLabel
			// 
			this.CompleteTripDetailsLabel.AutoSize = true;
			this.CompleteTripDetailsLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("43E11C72-E60F-48C9-86D5-3E181358A97A", "Confirm Trip Details Completed");
			this.CompleteTripDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CompleteTripDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 144, true);
			this.CompleteTripDetailsLabel.Name = "CompleteTripDetailsLabel";
			this.CompleteTripDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 13, true);
			this.CompleteTripDetailsLabel.TabIndex = 0;
			// 
			// CompleteTripDetailsDateEdit
			// 
			this.CompleteTripDetailsDateEdit.AllowDrop = true;
			this.CompleteTripDetailsDateEdit.AutoCompleteMonthThreshold = 1;
			this.CompleteTripDetailsDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CompleteTripDetailsDateEdit, "CompleteTripDetailsLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CompleteTripDetailsLastAcceptedDate)));
			this.CompleteTripDetailsDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompleteTripDetailsDateEdit, false);
			this.CompleteTripDetailsDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 141, true);
			this.CompleteTripDetailsDateEdit.Name = "CompleteTripDetailsDateEdit";
			this.CompleteTripDetailsDateEdit.TabIndex = 9;
			// 
			// CompleteTripDetailsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompleteTripDetailsStatusTextBox, "CompleteTripDetailsLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CompleteEManifestLastAcceptedStatus)));
			this.CompleteTripDetailsStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompleteTripDetailsStatusTextBox, false);
			this.CompleteTripDetailsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 141, true);
			this.CompleteTripDetailsStatusTextBox.Name = "CompleteTripDetailsLastAcceptedStatus";
			this.CompleteTripDetailsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.CompleteTripDetailsStatusTextBox.TabIndex = 10;
			// 
			// CancelTripAndLinkedShipmentsLabel
			// 
			this.CancelTripAndLinkedShipmentsLabel.AutoSize = true;
			this.CancelTripAndLinkedShipmentsLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("A2A1CAF3-B02D-4566-8E62-E430DB947D0E", "Cancel Trip And Linked Shipments");
			this.CancelTripAndLinkedShipmentsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CancelTripAndLinkedShipmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 170, true);
			this.CancelTripAndLinkedShipmentsLabel.Name = "CancelTripAndLinkedShipmentsLabel";
			this.CancelTripAndLinkedShipmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.CancelTripAndLinkedShipmentsLabel.TabIndex = 0;
			// 
			// CancelTripAndLinkedShipmentsDateEdit
			// 
			this.CancelTripAndLinkedShipmentsDateEdit.AllowDrop = true;
			this.CancelTripAndLinkedShipmentsDateEdit.AutoCompleteMonthThreshold = 1;
			this.CancelTripAndLinkedShipmentsDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CancelTripAndLinkedShipmentsDateEdit, "CancelTripAndLinkedShipmentsLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CancelTripAndLinkedShipmentsLastAcceptedDate)));
			this.CancelTripAndLinkedShipmentsDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CancelTripAndLinkedShipmentsDateEdit, false);
			this.CancelTripAndLinkedShipmentsDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 167, true);
			this.CancelTripAndLinkedShipmentsDateEdit.Name = "CancelTripAndLinkedShipmentsDateEdit";
			this.CancelTripAndLinkedShipmentsDateEdit.TabIndex = 11;
			// 
			// CancelTripAndLinkedShipmentsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CancelTripAndLinkedShipmentsStatusTextBox, "CancelTripAndLinkedShipmentsLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).CancelTripAndLinkedShipmentsLastAcceptedStatus)));
			this.CancelTripAndLinkedShipmentsStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CancelTripAndLinkedShipmentsStatusTextBox, false);
			this.CancelTripAndLinkedShipmentsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 167, true);
			this.CancelTripAndLinkedShipmentsStatusTextBox.Name = "CancelTripAndLinkedShipmentsStatusTextBox";
			this.CancelTripAndLinkedShipmentsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.CancelTripAndLinkedShipmentsStatusTextBox.TabIndex = 12;
			// 
			// RegisterCrewInformationLabel
			// 
			this.RegisterCrewInformationLabel.AutoSize = true;
			this.RegisterCrewInformationLabel.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("2002A05F-5C12-44BA-9150-B40CFC48678A", "Register Crew Information");
			this.RegisterCrewInformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RegisterCrewInformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 195, true);
			this.RegisterCrewInformationLabel.Name = "RegisterCrewInformationLabel";
			this.RegisterCrewInformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.RegisterCrewInformationLabel.TabIndex = 0;
			// 
			// RegisterCrewInformationDateEdit
			// 
			this.RegisterCrewInformationDateEdit.AllowDrop = true;
			this.RegisterCrewInformationDateEdit.AutoCompleteMonthThreshold = 1;
			this.RegisterCrewInformationDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RegisterCrewInformationDateEdit, "RegisterCrewInformationLastAcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).RegisterCrewInformationLastAcceptedDate)));
			this.RegisterCrewInformationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegisterCrewInformationDateEdit, false);
			this.RegisterCrewInformationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 193, true);
			this.RegisterCrewInformationDateEdit.Name = "RegisterCrewInformationDateEdit";
			this.RegisterCrewInformationDateEdit.TabIndex = 13;
			// 
			// RegisterCrewInformationStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegisterCrewInformationStatusTextBox, "RegisterCrewInformationLastAcceptedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.eManifest.Business.Trip)(null)).RegisterCrewInformationLastAcceptedStatus)));
			this.RegisterCrewInformationStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RegisterCrewInformationStatusTextBox, false);
			this.RegisterCrewInformationStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 193, true);
			this.RegisterCrewInformationStatusTextBox.Name = "RegisterCrewInformationStatusTextBox";
			this.RegisterCrewInformationStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 20, true);
			this.RegisterCrewInformationStatusTextBox.TabIndex = 14;
			// 
			// StatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusGroupBox);
			this.Name = "StatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 634, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.CompleteEManifestDateEdit.ResumeLayout(true);
			this.CompleteEManifestDateEdit.PerformLayout();
			this.UnassociatedShipmentsDateEdit.ResumeLayout(true);
			this.UnassociatedShipmentsDateEdit.PerformLayout();
			this.PreliminaryTripDetailsDateEdit.ResumeLayout(true);
			this.PreliminaryTripDetailsDateEdit.PerformLayout();
			this.CrewPassengersDateEdit.ResumeLayout(true);
			this.CrewPassengersDateEdit.PerformLayout();
			this.CompleteTripDetailsDateEdit.ResumeLayout(true);
			this.CompleteTripDetailsDateEdit.PerformLayout();
			this.CancelTripAndLinkedShipmentsDateEdit.ResumeLayout(true);
			this.CancelTripAndLinkedShipmentsDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private ZArchitecture.ZLabel MsgStatusLabel;
		private ZArchitecture.ZLabel AcceptedDateLabel;
		private ZArchitecture.ZLabel TypeLabel;
		private ZArchitecture.ZLabel CompleteEManifestLabel;
		private ZArchitecture.GUI.ZDateEdit CompleteEManifestDateEdit;
		private ZArchitecture.ZTextBox CompleteEManifestStatusTextBox;
		private ZArchitecture.ZLabel UnassociatedShipmentsLabel;
		private ZArchitecture.GUI.ZDateEdit UnassociatedShipmentsDateEdit;
		private ZArchitecture.ZTextBox UnassociatedShipmentsStatusTextBox;
		private ZArchitecture.ZLabel PreliminaryTripDetailsLabel;
		private ZArchitecture.GUI.ZDateEdit PreliminaryTripDetailsDateEdit;
		private ZArchitecture.ZTextBox PreliminaryTripDetailsStatusTextBox;
		private ZArchitecture.ZLabel CrewPassengersLabel;
		private ZArchitecture.GUI.ZDateEdit CrewPassengersDateEdit;
		private ZArchitecture.ZTextBox CrewPassengersStatusTextBox;
		private ZArchitecture.ZLabel CompleteTripDetailsLabel;
		private ZArchitecture.GUI.ZDateEdit CompleteTripDetailsDateEdit;
		private ZArchitecture.ZTextBox CompleteTripDetailsStatusTextBox;
		private ZArchitecture.ZLabel CancelTripAndLinkedShipmentsLabel;
		private ZArchitecture.GUI.ZDateEdit CancelTripAndLinkedShipmentsDateEdit;
		private ZArchitecture.ZTextBox CancelTripAndLinkedShipmentsStatusTextBox;
		private ZArchitecture.ZLabel RegisterCrewInformationLabel;
		private ZArchitecture.GUI.ZDateEdit RegisterCrewInformationDateEdit;
		private ZArchitecture.ZTextBox RegisterCrewInformationStatusTextBox;
	}
}
