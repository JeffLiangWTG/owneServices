namespace Enterprise.eTail.GUI
{
	partial class HVLVConsignmentsToStandAloneDeclarationsForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new protected void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			this.ButtonConvert = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectOrDeselectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectHeld = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonSelectNoneReported = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GridConsignments = new Enterprise.ZArchitecture.ZGrid();
			this.BoxGridConsignments = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GridConsignments)).BeginInit();
			this.GridConsignments.SuspendLayout();
			this.BoxGridConsignments.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.IHVLVConsignmentCollectionParent);
			// 
			// ButtonConvert
			// 
			this.ButtonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonConvert.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("8317c7c9-c9cd-40fa-b184-5bb0517fd73b", "Convert");
			this.ButtonConvert.IsCaptionOverridden = false;
			this.ButtonConvert.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 390, true);
			this.ButtonConvert.Name = "ButtonConvert";
			this.ButtonConvert.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonConvert.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ButtonConvert.TabIndex = 5;
			this.ButtonConvert.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonConvert.ToolTipCaption = null;
			this.ButtonConvert.UseVisualStyleBackColor = true;
			this.ButtonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
			// 
			// ButtonCancel
			//
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("fcb013ad-061b-435f-a39f-227e63ef135f", "Cancel");
			this.ButtonCancel.IsCaptionOverridden = false;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 390, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ButtonCancel.TabIndex = 6;
			this.ButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ButtonSelectOrDeselectAll
			// 
			this.ButtonSelectOrDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonSelectOrDeselectAll.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("e259ef32-50be-4764-828a-dfd0fe7a5bc7", "Select/Deselect All");
			this.ButtonSelectOrDeselectAll.IsCaptionOverridden = false;
			this.ButtonSelectOrDeselectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 390, true);
			this.ButtonSelectOrDeselectAll.Name = "ButtonSelectOrDeselectAll";
			this.ButtonSelectOrDeselectAll.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectOrDeselectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 25, true);
			this.ButtonSelectOrDeselectAll.TabIndex = 2;
			this.ButtonSelectOrDeselectAll.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSelectOrDeselectAll.ToolTipCaption = null;
			this.ButtonSelectOrDeselectAll.UseVisualStyleBackColor = true;
			this.ButtonSelectOrDeselectAll.Click += new System.EventHandler(this.ButtonSelectOrDeselectAll_Click);
			// 
			// ButtonSelectHeld
			// 
			this.ButtonSelectHeld.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonSelectHeld.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("52b4000e-7fa9-49cc-bf09-466d0988a8b6", "Select Held");
			this.ButtonSelectHeld.IsCaptionOverridden = false;
			this.ButtonSelectHeld.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 390, true);
			this.ButtonSelectHeld.Name = "ButtonSelectHeld";
			this.ButtonSelectHeld.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectHeld.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 25, true);
			this.ButtonSelectHeld.TabIndex = 3;
			this.ButtonSelectHeld.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSelectHeld.ToolTipCaption = null;
			this.ButtonSelectHeld.UseVisualStyleBackColor = true;
			this.ButtonSelectHeld.Click += new System.EventHandler(this.ButtonSelectHeld_Click);
			//
			// ButtonSelectNoneReported
			// 
			this.ButtonSelectNoneReported.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonSelectNoneReported.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("52b4000e-7fa9-49cc-bf09-466d0988a8b6", "Select None Reported");
			this.ButtonSelectNoneReported.IsCaptionOverridden = false;
			this.ButtonSelectNoneReported.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 390, true);
			this.ButtonSelectNoneReported.Name = "ButtonSelectNoneReported";
			this.ButtonSelectNoneReported.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonSelectNoneReported.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 25, true);
			this.ButtonSelectNoneReported.TabIndex = 4;
			this.ButtonSelectNoneReported.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonSelectNoneReported.ToolTipCaption = null;
			this.ButtonSelectNoneReported.UseVisualStyleBackColor = true;
			this.ButtonSelectNoneReported.Click += new System.EventHandler(this.ButtonSelectNoneReported_Click);
			// 
			// GridConsignments
			// 
			this.GridConsignments.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GridConsignments, "ConsignmentsConvertToStandAloneDeclarationView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			zTextBoxColumnStyleInfo1.ColumnName = "WaybillNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("fee8fee9-7e41-4b23-9305-59581d880451", "Waybill Number");

			zTextBoxColumnStyleInfo2.ColumnName = "ImportCustomsClearanceStatusDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("22369b6b-c8f2-4fbc-847b-9747e5be8e91", "Import Customs Clearance Status");

			zTextBoxColumnStyleInfo3.ColumnName = "ExportCustomsClearanceStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("56cbaed9-3bb4-41f4-b6af-dcde3f589b1c", "Export Customs Clearance Status");

			zCheckBoxColumnStyleInfo1.ColumnName = "Convert";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("69c86f0c-6693-4ddf-b1b4-bf3a7cb55879", "Convert?");
			this.GridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GridConsignments.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GridConsignments.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);

			this.GridConsignments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridConsignments.GridId = "18c04a01-9629-40a5-bf73-4e173c13be36";
			this.GridConsignments.AllowNavigation = false;
			this.GridConsignments.GridId = "a3bec31a-ba5d-4f22-a5f4-7554c9e4e8ec";
			this.GridConsignments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GridConsignments.LayoutKey = "HVLVConsignmentsGrid";
			this.GridConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GridConsignments.Name = "Grid";
			this.GridConsignments.TabIndex = 0;
			this.GridConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GridConsignments.Name = "GridConsignments";
			this.GridConsignments.ReadOnly = false;
			this.GridConsignments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 356, true);
			this.GridConsignments.TabIndex = 1;
			// 
			// BoxGridConsignments
			// 
			this.BoxGridConsignments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BoxGridConsignments.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("97c1df27-f73e-4b66-a14c-b0e960b8017e", "Consignments");
			this.BoxGridConsignments.Controls.Add(this.GridConsignments);
			this.BoxGridConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BoxGridConsignments.Name = "BoxGridConsignments";
			this.BoxGridConsignments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 375, true);
			this.BoxGridConsignments.TabIndex = 0;
			this.BoxGridConsignments.TabStop = false;
			// 
			// HVLVConsignmentsToStandAloneDeclarationsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("13e97fb7-2572-41a1-a955-0b71060a9f7b", "Convert Consignments to Stand Alone Declarations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 450, true);
			this.Controls.Add(this.BoxGridConsignments);
			this.Controls.Add(this.ButtonSelectHeld);
			this.Controls.Add(this.ButtonSelectNoneReported);
			this.Controls.Add(this.ButtonSelectOrDeselectAll);
			this.Controls.Add(this.ButtonConvert);
			this.Controls.Add(this.ButtonCancel);
			this.DataSourceType = typeof(Enterprise.eTail.Business.IHVLVConsignmentCollectionParent);
			this.Name = "HVLVConsignmentsToStandAloneDeclarationsForm";
			this.Text = "HVLVConvertConsignmentsToStandAloneDeclarationsForm";
			this.Controls.SetChildIndex(this.ButtonConvert, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			this.Controls.SetChildIndex(this.ButtonSelectOrDeselectAll, 0);
			this.Controls.SetChildIndex(this.ButtonSelectHeld, 0);
			this.Controls.SetChildIndex(this.ButtonSelectNoneReported, 0);
			this.Controls.SetChildIndex(this.BoxGridConsignments, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GridConsignments)).EndInit();
			this.GridConsignments.ResumeLayout(true);
			this.GridConsignments.PerformLayout();
			this.BoxGridConsignments.ResumeLayout(false);
			this.BoxGridConsignments.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox BoxGridConsignments;
		private Enterprise.ZArchitecture.ZGrid GridConsignments;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonConvert;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonSelectOrDeselectAll;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonSelectHeld;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonSelectNoneReported;
	}
}
