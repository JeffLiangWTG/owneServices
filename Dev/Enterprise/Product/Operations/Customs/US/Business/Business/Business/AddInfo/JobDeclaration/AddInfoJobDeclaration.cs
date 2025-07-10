using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobDeclaration : AddInfo, IWorkflowTriggerFieldChangeSource, IReconDeclarationAddInfo
	{
		public AddInfoJobDeclaration(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoJobDeclarationLookups Lookups
		{
			get { return (AddInfoJobDeclarationLookups)base.Lookups; }
		}

		public new AddInfoJobDeclarationValidation Validation
		{
			get { return (AddInfoJobDeclarationValidation)base.Validation; }
		}

		protected override USAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobDeclarationLookups(this);
		}

		protected override void LoadPropertiesFromAddInfoProperty(bool clearExisting)
		{
			if (!isInitialised)
			{
				PropertyValueChanged += AddInfoPropertyValueChanged;
			}
			base.LoadPropertiesFromAddInfoProperty(clearExisting);
		}

		readonly ConcurrentHashSet<string> changedProperties = new ();

		void AddInfoPropertyValueChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (!isInitialised || e.Property.Value.IsDefault || Declaration.IsUniversalCopying || Declaration.GetIsSettingDefaultValues() || IsCommonAddInfoProperty(e.Property.Name))
			{
				return;
			}

			_ = changedProperties.TryAdd(e.Property.Name);
		}

		static bool IsCommonAddInfoProperty(string propertyName)
		{
			return propertyName switch
			{
				nameof(US_ClientBranchDesignation)
					or nameof(US_ENSAction)
					or nameof(US_EntryFilerCode)
					or nameof(US_EstimatedEntryDate)
					or nameof(US_PaymentDate)
					or nameof(US_PaymentType)
					or nameof(US_Paid)
					or nameof(US_PreliminaryStatementPrintDate)
					or nameof(US_SchDEntry)
					or nameof(US_SuretyCode)
					or nameof(US_TeamNo) => true,
				_ => false
			};
		}

		internal void ReportUnexpectedProperties()
		{
			if (changedProperties.Count == 0)
			{
				return;
			}

			var propertyNames = string.Join(", ",
				changedProperties.Where(propertyName =>
				{
					var isReconProperty = typeof(ReconDeclaration).GetProperty(propertyName) is not null;
					return (Declaration.IsReconMessageType && !isReconProperty) || (!Declaration.IsReconMessageType && isReconProperty);
				}));

			changedProperties.Clear();

			if (!string.IsNullOrEmpty(propertyNames))
			{
				var errorMessage = Declaration.IsRecon
					? $"AddInfo properties [{propertyNames}] not in ReconDeclaration should not be set when the Declaration is a Recon."
					: $"AddInfo properties [{propertyNames}] related to ReconDeclaration should not be set for other types of declarations.";
				ErrorReporter.ReportDeveloperExceptionOnce(errorMessage, new InvalidOperationException(errorMessage));
			}
		}

		[MaxLength(JobDeclaration.Schema.US_WHSEntryNumberMaxLength)]
		public override ZString US_WHSEntryNumber
		{
			get { return base.US_WHSEntryNumber; }
			set { base.US_WHSEntryNumber = value; }
		}

		public override ZString US_SoldEnRouteIndicator
		{
			get { return base.US_SoldEnRouteIndicator; }
			set
			{
				ZString oldValue = US_SoldEnRouteIndicator;
				base.US_SoldEnRouteIndicator = value;
				if (!IsCopying && oldValue != US_SoldEnRouteIndicator)
				{
					if (IsExport)
					{
						Declaration.Invoices.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString US_CommodityFilingOption
		{
			get { return base.US_CommodityFilingOption; }
			set
			{
				bool hasChanged = value != US_CommodityFilingOption;
				base.US_CommodityFilingOption = value;
				if (!IsCopying && hasChanged)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime US_ITDate
		{
			get { return base.US_ITDate; }
			set
			{
				bool hasChanges = base.US_ITDate != value;
				base.US_ITDate = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_RoutedTransaction
		{
			get { return base.US_RoutedTransaction; }
			set
			{
				ZString oldValue = US_RoutedTransaction;
				base.US_RoutedTransaction = value;
				if (!IsCopying && isInitialised && oldValue != US_RoutedTransaction)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime US_EntryDate
		{
			get { return base.US_EntryDate; }
			set
			{
				var oldValue = US_EntryDate;
				base.US_EntryDate = value;
				if (!IsCopying && isInitialised && oldValue != US_EntryDate)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_EntryMode
		{
			get { return base.US_EntryMode; }
			set
			{
				ZString oldValue = US_EntryMode;
				base.US_EntryMode = value;
				if (!IsCopying && isInitialised && oldValue != US_EntryMode)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(US_BondType_ReadOnly))]
		public override ZString US_BondType
		{
			get { return base.US_BondType; }
			set
			{
				ZString oldValue = US_BondType;
				base.US_BondType = value;

				if (!IsCopying && isInitialised && oldValue != US_BondType)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		bool US_BondType_ReadOnly
		{
			get { return IsACEDrawback && !US_AcceleratedClaimInd || (Declaration != null && Declaration.IsLowValue); }
		}

		[ReadOnlyMember(nameof(US_ADDCVDSuretyCode_ReadOnly))]
		public override ZString US_ADDCVDSuretyCode
		{
			get { return base.US_ADDCVDSuretyCode; }
			set
			{
				bool hasChanges = base.US_ADDCVDSuretyCode != value;
				base.US_ADDCVDSuretyCode = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		bool US_ADDCVDSuretyCode_ReadOnly
		{
			get { return IsACEDrawback && !US_AcceleratedClaimInd || (Declaration != null && Declaration.IsLowValue); }
		}

		public override ZDateTime US_PresentationDate
		{
			get { return base.US_PresentationDate; }
			set
			{
				bool hasChanges = base.US_PresentationDate != value;
				base.US_PresentationDate = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime US_PreliminaryStatementPrintDate
		{
			get { return base.US_PreliminaryStatementPrintDate; }
			set
			{
				bool hasChanges = base.US_PreliminaryStatementPrintDate != value;
				base.US_PreliminaryStatementPrintDate = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime US_EstimatedEntryDate
		{
			get { return base.US_EstimatedEntryDate; }
			set
			{
				bool hasChanges = base.US_EstimatedEntryDate != value;
				base.US_EstimatedEntryDate = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_ExportCode
		{
			get { return base.US_ExportCode; }
			set
			{
				ZString oldValue = base.US_ExportCode;
				base.US_ExportCode = value;
				if (oldValue != base.US_ExportCode && IsExport && isInitialised)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override ZBool US_PGAExpeditedRelease
		{
			get { return base.US_PGAExpeditedRelease; }
			set
			{
				base.US_PGAExpeditedRelease = value;
				if (!IsCopying)
				{
					Declaration.MarkInvoiceLineAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZBool US_EnableENS
		{
			get { return base.US_EnableENS; }
			set
			{
				bool hasChanged = base.US_EnableENS != value;
				base.US_EnableENS = value;
				if (hasChanged && !IsCopying && isInitialised)
				{
					if (!Declaration.IsMarkingAsNeedingValidationSuspended)
					{
						Declaration.MarkAsNeedingValidationForMajorDataChange();
						Declaration.MarkInvoiceLineAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		public override ZBool US_EnableCRL
		{
			get { return base.US_EnableCRL; }
			set
			{
				bool hasChanged = base.US_EnableCRL != value;
				base.US_EnableCRL = value;
				if (hasChanged && !IsCopying && isInitialised)
				{
					Declaration.MarkAsNeedingValidationForMajorDataChange();
					Declaration.MarkInvoiceLineAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZBool US_EnableINB
		{
			get { return base.US_EnableINB; }
			set
			{
				bool hasChanges = base.US_EnableINB != value;
				base.US_EnableINB = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.MarkAsNeedingValidationForMajorDataChange();
				}
			}
		}

		public override ZString US_CargoReleaseType
		{
			get { return base.US_CargoReleaseType; }
			set
			{
				bool hasChanges = base.US_CargoReleaseType != value;
				base.US_CargoReleaseType = value;

				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
					Declaration.MarkInvoiceLineAsNeedingValidationIncludingChildren();
					Declaration.Bills.MarkAsNeedingValidation();
					Declaration.CusContainers.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool US_EnableSPN
		{
			get
			{
				return base.US_EnableSPN;
			}
			set
			{
				bool hasChanges = base.US_EnableSPN != value;
				base.US_EnableSPN = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_F_PNMode
		{
			get
			{
				return base.US_F_PNMode;
			}
			set
			{
				bool hasChanges = base.US_F_PNMode != value;
				base.US_F_PNMode = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool US_CertifyCargoRelease
		{
			get { return base.US_CertifyCargoRelease; }
			set
			{
				bool hasChanges = base.US_CertifyCargoRelease != value;
				base.US_CertifyCargoRelease = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.MarkAsNeedingValidationForMajorDataChange();
				}
			}
		}

		public override ZString US_EntryType
		{
			get { return base.US_EntryType; }
			set
			{
				bool hasChanges = base.US_EntryType != value;
				base.US_EntryType = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
					Declaration.MarkInvoiceLineAsNeedingValidationIncludingChildren();
					Declaration.PackingGroups.MarkAsNeedingValidation();
					Declaration.Bills.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_TariffType
		{
			get { return base.US_TariffType; }
			set
			{
				ZString oldValue = US_TariffType;
				base.US_TariffType = value;
				if (oldValue != US_TariffType && isInitialised)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public bool US_EntryFilerCode_ReadOnly
		{
			get { return Declaration.DecEntryNumber_ReadOnly; }
		}

		public override ZDateTime US_DateOfExport
		{
			get { return base.US_DateOfExport; }
			set
			{
				ZDateTime oldValue = US_DateOfExport;
				base.US_DateOfExport = value;
				if (!IsCopying && oldValue != US_DateOfExport && isInitialised)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime US_LatestRateDate
		{
			get { return base.US_LatestRateDate; }
			set
			{
				var oldValue = US_LatestRateDate;
				base.US_LatestRateDate = value;
				if (!IsCopying && oldValue != US_LatestRateDate && isInitialised)
				{
					Declaration.Invoices.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString US_UC_NKCountryOfExport
		{
			get { return base.US_UC_NKCountryOfExport; }
			set
			{
				ZString oldValue = US_UC_NKCountryOfExport;
				base.US_UC_NKCountryOfExport = value;
				if (!IsCopying && oldValue != US_UC_NKCountryOfExport && isInitialised && !IsExport)
				{
					Declaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool US_NonAMS
		{
			get { return base.US_NonAMS; }
			set
			{
				var hasChanges = base.US_NonAMS != value;
				base.US_NonAMS = value;
				if (hasChanges && !IsCopying && isInitialised)
				{
					Declaration.Bills.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool US_DomesticCargo
		{
			get { return base.US_DomesticCargo; }
			set
			{
				var hasChanges = base.US_DomesticCargo != value;
				base.US_DomesticCargo = value;
				if (hasChanges && !IsCopying)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override ZString US_ECCN
		{
			get { return base.US_ECCN; }
			set
			{
				var hasChanges = base.US_ECCN != value;
				base.US_ECCN = value;
				if (hasChanges && !IsCopying)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override ZString US_LicenseType
		{
			get { return base.US_LicenseType; }
			set
			{
				var hasChanges = base.US_LicenseType != value;
				base.US_LicenseType = value;
				if (hasChanges && !IsCopying)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		public override ZBool US_ImmediateDelivery
		{
			get { return base.US_ImmediateDelivery; }
			set
			{
				var hasChanges = base.US_ImmediateDelivery != value;
				base.US_ImmediateDelivery = value;
				if (hasChanges && !IsCopying)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		protected override USAddInfoValidation GetNewValidation()
		{
			USAddInfoValidation result = null;

			if (Declaration.IsRecon)
			{
				result = new ReconAddInfoJobDeclarationValidation(this);
			}
			else if (Declaration.IsProtest)
			{
				result = new Protest.ProtestAddInfoJobDeclarationValidation(this);
			}
			else if (Declaration.IsFTZAdmission)
			{
				result = new FTZAddInfoJobDeclarationValidation(this);
			}
			else if (Declaration.IsImport)
			{
				if (Declaration.IsACE)
				{
					result = new ACEImportAddInfoJobDeclarationValidation(this);
				}
				else
				{
					result = new FormalImportAddInfoJobDeclarationValidation(this);
				}
			}
			else if (Declaration.IsExport)
			{
				result = new ExportAddInfoJobDeclarationValidation(this);
			}
			else if (Declaration.IsDrawback)
			{
				if (Declaration.IsACEDrawback)
				{
					result = new ACEDrawbackAddInfoJobDeclarationValidation(this);
				}
				else
				{
					result = new ACSDrawbackAddInfoJobDeclarationValidation(this);
				}
			}
			else
			{
				result = new AddInfoJobDeclarationValidation(this);
			}

			return result;
		}

		protected override bool IsExportCore
		{
			get { return Declaration.IsExport; }
		}

		protected override ZString GetTransportMode()
		{
			return Declaration.JE_TransportMode;
		}

		protected override BusinessObject UseWrappedPropertiesOnly()
		{
			var declaration = Declaration;
			if (declaration.Protest is Protest.Protest protest)
			{
				return protest;
			}
			else if (declaration.ReconDeclaration is ReconDeclaration reconDeclaration)
			{
				return reconDeclaration;
			}
			else
			{
				return declaration;
			}
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[]
				{
					USAddInfoSchema.US_F_AdmissionType,
					USAddInfoSchema.US_US_NKLocationOfGoods,
					USAddInfoSchema.US_OtherReconIndicator,
					USAddInfoSchema.US_NAFTAReconIndicator,
					USAddInfoSchema.US_SuretyCode,
					USAddInfoSchema.US_EntryType,
					USAddInfoSchema.US_InbondType,
					USAddInfoSchema.US_EntryMode,
					USAddInfoSchema.US_SchDEntry,
					USAddInfoSchema.US_TIBExpiryDate,
					USAddInfoSchema.US_P_FilingDDPP,
					USAddInfoSchema.US_P_PeriodBaseDate,
					USAddInfoSchema.US_P_Assoc514ProtestNo,
					USAddInfoSchema.US_P_Assoc520PetitionNo,
					USAddInfoSchema.US_P_ProtestantType,
					USAddInfoSchema.US_P_ApplicationFurtherReview,
					USAddInfoSchema.US_P_AcceleratedDispositionInd,
					USAddInfoSchema.US_EntryFilerCode,
					USAddInfoSchema.US_BRDRefNo,
					USAddInfoSchema.US_PaymentDueDate,
					USAddInfoSchema.US_IsAIIRequested,
					USAddInfoSchema.US_PreliminaryStatementPrintDate,
					USAddInfoSchema.US_PaymentType,
					USAddInfoSchema.US_EstimatedEntryDate,
					USAddInfoSchema.US_SchDLoading,
					USAddInfoSchema.US_SchDArrival,
					USAddInfoSchema.US_PaperlessEntry,
					USAddInfoSchema.US_IssueCode,
					USAddInfoSchema.US_IsAggregate,
					USAddInfoSchema.US_EntryDate,
					USAddInfoSchema.US_TeamNo,
					USAddInfoSchema.US_NAFTADrawbackCountry,
					USAddInfoSchema.US_PreparerDistrictPort,
					USAddInfoSchema.US_ClaimPort,
					USAddInfoSchema.US_ExporterSummaryInd,
					USAddInfoSchema.US_PreInspectionInd,
					USAddInfoSchema.US_NAFTAClaimInd,
					USAddInfoSchema.US_AcceleratedClaimInd,
					USAddInfoSchema.US_WaiverNoticeInd,
					USAddInfoSchema.US_PetroleumClaimInd,
					USAddInfoSchema.US_EarliestExportDate,
					USAddInfoSchema.US_DRWDatePeriodFrom,
					USAddInfoSchema.US_DRWDatePeriodTo,
					USAddInfoSchema.US_PresentationDate,
					USAddInfoSchema.US_PSC,
					USAddInfoSchema.US_SchDExport,
					USAddInfoSchema.US_DateOfExport,
					USAddInfoSchema.US_RN_NKCountryOfDestination,
					USAddInfoSchema.US_TransportReference,
					USAddInfoSchema.US_UI_NKCarrierSCAC,
					USAddInfoSchema.US_P_StatusDate,
					USAddInfoSchema.US_DeferredTaxDueDate,
					USAddInfoSchema.US_BondProducerAccNo,
					USAddInfoSchema.US_ALDate,
					USAddInfoSchema.US_BondType,
					USAddInfoSchema.US_BondDispositionCode,
					USAddInfoSchema.US_BondDispositionCode2,
					USAddInfoSchema.US_InsuranceDisposition,
					USAddInfoSchema.US_FTZNo
				};
			}
		}

		void MarkInvoiceLinesAsNeedingValidation()
		{
			if (Declaration != null)
			{
				Declaration.InvoiceLines.MarkAsNeedingValidation();
			}
		}

		public IReadOnlyList<IWorkflowProvider> ParentWorkflowProviders
		{
			get { return new IWorkflowProvider[] { (IWorkflowProvider)this.Parent }; }
		}

		public BusinessObject ActuallyBizObj
		{
			get { return this.Parent; }
		}

#if DEBUG
		internal Customs.Business.BaseAddInfo DbAddInfoExposed => DbAddInfo;
#endif
	}
}
