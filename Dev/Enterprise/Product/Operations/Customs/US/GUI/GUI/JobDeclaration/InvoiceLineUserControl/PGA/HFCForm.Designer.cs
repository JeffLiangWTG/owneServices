namespace Enterprise.Customs.US.GUI
{
	partial class HFCForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.HFCImageSentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LineNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ASHRAENumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertifyingIndividualDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HydrofluorocarbonsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HydrofluorocarbonsDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.Panel2.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.HeaderGroupBox.SuspendLayout();
			this.CertifyingIndividualDropEdit.SuspendLayout();
			this.HydrofluorocarbonsDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HydrofluorocarbonsDetailsGrid)).BeginInit();
			this.HydrofluorocarbonsDetailsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USHFCHeader);
			// 
			// HFCImageSentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.HFCImageSentCheckBox, "US_HFCImageSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_HFCImageSent)));
			this.HFCImageSentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HFCImageSentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 143, true);
			this.HFCImageSentCheckBox.Name = "HFCImageSentCheckBox";
			this.HFCImageSentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 19, true);
			this.HFCImageSentCheckBox.TabIndex = 5;
			this.HFCImageSentCheckBox.Text = "Elec. Image Submitted";
			this.HFCImageSentCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.HFCImageSentCheckBox.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.IsCaptionOverridden = true;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 4, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 16;
			this.OKButton.Text = "Close";
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			this.DetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HeaderGroupBox);
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 446, true);
			this.DetailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(170);
			// 
			// DetailsSplitContainer.Panel2
			// 
			this.DetailsSplitContainer.Panel2.Controls.Add(this.HydrofluorocarbonsDetailsGroupBox);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(170);
			this.DetailsSplitContainer.TabIndex = 18;
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8dea27e1-2a6c-4af5-a211-1eb22b71bafe", "Hydrofluorocarbons");
			this.HeaderGroupBox.Controls.Add(this.LineNoTextBox);
			this.HeaderGroupBox.Controls.Add(this.NetWeightCalcEdit);
			this.HeaderGroupBox.Controls.Add(this.ASHRAENumberTextBox);
			this.HeaderGroupBox.Controls.Add(this.CertifyingIndividualDropEdit);
			this.HeaderGroupBox.Controls.Add(this.HFCImageSentCheckBox);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 170, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			this.HeaderGroupBox.Text = "Hydrofluorocarbons";
			// 
			// LineNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineNoTextBox, "US_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_LineNo)));
			this.LineNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 19, true);
			this.LineNoTextBox.Name = "LineNoTextBox";
			this.LineNoTextBox.ReadOnly = true;
			this.LineNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.LineNoTextBox.TabIndex = 1;
			// 
			// NetWeightCalcEdit
			// 
			this.NetWeightCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "US_NetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_NetWeight)));
			this.NetWeightCalcEdit.DecimalPlaces = 2;
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 81, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.NetWeightCalcEdit.TabIndex = 3;
			this.NetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NetWeightCalcEdit.TrackDisposedAccess = true;
			// 
			// ASHRAENumberTextBox
			// 
			this.ASHRAENumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ASHRAENumberTextBox, "US_ASHRAENumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_ASHRAENumber)));
			this.ASHRAENumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 50, true);
			this.ASHRAENumberTextBox.Name = "ASHRAENumberTextBox";
			this.ASHRAENumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.ASHRAENumberTextBox.TabIndex = 2;
			// 
			// CertifyingIndividualDropEdit
			// 
			this.CertifyingIndividualDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertifyingIndividualDropEdit, "US_CertifyingIndividual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).US_CertifyingIndividual)));
			this.CertifyingIndividualDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 112, true);
			this.CertifyingIndividualDropEdit.Name = "CertifyingIndividualDropEdit";
			this.CertifyingIndividualDropEdit.PreBoundMaxLength = 2;
			this.CertifyingIndividualDropEdit.ShowDescriptionBox = false;
			this.CertifyingIndividualDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.CertifyingIndividualDropEdit.TabIndex = 4;
			// 
			// HydrofluorocarbonsDetailsGroupBox
			// 
			this.HydrofluorocarbonsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bcd2b7d4-2367-40b8-9ea2-31b6ef96247e", "Hydrofluorocarbons Details");
			this.HydrofluorocarbonsDetailsGroupBox.Controls.Add(this.HydrofluorocarbonsDetailsGrid);
			this.HydrofluorocarbonsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HydrofluorocarbonsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HydrofluorocarbonsDetailsGroupBox.Name = "HydrofluorocarbonsDetailsGroupBox";
			this.HydrofluorocarbonsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 272, true);
			this.HydrofluorocarbonsDetailsGroupBox.TabIndex = 10;
			this.HydrofluorocarbonsDetailsGroupBox.TabStop = false;
			this.HydrofluorocarbonsDetailsGroupBox.Text = "Hydrofluorocarbons Details";
			// 
			// HydrofluorocarbonsDetailsGrid
			// 
			this.HydrofluorocarbonsDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HydrofluorocarbonsDetailsGrid, "USHFCDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_LPCONumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_NameOfActiveIngredient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USHFCDetail)(((System.Collections.IList)(((Enterprise.Customs.US.Business.USHFCHeader)(null)).USHFCDetails)).SyncRoot)).US_ActiveIngredientPercentage)));
			this.HydrofluorocarbonsDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "US_LPCONumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.ColumnName = "US_NameOfActiveIngredient";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "US_ActiveIngredientPercentage";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HydrofluorocarbonsDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HydrofluorocarbonsDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HydrofluorocarbonsDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HydrofluorocarbonsDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HydrofluorocarbonsDetailsGrid.GridId = "2464a765-ca91-4a82-9513-4ed697b9d1de";
			this.HydrofluorocarbonsDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HydrofluorocarbonsDetailsGrid.LayoutKey = "HydrofluorocarbonsDetailsGrid";
			this.HydrofluorocarbonsDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HydrofluorocarbonsDetailsGrid.Name = "HydrofluorocarbonsDetailsGrid";
			this.HydrofluorocarbonsDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 253, true);
			this.HydrofluorocarbonsDetailsGrid.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 446, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 30, true);
			this.BottomPanel.TabIndex = 19;
			// 
			// HFCForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fece9307-2885-44e1-ae0d-127ad57cfb3b", "Hydrofluorocarbons Edit");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 500, true);
			this.Controls.Add(this.DetailsSplitContainer);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USHFCHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "HFCForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Hydrofluorocarbons Edit";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.DetailsSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			this.DetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).EndInit();
			this.DetailsSplitContainer.ResumeLayout(false);
			this.DetailsSplitContainer.PerformLayout();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.CertifyingIndividualDropEdit.ResumeLayout(true);
			this.CertifyingIndividualDropEdit.PerformLayout();
			this.HydrofluorocarbonsDetailsGroupBox.ResumeLayout(false);
			this.HydrofluorocarbonsDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HydrofluorocarbonsDetailsGrid)).EndInit();
			this.HydrofluorocarbonsDetailsGrid.ResumeLayout(false);
			this.HydrofluorocarbonsDetailsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCheckBox HFCImageSentCheckBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZGroupBox HydrofluorocarbonsDetailsGroupBox;
		internal ZArchitecture.ZGrid HydrofluorocarbonsDetailsGrid;
		private ZArchitecture.GUI.ZDropEdit CertifyingIndividualDropEdit;
		internal ZArchitecture.ZTextBox ASHRAENumberTextBox;
		private ZArchitecture.ZTextBox LineNoTextBox;
		internal ZArchitecture.ZCalcEdit NetWeightCalcEdit;
	}
}
