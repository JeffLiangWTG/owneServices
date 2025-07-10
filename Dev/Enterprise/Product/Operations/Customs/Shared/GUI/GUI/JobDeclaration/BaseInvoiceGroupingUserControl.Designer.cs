using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class BaseInvoiceGroupingUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceGroupingGroupBox;
		internal Enterprise.Customs.GUI.BaseTreeViewUserControl baseTreeViewUserControl1;
		CargoWise.Windows.UI.KSplitter splitter1;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox SelectedInvoiceGroupGroupBox;
		protected internal ZTextBox JZ_InvoiceNumberBoundGroupTextBox1;
		protected ZGroupBox ChargesGroupBox;
		public ZGrid GroupChargeGrid;
		protected internal ZLabel CoveringLabel;
		System.ComponentModel.IContainer components = null;

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.InvoiceGroupingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.baseTreeViewUserControl1 = new Enterprise.Customs.GUI.BaseTreeViewUserControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.SelectedInvoiceGroupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JZ_InvoiceNumberBoundGroupTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GroupChargeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceGroupingGroupBox.SuspendLayout();
			this.SelectedInvoiceGroupGroupBox.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// InvoiceGroupingGroupBox
			// 
			this.InvoiceGroupingGroupBox.Controls.Add(this.baseTreeViewUserControl1);
			this.InvoiceGroupingGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.InvoiceGroupingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceGroupingGroupBox.Name = "InvoiceGroupingGroupBox";
			this.InvoiceGroupingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 520, true);
			this.InvoiceGroupingGroupBox.TabIndex = 3;
			this.InvoiceGroupingGroupBox.TabStop = false;
			this.InvoiceGroupingGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("992351D9-126D-4713-A8AF-A0C7E20680E3", "Invoice Groups");
			// 
			// baseTreeViewUserControl1
			// 
			this.baseTreeViewUserControl1.AllowDrop = true;
			this.baseTreeViewUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.baseTreeViewUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.baseTreeViewUserControl1.Name = "baseTreeViewUserControl1";
			this.baseTreeViewUserControl1.ReadOnly = false;
			this.baseTreeViewUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 501, true);
			this.baseTreeViewUserControl1.TabIndex = 3;
			this.baseTreeViewUserControl1.Load += new System.EventHandler(this.baseTreeViewUserControl1_Load);
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 0, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 520, true);
			this.splitter1.TabIndex = 5;
			this.splitter1.TabStop = false;
			// 
			// SelectedInvoiceGroupGroupBox
			// 
			this.SelectedInvoiceGroupGroupBox.Controls.Add(this.JZ_InvoiceNumberBoundGroupTextBox1);
			this.SelectedInvoiceGroupGroupBox.Controls.Add(this.ChargesGroupBox);
			this.SelectedInvoiceGroupGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedInvoiceGroupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 0, true);
			this.SelectedInvoiceGroupGroupBox.Name = "SelectedInvoiceGroupGroupBox";
			this.SelectedInvoiceGroupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 520, true);
			this.SelectedInvoiceGroupGroupBox.TabIndex = 7;
			this.SelectedInvoiceGroupGroupBox.TabStop = false;
			this.SelectedInvoiceGroupGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E946AF75-753E-43CF-9B30-AB4E78F27B66", "Selected Invoice Group");
			// 
			// JZ_InvoiceNumberBoundGroupTextBox1
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceNumberBoundGroupTextBox1, "ActiveGroupHeader.JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).JZ_InvoiceNumber)));
			this.JZ_InvoiceNumberBoundGroupTextBox1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("510d04a5-728d-40cf-80c6-eba1502d630f", "Group Name");
			this.JZ_InvoiceNumberBoundGroupTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JZ_InvoiceNumberBoundGroupTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 24, true);
			this.JZ_InvoiceNumberBoundGroupTextBox1.Name = "JZ_InvoiceNumberBoundGroupTextBox1";
			this.JZ_InvoiceNumberBoundGroupTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JZ_InvoiceNumberBoundGroupTextBox1.TabIndex = 103;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ChargesGroupBox.Controls.Add(this.GroupChargeGrid);
			this.ChargesGroupBox.Controls.Add(this.CoveringLabel);
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 192, true);
			this.ChargesGroupBox.TabIndex = 3;
			this.ChargesGroupBox.TabStop = false;
			this.ChargesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8FBB04A7-E143-4DDA-9BC2-B34688C623C7", "Group Charges");
			// 
			// GroupChargeGrid
			// 
			this.GroupChargeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GroupChargeGrid, "ActiveGroupHeader.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).NoOfDecimalsForPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_Percentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_PrepaidCollect)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).Lookups.PrepaidCollectList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_DistributeBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).Lookups.ChargeDistributionBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_FullOrPartialApportionment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).Lookups.ApportionmentTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseGroupInvoiceCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceGroupHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).ActiveGroupHeader)).SyncRoot)).Charges)).SyncRoot)).J7_Calc_IsIncludedInITOT)));
			this.GroupChargeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ChargeTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("444015A5-4E86-4562-9DD0-398CE11A8F65", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "J7_ChargeType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceGroupingUserControl|a01d06b8-e5d5-4ee6-960c-99e919159bff", "Desc.", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("24230057-C11A-4E37-A027-68B0FCEF6322", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "J7_Amount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceGroupingUserControl|e19f9732-0c7b-44e6-86ef-dc84b5d3c9f4", "Amount");
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.Currencies";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C885B932-A0DC-45B7-9280-FA5B067D6828", "Curr");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "J7_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceGroupingUserControl|e19f9732-0c7b-44e6-86ef-dc84b5d3c9f4", "Amount");
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("EF188EDF-9805-4C48-9C6F-DE2101C9FCE8", "Dutiable");
			zCheckBoxColumnStyleInfo1.ColumnName = "J7_IsDutiable";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ED8429BE-BB15-4E89-B711-F0F82706B91A", "GST Apply");
			zCheckBoxColumnStyleInfo2.ColumnName = "J7_IsGSTApplicable";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "NoOfDecimalsForPercentage";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("859D70AF-886C-44C4-8C71-AA8FB7022B95", "% of Line Price");
			zCalcEditColumnStyleInfo2.ColumnName = "J7_Percentage";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.ToolTip = Enterprise.Customs.GUI.Res.GetString("BCB7EF91-4193-47BB-ACB8-86EEA66D5C85", "If you enter a percentage value here, this will be defaulted to all invoices & invoice lines that belong to this group. The amount will be calculated based on line prices.");
			zDropEditColumnStyleInfo2.BindToList = "Lookups.PrepaidCollectList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6AE922CD-5FA5-4E24-A913-47A8544DA7ED", "Pay Type");
			zDropEditColumnStyleInfo2.ColumnName = "J7_PrepaidCollect";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo3.BindToList = "Lookups.ChargeDistributionBy";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6CE9749F-3E6B-40D9-BA08-4BA7EA5E8F62", "Distribute By");
			zDropEditColumnStyleInfo3.ColumnName = "J7_DistributeBy";
			zDropEditColumnStyleInfo4.BindToList = "Lookups.ApportionmentTypeList";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("A6493712-F2ED-41B5-B59B-A47FDC48F096", "Apportion Type");
			zDropEditColumnStyleInfo4.ColumnName = "J7_FullOrPartialApportionment";
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0F9805EA-4DA2-4E71-BCAD-0A29F7296E36", "Included In Line");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo5.ColumnName = "J7_Calc_IsIncludedInITOT";
			this.GroupChargeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GroupChargeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.GroupChargeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.GroupChargeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GroupChargeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GroupChargeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GroupChargeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GroupChargeGrid.CopySelectedRowsAllowed = true;
			this.GroupChargeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupChargeGrid.GridId = "f085f3d6-8638-45d5-b70b-bcb6ba5ff48d";
			this.GroupChargeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GroupChargeGrid.LayoutKey = "GroupChargeGrid";
			this.GroupChargeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GroupChargeGrid.Name = "GroupChargeGrid";
			this.GroupChargeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 173, true);
			this.GroupChargeGrid.TabIndex = 0;
			// 
			// CoveringLabel
			// 
			this.CoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CoveringLabel.IsFontBold = true;
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CoveringLabel.Name = "CoveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 173, true);
			this.CoveringLabel.TabIndex = 106;
			this.CoveringLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ECD76770-2F16-43E3-8ADA-7859C41D9F08", "You have selected an invoice. If you want to enter a charge for an invoice, you have to do it at Invoice Headers tab. This is for GROUP INVOICES only.");
			this.CoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// BaseInvoiceGroupingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SelectedInvoiceGroupGroupBox);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.InvoiceGroupingGroupBox);
			this.Name = "BaseInvoiceGroupingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceGroupingGroupBox.ResumeLayout(false);
			this.SelectedInvoiceGroupGroupBox.ResumeLayout(false);
			this.SelectedInvoiceGroupGroupBox.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GroupChargeGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
