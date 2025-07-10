using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class BaseCustomsPackingUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected ZPanel PackingDetailsPanel;
		protected ZPanel PackingDetailGroupBoxPanel;
		protected ZGroupBox PackingDetailsGroupBox;
		protected internal Enterprise.ZArchitecture.ZGrid PackingDetailsGrid;
		protected ZPanel TotalCountPanel;
		private Enterprise.ZArchitecture.ZCalcEdit PacksEnteredCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit TotalPacksCalcEdit;
		protected CargoWise.Windows.UI.KSplitter Splitter;
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PackingDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackingDetailGroupBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalCountPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PacksEnteredCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPacksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			this.HouseBillPanel.SuspendLayout();
			this.BillFilterByAndGridPanel.SuspendLayout();
			this.FilterByPanel.SuspendLayout();
			this.BillGroupBoxPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackingDetailsPanel.SuspendLayout();
			this.PackingDetailGroupBoxPanel.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.TotalCountPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// HouseBillPanel
			// 
			this.HouseBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 221, true);
			// 
			// BillFilterByAndGridPanel
			// 
			this.BillFilterByAndGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 221, true);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 45, true);
			// 
			// BillGroupBoxPanel
			// 
			this.BillGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 176, true);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 176, true);
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 157, true);
			// 
			// PackingDetailsPanel
			// 
			this.PackingDetailsPanel.Controls.Add(this.PackingDetailGroupBoxPanel);
			this.PackingDetailsPanel.Controls.Add(this.TotalCountPanel);
			this.PackingDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackingDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 224, true);
			this.PackingDetailsPanel.Name = "PackingDetailsPanel";
			this.PackingDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 296, true);
			this.PackingDetailsPanel.TabIndex = 2;
			// 
			// PackingDetailGroupBoxPanel
			// 
			this.PackingDetailGroupBoxPanel.Controls.Add(this.PackingDetailsGroupBox);
			this.PackingDetailGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailGroupBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingDetailGroupBoxPanel.Name = "PackingDetailGroupBoxPanel";
			this.PackingDetailGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 266, true);
			this.PackingDetailGroupBoxPanel.TabIndex = 0;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 266, true);
			this.PackingDetailsGroupBox.TabIndex = 0;
			this.PackingDetailsGroupBox.TabStop = false;
			this.PackingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("82451C28-64E3-4835-82AE-3CFF24AC5B90", "Packing Details");
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.PackingDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackingDetailsGrid, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).PackTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).Lookups.LowestBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_ContainerNoOrEquipmentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).ContainersAndEquipmentsOnDeclaration_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_MarksAndNos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BasePackage)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Packages)).SyncRoot)).CW_CW_Parent)));
			this.PackingDetailsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BFEBB446-FB70-46D1-98CA-506B3D762EAD", "Pack Qty");
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "CW_PackQty";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "PackTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3CAD94D2-15A3-4277-8F01-32C0212AEEE8", "Pack Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CW_PackType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+LowestBills";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8CA76965-BB70-4004-837D-6F5FA4E48010", "Linked Bill(Lowest Bill)");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CW_HouseBill";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			zDropEditColumnStyleInfo3.BindToList = "ContainersAndEquipmentsOnDeclaration_List";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E987C5EB-5DC8-4F5B-9148-B6A109B6448F", "Container No");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CW_ContainerNoOrEquipmentNo";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("81d0325c-66d7-42c4-9aeb-7ef7de58aa9a", "Marks", "Marks & Numbers");
			zTextBoxColumnStyleInfo1.ColumnName = "CW_MarksAndNos";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0A244292-9404-4ECD-82C4-8E867744034F", "Parent");
			zDropEditColumnStyleInfo4.ColumnName = "CW_CW_Parent";
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackingDetailsGrid.GridId = "d87612d5-13b2-4f15-ab3c-83202d1e2bea";
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 248, true);
			this.PackingDetailsGrid.TabIndex = 0;
			// 
			// TotalCountPanel
			// 
			this.TotalCountPanel.Controls.Add(this.PacksEnteredCalcEdit);
			this.TotalCountPanel.Controls.Add(this.TotalPacksCalcEdit);
			this.TotalCountPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalCountPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 266, true);
			this.TotalCountPanel.Name = "TotalCountPanel";
			this.TotalCountPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 30, true);
			this.TotalCountPanel.TabIndex = 1;
			// 
			// PacksEnteredCalcEdit
			// 
			this.PacksEnteredCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PacksEnteredCalcEdit, "PackagesActualPackageCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).PackagesActualPackageCount)));
			this.PacksEnteredCalcEdit.DecimalPlaces = 0;
			this.PacksEnteredCalcEdit.Decimals = 0;
			this.PacksEnteredCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 4, true);
			this.PacksEnteredCalcEdit.Name = "PacksEnteredCalcEdit";
			this.PacksEnteredCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.PacksEnteredCalcEdit.TabIndex = 1;
			this.PacksEnteredCalcEdit.Text = "0";
			this.PacksEnteredCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PacksEnteredCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7F269514-2657-408D-94B9-67658EDE0F2F", "Packs Entered");
			// 
			// TotalPacksCalcEdit
			// 
			this.TotalPacksCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TotalPacksCalcEdit, "PackagesRequiredPackageCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).PackagesRequiredPackageCount)));
			this.TotalPacksCalcEdit.DecimalPlaces = 2;
			this.TotalPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 4, true);
			this.TotalPacksCalcEdit.Name = "TotalPacksCalcEdit";
			this.TotalPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.TotalPacksCalcEdit.TabIndex = 3;
			this.TotalPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPacksCalcEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5F3BD542-525D-4CD6-859F-BD09FBE24E2B", "Total Packs");
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 3, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			// 
			// BaseCustomsPackingUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.PackingDetailsPanel);
			this.Name = "BaseCustomsPackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 520, true);
			this.Controls.SetChildIndex(this.PackingDetailsPanel, 0);
			this.Controls.SetChildIndex(this.Splitter, 0);
			this.Controls.SetChildIndex(this.HouseBillPanel, 0);
			this.HouseBillPanel.ResumeLayout(false);
			this.HouseBillPanel.PerformLayout();
			this.BillFilterByAndGridPanel.ResumeLayout(false);
			this.BillFilterByAndGridPanel.PerformLayout();
			this.FilterByPanel.ResumeLayout(false);
			this.FilterByPanel.PerformLayout();
			this.BillGroupBoxPanel.ResumeLayout(false);
			this.BillGroupBoxPanel.PerformLayout();
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HouseBillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackingDetailsPanel.ResumeLayout(false);
			this.PackingDetailsPanel.PerformLayout();
			this.PackingDetailGroupBoxPanel.ResumeLayout(false);
			this.PackingDetailGroupBoxPanel.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.TotalCountPanel.ResumeLayout(false);
			this.TotalCountPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
