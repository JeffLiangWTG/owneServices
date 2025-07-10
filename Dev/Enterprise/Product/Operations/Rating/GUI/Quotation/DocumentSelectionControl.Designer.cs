using CargoWise.Windows.UI;

namespace Enterprise.Rating.GUI
{
	partial class DocumentSelectionControl
	{
		private System.ComponentModel.Container components = null;

		protected override void Dispose(bool IsNotFinalizing)
		{
			if (IsNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(IsNotFinalizing);
		}

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AvailableImagesPreviewControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.SelectedImagesPreviewControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SelectedPagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.AddSelectedPagesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveSelectedPagesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.AvailablePagesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AvailableImagesPreviewControl.SuspendLayout();
			this.SelectedImagesPreviewControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedPagesGrid)).BeginInit();
			this.SelectedPagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailablePagesGrid)).BeginInit();
			this.AvailablePagesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.Quote);
			// 
			// AvailableImagesPreviewControl
			// 
			this.AvailableImagesPreviewControl.AllowDrop = true;
			this.AvailableImagesPreviewControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AvailableImagesPreviewControl, "AvailablePages.Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Rating.Business.RateAttachmentSet)(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).AvailablePages)).SyncRoot)).Image)));
			this.AvailableImagesPreviewControl.CanSelectImage = false;
			this.AvailableImagesPreviewControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 24, true);
			this.AvailableImagesPreviewControl.Name = "AvailableImagesPreviewControl";
			this.AvailableImagesPreviewControl.ReadOnly = true;
			this.AvailableImagesPreviewControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 440, true);
			this.AvailableImagesPreviewControl.TabIndex = 1;
			this.AvailableImagesPreviewControl.Visible = false;
			// 
			// SelectedImagesPreviewControl
			// 
			this.SelectedImagesPreviewControl.AllowDrop = true;
			this.SelectedImagesPreviewControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectedImagesPreviewControl, "SelectedPages.Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Rating.Business.RateAttachment)(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).SelectedPages)).SyncRoot)).Image)));
			this.SelectedImagesPreviewControl.CanSelectImage = false;
			this.SelectedImagesPreviewControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 24, true);
			this.SelectedImagesPreviewControl.Name = "SelectedImagesPreviewControl";
			this.SelectedImagesPreviewControl.ReadOnly = false;
			this.SelectedImagesPreviewControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 440, true);
			this.SelectedImagesPreviewControl.TabIndex = 1;
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.SelectedPagesGrid);
			this.splitContainer.Panel1.Controls.Add(this.zLabel7);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.AddSelectedPagesButton);
			this.splitContainer.Panel2.Controls.Add(this.RemoveSelectedPagesButton);
			this.splitContainer.Panel2.Controls.Add(this.zLabel8);
			this.splitContainer.Panel2.Controls.Add(this.AvailablePagesGrid);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 458, true);
			this.splitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(0);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(229);
			this.splitContainer.TabIndex = 0;
			// 
			// SelectedPagesGrid
			// 
			this.SelectedPagesGrid.AllowNavigation = false;
			this.SelectedPagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectedPagesGrid, "SelectedPages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).SelectedPages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateAttachment)(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).SelectedPages)).SyncRoot)).TA_RateAttachmentName)));
			this.SelectedPagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|b50860d2-e270-4d95-a9fc-f433c76b627d", "Attachment Name");
			zTextBoxColumnStyleInfo1.ColumnName = "TA_RateAttachmentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(512);
			this.SelectedPagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedPagesGrid.GridId = "ec190781-509b-4b52-bc11-21feeea046ac";
			this.SelectedPagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedPagesGrid.IsWholeRowSelectedOnClick = true;
			this.SelectedPagesGrid.LayoutKey = "zGrid1";
			this.SelectedPagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.SelectedPagesGrid.Name = "SelectedPagesGrid";
			this.SelectedPagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SelectedPagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 206, true);
			this.SelectedPagesGrid.TabIndex = 1;
			this.SelectedPagesGrid.Enter += new System.EventHandler(this.SelectedAvailablePagesGrid_Enter);
			this.SelectedPagesGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SelectedPagesGrid_MouseDown);
			// 
			// zLabel7
			// 
			this.zLabel7.AutoSize = true;
			this.zLabel7.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|f7a3a809-bf3a-40c2-ba72-9eaad09cb53b", "Selected Pages");
			this.zLabel7.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel7.IsFontBold = true;
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
			this.zLabel7.TabIndex = 0;
			// 
			// AddSelectedPagesButton
			// 
			this.AddSelectedPagesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddSelectedPagesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|f576e8c5-de3e-492c-bb23-f97b532bd6d5", "Add to Selected Pages");
			this.AddSelectedPagesButton.IsCaptionOverridden = false;
			this.AddSelectedPagesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 27, true);
			this.AddSelectedPagesButton.Name = "AddSelectedPagesButton";
			this.AddSelectedPagesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddSelectedPagesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.AddSelectedPagesButton.TabIndex = 1;
			this.AddSelectedPagesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AddSelectedPagesButton.ToolTipCaption = null;
			this.AddSelectedPagesButton.Click += new System.EventHandler(this.AddSelectedPagesButton_Click);
			// 
			// RemoveSelectedPagesButton
			// 
			this.RemoveSelectedPagesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveSelectedPagesButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|9b7662c8-1ef2-4932-b8a5-5260f9bb4cb7", "Remove From Selected Pages");
			this.RemoveSelectedPagesButton.IsCaptionOverridden = false;
			this.RemoveSelectedPagesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 3, true);
			this.RemoveSelectedPagesButton.Name = "RemoveSelectedPagesButton";
			this.RemoveSelectedPagesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RemoveSelectedPagesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.RemoveSelectedPagesButton.TabIndex = 0;
			this.RemoveSelectedPagesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RemoveSelectedPagesButton.ToolTipCaption = null;
			this.RemoveSelectedPagesButton.Click += new System.EventHandler(this.RemoveSelectedPagesButton_Click);
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|2855cf62-6b34-4c21-8575-617f50516d82", "Available Pages");
			this.zLabel8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel8.IsFontBold = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 39, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.zLabel8.TabIndex = 2;
			// 
			// AvailablePagesGrid
			// 
			this.AvailablePagesGrid.AllowNavigation = false;
			this.AvailablePagesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AvailablePagesGrid, "AvailablePages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).AvailablePages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateAttachmentSet)(((System.Collections.IList)(((Enterprise.Rating.Business.Quote)(null)).AvailablePages)).SyncRoot)).TS_AttachmentNameMultilingual)));
			this.AvailablePagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("DocumentSelectionControl|b50860d2-e270-4d95-a9fc-f433c76b627d", "Attachment Name");
			zTextBoxColumnStyleInfo2.ColumnName = "TS_AttachmentNameMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(512);
			this.AvailablePagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AvailablePagesGrid.GridId = "71f81ba0-6137-4d0c-9d00-50686693b6b5";
			this.AvailablePagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AvailablePagesGrid.IsWholeRowSelectedOnClick = true;
			this.AvailablePagesGrid.LayoutKey = "zGrid1";
			this.AvailablePagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.AvailablePagesGrid.Name = "AvailablePagesGrid";
			this.AvailablePagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AvailablePagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 166, true);
			this.AvailablePagesGrid.TabIndex = 3;
			this.AvailablePagesGrid.Enter += new System.EventHandler(this.SelectedAvailablePagesGrid_Enter);
			this.AvailablePagesGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AvailablePagesGrid_MouseDown);
			// 
			// DocumentSelectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.SelectedImagesPreviewControl);
			this.Controls.Add(this.AvailableImagesPreviewControl);
			this.Name = "DocumentSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AvailableImagesPreviewControl.ResumeLayout(true);
			this.AvailableImagesPreviewControl.PerformLayout();
			this.SelectedImagesPreviewControl.ResumeLayout(true);
			this.SelectedImagesPreviewControl.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel1.PerformLayout();
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedPagesGrid)).EndInit();
			this.SelectedPagesGrid.ResumeLayout(false);
			this.SelectedPagesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AvailablePagesGrid)).EndInit();
			this.AvailablePagesGrid.ResumeLayout(false);
			this.AvailablePagesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private KSplitContainer splitContainer;
		private Enterprise.ZArchitecture.ZGrid SelectedPagesGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel7;
		private Enterprise.ZArchitecture.GUI.ZButton AddSelectedPagesButton;
		private Enterprise.ZArchitecture.GUI.ZButton RemoveSelectedPagesButton;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		private Enterprise.ZArchitecture.ZGrid AvailablePagesGrid;
		private Enterprise.ZArchitecture.GUI.ImageSelectionControl AvailableImagesPreviewControl;
		private Enterprise.ZArchitecture.GUI.ImageSelectionControl SelectedImagesPreviewControl;
	}
}
