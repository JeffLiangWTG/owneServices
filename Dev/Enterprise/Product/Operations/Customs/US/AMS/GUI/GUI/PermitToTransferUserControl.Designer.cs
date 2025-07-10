namespace Enterprise.Customs.US.AMS.GUI
{
	partial class PermitToTransferUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PTTMoveDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PTTDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MovementHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MovementHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PTTMoveDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PTTDetailsGrid)).BeginInit();
			this.MovementHeadersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementHeadersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			// 
			// PTTMoveDetailsGroupBox
			//
			this.PTTMoveDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("FD2139C7-76B4-4EB6-82AD-3FBAE48140AE", "PTT Move Details");
			this.PTTMoveDetailsGroupBox.Controls.Add(this.PTTDetailsGrid);
			this.PTTMoveDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PTTMoveDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 189, true);
			this.PTTMoveDetailsGroupBox.Name = "PTTMoveDetailsGroupBox";
			this.PTTMoveDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 242, true);
			this.PTTMoveDetailsGroupBox.TabIndex = 4;
			this.PTTMoveDetailsGroupBox.TabStop = false;
			// 
			// PTTDetailsGrid
			// 
			this.PTTDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PTTDetailsGrid, "PTTMovements.MovementDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_B0)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_InBoundQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_CustomsStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveDetail)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).MovementDetails)).SyncRoot)).B9_MessageStatusDescription)));
			this.PTTDetailsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("fc52e17b-e020-44f0-8c2a-d8323c741903", "Bill");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "B9_B0";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("266a6f90-2cdd-48cc-928f-8e57748fa2c3", "Quantity");
			zCalcEditColumnStyleInfo1.ColumnName = "B9_InBoundQty";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			zTextBoxColumnStyleInfo1.ColumnName = "B9_CustomsStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo2.ColumnName = "B9_CustomsStatusDescription";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo3.ColumnName = "B9_MessageStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo4.ColumnName = "B9_MessageStatusDescription";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			this.PTTDetailsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PTTDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PTTDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PTTDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PTTDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PTTDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PTTDetailsGrid.CopySelectedRowsAllowed = true;
			this.PTTDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PTTDetailsGrid.GridId = "244d5147-67b3-4163-bf97-5e01e6c8a838";
			this.PTTDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PTTDetailsGrid.LayoutKey = "PTTDetailsGrid";
			this.PTTDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PTTDetailsGrid.Name = "PTTDetailsGrid";
			this.PTTDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 223, true);
			this.PTTDetailsGrid.TabIndex = 0;
			// 
			// MovementHeadersGroupBox
			//
			this.MovementHeadersGroupBox.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("80952635-4E49-43FF-B683-6F9BE8155C8E", "Movement Headers");
			this.MovementHeadersGroupBox.Controls.Add(this.MovementHeadersGrid);
			this.MovementHeadersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MovementHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MovementHeadersGroupBox.Name = "MovementHeadersGroupBox";
			this.MovementHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 189, true);
			this.MovementHeadersGroupBox.TabIndex = 3;
			this.MovementHeadersGroupBox.TabStop = false;
			// 
			// MovementHeadersGrid
			// 
			this.MovementHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MovementHeadersGrid, "PTTMovements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).BM_InBondCarrierID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).BM_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.AMS.Business.CusInBondMoveHeader)(((System.Collections.IList)(((Enterprise.Customs.US.AMS.Business.CusInBondHeader)(null)).PTTMovements)).SyncRoot)).BM_CustomsStatusDescription)));
			this.MovementHeadersGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BM_InBondCarrierID";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zTextBoxColumnStyleInfo5.ColumnName = "BM_CustomsStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo6.ColumnName = "BM_CustomsStatusDescription";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(286);
			this.MovementHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MovementHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MovementHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MovementHeadersGrid.CopySelectedRowsAllowed = true;
			this.MovementHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementHeadersGrid.GridId = "054d187f-bccc-4d1f-bf1f-f63b05acb4f7";
			this.MovementHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MovementHeadersGrid.LayoutKey = "MovementHeadersGrid";
			this.MovementHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MovementHeadersGrid.Name = "MovementHeadersGrid";
			this.MovementHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 170, true);
			this.MovementHeadersGrid.TabIndex = 1;
			// 
			// PermitToTransferUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PTTMoveDetailsGroupBox);
			this.Controls.Add(this.MovementHeadersGroupBox);
			this.Name = "PermitToTransferUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 431, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PTTMoveDetailsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PTTDetailsGrid)).EndInit();
			this.MovementHeadersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MovementHeadersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PTTMoveDetailsGroupBox;
		private ZArchitecture.ZGrid PTTDetailsGrid;
		private ZArchitecture.GUI.ZGroupBox MovementHeadersGroupBox;
		private ZArchitecture.ZGrid MovementHeadersGrid;
	}
}
