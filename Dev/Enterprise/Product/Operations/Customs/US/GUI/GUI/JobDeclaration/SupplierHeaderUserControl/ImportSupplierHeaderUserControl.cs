using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public partial class USImportSupplierHeaderUserControl : USCustomsSupplierHeaderUserControl
	{
		public USImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeGridLayoutCore();

			FDAShipperAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;

			invoiceHeaderAIIUserControl.ManageDeclarationRelatedControlsVisibility(true);
			InvoiceHeadersBoundGrid.AllowCopyToNewRowMenuItem = true;

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			JobComInvoiceHeadersBoundGrid.Detached += OnJobComInvoiceDetached;
			JobComInvoiceHeadersBoundGrid.InnerGrid.Deleted += OnJobComInvoiceDeleted;
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutUarUzrY6sQK2iHYZ7vyVJA==";
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (InvoiceHeadersBoundGrid.ListManager != null && InvoiceHeadersBoundGrid.List != null)
			{
				var discriptor = InvoiceHeadersBoundGrid.ListManager.GetItemProperties()["JZ_InvoiceDisplaySequence"];
				InvoiceHeadersBoundGrid.List.ApplySort(discriptor, ListSortDirection.Ascending);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			PrivilegedStatusFilingDateDateEdit.DataBindings.RemoveBinding(PrivilegedStatusFilingDatePropertyNameForBinding);

			if (dataSource != null)
			{
				invoiceHeaderAIIUserControl.SetDataBinding(dataSource, dataMember);
				invoiceHeaderOrganisationUserControl.SetDataBinding(dataSource, dataMember);

				PrivilegedStatusFilingDateDateEdit.DataBindings.Add(new KBinding(PrivilegedStatusFilingDatePropertyNameForBinding, BindingSource.DataSource, "FilteredInvoices.PrivilegedStatusDateVisible", false, DataSourceUpdateMode.Never));

				declaration = dataSource as JobDeclaration;
				if (declaration != null)
				{
					declaration.US_ConsolACEInfo.ValueChanged -= IsConsolACEInfo_ValueChanged;
					declaration.US_ConsolACEInfo.ValueChanged += IsConsolACEInfo_ValueChanged;
					ShowReleaseEntryNumber(declaration.US_ConsolACE);
				}
			}
		}
		const string PrivilegedStatusFilingDatePropertyNameForBinding = "IsVisibleForBinding";

		void IsConsolACEInfo_ValueChanged(object sender, EventArgs e)
		{
			var isConsolACE = declaration == null ? ZBool.False : declaration.US_ConsolACE;
			ShowReleaseEntryNumber(isConsolACE);
		}

		void ShowReleaseEntryNumber(bool isConsolACE)
		{
			ReleaseEntryNumberCodeFindBox.Visible = isConsolACE;

			if (isConsolACE)
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.AddToAvailableColumns(JobComInvoiceHeader.Schema.US_ReleaseEntryNumber);
			}
			else
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.US_ReleaseEntryNumber);
			}
		}

		JobDeclaration declaration;

		protected void InitializeGridLayoutCore()
		{
			using (InvoiceHeadersBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceHeadersBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				InvoiceHeadersBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				InvoiceHeadersBoundGrid.SetColumnVisible(true, defaultColumnsForGrid);
				InvoiceHeadersBoundGrid.AfterBind += new EventHandler(InvoiceHeadersBoundGrid_AfterBind);
			}

			ZGuidDropEditColumnStyleInfo invoicerAddressColumnStyleInfo = (ZGuidDropEditColumnStyleInfo)InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_InvoicerDocAddress);
			invoicerAddressColumnStyleInfo.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			ZGuidDropEditColumnStyleInfo manufacturerAddressColumnStyleInfo = (ZGuidDropEditColumnStyleInfo)InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress);
			manufacturerAddressColumnStyleInfo.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			ZGuidDropEditColumnStyleInfo supplierAddressColumnStyleInfo = (ZGuidDropEditColumnStyleInfo)InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
			supplierAddressColumnStyleInfo.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;

			BaseGroupChargesGrid.ReOrderColumns(GetChargeColumnNamesInSortOrder(true));
			BaseGroupChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);

			BaseGroupChargesGrid.AfterBind += new EventHandler(BaseGroupChargesGrid_AfterBind);

			string[] columnsInOrderForInvoice = GetChargeColumnNamesInSortOrder(false);
			InvoiceChargesGrid.ReOrderColumns(columnsInOrderForInvoice);
			InvoiceChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);

			ApportionedChargesGrid.ReOrderColumns(columnsInOrderForInvoice);
			ApportionedChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);
		}

		#region Invoice Header Grid Columns

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add(JobComInvoiceHeader.Schema.BuyerOrgPK);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_OH_BuyerAgent);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer);
					columns.Add(USAddInfoSchema.Constants.US_DestinationState);
					columns.Add(USAddInfoSchema.Constants.US_UC_NKCountryOfExport);
					columns.Add(USAddInfoSchema.Constants.US_DateOfExport);
					columns.Add(USAddInfoSchema.Constants.US_DateOfExportFromCountryOfOrigin);
					columns.Add(JobComInvoiceHeader.Schema.ExporterOrgPK);
					columns.Add(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate);
					columns.Add(JobComInvoiceHeader.Schema.SellerOrgPK);
					columns.Add(JobComInvoiceHeader.Schema.JZ_OA_SellerAddress);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_OH_SellingAgent);
					columns.Add(JobComInvoiceHeader.Schema.SupplierName);
					columns.Add(USAddInfoSchema.Constants.US_TransactionsRelated);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_Volume);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_VolumeUQ);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_Weight);
					columns.Add(JobComInvoiceHeaderSchema.Constants.JZ_WeightUQ);
					columns.Add(USAddInfoSchema.Constants.US_FirstSale);
					columns.Add(USAddInfoSchema.Constants.US_DeductADDCVDDuty);
					columns.Add(JobComInvoiceHeader.Schema.ShipperOrgPK);
					columns.Add(JobComInvoiceHeader.Schema.JZ_OA_ShipperAddress);
					columns.Add(JobComInvoiceHeader.Schema.DistributorOrgPK);
					columns.Add(JobComInvoiceHeader.Schema.JZ_OA_DistributorAddress);
					columns.Add(JobComInvoiceHeader.Schema.PackagerOrgPK);
					columns.Add(JobComInvoiceHeader.Schema.JZ_OA_PackagerAddress);
					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new[]
					{
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber,
						JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier,
						JobComInvoiceHeaderSchema.Constants.JZ_OA_SupplierAddress,
						JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
						JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount,
						JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.InvoiceLineTotal,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						USAddInfoSchema.Constants.US_UC_NKCountryOfOrigin,
						JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount,
						JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency,
						JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount,
						JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency
					};
				}

				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		void InvoiceHeadersBoundGrid_AfterBind(object sender, EventArgs e)
		{
			InvoiceHeadersBoundGrid.SetColumnCaption(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer, "Importer");
			InvoiceHeadersBoundGrid.ListManager.CurrentChanged += InvoiceHeadersBoundGrid_CurrentChanged;
			ChangeDeductADD_CVDDutyVisibility();
		}

		void InvoiceHeadersBoundGrid_CurrentChanged(object sender, EventArgs e)
		{
			ChangeDeductADD_CVDDutyVisibility();
		}

		void ChangeDeductADD_CVDDutyVisibility()
		{
			UnhookInvoiceHeaderEvents();
			currentHeader = null;
			var listManager = InvoiceHeadersBoundGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				currentHeader = (JobComInvoiceHeader)listManager.GetCurrent();
				ChangeDeductADD_CVDDutyVisibilityCore();
			}
			HookInvoiceHeaderEvents();
		}

		void HookInvoiceHeaderEvents()
		{
			if (currentHeader != null)
			{
				currentHeader.JZ_IncoTermInfo.ValueChanged += JZ_IncoTermInfo_ValueChanged;
			}
		}

		void UnhookInvoiceHeaderEvents()
		{
			if (currentHeader != null)
			{
				currentHeader.JZ_IncoTermInfo.ValueChanged -= JZ_IncoTermInfo_ValueChanged;
			}
		}

		JobComInvoiceHeader currentHeader;

		#endregion

		#region Group Charges Grid

		void BaseGroupChargesGrid_AfterBind(object sender, EventArgs e)
		{
			BaseGroupChargesGrid.SetColumnMandatory(JobComInvHeaderChargeSchema.Constants.J7_ChargeType, true);
			BaseGroupChargesGrid.SetColumnMandatory(JobComInvHeaderChargeSchema.Constants.J7_Amount, true);
			BaseGroupChargesGrid.SetColumnMandatory(JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency, true);
			BaseGroupChargesGrid.SetColumnMandatory(Customs.Business.BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT, true);
			BaseGroupChargesGrid.SetColumnMandatory(JobComInvHeaderChargeSchema.Constants.J7_IsDutiable, true);
			BaseGroupChargesGrid.SetColumnMandatory(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, true);
		}

		#endregion

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (valueChangedAnnouncer != null)
			{
				valueChangedAnnouncer.OnValueChanged -= new EventHandler(valueChangedAnnouncer_OnValueChanged);
				valueChangedAnnouncer.Dispose();
			}

			if (CurrentDataItem is JobDeclaration declaration)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
				declaration.US_CargoReleaseTypeInfo.ValueChanged -= new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
				declaration.US_F_PNModeInfo.ValueChanged -= new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
				declaration.US_SPNIDTypeInfo.ValueChanged -= new EventHandler(US_SPNIDTypeInfo_ValueChanged);
				declaration.US_EntryTypeInfo.ValueChanged -= new EventHandler(US_EntryTypeInfo_ValueChanged);
				declaration.JE_TransportModeInfo.ValueChanged -= new EventHandler(JE_TransportModeInfo_ValueChanged);
				declaration.US_F_AdmissionTypeInfo.ValueChanged -= new EventHandler(US_F_AdmissionTypeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				valueChangedAnnouncer = CurrentDataItem.GetValueChangedAnnouncer();
				if (valueChangedAnnouncer != null)
				{
					valueChangedAnnouncer.OnValueChanged += new EventHandler(valueChangedAnnouncer_OnValueChanged);
				}
				ChangeVisiblityAndGUIControls();

				if (CurrentDataItem is JobDeclaration declaration)
				{
					declaration.JE_ApplicationCodeInfo.ValueChanged += new EventHandler(JE_ApplicationCodeInfo_ValueChanged);
					declaration.US_CargoReleaseTypeInfo.ValueChanged += new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
					declaration.US_F_PNModeInfo.ValueChanged += new EventHandler(US_CargoReleaseTypeInfo_ValueChanged);
					declaration.US_SPNIDTypeInfo.ValueChanged += new EventHandler(US_SPNIDTypeInfo_ValueChanged);
					declaration.US_EntryTypeInfo.ValueChanged += new EventHandler(US_EntryTypeInfo_ValueChanged);
					declaration.JE_TransportModeInfo.ValueChanged += new EventHandler(JE_TransportModeInfo_ValueChanged);
					declaration.US_F_AdmissionTypeInfo.ValueChanged += new EventHandler(US_F_AdmissionTypeInfo_ValueChanged);
				}
				ChangeACEControlsVisibility();
				ChangeDependantControlsVisibility();
			}
		}

		Customs.Business.IInvoicesProviderValueChangedAnnouncer valueChangedAnnouncer;

		void OnJobComInvoiceDetached(object sender, ModuleButtonGridOnDetachedEventArgs e)
		{
			var invoices = e.DetachedBusinessObjects.OfType<JobComInvoiceHeader>();
			invoices.ForEach(x => x.CleanUpInvoiceAfterDetachedOrDeleted("Detached"));
		}

		void OnJobComInvoiceDeleted(object sender, GridRowOnDeletedEventArgs e)
		{
			var invoices = e.DeletedBusinessObjects.OfType<JobComInvoiceHeader>();
			invoices.ForEach(x => x.CleanUpInvoiceAfterDetachedOrDeleted("Deleted"));
		}

		#region Controls Visiblity

		void valueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			ChangeVisiblityAndGUIControls();
		}

		void ChangeVisiblityAndGUIControls()
		{
			if (CurrentDataItem != null)
			{
				ElectronicInvoiceTabPage.TabVisible = CurrentDataItem.ShouldElectronicInvoicesBeVisible;
				ShowOrHideFTZRelatedControls();
			}
		}

		void ShowOrHideFTZRelatedControls()
		{
			if (CurrentDataItem != null)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				if (FTZDependentPanel != null)
				{
					FTZDependentPanel.Visible = CurrentDataItem.IsConsumptionFTZ || declaration.IsFTZAdmission;
				}

				UpdateRelatedBillPanelAndSplitDetailVisibility();
				UpdateFTZSplitShipmentDetailVisibility();
			}
		}

		void UpdateRelatedBillPanelAndSplitDetailVisibility()
		{
			if (CurrentDataItem is JobDeclaration declaration)
			{
				var isRelatedBillRelevant = declaration.IsFTZAdmission || declaration.IsACEBLNStandAlonePriorNotice;
				RelatedBillPanel.Visible = isRelatedBillRelevant;

				if (isRelatedBillRelevant)
				{
					InvoiceHeadersBoundGrid.AddToAvailableColumns(JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill.Name);
				}
				else
				{
					InvoiceHeadersBoundGrid.RemoveFromAvailableColumns(JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill.Name);
				}

				UpdateFTZSplitShipmentDetailVisibility();
			}
		}

		void UpdateFTZSplitShipmentDetailVisibility()
		{
			if (CurrentDataItem is JobDeclaration declaration)
			{
				var isFTZSplitDetailsRelevant = declaration.IsFTZSplitDetailsRelevant;
				US_SplitShipmentDetailDropEdit.Visible = isFTZSplitDetailsRelevant;

				if (isFTZSplitDetailsRelevant)
				{
					InvoiceHeadersBoundGrid.AddToAvailableColumns(JobComInvoiceHeader.Schema.US_SplitShipmentDetail);
					this.InvoiceDetailsBottomPanel.SuspendLayout();
					this.InvoiceDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 80, true);
					this.InvoiceDetailsBottomPanel.ResumeLayout(false);
					this.InvoiceDetailsBottomPanel.PerformLayout();
				}
				else
				{
					InvoiceHeadersBoundGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.US_SplitShipmentDetail);
					this.InvoiceDetailsBottomPanel.SuspendLayout();
					this.InvoiceDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 50, true);
					this.InvoiceDetailsBottomPanel.ResumeLayout(false);
					this.InvoiceDetailsBottomPanel.PerformLayout();
				}
			}
		}

		void US_EntryTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeDeductADD_CVDDutyVisibilityCore();
		}

		void JZ_IncoTermInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeDeductADD_CVDDutyVisibilityCore();
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeACEControlsVisibility();
		}
		void US_CargoReleaseTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeDependantControlsVisibility();
		}

		void US_SPNIDTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateRelatedBillPanelAndSplitDetailVisibility();
		}

		void JE_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateFTZSplitShipmentDetailVisibility();
		}

		void US_F_AdmissionTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateFTZSplitShipmentDetailVisibility();
		}

		void ChangeACEControlsVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;

			if (declaration != null)
			{
				var isACE = declaration.IsACE;
				invoiceHeaderOrganisationUserControl.UpdateControlVisibilityAndCaptions(isACE);

				var columnsAvailableForACE = new string[]
				{
					JobComInvoiceHeader.Schema.DistributorOrgPK, JobComInvoiceHeader.Schema.JZ_OA_DistributorAddress,
					JobComInvoiceHeader.Schema.PackagerOrgPK, JobComInvoiceHeader.Schema.JZ_OA_PackagerAddress,
					JobComInvoiceHeader.Schema.ShipperOrgPK, JobComInvoiceHeader.Schema.JZ_OA_ShipperAddress
				};

				var columnsUnavailableForACE = new string[]
				{
					JobComInvoiceHeader.Schema.InvoicerOrgPK, JobComInvoiceHeader.Schema.JZ_OA_InvoicerAddress,
					JobComInvoiceHeader.Schema.JZ_OH_SellingAgent, JobComInvoiceHeader.Schema.JZ_OH_BuyerAgent
				};

				if (isACE)
				{
					UpdateColumnCaption(JobComInvoiceHeadersBoundGrid.InnerGrid, JobComInvoiceHeader.Schema.ConsigneeAddressOrgPK, Enterprise.Customs.US.GUI.Res.GetData("USImportSupplierHeaderUserControl|e4fd7475-238c-48a7-b999-82c800692e46", "Consignee"));
					UpdateColumnGroupName(JobComInvoiceHeadersBoundGrid.InnerGrid, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress, Enterprise.Customs.US.GUI.Res.GetData("USImportSupplierHeaderUserControl|e4fd7475-238c-48a7-b999-82c800692e46", "Consignee"));

					JobComInvoiceHeadersBoundGrid.InnerGrid.AddToAvailableColumns(columnsAvailableForACE);
					JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(columnsUnavailableForACE);
				}
				else
				{
					UpdateColumnCaption(JobComInvoiceHeadersBoundGrid.InnerGrid, JobComInvoiceHeader.Schema.ConsigneeAddressOrgPK, Enterprise.Customs.US.GUI.Res.GetData("USImportSupplierHeaderUserControl|7c4d0a20-ed29-4eed-97dc-6d19f3f744d5", "Ultimate Consignee"));
					UpdateColumnGroupName(JobComInvoiceHeadersBoundGrid.InnerGrid, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress, Enterprise.Customs.US.GUI.Res.GetData("USImportSupplierHeaderUserControl|7c4d0a20-ed29-4eed-97dc-6d19f3f744d5", "Ultimate Consignee"));

					JobComInvoiceHeadersBoundGrid.InnerGrid.AddToAvailableColumns(columnsUnavailableForACE);
					JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(columnsAvailableForACE);
				}
			}
		}

		void ChangeDeductADD_CVDDutyVisibilityCore()
		{
			if (currentHeader != null)
			{
				DeductADD_CVDDutyDropEdit.Visible = currentHeader.IsDeductADD_CVDDutyRequired;
			}
		}

		void ChangeDependantControlsVisibility()
		{
			var declaration = (JobDeclaration)CurrentDataItem;

			if (declaration != null)
			{
				FDAShipperAddressAddressControl.Visible = !declaration.CanHavePGAFDA;
			}
		}

		void UpdateColumnCaption(ZGrid boundGrid, ZString columnName, ResourceStringData captionString)
		{
			boundGrid.SetColumnCaption(columnName, captionString.Caption);
			UpdateColumnGroupName(boundGrid, columnName, captionString);
		}

		void UpdateColumnGroupName(ZGrid boundGrid, ZString columnName, ResourceStringData captionString)
		{
			boundGrid.SetColumnGroupName(columnName, captionString);
		}

		#endregion

		string[] GetChargeColumnNamesInSortOrder(bool isForGroupCharge)
		{
			List<string> columnNamesInSortOrderList = new List<string>();
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_ChargeType);
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_Amount);
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency);
			columnNamesInSortOrderList.Add("J7_Calc_IsIncludedInInvoiceAmount");

			if (isForGroupCharge)
			{
				columnNamesInSortOrderList.Add(Customs.Business.BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT);
			}
			else
			{
				columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_IsIncludedInITOT);
			}

			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_IsDutiable);
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable);
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_ExchangeRate);
			columnNamesInSortOrderList.Add("IsJ7_ExchangeRateUserEnterable");
			columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_DistributeBy);
			columnNamesInSortOrderList.Add("ChargeCodeDescription");
			return columnNamesInSortOrderList.ToArray();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookInvoiceHeaderEvents();

				if (declaration != null)
				{
					declaration.US_ConsolACEInfo.ValueChanged -= IsConsolACEInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		internal ConvertToLocalCurrencyControl JZ_FOBAmountBoundCurrencyControlInternal => JZ_FOBAmountBoundCurrencyControl;

		internal ConvertToLocalCurrencyControl LineTotalBoundConvertToLocalCurrencyControlInternal => LineTotalBoundConvertToLocalCurrencyControl;
	}
}
