using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	public partial class NewManifestSelectionForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.potentialManifestsGrid = new ZArchitecture.ZGrid();
			this.potentialManifestsGroupBox = new ZGroupBox();
			this.createManifestButton = new ZButton();
			this.closeButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.potentialManifestsGrid)).BeginInit();
			this.potentialManifestsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 334, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// PotentialManifestsGrid
			// 
			this.potentialManifestsGrid.AllowNavigation = false;
			this.potentialManifestsGrid.BindTo = "PotentialManifests";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((NewManifestCreator)null).PotentialManifests);
			this.potentialManifestsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "MessageTypeDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "JE_VoyageFlightNo";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo1.ColumnName = "BarrierDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.ToolTip = "Local Transfer Date - Date of Arrival for Import Jobs or Export Date for Export Jobs";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo3.ColumnName = "JE_FormattedMasterBill";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BarrierPort";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Local Transfer Port - Arrival Port for Import Jobs or Port of Loading for Export Jobs";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "ShippingLineFullName";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo1.ColumnName = "DeclarationCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.ToolTip = "ECI Writeoff Declarations Ready for Manifest";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.potentialManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.potentialManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.potentialManifestsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.potentialManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.potentialManifestsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.potentialManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.potentialManifestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.potentialManifestsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.potentialManifestsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.potentialManifestsGrid.IsWholeRowSelectedOnClick = true;
			this.potentialManifestsGrid.LayoutKey = "zGrid1";
			this.potentialManifestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.potentialManifestsGrid.Name = "PotentialManifestsGrid";
			this.potentialManifestsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.potentialManifestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 269, true);
			this.potentialManifestsGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).MessageTypeDescriptionInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).MessageTypeDescription);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).JE_VoyageFlightNoInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).JE_VoyageFlightNo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).BarrierDate);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).BarrierDateInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).JE_FormattedMasterBillInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).JE_FormattedMasterBill);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).BarrierPortInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).BarrierPort);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).ShippingLineFullNameInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).ShippingLineFullName);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).DeclarationCount);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((PotentialManifest)(object)((NewManifestCreator)null).PotentialManifests).DeclarationCountInfo);
			// 
			// PotentialManifestsGroupBox
			// 
			this.potentialManifestsGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.potentialManifestsGroupBox.Controls.Add(this.potentialManifestsGrid);
			this.potentialManifestsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.potentialManifestsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.potentialManifestsGroupBox.Name = "PotentialManifestsGroupBox";
			this.potentialManifestsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 288, true);
			this.potentialManifestsGroupBox.TabIndex = 0;
			this.potentialManifestsGroupBox.TabStop = false;
			this.potentialManifestsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d92f6295-a4cc-47b1-9965-2aeaeff71afb", "Potential Manifests");
			// 
			// CreateManifestButton
			// 
			this.createManifestButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.createManifestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 304, true);
			this.createManifestButton.Name = "CreateManifestButton";
			this.createManifestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.createManifestButton.TabIndex = 1;
			this.createManifestButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9251f64c-b6f1-4568-8a4b-016e67653253", "Create &Manifest");
			this.createManifestButton.Click += new System.EventHandler(this.CreateManifestButton_Click);
			// 
			// CloseButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 304, true);
			this.closeButton.Name = "CloseButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.closeButton.TabIndex = 2;
			this.closeButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("0875a6e4-d7dd-432f-9cdb-931cc137eb99", "Close");
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// NewManifestSelectionForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 358, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.createManifestButton);
			this.Controls.Add(this.potentialManifestsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.NewManifestCreator";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 392, true);
			this.Name = "NewManifestSelectionForm";
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3bf5b75a-f899-4063-8227-b4f43e607dcf", "ECI Write-Off Manifest");
			this.Load += new System.EventHandler(this.NewManifestSelectionForm_Load);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.potentialManifestsGroupBox, 0);
			this.Controls.SetChildIndex(this.createManifestButton, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.potentialManifestsGrid)).EndInit();
			this.potentialManifestsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
