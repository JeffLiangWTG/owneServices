namespace Enterprise.Customs.GUI
{
	partial class DocumentCusContainerForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Enterprise.ZArchitecture.ZLabel SelectContainersLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CancelPrintButton;
		private Enterprise.ZArchitecture.GUI.ZButton PrintButton;
		private Enterprise.ZArchitecture.ZGrid ContainersToPrintGrid;
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SelectContainersLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContainersToPrintGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelPrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersToPrintGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 340, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 22, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(252);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(253);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.DocumentCusContainerCollectionHeader);
			// 
			// SelectContainersLabel
			// 
			this.SelectContainersLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|9f6a9f0c-225e-4a0e-87d7-2a6d55a0ca4b", "Select Containers");
			this.SelectContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.SelectContainersLabel.Name = "SelectContainersLabel";
			this.SelectContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.SelectContainersLabel.TabIndex = 0;
			// 
			// ContainersToPrintGrid
			// 
			this.ContainersToPrintGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersToPrintGrid, "DocumentCusContainerCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).Container.CO_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).Container.CO_Seal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).Container.Container.RC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).Container.CO_FCL_LCL_AIR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).Container.CO_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.DocumentCusContainer)(((System.Collections.IList)(((Enterprise.Customs.Business.DocumentCusContainerCollectionHeader)(null)).DocumentCusContainerCollection)).SyncRoot)).PrintContainer)));
			this.ContainersToPrintGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ContainersToPrintGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|dfb9f6b5-0768-4e33-8489-042c9a497740", "Container #");
			zTextBoxColumnStyleInfo1.ColumnName = "Container+CO_ContainerNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|2ef24637-839b-4639-b8ed-30e234a60e25", "Seal");
			zTextBoxColumnStyleInfo2.ColumnName = "Container+CO_Seal";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|a08de3b3-b7d8-4045-b9b0-7ea366d1d9fa", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "Container+Container+RC_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "Container+CO_FCL_LCL_AIR";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|b7c1b4be-1884-4ee0-ae61-2ca18214551a", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "Container+CO_Weight";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|1b1bc069-1bdf-45a4-bb8d-1aaf76b416e2", "Print Container");
			zCheckBoxColumnStyleInfo1.ColumnName = "PrintContainer";
			this.ContainersToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersToPrintGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainersToPrintGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersToPrintGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContainersToPrintGrid.GridId = "e6c1accc-7cc7-4bd8-b05e-5a22f01e6c57";
			this.ContainersToPrintGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersToPrintGrid.LayoutKey = "zGrid1";
			this.ContainersToPrintGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 30, true);
			this.ContainersToPrintGrid.Name = "ContainersToPrintGrid";
			this.ContainersToPrintGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ContainersToPrintGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 256, true);
			this.ContainersToPrintGrid.TabIndex = 1;
			// 
			// CancelPrintButton
			// 
			this.CancelPrintButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|e5bb742b-3799-4e6b-9a12-2753e875dd03", "Cancel");
			this.CancelPrintButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelPrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 290, true);
			this.CancelPrintButton.Name = "CancelPrintButton";
			this.CancelPrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelPrintButton.TabIndex = 2;
			this.CancelPrintButton.Click += new System.EventHandler(this.CancelPrintButton_Click);
			// 
			// PrintButton
			// 
			this.PrintButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|4a214cab-51ee-4fc4-b5d8-f05ecee1ff6a", "Print");
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 290, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PrintButton.TabIndex = 3;
			this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
			// 
			// DocumentCusContainerForm
			// 
			this.AcceptButton = this.PrintButton;

			this.CancelButton = this.CancelPrintButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 362, true);
			this.ControlBox = false;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("DocumentCusContainerForm|b39b8168-3544-4b50-b4f1-6da2298a7422", "Print Containers");
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelPrintButton);
			this.Controls.Add(this.ContainersToPrintGrid);
			this.Controls.Add(this.SelectContainersLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.DocumentCusContainerCollectionHeader);
			this.DataSourceTypeName = "Enterprise.Customs.Business.DocumentCusContainerCollectionHeader";
			this.MinimizeBox = false;
			this.Name = "DocumentCusContainerForm";
			this.Controls.SetChildIndex(this.SelectContainersLabel, 0);
			this.Controls.SetChildIndex(this.ContainersToPrintGrid, 0);
			this.Controls.SetChildIndex(this.CancelPrintButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersToPrintGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
