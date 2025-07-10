using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class USImportInvoiceLineUserControl : USInvoiceLineUserControl
	{
		public USImportInvoiceLineUserControl()
		{
			InitializeComponent();
			ElectronicInvoiceNeedsRegenerationLabel.Text = NoOfAIILinesNotMatchLineGrouping;
			JI_OA_ManufacturerAddressAddressControl.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			InitializeGridLayoutCore();

			//These come from Customs.GUI.USInvoiceLineUserControl in a different group box to what we want to place them
			JI_CountryOfOriginBoundFindBox.Visible = false;

			this.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);

			uNDGDataManager = new UNDGDataItemFormManager(CustomsInvoiceLinesBoundGrid);
			uNDGDataManager.Initialize(DGLinkLabel, DGGuidFindBox, FlashPointTempCalcEdit, HazardousMaterialContactGuidFindBox);
			saveCustomsInvoiceLinesBoundGridID = CustomsInvoiceLinesBoundGrid.GridId;

			LineDetailTabControl.ControlAdded += LineDetailTabControl_ControlAdded;
			CusContainerInvoiceLineGrid.VisibleChanged += CusContainerInvoiceLineGrid_VisibleChanged;
			ACELaceyActUserControl.Visible = false;

			manufacturerFindBox = (ZAddressFindBox.Bare)JI_OA_ManufacturerAddressAddressControl.Controls["OrganisationFindBox"];
			manufacturerFindBox.ReadOnlyChanged += ManufacturerFindBox_ReadOnlyChanged;
		}

		readonly ZAddressFindBox.Bare manufacturerFindBox;

		void ManufacturerFindBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			JI_OA_ManufacturerAddressAddressControl.Enabled = !JI_OA_ManufacturerAddressAddressControl.ReadOnly;
		}

		const string NoOfAIILinesNotMatchLineGrouping = "The number of AII Lines that should be generated based on the Line Grouping does not match the actual AII Lines that currently exist.\r\nPlease re-run Brokerage -> Generate AII Lines.";

		void LineDetailTabControl_ControlAdded(object sender, ControlEventArgs e)
		{
			//fixed in WI00083522, but keep it for a while to see if there is another cause.
			var tabPage = e.Control as TabPage;
			if (tabPage != null && LineDetailTabControl.TabPages.Cast<ZTabPage>().Where(x => x == tabPage).Take(2).Count() > 1)
			{
				ErrorReporter.ReportOnce(string.Format("A tab is inserted more than one", tabPage.Name));
			}
		}
		readonly UNDGDataItemFormManager uNDGDataManager;

		protected override ZArchitecture.Modules.ModuleIdentifier ClassificationModuleID
		{
			get { return Enterprise.ZArchitecture.Modules.ModuleIDs.ImportClassification; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			PrivilegedStatusFilingDateDateEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			TaxRateCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);
			TaxQuantityCalcEdit.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				PrivilegedStatusFilingDateDateEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.PrivilegedStatusDateVisible", false, DataSourceUpdateMode.Never));
				TaxRateCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsTaxRateSpecifiedManually", false, DataSourceUpdateMode.Never));
				TaxQuantityCalcEdit.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "FilteredInvoiceLines.IsTaxQtyRequired", false, DataSourceUpdateMode.Never));
			}
		}

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (valueChangedAnnouncer != null)
			{
				valueChangedAnnouncer.OnValueChanged -= new EventHandler(valueChangedAnnouncer_OnValueChanged);
				valueChangedAnnouncer.Dispose();
			}

			var topGroupInvoice = CurrentDataItem?.TopGroupInvoice;
			if (topGroupInvoice != null)
			{
				var declaration = topGroupInvoice.JobDeclaration;
				if (declaration != null && declaration.IsPersistent)
				{
					declaration.CusContainers.CountChanged -= CusContainers_CountChanged;
				}
			}
		}

		void CusContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			needRefreshContainerPivot = true;
		}
		bool needRefreshContainerPivot;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				var topGroupInvoice = CurrentDataItem.TopGroupInvoice;
				if (topGroupInvoice != null)
				{
					var declaration = topGroupInvoice.JobDeclaration;
					if (declaration != null && declaration.IsPersistent)
					{
						declaration.CusContainers.CountChanged -= CusContainers_CountChanged;
						declaration.CusContainers.CountChanged += CusContainers_CountChanged;
					}
				}

				valueChangedAnnouncer = CurrentDataItem.GetValueChangedAnnouncer();
				if (valueChangedAnnouncer != null)
				{
					valueChangedAnnouncer.OnValueChanged += new EventHandler(valueChangedAnnouncer_OnValueChanged);
				}
				valueChangedAnnouncer_OnValueChanged(this, EventArgs.Empty);
			}
		}

		Customs.Business.IInvoicesProviderValueChangedAnnouncer valueChangedAnnouncer;

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		public new InvoiceLineChargesUserControl InvoiceLineCharges
		{
			get { return (InvoiceLineChargesUserControl)base.InvoiceLineCharges; }
		}

		void valueChangedAnnouncer_OnValueChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				ShowOrHideLineGroupingDetails();
				ShowOrHideElectronicInvoiceRelatedControls();
				ShowOrHideFTZRelatedControls();
				ShowOrHideFSISTabPage();
				ShowOrHideWHSQuantityDetails();
				UpdateControlAndColumnCaptions();
			}
		}

		void InitializeGridLayoutCore()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, defaultColumnsForGrid);

				CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
			}

			InvoiceLineCharges.ChargesGrid.ReOrderColumns(ChargeColumnNamesInSortOrder);
			InvoiceLineCharges.ChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);

			InvoiceLineCharges.ApportionedChargesGrid.ReOrderColumns(ChargeColumnNamesInSortOrder);
			InvoiceLineCharges.ApportionedChargesGrid.RemoveFromAvailableColumns(JobComInvHeaderChargeSchema.Constants.J7_PrepaidCollect);

			InvoiceLineCharges.AddOverrideCheckBoxes();

			ZGuidDropEditColumnStyleInfo manufacturerAddressColumnStyleInfo = (ZGuidDropEditColumnStyleInfo)CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress);
			if (manufacturerAddressColumnStyleInfo != null)
			{
				manufacturerAddressColumnStyleInfo.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			}
			ZGuidDropEditColumnStyleInfo jI_OA_ManufacturerAddressControlInfo = (ZGuidDropEditColumnStyleInfo)FDALinesGrid.GetColumnStyle(FDA.Schema.US_FDAManufacturerAddress);
			if (jI_OA_ManufacturerAddressControlInfo != null)
			{
				jI_OA_ManufacturerAddressControlInfo.Parse = AddressParser.GetAddressPKFromMatchingMIDCode;
			}
			CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.JI_LinePrice.Name).CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|d8d41f41-7969-4af2-b957-e99258fb7032", "Line Price");
			CustomsInvoiceLinesBoundGrid.GetColumnStyle("UnitPrice").CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|cec4f9bb-b264-4d76-96c9-5821eba5d72d", "Price/Unit");
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (CurrentDataItem != null)
			{
				UnHookLineGroupingRangeEvents(currentLineGroupingRange);
				currentLineGroupingRange = null;
				LineGroupingSequenceGridListManager_PositionChanged(null, null);
				ShowOrHideLineGroupingDetails();
				TariffCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption = GetTariffCaption();
				ChangeSupTariffComponentType();
			}
		}

		#region Grid Columns

		#region Invoice Line Grid Columns

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					List<string> columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib4);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib5);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomAttrib6);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomTextBlob1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_OrderNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_ParentID);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib1);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib2);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartAttrib3);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_SerialNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.UnitPrice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Volume);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_VolumeUQ);
					columns.Add(USAddInfoSchema.Constants.US_UC_NKCountryOfExport);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsThirdQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsThirdUnitQty);
					columns.Add(JobComInvoiceLine.Schema.ConsigneeAddressOrgPK);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_OA_ConsigneeAddress);
					columns.Add(USAddInfoSchema.Constants.US_ADD_NA);
					columns.Add(USAddInfoSchema.Constants.US_CVD_NA);
					columns.Add(USAddInfoSchema.Constants.US_SchDLoading);
					columns.Add(USAddInfoSchema.Constants.US_DateOfExport);
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
					List<string> columns = new List<string>();
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
					columns.Add(JobComInvoiceLine.Schema.EntryNumberAndMergeLineNumber);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CC);
					columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsQuantity);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsUnitQty);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
					columns.Add(JobComInvoiceLine.Schema.SupTariffFormatted);
					columns.Add(USAddInfoSchema.Constants.US_98GoodsValue);
					columns.Add(USAddInfoSchema.Constants.US_98ValueInvCurr);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
					columns.Add(USAddInfoSchema.Constants.US_UC_NKCountryOfOrigin);
					columns.Add(USAddInfoSchema.Constants.US_SPI);
					columns.Add(USAddInfoSchema.Constants.US_SecondarySPI);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_Weight);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_WeightUQ);
					columns.Add(JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code);
					defaultColumnsForGrid = columns.ToArray();
				}

				return defaultColumnsForGrid;
			}
		}
		string[] defaultColumnsForGrid;

		#endregion

		string[] ChargeColumnNamesInSortOrder
		{
			get
			{
				if (chargeColumnNamesInSortOrder == null)
				{
					List<string> columnNamesInSortOrderList = new List<string>();
					columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_ChargeType);
					columnNamesInSortOrderList.Add(InvoiceLineCharge.Schema.ChargeCodeDescription);
					columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_Amount);
					columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency);
					columnNamesInSortOrderList.Add(InvoiceLineCharge.Schema.IsJ7_ExchangeRateUserEnterable);
					columnNamesInSortOrderList.Add(JobComInvHeaderChargeSchema.Constants.J7_ExchangeRate);
					chargeColumnNamesInSortOrder = columnNamesInSortOrderList.ToArray();
				}
				return chargeColumnNamesInSortOrder;
			}
		}
		string[] chargeColumnNamesInSortOrder;

		#endregion

		#region Control Visibility

		void ShowOrHideWHSQuantityDetails()
		{
			var currentDataItem = CurrentDataItem;
			var isInwardBondedWarehousingEnabled = false;
			var isOutwardBondedWarehousingEnabled = false;
			var isConsumptionFTZ = false;
			var isInwardBondedWarehousingEnabledAndImportByExternalBroker = false;
			if (currentDataItem != null)
			{
				isConsumptionFTZ = currentDataItem.IsConsumptionFTZ;
				isInwardBondedWarehousingEnabled = currentDataItem.IsInwardBondedWarehousingEnabled;
				isOutwardBondedWarehousingEnabled = currentDataItem.IsOutwardBondedWarehousingEnabled;
				isInwardBondedWarehousingEnabledAndImportByExternalBroker = isInwardBondedWarehousingEnabled && currentDataItem.IsImportByExternalBroker;
			}
			using (this.CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				this.CustomsInvoiceLinesBoundGrid.SetAvailability(isInwardBondedWarehousingEnabled, JobComInvoiceLine.Schema.BondedWhsQuantityForGUI);
				this.CustomsInvoiceLinesBoundGrid.SetAvailability(isOutwardBondedWarehousingEnabled || isInwardBondedWarehousingEnabledAndImportByExternalBroker, JobComInvoiceLine.Schema.US_WHSEntryLineNo);
				this.CustomsInvoiceLinesBoundGrid.SetAvailability(isOutwardBondedWarehousingEnabled && isConsumptionFTZ, JobComInvoiceLine.Schema.US_WHSEntryNumber);
				this.CustomsInvoiceLinesBoundGrid.GridId = saveCustomsInvoiceLinesBoundGridID + (isInwardBondedWarehousingEnabled ? "INWARD" : isOutwardBondedWarehousingEnabled ? "OUTWARD" : "");
			}
		}
		readonly string saveCustomsInvoiceLinesBoundGridID;

		void UpdateControlAndColumnCaptions()
		{
			var declaration = CurrentDataItem as JobDeclaration;
			if (declaration != null)
			{
				if (declaration.IsACE)
				{
					JI_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption = "Consignee";
					UpdateColumnCaption(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.ConsigneeAddressOrgPK, Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|1fdea4ad-f333-4d2e-8d66-f9b99986f6df", "Consignee"));
					UpdateColumnGroupName(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|1fdea4ad-f333-4d2e-8d66-f9b99986f6df", "Consignee"));
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.US_TSCAIndicator, JobComInvoiceLine.Schema.US_TSCAName);
					CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.US_ExclusionNumber, JobComInvoiceLine.Schema.US_ProductExclusion);
				}
				else
				{
					JI_OA_ConsigneeAddressControl.GetExtension<ILabelCaptionRenderer>().Caption = "Ult. Consignee";
					UpdateColumnCaption(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.ConsigneeAddressOrgPK, Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|4c35f687-a9c0-4bee-a524-05129ecc1cef", "Ultimate Consignee"));
					UpdateColumnGroupName(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|4c35f687-a9c0-4bee-a524-05129ecc1cef", "Ultimate Consignee"));
					CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.US_TSCAIndicator, JobComInvoiceLine.Schema.US_TSCAName);
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.US_ExclusionNumber, JobComInvoiceLine.Schema.US_ProductExclusion);
				}
			}
		}

		protected void UpdateColumnCaption(ZGrid boundGrid, ZString columnName, ResourceStringData captionString)
		{
			boundGrid.SetColumnCaption(columnName, captionString.Caption);
			UpdateColumnGroupName(boundGrid, columnName, captionString);
		}

		protected void UpdateColumnGroupName(ZGrid boundGrid, ZString columnName, ResourceStringData captionString)
		{
			boundGrid.SetColumnGroupName(columnName, captionString);
		}

		void ShowOrHideLineGroupingDetails()
		{
			bool isLineGroupingEnabled = false;
			bool isLineGroupingEnabledForLine = false;
			bool isChildLine = false;
			if (currentInvoiceLine != null)
			{
				JobDeclaration declaration = currentInvoiceLine.Declaration;
				isLineGroupingEnabled = declaration != null && declaration.IsLineGroupingSupported;
				isLineGroupingEnabledForLine = currentInvoiceLine.IsLineGroupingEnabled;
				isChildLine = currentInvoiceLine.IsChildLine;
			}
			if (isLineGroupingEnabled)
			{
				int expectedIndex = LineDetailTabControl.TabPages.IndexOf(LicencePermitsDetailsTabPage) + 1;
				int actualIndex = LineDetailTabControl.TabPages.IndexOf(LineGroupingTabPage);
				if (actualIndex != expectedIndex && expectedIndex >= 0)
				{
					LineDetailTabControl.TabPages.Remove(LineGroupingTabPage);
					LineDetailTabControl.TabPages.Insert(LineGroupingTabPage, expectedIndex);
				}
			}

			LineGroupingTabPage.TabVisible = isLineGroupingEnabled;
			ChildLineGroupingSequencesMessageLabel.Visible = isLineGroupingEnabledForLine && isChildLine;
			LineGroupingSequenceGroupBox.Visible = isLineGroupingEnabledForLine && !isChildLine;
			LineGroupingSequencesNotAvailableForInvoiceLabel.Visible = !isLineGroupingEnabledForLine;
		}

		void ShowOrHideElectronicInvoiceRelatedControls()
		{
			if (ElectronicInvoiceTabPage != null)
			{
				bool isLineGroupingEnabled = false;
				bool isCurrentInvoiceLineValidForAII = false;
				bool doesAIINeedRegeneration = false;
				bool hasNoOfSequences = false;
				if (currentInvoiceLine != null)
				{
					isLineGroupingEnabled = currentInvoiceLine.IsLineGroupingEnabled;
					isCurrentInvoiceLineValidForAII = currentInvoiceLine.IsValidForAII;
					if (isLineGroupingEnabled)
					{
						int totalNoOfSequences = currentInvoiceLine.TotalNoOfSequences;

						hasNoOfSequences = totalNoOfSequences > 0;
						if (hasNoOfSequences)
						{
							doesAIINeedRegeneration = currentInvoiceLine.DoesAIIRequireRegeneration;
						}
					}
				}

				bool isElectronicInvoiceEnabled = (CurrentDataItem != null && CurrentDataItem.ShouldElectronicInvoicesBeVisible);

				if (isElectronicInvoiceEnabled)
				{
					int expectedIndex = LineDetailTabControl.TabPages.IndexOf(LicencePermitsDetailsTabPage) + 2;
					int actualIndex = LineDetailTabControl.TabPages.IndexOf(ElectronicInvoiceTabPage);
					if (actualIndex != expectedIndex && expectedIndex >= 0)
					{
						ElectronicInvoiceTabPage.BindingContext = ElectronicInvoiceTabPage.BindingContext; // no context switching when remove
						LineDetailTabControl.TabPages.Remove(ElectronicInvoiceTabPage);
						LineDetailTabControl.TabPages.Insert(ElectronicInvoiceTabPage, expectedIndex);
						ElectronicInvoiceTabPage.BindingContext = null; // take context from parent
					}
				}

				ElectronicInvoiceTabPage.TabVisible = isElectronicInvoiceEnabled;
				NotValidForElectronicInvoiceLabel.Visible = !isCurrentInvoiceLineValidForAII;
				NonLineGroupingElectronicInvoicePanel.Visible = !isLineGroupingEnabled && isCurrentInvoiceLineValidForAII;
				LineGroupingAIIPanel.Visible = isLineGroupingEnabled && isCurrentInvoiceLineValidForAII && hasNoOfSequences && !doesAIINeedRegeneration;
				ElectronicInvoiceNeedsRegenerationLabel.Visible = isLineGroupingEnabled && isCurrentInvoiceLineValidForAII && hasNoOfSequences && doesAIINeedRegeneration;
				LineGroupingSequencesMustBeSpecifiedLabel.Visible = isLineGroupingEnabled && isCurrentInvoiceLineValidForAII && !hasNoOfSequences;
			}
		}

		void ShowOrHideFTZRelatedControls()
		{
			if (CurrentDataItem != null)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				var isVisibleForFTZ = declaration != null ? declaration.IsFTZAdmission : ZBool.False;
				var isVisibleForConsumptionFTZ = declaration != null && declaration.IsConsumptionFTZ;
				var isACECargoCertificationMode = declaration != null && declaration.IsACECargoCertificationMode;

				if (FTZDependentPanel != null)
				{
					FTZDependentPanel.Visible = CurrentDataItem.IsConsumptionFTZ || isVisibleForFTZ;
				}

				PGATabPage.TabVisible = !isVisibleForFTZ && !declaration.IsACE;

				FeesTabPage.TabVisible = !isVisibleForFTZ;
				LicencePermitsDetailsTabPage.TabVisible = !isVisibleForFTZ;
				OGATabPage.TabVisible = !isVisibleForFTZ && declaration != null && !declaration.IsACE;
				OtherTabPage.TabVisible = !isVisibleForFTZ;

				FTZCategoryNoTextBox.Visible = isVisibleForFTZ;
				OverrideDutyCheckBox.Visible = !isVisibleForFTZ;
				JI_Calc_DutyConvertToLocalCurrencyControl.Visible = !isVisibleForFTZ;
				OverrideSupDutyCheckBox.Visible = !isVisibleForFTZ;
				SupDutyCalcFindBox.Visible = !isVisibleForFTZ;
				TaxApplyDropEdit.Visible = !isVisibleForFTZ;
				TaxCodeDropEdit.Visible = !isVisibleForFTZ;
				TaxRateTypeDropEdit.Visible = !isVisibleForFTZ;
				TaxRateDropEdit.Visible = !isVisibleForFTZ;
				TaxRateCalcEdit.Visible = !isVisibleForFTZ;
				FTZFDAIndicatorDropEdit.Visible = isVisibleForFTZ;
				FTZRequiredTextBox.Visible = isVisibleForFTZ;
				FTZRequiredTextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Required";

				FDAIndicatorDropEdit.Visible = !isVisibleForFTZ;
				FDARequirementTextBox.Visible = !isVisibleForFTZ;
				FDAGroupBox.Visible = !isVisibleForFTZ;

				FTZHMFCalcFindBox.Visible = isVisibleForFTZ;

				if (isVisibleForConsumptionFTZ || isVisibleForFTZ)
				{
					CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.US_ZoneStatus);
				}
				else
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.US_ZoneStatus);
				}

				if (isVisibleForFTZ)
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.US_CottonFeeExempt,
										JobComInvoiceLine.Schema.US_HazMatClassDesc,
										JobComInvoiceLine.Schema.US_HazMatDesc,
										JobComInvoiceLine.Schema.JI_HazMatCode,
										JobComInvoiceLine.Schema.JI_HazMatCodeQualifier,
										JobComInvoiceLine.Schema.US_DOTIndicator,
										JobComInvoiceLine.Schema.US_FCCIndicator,
										JobComInvoiceLine.Schema.US_IsExcludedFromAII,
										JobComInvoiceLine.Schema.US_LumberExportCharges,
										JobComInvoiceLine.Schema.US_LumberExportPrice,
										JobComInvoiceLine.Schema.US_LumberImporterDeclaration,
										JobComInvoiceLine.Schema.US_TSCAName,
										JobComInvoiceLine.Schema.US_TSCAIndicator,
										JobComInvoiceLine.Schema.JI_Calc_DutyAmount,
										JobComInvoiceLine.Schema.US_ArticleNoA,
										JobComInvoiceLine.Schema.US_ArticleNoB,
										JobComInvoiceLine.Schema.US_SelectedRateType,
										JobComInvoiceLine.Schema.US_SupDuty,
										JobComInvoiceLine.Schema.US_OverrideSupDuty);
					uNDGDataManager.RemoveAllColumns(CustomsInvoiceLinesBoundGrid);
					BindingSource.SetBindingMember(TransactionsRelatedDropEdit, "FilteredInvoiceLines.US_F_PNDisclaimer");
					TransactionsRelatedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|03133E53-01DD-42DA-88A9-BAECE8D57D10", "PN Disclaimer");
				}
				else
				{
					CustomsInvoiceLinesBoundGrid.AddToAvailableColumns(JobComInvoiceLine.Schema.US_CottonFeeExempt,
										JobComInvoiceLine.Schema.US_HazMatClassDesc,
										JobComInvoiceLine.Schema.US_HazMatDesc,
										JobComInvoiceLine.Schema.JI_HazMatCode,
										JobComInvoiceLine.Schema.JI_HazMatCodeQualifier,
										JobComInvoiceLine.Schema.US_DOTIndicator,
										JobComInvoiceLine.Schema.US_FCCIndicator,
										JobComInvoiceLine.Schema.US_IsExcludedFromAII,
										JobComInvoiceLine.Schema.US_LumberExportCharges,
										JobComInvoiceLine.Schema.US_LumberExportPrice,
										JobComInvoiceLine.Schema.US_LumberImporterDeclaration,
										JobComInvoiceLine.Schema.US_TSCAName,
										JobComInvoiceLine.Schema.US_TSCAIndicator,
										JobComInvoiceLine.Schema.JI_Calc_DutyAmount,
										JobComInvoiceLine.Schema.US_ArticleNoA,
										JobComInvoiceLine.Schema.US_ArticleNoB,
										JobComInvoiceLine.Schema.US_SelectedRateType,
										JobComInvoiceLine.Schema.US_SupDuty,
										JobComInvoiceLine.Schema.US_OverrideSupDuty);
					uNDGDataManager.AddColumns();
					BindingSource.SetBindingMember(TransactionsRelatedDropEdit, "FilteredInvoiceLines.US_TransactionsRelated");
					TransactionsRelatedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("USImportInvoiceLineUserControl|25cf3be0-d58c-44c7-94b8-66a2138e88c7", "Trans Related", "Transaction Related", "");
				}

				ShowOrHideLicencePermitsPage(isVisibleForFTZ);
				ShowOrHideOtherConsumptionFTZControlsCore(isVisibleForConsumptionFTZ, isACECargoCertificationMode);
			}
		}

		protected virtual void ShowOrHideOtherConsumptionFTZControlsCore(ZBool isVisibleForConsumptionFTZ, ZBool isACECargoCertificationMode)
		{
		}

		void ShowOrHideFSISTabPage()
		{
			if (CurrentDataItem != null)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				FSISTabPage.TabVisible = declaration.IsPersistent && !declaration.IsFTZAdmission && !declaration.IsACE;
			}
		}

		protected virtual void ShowOrHideLicencePermitsPage(ZBool isVisible)
		{
			LicencePermitsDetailsTabPage.TabVisible = !isVisible;
		}

		#endregion

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib1Column()
		{
			ZDropEditColumnStyleInfo partAttrib1DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib1DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib1";
			partAttrib1DropEditColumnStyleInfo.ToolTip = "Invoice line part attribute 1";
			partAttrib1DropEditColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			return partAttrib1DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib2Column()
		{
			ZDropEditColumnStyleInfo partAttrib2DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib2DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib2";
			partAttrib2DropEditColumnStyleInfo.ToolTip = "Invoice line part attribute 2";
			return partAttrib2DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreatePartAttrib3Column()
		{
			ZDropEditColumnStyleInfo partAttrib3DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib3DropEditColumnStyleInfo.ColumnName = "JI_PartAttrib3";
			partAttrib3DropEditColumnStyleInfo.ToolTip = "Invoice line part attribute 3";
			return partAttrib3DropEditColumnStyleInfo;
		}

		protected override Core.Forms.ZGridColumnInfo CreateSerialNumberColumn()
		{
			var serialNumberDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			serialNumberDropEditColumnStyleInfo.ColumnName = "JI_SerialNumber";
			serialNumberDropEditColumnStyleInfo.ToolTip = "Invoice line serial number";
			return serialNumberDropEditColumnStyleInfo;
		}

		void CusContainerInvoiceLineGrid_VisibleChanged(object sender, EventArgs e)
		{
			if (needRefreshContainerPivot && CusContainerInvoiceLineGrid.Visible)
			{
				needRefreshContainerPivot = false;
				if (CusContainerInvoiceLineGrid.ListManager != null)
				{
					var collection = CusContainerInvoiceLineGrid.ListManager.List as NonPersistentCusContainerCollection;
					if (collection != null)
					{
						collection.RebuildElements();
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (CurrentDataItem != null)
				{
					var topGroupInvoice = CurrentDataItem.TopGroupInvoice;
					if (topGroupInvoice != null)
					{
						var declaration = topGroupInvoice.JobDeclaration;
						if (declaration != null && declaration.IsPersistent)
						{
							declaration.CusContainers.CountChanged -= CusContainers_CountChanged;
						}
					}
				}
				CusContainerInvoiceLineGrid.VisibleChanged -= CusContainerInvoiceLineGrid_VisibleChanged;

				UnHookLineGroupingRangeEvents(currentLineGroupingRange);

				if (manufacturerFindBox != null)
				{
					manufacturerFindBox.ReadOnlyChanged -= ManufacturerFindBox_ReadOnlyChanged;
				}
			}
			base.Dispose(disposing);
		}

		internal void FDAViewEditButton_Click(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				var linesGrid = FDALinesGrid;

				if (linesGrid != null)
				{
					FDA fda = null;

					if (linesGrid.ListManager != null && linesGrid.CurrentRowIndex >= 0)
					{
						fda = (FDA)linesGrid.ListManager.GetCurrent();
					}

					if (fda == null)
					{
						Globals.Message.ShowInformation("Please select (highlight) a FDA line.", "Edit FDA Line");
					}
					else
					{
						ZFormModaliser.ShowDialogAndDispose(new FDAForm(fda));
						linesGrid.ListManager.EndCurrentEdit();
					}
				}
			}
		}

		void DOTViewEditButton_Click(object sender, EventArgs e)
		{
			DOT dot = null;

			if (DOTLinesGrid.ListManager != null && DOTLinesGrid.CurrentRowIndex >= 0)
			{
				dot = (DOT)DOTLinesGrid.ListManager.GetCurrent();
			}

			if (dot == null)
			{
				Globals.Message.ShowInformation("Please select (highlight) a DOT line.", "Edit DOT Line");
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new DOTForm(dot));
				DOTLinesGrid.ListManager.EndCurrentEdit();
			}
		}

		protected override void HookInvoiceLineEvents(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.HookInvoiceLineEvents(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_JZInfo.ValueChanged += AIIRelated_ValueChanged;
				invoiceLine.JI_TariffInfo.ValueChanged += JI_TariffInfo_ValueChanged;
				invoiceLine.JI_ParentIDInfo.ValueChanged += AIIRelated_ValueChanged;
				invoiceLine.US_SecondarySPIInfo.ValueChanged += AIIRelated_ValueChanged;
				invoiceLine.AIILines.CountChanged += AIILines_CountChanged;
				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.JI_TariffInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.JI_ParentIDInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKPrimCtryInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKSecCtryInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKCastCtryInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKCertOriginInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKMeltCtryInfo.ValueChanged += new EventHandler(SupTariffTrigger_ValueChanged);
			}
			AIIRelated_ValueChanged(null, null);
			ChangeSupTariffComponentType();
		}

		protected override void UnHookInvoiceLineEvents(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.UnHookInvoiceLineEvents(baseInvoiceLine);

			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				invoiceLine.JI_JZInfo.ValueChanged -= AIIRelated_ValueChanged;
				invoiceLine.JI_TariffInfo.ValueChanged -= JI_TariffInfo_ValueChanged;
				invoiceLine.JI_ParentIDInfo.ValueChanged -= AIIRelated_ValueChanged;
				invoiceLine.US_SecondarySPIInfo.ValueChanged -= AIIRelated_ValueChanged;
				invoiceLine.AIILines.CountChanged -= AIILines_CountChanged;

				invoiceLine.US_UC_NKCountryOfOriginInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.JI_TariffInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.JI_ParentIDInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKPrimCtryInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKSecCtryInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKCastCtryInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKCertOriginInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
				invoiceLine.US_RN_NKMeltCtryInfo.ValueChanged -= new EventHandler(SupTariffTrigger_ValueChanged);
			}
		}

		void AIILines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			AIIRelated_ValueChanged(sender, e);
		}

		void AIIRelated_ValueChanged(object sender, EventArgs e)
		{
			UpdateAIIFieldsVisibility();
		}

		void UpdateAIIFieldsVisibility()
		{
			ShowOrHideLineGroupingDetails();
			ShowOrHideElectronicInvoiceRelatedControls();
		}

		void JI_TariffInfo_ValueChanged(object sender, EventArgs e)
		{
			OnCurrentInvoiceLineTariffChanged();
		}

		protected virtual void OnCurrentInvoiceLineTariffChanged()
		{
			UpdateAIIFieldsVisibility();
		}

		void LineGroupingSequenceGrid_AfterBind(object sender, EventArgs e)
		{
			LineGroupingSequenceGrid.ListManager.PositionChanged += (LineGroupingSequenceGridListManager_PositionChanged);
			LineGroupingSequenceGridListManager_PositionChanged(null, null);
		}

		void LineGroupingSequenceGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = LineGroupingSequenceGrid.ListManager;
			var current = listManager != null ? (InvoiceLineGroupingRange)listManager.GetCurrent() : null;
			if ((current != null && current.IsDeleted) || (CurrentInvoiceLine != null && CurrentInvoiceLine.IsDeleted))
			{
				current = null;
			}
			if (currentLineGroupingRange != current)
			{
				UnHookLineGroupingRangeEvents(currentLineGroupingRange);
				currentLineGroupingRange = current;
				HookLineGroupingRangeEvents(currentLineGroupingRange);
			}
		}
		InvoiceLineGroupingRange currentLineGroupingRange;

		void HookLineGroupingRangeEvents(InvoiceLineGroupingRange range)
		{
			if (range != null)
			{
				UnHookLineGroupingRangeEvents(range);
				range.CY_ParentIDInfo.ValueChanged += new EventHandler(AIIRelated_ValueChanged);
				range.US_StartSequenceNoInfo.ValueChanged += new EventHandler(AIIRelated_ValueChanged);
				range.US_EndSequenceNoInfo.ValueChanged += new EventHandler(AIIRelated_ValueChanged);
			}
			AIIRelated_ValueChanged(null, null);
		}

		void UnHookLineGroupingRangeEvents(InvoiceLineGroupingRange range)
		{
			if (range != null)
			{
				range.CY_ParentIDInfo.ValueChanged -= new EventHandler(AIIRelated_ValueChanged);
				range.US_StartSequenceNoInfo.ValueChanged -= new EventHandler(AIIRelated_ValueChanged);
				range.US_EndSequenceNoInfo.ValueChanged -= new EventHandler(AIIRelated_ValueChanged);
			}
		}

		void SupTariffTrigger_ValueChanged(object sender, EventArgs e)
		{
			ChangeSupTariffComponentType();
		}

		void ChangeSupTariffComponentType()
		{
			if (currentInvoiceLine != null && !currentInvoiceLine.IsDeleted)
			{
				OnSupTariffFormattedFieldTypeChanged(currentInvoiceLine.SupTariffFormattedFieldType == nameof(FieldType.TextDropEdit));
			}
		}

		protected virtual void OnSupTariffFormattedFieldTypeChanged(bool shouldUseDropEdit)
		{
			SupTariffDropEdit.Visible = shouldUseDropEdit;
			SupTariffFindBox.Visible = !shouldUseDropEdit;
		}
	}
}
