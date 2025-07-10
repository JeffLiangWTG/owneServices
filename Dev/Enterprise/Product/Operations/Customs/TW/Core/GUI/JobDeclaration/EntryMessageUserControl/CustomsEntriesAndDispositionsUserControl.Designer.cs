namespace Enterprise.Customs.TW.GUI
{
	partial class CustomsEntriesAndDispositionsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.EntryHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntryReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalNetWeightInKilogramsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationIncotermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearanceStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsResponseCodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CustomsResponseCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsResponseCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryHeaderPanel.SuspendLayout();
			this.EntryReleaseDateDateEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			this.DeclarationIncotermDropEdit.SuspendLayout();
			this.ClearanceStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.CustomsResponseCodePanel.SuspendLayout();
			this.CustomsResponseCodeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsResponseCodeGrid)).BeginInit();
			this.CustomsResponseCodeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// EntryHeaderPanel
			// 
			this.EntryHeaderPanel.Controls.Add(this.EntryReleaseDateDateEdit);
			this.EntryHeaderPanel.Controls.Add(this.MarksAndNumbersTextBox);
			this.EntryHeaderPanel.Controls.Add(this.TotalNetWeightInKilogramsCalcEdit);
			this.EntryHeaderPanel.Controls.Add(this.EntryStatusDropEdit);
			this.EntryHeaderPanel.Controls.Add(this.DeclarationIncotermDropEdit);
			this.EntryHeaderPanel.Controls.Add(this.ClearanceStatusDropEdit);
			this.EntryHeaderPanel.Controls.Add(this.MessageStatusDropEdit);
			this.EntryHeaderPanel.Controls.Add(this.EntryNumberTextBox);
			this.EntryHeaderPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.EntryHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryHeaderPanel.Name = "EntryHeaderPanel";
			this.EntryHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 246, true);
			this.EntryHeaderPanel.TabIndex = 0;
			// 
			// EntryReleaseDateDateEdit
			// 
			this.EntryReleaseDateDateEdit.AllowDrop = true;
			this.EntryReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EntryReleaseDateDateEdit, "CustomsEntryHeaders.CH_EntryReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			this.EntryReleaseDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.EntryReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 136, true);
			this.EntryReleaseDateDateEdit.Name = "EntryReleaseDateDateEdit";
			this.EntryReleaseDateDateEdit.TabIndex = 14;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "CustomsEntryHeaders.MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("ad489c74-2750-4708-9182-ef047d30e94f", "Marks & Numbers");
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 186, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 45, true);
			this.MarksAndNumbersTextBox.TabIndex = 16;
			// 
			// TotalNetWeightInKilogramsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalNetWeightInKilogramsCalcEdit, "CustomsEntryHeaders.CH_TotalNetWeightInKilograms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_TotalNetWeightInKilograms)));
			this.TotalNetWeightInKilogramsCalcEdit.DecimalPlaces = 3;
			this.TotalNetWeightInKilogramsCalcEdit.Decimals = 3;
			this.TotalNetWeightInKilogramsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 162, true);
			this.TotalNetWeightInKilogramsCalcEdit.Name = "TotalNetWeightInKilogramsCalcEdit";
			this.TotalNetWeightInKilogramsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.TotalNetWeightInKilogramsCalcEdit.TabIndex = 15;
			this.TotalNetWeightInKilogramsCalcEdit.Text = "0.000";
			this.TotalNetWeightInKilogramsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalNetWeightInKilogramsCalcEdit.TrackDisposedAccess = true;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "JE_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_EntryStatusDescription)));
			this.EntryStatusDropEdit.BindToForDescription = "JE_EntryStatusDescription";
			this.EntryStatusDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e46938b4-44d8-4245-950e-6ec162b78469", "Entry Status");
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 64, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.PreBoundMaxLength = 3;
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.EntryStatusDropEdit.TabIndex = 11;
			// 
			// DeclarationIncotermDropEdit
			// 
			this.DeclarationIncotermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationIncotermDropEdit, "CustomsEntryHeaders.CH_DeclarationIncoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_DeclarationIncoterm)));
			this.DeclarationIncotermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 112, true);
			this.DeclarationIncotermDropEdit.Name = "DeclarationIncotermDropEdit";
			this.DeclarationIncotermDropEdit.PreBoundMaxLength = 3;
			this.DeclarationIncotermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.DeclarationIncotermDropEdit.TabIndex = 13;
			// 
			// ClearanceStatusDropEdit
			// 
			this.ClearanceStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearanceStatusDropEdit, "ClearanceStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).ClearanceStatus)));
			this.ClearanceStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 40, true);
			this.ClearanceStatusDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ClearanceStatusDropEdit.Name = "ClearanceStatusDropEdit";
			this.ClearanceStatusDropEdit.PreBoundMaxLength = 3;
			this.ClearanceStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.ClearanceStatusDropEdit.TabIndex = 10;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "JE_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).JE_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 88, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.PreBoundMaxLength = 3;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.MessageStatusDropEdit.TabIndex = 12;
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "DeclarationNumberDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).DeclarationNumberDisplay)));
			this.EntryNumberTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("68e58b1c-8376-4941-b553-e6d00393881f", "Entry Number");
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.ReadOnly = true;
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.EntryNumberTextBox.TabIndex = 9;
			// 
			// CustomsResponseCodePanel
			// 
			this.CustomsResponseCodePanel.Controls.Add(this.CustomsResponseCodeGroupBox);
			this.CustomsResponseCodePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsResponseCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 0, true);
			this.CustomsResponseCodePanel.Name = "CustomsResponseCodePanel";
			this.CustomsResponseCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 246, true);
			this.CustomsResponseCodePanel.TabIndex = 1;
			// 
			// CustomsResponseCodeGroupBox
			// 
			this.CustomsResponseCodeGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("be2ee8b7-7da3-4cae-8df4-842aad7d80ee", "Customs Response Code");
			this.CustomsResponseCodeGroupBox.Controls.Add(this.CustomsResponseCodeGrid);
			this.CustomsResponseCodeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsResponseCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsResponseCodeGroupBox.Name = "CustomsResponseCodeGroupBox";
			this.CustomsResponseCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 246, true);
			this.CustomsResponseCodeGroupBox.TabIndex = 1;
			this.CustomsResponseCodeGroupBox.TabStop = false;
			// 
			// CustomsResponseCodeGrid
			// 
			this.CustomsResponseCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CustomsResponseCodeGrid, "CustomsEntryHeaders.CusDispositions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusDispositions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusDisposition)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusDispositions)).SyncRoot)).CDI_StatusKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusDisposition)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusDispositions)).SyncRoot)).CDI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.CusDisposition)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusDispositions)).SyncRoot)).CDI_StatusDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusDisposition)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusDispositions)).SyncRoot)).StatusDescription)));
			this.CustomsResponseCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CDI_StatusKey";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "CDI_Status";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "CDI_StatusDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.CustomsResponseCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CustomsResponseCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsResponseCodeGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CustomsResponseCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CustomsResponseCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsResponseCodeGrid.GridId = "3f347eb9-3f4c-4498-a3ae-a47cbf7a1263";
			this.CustomsResponseCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsResponseCodeGrid.LayoutKey = "CustomsResponseCodeGrid";
			this.CustomsResponseCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsResponseCodeGrid.Name = "CustomsResponseCodeGrid";
			this.CustomsResponseCodeGrid.ReadOnly = true;
			this.CustomsResponseCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 227, true);
			this.CustomsResponseCodeGrid.TabIndex = 0;
			// 
			// CustomsEntriesAndDispositionsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CustomsResponseCodePanel);
			this.Controls.Add(this.EntryHeaderPanel);
			this.Name = "CustomsEntriesAndDispositionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryHeaderPanel.ResumeLayout(false);
			this.EntryHeaderPanel.PerformLayout();
			this.EntryReleaseDateDateEdit.ResumeLayout(true);
			this.EntryReleaseDateDateEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.DeclarationIncotermDropEdit.ResumeLayout(true);
			this.DeclarationIncotermDropEdit.PerformLayout();
			this.ClearanceStatusDropEdit.ResumeLayout(true);
			this.ClearanceStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.CustomsResponseCodePanel.ResumeLayout(false);
			this.CustomsResponseCodePanel.PerformLayout();
			this.CustomsResponseCodeGroupBox.ResumeLayout(false);
			this.CustomsResponseCodeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsResponseCodeGrid)).EndInit();
			this.CustomsResponseCodeGrid.ResumeLayout(false);
			this.CustomsResponseCodeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel EntryHeaderPanel;
		private ZArchitecture.GUI.ZPanel CustomsResponseCodePanel;
		private ZArchitecture.GUI.ZDateEdit EntryReleaseDateDateEdit;
		private ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		private ZArchitecture.ZCalcEdit TotalNetWeightInKilogramsCalcEdit;
		private ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit DeclarationIncotermDropEdit;
		private ZArchitecture.GUI.ZDropEdit ClearanceStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.ZTextBox EntryNumberTextBox;
		private ZArchitecture.ZGrid CustomsResponseCodeGrid;
		private ZArchitecture.GUI.ZGroupBox CustomsResponseCodeGroupBox;
	}
}
