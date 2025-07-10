using Enterprise.ZArchitecture;

namespace Enterprise.GlobalCommercialInvoice.GUI
{
	partial class GlobalCommercialInvoiceHeaderGridUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.InvoiceHeaderCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderCollectionGrid)).BeginInit();
			this.InvoiceHeaderCollectionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject);
			// 
			// InvoiceHeaderCollectionGrid
			// 
			this.InvoiceHeaderCollectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceHeaderCollectionGrid, "Headers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_RX_NKInvoiceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_RN_NKCountryExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_RN_NKCountryImport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_RN_NKCountryOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OA_SupplierAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OA_SupplierAddress_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OH_Importer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OA_ImporterAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoiceHeader)(((System.Collections.IList)(((Enterprise.GlobalCommercialInvoice.Business.GlobalCommercialInvoicePluginBusinessObject)(null)).Headers)).SyncRoot)).GIH_OA_ImporterAddress_ZAddress.OrgAddress_List)));
			this.InvoiceHeaderCollectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("26111fd6-0f60-48b7-820e-4a8ffc2e26b4", "Invoice No.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "GIH_InvoiceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("a62d1cd8-2675-4663-9549-eb3458f370bc", "Inv. Date");
			zDateEditColumnStyleInfo1.ColumnName = "GIH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("9fb6f230-1f98-4ef8-9ddb-2ecee24aa35e", "Invoice Total");
			zCalcEditColumnStyleInfo1.ColumnName = "GIH_InvoiceAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("4fea1afc-28b5-4766-9bb7-a39146dec4cf", "Curr.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GIH_RX_NKInvoiceCurrency";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("2d274e07-2de7-43be-821e-1af459426efb", "Goods Description");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "GIH_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("703d7e5f-386a-474d-ab31-954d85b50844", "Ctry/Rgn. Of Export");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "GIH_RN_NKCountryExport";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("62bad9bd-754d-4a56-8ae1-c8b2383505a8", "Ctry/Rgn. Of Import");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "GIH_RN_NKCountryImport";
			zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("2c8efbf4-7c29-459a-819c-0b3c1a31c987", "Goods Origin");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "GIH_RN_NKCountryOrigin";
			zCodeFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("440b448c-cef1-43f4-a17b-be8ae6a098ad", "Supplier");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "GIH_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo1.BindToList = "GIH_OA_SupplierAddress_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("c8a8a6c4-1219-41eb-a6d5-3eb88c42538c", "Supplier Address");
			zGuidDropEditColumnStyleInfo1.ColumnName = "GIH_OA_SupplierAddress";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("5fe4033a-7712-4896-a3e9-7126b4066558", "Importer");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GIH_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.BindToList = "GIH_OA_ImporterAddress_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.GlobalCommercialInvoice.GUI.Res.GetData("65c57ef1-d6f9-4532-9d15-d0191b6f3ef5", "Importer Address");
			zGuidDropEditColumnStyleInfo2.ColumnName = "GIH_OA_ImporterAddress";
			zGuidDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.InvoiceHeaderCollectionGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.InvoiceHeaderCollectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceHeaderCollectionGrid.GridId = "2fe22ce4-6469-47af-8b47-973ec6e21cfa";
			this.InvoiceHeaderCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceHeaderCollectionGrid.LayoutKey = "invoiceHeaderCollectionGrid";
			this.InvoiceHeaderCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceHeaderCollectionGrid.Name = "InvoiceHeaderCollectionGrid";
			this.InvoiceHeaderCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1398, 147, true);
			this.InvoiceHeaderCollectionGrid.TabIndex = 0;
			// 
			// GlobalCommercialInvoiceHeaderGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InvoiceHeaderCollectionGrid);
			this.Name = "GlobalCommercialInvoiceHeaderGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1398, 147, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderCollectionGrid)).EndInit();
			this.InvoiceHeaderCollectionGrid.ResumeLayout(false);
			this.InvoiceHeaderCollectionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGrid InvoiceHeaderCollectionGrid;
	}
}
