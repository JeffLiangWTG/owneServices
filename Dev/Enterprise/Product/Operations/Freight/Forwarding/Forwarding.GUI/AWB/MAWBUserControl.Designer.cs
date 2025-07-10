using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class MAWBUserControl : AWBUserControl
	{
		protected ZTextBox EH_KnownConsignorCodeTextBox;
		ZGrid OtherChargesGrid;
		private ZTabPage zTabPage1;
		private ZGrid specialHandingGrid;

		private void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.OtherChargesGrid = new ZGrid();
			this.zTabPage1 = new ZTabPage();
			this.specialHandingGrid = new ZGrid();
			this.EH_KnownConsignorCodeTextBox = new ZTextBox();
			this.MainPanel.SuspendLayout();
			this.AWBBottomPanel.SuspendLayout();
			this.AWBTopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OtherChargesGrid)).BeginInit();
			this.zTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.specialHandingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// AWBBottomPanel
			// 
			this.AWBBottomPanel.Controls.Add(this.OtherChargesGrid);
			this.AWBBottomPanel.Controls.SetChildIndex(this.AWBPanel, 0);
			this.AWBBottomPanel.Controls.SetChildIndex(this.OtherChargesGrid, 0);
			// 
			// AWBImagelabel
			// 
			this.AWBImagelabel.Controls.Add(this.EH_KnownConsignorCodeTextBox);
			//
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ForwardingConsol);
			// 
			// OtherChargesGrid
			// 
			this.OtherChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherChargesGrid, "AWBHeaderManager.AWBOtherCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.OtherChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EO_ChargeCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "EO_EntitlementCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "EO_ChargeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EO_Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OtherChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OtherChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OtherChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OtherChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OtherChargesGrid.GridId = "0438792f-d123-482d-9fa1-71cd2ac1bc06";
			this.OtherChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherChargesGrid.LayoutKey = "OtherChargesGrid";
			this.OtherChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 1008, true);
			this.OtherChargesGrid.Name = "OtherChargesGrid";
			this.OtherChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 108, true);
			this.OtherChargesGrid.TabIndex = 302;
			// 
			// zTabPage1
			// 
			this.AccountingInformationTabControl.Controls.Add(this.zTabPage1);
			this.zTabPage1.Controls.Add(this.specialHandingGrid);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 107, true);
			this.zTabPage1.TabIndex = 3;
			this.zTabPage1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|b1b281d6-c31c-4f47-b541-a8a401977a51", "Special Handling Code", "Special Handling Code", "Special Handling Code", "");
			this.zTabPage1.UseVisualStyleBackColor = true;
			// 
			// specialHandingGrid
			// 
			this.specialHandingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.specialHandingGrid, "AWBHeaderManager.AWBSpecialHandlingItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AWB.ExportAWBSpecialHandling)(((System.Collections.IList)(((Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBSpecialHandlingItems)).SyncRoot)).EP_SpecialHandling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AWB.ExportAWBSpecialHandling)(((System.Collections.IList)(((Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Business.AWB.IAWBParent)(null)).AWBHeader)).SyncRoot)).AWBSpecialHandlingItems)).SyncRoot)).SpecialHandlingDescription)));
			this.specialHandingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|b1b281d6-c31c-4f47-b541-a8a401977a51", "Special Handling Code", "Special Handling Code", "Special Handling Code", "");
			zDropEditColumnStyleInfo3.ColumnName = "EP_SpecialHandling";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|4871fe99-607f-45af-9e4d-cd72968737bd", "Special Handling Description", "Special Handling Description", "Special Handling Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "SpecialHandlingDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(370);
			this.specialHandingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.specialHandingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.specialHandingGrid.GridId = "78854832-af95-44d2-a34a-5faa7245ebd8";
			this.specialHandingGrid.CopySelectedRowsAllowed = true;
			this.specialHandingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.specialHandingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.specialHandingGrid.LayoutKey = "zGrid1";
			this.specialHandingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.specialHandingGrid.Name = "specialHandingGrid";
			this.specialHandingGrid.PreferredColumnWidth = 200;
			this.specialHandingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 101, true);
			this.specialHandingGrid.TabIndex = 0;
			// 
			// EH_KnownConsignorCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EH_KnownConsignorCodeTextBox, "AWBHeaderManager.EH_KnownConsignorCode");
			this.EH_KnownConsignorCodeTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBUserControl|46144BE4-1B1A-4944-B157-7AC1A3BFF2A9", "Security Status");
			this.EH_KnownConsignorCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(896, 50, true);
			this.EH_KnownConsignorCodeTextBox.Name = "EH_KnownConsignorCodeTextBox";
			this.EH_KnownConsignorCodeTextBox.ReadOnly = true;
			this.EH_KnownConsignorCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.EH_KnownConsignorCodeTextBox.TabIndex = 232;
			// 
			// MAWBUserControl
			// 
			this.Name = "MAWBUserControl";
			this.MainPanel.ResumeLayout(false);
			this.AWBBottomPanel.ResumeLayout(false);
			this.AWBTopPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OtherChargesGrid)).EndInit();
			this.zTabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.specialHandingGrid)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
