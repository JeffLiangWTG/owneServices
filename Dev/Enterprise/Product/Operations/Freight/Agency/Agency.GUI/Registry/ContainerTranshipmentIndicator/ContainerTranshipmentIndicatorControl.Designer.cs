namespace Enterprise.Freight.Agency.GUI
{
	partial class ContainerTranshipmentIndicatorControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			codeElementsPositionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			indicatorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			codeElementsPositionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(indicatorsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicatorCollection);
			// 
			// codeElementsPositionGroupBox
			// 
			codeElementsPositionGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ContainerTranshipmentIndicatorControl|56b61800-1dd0-4833-8c97-b4c154c77877", "Code Indicators");
			codeElementsPositionGroupBox.Controls.Add(indicatorsGrid);
			codeElementsPositionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			codeElementsPositionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			codeElementsPositionGroupBox.Name = "codeElementsPositionGroupBox";
			codeElementsPositionGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 5, true);
			codeElementsPositionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 336, true);
			codeElementsPositionGroupBox.TabIndex = 2;
			codeElementsPositionGroupBox.TabStop = false;
			// 
			// indicatorsGrid
			// 
			indicatorsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(indicatorsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicator)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicator)(null)).Key)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicator)(null)).Direct)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicator)(null)).Tranship)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.ContainerTranshipmentIndicator)(null)).Domestic)));
			indicatorsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Key";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "Direct";
			zTextBoxColumnStyleInfo3.ColumnName = "Tranship";
			zTextBoxColumnStyleInfo4.ColumnName = "Domestic";
			indicatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			indicatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			indicatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			indicatorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			indicatorsGrid.GridId = "e2b47724-86d7-438d-b29a-d23e6f8f36db";
			indicatorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			indicatorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			indicatorsGrid.LayoutKey = "panel1";
			indicatorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			indicatorsGrid.Name = "indicatorsGrid";
			indicatorsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			indicatorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 315, true);
			indicatorsGrid.TabIndex = 0;
			// 
			// ContainerTranshipmentIndicatorControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(codeElementsPositionGroupBox);
			this.Name = "ContainerTranshipmentIndicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			codeElementsPositionGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(indicatorsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox codeElementsPositionGroupBox;
		Enterprise.ZArchitecture.ZGrid indicatorsGrid;
	}
}
