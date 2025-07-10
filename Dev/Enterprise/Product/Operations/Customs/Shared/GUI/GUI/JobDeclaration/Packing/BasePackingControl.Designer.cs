using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class BasePackingControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		public Enterprise.ZArchitecture.GUI.ZPanel HouseBillPanel;
		protected ZPanel BillFilterByAndGridPanel;
		protected ZPanel FilterByPanel;
		private ZDropEdit FilterByDropEdit;
		protected ZPanel BillGroupBoxPanel;
		public ZGroupBox HouseBillsGroupBox;
		public ZGrid HouseBillsGrid;
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.HouseBillPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillFilterByAndGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillGroupBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HouseBillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HouseBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FilterByPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FilterByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HouseBillPanel.SuspendLayout();
			this.BillFilterByAndGridPanel.SuspendLayout();
			this.BillGroupBoxPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.FilterByPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// HouseBillPanel
			// 
			this.HouseBillPanel.Controls.Add(this.BillFilterByAndGridPanel);
			this.HouseBillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillPanel.Name = "HouseBillPanel";
			this.HouseBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 472, true);
			this.HouseBillPanel.TabIndex = 0;
			// 
			// BillFilterByAndGridPanel
			// 
			this.BillFilterByAndGridPanel.Controls.Add(this.BillGroupBoxPanel);
			this.BillFilterByAndGridPanel.Controls.Add(this.FilterByPanel);
			this.BillFilterByAndGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillFilterByAndGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillFilterByAndGridPanel.Name = "BillFilterByAndGridPanel";
			this.BillFilterByAndGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 472, true);
			this.BillFilterByAndGridPanel.TabIndex = 0;
			// 
			// BillGroupBoxPanel
			// 
			this.BillGroupBoxPanel.Controls.Add(this.HouseBillsGroupBox);
			this.BillGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.BillGroupBoxPanel.Name = "BillGroupBoxPanel";
			this.BillGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 427, true);
			this.BillGroupBoxPanel.TabIndex = 0;
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.CaptionResourceString = null;
			this.HouseBillsGroupBox.Controls.Add(this.HouseBillsGrid);
			this.HouseBillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillsGroupBox.Name = "HouseBillsGroupBox";
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 427, true);
			this.HouseBillsGroupBox.TabIndex = 0;
			this.HouseBillsGroupBox.TabStop = false;
			this.HouseBillsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DC745145-7A32-4733-9298-75F996454325", "Bills");
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseBillsGrid, "FilteredBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_BillType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).Lookups.CU_BillTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_BillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_ParentBillUniqueCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).Lookups.CU_ParentBillList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).FilteredBills)).SyncRoot)).CU_PackType)));
			this.HouseBillsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+CU_BillTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1EA62AB4-9323-4293-9028-DD650D62EB50", "Bill Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CU_BillType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6A30DDDB-985C-4CA5-87BB-96A8B3415973", "Bill Num.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CU_BillNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FC0AD16B-302D-4146-AD83-A164C8DC2717", "HBL Issue Date");
			zDateEditColumnStyleInfo1.ColumnName = "CU_IssueDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+CU_ParentBillList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4D9FC895-B98D-401A-B19E-F0ACC5645494", "Parent Bill ");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CU_ParentBillUniqueCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2BA14429-CD97-4693-BB16-3EE728D8F572", "Manifest Qty");
			zCalcEditColumnStyleInfo1.ColumnName = "CU_NoOfPacks";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.GUI.Res.GetData("BasePacking|30b066f3-1330-43e2-8867-e79d4323bfc9", "Manifest Qty/UQ");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("51DD129E-5263-476E-B928-619E2312D9B5", "UQ");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CU_PackType";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.GUI.Res.GetData("BasePacking|30b066f3-1330-43e2-8867-e79d4323bfc9", "Manifest Qty/UQ");
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.HouseBillsGrid.CopySelectedRowsAllowed = true;
			this.HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGrid.GridId = "9fdaa7fa-8551-49a5-8c57-8899985df5c3";
			this.HouseBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsGrid.LayoutKey = "HouseBillsGrid";
			this.HouseBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HouseBillsGrid.Name = "HouseBillsGrid";
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 408, true);
			this.HouseBillsGrid.TabIndex = 0;
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Controls.Add(this.FilterByDropEdit);
			this.FilterByPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilterByPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterByPanel.Name = "FilterByPanel";
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 45, true);
			this.FilterByPanel.TabIndex = 0;
			// 
			// FilterByDropEdit
			// 
			this.FilterByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterByDropEdit, "JE_BillsFilterBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_BillsFilterBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.BillFilterByList)));
			this.FilterByDropEdit.BindToList = "Lookups+BillFilterByList";
			this.FilterByDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("75947968-2bae-424f-8382-b948d6963b99", "Filter By");
			this.FilterByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 13, true);
			this.FilterByDropEdit.Name = "FilterByDropEdit";
			this.FilterByDropEdit.PreBoundMaxLength = 3;
			this.FilterByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 20, true);
			this.FilterByDropEdit.TabIndex = 1;
			// 
			// BasePackingControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseBillPanel);
			this.Name = "BasePackingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 472, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseBillPanel.ResumeLayout(false);
			this.BillFilterByAndGridPanel.ResumeLayout(false);
			this.BillGroupBoxPanel.ResumeLayout(false);
			this.HouseBillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.FilterByPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
