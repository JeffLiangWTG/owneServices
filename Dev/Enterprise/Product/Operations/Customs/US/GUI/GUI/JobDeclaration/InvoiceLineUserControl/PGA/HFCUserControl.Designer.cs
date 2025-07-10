namespace Enterprise.Customs.US.GUI
{
	partial class HFCUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.HFCDetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HFCGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HFCGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ViewEditPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ViewEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HFCDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HFCGridDetail = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HFCDetailsSplitContainer)).BeginInit();
			this.HFCDetailsSplitContainer.Panel1.SuspendLayout();
			this.HFCDetailsSplitContainer.Panel2.SuspendLayout();
			this.HFCDetailsSplitContainer.SuspendLayout();
			this.HFCGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HFCGrid)).BeginInit();
			this.HFCGrid.SuspendLayout();
			this.ViewEditPanel.SuspendLayout();
			this.HFCDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HFCGridDetail)).BeginInit();
			this.HFCGridDetail.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USHFCHeader);
			// 
			// HFCDetailsSplitContainer
			// 
			this.HFCDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HFCDetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HFCDetailsSplitContainer.Name = "HFCDetailsSplitContainer";
			this.HFCDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// HFCDetailsSplitContainer.Panel1
			// 
			this.HFCDetailsSplitContainer.Panel1.Controls.Add(this.HFCGroupBox);
			this.HFCDetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			this.HFCDetailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// HFCDetailsSplitContainer.Panel2
			// 
			this.HFCDetailsSplitContainer.Panel2.Controls.Add(this.HFCDetailsGroupBox);
			this.HFCDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(187);
			this.HFCDetailsSplitContainer.TabIndex = 1;
			// 
			// HFCGroupBox
			// 
			this.HFCGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("26ddd5bd-febc-445c-8a06-bc28b56fdd99", "Environmental Protection Agency - Hydrofluorocarbons");
			this.HFCGroupBox.Controls.Add(this.HFCGrid);
			this.HFCGroupBox.Controls.Add(this.ViewEditPanel);
			this.HFCGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HFCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HFCGroupBox.Name = "HFCGroupBox";
			this.HFCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 187, true);
			this.HFCGroupBox.TabIndex = 0;
			this.HFCGroupBox.TabStop = false;
			// 
			// HFCGrid
			// 
			this.HFCGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HFCGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_ASHRAENumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_CertifyingIndividual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_HFCImageSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_TrackingStatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).StatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).StatusDate)));
			this.HFCGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "US_LineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "US_ASHRAENumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "US_NetWeight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "US_CertifyingIndividual";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "US_HFCImageSent";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a0c81ea0-e979-449a-8006-7b0ef8731113", "Message Status");
			zTextBoxColumnStyleInfo2.ColumnName = "US_TrackingStatusDesc";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4dc8d4ae-60ee-4dd8-a026-bce1a6fc1e40", "PGA Line Status");
			zTextBoxColumnStyleInfo3.ColumnName = "Status";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d75b63e6-2619-4015-a913-23b9f15b3512", "PGA Line Status Desc.");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDesc";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2ea25fc5-bdb0-4ea1-8a66-345124336f1b", "PGA Line Status Date");
			zDateEditColumnStyleInfo1.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.HFCGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HFCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HFCGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.HFCGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HFCGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.HFCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HFCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.HFCGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.HFCGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.HFCGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HFCGrid.GridId = "372f64af-6efd-4ddb-86e0-90ed7434a1c7";
			this.HFCGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HFCGrid.LayoutKey = "EPAGrid";
			this.HFCGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HFCGrid.Name = "HFCGrid";
			this.HFCGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 143, true);
			this.HFCGrid.TabIndex = 2;
			// 
			// ViewEditPanel
			// 
			this.ViewEditPanel.Controls.Add(this.ViewEditButton);
			this.ViewEditPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ViewEditPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 159, true);
			this.ViewEditPanel.Name = "ViewEditPanel";
			this.ViewEditPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 25, true);
			this.ViewEditPanel.TabIndex = 2;
			// 
			// ViewEditButton
			// 
			this.ViewEditButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f7738c08-7e9b-4450-9f39-03b557dea00d", "View/Edit");
			this.ViewEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ViewEditButton.Name = "ViewEditButton";
			this.ViewEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewEditButton.TabIndex = 0;
			this.ViewEditButton.ToolTipCaption = null;
			this.ViewEditButton.UseVisualStyleBackColor = true;
			this.ViewEditButton.Click += new System.EventHandler(this.ViewEditButton_Click);
			// 
			// HFCDetailsGroupBox
			// 
			this.HFCDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2cef1447-d224-4a62-b258-6bcb7c7a5552", "Hydrofluorocarbons Details");
			this.HFCDetailsGroupBox.Controls.Add(this.HFCGridDetail);
			this.HFCDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HFCDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HFCDetailsGroupBox.Name = "HFCDetailsGroupBox";
			this.HFCDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 119, true);
			this.HFCDetailsGroupBox.TabIndex = 0;
			this.HFCDetailsGroupBox.TabStop = false;
			// 
			// HFCGridDetail
			// 
			this.HFCGridDetail.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HFCGridDetail, "USHFCDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_LPCONumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_NameOfActiveIngredient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_ActiveIngredientPercentage)));
			this.HFCGridDetail.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "US_LPCONumber";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo6.ColumnName = "US_NameOfActiveIngredient";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "US_ActiveIngredientPercentage";
			zCalcEditColumnStyleInfo3.Decimals = 4;
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HFCGridDetail.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.HFCGridDetail.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.HFCGridDetail.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.HFCGridDetail.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HFCGridDetail.GridId = "372f64af-6efd-4ddb-86e0-90ed7434a1c7";
			this.HFCGridDetail.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HFCGridDetail.LayoutKey = "EPAGrid";
			this.HFCGridDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HFCGridDetail.Name = "HFCGridDetail";
			this.HFCGridDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 100, true);
			this.HFCGridDetail.TabIndex = 3;
			// 
			// HFCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.HFCDetailsSplitContainer);
			this.Name = "HFCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HFCDetailsSplitContainer.Panel1.ResumeLayout(false);
			this.HFCDetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HFCDetailsSplitContainer)).EndInit();
			this.HFCDetailsSplitContainer.ResumeLayout(false);
			this.HFCDetailsSplitContainer.PerformLayout();
			this.HFCGroupBox.ResumeLayout(false);
			this.HFCGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HFCGrid)).EndInit();
			this.HFCGrid.ResumeLayout(false);
			this.HFCGrid.PerformLayout();
			this.ViewEditPanel.ResumeLayout(false);
			this.ViewEditPanel.PerformLayout();
			this.HFCDetailsGroupBox.ResumeLayout(false);
			this.HFCDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HFCGridDetail)).EndInit();
			this.HFCGridDetail.ResumeLayout(false);
			this.HFCGridDetail.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer HFCDetailsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox HFCGroupBox;
		internal ZArchitecture.GUI.ZButton ViewEditButton;
		private ZArchitecture.GUI.ZGroupBox HFCDetailsGroupBox;
		internal ZArchitecture.ZGrid HFCGrid;
		protected ZArchitecture.ZGrid HFCGridDetail;
		private ZArchitecture.GUI.ZPanel ViewEditPanel;

	}
}
