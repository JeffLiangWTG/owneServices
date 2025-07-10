namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class RMAForOrderLinesForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnWhsOverride = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CommonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			this.CommonOKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CommonCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommonPanel.SuspendLayout();
			this.GridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 291, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsRMAOrder);
			// 
			// CommonPanel
			// 
			this.CommonPanel.Controls.Add(this.GridPanel);
			this.CommonPanel.Controls.Add(this.CommonOKButton);
			this.CommonPanel.Controls.Add(this.CommonCancelButton);
			this.CommonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommonPanel.Name = "CommonPanel";
			this.CommonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 291, true);
			this.CommonPanel.TabIndex = 1;
			// 
			// GridPanel
			// 
			this.GridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GridPanel.Controls.Add(this.Grid);
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 244, true);
			this.GridPanel.TabIndex = 6;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).ProductPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).AvailableQtyToReturn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).QuantityToReturn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).WhsOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).PackingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).PartAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).PartAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).PartAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsRMAOrder)(null)).Lines)).SyncRoot)).SerialNumber)));
			this.Grid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("8abb9938-0084-43c9-b384-b7884b53aa48", "Part.", "Product", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ProductPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("95b0af7f-b0fc-4a31-87e1-ec2ce3755903", "Released Qty.", "Released Quantity", "");
			zCalcEditColumnStyleInfo1.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("302b2514-0b76-4ff2-818c-d063d457eae9", "Available Qty. To Return", "Available Quantity To Return", "");
			zCalcEditColumnStyleInfo2.ColumnName = "AvailableQtyToReturn";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("cf8b11de-be68-4c78-8cbd-b73839b45c7d", "Qty. To Return", "Quantity To Return", "");
			zCalcEditColumnStyleInfo3.ColumnName = "QuantityToReturn";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zGuidFindBoxColumnWhsOverride.ColumnName = "WhsOverride";
			zGuidFindBoxColumnWhsOverride.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("72a926b1-4f15-4eb2-b956-d6a8f6c90668", "Expiry Date");
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "ExpiryDate";
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("76561004-ea8d-4e14-aefe-871fee220dd8", "Packing Date");
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo2.ColumnName = "PackingDate";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("2dc4ae59-7d71-4432-b058-f86920a654e8", "Attribute 1", "Attribute 1", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "PartAttrib1";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("51959f47-f312-4dfa-ba11-5314872ea9e5", "Attribute 2", "Attribute 2", "");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PartAttrib2";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("071eaedd-9bdb-4858-8806-b33ea660dce0", "Attribute 3", "Attribute 3", "");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "PartAttrib3";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("15b294a8-0cc3-4475-86bd-073d7b033f74", "Serial Number");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "SerialNumber";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnWhsOverride);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "e92e7b01-b3ef-4a25-93b0-096bbf16d785";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 244, true);
			this.Grid.TabIndex = 0;
			// 
			// CommonOKButton
			// 
			this.CommonOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CommonOKButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("52615b53-1d1a-47a2-8853-36475452af14", "OK");
			this.CommonOKButton.IsCaptionOverridden = false;
			this.CommonOKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 262, true);
			this.CommonOKButton.Name = "CommonOKButton";
			this.CommonOKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CommonOKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CommonOKButton.TabIndex = 4;
			this.CommonOKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CommonOKButton.ToolTipCaption = null;
			this.CommonOKButton.UseVisualStyleBackColor = true;
			this.CommonOKButton.Click += new System.EventHandler(this.CommonOKButton_Click);
			// 
			// CommonCancelButton
			// 
			this.CommonCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CommonCancelButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("9442709b-fc56-41a4-bff6-20127d02f7d7", "Cancel");
			this.CommonCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CommonCancelButton.IsCaptionOverridden = false;
			this.CommonCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(697, 262, true);
			this.CommonCancelButton.Name = "CommonCancelButton";
			this.CommonCancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CommonCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CommonCancelButton.TabIndex = 5;
			this.CommonCancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CommonCancelButton.ToolTipCaption = null;
			this.CommonCancelButton.UseVisualStyleBackColor = true;
			// 
			// RMAForOrderLinesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("40aa22c5-b74f-4b97-9a26-6193f4670be2", "Generate RMA for Order Lines");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 315, true);
			this.Controls.Add(this.CommonPanel);
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsRMAOrder);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 354, true);
			this.Name = "RMAForOrderLinesForm";
			this.Text = "Generate RMA for Order Lines";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CommonPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommonPanel.ResumeLayout(false);
			this.CommonPanel.PerformLayout();
			this.GridPanel.ResumeLayout(false);
			this.GridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton CommonOKButton;
		protected ZArchitecture.GUI.ZButton CommonCancelButton;
		private ZArchitecture.GUI.ZPanel CommonPanel;
		private ZArchitecture.GUI.ZPanel GridPanel;
		private ZArchitecture.ZGrid Grid;
	}
}
