using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class BaseInvoiceLineUserControl : ZUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(PendingApportionmentLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			AddPartAttribColumns();
			AddNewOwnerPartColumns();
			AddInvoiceLineChargesUserControl();
			AddCustomFieldsUserControl();

			if (!DesignModeFinder.IsDesigning
				&& SupportsInwardProcessing.IsSupported()
				&& CustomsDataRegistry.Instance.EnableByProductFunctionality.Value)
			{
				AddInvoiceLineComponentsUserControl();
			}

			if (!DesignModeFinder.IsDesigning)
			{
				JI_Calc_GSTConvertToLocalCurrencyControl.CaptionResourceString = GetJI_Calc_GSTConvertToLocalCurrencyControlCaption();
				AddDynamicLayoutUserControl();
			}

			CustomsInvoiceLinesBoundGrid.SetColumnModuleID(BaseJobComInvoiceLine.Schema.JI_CC, ClassificationModuleID);
			CustomsInvoiceLinesBoundGrid.RowsDeleting += new EventHandler<RowsDeletingEventArgs>(this.CustomsInvoiceLinesBoundGrid_RowsDeleting);
			CustomsInvoiceLinesBoundGrid.RowsDeleted += new EventHandler<RowsDeletingEventArgs>(this.CustomsInvoiceLinesBoundGrid_RowsDeleted);

			MenuItem selectContainerForAllInvoiceLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("cfcfe033-18f2-4998-b5c0-5edcc40640d9", "Attach this Container to All Invoice Lines"), new EventHandler(SelectContainerForAllInvoiceLines));
			CusContainerInvoiceLineGrid.ContextMenu.MenuItems.Add(selectContainerForAllInvoiceLinesMenuItem);

			MenuItem unselectContainerForAllInvoiceLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("d24baea6-727a-45de-8473-ab166d98f3ce", "Detach this container from All Invoice Lines"), new EventHandler(UnselectContainerForAllInvoiceLines));
			CusContainerInvoiceLineGrid.ContextMenu.MenuItems.Add(unselectContainerForAllInvoiceLinesMenuItem);
			AddUniversalTariffDetails();

			var showHideFilterMenuItem = new ZMenuItem(ResString.GetMultilingualString("FE7CBA8A-8DDA-4346-B44F-59AF5E28A9A0", "Show/Hide Filters"), ShowFilterMenuItem_Click);
			CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(4, showHideFilterMenuItem);

			StripControl.Visible = false;
			StripControl.CaptionRenderingEnabled = true;
			StripControl.PerformSearch += StripControl_PerformSearch;

			if (!SupportsExtraPhysicalQuantitiesOnC2Pivot)
			{
				c2ContainerSplitCurrencyColumnStyleInfo2.IsVisible = false;
				c2PivotContainerGrossWeightColumnStyleInfo10.IsVisible = false;
				c2PivotContainerNetWeightColumnStyleInfo11.IsVisible = false;
				c2PivotContainerPackQtyColumnStyleInfo12.IsVisible = false;
				c2PivotContainerSplitValueColumnStyleInfo13.IsVisible = false;
				c2PivotContainerWeightColumnStyleInfo9.IsVisible = false;
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2ContainerSplitCurrencyColumnStyleInfo2);
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2PivotContainerGrossWeightColumnStyleInfo10);
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2PivotContainerNetWeightColumnStyleInfo11);
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2PivotContainerPackQtyColumnStyleInfo12);
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2PivotContainerSplitValueColumnStyleInfo13);
				CusContainerInvoiceLineGrid.ColumnStyles.Remove(c2PivotContainerWeightColumnStyleInfo9);
			}

			CustomsInvoiceLinesBoundGrid.GridColourSchemeManagerDeciding += CustomsInvoiceLinesBoundGrid_GridColourSchemeManagerDeciding;

			CustomsInvoiceLinesBoundGrid.AfterBind -= CustomsInvoiceLinesBoundGrid_AfterBind;
			CustomsInvoiceLinesBoundGrid.AfterBind += CustomsInvoiceLinesBoundGrid_AfterBind;

			PendingApportionmentLabel.AllowOverlap(LineSummaryPanel);
			PendingApportionmentLabel.AllowOverlap(CurrentInvoicePanel);
			ClassificationPanel.AllowOverlap(InvoiceDetailsGroupBox);

			PendingApportionmentLabel.AllowOutsideOfParent();
		}

		#region AddCpwUserControlAndRegisterInteractEventForDeclarationIfNeeded

		void InitializeLinkedModuleCommodityIfNeeded(BaseJobComInvoiceLine invoiceLine)
		{
			if (supportInteractionWithComplianceWiseCommodities)
			{
				invoiceLine.LinkedModuleCommodity.InitializeIfNeeded();
			}
		}

		ZTabPage cpwInvocingLineTabPage;
		ILinkedModuleCommodityUserControl linkedModuleCommodityUserControl;

		bool supportInteractionWithComplianceWiseCommodities;

		bool IsAddCpwUserControlAndRegisterInteractEventForDeclaration =>
			CurrentDataItem is BaseJobDeclaration declaration
			&& declaration.IsStandAlone
			&& declaration is IComplianceItemRiskStatusProvider itemRiskStatusProvider
			&& itemRiskStatusProvider.IsEnabledComplianceWise
			&& declaration is ISupportInteractionWithComplianceWiseCommodities supportInteractionWithCommodities
			&& supportInteractionWithCommodities.Helper != null
			&& supportInteractionWithCommodities.Enabled;

		void AddCpwUserControlAndRegisterInteractEventForDeclarationIfNeeded()
		{
			var mainForm = FindForm() as ZForm;
			if (mainForm != null && IsAddCpwUserControlAndRegisterInteractEventForDeclaration
				&& CurrentDataItem is BaseJobDeclaration declaration
				&& declaration is ISupportInteractionWithComplianceWiseCommodities supportInteractionWithCommodities)
			{
				supportInteractionWithComplianceWiseCommodities = true;

				#region Add Cpw User Control

				cpwInvocingLineTabPage = new ZTabPage();
				cpwInvocingLineTabPage.CaptionResourceString = Res.GetData("90206478-C17F-4D86-AB6C-04FA3A5FAB67", "Compliance Assessment");
				cpwInvocingLineTabPage.Name = "CpwInvocingLineTabPage";
				cpwInvocingLineTabPage.Size = ControlDpiScalingHelper.NewScaledSize(704, 293);

				linkedModuleCommodityUserControl = ObjectFactory.Get<ILinkedModuleCommodityUserControl>();

				var userControl = linkedModuleCommodityUserControl as ZUserControl;
				userControl.Dock = DockStyle.Fill;
				userControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				userControl.Name = "LinkedModuleCommodityUserControl";
				userControl.TabIndex = 0;
				BindingSource.SetBindingMember(userControl, "FilteredInvoiceLines.LinkedModuleCommodity");
				cpwInvocingLineTabPage.Controls.Add(userControl);
				LineDetailTabControl.Controls.Add(cpwInvocingLineTabPage);

				#endregion

				var commodities = supportInteractionWithCommodities.Helper.SourceSideCommodities.GetCommoditiesStatusFromCpw?.Invoke();
				if (commodities?.Length > 0)
				{
					var currentLineDictionary = declaration.FilteredInvoiceLines.GroupBy(GetKeyFromLine).ToDictionary(u => u.Key);
					var commodityDictionary = commodities.GroupBy(GetKeyFromCommodity).ToDictionary(u => u.Key);

					foreach (var currentLineGrouped in currentLineDictionary)
					{
						if (commodityDictionary.TryGetValue(currentLineGrouped.Key, out var groupedCommodities))
						{
							currentLineGrouped.Value.ForEach(line =>
							{
								var firstCommodity = groupedCommodities.First();
								line.LinkedModuleCommodity.BatchInitialize(firstCommodity);
							});
						}
						else
						{
							currentLineGrouped.Value.ForEach(line =>
							{
								line.LinkedModuleCommodity.BatchInitialize(null);
							});
						}
					}
				}

				supportInteractionWithCommodities.Helper.CpwSideCommodities.CommoditiesChanged = (commodities) =>
				{
					var linesDictionary = declaration.FilteredInvoiceLines.GroupBy(GetKeyFromLine).ToDictionary(u => u.Key);

					foreach (var commodity in commodities)
					{
						if (linesDictionary.TryGetValue(GetKeyFromCommodity(commodity), out var groupedLines))
						{
							groupedLines.ForEach(line =>
							{
								line.LinkedModuleCommodity.SetCommodityInfo(commodity);
							});
						}
					}
				};
			}
		}

		string GetKeyFromLine(BaseJobComInvoiceLine line)
		{
			var description = line.JI_Description.IsEmpty ? line.JI_NDescription : line.JI_Description;
			if (description.Length > ComplianceCommodityDetailSchema.CCD_Description.MaxLength)
			{
				description = description.Substring(0, ComplianceCommodityDetailSchema.CCD_Description.MaxLength);
			}

			var hsCode = ComplianceRiskHelper.ExtractAllAlphanumericFromHsCode(line.JI_TariffForComplianceWise.ToUpperInvariant());
			var groupingOrCountry = Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
			return ComplianceRiskHelper.GetUnionKeyFromCommodity(hsCode, groupingOrCountry, line.JI_CountryOfOrigin, description);
		}

		string GetKeyFromCommodity(ComplianceResultFromCpw commodity)
		{
			return ComplianceRiskHelper.GetUnionKeyFromCommodity(commodity.HarmonizedCode, commodity.GroupingOrCountry,
				commodity.OriginOfGoods, commodity.GoodsDescription);
		}

		#endregion

		protected virtual ResourceStringData GetJI_Calc_GSTConvertToLocalCurrencyControlCaption()
		{
			return JI_Calc_GSTConvertToLocalCurrencyControl.CaptionResourceString.Format(BaseJobComInvoiceLine.ConsumptionTaxDescription);
		}

		void CustomsInvoiceLinesBoundGrid_GridColourSchemeManagerDeciding(object sender, GridColourSchemeManagerDecidingEventArgs e)
		{
			e.GridColourSchemeManager = new CustomsInvoiceLinesGridColorSchemaManager(CustomsInvoiceLinesBoundGrid);
		}

		void StripControl_PerformSearch(object sender, EventArgs e)
		{
			FilterBusinessObject.Search();
		}

		InvoiceLineFilterBusinessObject filterBusinessObject;

#if DEBUG
		public
#endif
		InvoiceLineFilterBusinessObject FilterBusinessObject
		{
			get
			{
				return filterBusinessObject ?? (filterBusinessObject = CreateFilterBusinessObject(() => CurrentDataItem,
					(columnName) => CustomsInvoiceLinesBoundGrid.Columns.Any(x => x.ColumnName == columnName && !x.IsUnavailable)));
			}
		}

		protected virtual InvoiceLineFilterBusinessObject CreateFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return DesignModeFinder.IsDesigning ? null : new InvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		void ShowFilterMenuItem_Click(object sender, EventArgs e)
		{
			StripControl.Visible = !StripControl.Visible;
			if (!StripControl.Visible)
			{
				StripControl.ResetFilterStrips();
				CurrentDataItem?.FilteredInvoiceLines.LoadFilteredLines(x => true);
			}
			else
			{
				StripControl.FilterStripLoaded();
			}
		}

		void AddUniversalTariffDetails()
		{
			if (UseUniversalTariff)
			{
				AddUniversalTariffColumnToGrid();
			}
		}

		public void AddUniversalTariffColumnToGrid()
		{
			var tariffColumnStyleInfo = CreateUniversalTariffColumnStyleInfo(TariffColumnName);
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tariffColumnStyleInfo.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;

			var ccInfo = CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_CC);
			if (ccInfo != null)
			{
				var index = CustomsInvoiceLinesBoundGrid.ColumnStyles.IndexOf(ccInfo);
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(index + 1, tariffColumnStyleInfo);
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyleInfo);
			}
		}

		protected virtual Universal.GUI.TariffColumnStyleInfo CreateUniversalTariffColumnStyleInfo(string columnName)
		{
			return new Universal.GUI.TariffColumnStyleInfo()
			{
				GetCountryCode = GetCustomsCountryCode,
				GetDataGrouping = GetDataGroupingForUniversalTariff,
				GetTariffType = GetUniversalTariffType,
				ColumnName = columnName
			};
		}

		protected virtual bool UseUniversalTariff => true;

		public ZString TariffColumnName => TariffColumnNameCore;

		protected virtual ZString TariffColumnNameCore => BaseJobComInvoiceLine.Schema.JI_Tariff;

		protected virtual ZDateTime GetEffectiveAssessmentDateForUniversalTariff() => CurrentInvoiceLine?.EffectiveAssessmentDate ?? ZDateTime.Today;

		protected Func<ZString> GetUniversalTariffType => () => UniversalTariffType;

		protected virtual ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected virtual ZString GetDataGroupingForUniversalTariff() => CurrentInvoiceLine?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
		protected virtual string GetCustomsCountryCode()
		{
			string result = null;
			if (!this.IsDesignMode())
			{
				var invoiceLine = CurrentInvoiceLine;
				result = invoiceLine == null || invoiceLine.IsDeleted ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : invoiceLine.CustomsCountryCode;
			}
			return result;
		}

		protected override void InitLayout()
		{
			base.InitLayout();
			ReorderCustomFieldsTab();
		}

		protected void AddPartAttribColumns()
		{
			Core.Forms.ZGridColumnInfo partAttrib1TextBoxColumnStyleInfo = CreatePartAttrib1Column();
			CargoWise.Common.Argument.NotNull(partAttrib1TextBoxColumnStyleInfo, "partAttrib1TextBoxColumnStyleInfo");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib1TextBoxColumnStyleInfo);

			Core.Forms.ZGridColumnInfo partAttrib2TextBoxColumnStyleInfo = CreatePartAttrib2Column();
			CargoWise.Common.Argument.NotNull(partAttrib2TextBoxColumnStyleInfo, "partAttrib2TextBoxColumnStyleInfo");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib2TextBoxColumnStyleInfo);

			Core.Forms.ZGridColumnInfo partAttrib3TextBoxColumnStyleInfo = CreatePartAttrib3Column();
			CargoWise.Common.Argument.NotNull(partAttrib3TextBoxColumnStyleInfo, "partAttrib3TextBoxColumnStyleInfo");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib3TextBoxColumnStyleInfo);

			Core.Forms.ZGridColumnInfo serialNumberTextBoxColumnStyleInfo = CreateSerialNumberColumn();
			CargoWise.Common.Argument.NotNull(serialNumberTextBoxColumnStyleInfo, "serialNumberTextBoxColumnStyleInfo");
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(serialNumberTextBoxColumnStyleInfo);
		}

		protected virtual Core.Forms.ZGridColumnInfo CreatePartAttrib1Column()
		{
			ZTextBoxColumnStyleInfo partAttrib1TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib1TextBoxColumnStyleInfo.ColumnName = "JI_PartAttrib1";
			partAttrib1TextBoxColumnStyleInfo.ToolTip = Res.GetString("efc96e37-3b49-4da4-a8ba-ea705099bd80", "Invoice line part attribute 1");
			partAttrib1TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			return partAttrib1TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreatePartAttrib2Column()
		{
			ZTextBoxColumnStyleInfo partAttrib2TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib2TextBoxColumnStyleInfo.ColumnName = "JI_PartAttrib2";
			partAttrib2TextBoxColumnStyleInfo.ToolTip = Res.GetString("987b93e5-53f2-4862-95e1-eba4a9b860ff", "Invoice line part attribute 2");
			partAttrib2TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			return partAttrib2TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreatePartAttrib3Column()
		{
			ZTextBoxColumnStyleInfo partAttrib3TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib3TextBoxColumnStyleInfo.ColumnName = "JI_PartAttrib3";
			partAttrib3TextBoxColumnStyleInfo.ToolTip = Res.GetString("1af8a1a2-6a5d-4aec-975c-8208381db880", "Invoice line part attribute 3");
			partAttrib3TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			return partAttrib3TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateSerialNumberColumn()
		{
			ZTextBoxColumnStyleInfo serialNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			serialNumberTextBoxColumnStyleInfo.ColumnName = "JI_SerialNumber";
			serialNumberTextBoxColumnStyleInfo.ToolTip = Res.GetString("126adc8b-4714-4c26-9cfd-b22a6921a659", "Invoice line serial number");
			serialNumberTextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			return serialNumberTextBoxColumnStyleInfo;
		}

		void AddInvoiceLineChargesUserControl()
		{
			this.LineChargesTabPage = new ZTabPage();
			this.LineChargesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|31c15de1-5d2f-407e-8698-036c6800e5db", "Charges");
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23);
			this.LineChargesTabPage.Name = "LineChargesTabPage";
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293);
			this.LineChargesTabPage.TabIndex = 1;

			fInvoiceLineCharges = GetInvoiceLineChargesUserControl();
			fInvoiceLineCharges.Dock = System.Windows.Forms.DockStyle.Fill;
			fInvoiceLineCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			fInvoiceLineCharges.Name = "InvoiceLineCharges";
			fInvoiceLineCharges.TabIndex = 0;
			LineChargesTabPage.Controls.Add(fInvoiceLineCharges);

			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 289);
			this.LineDetailTabControl.Controls.Add(this.LineChargesTabPage);
		}

		protected virtual InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl()
		{
			return new InvoiceLineChargesUserControl();
		}

		void AddInvoiceLineComponentsUserControl()
		{
			LineComponentsTabPage = new ZTabPage();
			LineComponentsTabPage.CaptionResourceString = Res.GetData("BaseInvoiceLineUserControl|31097dc6-de8a-4e58-8198-aa61d64ef992", "Allocated Inventory");
			LineComponentsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23);
			LineComponentsTabPage.Name = "LineComponentsTabPage";
			LineComponentsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(704, 293);
			LineComponentsTabPage.TabIndex = 1;

			InvoiceLineComponentsControl = new InvoiceLineComponentsUserControl();
			InvoiceLineComponentsControl.Dock = DockStyle.Fill;
			InvoiceLineComponentsControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			InvoiceLineComponentsControl.Name = "InvoiceLineComponentsControl";
			InvoiceLineComponentsControl.TabIndex = 0;
			BindingSource.SetBindingMember(InvoiceLineComponentsControl, "FilteredInvoiceLines");
			LineComponentsTabPage.Controls.Add(InvoiceLineComponentsControl);

			LineDetailTabControl.Controls.Add(LineComponentsTabPage);
		}

		void AddCustomFieldsUserControl()
		{
			CustomFieldsTabPage = new ZTabPage();
			CustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseInvoiceLineUserControl|568b6735-ae6b-43a4-aee4-642b95b0aae8", "Custom Fields");
			CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23);
			CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293);
			CustomFieldsTabPage.TabIndex = 1;

			CustomFieldsControl = new InvoiceLineCustomFieldsControl();
			BindingSource.SetBindingMember(this.CustomFieldsControl, "FilteredInvoiceLines");
			CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			CustomFieldsControl.Name = "CustomFieldsControl";
			CustomFieldsControl.TabIndex = 0;
			CustomFieldsTabPage.Controls.Add(CustomFieldsControl);

			LineDetailTabControl.Controls.Add(CustomFieldsTabPage);
		}

		protected void ReorderCustomFieldsTab()
		{
			if (CustomFieldsTabPage != null)
			{
				LineDetailTabControl.Controls.Remove(CustomFieldsTabPage);
				LineDetailTabControl.Controls.Add(CustomFieldsTabPage);
			}
		}

		List<IDisposable> allSuspendersForInvoiceLineDeleting;
		List<BaseJobComInvoiceHeader> invoiceHeaders;
		void CustomsInvoiceLinesBoundGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			invoiceHeaders = new List<BaseJobComInvoiceHeader>();
			allSuspendersForInvoiceLineDeleting = new List<IDisposable>();
			foreach (BaseJobComInvoiceLine invoiceLine in e.Objects)
			{
				if (invoiceLine.InvoiceHeader is BaseJobComInvoiceHeader invoiceHeader && !invoiceHeaders.Contains(invoiceHeader))
				{
					invoiceHeaders.Add(invoiceHeader);
				}
			}
			if (CurrentDataItem != null)
			{
				var declaration = CurrentDataItem as ICommonInvoiceDataProvider;
				if (declaration != null)
				{
					allSuspendersForInvoiceLineDeleting.Add(declaration.SuspendWeightApportionment());
				}
				foreach (var invoiceHeader in invoiceHeaders)
				{
					allSuspendersForInvoiceLineDeleting.Add(invoiceHeader.GetLineNumberRenumberingSuspender());
				}
			}
		}

		void CustomsInvoiceLinesBoundGrid_RowsDeleted(object sender, RowsDeletingEventArgs e)
		{
			if (allSuspendersForInvoiceLineDeleting != null)
			{
				foreach (IDisposable suspender in allSuspendersForInvoiceLineDeleting)
				{
					if (suspender != null)
					{
						suspender.Dispose();
					}
				}
			}

			if (CurrentDataItem != null)
			{
				if (invoiceHeaders != null)
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in invoiceHeaders)
					{
						invoiceHeader.InvoiceLineLineNumberGenerator.ReCalculateAll();
						invoiceHeader.ReApportionLineWeightIfNeeded(true);
					}
				}
			}
			allSuspendersForInvoiceLineDeleting = null;
			invoiceHeaders = null;
		}

		#region ApportionWeight

		MenuItem apportionWeightSeparatorMenuItem;
		MenuItem apportionWeightMenuItem;

		void AddApportionWeightMenuToGrid()
		{
			apportionWeightSeparatorMenuItem = new ZMenuItem("-") { Name = nameof(apportionWeightSeparatorMenuItem) };
			apportionWeightMenuItem = new ZMenuItem(ResString.GetMultilingualString("925342E2-EA0F-4B3D-9BE2-5D791FDDF5B2", "Allocate Weight to Selected Invoice Lines"), AllocateWeightToSelectedInvoiceLines) { Name = nameof(apportionWeightMenuItem) };
			CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(apportionWeightSeparatorMenuItem);
			CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(apportionWeightMenuItem);
		}

		void AllocateWeightToSelectedInvoiceLines(object sender, EventArgs args)
		{
			var mainForm = FindForm() as ZForm;
			if (mainForm != null && CurrentDataItem is BaseJobDeclaration declaration)
			{
				if (declaration.JE_AutoWeightApportion)
				{
					if (Globals.Message.Show(
					Res.GetString("d212aa79-1346-4e9b-8f50-71c7a281dfcd", "'Auto Apportion Weight' will be turned off before allocating weight. Do you want to continue?"),
					Res.GetString("a821c592-9bc9-4043-8c11-b4c9e7b16099", "Warning - Auto Apportion Weight will be turned off"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						declaration.JE_AutoWeightApportion = false;
						AllocateWeight();
					}
				}
				else
				{
					AllocateWeight();
				}
			}
		}

		void AllocateWeight()
		{
			var baseJobComInvoiceLines = CustomsInvoiceLinesBoundGrid.GetSelectedRows().Cast<BaseJobComInvoiceLine>().OrderBy(c => c.JI_LineNo);
			if (baseJobComInvoiceLines.Any())
			{
				var allocateWeight = new AllocateWeight(CurrentDataItem.Factory, baseJobComInvoiceLines);
				using (var form = new AllocateWeightForm(allocateWeight))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						AllocateWeightHelper.AllocateNetWeight(allocateWeight, baseJobComInvoiceLines);
						AllocateWeightHelper.AllocateGrossWeight(allocateWeight, baseJobComInvoiceLines);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("204D8718-0CD7-4037-893E-46272FE5E16A", "You must select at least one Invoice Line."));
			}
		}

		#endregion

		#region Create Classification Assistant Request

		MenuItem createClassificationAssistantRequestMenuItem;

		void AddCreateClassificationAssistantRequestMenuToGrid()
		{
			var currentDataItem = CurrentDataItem;
			var declaration = currentDataItem as BaseJobDeclaration;
			if (DisplayCreateClassificationAssistantRequestMenu && (declaration?.EnableClassificationAssistant ?? false))
			{
				createClassificationAssistantRequestMenuItem = new ZMenuItem(ResString.GetMultilingualString("Enterprise.Customs.GUI.BaseInvoiceLineUserControl|CreateClassificationAssistantRequestMenu", "Create Classification Assistant Request"), CreateClassificationAssistantRequestMenuClick)
				{
					Name = nameof(createClassificationAssistantRequestMenuItem)
				};
				CustomsInvoiceLinesBoundGrid.ContextMenu.MenuItems.Add(createClassificationAssistantRequestMenuItem);
			}
		}

		void CreateClassificationAssistantRequestMenuClick(object sender, EventArgs args)
		{
			var baseJobComInvoiceLines = CustomsInvoiceLinesBoundGrid.GetSelectedRows().Cast<BaseJobComInvoiceLine>().ToArray();
			var declaration = BindingSource.DataSource as BaseJobDeclaration;
			var information = new ProductInformation(CurrentInvoiceLine.Factory, declaration, baseJobComInvoiceLines);
			if (!string.IsNullOrEmpty(information.ConstructionErrorMessage))
			{
				Globals.Message.ShowError(information.ConstructionErrorMessage);
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new SendProductInformationRequestForm(information));
			}
		}

		protected virtual bool DisplayCreateClassificationAssistantRequestMenu => false;

		#endregion

		#region Controls

		public InvoiceLineChargesUserControl InvoiceLineCharges
		{
			get { return fInvoiceLineCharges; }
		}
		InvoiceLineChargesUserControl fInvoiceLineCharges;

		#endregion

		#region New Owner Part
		void AddNewOwnerPartColumns()
		{
			if (SupportNewOwnerPartDetails)
			{
				Core.Forms.ZGridColumnInfo partPartNoColumnStyleInfo = CreateNewOwnerPartNoColumn();
				CargoWise.Common.Argument.NotNull(partPartNoColumnStyleInfo, "partPartNoColumnStyleInfo");
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partPartNoColumnStyleInfo);

				Core.Forms.ZGridColumnInfo partAttrib1TextBoxColumnStyleInfo = CreateNewOwnerPartAttrib1Column();
				CargoWise.Common.Argument.NotNull(partAttrib1TextBoxColumnStyleInfo, "partAttrib1TextBoxColumnStyleInfo");
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib1TextBoxColumnStyleInfo);

				Core.Forms.ZGridColumnInfo partAttrib2TextBoxColumnStyleInfo = CreateNewOwnerPartAttrib2Column();
				CargoWise.Common.Argument.NotNull(partAttrib2TextBoxColumnStyleInfo, "partAttrib2TextBoxColumnStyleInfo");
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib2TextBoxColumnStyleInfo);

				Core.Forms.ZGridColumnInfo partAttrib3TextBoxColumnStyleInfo = CreateNewOwnerPartAttrib3Column();
				CargoWise.Common.Argument.NotNull(partAttrib3TextBoxColumnStyleInfo, "partAttrib3TextBoxColumnStyleInfo");
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(partAttrib3TextBoxColumnStyleInfo);

				Core.Forms.ZGridColumnInfo serialNumberTextBoxColumnStyleInfo = CreateNewOwnerSerialNumberColumn();
				CargoWise.Common.Argument.NotNull(serialNumberTextBoxColumnStyleInfo, "serialNumberTextBoxColumnStyleInfo");
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(serialNumberTextBoxColumnStyleInfo);
			}
		}

		protected virtual bool SupportNewOwnerPartDetails => false;

		/// <summary>
		/// Should C2_PacKQty, C2_Weight etc be shown on the grid? No except for AU & US
		/// </summary>
		protected virtual bool SupportsExtraPhysicalQuantitiesOnC2Pivot
		{
			get { return false; }
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateNewOwnerPartNoColumn()
		{
			var partNoCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			partNoCodeFindBoxColumnStyleInfo.Caption = Res.GetString("{6DC1953E-3B0E-4178-B52D-0DEE60E43A8D}", "Owner Product");
			partNoCodeFindBoxColumnStyleInfo.ColumnName = NewOwnerPartNoColumnName;
			partNoCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			return partNoCodeFindBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateNewOwnerPartAttrib1Column()
		{
			var partAttrib1TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib1TextBoxColumnStyleInfo.ColumnName = NewOwnerPartAttrib1ColumnName;
			partAttrib1TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			partAttrib1TextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			return partAttrib1TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateNewOwnerPartAttrib2Column()
		{
			var partAttrib2TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib2TextBoxColumnStyleInfo.ColumnName = NewOwnerPartAttrib2ColumnName;
			partAttrib2TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			partAttrib2TextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			return partAttrib2TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateNewOwnerPartAttrib3Column()
		{
			var partAttrib3TextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			partAttrib3TextBoxColumnStyleInfo.ColumnName = NewOwnerPartAttrib3ColumnName;
			partAttrib3TextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			partAttrib3TextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			return partAttrib3TextBoxColumnStyleInfo;
		}

		protected virtual Core.Forms.ZGridColumnInfo CreateNewOwnerSerialNumberColumn()
		{
			var serialNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			serialNumberTextBoxColumnStyleInfo.ColumnName = NewOwnerSerialNumberColumnName;
			serialNumberTextBoxColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			serialNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			return serialNumberTextBoxColumnStyleInfo;
		}

		protected virtual string NewOwnerPartNoColumnName
		{
			get { return ""; }
		}

		protected virtual string NewOwnerPartAttrib1ColumnName
		{
			get { return ""; }
		}

		protected virtual string NewOwnerPartAttrib2ColumnName
		{
			get { return ""; }
		}

		protected virtual string NewOwnerPartAttrib3ColumnName
		{
			get { return ""; }
		}

		protected virtual string NewOwnerSerialNumberColumnName => "";

		void CurrentDataItem_OnNewOwnerPartAttributeCaptionDetailsChanged(object sender, EventArgs e)
		{
			SetNewOwnerPartVisibility();
		}

		protected void SetNewOwnerPartVisibility()
		{
			if (SupportNewOwnerPartDetails)
			{
				UpdateCustomsInvoiceLinesBoundGridColumnLayoutContext();
				var owner = CurrentDataItem?.NewOwner;
				var hasOwner = owner != null;
				CustomsInvoiceLinesBoundGrid.SetAvailability(hasOwner, new[] { NewOwnerPartNoColumnName, NewOwnerPartAttrib1ColumnName, NewOwnerPartAttrib2ColumnName, NewOwnerPartAttrib3ColumnName, NewOwnerSerialNumberColumnName });
				if (hasOwner)
				{
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib1ColumnName, GetOwnerPartAttributeCaption(owner.PartAttributeManager.PartAttributeName1));
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib2ColumnName, GetOwnerPartAttributeCaption(owner.PartAttributeManager.PartAttributeName2));
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib3ColumnName, GetOwnerPartAttributeCaption(owner.PartAttributeManager.PartAttributeName3));
				}
				else
				{
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib1ColumnName, GetOwnerPartAttributeCaption(ResString.GetMultilingualString("{3C8FA718-566A-4E45-A2CC-034A8FE0B4ED}", "Part Attrib. 1")));
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib2ColumnName, GetOwnerPartAttributeCaption(ResString.GetMultilingualString("{9FB9B2DE-D452-41F1-8D0B-BA83E25A1A0A}", "Part Attrib. 2")));
					CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerPartAttrib3ColumnName, GetOwnerPartAttributeCaption(ResString.GetMultilingualString("{E1E18665-A9B2-48BE-B355-8709DDCDE214}", "Part Attrib. 3")));
				}

				CustomsInvoiceLinesBoundGrid.SetColumnCaption(NewOwnerSerialNumberColumnName, GetOwnerPartAttributeCaption(ResString.GetMultilingualString("1a52708c-2e5e-4d64-b313-180d60ac4053", "Serial Number")));

				if (CustomsInvoiceLinesBoundGrid.DataSource != null)
				{
					CustomsInvoiceLinesBoundGrid.GridLayoutManager.LoadUserLayoutSettings(CustomsInvoiceLinesBoundGrid);
				}
			}
		}
		const string OwnerGridLayout = "|OWNER";
		ZString customsInvoiceLinesBoundGridColumnLayoutContext;

		void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateCustomsInvoiceLinesBoundGridColumnLayoutContext();
		}

		void UpdateCustomsInvoiceLinesBoundGridColumnLayoutContext()
		{
			var hasOwner = CurrentDataItem?.NewOwner != null;
			string columnLayoutContext = customsInvoiceLinesBoundGridColumnLayoutContext;
			var newColumnLayoutContext = columnLayoutContext.EndsWith(OwnerGridLayout, StringComparison.OrdinalIgnoreCase) ? (hasOwner ? columnLayoutContext : columnLayoutContext.Substring(0, columnLayoutContext.Length - 6)) : columnLayoutContext + (hasOwner ? OwnerGridLayout : "");
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = newColumnLayoutContext;
		}

		protected virtual ZString GetCustomsInvoiceLinesBoundGridColumnLayoutContext()
		{
			var result = CurrentDataItem?.MessageTypeInfo?.Value;
			return result == null ? ZString.Empty : (ZString)result;
		}

		string GetOwnerPartAttributeCaption(MultilingualString partAttributeName)
		{
			return Res.GetString("{49CD27A2-8A3E-42CC-BD67-77661B9771F6}", "Owner {0}", partAttributeName);
		}

		#endregion

		#region Control Visibility

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (CurrentDataItem != null)
			{
				if (fInvoiceLineGridLayoutPersister != null)
				{
					fInvoiceLineGridLayoutPersister.Dispose();
					fInvoiceLineGridLayoutPersister = null;
				}
				if (fWorkflowCustomFieldsGridLayoutPersister != null)
				{
					fWorkflowCustomFieldsGridLayoutPersister.Dispose();
					fWorkflowCustomFieldsGridLayoutPersister = null;
				}
				UnHookControlVisibilityChangeEventsByCurrentDataItem();
			}
			base.SetDataBinding(dataSource, dataMember);
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				currentDataItem.OnApportionmentDirtyChanged += new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(CurrentDataItem_OnApportionmentDirtyChanged);
				currentDataItem.OnContainerDetailsChanged += new EventHandler(CurrentDataItem_OnContainerDetailsChanged);
				currentDataItem.OnPartAttributeCaptionDetailsChanged += new EventHandler(CurrentDataItem_OnPartAttributeCaptionDetailsChanged);
				if (SupportNewOwnerPartDetails)
				{
					currentDataItem.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
					customsInvoiceLinesBoundGridColumnLayoutContext = GetCustomsInvoiceLinesBoundGridColumnLayoutContext();
					currentDataItem.OnNewOwnerPartAttributeCaptionDetailsChanged += CurrentDataItem_OnNewOwnerPartAttributeCaptionDetailsChanged;
				}

				currentDataItem.OnInvoiceLinesVisibilityChanged += new EventHandler(invoiceProvider_OnInvoiceLinesVisibilityChanged);
				InvoiceStructureChangeEvent.AddInvoiceStructureChangedEventHandler(currentDataItem.Factory, invoiceProvider_OnInvoiceLinesVisibilityChanged);

				var declaration = currentDataItem as BaseJobDeclaration;
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
					declaration.MergedSuccessfully += RefreshIvoiceLineFees;
					fWorkflowCustomFieldsGridLayoutPersister = new WorkflowCustomFieldsGridLayoutPersister(CustomsInvoiceLinesBoundGrid, declaration);

					var isAddCpwUSerControlAndEvent = IsAddCpwUserControlAndRegisterInteractEventForDeclaration;

					if (!isAddCpwUSerControlAndEvent)
					{
						var riskStatusColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(BaseJobComInvoiceLine.LinkedModuleCommodityRiskStatusDescription));
						if (riskStatusColumnStyle != null)
						{
							CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(riskStatusColumnStyle);
						}

						var complianceAlertsColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(BaseJobComInvoiceLine.LegalBookLink));
						if (complianceAlertsColumnStyle != null)
						{
							CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(complianceAlertsColumnStyle);
						}
					}

					if (!isAddCpwUSerControlAndEvent
						|| !ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice
						|| !OrganisationsDataRegistry.Instance.CustomsShowImportAlertsOnExportDeclarations.Value
						|| currentDataItem is not IComplianceCommodityRiskStatusProvider provider
						|| provider.RiskCalculateFactor != CommodityRiskCalculateFactor.Export)
					{
						var importAlsertColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(BaseJobComInvoiceLine.LinkedModuleCommodityImportAlertDescription));
						if (importAlsertColumnStyle != null)
						{
							CustomsInvoiceLinesBoundGrid.ColumnStyles.Remove(importAlsertColumnStyle);
						}
					}
				}

				CurrentDataItem_OnApportionmentDirtyChanged();
				ChangeContainerTabVisibility();
				SetCantCreateInvoiceLinesLabel();
				if (ShowCustomLabelsForInvoiceLine)
				{
					fInvoiceLineGridLayoutPersister = new CustomLabelsGridLayoutPersister(CustomsInvoiceLinesBoundGrid, new BaseJobComInvoiceLine.CustomLabelsProvider((ICustomsCustomLabelsConfigOrgProvider)currentDataItem), "", Core.Constants.CustomLabels.ComInvoiceLine.Prefix);
				}
			}
		}

		protected internal WorkflowCustomFieldsGridLayoutPersister fWorkflowCustomFieldsGridLayoutPersister;

		protected virtual bool ShowCustomLabelsForInvoiceLine
		{ get { return true; } }

		void UnHookControlVisibilityChangeEventsByCurrentDataItem()
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				currentDataItem.OnApportionmentDirtyChanged -= new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(CurrentDataItem_OnApportionmentDirtyChanged);
				currentDataItem.OnContainerDetailsChanged -= new EventHandler(CurrentDataItem_OnContainerDetailsChanged);
				currentDataItem.OnPartAttributeCaptionDetailsChanged -= new EventHandler(CurrentDataItem_OnPartAttributeCaptionDetailsChanged);

				if (SupportNewOwnerPartDetails)
				{
					currentDataItem.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
					currentDataItem.OnNewOwnerPartAttributeCaptionDetailsChanged -= CurrentDataItem_OnNewOwnerPartAttributeCaptionDetailsChanged;
				}
				currentDataItem.OnInvoiceLinesVisibilityChanged -= new EventHandler(invoiceProvider_OnInvoiceLinesVisibilityChanged);
				InvoiceStructureChangeEvent.RemoveInvoiceStructureChangedEventHandler(currentDataItem.Factory, invoiceProvider_OnInvoiceLinesVisibilityChanged);
				var declaration = currentDataItem as BaseJobDeclaration;
				if (declaration != null)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
					declaration.MergedSuccessfully -= RefreshIvoiceLineFees;
				}
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetInvoiceLineDetailsLayout();
		}

		void RefreshIvoiceLineFees()
		{
			if (CurrentDataItem != null && CurrentInvoiceLine != null)
			{
				RefreshIvoiceLineFeesCore(CurrentInvoiceLine);
			}
		}

		protected virtual void RefreshIvoiceLineFeesCore(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_Calc_DutyAmountIncludingWHEstimateInfo.RefreshBinding();
			invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimateInfo.RefreshBinding();
			invoiceLine.JI_Calc_GSTVATDeferredInfo.RefreshBinding();
		}

		bool ShouldInvoiceLinesUserControlBeHidden
		{
			get { return CurrentDataItem != null && (!CurrentDataItem.Invoices.Any() || CurrentDataItem.ShouldCreateDummyInvoiceLinesForMerge); }
		}

#if DEBUG
		protected virtual
#endif
		void SetCantCreateInvoiceLinesLabel()
		{
			var declaration = CurrentDataItem as BaseJobDeclaration;

			if (declaration != null && !declaration.IsDeleted && ShouldInvoiceLinesUserControlBeHidden)
			{
				TopPanel.Visible = false;
				BottomPanel.Visible = false;

				CantCreateInvoiceLinesLabel.Visible = true;
				CantCreateInvoiceLinesLabel.GetExtension<ILabelCaptionRenderer>().Caption = CantCreateInvoiceLinesText;
				CantCreateInvoiceLinesLabel.BringToFront();
			}
			else
			{
				CantCreateInvoiceLinesLabel.Visible = false;

				BottomPanel.Visible = true;
				TopPanel.Visible = true;
				TopPanel.BringToFront();
			}
		}

		protected virtual ZString CantCreateInvoiceLinesText
		{
			get { return Res.GetString("15f3890a-a8b9-40b6-bcd1-b92b37e0374a", "You must enter at least one Invoice Header before you can create Invoice Lines."); }
		}

		void CustomsInvoiceLinesBoundGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (((BaseJobComInvoiceLine)e.ObjectAtRow).HasNoChangesAndHasErrorResponse)
			{
				e.Colour = Color.Yellow;
			}
		}

		void CurrentDataItem_OnPartAttributeCaptionDetailsChanged(object sender, EventArgs e)
		{
			SetPartAttributeCaptions();
		}

		void CurrentDataItem_OnContainerDetailsChanged(object sender, EventArgs e)
		{
			ChangeContainerTabVisibility();
		}

		void invoiceProvider_OnInvoiceLinesVisibilityChanged(object sender, EventArgs e)
		{
			SetCantCreateInvoiceLinesLabel();
		}

		void SetPartAttributeCaptions()
		{
			var importer = CurrentDataItem?.Importer;
			if (importer != null)
			{
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1, importer.PartAttributeManager.PartAttributeName1);
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2, importer.PartAttributeManager.PartAttributeName2);
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3, importer.PartAttributeManager.PartAttributeName3);
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib1, Res.GetString("26CFD13A-762E-4752-9C29-F564A1ABAB11", "Part Attrib. 1"));
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib2, Res.GetString("ABDC1584-3FC5-4552-9C3A-E5016AC4CAA5", "Part Attrib. 2"));
				CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLineSchema.Constants.JI_PartAttrib3, Res.GetString("9C63CA12-7D84-44c0-8A87-516B697BB4CD", "Part Attrib. 3"));
			}
		}

		void CurrentDataItem_OnApportionmentDirtyChanged()
		{
			if (CurrentDataItem != null)
			{
				PendingApportionmentLabel.Visible = CurrentDataItem.ApportionmentDirty;
				if (CurrentDataItem.ApportionmentDirty)
				{
					PendingApportionmentLabel.BringToFront();
					PendingApportionmentLabel.Visible = true;
				}
				else
				{
					RefreshIvoiceLineFees();
					PendingApportionmentLabel.SendToBack();
					PendingApportionmentLabel.Visible = false;
				}

				if (fInvoiceLineCharges != null && LineChargesTabPage.TabVisible && CurrentDataItem is BaseJobDeclaration declaration)
				{
					var incoTermAndChargeFactory = declaration.IncoTermAndChargeFactory;
					var invoiceLineCharges = incoTermAndChargeFactory.GetChargeList(Common.ChargeParentTypes.InvoiceLine);
					fInvoiceLineCharges.ControlVisibilityOfChargesGrid(invoiceLineCharges.Any());
				}
			}
		}

		protected void ControlVisiblityOfLineChargesTabPage(bool isVisible)
		{
			if (LineChargesTabPage != null)
			{
				LineChargesTabPage.TabVisible = isVisible;
			}
		}

		protected virtual void ChangeContainerTabVisibility()
		{
			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.ContainersRequired)
				{
					if (!LineDetailTabControl.TabPages.Contains(ContainersTabPage))
					{
						LineDetailTabControl.TabPages.Insert(ContainersTabPage, 2);
					}
				}
				else if (LineDetailTabControl.TabPages.Contains(ContainersTabPage))
				{
					LineDetailTabControl.TabPages.Remove(ContainersTabPage);
					LineDetailTabControl.SelectedTab = (ZTabPage)LineDetailTabControl.TabPages[0];
				}
			}
		}

		void ChangeInvoiceLineComponentsVisibility(object sender, EventArgs e)
		{
			if (LineComponentsTabPage != null)
			{
				LineComponentsTabPage.TabVisible = CurrentInvoiceLine?.IsInvoiceLineComponentsVisible ?? false;
			}
		}

		async void InvoiceLine_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 1)
			{
				var hitTest = CustomsInvoiceLinesBoundGrid.HitTest(e.X, e.Y);
				var row = hitTest.Row;
				var col = hitTest.Column;
				if (row >= 0 && col >= 0)
				{
					const string columnName = "LegalBookLink";
					var column = CustomsInvoiceLinesBoundGrid.Columns[col];
					if (column.ColumnName == columnName)
					{
						var listManager = CustomsInvoiceLinesBoundGrid.ListManager.List;
						if (row < listManager.Count)
						{
							if (listManager[row] is BaseJobComInvoiceLine commodityDetail)
							{
								await commodityDetail.LinkedModuleCommodity.ViewBorderWisePortalIfAvailable();
							}
						}
					}
				}
			}
		}

		#endregion

		#region Invoice Line Container Tab

		void SelectContainerForAllInvoiceLines(object sender, EventArgs args)
		{
			if (CusContainerInvoiceLineGrid.CurrentRowIndex >= 0)
			{
				NonPersistentCusContainer selectedContainer = CusContainerInvoiceLineGrid.ListManager.GetCurrent() as NonPersistentCusContainer;
				if (selectedContainer != null)
				{
					selectedContainer.IsForInvoiceLine = true;
					CurrentDataItem.SelectContainerForAllInvoiceLines(selectedContainer);
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("562dd9e2-7541-4d9d-a427-995bcefd6757", "Please select a row before attempting to select a container for all invoice lines."));
			}
		}

		void UnselectContainerForAllInvoiceLines(object sender, EventArgs args)
		{
			if (CusContainerInvoiceLineGrid.CurrentRowIndex >= 0)
			{
				NonPersistentCusContainer selectedContainer = CusContainerInvoiceLineGrid.ListManager.GetCurrent() as NonPersistentCusContainer;
				if (selectedContainer != null)
				{
					selectedContainer.IsForInvoiceLine = false;
					CurrentDataItem.UnSelectContainerForAllInvoiceLines(selectedContainer);
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("bd6b1680-65b1-457a-9541-a9f095424fcd", "Please select a row before attempting to deselect a container for all invoice lines."));
			}
		}

		#endregion

		#region Grid

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			SetPartAttributeCaptions();
			SetNewOwnerPartVisibility();
			FixCustomsInvoiceLinesBoundGridBug();
			AddApportionWeightMenuToGrid();
			AddCreateClassificationAssistantRequestMenuToGrid();
			SetLineCalculationsPanelLayout();
			AddCpwUserControlAndRegisterInteractEventForDeclarationIfNeeded();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			InitTabsVisibility();
		}

		#region CurrentInvoiceLine & CustomsInvoiceLinesBoundGrid PositionChanged

		protected virtual void CustomsInvoiceLinesBoundGrid_AfterBind(object sender, EventArgs e)
		{
			CustomsInvoiceLinesBoundGrid.ListManager.CurrentChanged -= CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged;
			CustomsInvoiceLinesBoundGrid.ListManager.CurrentChanged += CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged;
			CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(null, null);

			CustomsInvoiceLinesBoundGrid.ListManager.CurrentItemChanged -= CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged;
			CustomsInvoiceLinesBoundGrid.ListManager.CurrentItemChanged += CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged;
			CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(null, null);
		}

		protected virtual void CustomsInvoiceLinesBoundGrid_ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UpdateCurrentInvoiceLineAndRehookEvents();
			UpdateBondedWHSControlsVisibility();
		}

		void UpdateBondedWHSControlsVisibility()
		{
			if (CurrentInvoiceLine != null)
			{
				BondedWHSOrderLineNumberCalcEdit.Visible = BondedWHSOrderNumberTextBox.Visible = CurrentInvoiceLine.IsBondedWHSOrderNumberVisible;
			}
		}

		protected void UpdateCurrentInvoiceLineAndRehookEvents()
		{
			var currentGridItem = CustomsInvoiceLinesBoundGrid?.ListManager?.GetCurrent() as BaseJobComInvoiceLine;
			if (currentGridItem == null || currentGridItem.IsDeleted)
			{
				fCurrentInvoiceLine = null;
			}
			else if (fCurrentInvoiceLine != currentGridItem)
			{
				UnHookInvoiceLineEvents(fCurrentInvoiceLine);
				fCurrentInvoiceLine = currentGridItem;
				HookInvoiceLineEvents(fCurrentInvoiceLine);
				InitializeLinkedModuleCommodityIfNeeded(fCurrentInvoiceLine);
			}
		}

		protected virtual void HookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				invoiceLine.JI_ProcedureInfo.ValueChanged += ChangeInvoiceLineComponentsVisibility;
				ChangeInvoiceLineComponentsVisibility(null, null);
			}
		}

		protected virtual void UnHookInvoiceLineEvents(BaseJobComInvoiceLine invoiceLine)
		{
			if (invoiceLine != null)
			{
				invoiceLine.JI_ProcedureInfo.ValueChanged -= ChangeInvoiceLineComponentsVisibility;
			}
		}

		protected virtual void CustomsInvoiceLinesBoundGrid_ListManager_CurrentItemChanged(object sender, EventArgs e) { }

		BaseJobComInvoiceLine fCurrentInvoiceLine;
		protected internal BaseJobComInvoiceLine CurrentInvoiceLine => fCurrentInvoiceLine;

		#endregion

		void InitTabsVisibility()
		{
			var dynamicLayoutApplied = DynamicLayoutApplied;
			NewLineDetailsTabPage.TabVisible = dynamicLayoutApplied;
			LineDetailsTabPage.TabVisible = !dynamicLayoutApplied;
		}

		void FixCustomsInvoiceLinesBoundGridBug()
		{
			// This is a temporary hack for the grid focus problem which will be resolved properly when ZPropertyInfo.ReadOnly is replaced by K MetaData
			// Please remove when ZPropertyInfo.ReadOnly is using MetaData
			BeginInvoke(new MethodInvoker(delegate
			{
				if (CustomsInvoiceLinesBoundGrid != null && CustomsInvoiceLinesBoundGrid.ListManager != null)
				{
					CustomsInvoiceLinesBoundGrid.ListManager.Refresh();
				}
			}));
		}

		protected internal CustomLabelsGridLayoutPersister fInvoiceLineGridLayoutPersister;

		protected virtual ZArchitecture.Modules.ModuleIdentifier ClassificationModuleID
		{
			get { return Enterprise.ZArchitecture.Modules.ModuleIDs.SingleTariffClassification; }
		}

		#endregion

		#region Exposing Controls for unit tests

		public ConvertToLocalCurrencyControl BalanceConvertToLocalCurrencyControl
		{
			get { return JI_Calc_BalanceConvertToLocalCurrencyControl; }
		}

		public Control DutyConvertToLocalCurrencyControl
		{
			get { return JI_Calc_DutyConvertToLocalCurrencyControl; }
		}

		public Control GSTConvertToLocalCurrencyControl
		{
			get { return JI_Calc_GSTConvertToLocalCurrencyControl; }
		}

		#endregion

		#region Autogenerated Codes
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fInvoiceLineCharges != null)
				{
					fInvoiceLineCharges.Dispose();
				}

				if (ContainersTabPage != null)
				{
					ContainersTabPage.Dispose();
				}
				if (fInvoiceLineGridLayoutPersister != null)
				{
					fInvoiceLineGridLayoutPersister.Dispose();
					fInvoiceLineGridLayoutPersister = null;
				}
				UnHookControlVisibilityChangeEventsByCurrentDataItem();

				var invoiceLine = CustomsInvoiceLinesBoundGrid.ListManager?.GetCurrent() as BaseJobComInvoiceLine;
				if (invoiceLine != null)
				{
					UnHookInvoiceLineEvents(invoiceLine);
				}

				if (components != null)
				{
					components.Dispose();
				}

				if (allSuspendersForInvoiceLineDeleting != null)
				{
					foreach (IDisposable suspender in allSuspendersForInvoiceLineDeleting)
					{
						if (suspender != null)
						{
							suspender.Dispose();
						}
					}
					allSuspendersForInvoiceLineDeleting = null;
				}

				if (StripControl != null)
				{
					StripControl.PerformSearch -= StripControl_PerformSearch;
				}
				invoiceHeaders = null;
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Dynamic Layout

		void AddDynamicLayoutUserControl()
		{
			if (DynamicLayoutApplied)
			{
				InvoiceLineDetailsUserControl.SetInvoiceLineDetailsLayout(InvoiceLineDetailsPanelLayout);
			}
		}

		void SetInvoiceLineDetailsLayout()
		{
			if (HasDifferentPanelLayout && DynamicLayoutApplied)
			{
				invoiceLineDetailsPanelLayout = null;
				InvoiceLineDetailsUserControl.SetInvoiceLineDetailsLayout(InvoiceLineDetailsPanelLayout);
			}
		}

		IPanelLayoutProvider InvoiceLineDetailsPanelLayout => invoiceLineDetailsPanelLayout ?? (invoiceLineDetailsPanelLayout = GetNewInvoiceLineDetailsPanelLayout());
		IPanelLayoutProvider invoiceLineDetailsPanelLayout;

		protected virtual IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new CommonInvoiceLineDetailsLayouts();

		protected virtual ZBool DynamicLayoutApplied => ZBool.False;

		protected virtual ZBool HasDifferentPanelLayout => ZBool.False;

		#endregion

		BaseJobDeclaration JobDeclaration => CurrentDataItem as BaseJobDeclaration;

		IDeclarationFormLayoutProvider DeclarationFormLayoutProvider
		{
			get
			{
				if (declarationFormLayoutProvider == null)
				{
					declarationFormLayoutProvider = GUI.DeclarationFormLayoutProvider.GetLayoutProvider(JobDeclaration);
				}
				return declarationFormLayoutProvider;
			}
		}
		IDeclarationFormLayoutProvider declarationFormLayoutProvider;

		#region Dynamic Layout: Invoice Line Summary

		void SetLineCalculationsPanelLayout()
		{
			if (DeclarationFormLayoutProvider?.GetInvoiceLineCalculationsPanelLayout(JobDeclaration) is IPanelLayoutProvider layoutProvider)
			{
				var calculationsDynamicLayoutPanel = new DynamicLayoutPanel();
				BindingSource.SetBindingMember(calculationsDynamicLayoutPanel, nameof(BaseJobDeclaration.FilteredInvoiceLines));
				calculationsDynamicLayoutPanel.Dock = DockStyle.Fill;
				calculationsDynamicLayoutPanel.Name = "CalculationsDynamicLayoutPanel";

				InvoiceLinesSummaryGroupBox.Controls.Remove(LineSummaryPanel);
				LineSummaryPanel.Dispose();
				InvoiceLinesSummaryGroupBox.Controls.Remove(CurrentInvoicePanel);
				CurrentInvoicePanel.Dispose();

				InvoiceLinesSummaryGroupBox.Controls.Add(calculationsDynamicLayoutPanel);
				PendingApportionmentLabel.AllowOverlap(calculationsDynamicLayoutPanel);

				calculationsDynamicLayoutPanel.UpdateLayout(layoutProvider);
			}
		}

		#endregion

		ZCalcEditColumnStyleInfo c2PivotContainerWeightColumnStyleInfo9;
		ZCalcEditColumnStyleInfo c2PivotContainerGrossWeightColumnStyleInfo10;
		ZCalcEditColumnStyleInfo c2PivotContainerNetWeightColumnStyleInfo11;
		ZCalcEditColumnStyleInfo c2PivotContainerPackQtyColumnStyleInfo12;
		ZCalcEditColumnStyleInfo c2PivotContainerSplitValueColumnStyleInfo13;
		ZGuidFindBoxColumnStyleInfo c2ContainerSplitCurrencyColumnStyleInfo2;
	}
}
