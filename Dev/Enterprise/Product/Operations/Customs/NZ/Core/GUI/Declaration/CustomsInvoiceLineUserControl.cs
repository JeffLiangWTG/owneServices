using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public CustomsInvoiceLineUserControl()
			: base()
		{
			InitializeComponent();
			SetupTariffFindBox();
			SetupNonDesignerableItems();

			TSWTabPage.AllowOverlap(ProductNameGroupBox);
			ProductNameGroupBox.AllowOutsideOfParent();
			TSWTabPage.AllowOverlap(DangerousGoodsGroupBox);
			DangerousGoodsGroupBox.AllowOutsideOfParent();
			TSWTabPage.AllowOverlap(ProductsGroupBox);
			ProductsGroupBox.AllowOutsideOfParent();
			TSWTabPage.AllowOverlap(ProductCharacteristicGroupBox);
			ProductCharacteristicGroupBox.AllowOutsideOfParent();
			TSWTabPage.AllowOverlap(IntendedUseGroupBox);
			IntendedUseGroupBox.AllowOutsideOfParent();
			GeneticallyModifiedCheckBox.AllowOverlap(UsedGoodsCheckBox);
		}

		protected override bool UseUniversalTariff => UniversalTariffHelper.UseRefDatabaseData;

		protected override ZDateTime GetEffectiveAssessmentDateForUniversalTariff() => (CurrentInvoiceLine as ITariffValidationData)?.DateForDutyRate ?? ZDateTime.Today;

		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.NewZealand;

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.NewZealand;

		void SetupTariffFindBox()
		{
			TariffCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			TariffCodeFindBox.GetCountryCode = GetCustomsCountryCode;
			TariffCodeFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			TariffCodeFindBox.GetEffectiveDate = () => (CurrentInvoiceLine as ITariffValidationData)?.DateForDutyRate ?? ZDateTime.Today;

			PartsOfClassificationFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			PartsOfClassificationFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.NewZealand;
			PartsOfClassificationFindBox.GetEffectiveDate = () => (CurrentInvoiceLine as ITariffValidationData)?.DateForDutyRate ?? ZDateTime.Today;
		}

		void SetupNonDesignerableItems()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var useRefDB = UniversalTariffHelper.UseRefDatabaseData;
				NZCClassificationFindBox.Visible = !useRefDB;
				JI_PartsOfClassificationNZcClassFindBox.Visible = !useRefDB;
				ConcessionCodeFindBox.Visible = !useRefDB;

				TariffCodeFindBox.Visible = useRefDB;
				PartsOfClassificationFindBox.Visible = useRefDB;
				ConcessionCodeDropEdit.Visible = useRefDB;

				if (!useRefDB)
				{
					var zCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
					zCodeFindBoxColumnStyleInfo.ColumnName = "JI_ConcessionCode";
					zCodeFindBoxColumnStyleInfo.ModuleID = ModuleIDs.RefCountry;
					_ = CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo);

					ConcessionCodeFindBox.ModuleID = ModuleIDs.Customs.NZ.Concession;
					CustomsInvoiceLinesBoundGrid.SetColumnModuleID(JobComInvoiceLine.Schema.JI_ConcessionCode, ModuleIDs.Customs.NZ.Concession);

					var nzcClassColumnStyleInfo = new NZCClassColumnStyleInfo();
					nzcClassColumnStyleInfo.BindToList = "Lookups.TariffList";
					nzcClassColumnStyleInfo.ColumnName = "JI_Tariff";
					nzcClassColumnStyleInfo.ToolTip = "NZ Classification";
					nzcClassColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
					_ = CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(nzcClassColumnStyleInfo);
				}
				else
				{
					var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
					zDropEditColumnStyleInfo.ColumnName = "JI_ConcessionCode";
					zDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
					zDropEditColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
					_ = CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
				}
				ContainerControlToolTip.SetToolTip(this.JI_Calc_GSTConvertToLocalCurrencyControl, GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription + " value for current line item");
				SetColumnPositionsAndCaption();
			}

			LineDetailTabControl.Dock = DockStyle.Fill;
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
			set { base.JobDeclaration = value; }
		}

		void SetColumnPositionsAndCaption()
		{
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_LineNo, 0, ResString.GetMultilingualString("7821df59-57b1-4813-8744-ed612743d8c9", "Inv. Line#"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.BaseJobComInvoiceLine.Schema.MergedLineNumber, 1, ResString.GetMultilingualString("6fb49185-51b4-4e53-a946-9e4323a7dc33", "Merged Line#"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, 2, ResString.GetMultilingualString("6fdca41b-4ccd-4429-80b8-cb50f84f9bf4", "Invoice"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_PartNo, 3, ResString.GetMultilingualString("b75d02ad-6c66-422f-b4c7-7bd9219cfd81", "Product Code"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_CC, 4, ResString.GetMultilingualString("995e6a77-a5de-4026-afca-87f1b030e890", "Lookup Code"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_Tariff, 5, ResString.GetMultilingualString("79366b7a-d4d1-46b1-989a-d5b817e2044d", "Tariff Code"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_ConcessionCode, 6, ResString.GetMultilingualString("1cb8a48a-d170-46ae-9ffa-68277f45e121", "Concession Code"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description, 7, ResString.GetMultilingualString("bb1f861c-f651-4710-a64a-f590946b72dd", "Goods Description"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_InvoiceQuantity, 8, ResString.GetMultilingualString("4732494a-6816-4262-91a0-22ae692b8224", "Inv. Qty"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_InvoiceUQ, 9, ResString.GetMultilingualString("a4427f95-cfb0-4d8e-83b7-cc197f3178e2", "Inv. UQ"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.BaseJobComInvoiceLine.Schema.UnitPrice, 10, ResString.GetMultilingualString("a0448f1d-a547-465c-879d-9dfb4cc3501c", "Unit Price"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice, 11, ResString.GetMultilingualString("8281efcc-f8de-4df9-a66e-148d10ec8394", "Line Price"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin, 12, ResString.GetMultilingualString("e9dd3cbb-a214-421a-9fdf-c26f637ef339", "Origin"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_RN_NKCountryOfExport, 13, ResString.GetMultilingualString("a5424841-18f5-418e-8674-778b576950e6", "Export"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_QualifiesForPreferentialDuty, 14, ResString.GetMultilingualString("2b6fd69e-c83e-4a0d-9ce3-0b776b00da7e", "Pref"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_PreferentialCountryGroup, 15, ResString.GetMultilingualString("63bd5e63-a7c4-4f1f-a4ae-42a32a09bb7a", "Pref Group"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsQuantity, 16, ResString.GetMultilingualString("44bd542d-470b-47de-b108-e2c0bd601426", "Cust. Qty"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsUnitQty, 17, ResString.GetMultilingualString("4049339a-9bd3-4316-a36d-b5eb77a531bb", "Cust. UQ"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_SupplementaryQty, 18, ResString.GetMultilingualString("fc2cf9b3-e4cb-4583-9d98-06f8999fd214", "Supp. Qty"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_SupplementaryUQ, 19, ResString.GetMultilingualString("990532e4-e814-46ce-a474-b65a2b3d86bb", "Supp. UQ"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_IsZeroRatedDuty, 20, ResString.GetMultilingualString("053bccb4-1932-4f7a-a429-a653791c4fe8", "Duty Zero Rated"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_IsZeroRatedExcise, 21, ResString.GetMultilingualString("535f6097-22aa-4b2c-9ff0-41492eac0191", "Excise Zero Rated"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_IsZeroRatedLevies, 22, ResString.GetMultilingualString("3ba5a7f7-68fb-4532-b872-065e115233aa", "Levies Zero Rated"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_IsZeroRatedGST, 23, ResString.GetMultilingualString("58a469c2-74d5-4137-a641-aa3a23a6340b", "GST Zero Rated"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_AntiDumpingDutyAmount, 24, ResString.GetMultilingualString("da3965f1-b9f6-4502-bc81-1a58b4df9b00", "Anti Dumping Duty"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_CountervailingDutyAmount, 25, ResString.GetMultilingualString("1a2b615c-46f9-469b-90dd-458b5ce8ca71", "Countervailing Duty"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_DutyCreditAmount, 26, ResString.GetMultilingualString("06bc99c8-a20c-4594-b81d-54e54855b18a", "Duty Credit"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_GSTCreditAmount, 27, ResString.GetMultilingualString("96f6eb05-a508-47a4-8b6c-fcd4f4dfb67c", "GST Credit"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_LevyCreditAmount, 28, ResString.GetMultilingualString("df1f94ce-c3a8-4aea-9364-730fa1de1c61", "Levy Credit"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_LevyCreditAmountCode, 29, ResString.GetMultilingualString("98d915a2-6c49-426d-82d9-d01e2783cfdf", "Levy Credit Code"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_DepositRefundAmount, 30, ResString.GetMultilingualString("6ea1abd2-bea8-4b8c-b44f-618e2256ea33", "Deposit Refund"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, JobComInvoiceLine.Schema.JI_ExciseDutyCreditAmount, 31, ResString.GetMultilingualString("bd8b9927-79d8-492b-aba4-3e5bd64c6642", "Excise Duty Credit"));
			SetColumnPositionAndCaptionBeforeBinding(CustomsInvoiceLinesBoundGrid, Customs.Business.AutoJobComInvoiceLine.Schema.JI_RH_NKCommodity_Code, 32, ResString.GetMultilingualString("8ef68e2e-b309-4bea-acfb-a4fac1866158", "Commodity Code"));
		}

		void SetColumnPositionAndCaptionBeforeBinding(ZGrid grid, string columnName, int newIndex, string columnCaption)
		{
			Core.Forms.ZGridColumnInfo columnInfo = grid.GetColumnStyle(columnName);
			if (columnInfo == null)
			{
				ErrorReporter.ReportOnce(columnName + ":" + this.Name + "." + grid.Name, "Column [" + columnName + "] does not exist in the Grid [" + this.Name + "." + grid.Name + "]. Cannot set Column Position.");
			}
			else
			{
				if (columnCaption != null)
				{
					columnInfo.Caption = columnCaption;
				}

				grid.ColumnStyles.Remove(columnInfo);
				grid.ColumnStyles.Insert(newIndex, columnInfo);
			}
		}

		#region Code List Tab Control Title Management

		#region Event Hookup to Setup/Maintain CurrentInvoiceLine

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (FilteredInvoiceLinesListManager != null)
			{
				FilteredInvoiceLinesListManager.CurrentChanged -= new EventHandler(FilteredInvoiceLinesListManager_CurrentChanged);
			}

			base.SetDataBinding(dataSource, dataMember);
			if (FilteredInvoiceLinesListManager != null)
			{
				CurrentInvoiceLine = JobDeclaration.FilteredInvoiceLines.Count > 0 ? FilteredInvoiceLinesListManager.GetCurrent() as JobComInvoiceLine : null;
				FilteredInvoiceLinesListManager.CurrentChanged += new EventHandler(FilteredInvoiceLinesListManager_CurrentChanged);
			}

			if (JobDeclaration != null)
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(JobDeclaration.IsImport || JobDeclaration.IsExcise, JobComInvoiceLine.Schema.JI_ConcessionCode);
			}
		}

		CurrencyManager FilteredInvoiceLinesListManager
		{
			get { return (CurrencyManager)GetBindingManager("FilteredInvoiceLines"); }
		}

		protected override void Dispose(bool disposing)
		{
			CurrentInvoiceLine = null; // Unhooks events attached to the CurrentInvoiceLine.
			base.Dispose(disposing);
		}

		void FilteredInvoiceLinesListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (FilteredInvoiceLinesListManager != null)
			{
				if (JobDeclaration.FilteredInvoiceLines.Count > 0)
				{
					CurrentInvoiceLine = FilteredInvoiceLinesListManager.GetCurrent() as JobComInvoiceLine;
				}
				else
				{
					CurrentInvoiceLine = null;
				}
			}
		}
		#endregion

		#region CurrentInvoiceLine and associated Event Linking/Unlinking to the CurrentInvoiceLine
		JobComInvoiceLine fCurrentInvoiceLine;
		protected new JobComInvoiceLine CurrentInvoiceLine
		{
			get { return fCurrentInvoiceLine; }
			set
			{
				if (fCurrentInvoiceLine != null)
				{
					fCurrentInvoiceLine.PermitCodes.CountChanged -= new CollectionCountChangedEventHandler(PermitCodes_CountChanged);
					fCurrentInvoiceLine.ProhibitedCodes.CountChanged -= new CollectionCountChangedEventHandler(ProhibitedCodes_CountChanged);
					fCurrentInvoiceLine.OtherInfos.CountChanged -= new CollectionCountChangedEventHandler(OtherInfos_CountChanged);
				}

				fCurrentInvoiceLine = value;
				CodeInfosTabControl.Enabled = value != null;
				if (fCurrentInvoiceLine != null)
				{
					fCurrentInvoiceLine.PermitCodes.CountChanged += new CollectionCountChangedEventHandler(PermitCodes_CountChanged);
					fCurrentInvoiceLine.ProhibitedCodes.CountChanged += new CollectionCountChangedEventHandler(ProhibitedCodes_CountChanged);
					fCurrentInvoiceLine.OtherInfos.CountChanged += new CollectionCountChangedEventHandler(OtherInfos_CountChanged);

					CodeInfosTabControl.Enabled = true;
				}
				else
				{
					CodeInfosTabControl.Enabled = false;
				}

				UpdatePermitCodesTitleWithCount();
				UpdateProhibitedCodesTitleWithCount();
				UpdateOtherInfosTitleWithCount();
				SetLevyVisibility();
			}
		}

		void PermitCodes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdatePermitCodesTitleWithCount();
		}

		void ProhibitedCodes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateProhibitedCodesTitleWithCount();
		}

		void OtherInfos_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateOtherInfosTitleWithCount();
		}
		#endregion

		#region Code List Title Update Methods
		void UpdatePermitCodesTitleWithCount()
		{
			int currentCount = CurrentInvoiceLine == null ? 0 : CurrentInvoiceLine.PermitCodes.Count;
			PermitCodesPage.Text = Enterprise.Customs.NZ.GUI.Res.GetString("1c113353-1822-4265-8b0a-b7f2768d6bac", "Permits ({0})", currentCount.ToString());
		}

		void UpdateProhibitedCodesTitleWithCount()
		{
			int currentCount = CurrentInvoiceLine == null ? 0 : CurrentInvoiceLine.ProhibitedCodes.Count;
			ProhibitedCodesTabPage.Text = Enterprise.Customs.NZ.GUI.Res.GetString("c213c6f7-b6a0-4edb-a9bf-7d4b741640f2", "Prohibited Codes ({0})", currentCount.ToString());
		}

		void UpdateOtherInfosTitleWithCount()
		{
			int currentCount = CurrentInvoiceLine == null ? 0 : CurrentInvoiceLine.OtherInfos.Count;
			OtherInfosTabPage.Text = Enterprise.Customs.NZ.GUI.Res.GetString("79937ede-dd4d-4870-a079-d953f3b9575d", "Other Infos ({0})", currentCount.ToString());
		}
		#endregion

		#endregion

		#region Control Visibility based on Message Type

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				LoadPlugins();
			}
			ChangeControlsVisibility();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			if (!DesignModeFinder.IsDesigning)
			{
				var isImport = JobDeclaration.IsImport;
				var isExport = JobDeclaration.IsExport;
				var isExcise = JobDeclaration.IsExcise;
				var declarationCanHaveCalculatedDutyLevyOrGST = (isImport || isExcise);
				var isExportDrawbackOrCompletion = (isExport && (JobDeclaration.IsDrawback || JobDeclaration.IsCompletion));
				var isCompletion = ((isImport || isExport) && JobDeclaration.IsCompletion);
				var isTSW = JobDeclaration.IsTSWDeclaration;
				var isWriteOff = JobDeclaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff;
				var useRefDb = UniversalTariffHelper.UseRefDatabaseData;

				#region Origin Region when Export, CUS

				JI_EffectiveOriginRegionTextBox.Visible = isTSW || !isExport;
				JI_OriginRegionTextBox.Visible = isTSW || !isExport;

				#endregion

				#region User Entered Customs Charges.
				SetLevyVisibility();
				AntiDumpingDutyCalcEdit.Visible = isImport;
				CountervailingDutyCalcEdit.Visible = isImport;
				DutyCreditCalcEdit.Visible = isExportDrawbackOrCompletion;
				GSTCreditCalcEdit.Visible = isExportDrawbackOrCompletion;
				DepositRefundCalcEdit.Visible = isCompletion;
				ExciseDutyCreditCalcEdit.Visible = isExcise;
				#endregion

				#region Fields Used For Calculation Of Duty/Levy/GST
				PreferenceDropEdit.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				PreferentialCountryGroupDropEdit.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				ZeroRatedGroupBox.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				CountryOfExportCodeFindBox.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				ConcessionCodeFindBox.Visible = !useRefDb && declarationCanHaveCalculatedDutyLevyOrGST;
				ConcessionCodeDropEdit.Visible = useRefDb && declarationCanHaveCalculatedDutyLevyOrGST;
				DutyRateTextBox.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				#endregion

				#region Calculated Duty/Levy/GST Controls
				JI_Calc_DutyConvertToLocalCurrencyControl.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				LevyCurrencyControl.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				SetDataSourceBinding(LevyCurrencyControl.GetExtension<LabelCaptionRenderer>(), "Caption", "FilteredInvoiceLines.JI_Calc_LevyTypeDescription", true);
				JI_Calc_GSTConvertToLocalCurrencyControl.Visible = declarationCanHaveCalculatedDutyLevyOrGST;
				#endregion

				#region Trade Single Window

				TSWTabPage.TabVisible = isTSW;
				DangerousGoodsTabPage.TabVisible = !isTSW; // These fields are now on the Additional Detail tab for TSW
				PackagingGroupBox.Visible = isTSW && !isWriteOff;
				CommodityDetailsGroupBox.Visible = !isExport;
				IntendedUseGroupBox.Visible = !isExport;
				ClassificationGroupBox.Visible = isTSW && !isExport;
				ProductsGroupBox.Visible = isTSW && !isExport;
				ConstituentsGroupBox.Visible = !isExport;
				ProductNameGroupBox.Visible = !isExport;

				#endregion
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			var isImport = JobDeclaration.IsImport;
			var isExport = JobDeclaration.IsExport;
			var isExcise = JobDeclaration.IsExcise;
			var declarationCanHaveCalculatedDutyLevyOrGST = (isImport || isExcise);
			var isExportDrawbackOrCompletion = (isExport && (JobDeclaration.IsDrawback || JobDeclaration.IsCompletion));
			var isCompletion = ((isImport || isExport) && JobDeclaration.IsCompletion);
			var isTSW = JobDeclaration.IsTSWDeclaration;
			var isWriteOff = JobDeclaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff;

			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(isExportDrawbackOrCompletion, JobComInvoiceLine.Schema.JI_LevyCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isExportDrawbackOrCompletion && isTSW, JobComInvoiceLine.Schema.JI_LevyCreditAmountCode);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_AntiDumpingDutyAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isImport, JobComInvoiceLine.Schema.JI_CountervailingDutyAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isExportDrawbackOrCompletion, JobComInvoiceLine.Schema.JI_DutyCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isExportDrawbackOrCompletion, JobComInvoiceLine.Schema.JI_GSTCreditAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isCompletion, JobComInvoiceLine.Schema.JI_DepositRefundAmount);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isExcise, JobComInvoiceLine.Schema.JI_ExciseDutyCreditAmount);

				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_QualifiesForPreferentialDuty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_PreferentialCountryGroup);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_IsZeroRatedDuty);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_IsZeroRatedExcise);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_IsZeroRatedLevies);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_IsZeroRatedGST);
				CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, Customs.Business.AutoJobComInvoiceLine.Schema.JI_RN_NKCountryOfExport);
				if (CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ConcessionCode))
				{
					CustomsInvoiceLinesBoundGrid.SetAvailability(declarationCanHaveCalculatedDutyLevyOrGST, JobComInvoiceLine.Schema.JI_ConcessionCode);
				}

				CustomsInvoiceLinesBoundGrid.SetAvailability(isTSW && isImport, JobComInvoiceLine.Schema.JI_IntendedUse);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isTSW && isImport, JobComInvoiceLine.Schema.JI_IntendedUseCode);
				CustomsInvoiceLinesBoundGrid.SetAvailability(isTSW || !isExport, JobComInvoiceLine.Schema.JI_OriginRegion);

				ChangeGridColumnsMandatory(isTSW, isWriteOff, isImport);

				using (ItemPackagingGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					ItemPackagingGrid.SetAvailability(isTSW && isImport, AutoItemPackaging.Schema.NZ_PackingMaterial);
				}
			}
		}

		protected override bool SupportsExtraPhysicalQuantitiesOnC2Pivot => true;

		void ChangeGridColumnsMandatory(bool isTSW, bool isWriteOff, bool isImport)
		{
			var existsInvoiceColumnStyle = CustomsInvoiceLinesBoundGrid.GetColumnStyle(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice) != null;
			if (existsInvoiceColumnStyle)
			{
				CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice, isTSW);
			}

			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_Tariff, isTSW);
			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice, isTSW);
			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description, isTSW);
			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin, isTSW);
			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_Weight, isTSW);
			CustomsInvoiceLinesBoundGrid.SetColumnMandatory(Customs.Business.AutoJobComInvoiceLine.Schema.JI_NetWeight, isTSW);

			SetGridFieldProperties("NumberOfPackages1", isTSW && !isWriteOff, isImport);
			SetGridFieldProperties("Packages1UQ", isTSW && !isWriteOff, isImport);
			SetGridFieldProperties("PackagesVolume1", isTSW && !isWriteOff, isImport);
			SetGridFieldProperties("PackageVolume1UQ", isTSW && !isWriteOff, isImport);
			SetGridFieldProperties("PackagingMarks1", isTSW && !isWriteOff, isImport);
			SetGridFieldProperties("PackagingMaterial1", isTSW && !isWriteOff, isImport);

			if (isTSW)
			{
				if (existsInvoiceColumnStyle)
				{
					CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
				}

				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_Tariff);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_Weight);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, Customs.Business.AutoJobComInvoiceLine.Schema.JI_NetWeight);
			}
		}

		void SetGridFieldProperties(string columnField, bool isTSW, bool isImport)
		{
			if (columnField == "PackagingMaterial1")
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(isTSW && isImport, columnField);
				CustomsInvoiceLinesBoundGrid.SetColumnMandatory(columnField, false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(isTSW && isImport, columnField);
			}
			else
			{
				CustomsInvoiceLinesBoundGrid.SetAvailability(isTSW, columnField);
				CustomsInvoiceLinesBoundGrid.SetColumnMandatory(columnField, isTSW && columnField != "PackagingMaterial1");
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(isTSW, columnField);
			}
		}

		void SetLevyVisibility()
		{
			if (JobDeclaration != null)
			{
				var isExportDrawbackOrCompletion = JobDeclaration.IsExport && (JobDeclaration.IsDrawback || JobDeclaration.IsCompletion);
				LevyCreditCalcEdit.Visible = isExportDrawbackOrCompletion;
				LevyCodeDropEdit.Visible = isExportDrawbackOrCompletion;
				if (isExportDrawbackOrCompletion)
				{
					LevyCodeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("663e8066-73c5-4f0e-a352-d59bbfdc8254", "Levy Credit Amount");
					LevyCodeDropEdit.RefreshCaptionLabel();
				}
			}
		}

		protected override bool DisplayCreateClassificationAssistantRequestMenu => JobDeclaration.JE_MessageType != NZJobMessageTypeList.Codes.WriteOff && JobDeclaration.JE_MessageType != NZJobMessageTypeList.Codes.MiscellaneousCustoms;

		#endregion

		void LoadPlugins()
		{
			if (!JobDeclaration.IsTSWCREWriteOff)
			{
				LineDetailTabControl.PlugIns.Add(ControllerIDs.Customs.NZ.MAFeBACCaInvoiceLinePlugin);
			}
		}
	}
}
