using Enterprise.ZArchitecture;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	partial class GlobalCommercialInvoiceLineGridUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo2 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoiceLineCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineCollectionGrid)).BeginInit();
			this.InvoiceLineCollectionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject);
			// 
			// InvoiceLineCollectionGrid
			// 
			this.InvoiceLineCollectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceLineCollectionGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_GIH_Header)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.ProductCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.TariffCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_Tariff1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.TariffCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_Tariff2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_InvoiceUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.InvoiceUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.VolumeUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.GrossWeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).Lookups.NetWeightUQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceLine)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Lines)).SyncRoot)).GIL_LinePrice)));
			this.InvoiceLineCollectionGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("ab3847f0-df9a-4fb6-a804-400fd8361d03", "Invoice No.");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "GIL_GIH_Header";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("765bce52-9dee-4514-9ca6-15313085f99e", "Inv. Line #");
			zCalcEditColumnStyleInfo1.ColumnName = "GIL_LineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.ProductCodeList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("e8025964-e207-4b09-9f62-f64842a6b961", "Product Code");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GIL_Product";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.BindToList = "Lookups.TariffCodeList";
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("8d6771ae-0e9c-481a-a3e0-bc0b56de8227", "Tariff 1");
			tariffColumnStyleInfo1.ColumnName = "GIL_Tariff1";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.NeedLoadNomenclatureWhenTariffNotFound = false;
			tariffColumnStyleInfo1.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo2.BindToList = "Lookups.TariffCodeList";
			tariffColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("3a8347db-f3ae-4d8e-96cb-49150d314c8e", "Tariff 2");
			tariffColumnStyleInfo2.ColumnName = "GIL_Tariff2";
			tariffColumnStyleInfo2.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo2.NeedLoadNomenclatureWhenTariffNotFound = false;
			tariffColumnStyleInfo2.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo2.SelectNomenclatureModes = null;
			tariffColumnStyleInfo2.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo2.TariffType = null;
			tariffColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("1eeb8279-666f-4294-a546-3035ae659a9f", "Goods Description");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "GIL_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("ec2c81ba-4578-49ce-99b5-75878a0d704d", "Invoice Qty");
			zCalcEditColumnStyleInfo2.ColumnName = "GIL_InvoiceQuantity";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.InvoiceUQList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("334daf40-590c-4029-ab41-ae2d4fbcc0f7", "UQ");
			zDropEditColumnStyleInfo1.ColumnName = "GIL_InvoiceUQ";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("50249b90-83ae-437c-86b6-865bb6c40cc8", "Volume");
			zCalcEditColumnStyleInfo3.ColumnName = "GIL_Volume";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.VolumeUQList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("67f03741-499f-4875-bca0-0a0209d03ed8", "UQ");
			zDropEditColumnStyleInfo2.ColumnName = "GIL_VolumeUQ";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("19846fa2-7357-4b86-9279-71161fddfb28", "Gross Weight");
			zCalcEditColumnStyleInfo4.ColumnName = "GIL_GrossWeight";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.GrossWeightUQList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("abeb951f-277a-43cd-9e4c-07df3537c9a4", "UQ");
			zDropEditColumnStyleInfo3.ColumnName = "GIL_GrossWeightUQ";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("e39b8836-4f71-4506-830c-e56a14bff166", "Net Weight");
			zCalcEditColumnStyleInfo5.ColumnName = "GIL_NetWeight";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.NetWeightUQList";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("5b659067-389d-41ed-8556-4ef9ee733463", "UQ");
			zDropEditColumnStyleInfo4.ColumnName = "GIL_NetWeightUQ";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("b3db260d-2cc6-4f02-8340-e41a64d068e5", "Price");
			zCalcEditColumnStyleInfo6.ColumnName = "GIL_LinePrice";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(tariffColumnStyleInfo2);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.InvoiceLineCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.InvoiceLineCollectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineCollectionGrid.GridId = "58b08295-3257-4cec-bbe8-9177b350ebae";
			this.InvoiceLineCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceLineCollectionGrid.LayoutKey = "invoiceLineCollectionGrid";
			this.InvoiceLineCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineCollectionGrid.Name = "InvoiceLineCollectionGrid";
			this.InvoiceLineCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1398, 147, true);
			this.InvoiceLineCollectionGrid.TabIndex = 0;
			// 
			// GlobalCommercialInvoiceLineGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InvoiceLineCollectionGrid);
			this.Name = "GlobalCommercialInvoiceLineGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1398, 147, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineCollectionGrid)).EndInit();
			this.InvoiceLineCollectionGrid.ResumeLayout(false);
			this.InvoiceLineCollectionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGrid InvoiceLineCollectionGrid;
	}
}
