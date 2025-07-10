using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	public partial class SubmitToCustomsForm : ECIWriteOff.SubmitToCustomsForm
	{
		private ZArchitecture.GUI.ZGroupBox manifestDeclarationsGroupBox;
		private ZArchitecture.ZGrid declarationsGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.manifestDeclarationsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.declarationsGrid = new ZArchitecture.ZGrid();
			this.RemarksGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.manifestDeclarationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.declarationsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// QueueForManifestingCheckBox
			// 
			this.queueForManifestingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 256, true);
			this.queueForManifestingCheckBox.TabIndex = 3;
			this.queueForManifestingCheckBox.Visible = false;
			// 
			// CancelBtn
			// 
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 253, true);
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CancelBtn.TabIndex = 5;
			// 
			// OkButton
			// 
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 253, true);
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.OkButton.TabIndex = 4;
			// 
			// SelectDefaultRemarksButton
			// 
			this.SelectDefaultRemarksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(401, 83, true);
			this.SelectDefaultRemarksButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 21, true);
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 58, true);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.RemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 137, true);
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 110, true);
			this.RemarksGroupBox.TabIndex = 2;
			// 
			// MessageTypeLabel
			// 
			this.MessageTypeLabel.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.MessageTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 112, true);
			this.MessageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 22, true);
			this.MessageTypeLabel.TabIndex = 1;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 283, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// ManifestDeclarationsGroupBox
			// 
			this.manifestDeclarationsGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.manifestDeclarationsGroupBox.Controls.Add(this.declarationsGrid);
			this.manifestDeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 0, true);
			this.manifestDeclarationsGroupBox.Name = "ManifestDeclarationsGroupBox";
			this.manifestDeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 109, true);
			this.manifestDeclarationsGroupBox.TabIndex = 0;
			this.manifestDeclarationsGroupBox.TabStop = false;
			this.manifestDeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2e78a855-8695-4738-9912-7b4766d8e85a", "Manifested Declarations");
			// 
			// DeclarationsGrid
			// 
			this.declarationsGrid.AllowNavigation = false;
			this.declarationsGrid.BindTo = "Declarations";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).Declarations);
			this.declarationsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("fd3b912b-d252-45f2-90e0-c58993a59783", "Declaration No.");
			zTextBoxColumnStyleInfo1.ColumnName = "JE_DeclarationReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7cfed6c4-4102-4cf5-b504-dacf4dfef497", "Entry Status");
			zTextBoxColumnStyleInfo2.ColumnName = "JE_EntryStatusDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ad2f7ddf-e385-42ae-b0e5-3f1e02430b20", "House Bill");
			zTextBoxColumnStyleInfo3.ColumnName = "JE_HouseBill";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("094762a3-43ac-4b9c-a987-d3c4863bc0db", "Destination");
			zTextBoxColumnStyleInfo4.ColumnName = "JE_RL_NKFinalDestination";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("73a78a36-f395-43e8-9896-a96fda39dbd7", "Packages");
			zCalcEditColumnStyleInfo1.ColumnName = "JE_TotalNoOfPacks";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups.SuppliersList";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("0f88db7d-fbe1-40cc-a814-c583409e32bf", "Supplier");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JE_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups.ImportersList";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("117da1b9-1de2-4bd7-8d5a-597fed9f967f", "Importer");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "JE_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("cef50b3e-1d3d-46a4-a22a-520ba4f5e3a5", "Goods Description");
			zTextBoxColumnStyleInfo5.ColumnName = "JE_GoodsDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.declarationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.declarationsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.declarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.declarationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.declarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.declarationsGrid.LayoutKey = "DeclarationsGrid";
			this.declarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.declarationsGrid.Name = "DeclarationsGrid";
			this.declarationsGrid.ReadOnly = true;
			this.declarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 90, true);
			this.declarationsGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_DeclarationReferenceInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_DeclarationReference);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_EntryStatusDescriptionInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_EntryStatusDescription);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_HouseBillInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_HouseBill);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_RL_NKFinalDestinationInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_RL_NKFinalDestination);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_TotalNoOfPacks);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_TotalNoOfPacksInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_OH_Supplier);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_OH_SupplierInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).Lookups.SuppliersList);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_OH_Importer);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_OH_ImporterInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).Lookups.ImportersList);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_GoodsDescriptionInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.Declaration.JobDeclaration)(((object)(((MessageManager)(null)).Declarations)))).JE_GoodsDescription);
			// 
			// SubmitToCustomsForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 307, true);
			this.Controls.Add(this.manifestDeclarationsGroupBox);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting.MessageManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 334, true);
			this.Name = "SubmitToCustomsForm";
			this.Controls.SetChildIndex(this.MessageTypeLabel, 0);
			this.Controls.SetChildIndex(this.queueForManifestingCheckBox, 0);
			this.Controls.SetChildIndex(this.manifestDeclarationsGroupBox, 0);
			this.Controls.SetChildIndex(this.RemarksGroupBox, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.manifestDeclarationsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.declarationsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private readonly System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
