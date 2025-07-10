using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class ZExceptionsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo ZGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo ZGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo ZDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo ZCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ExceptionsGrid = new ZGrid();
			this.colourLegend = new Enterprise.ZArchitecture.GUI.ColourLegend();
			this.RichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.exceptionsHintSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.exceptionsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionsGrid)).BeginInit();
			this.ExceptionsGrid.SuspendLayout();
			this.RichTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.exceptionsHintSplitContainer)).BeginInit();
			this.exceptionsHintSplitContainer.Panel1.SuspendLayout();
			this.exceptionsHintSplitContainer.Panel2.SuspendLayout();
			this.exceptionsHintSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.exceptionsHintSplitContainer);
			this.splitContainer.Panel1.Controls.Add(this.colourLegend);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 466, true);
			this.splitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.RichTextBox);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(281);
			this.splitContainer.TabIndex = 2;
			// 
			// ExceptionsGrid
			// 
			this.ExceptionsGrid.AccessibleDescription = "";
			this.ExceptionsGrid.AllowNavigation = false;
			this.ExceptionsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.ExceptionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).IsExceptionActionedForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GS_NKAssignedStaffMember)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GG_AssignedGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionCausePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionCauseDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionResolutionPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionResolutionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionTypeCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionTypeCategoryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).CompletedTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_CompletedTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).ExceptionAssignedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDateUtcForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ActualDateLocalForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_RL_NKExceptionLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ExceptionDurationHours)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_ExceptionEndDate)));
			this.ExceptionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|4198d5d8-0098-4882-ab71-a3e3ac3df987", "Actioned");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsExceptionActionedForBinding";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "P9_GS_NKAssignedStaffMember";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "P9_GG_AssignedGroup";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|6515a229-d6d0-4035-8847-793148e489d2", "Exception Time");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "P9_ActualDateForBinding";
			zDateTimeOffsetEditColumnStyleInfo1.HasTimeZoneFindBox = true;
			zDateTimeOffsetEditColumnStyleInfo1.IsMandatory = true;
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|6515a229-d6d0-4035-8847-793148e489d3", "Exception Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "P9_ActualDateUtcForBinding";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|6515a229-d6d0-4035-8847-793148e489d4", "Exception Time (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "P9_ActualDateLocalForBinding";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|C08610bf-d98e-4ec1-a92c-e4a18e560a0a", "Type");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ExceptionTypeCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WorkflowExceptionTypes;
			ZDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|fd9d5c95-1f8a-42f6-aa67-c048c6c7430c", "Type Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ExceptionTypeDescription";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			ZGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|7a6a7636-9300-48cb-befb-36f4061d7ebc", "Cause");
			ZGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ZGuidDropEditColumnStyleInfo1.ColumnName = "ExceptionCausePK";
			ZGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|ded461c2-c8de-48ad-87ea-5b42c327e42e", "Cause Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ExceptionCauseDescription";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			ZGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|f39f2be4-5ff8-42a7-aa9a-3b4a5ed4bde4", "Resolution");
			ZGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ZGuidDropEditColumnStyleInfo2.ColumnName = "ExceptionResolutionPK";
			ZGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|e8e76982-3d1a-4c5c-affa-47d100921c04", "Resolution Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ExceptionResolutionDescription";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			ZDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|4a15e8a5-1afd-43c5-ae2f-a0ca87ed71ef", "Category");
			ZDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ZDropEditColumnStyleInfo1.ColumnName = "ExceptionTypeCategory";
			ZDropEditColumnStyleInfo1.IsMandatory = true;
			ZDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|add1d58a-9009-47e7-872d-6ea0ac4bf05d", "Category Description");
			zTextBoxColumnStyleInfo5.ColumnName = "ExceptionTypeCategoryDescription";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|9340daca-ae3f-4dcf-90d6-7c49d579d738", "Actioned Time (Local)");
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "CompletedTimeLocal";
			zDateTimeOffsetEditColumnStyleInfo2.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo2.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateTimeOffsetEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|9340daca-ae3f-4dcf-90d6-7c49d579d739", "Actioned Time (UTC)");
			zDateTimeOffsetEditColumnStyleInfo4.ColumnName = "P9_CompletedTimeUtc";
			zDateTimeOffsetEditColumnStyleInfo4.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo4.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|7ee6b07d-20c0-4471-b5d7-35e047f2daf9", "Assigned Time");
			zDateTimeOffsetEditColumnStyleInfo3.ColumnName = "ExceptionAssignedDate";
			zDateTimeOffsetEditColumnStyleInfo3.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo3.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "P9_RL_NKExceptionLocation";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|99633aa4-012a-4dfe-231c-affbe7e40221", "Location", "Exception Location", "Location where the exception occurred.");
			ZCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|13633aa4-012a-4dfe-b11c-affbe7e402be", "Duration (Hours)", "Exception Duration (Hours)", "Duration of exception in hours.");
			ZCalcEditColumnStyleInfo.ColumnName = "P9_ExceptionDurationHours";
			ZCalcEditColumnStyleInfo.Decimals = 0;
			ZCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZExceptionsUserControl|9340d2ca-ae3f-4dcf-90d6-7c49d579d744", "Exception End");
			zDateTimeOffsetEditColumnStyleInfo6.ColumnName = "P9_ExceptionEndDate";
			zDateTimeOffsetEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.ExceptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(ZDropEditColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ExceptionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(ZGuidDropEditColumnStyleInfo1);
			this.ExceptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ExceptionsGrid.ColumnStyles.Add(ZGuidDropEditColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ExceptionsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.ExceptionsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo3);
			this.ExceptionsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo4);
			this.ExceptionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ExceptionsGrid.ColumnStyles.Add(ZCalcEditColumnStyleInfo);
			this.ExceptionsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo6);
			this.ExceptionsGrid.CopySelectedRowsAllowed = true;
			this.ExceptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExceptionsGrid.GridId = "1025db83-3d6e-48ac-a66f-637e5892f461";
			this.ExceptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExceptionsGrid.LayoutKey = "zGrid1";
			this.ExceptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExceptionsGrid.Name = "ExceptionsGrid";
			this.ExceptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 209, true);
			this.ExceptionsGrid.TabIndex = 1;
			// 
			// colourLegend
			// 
			this.colourLegend.AutoScroll = true;
			this.colourLegend.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.colourLegend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 251, true);
			this.colourLegend.Name = "colourLegend";
			this.colourLegend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 30, true);
			this.colourLegend.TabIndex = 0;
			// 
			// RichTextBox
			// 
			this.BindingSource.SetBindingMember(this.RichTextBox, "P9_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Notes)));
			this.RichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RichTextBox, false);
			this.RichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RichTextBox.MaxLength = 10000000;
			this.RichTextBox.Name = "RichTextBox";
			this.RichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 181, true);
			this.RichTextBox.TabIndex = 2;
			// 
			// exceptionHintSplitContainer
			// 
			this.exceptionsHintSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exceptionsHintSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exceptionsHintSplitContainer.Name = "exceptionHintSplitContainer";
			this.exceptionsHintSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// exceptionHintSplitContainer.Panel1
			// 
			this.exceptionsHintSplitContainer.Panel1.Controls.Add(this.exceptionsHintLabel);
			// 
			// exceptionHintSplitContainer.Panel2
			// 
			this.exceptionsHintSplitContainer.Panel2.Controls.Add(this.ExceptionsGrid);
			this.exceptionsHintSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 251, true);
			this.exceptionsHintSplitContainer.SplitterDistance = 38;
			this.exceptionsHintSplitContainer.TabIndex = 2;
			// 
			// exceptionsHintLabel
			// 
			this.exceptionsHintLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exceptionsHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.exceptionsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exceptionsHintLabel.Name = "exceptionsHintLabel";
			this.exceptionsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 38, true);
			this.exceptionsHintLabel.TabIndex = 0;
			// 
			// ZExceptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "ZExceptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExceptionsGrid)).EndInit();
			this.ExceptionsGrid.ResumeLayout(false);
			this.ExceptionsGrid.PerformLayout();
			this.RichTextBox.ResumeLayout(true);
			this.RichTextBox.PerformLayout();
			this.exceptionsHintSplitContainer.Panel1.ResumeLayout(false);
			this.exceptionsHintSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.exceptionsHintSplitContainer)).EndInit();
			this.exceptionsHintSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer;
		private Enterprise.ZArchitecture.GUI.ZRichTextBox RichTextBox;
		protected Enterprise.ZArchitecture.ZGrid ExceptionsGrid;
		protected Enterprise.ZArchitecture.GUI.ColourLegend colourLegend;
		private CargoWise.Windows.UI.KSplitContainer exceptionsHintSplitContainer;
		private ZArchitecture.ZLabel exceptionsHintLabel;
	}
}
