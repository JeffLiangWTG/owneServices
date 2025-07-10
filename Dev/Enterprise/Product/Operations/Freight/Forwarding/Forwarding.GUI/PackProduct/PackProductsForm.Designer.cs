namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PackProductsForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.ProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingPackLine);
			// 
			// ProductsGrid
			// 
			this.ProductsGrid.AllowNavigation = false;
			this.ProductsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProductsGrid, "Products");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackProduct)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)).SyncRoot)).D2_ProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.PackProduct)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)).SyncRoot)).D2_ProductQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackProduct)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)).SyncRoot)).D2_ProductUnitOfQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackProduct)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)).SyncRoot)).Product.OP_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.PackProduct)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(null)).Products)).SyncRoot)).D2_JO)));
			this.ProductsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "D2_ProductCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "D2_ProductQuantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "D2_ProductUnitOfQty";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackProductsForm|87142ab0-3e55-450c-8859-b9011c672fa5", "Description", "Product Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Product+OP_Desc";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "D2_JO";
			this.ProductsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ProductsGrid.GridId = "7e5ec7e8-7d56-4351-a6f5-aff28551d820";
			this.ProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductsGrid.LayoutKey = "ProductsGrid";
			this.ProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ProductsGrid.Name = "ProductsGrid";
			this.ProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 130, true);
			this.ProductsGrid.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackProductsForm|6842b257-5be7-4ec3-af51-615864bc090c", "OK");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(631, 151, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// PackProductsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 201, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackProductsForm|007e0996-7fb1-43d8-8353-d324112aa911", "Multiple Products", "Multiple Products Form.");
			this.Controls.Add(this.ProductsGrid);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingPackLine);
			this.Name = "PackProductsForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProductsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ProductsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}
