namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	partial class PortMessagingShipmentControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.PackLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ShipmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MRNCompleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Annex30AFailureProcessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.Annex30ADropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExemptionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ATBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportDecReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForwardingOfficeIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LRNCompleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.ShipmentGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager);
			// 
			// PackLinesGroupBox
			// 
			this.PackLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PackLinesGroupBox.Controls.Add(this.PackLinesGrid);
			this.PackLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 107, true);
			this.PackLinesGroupBox.Name = "PackLinesGroupBox";
			this.PackLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 120, true);
			this.PackLinesGroupBox.TabIndex = 10;
			this.PackLinesGroupBox.TabStop = false;
			this.PackLinesGroupBox.Text = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetString("c90b4b81-5164-49c6-80ad-6a64e10fb582", "Pack Lines");
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "PackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_JC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_HarmonisedCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).JL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_ATBNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_MovementReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_MovementReferenceNumberComplete)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_LocalReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_LocalReferenceNumberComplete)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_CustomsReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_ExemptionReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_Annex30AType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_Annex30AFailureProcess)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.ForwardingPackLineWithPortMessaging)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PackLines)).SyncRoot)).PortMessaging.JLM_ExportDeclarationReference)));
			this.PackLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "";
			zCalcEditColumnStyleInfo1.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "JL_F3_NKPackType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JL_JC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "JL_RH_NKCommodityCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo3.ColumnName = "JL_HarmonisedCode";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.ColumnName = "JL_ActualWeightUQ";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "JL_ActualVolumeUQ";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo6.ColumnName = "JL_Description";
			zDropEditColumnStyleInfo1.ColumnName = "PortMessaging+JLM_EntryType";
			zTextBoxColumnStyleInfo7.ColumnName = "PortMessaging+JLM_ATBNumber";
			zTextBoxColumnStyleInfo8.ColumnName = "PortMessaging+JLM_MovementReferenceNumber";
			zCheckBoxColumnStyleInfo1.ColumnName = "PortMessaging+JLM_MovementReferenceNumberComplete";
			zTextBoxColumnStyleInfo10.ColumnName = "PortMessaging+JLM_LocalReferenceNumber";
			zCheckBoxColumnStyleInfo3.ColumnName = "PortMessaging+JLM_LocalReferenceNumberComplete";
			zDateEditColumnStyleInfo1.ColumnName = "PortMessaging+JLM_CustomsReleaseDate";
			zDropEditColumnStyleInfo2.ColumnName = "PortMessaging+JLM_ExemptionReason";
			zDropEditColumnStyleInfo3.ColumnName = "PortMessaging+JLM_Annex30AType";
			zCheckBoxColumnStyleInfo2.ColumnName = "PortMessaging+JLM_Annex30AFailureProcess";
			zTextBoxColumnStyleInfo9.ColumnName = "PortMessaging+JLM_ExportDeclarationReference";
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PackLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PackLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.PackLinesGrid.CopySelectedRowsAllowed = true;
			this.PackLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesGrid.GridId = "c1a490df-6ba2-4593-9fba-6b7ed02a5964";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 101, true);
			this.PackLinesGrid.TabIndex = 0;
			// 
			// ShipmentGroupBox
			// 
			this.ShipmentGroupBox.Controls.Add(this.CustomsReleaseDateEdit);
			this.ShipmentGroupBox.Controls.Add(this.ExportDecReferenceTextBox);
			this.ShipmentGroupBox.Controls.Add(this.ForwardingOfficeIDTextBox);
			this.ShipmentGroupBox.Controls.Add(this.MRNCompleteCheckBox);
			this.ShipmentGroupBox.Controls.Add(this.PackLinesGroupBox);
			this.ShipmentGroupBox.Controls.Add(this.Annex30AFailureProcessCheckBox);
			this.ShipmentGroupBox.Controls.Add(this.Annex30ADropEdit);
			this.ShipmentGroupBox.Controls.Add(this.ExemptionReasonDropEdit);
			this.ShipmentGroupBox.Controls.Add(this.ATBNumberTextBox);
			this.ShipmentGroupBox.Controls.Add(this.MRNTextBox);
			this.ShipmentGroupBox.Controls.Add(this.EntryTypeDropEdit);
			this.ShipmentGroupBox.Controls.Add(this.LRNTextBox);
			this.ShipmentGroupBox.Controls.Add(this.LRNCompleteCheckBox);
			this.ShipmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentGroupBox.Name = "ShipmentGroupBox";
			this.ShipmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 230, true);
			this.ShipmentGroupBox.TabIndex = 0;
			this.ShipmentGroupBox.TabStop = false;
			this.ShipmentGroupBox.Text = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetString("be13eef0-e320-454b-8559-ce3eeca3d3fe", "Customs Fields");
			// 
			// CustomsReleaseDateEdit
			// 
			this.CustomsReleaseDateEdit.AllowDrop = true;
			this.CustomsReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomsReleaseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomsReleaseDateEdit, "PortMessaging.JSM_CustomsReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_CustomsReleaseDate)));
			this.CustomsReleaseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CustomsReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 81, true);
			this.CustomsReleaseDateEdit.Name = "CustomsReleaseDateEdit";
			this.CustomsReleaseDateEdit.TabIndex = 8;
			// 
			// MRNCompleteCheckBox
			// 
			this.MRNCompleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MRNCompleteCheckBox, "PortMessaging.JSM_MovementReferenceNumberComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_MovementReferenceNumberComplete)));
			this.MRNCompleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MRNCompleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 62, true);
			this.MRNCompleteCheckBox.Name = "MRNCompleteCheckBox";
			this.MRNCompleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.MRNCompleteCheckBox.TabIndex = 6;
			this.MRNCompleteCheckBox.UseVisualStyleBackColor = true;
			// 
			// Annex30AFailureProcessCheckBox
			// 
			this.Annex30AFailureProcessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.Annex30AFailureProcessCheckBox, "PortMessaging.JSM_Annex30AFailureProcess");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_Annex30AFailureProcess)));
			this.Annex30AFailureProcessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Annex30AFailureProcessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 62, true);
			this.Annex30AFailureProcessCheckBox.Name = "Annex30AFailureProcessCheckBox";
			this.Annex30AFailureProcessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.Annex30AFailureProcessCheckBox.TabIndex = 7;
			this.Annex30AFailureProcessCheckBox.UseVisualStyleBackColor = true;
			// 
			// Annex30ADropEdit
			// 
			this.Annex30ADropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Annex30ADropEdit, "PortMessaging.JSM_Annex30AType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_Annex30AType)));
			this.Annex30ADropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 37, true);
			this.Annex30ADropEdit.Name = "Annex30ADropEdit";
			this.Annex30ADropEdit.PreBoundMaxLength = 1;
			this.Annex30ADropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.Annex30ADropEdit.TabIndex = 4;
			// 
			// ExemptionReasonDropEdit
			// 
			this.ExemptionReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExemptionReasonDropEdit, "PortMessaging.JSM_ExemptionReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_ExemptionReason)));
			this.ExemptionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 15, true);
			this.ExemptionReasonDropEdit.Name = "ExemptionReasonDropEdit";
			this.ExemptionReasonDropEdit.PreBoundMaxLength = 1;
			this.ExemptionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ExemptionReasonDropEdit.TabIndex = 1;
			// 
			// ATBNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ATBNumberTextBox, "PortMessaging.JSM_ATBNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_ATBNumber)));
			this.ATBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 36, true);
			this.ATBNumberTextBox.Name = "ATBNumberTextBox";
			this.ATBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.ATBNumberTextBox.TabIndex = 5;
			// 
			// MRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MRNTextBox, "PortMessaging.JSM_MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_MovementReferenceNumber)));
			this.MRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 37, true);
			this.MRNTextBox.Name = "MRNTextBox";
			this.MRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.MRNTextBox.TabIndex = 3;
			// 
			// EntryTypeDropEdit
			// 
			this.EntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "PortMessaging.JSM_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_EntryType)));
			this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.EntryTypeDropEdit.PreBoundMaxLength = 3;
			this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.EntryTypeDropEdit.TabIndex = 0;
			// 
			// ExportDecReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportDecReferenceTextBox, "PortMessaging.JSM_ExportDeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_ExportDeclarationReference)));
			this.ExportDecReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 81, true);
			this.ExportDecReferenceTextBox.Name = "ExportDecReferenceTextBox";
			this.ExportDecReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ExportDecReferenceTextBox.TabIndex = 9;
			// 
			// ForwardingOfficeIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForwardingOfficeIDTextBox, "PortMessaging.JSM_ForwardingCustomsOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_ForwardingCustomsOfficeCode)));
			this.ForwardingOfficeIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 15, true);
			this.ForwardingOfficeIDTextBox.Name = "ForwardingOfficeIDTextBox";
			this.ForwardingOfficeIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.ForwardingOfficeIDTextBox.TabIndex = 2;
			// 
			// LRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNTextBox, "PortMessaging.JSM_LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_LocalReferenceNumber)));
			this.LRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 37, true);
			this.LRNTextBox.Name = "LRNTextBox";
			this.LRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.LRNTextBox.TabIndex = 3;
			// 
			// LRNCompleteCheckBox
			// 
			this.LRNCompleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LRNCompleteCheckBox, "PortMessaging.JSM_LocalReferenceNumberComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).PortMessaging.JSM_LocalReferenceNumberComplete)));
			this.LRNCompleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LRNCompleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 62, true);
			this.LRNCompleteCheckBox.Name = "LRNCompleteCheckBox";
			this.LRNCompleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.LRNCompleteCheckBox.TabIndex = 6;
			this.LRNCompleteCheckBox.UseVisualStyleBackColor = true;
			// 
			// PortMessagingShipmentControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShipmentGroupBox);
			this.Name = "PortMessagingShipmentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 230, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackLinesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.ShipmentGroupBox.ResumeLayout(false);
			this.ShipmentGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PackLinesGroupBox;
		private ZArchitecture.ZGrid PackLinesGrid;
		private ZArchitecture.GUI.ZGroupBox ShipmentGroupBox;
		private ZArchitecture.GUI.ZDropEdit EntryTypeDropEdit;
		private ZArchitecture.ZTextBox MRNTextBox;
		private ZArchitecture.ZTextBox ATBNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit ExemptionReasonDropEdit;
		private ZArchitecture.GUI.ZDropEdit Annex30ADropEdit;
		private ZArchitecture.GUI.ZCheckBox Annex30AFailureProcessCheckBox;
		private ZArchitecture.GUI.ZCheckBox MRNCompleteCheckBox;
		private ZArchitecture.ZTextBox ExportDecReferenceTextBox;
		private ZArchitecture.ZTextBox ForwardingOfficeIDTextBox;
		private ZArchitecture.GUI.ZDateEdit CustomsReleaseDateEdit;
		private ZArchitecture.ZTextBox LRNTextBox;
		private ZArchitecture.GUI.ZCheckBox LRNCompleteCheckBox;
	}
}
